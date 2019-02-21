using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyHandler : MonoBehaviour {

    public InputField TextInputField;
    public Text PlayerName;
    public BeamLiteNetworkManager NetworkManager;
    public GameObject Keyboard;
    public GameObject NetworkManagment;
    public GameObject Voice;
    public GameObject DrumStickLeft;
    public GameObject DrumStickRight;
    private bool Returned = false;

    // Use this for initialization
    void Start () {
        TextInputField.ActivateInputField();

    }
    
    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown("return"))
        {
            Debug.Log("return down");
            Keyboard.SetActive(false);
            NetworkManagment.SetActive(true);
            NetworkManager.LocalPlayerName = PlayerName.text;
            Voice.SetActive(true);
            DrumStickRight.SetActive(false);
            DrumStickLeft.SetActive(false);
            Returned = true;
        }
        if (!Returned)
        {
            TextInputField.ActivateInputField();
        }
    }
}
