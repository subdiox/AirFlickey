using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialScrollController : MonoBehaviour {
    [HideInInspector]
    public bool isDialing;
    public Scrollbar targetScrollbar;
    public bool isFixedCenter = false;

    // Use this for initialization
    void Start() {
        isDialing = false;
    }

    public void StartDialing() {
        if (!isDialing) {
            isDialing = true;
            CalculateCircle.Initialize();
        }
    }

    public void StopDialing() {
        isDialing = false;
    }

    public void Dialing(Vector3 vec) {
        if (isDialing) {
            GetCalculateCircle(vec);
        }
    }

    void GetCalculateCircle(Vector3 vec) {
        CalculateCircleData data = null;

        data = CalculateCircle.GetCalculateCircleDataPertmpTarce(vec.x, vec.y);
        
        if (data.isCalculated) {
            data.HandPos = vec;
            ShowData(data);
        }
    }

    void ShowData(CalculateCircleData data) {
        Debug.Log(data.addAngle);
        targetScrollbar.value = Mathf.Clamp((targetScrollbar.value - (data.addAngle * 0.002f)), 0f, 1f);
    }
}