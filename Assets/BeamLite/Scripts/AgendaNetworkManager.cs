using HoloToolkit.Examples.InteractiveElements;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AgendaNetworkManager : NetworkBehaviour
{
    [SyncVar]
    public bool Todo1State;

    [SyncVar]
    public bool Todo2State;

    [SyncVar]
    public bool Todo3State;
    
}
