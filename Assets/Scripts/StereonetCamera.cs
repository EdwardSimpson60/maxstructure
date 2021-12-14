﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StereonetCamera : MonoBehaviour
{
    public static StereonetCamera instance;

    public Camera cam;

    private void Awake()
    {
        instance = this;
    }

    private void OnDisable()
    {
        cam.Render();
    }

    public void Toggle()
    {
        cam.enabled = !cam.enabled;
    }

    public void UpdateStereonet()
    {
        StartCoroutine(UpdateStereonetCoroutine());
    }

    public void UpdateStereonetImmediate()
    {
        cam.Render();
    }

    private IEnumerator UpdateStereonetCoroutine()
    {
        yield return new WaitForEndOfFrame();
        cam.Render();
    }

}