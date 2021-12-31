using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoseWidget : MonoBehaviour
{
    public float timer = 0f;
    public float DisplayTime = 2f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable() {
        timer = DisplayTime;
    }

    // Update is called once per frame
    void Update()
    {
        if(timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
