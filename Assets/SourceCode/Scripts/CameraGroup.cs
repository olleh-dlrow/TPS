using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;

public class CameraGroup : MonoBehaviour
{
    private void Start() {
        var TPController = GameObject.FindObjectOfType<ThirdPersonController>();
        Debug.Assert(TPController);

        var vCameras = transform.GetComponentsInChildren<CinemachineVirtualCamera>(true);
        foreach(var vc in vCameras)
        {
            vc.Follow = TPController.VCameraFollowTarget;
        }
    }
}
