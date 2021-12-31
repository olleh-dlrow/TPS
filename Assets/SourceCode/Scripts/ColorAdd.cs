using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ColorAdd : MonoBehaviour
{
    public RenderTexture texture;
    public Material mat;
    // Start is called before the first frame update
    void Start()
    {
        var camera = GetComponent<Camera>();
        camera.depthTextureMode |= DepthTextureMode.Depth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnRenderImage(RenderTexture src, RenderTexture dest) {
        if(mat)
        {
            
            Graphics.Blit(src, dest, mat);
        }
    }
}
