using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Elevator : MonoBehaviour
{
    public bool IsRunning
    {
        get => _isRunning;
    }
    [SerializeField] private bool _isRunning = false;
    [SerializeField] private float _maxSpeed = 5f;
    [SerializeField] private float _speed;
    [SerializeField] private int _dir = 1;
    [SerializeField] private float _maxDist = 50f;
    private float _curDist = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        if(_isRunning)
        {
            float offset = _dir * _speed * Time.deltaTime;
            transform.position += new Vector3(0f, offset, 0f);
            _curDist += Mathf.Abs(offset);

            if(_curDist >= _maxDist)
            {
                TurnOff();
            }
        }
    }

    public void TurnOn()
    {
        _isRunning = true;
        _speed = _maxSpeed;
    }

    public void TurnOff()
    {
        _isRunning = false;
        _speed = 0f;
        _curDist = 0f;
        _dir *= -1;
    }
}
