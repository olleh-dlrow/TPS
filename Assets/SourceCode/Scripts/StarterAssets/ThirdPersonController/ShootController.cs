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
    [SerializeField] private LayerMask _aimColliderLayerMask;
    
    private float _inputMoveX = 0f;
    private float _inputMoveY = 0f;
    private float _inputMoveDelta = 5f;
    private int _animIDAiming = Animator.StringToHash("Rifle Idle Aiming");

    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera _aimVirtualCamera;
    [SerializeField] private Animator _characterAnimatorController;
    [SerializeField, Required] private Transform _weaponHolder;
    private WeaponBag _weaponBag;
    private ThirdPersonController _TPController;
    private StarterAssetsInputs _starterAssetsInputs;
    private IKController _characterIKController;

    [SerializeField, Required, AssetsOnly] private GameObject _energyExplosion;
#endregion

    private void Awake() {
        _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        _TPController = GetComponent<ThirdPersonController>();
        _weaponBag = GetComponentInChildren<WeaponBag>();
        _characterIKController = GetComponentInChildren<IKController>();
    }

    private void Start() {
        InitializeWeaponInHand();
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
        _TPController.SetWeaponIK(weaponIK);
        _characterIKController.SetLeftHandIK(weaponIK.LeftHandIKTransform);
    }

    private void Update() {
        if(IsAiming())
        {
            EnterAimState();
            OnAimState();
        }
        else
        {
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
        _characterAnimatorController.SetFloat("horizontal", _inputMoveX);
        _characterAnimatorController.SetFloat("vertical", _inputMoveY);
    }

    private void SetAnimationOnAimState()
    {
        _characterAnimatorController.SetBool("Armed", true);
        _characterAnimatorController.SetLayerWeight(1, 1);
    }

    private void SwitchCameraToAimMode() { _aimVirtualCamera.gameObject.SetActive(true); }

    private void SwitchMotionControlToAimMode()
    {
        _TPController.Sensitivity = _aimSensitivity;
    }

    private void OnAimState()
    {
        MoveOnAimState();

        Vector3 aimPoint = GetAimPoint();
        bool inAimingAnimation = _characterAnimatorController.GetCurrentAnimatorStateInfo(0).shortNameHash == _animIDAiming;
        if(inAimingAnimation && _TPController.Grounded && PrepareShoot())
        {
            Shoot(aimPoint);
            OnShootEnd();
        }
    }

    private void OnShootEnd() {_starterAssetsInputs.shoot = false;}

    private Vector3 GetAimPoint()
    {
        Vector3 aimPoint = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2, Screen.height / 2);
        Ray rayToScreenCenter = Camera.main.ScreenPointToRay(screenCenterPoint);

        if(Physics.Raycast(rayToScreenCenter, out RaycastHit hit, System.Single.MaxValue, _aimColliderLayerMask))
        {
            aimPoint = hit.point;
        }
        return aimPoint;
    }

    private void ExitAimState()
    {
        AnimationExitAimState();

        ExitCameraFromAimMode();
        ExitMotionControlFromAimMode();
    }

    private void AnimationExitAimState()
    {
        _characterAnimatorController.SetBool("Armed", false);
        _characterAnimatorController.SetLayerWeight(1, 0);
    }

    private void ExitCameraFromAimMode()
    {
        _aimVirtualCamera.gameObject.SetActive(false);
    }

    private void ExitMotionControlFromAimMode()
    {
        _TPController.Sensitivity = _normalSensitivity;
    }

    private bool PrepareShoot()
    {
        return _starterAssetsInputs.shoot == true;
    }

    private void Shoot(Vector3 aimPoint)
    {
        GameObject explosion = Instantiate(_energyExplosion, aimPoint, Quaternion.identity);
        StartCoroutine(PlayExplosion(explosion.GetComponent<ParticleSystem>()));
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
