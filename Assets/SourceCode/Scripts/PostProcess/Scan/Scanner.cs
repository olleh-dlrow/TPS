using UnityEngine;
using System.Collections.Generic;
using StarterAssets;
using System.Linq;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class Scanner : MonoBehaviour, IPostProcessEffect
{
    public float ScanSpeed
	{
		get => _scanSpeed;
		set => _scanSpeed = value;
	}

	public float MaxScanDistance
	{
		get => _maxScanDistance;
		set => _maxScanDistance = value;
	}

	public float CoolDownTime
	{
		get => _coolDownTime;
		set => _coolDownTime = value;
	}

	[SerializeField] private float _scanSpeed = 50f;
	[SerializeField] private float _maxScanDistance = 100f;
	[SerializeField] private float _coolDownTime = 5f;
	[SerializeField] private Material _effectMat;
	[SerializeField, ReadOnly] private float _scanDistance;
	[SerializeField, ReadOnly] private float _curCoolDownTime;

	private ScanCamera _scanCamera;
	private Vector3 _scannerOrigin;
	private Camera _camera;
	private List<TempEnemy> enemies;
	private ThirdPersonController _TPController;

	private List<ScannableObject> SObjs;

	private bool _isScanning;

	private bool _inCoolDown;

    private void Awake() 
    {
        if(_effectMat == null)
        {
            _effectMat = new Material(Shader.Find("ScannerEffect"));
        }

    }

	void Start()
	{
		_scanCamera = GameObject.FindObjectOfType<ScanCamera>();
		enemies = GameObject.FindObjectsOfType<TempEnemy>().ToList();
		SObjs = GameObject.FindObjectsOfType<ScannableObject>().ToList();

		_TPController = GameObject.FindObjectOfType<ThirdPersonController>();
		Debug.AssertFormat(_TPController != null, "Player is null!");
		Debug.AssertFormat(enemies != null, "enemy is null!");
    }

	void Update()
	{
		if (_isScanning)
		{
			_scanDistance += Time.deltaTime * ScanSpeed;

			// display enemies
			foreach(var enemy in enemies)
			{
				if(enemy == null)
					continue;
				// if(!enemy.scanned && Vector3.Distance(_scannerOrigin, enemy.transform.position) < ScanDistance)
				if(Vector3.Distance(_scannerOrigin, enemy.transform.position) < _scanDistance)
				{
					enemy.Scanned();
				}
			}

			foreach(var SO in SObjs)
			{
				if(SO == null || SO.ScanBoost)continue;
				if(Vector3.Distance(_scannerOrigin, SO.transform.position) < _scanDistance)
				{
					SO.OnScanBegin();
					Timer.SetTimer(_scanCamera.RenderDuration, SO.OnScanEnd);
				}
			}

			if(_scanDistance > MaxScanDistance)
			{
				_isScanning = false;
				_scanDistance = 0f;
			}
		}


		if (!_inCoolDown && Input.GetKeyDown(KeyCode.Q))
		{
			GenerateScanStartPoint();
			_isScanning = true;
			_scanDistance = 0;

			_inCoolDown = true;
			_curCoolDownTime = CoolDownTime;
			Timer.SetTimer(CoolDownTime, 1f, ()=>{_curCoolDownTime -= 1;}, ()=>{_inCoolDown = false;});
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
				_isScanning = true;
				_scanDistance = 0;
				_scannerOrigin = hit.point;
			}
		}
    }

	void OnEnable()
	{
		_camera = GetComponent<Camera>();
		_camera.depthTextureMode |= DepthTextureMode.Depth;
		_camera.depthTextureMode |= DepthTextureMode.DepthNormals;
	}

	public void RenderEffect(RenderTexture src, RenderTexture dst)
	{
		_effectMat.SetVector("_WorldSpaceScannerPos", _scannerOrigin);
		_effectMat.SetFloat("_ScanDistance", _scanDistance);

		RaycastCornerBlit(src, dst, _effectMat);
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
