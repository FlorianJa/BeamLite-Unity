using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.UnityEventHelper;

public class WhiteboardClearButtonScript : MonoBehaviour, IInputClickHandler
{
    public NetworkPlayer NetworkPlayer;


    private void Start()
    {

#if !UNITY_WSA
        GetComponent<VRTK_InteractableObject>().InteractableObjectTouched += WhiteboardClearButtonScript_InteractableObjectTouched;
        GetComponent<VRTK_InteractableObject>().InteractableObjectUsed += WhiteboardPenColorChanger_InteractableObjectUsed;
#endif

    }

    private void WhiteboardPenColorChanger_InteractableObjectUsed(object sender, InteractableObjectEventArgs e)
    {
        Clear();
    }

    private void WhiteboardClearButtonScript_InteractableObjectTouched(object sender, InteractableObjectEventArgs e)
    {
        Clear();
    }

    public void OnInputClicked(InputClickedEventData eventData)
    {
        Clear();
    }

    private void Clear()
    {
        if (NetworkPlayer != null)
        {
            NetworkPlayer.ClearWhiteboard();
        }
    }
}
