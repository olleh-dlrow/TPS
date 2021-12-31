/**
    IK控制器
    控制瞄准时双臂的移动和胸腔的移动
    视觉性功能
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class CharacterIKController : MonoBehaviour
{
    private StarterAssetsInputs _input;
    private ThirdPersonController _TPController;
    private Transform _leftHandIK;
    private Animator _animator;

    private void Awake() {
        Debug.Assert(_animator = GetComponent<Animator>());
    }
    private void Start() 
    {
        Debug.Assert(_input = GetComponentInParent<StarterAssetsInputs>());
        Debug.Assert(_TPController = GetComponentInParent<ThirdPersonController>());
    }

    /// <summary>
    /// 在切换武器后执行，设置左手握住武器的IK
    /// </summary>
    /// <param name="ik"></param>
    public void SetLeftHandIK(Transform ik)
    {
        _leftHandIK = ik;
    }

    private void OnAnimatorIK(int layerIndex) {
        if(_input.aim)
        {
            // set chest rotation
            var cameraRotation = _TPController.DesiredCameraRotation;
            var cameraEulerAngles = cameraRotation.eulerAngles;
            var targetRotation = Quaternion.AngleAxis(cameraEulerAngles.x, Vector3.right);
            _animator.SetBoneLocalRotation(HumanBodyBones.Chest, targetRotation);

            if(_leftHandIK)
                UpdateLeftHandIK();
        }
    }

    private void UpdateLeftHandIK()
    {
        // left hand
        _animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        _animator.SetIKPosition(AvatarIKGoal.LeftHand, _leftHandIK.position);

        _animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        _animator.SetIKRotation(AvatarIKGoal.LeftHand, _leftHandIK.rotation);
    }
}
