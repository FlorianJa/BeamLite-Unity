using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Vuforia;

public class UpdateCursorColor : MonoBehaviour {

    private VuMarkManager _vuMarkManager;
    private Renderer[] _renderer;
    public Material DefaultMaterial;
    public Material TrackingMaterial;

    // Use this for initialization
    void Start () {

        _vuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        _vuMarkManager.RegisterVuMarkDetectedCallback(OnVuMarkDetected);
        _vuMarkManager.RegisterVuMarkLostCallback(OnVuMarkLost);
        VuMarkHandler.Instance.VuMarkDetected += Instance_VuMarkDetected;

        _renderer = GetComponentsInChildren<Renderer>();
    }

    private void Instance_VuMarkDetected(Transform Marker, Transform Headset, float alignmentRotation)
    {
        OnVuMarkLost(null);
    }

    private void OnVuMarkLost(VuMarkTarget obj)
    {
        if (_renderer.Length > 0)
        {
            foreach (var renderer in _renderer)
            {
                renderer.material = DefaultMaterial;
            }
        }
    }

    private void OnVuMarkDetected(VuMarkTarget obj)
    {
        if (_renderer.Length > 0)
        {
            foreach (var renderer in _renderer)
            {
                renderer.material = TrackingMaterial;
            }
        }
    }

    
}
