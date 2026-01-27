using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAltenator : MonoBehaviour
{
    public Camera[] cameras;
    int onCam = 0;

    public string showingCamera;

    void Awake()
    {
        // disable all the cameras
        foreach (Camera c in cameras) { 
            c.enabled = false;
        }
    }

    void Update()
    {
        // Give it a second before starting
        if (Time.time < 1) return;

        // disable the previous camera
        cameras[onCam].enabled = false;

        // move to the next
        onCam++;
        onCam %= cameras.Length;

        // ... and show
        cameras[onCam].enabled = true;

        // to display which one you are viewing in the editor
        showingCamera = cameras[onCam].name;



    }
}
