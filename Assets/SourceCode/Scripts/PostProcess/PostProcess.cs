using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Sirenix.OdinInspector;

[ExecuteInEditMode]
public class PostProcess : MonoBehaviour
{
    [SerializeField] private List<MonoBehaviour> _effects = new List<MonoBehaviour>();

    private void Reset() 
    {
        Init();
    }

    private void Awake() 
    {
        Init();
    }

    private void Init()
    {
        // _effects.AddRange(new MonoBehaviour[]{            
        //     GetComponent<Fog>(),
        //     GetComponent<Scanner>(),
        //     GetComponent<BulletTime>()});
    }

    private void Update() 
    {
        
    }

	[ImageEffectOpaque]
    private void OnRenderImage(RenderTexture src, RenderTexture dest) 
    {
        RenderTexture orgSrc = src;

        foreach(var effect in _effects)
        {            
            if(effect && effect.enabled)
            {
                var postProcess = effect as IPostProcessEffect;
                if(postProcess != null)
                {
                    postProcess.RenderEffect(src, dest);
                    RenderTexture tmp = src;
                    src = dest;
                    dest = tmp;
                }
            }
        }
        if(orgSrc == src)
            Graphics.Blit(src, dest);
    }
}
