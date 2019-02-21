using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
using VRTK;

public class SizechangeScript : MonoBehaviour, IInputClickHandler
{

    public SizeController controller;
    public bool up;
    // Use this for initialization
    void Start()
    {

#if !UNITY_WSA
        GetComponent<VRTK_InteractableObject>().InteractableObjectTouched += Save;
        GetComponent<VRTK_InteractableObject>().InteractableObjectUsed += Save;
#endif
    }

    private void Save(object sender, InteractableObjectEventArgs e)
    {
        updown();
    }


    public void OnInputClicked(InputClickedEventData eventData)
    {
        updown();
    }

    private void updown()
    {
        if (controller != null)
        {
            controller.updown(up);
        }
    }

}
