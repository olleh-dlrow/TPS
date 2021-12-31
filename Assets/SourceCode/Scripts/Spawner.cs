using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class Spawner : MonoBehaviour
{
    [SerializeField, AssetsOnly, AssetSelector] private List<GameObject> _prefabs;
    // Start is called before the first frame update
    private void Awake() {
        foreach(var prefab in _prefabs)
        {
            Instantiate(prefab);
        }
    }
    
}
