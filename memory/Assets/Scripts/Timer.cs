using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Timer : MonoBehaviour {
    [NonSerialized] public bool running;

    private TextMeshPro _timerText;
    private float       _startTime;
    private float       _count;

    public float Count {
        get => _count;
        set {
            _count             = value;
            if(_timerText) _timerText.text    = Count.ToString("F2");
        }
    }
    
    void Awake() {
        Count      = 0;
        _timerText = GetComponent<TextMeshPro>();
        _startTime = Time.time;
    }

    void Update() {
        if (running)
            Count = Time.time - _startTime;
    }

    public void Reset() {
        _startTime = Time.time;
        Count      = 0;
    }
}
