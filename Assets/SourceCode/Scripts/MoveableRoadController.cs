/**
    可移动道路控制器
    玩法性组件
    可控制MoveableRoad的旋转和上下移动
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using StarterAssets;

public class MoveableRoadController : MonoBehaviour
{
    [SerializeField] MoveableRoad _road;
    [SerializeField, ReadOnly] bool _isControlled;
    [SerializeField, ReadOnly] bool _isInRange;

    private StarterAssetsInputs _input;
    // Start is called before the first frame update
    void Start()
    {
        if(!_road)Debug.LogWarning("Can't find road");
    }

    // Update is called once per frame
    void Update()
    {
        if(!_road)return;

        if(_isInRange)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                if(!_isControlled)
                {
                    _input.ChangeInputResponse("Move", false);
                    _isControlled = true;
                }
                else
                {
                    _isControlled = false;
                    _input.ChangeInputResponse("Move", true);
                }
            }
            else
            {
                if(_isControlled)
                {
                    ControlMove();
                }
            }
        }
    }

    private void ControlMove()
    {
        if(Input.GetKey(KeyCode.W))
        {
            _road.CurrentState = MoveableRoad.ControlState.Shift;
        }
        else if(Input.GetKey(KeyCode.S))
        {
            _road.CurrentState = MoveableRoad.ControlState.Drop;
        }
        else if(Input.GetKey(KeyCode.A))
        {
            _road.CurrentState = MoveableRoad.ControlState.AntiClockWise;
        }
        else if(Input.GetKey(KeyCode.D))
        {
            _road.CurrentState = MoveableRoad.ControlState.ClockWise;
        }
        else
        {
            _road.CurrentState = MoveableRoad.ControlState.Idle;
        }
    }

    private void OnTriggerEnter(Collider other) 
    {
        if(_input || other.TryGetComponent<StarterAssetsInputs>(out _input))
        {
            _isInRange = true;
        }
    }

    private void OnTriggerExit(Collider other) {
        if(_input || other.TryGetComponent<StarterAssetsInputs>(out _input))
        {
            _isInRange = false;
        }
    }
}
