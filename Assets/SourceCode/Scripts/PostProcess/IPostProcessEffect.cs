using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPostProcessEffect
{
    public void RenderEffect(RenderTexture src, RenderTexture dest);
}
