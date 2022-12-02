using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Timer : MonoBehaviour
{
    struct TimerInfo: IComparable<TimerInfo>
    {
        public float TargetTime;
        public Action OnTimerEnd;

        public TimerInfo(float targetTime, Action onTimerEnd)
        {
            TargetTime = targetTime;
            OnTimerEnd = onTimerEnd;
        }

        public int CompareTo(TimerInfo obj)
        {
            return (int)(obj.TargetTime - TargetTime);
        }

        public override string ToString()
        {
            return "TargetTime: " + TargetTime;
        }
    }
    
    private static Timer _instance;

    private List<TimerInfo> _timers = new List<TimerInfo>();
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(_timers.Count > 0)
        {
            var first = _timers[_timers.Count - 1];
            if(Time.time >= first.TargetTime)
            {
                first.OnTimerEnd.Invoke();
                _timers.RemoveAt(_timers.Count - 1);
            }    
        }
    }

    public static void SetTimer(float value, Action onTimerEnd)
    {
        if(!_instance)
        {
            var obj = new GameObject();
            _instance = obj.AddComponent<Timer>();
        }

        _instance._timers.Add(new TimerInfo(Time.time + value, onTimerEnd));
        _instance._timers.Sort();
    }

    public static void SetTimer(float value, float cycle, Action onCircleEnd, Action onTimerEnd)
    {
        if(!_instance)
        {
            var obj = new GameObject();
            _instance = obj.AddComponent<Timer>();            
        }

        int cnt = Mathf.FloorToInt(value / cycle);
        for(int i = 1; i <= cnt; ++i)
        {
            _instance._timers.Add(new TimerInfo(Time.time + i * cycle, onCircleEnd));
        }
        _instance._timers.Add(new TimerInfo(Time.time + value, onTimerEnd));
        _instance._timers.Sort();
    }

    private void OnGUI() {
        foreach(var info in _instance._timers)
        {
            GUILayout.Label(info.ToString());
        }
    }
}
