using UnityEngine;
using System.Collections;
using StarterAssets;

[ExecuteInEditMode]
public class Scanner : MonoBehaviour
{
    /// <summary>
    /// 扫描开始位置
    /// </summary>
	private Vector3 _scannerOrigin;
    /// <summary>
    /// 扫描效果材质
    /// </summary>
	public Material EffectMaterial;
    /// <summary>
    /// 当前扫描距离
    /// </summary>
	public float ScanDistance;
    /// <summary>
    /// 扫描移动速度
    /// </summary>
    public float ScanSpeed = 50f;

	private Camera _camera;
    [SerializeField] private StarterAssetsInputs _input;

	bool _scanning;

    private void Awake() 
    {
        if(EffectMaterial == null)
        {
            EffectMaterial = new Material(Shader.Find("ScannerEffect"));
        }      
    }

	void Start()
	{

    }

	void Update()
	{
		if (_scanning)
		{
			ScanDistance += Time.deltaTime * ScanSpeed;
		}


		if (Input.GetKeyDown(KeyCode.Q))
		{
			GenerateScanStartPoint();
			_scanning = true;
			ScanDistance = 0;
		}
	}

    private void GenerateScanStartPoint()
    {
        _scannerOrigin = transform.position;
    }

    // 在鼠标按下的位置扫描
    private void ScanInMouseWorldPoint()
    {
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
			RaycastHit hit;

			if (Physics.Raycast(ray, out hit))
			{
				_scanning = true;
				ScanDistance = 0;
				_scannerOrigin = hit.point;
			}
		}
    }

	void OnEnable()
	{
		_camera = GetComponent<Camera>();
		_camera.depthTextureMode = DepthTextureMode.Depth;
	}

	[ImageEffectOpaque]
	void OnRenderImage(RenderTexture src, RenderTexture dst)
	{
		EffectMaterial.SetVector("_WorldSpaceScannerPos", _scannerOrigin);
		EffectMaterial.SetFloat("_ScanDistance", ScanDistance);
		RaycastCornerBlit(src, dst, EffectMaterial);
	}

	void RaycastCornerBlit(RenderTexture source, RenderTexture dest, Material mat)
	{
		// Compute Frustum Corners
		float camFar = _camera.farClipPlane;
		float camFov = _camera.fieldOfView;
		float camAspect = _camera.aspect;

		float fovWHalf = camFov * 0.5f;

		Vector3 toRight = _camera.transform.right * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camAspect;
		Vector3 toTop = _camera.transform.up * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

		Vector3 topLeft = (_camera.transform.forward - toRight + toTop);
		float camScale = topLeft.magnitude * camFar;

		topLeft.Normalize();
		topLeft *= camScale;

		Vector3 topRight = (_camera.transform.forward + toRight + toTop);
		topRight.Normalize();
		topRight *= camScale;

		Vector3 bottomRight = (_camera.transform.forward + toRight - toTop);
		bottomRight.Normalize();
		bottomRight *= camScale;

		Vector3 bottomLeft = (_camera.transform.forward - toRight - toTop);
		bottomLeft.Normalize();
		bottomLeft *= camScale;

		// Custom Blit, encoding Frustum Corners as additional Texture Coordinates
		RenderTexture.active = dest;

		mat.SetTexture("_MainTex", source);

		GL.PushMatrix();
		GL.LoadOrtho();

		mat.SetPass(0);

		GL.Begin(GL.QUADS);

		GL.MultiTexCoord2(0, 0.0f, 0.0f);
		GL.MultiTexCoord(1, bottomLeft);
		GL.Vertex3(0.0f, 0.0f, 0.0f);

		GL.MultiTexCoord2(0, 1.0f, 0.0f);
		GL.MultiTexCoord(1, bottomRight);
		GL.Vertex3(1.0f, 0.0f, 0.0f);

		GL.MultiTexCoord2(0, 1.0f, 1.0f);
		GL.MultiTexCoord(1, topRight);
		GL.Vertex3(1.0f, 1.0f, 0.0f);

		GL.MultiTexCoord2(0, 0.0f, 1.0f);
		GL.MultiTexCoord(1, topLeft);
		GL.Vertex3(0.0f, 1.0f, 0.0f);

		GL.End();
		GL.PopMatrix();
	}
}
