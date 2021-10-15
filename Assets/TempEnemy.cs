using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using StarterAssets;

public class TempEnemy : MonoBehaviour
{
    // 角色的原始材质
    public Material[] OrgMaterials;
    // 扫描后显示的材质
    public Material[] ScanMaterials;
    // 扫描后显示时间
    public float DisplayTime = 4f;
    [Header("侦察圈")]
    public float DeathRadius = 5f;
    public Color CircleColor;
    [ReadOnly] public float timer = 0f;
    [ReadOnly] public bool scanned = false;
    SkinnedMeshRenderer _renderer;

    public Transform SpawnPoint;
    [SerializeField] Transform _characterTransform;
    // Start is called before the first frame update
    void Start()
    {
        SpawnPoint = GameObject.FindObjectOfType<SpawnPoint>().transform;
        _characterTransform = GameObject.FindObjectOfType<ThirdPersonController>().transform;

        _renderer = GetComponentInChildren<SkinnedMeshRenderer>();
        OrgMaterials = _renderer.materials;
        Debug.AssertFormat(OrgMaterials != null, "org mat error");
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
    }

    private void LateUpdate() {
        if(Vector3.Distance(transform.position, _characterTransform.position) < DeathRadius)
        {
            _characterTransform.position = SpawnPoint.position;
        }  
    }

    // 被扫描后调用
    public void Scanned()
    {
        scanned = true;
        timer = DisplayTime;
        _renderer.materials = ScanMaterials;
    }

    private void OnDrawGizmos() {
        Gizmos.color = CircleColor;
        if(scanned)
            Gizmos.DrawSphere(transform.position, DeathRadius);
    }

    private void OnDrawGizmosSelected() {
        Gizmos.color = CircleColor;
        Gizmos.DrawSphere(transform.position, DeathRadius);        
    }
}
