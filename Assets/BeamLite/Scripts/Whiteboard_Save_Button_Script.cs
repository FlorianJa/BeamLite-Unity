using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.UnityEventHelper;

public class Whiteboard_Save_Button_Script : MonoBehaviour, IInputClickHandler
{

    public Whiteboard whiteboard;
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
        Save();
    }


    public void OnInputClicked(InputClickedEventData eventData)
    {
        Save();
    }

    private void Save()
    {
        if (whiteboard != null)
        {
            whiteboard.Save();
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (whiteboard.saved)
        {
            GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
