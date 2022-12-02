using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class ElevatorController : MonoBehaviour
{
    [SerializeField] private Elevator _elevator;
    private StarterAssetsInputs _input;
    private bool _isInRange = false;
    // Start is called before the first frame update
    void Start()
    {
        if(!_elevator)
            _elevator = transform.parent.GetComponent<Elevator>();
            
        Debug.Assert(_elevator != null);
    }

    // Update is called once per frame
    void Update()
    {
        if(_isInRange)
        {
            if(!_elevator.IsRunning && Input.GetKeyDown(KeyCode.E))
            {
                _elevator.TurnOn();
            }
        }
    }

    private void OnTriggerEnter(Collider other) {
        if(_input || other.TryGetComponent(out _input))
        {
            Debug.Log("Enter Range");
            _isInRange = true;
        }

    }

    private void OnTriggerExit(Collider other) {
        if(_input || other.TryGetComponent<StarterAssetsInputs>(out _input))
        {
            Debug.Log("Exit Range");
            _isInRange = false;
        }
    }
}
