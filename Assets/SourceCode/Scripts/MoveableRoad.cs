/**
    可移动道路
    玩法性组件，可以被MoveableRoadController控制
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class MoveableRoad : MonoBehaviour
{
    public enum ControlState
    {
        Idle,
        ClockWise,
        AntiClockWise,
        Shift,
        Drop,
    }

    public ControlState CurrentState 
    {
        get => _currentState;
        set => _currentState = value;
    }

    [SerializeField] private float _rotateSpeed = 10f;
    [SerializeField] private float _moveSpeed = 10f;
    [SerializeField] private float _topDistance = 5f;
    [SerializeField] private float _bottomDistance = 5f;
    [SerializeField] private float _currentMoveDistance = 0f;
    [SerializeField] private ControlState _currentState;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate() 
    {
        switch (_currentState)
        {
            case ControlState.Idle:
            break;

            case ControlState.ClockWise:
            transform.Rotate(new Vector3(0, _rotateSpeed * Time.deltaTime, 0));
            break;

            case ControlState.AntiClockWise:
            transform.Rotate(new Vector3(0, -1 * _rotateSpeed * Time.deltaTime, 0));
            break;

            case ControlState.Shift:
            if(_currentMoveDistance < _topDistance)
            {
                float shiftDistance = _moveSpeed * Time.deltaTime;
                transform.Translate(new Vector3(0, shiftDistance, 0));
                _currentMoveDistance += shiftDistance;
            }
            break;

            case ControlState.Drop:
            if(_currentMoveDistance > -1 * _bottomDistance)
            {
                float dropDistance = _moveSpeed * Time.deltaTime;
                transform.Translate(new Vector3(0, -1 * dropDistance, 0));
                _currentMoveDistance -= dropDistance;
            }
            break;

            default:
            break;
        }
    }
}
