using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuButton : MonoBehaviour {

    public int         rows, cols;
    public Vector3     offset;
    public TextMeshPro best;

    private void Awake() { best.text = ""; }

    private void OnMouseDown() {
        GameManager.Instance.shuffle(rows, cols, offset);
        GameManager.Instance.bestTimer = GetComponentInChildren<Timer>();
    }
}
