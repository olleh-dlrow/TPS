using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class ScannerEffect : MonoBehaviour
{
    bool firstScan = true;
    Material[] beforeScanMat;
    public Material[] afterScanMat;
    public Material mat;
    public float velocity = 2;
    [Range(0, 1)]
    public float width = 0.3f;
    bool isScanning;
    float dist;
    Camera mCamera;
    new SphereCollider collider;
    //public float timer = 0f;
    public float interval = 3f;

    private void Start() {
        mCamera = GetComponent<Camera>();
        collider = GetComponent<SphereCollider>();
        mCamera.depthTextureMode |= DepthTextureMode.Depth;
        mCamera.depthTextureMode |= DepthTextureMode.DepthNormals;
    }

    private void Update() {
        if(isScanning)
        {
            dist += Time.deltaTime * velocity;
            collider.radius += Time.deltaTime * 30f;
            if(dist > 5f)
            {
                isScanning = false;
            }
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            isScanning = true;
            dist = 0f;
            collider.radius = 1e-5f;
        }
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        
        //Matrix4x4 farFarClipPos = CalculateFarClipPos();
        //mat.SetMatrix("_FarClipRay", farFarClipPos);
        mat.SetFloat("_ScanDistance", dist);
        mat.SetFloat("_ScanWidth", width);
        Graphics.Blit(src, dest, mat);
    }

    private Matrix4x4 CalculateFarClipPos()
    {
        Matrix4x4 farFarClipPos = new Matrix4x4();

        //计算远平面 这里先只处理透视相机
        float fov = mCamera.fieldOfView;
        float near = mCamera.nearClipPlane;
        float far = mCamera.farClipPlane;
        float aspect = mCamera.aspect;//视锥宽高比

        float halfHeight = far * Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * aspect;
        Vector3 toRight = halfWidth * mCamera.transform.right;
        Vector3 toTop = halfHeight * mCamera.transform.up;


        Vector3 farCenter = mCamera.transform.forward * far;
        Vector3 farTopLeft = farCenter + toTop - toRight;
        Vector3 farTopRight = farCenter + toTop + toRight;
        Vector3 farBottomLeft = farCenter - toTop - toRight;
        Vector3 farBottomRight = farCenter - toTop + toRight;

        farFarClipPos.SetRow(0, farBottomLeft);
        farFarClipPos.SetRow(1, farBottomRight);
        farFarClipPos.SetRow(2, farTopRight);
        farFarClipPos.SetRow(3, farTopLeft);

        return farFarClipPos;
    }

    private void OnTriggerEnter(Collider other) {
        Debug.Log(other.name);
        if(other.TryGetComponent<BulletTarget>(out BulletTarget obj))
        {
            var mr = obj.GetComponent<MeshRenderer>();
            if(firstScan) {
                firstScan = false;
                beforeScanMat = mr.materials;
            }
            mr.materials = afterScanMat;

            StartCoroutine(Disappear(mr));
        }
    }

    IEnumerator Disappear(MeshRenderer mr)
    {
        float timer = interval;
        while(timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        if(mr)mr.materials = beforeScanMat;
    }
}
