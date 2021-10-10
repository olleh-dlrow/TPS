using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class IKController : MonoBehaviour
{
    [SerializeField] private StarterAssetsInputs _input;
    [SerializeField] private ThirdPersonController _TPController;
    private Transform _leftHandIK;
    private Animator _animator;

    public void SetLeftHandIK(Transform ik)
    {
        _leftHandIK = ik;
    }

    private void Awake() {
        _animator = GetComponent<Animator>();
    }
    private void OnAnimatorIK(int layerIndex) {
        if(_input.aim)
        {
            // set chest rotation
            var cameraRotation = _TPController.DesiredCameraRotation;
            var cameraEulerAngles = cameraRotation.eulerAngles;
            var targetRotation = Quaternion.AngleAxis(cameraEulerAngles.x, Vector3.right);
            _animator.SetBoneLocalRotation(HumanBodyBones.Chest, targetRotation);

            LeftHandIK();

        }
    }

    private void LeftHandIK()
    {
        // left hand
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandIK.position);

        _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        _animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandIK.rotation);
    }

    private void Update() {

    }
}
