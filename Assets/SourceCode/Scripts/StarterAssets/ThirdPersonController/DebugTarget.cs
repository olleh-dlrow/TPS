using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTarget : MonoBehaviour
{
    public Color targetColor = new Color(0.6f, 0.6f, 0.8f, 0.3f);
    public float radius = 0.3f;
    private void OnDrawGizmos() {
        Gizmos.color = targetColor;
        Gizmos.DrawSphere(transform.position, radius);
    }
    private void Update() {
        
    }
}
