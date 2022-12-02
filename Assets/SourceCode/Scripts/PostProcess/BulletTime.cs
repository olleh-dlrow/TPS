using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class BulletTime : MonoBehaviour, IPostProcessEffect
{
    [SerializeField] private Material _effectMat;

    [SerializeField] private Color _btColor;

    private Color _curColor = Color.white;

    [SerializeField] private float _colorGradient = 0.01f;

    private ThirdPersonController _TPController;

    private void Awake() {
        if(_effectMat)
            _effectMat = new Material(Shader.Find("BulletTimeShader"));
    }


    // Start is called before the first frame update
    void Start()
    {
        Debug.Assert(_TPController = Character.GetCharacter(0).TPController);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RenderEffect(RenderTexture src, RenderTexture dest)
    {
        if(_TPController)
        {
            if(_TPController.IsInBulletTime)
            {
                _curColor = Color.Lerp(_curColor, _btColor, _colorGradient);
            }
            else
            {
                _curColor = Color.Lerp(_curColor, Color.white, _colorGradient);
            }
            _effectMat.SetColor("_BTColor", _curColor);

            Graphics.Blit(src, dest, _effectMat);
        }
        else
        {
            Graphics.Blit(src, dest);
        }
    }
}
