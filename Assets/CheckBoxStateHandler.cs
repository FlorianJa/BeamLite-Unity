using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckBoxStateHandler : MonoBehaviour {

    public NetworkPlayer NetworkPlayer;


    public void SetTodo1State(bool state)
    {
        if (NetworkPlayer != null)
        {
            NetworkPlayer.SetTodo1State(state);
        }
    }

    public void SetTodo2State(bool state)
    {
        if (NetworkPlayer != null)
        {
            NetworkPlayer.SetTodo2State(state);
        }
    }

    public void SetTodo3State(bool state)
    {
        if (NetworkPlayer != null)
        {
            NetworkPlayer.SetTodo3State(state);
        }
    }
}
