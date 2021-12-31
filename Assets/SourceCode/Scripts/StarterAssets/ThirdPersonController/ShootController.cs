/**
    射击控制器
    控制人物的射击
    功能性组件
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Sirenix.OdinInspector;
using StarterAssets;

public class ShootController : MonoBehaviour
{
#region "Public Variables"
    public float AimMoveSpeed { get => _aimMoveSpeed; }
#endregion

#region "Private Variables"
    [SerializeField] private float _aimMoveSpeed = 1.0f;
    [SerializeField, GUIColor(0.8f, 0.3f, 0.3f)] private float _rotateSpeed;
    [SerializeField, GUIColor(0.5f, 0.8f, 0.5f)] private float _normalSensitivity = 0.5f;
    [SerializeField, GUIColor(0.3f, 0.8f, 0.8f)] private float _aimSensitivity = 0.5f;
    /// <summary>
    /// 能够击中的层
    /// </summary>
    [Tooltip("能够击中的层")]
    [SerializeField] private LayerMask _aimColliderLayerMask;
    
    private float _inputMoveX = 0f;
    private float _inputMoveY = 0f;
    private float _inputMoveDelta = 5f;
    private int _animIDAiming = Animator.StringToHash("Rifle Idle Aiming");

    [Header("References")]
    private CinemachineVirtualCamera _aimVirtualCamera;
    private Animator _characterAnimator;
    private Transform _weaponHolder;
    private WeaponBag _weaponBag;
    private ThirdPersonController _TPController;
    private StarterAssetsInputs _starterAssetsInputs;
    private CharacterIKController _characterIKController;
    private GameObject _crossHair;

    [SerializeField, AssetsOnly] private GameObject _pfCrossHair;
    [SerializeField, AssetsOnly] private GameObject _energyExplosion;
#endregion

    private void Awake() {
        Debug.Assert(_starterAssetsInputs = GetComponent<StarterAssetsInputs>()                  );
        Debug.Assert(_TPController = GetComponent<ThirdPersonController>()                       );
        Debug.Assert(_weaponBag = GetComponentInChildren<WeaponBag>()                            );
        Debug.Assert(_characterIKController = GetComponentInChildren<CharacterIKController>()    );
        Debug.Assert(_weaponHolder = GameObject.Find("WeaponHolder").transform);
        
        Debug.Assert(_characterAnimator = GetComponentInChildren<Character>().GetComponent<Animator>());

    }

    private void Start() {
        
        InitializeAimVirtualCamera();
        InitializeCrossHair();
        InitializeWeaponInHand();
    }

    private void InitializeCrossHair()
    {
        if(_pfCrossHair)
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
            Debug.Assert(canvas);

            _crossHair = Instantiate(_pfCrossHair, canvas.transform);
        }
    }

    /// <summary>
    /// set aim camera
    /// </summary>
    private void InitializeAimVirtualCamera()
    {
        _aimVirtualCamera = GameObject.FindObjectOfType<CameraGroup>()
                            .transform.Find("PlayerAimCamera")
                            .GetComponent<CinemachineVirtualCamera>();

        Debug.Assert(_aimVirtualCamera);
    }

    /// <summary>
    /// execute in start function
    /// </summary>
    private void InitializeWeaponInHand()
    {
        var weaponInHand = _weaponBag.GetCurrentWeaponTransform();
        weaponInHand.parent = _weaponHolder;
        weaponInHand.localPosition = Vector3.zero;
        weaponInHand.localRotation = Quaternion.identity;
        weaponInHand.gameObject.SetActive(true);

        var weaponIK = weaponInHand.GetComponent<WeaponIK>();

        _characterIKController.SetLeftHandIK(weaponIK.LeftHandIKTransform);
    }

    private void Update() {
        if(IsAiming())
        {
            _crossHair?.SetActive(true);
            EnterAimState();
            OnAimState();
        }
        else
        {
            _crossHair?.SetActive(false);
            ExitAimState();
        }
    }

    private bool IsAiming(){ return _starterAssetsInputs.aim == true; }

    private void EnterAimState()
    {
        SetAnimationOnAimState();
        SwitchCameraToAimMode();
        SwitchMotionControlToAimMode();
    }

    private void MoveOnAimState()
    {
        _inputMoveX = Mathf.MoveTowards(_inputMoveX, _starterAssetsInputs.move.x, Time.deltaTime * _inputMoveDelta);
        _inputMoveY = Mathf.MoveTowards(_inputMoveY, _starterAssetsInputs.move.y, Time.deltaTime * _inputMoveDelta);
        _characterAnimator.SetFloat("horizontal", _inputMoveX);
        _characterAnimator.SetFloat("vertical", _inputMoveY);
    }

    private void SetAnimationOnAimState()
    {
        _characterAnimator.SetBool("Armed", true);
        _characterAnimator.SetLayerWeight(1, 1);
    }

    private void SwitchCameraToAimMode() 
    {
         _aimVirtualCamera.gameObject.SetActive(true);
    }

    private void SwitchMotionControlToAimMode()
    {
        _TPController.Sensitivity = _aimSensitivity * _TPController.TimeScale;
    }

    private void OnAimState()
    {
        MoveOnAimState();

        RaycastHit aimHit = GetAimHit();
        bool inAimingAnimation = _characterAnimator.GetCurrentAnimatorStateInfo(0).shortNameHash == _animIDAiming;
        if(inAimingAnimation && _TPController.Grounded && PrepareShoot())
        {
            Shoot(aimHit);
            OnShootEnd();
        }
    }

    private void OnShootEnd() {_starterAssetsInputs.shoot = false;}

    private RaycastHit GetAimHit()
    {
        Vector3 aimPoint = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray rayToScreenCenter = Camera.main.ScreenPointToRay(screenCenterPoint);

        if(Physics.Raycast(rayToScreenCenter, out RaycastHit hit, System.Single.MaxValue, _aimColliderLayerMask))
        {
            aimPoint = hit.point;
        }
        return hit;
    }

    private void ExitAimState()
    {
        AnimationExitAimState();

        ExitCameraFromAimMode();
        ExitMotionControlFromAimMode();
    }

    private void AnimationExitAimState()
    {
        _characterAnimator.SetBool("Armed", false);
        _characterAnimator.SetLayerWeight(1, 0);
    }

    private void ExitCameraFromAimMode()
    {
        _aimVirtualCamera.gameObject.SetActive(false);
    }

    private void ExitMotionControlFromAimMode()
    {
        _TPController.Sensitivity = _normalSensitivity * _TPController.TimeScale;
    }

    private bool PrepareShoot()
    {
        return _starterAssetsInputs.shoot == true;
    }

    private void Shoot(RaycastHit hit)
    {
        if(hit.collider)
        {
            if(_energyExplosion)
            {
                GameObject explosion = Instantiate(_energyExplosion, hit.point, Quaternion.identity);
                StartCoroutine(PlayExplosion(explosion.GetComponent<ParticleSystem>()));
            }
            // check target
            if(hit.collider.TryGetComponent<TempEnemy>(out var enemy))
            {
                Destroy(hit.collider.gameObject);       
            } 
        }

    }

    private IEnumerator PlayExplosion(ParticleSystem particle)
    {
        float timer = particle.main.duration;
        while(timer > 0f)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        Destroy(particle.gameObject);
    }
}
