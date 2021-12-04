using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using StarterAssets;
using UnityEngine.AI;

public enum EnemyState
{
    Attack,
    Idle,
    Patrol
}

public class TempEnemy : MonoBehaviour
{
    // 敌人当前状态
    public EnemyState CurrentState;
    LoseWidget LoseText;
    // 到达巡逻点后的休息时间
    public float WaitTime = 3f;
    [ReadOnly] public float PatrolTimer = 0f;
    [Header("巡逻点")]
    public Transform[] PatrolPoints;
    public int curPatrolIndex = 0;
    private NavMeshAgent agent;
    // 角色的原始材质
    public Material[] OrgMaterials;
    // 警示圈外被扫描后显示的材质
    public Material[] ScanMaterials;
    // 警示圈内，侦察圈外被扫描后显示的材质
    public Material[] WarnMaterials;
    // 扫描后显示时间
    public float DisplayTime = 4f;
    [Header("警示圈")]
    public float WarnRadius = 6f;
    [Header("侦察圈")]
    public float DeathRadius = 4f;
    public float DeathTime = 3f;
    public Color CircleColor;
    [ReadOnly] public float timer = 0f;
    [ReadOnly] public bool scanned = false;
    // 目标角度
    float TargetAngle;
    // 敌人视野范围
    public float ViewAngle = 60f;

    // 角度差，每帧更新
    float AngleDiff;
    //是否通过射线能看到
    bool isRayCast;
    // 是否在扇形视角范围内
    bool isInView;   
    bool lose;
    SkinnedMeshRenderer _renderer;

