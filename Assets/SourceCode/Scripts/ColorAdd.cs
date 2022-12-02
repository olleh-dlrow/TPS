// 渲染顺序是这样的...
// 首先相机-1渲染出包含层的像素
// 相机0渲染出包含层的像素
// 两个层发生混合
// 调用OnRenderImage，如果有的话

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorAdd : MonoBehaviour
{
    public Renderer cubeRD;
    public Camera additionalCamera;
    public Material TargetMat;
    public Material OrgMat;
    private void Reset() {
        // Init();
    }
    private void Awake() {
        // Init();
        OrgMat = cubeRD.material;
    }

    void Init()
    {
        additionalCamera.CopyFrom(Camera.main);
        additionalCamera.clearFlags = CameraClearFlags.Color;
        additionalCamera.backgroundColor = Color.black;
        additionalCamera.cullingMask = 1 << LayerMask.NameToLayer("Layer1");  
    }


    private void OnPreRender() {
        cubeRD.material = TargetMat;
    }

    private void OnPostRender() {
        cubeRD.material = OrgMat;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
