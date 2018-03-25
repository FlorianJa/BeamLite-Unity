using HoloToolkit.Examples.InteractiveElements;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class GlobalSyncingManager : NetworkBehaviour
{
    /// <summary>
    /// Shares the id of the NetworkPlayer which is currently writing, -1 for nobody 
    /// </summary>
    [SyncVar]
    public int NetworkPlayerIdOfWritingPlayer;


    [SyncVar(hook = "OnChangeTodo1State")]
    public bool Todo1State;

    [SyncVar(hook = "OnChangeTodo2State")]
    public bool Todo2State;

    [SyncVar(hook = "OnChangeTodo3State")]
    public bool Todo3State;

    public InteractiveToggle Todo1;
    public TextMeshPro Todo1Label;
    public string Todo1TrueText, Todo1FalseText;
    public InteractiveToggle Todo2;
    public TextMeshPro Todo2Label;
    public string Todo2TrueText, Todo2FalseText;
    public InteractiveToggle Todo3;
    public TextMeshPro Todo3Label;
    public string Todo3TrueText, Todo3FalseText;

    public void OnChangeTodo1State(bool state)
    {
        if(Todo1 != null)
        {
            Todo1.HasSelection = state;
            Todo1Label.text = state ? Todo1TrueText: Todo1FalseText;
        }
    }

    public void OnChangeTodo2State(bool state)
    {
        if (Todo2 != null)
        {
            Todo2.HasSelection = state;
            Todo2Label.text = state ? Todo2TrueText : Todo2FalseText;
        }
    }

    public void OnChangeTodo3State(bool state)
    {
        if (Todo3 != null)
        {
            Todo3.HasSelection = state;
            Todo3Label.text = state ? Todo3TrueText : Todo3FalseText;
        }
    }
}
