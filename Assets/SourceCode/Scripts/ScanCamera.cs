/**
    扫描摄像机
    渲染可以被扫描的部分物体
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ScanCamera : MonoBehaviour
{
    public float GradientSpeed 
    {
        get => _gradientSpeed;
    }
    public float RenderDuration
    {
        get => _renderDuration;
    }

    private List<ScannableObject> SObjs;
    private Camera _scanCamera;
    private Camera _mainCamera;
    [SerializeField] private float _gradientSpeed = 0.1f;
    [SerializeField] private float _renderDuration = 3f;
    // Start is called before the first frame update
    private void Awake() {
        _scanCamera = GetComponent<Camera>();
        _mainCamera = Camera.main;
    }
    
    void Start()
    {
        SObjs = GameObject.FindObjectsOfType<ScannableObject>().ToList();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnPreRender() {
        _scanCamera.fieldOfView = _mainCamera.fieldOfView;
        foreach(var obj in SObjs)
        {
            obj.Org2Scan();
        }
    }

    private void OnPostRender() {
        foreach(var obj in SObjs)
        {
            obj.Scan2Org();
        }
    }
}
