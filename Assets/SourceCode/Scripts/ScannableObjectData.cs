/*
    废弃的方法，不要使用！
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="ScanObjData")]
public class ScannableObjectData: ScriptableObject
{
    public float Speed = 0.1f;
    public float Duration = 3f;
    public static ScannableObjectData Instance
    {
        get 
        {
            if(!_instance)
            {
                var SODatas = Resources.FindObjectsOfTypeAll<ScannableObjectData>();
                if(SODatas.Length == 0)
                {
                    _instance = Resources.Load<ScannableObjectData>("ScriptableObject/ScannableObjectData.asset");
                }
                else
                {
                    _instance = SODatas[0];

                }
            }
            return _instance;
        }
    }

    private static ScannableObjectData _instance;    
}
