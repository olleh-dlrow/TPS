using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class FieldOfview : MonoBehaviour
{
    [SerializeField] public LayerMask layrmask;//只和指定物体互动,选择指定物体的层级就行
    private Mesh mesh;
    private float fov;
    private Vector3 origin;
    private void Start()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
        fov = 60;
        origin = Vector3.zero;//端点
    }

    private void FixedUpdate()
    {
        int rayCount = 50; //射线数量
        float angle = 0;
        float angleIncrease = fov / rayCount;
        float viewDistance = 50;//视野距离

        Vector3[] vertices = new Vector3[rayCount + 1 + 1];
        Vector2[] uv = new Vector2[vertices.Length];
        int[] triangles = new int[rayCount * 3];

        vertices[0] = origin;

        int vertexIndex = 1;
        int triangleIndex = 0;
        for (int i = 0; i < rayCount; i++)
        {
            Vector3 vertex;
            RaycastHit2D raycastHid2D = Physics2D.Raycast(origin, GetVectorFroAngle(angle), viewDistance);
            //RaycastHit2D raycastHid2D = Physics2D.Raycast(origin, GetVectorFroAngle(angle), viewDistance, layrmask);//需要只和指定物体互动
            if (raycastHid2D.collider == null)
            {
                vertex = origin + GetVectorFroAngle(angle) * viewDistance;

            }
            else
            {
                vertex = raycastHid2D.point;
            }
            vertices[vertexIndex] = vertex;
            if (i > 0)
            {
                triangles[triangleIndex + 0] = 0;
                triangles[triangleIndex + 1] = vertexIndex - 1;
                triangles[triangleIndex + 2] = vertexIndex;

                triangleIndex += 3;
            }
            vertexIndex++;
            angle -= angleIncrease;
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }



    public Vector3 GetVectorFroAngle(float angle)
    {
        float angleRad = angle * (Mathf.PI / 180);
        return new Vector3(Mathf.Cos(angleRad), Mathf.Sin(angleRad));
    }

}