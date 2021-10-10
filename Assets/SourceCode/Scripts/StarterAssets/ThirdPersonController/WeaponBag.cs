using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBag : MonoBehaviour
{
    [SerializeField] List<Transform> weaponTransformList;
    private void Awake() {
        for(int i = 0; i < transform.childCount; i++)
        {
            weaponTransformList.Add(transform.GetChild(i));
        }       
    }
    public Transform GetCurrentWeaponTransform()
    {
        Debug.AssertFormat(weaponTransformList.Count > 0, "error");
        return weaponTransformList[0];
    }
}
