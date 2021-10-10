using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class WeaponIK : MonoBehaviour
{
    public Transform LeftHandIKTransform {get => _leftHandIKTransform; }
    public Transform RightHandIKTransform { get=> _rightHandIKTransform; }
    [SerializeField, Required] private Transform _leftHandIKTransform;
    [SerializeField, Required] private Transform _rightHandIKTransform;
    [SerializeField] private float _sensitivity = 0.01f;
    public void LookAtTarget(float inputY)
    {
        // float up = 80f;
        // float down = -80f;
        // float angle = Mathf.Clamp(inputY * _sensitivity, down, up);
        // Debug.Log("inputy:" + inputY);
        // Debug.Log("angle:" + angle);
        // transform.Rotate(angle, 0, 0, Space.Self);
    //     Vector3 orgEulerAngles = transform.localEulerAngles;
    //     transform.LookAt(target);
    //     float x = Mathf.Lerp(orgEulerAngles.x, transform.localEulerAngles.x, weight);
    //     transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, orgEulerAngles.y, orgEulerAngles.z);
    }
}
