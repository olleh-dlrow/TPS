

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using StarterAssets;

public class Character : MonoBehaviour
{
    public ThirdPersonController TPController
    {
        get => _TPController;
    }

    private ThirdPersonController _TPController;

    private static List<Character> _characters = new List<Character>();

    private void Awake() {
        foreach(var ch in _characters)
        {
            if(ch == this)return;
        }
        _characters.Add(this);

        _TPController = GetComponentInParent<ThirdPersonController>();
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Character GetCharacter(int index)
    {
        Debug.Assert(index >= 0 && index < _characters.Count);
        return _characters[index];
    }
}
