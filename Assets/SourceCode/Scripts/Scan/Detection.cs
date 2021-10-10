using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    public Transform controller;
    public float radius = 5f;
    private void Awake() {
    }

    private void Update() {
        if(Vector3.Distance(controller.position, transform.position) < radius)
        {
            Debug.Log(name + " collide " + controller.name);
            controller.position = Vector3.zero;
        }
    }


    private void OnDrawGizmos() {
        Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
