using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharactercreationWindowButtonHandler : MonoBehaviour, IInputClickHandler
{
    public string name;
    public CharactercreationScreen Controller;
    public void OnInputClicked(InputClickedEventData eventData)
    {
        Controller.ButtonClickedHololens(name);
    }

}