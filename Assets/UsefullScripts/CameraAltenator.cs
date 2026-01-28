using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraAltenator : MonoBehaviour
{
    public bool autoGetCameras;
    public Camera[] cameras;
    int onCam = 0;

    public int perFrame = 1;
    int frameCount = 0;

    public string showingCamera;

    void Awake()
    {
        if (autoGetCameras) GetCameras();

        // disable all the cameras
        foreach (Camera c in cameras) { 
            c.enabled = false;
        }
    }
    void GetCameras() {
        cameras = GameObject.FindObjectsOfType<Camera>();

        List<Camera> list = new List<Camera>();
        foreach (Camera c in cameras)
        {
            if (c.targetTexture != null) list.Add(c);
        }
        cameras = list.ToArray();
    }
    private void OnValidate()
    {
        if (autoGetCameras) GetCameras();
    }
    void Update()
    {
        // Give it a second before starting
        if (Time.time < 1) return;

        frameCount++;

        // disable the previous camera
        cameras[onCam].enabled = false;


        if (frameCount < perFrame) return; 

        frameCount = 0;



        // move to the next
        onCam++;
        onCam %= cameras.Length;

        // ... and show
        cameras[onCam].enabled = true;

        // to display which one you are viewing in the editor
        showingCamera = cameras[onCam].name;



    }
}
