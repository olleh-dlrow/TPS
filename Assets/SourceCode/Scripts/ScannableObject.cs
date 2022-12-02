using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

[RequireComponent(typeof(Renderer))]
public class ScannableObject: MonoBehaviour
{
    public Material ScanMat;
    public bool ScanBoost 
    {
        get => _scanBoost;
    }

    private ScanCamera _scanCamera;
    private float _transparency;
    private Material _orgMat;
    private Material _scanMatInst;
    private Renderer _renderer;
    private bool _scanBoost;

    private void Awake() {
        Debug.Assert(_renderer = GetComponent<Renderer>());
        _orgMat = _renderer.material;
        Debug.Assert(ScanMat);
        _scanMatInst = new Material(ScanMat);
    }

    private void Start() 
    {
        _scanCamera = GameObject.FindObjectOfType<ScanCamera>();
        Debug.Assert(_scanCamera != null);

        Color color = _scanMatInst.GetColor("_OcclusionColor");
        _transparency = color.a;

        color.a = 0f;
        _scanMatInst.SetColor("_OcclusionColor", color);        
    }
    private void Update() 
    {
        Color color = _scanMatInst.GetColor("_OcclusionColor");
        float a = color.a;
        if(_scanBoost)
        {
            a = Mathf.MoveTowards(a, _transparency, _scanCamera.GradientSpeed);
        }
        else
        {
            a = Mathf.MoveTowards(a, 0f, _scanCamera.GradientSpeed);
        }
        color.a = a;
        _scanMatInst.SetColor("_OcclusionColor", color);
    }

    public void Org2Scan()
    {
        _renderer.material = _scanMatInst;
    }

    public void Scan2Org()
    {
        _renderer.material = _orgMat;
    }

    public void OnScanBegin()
    {
        _scanBoost = true;
    }

    public void OnScanEnd()
    {
        _scanBoost = false;
    }
}
