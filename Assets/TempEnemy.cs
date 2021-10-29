using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using StarterAssets;
using UnityEngine.AI;

public class TempEnemy : MonoBehaviour
{
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
    public Color CircleColor;
    [ReadOnly] public float timer = 0f;
    [ReadOnly] public bool scanned = false;
    SkinnedMeshRenderer _renderer;

    public Transform SpawnPoint;
    [SerializeField] Transform _characterTransform;
    Animator _anim;
    // Start is called before the first frame update
    void Start()
    {
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
            Patrol();
    }

    private void Patrol()
    {
        // 计时器运行
        if(PatrolTimer > 0f)
        {
            PatrolTimer -= Time.deltaTime;
        }

        if(agent.velocity != Vector3.zero)
        {
            _anim.SetFloat("vertical", 1f, 0.2f, Time.deltaTime);
        }
        else
        {
            _anim.SetFloat("vertical", 0f, 0.2f, Time.deltaTime);
        }

        var p = PatrolPoints[curPatrolIndex].position;
        var p1 = new Vector3(transform.position.x, 0, transform.position.z);
        var p2 = new Vector3(p.x, 0, p.z);

        float dist = Vector3.Distance(p1, p2);
        if(dist < 0.4f)
        {
            // 设置人物停止一段时间
            if(!agent.isStopped)
            {
                agent.isStopped = true;
                PatrolTimer = WaitTime;
                curPatrolIndex = (1 + curPatrolIndex) % PatrolPoints.Length;
                transform.LookAt(PatrolPoints[curPatrolIndex].position);
                agent.SetDestination(PatrolPoints[curPatrolIndex].position);
            }

        }
        if(agent.isStopped && PatrolTimer <= 0f)
        {
            agent.isStopped = false;
        }
    }

    private void LateUpdate() {
        if(_characterTransform != null)
        {
            // 被发现
            if(Vector3.Distance(transform.position, _characterTransform.position) < DeathRadius)
            {
                LoseText.gameObject.SetActive(true);
                _characterTransform.position = SpawnPoint.position;
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
    }
}