    public Transform SpawnPoint;
    [SerializeField] Transform _characterTransform;
    Animator _anim;
    // Start is called before the first frame update
    void Start()
    {
        // 初始化敌人状态
        CurrentState = EnemyState.Patrol;
        // 初始化文字引用
        LoseText = Resources.FindObjectsOfTypeAll<LoseWidget>()[0];
        _anim = GetComponent<Animator>();
        // 初始化角色生成点
        SpawnPoint = GameObject.FindObjectOfType<SpawnPoint>()?.transform;
        // 获取角色位置
        _characterTransform = GameObject.FindObjectOfType<ThirdPersonController>()?.transform;

        // 保存原始材质
        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        OrgMaterials = _renderer.materials;
        Debug.AssertFormat(OrgMaterials != null, "org mat error");

        // 设置AI组件
        if(PatrolPoints != null && PatrolPoints.Length > 0)
        {
            agent = GetComponent<NavMeshAgent>();
            transform.LookAt(PatrolPoints[0].position);
            agent.SetDestination(PatrolPoints[0].position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // 被扫描状态
        if(timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            if(scanned)
            {
                _renderer.materials = OrgMaterials;
                scanned = false;
            }
        }

        if(PatrolPoints != null)
            StateUpdate();
    }

    // 更新敌人状态
    private void StateUpdate()
    {
        CalculateAngle();
        JudgeView();

        // 计时器运行
        if(PatrolTimer > 0f)
        {
            PatrolTimer -= Time.deltaTime;
        }

        // 更新敌人动画
        if(agent.velocity != Vector3.zero)
        {
            _anim.SetFloat("vertical", 1f, 0.2f, Time.deltaTime);
        }
        else
        {
            _anim.SetFloat("vertical", 0f, 0.2f, Time.deltaTime);
        }

        if(CurrentState == EnemyState.Patrol)
        {
            if(CheckFoundTarget())
            {
                OnTargetFound();
            }
            else
            {
                // 计算当前人物和巡逻点的距离
                var p = PatrolPoints[curPatrolIndex].position;
                var p1 = new Vector3(transform.position.x, 0, transform.position.z);
                var p2 = new Vector3(p.x, 0, p.z);

                float dist = Vector3.Distance(p1, p2);
                if(dist < 0.4f)
                {
                    // 设置人物停止一段时间
                    if(!agent.isStopped)
                    {
                        CurrentState = EnemyState.Idle;

                        agent.isStopped = true;
                        PatrolTimer = WaitTime;
                        curPatrolIndex = (1 + curPatrolIndex) % PatrolPoints.Length;
                        transform.LookAt(PatrolPoints[curPatrolIndex].position);
                        agent.SetDestination(PatrolPoints[curPatrolIndex].position);
                    }
                }
            }
        }

        if(CurrentState == EnemyState.Idle)
        {
            if(CheckFoundTarget())
            {
                OnTargetFound();
            }
            else if(agent.isStopped && PatrolTimer <= 0f)
            {
                CurrentState = EnemyState.Patrol;

                agent.isStopped = false;
            }
        }

        if(CurrentState == EnemyState.Attack)
        {
            if(CheckFoundTarget())
            {
                transform.LookAt(_characterTransform);

            }
            else
            {
                CurrentState = EnemyState.Patrol;
                Time.timeScale = 1.0f;
                agent.isStopped = false;
                OnTargetLost();
            }
        }
    }

    private void OnDestroy() {
        Time.timeScale = 1f;
        OnTargetLost();
    }

    private void LateUpdate() {

    }

    // 检查是否发现了目标
    private bool CheckFoundTarget()
    {
        if(_characterTransform != null)
        {
            // 被发现
            float dist = Vector3.Distance(transform.position, _characterTransform.position);
            if(isInView && isRayCast && dist < DeathRadius)
                return true;
        }
        return false;
    }

    private void OnTargetFound()
    {
        CurrentState = EnemyState.Attack;

        agent.isStopped = true;
        Time.timeScale = 0.5f;
        if(_characterTransform.TryGetComponent<ThirdPersonController>(out var TPC))
        {
            TPC.TimeScale = 0.1f;
            if(Camera.main.TryGetComponent<Scanner>(out var scanner))
            {
                scanner.IsBulletTime = true;
                // StartCoroutine(DeathCountDown());
            }
        }
    }

    // 玩家死亡倒计时
    private IEnumerator DeathCountDown()
    {
        float deathTimer = DeathTime;
        while(deathTimer > 0f)
        {
            deathTimer -= Time.unscaledDeltaTime;
            yield return null;
        }
        LoseText.gameObject.SetActive(true);
        lose = true;
    }

    // 目标丢失
    private void OnTargetLost()
    {
        if(_characterTransform && _characterTransform.TryGetComponent<ThirdPersonController>(out var TPC))
        {
            TPC.TimeScale = 1f;
            if(Camera.main && Camera.main.TryGetComponent<Scanner>(out var scanner))
            {
                scanner.IsBulletTime = false;
                StopAllCoroutines();
            }
        }
    }

    // 被扫描后调用
    public void Scanned()
    {
        scanned = true;
        timer = DisplayTime;
        
        if(_characterTransform)
        {
            float dist = Vector3.Distance(_characterTransform.position, transform.position);
            // 在警示圈外
            if(dist > WarnRadius)
                _renderer.materials = ScanMaterials;

            // 在警示圈内
            else if(dist <= WarnRadius)
            {
                _renderer.materials = WarnMaterials;
            }
        }
    }

    // 计算玩家和敌人之间的视角差
   void CalculateAngle()
    {
        if (_characterTransform != null)
        {
            float AtanAngle = (Mathf.Atan((_characterTransform.position.z - this.transform.position.z) /
            (_characterTransform.position.x - this.transform.position.x))
            * 180.0f / 3.14159f);
            //Debug.Log (this.transform.rotation.eulerAngles+"   "+AtanAngle);
 
            //1象限角度转换
            if ((_characterTransform.position.z - this.transform.position.z) > 0
               &&
            (_characterTransform.position.x - this.transform.position.x) > 0
               )
            {
                TargetAngle = 90f - AtanAngle;
                //Debug.Log ("象限1 "+TargetAngle);
            }
 
            //2象限角度转换
            if ((_characterTransform.position.z - this.transform.position.z) <= 0
               &&
            (_characterTransform.position.x - this.transform.position.x) > 0
               )
            {
                TargetAngle = 90f + -AtanAngle;
                //Debug.Log ("象限2 "+TargetAngle);
            }
 
            //3象限角度转换
            if ((_characterTransform.position.z - this.transform.position.z) <= 0
               &&
            (_characterTransform.position.x - this.transform.position.x) <= 0
               )
            {
                TargetAngle = 90f - AtanAngle + 180f;
                //Debug.Log ("象限3 "+TargetAngle);
            }
 
            //4象限角度转换
            if ((_characterTransform.position.z - this.transform.position.z) > 0
               &&
            (_characterTransform.position.x - this.transform.position.x) <= 0
               )
            {
                TargetAngle = 270f + -AtanAngle;
                //Debug.Log ("象限4 "+TargetAngle);
            }
 
 
            //调整TargetAngle
            float OriginTargetAngle = TargetAngle;
            if (Mathf.Abs(TargetAngle + 360 - this.transform.rotation.eulerAngles.y)
               <
            Mathf.Abs(TargetAngle - this.transform.rotation.eulerAngles.y)
               )
            {
                TargetAngle += 360f;
            }
            if (Mathf.Abs(TargetAngle - 360 - this.transform.rotation.eulerAngles.y)
               <
            Mathf.Abs(TargetAngle - this.transform.rotation.eulerAngles.y)
               )
            {
                TargetAngle -= 360f;
            }
 
            //输出角度差
            AngleDiff = Mathf.Abs(TargetAngle - this.transform.rotation.eulerAngles.y);
            // Debug.Log("角度差:" + TargetAngle + "(" + OriginTargetAngle + ")-" + this.transform.rotation.eulerAngles.y + "=" + AngleDiff);
        }
    }

    // 感知视野的相关计算 判断isRayCast和isInView
    void JudgeView()
    {
 
        //感知角度相关计算
        if (_characterTransform != null)
        {
            //指向玩家的向量计算
            Vector3 vec = new Vector3(_characterTransform.position.x - this.transform.position.x,
                                    0f,
                                    _characterTransform.position.z - this.transform.position.z);
 
            //射线碰撞判断
            RaycastHit hitInfo;
            if (Physics.Raycast(this.transform.position + Vector3.up, vec, out
                               hitInfo, 20))
            {
                GameObject gameObj = hitInfo.collider.gameObject;
                //Debug.Log("Object name is " + gameObj.name);
                if (gameObj.TryGetComponent<ThirdPersonController>(out var TPC))//当射线碰撞目标为boot类型的物品 ，执行拾取操作
                {
                    // Debug.Log("Seen!");
                    isRayCast = true;
                }
                else
                {
                    isRayCast = false;
                }
            }
 
            //画出碰撞线
            Debug.DrawLine(this.transform.position, hitInfo.point, Color.red, 1);
            //视野中的射线碰撞判断结束
 
            //视野范围判断
            //物体在范围角度内,警戒模式下范围为原来1.5倍
            if (AngleDiff * 2 < ViewAngle
               )
            {
                // Debug.Log("InView!");
                isInView = true;
            }
            else
            {
                isInView = false;
            }
            //Debug.Log ("角度差 "+AngleDiff);
        }
    }

    private void OnDrawGizmos() {
        //Gizmos.color = CircleColor;
        //if(scanned)
        //    Gizmos.DrawSphere(transform.position, DeathRadius);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = CircleColor;
        Gizmos.DrawSphere(transform.position, DeathRadius);        
    }

    private void OnGUI() {
        if(CurrentState == EnemyState.Attack)
        {

        }
    }
}
