using HoloToolkit.UI.Keyboard;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoinButton : MonoBehaviour {

    private Button buttonScript;
    private KeyboardInputField textInput;
    public BeamLiteNetworkManager NetworkManager;

    // Use this for initialization
    void Start () {
        buttonScript = GetComponent<Button>();
        textInput = GameObject.FindGameObjectWithTag("NameInput").GetComponent<KeyboardInputField>();
	}


    public void SetButtonInteractable()
    {
        if (textInput.text != string.Empty)
        {
            NetworkManager.LocalPlayerName = textInput.text;
            buttonScript.interactable = true;
        }
    }

}
