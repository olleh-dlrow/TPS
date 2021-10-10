using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.InputSystem;

public class ThirdPersonShootController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera aimVirtualCamera;
    [SerializeField] private float normalSensitivity = 0.5f;
    [SerializeField] private float aimSensitivity = 0.5f;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform debugTransform;
    [SerializeField] private Transform pfBulletProjectile;
    [SerializeField] private Transform spawnBulletPosition;
    public Transform VFXGreen;
    public Transform VFXRed;

    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;
    private Animator animator;
    Transform hitTransform;
    Vector3 rayCastHitPoint;

    private void Awake() {
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        thirdPersonController = GetComponent<ThirdPersonController>();
        animator = GetComponent<Animator>();
    }
    private void Update()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        if(Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderLayerMask))
        {
            rayCastHitPoint = raycastHit.point;
            debugTransform.position = raycastHit.point;
            mouseWorldPosition = raycastHit.point;
            hitTransform = raycastHit.transform;
        }

        if(starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.Sensitivity = aimSensitivity;
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f));

            Vector3 worldAimTarget = mouseWorldPosition;
            worldAimTarget.y = transform.position.y;
            Vector3 aimDirection = (worldAimTarget - transform.position).normalized;

            transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
        } else 
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.Sensitivity = normalSensitivity;
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f));
        }

        if(starterAssetsInputs.shoot)
        {
            //Vector3 aimDir = (mouseWorldPosition - spawnBulletPosition.position).normalized;
            //Instantiate(pfBulletProjectile, spawnBulletPosition.position, Quaternion.LookRotation(aimDir, Vector3.up));

            if(hitTransform.TryGetComponent<BulletTarget>(out var t))
            {
                Instantiate(VFXGreen, rayCastHitPoint, Quaternion.identity);
                Destroy(t.gameObject);
            } else 
            {
                Instantiate(VFXRed, rayCastHitPoint, Quaternion.identity);
            }
            // Debug.Log(hitTransform.name);

            starterAssetsInputs.shoot = false;
        }
    }

    private void OnGUI() {
        GUILayout.Label("Raycast Hit: " + rayCastHitPoint);
        if(hitTransform) GUILayout.Label("hit trans: " + hitTransform.position);
    }
}
