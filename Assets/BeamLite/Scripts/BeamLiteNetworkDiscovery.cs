using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class BeamLiteNetworkDiscovery : NetworkDiscovery
{
    public int TimeForServerSearching = 15;
    public BeamLiteNetworkManager NetworkManager;

    private DateTime StartTime;

    // Use this for initialization
    void Start()
    {
        StartTime = DateTime.Now;
        Initialize();
        StartAsClient();
        if(Utils.IsHoloLens)
        {
            useNetworkManager = true;
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //if is in searching mode
        if (isClient)
        {
            if (Utils.IsVR)
            {
                //if no server is found after "TimeForServerSearching", stop searching and start as server
                if ((DateTime.Now - StartTime).Seconds > TimeForServerSearching)
                {
                    Debug.Log("No Server found. Start broadcasting");

                    //StopBroadcasting stops searching
                    this.StopBroadcast();
                    //after stopping broadcasting, discovery needs to reinitialize
                    this.Initialize();
                    

                    this.NetworkManager.CreateMatch();


                    //broadcast as server
                    this.StartAsServer();

                    //this.gameObject.SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// Function which is called when a broadcast message is received
    /// </summary>
    /// <param name="fromAddress">Address of the sender of the broadcast</param>
    /// <param name="data">Data of the broadcast</param>
    public override void OnReceivedBroadcast(string fromAddress, string data)
    {
        Debug.Log("Address: " + fromAddress + " Data: " + data);
        
        this.StopBroadcast();
        this.NetworkManager.networkAddress = fromAddress;
        this.NetworkManager.ServerAddressIsSet = true;
        this.NetworkManager.JoinMatch();

        this.gameObject.SetActive(false);
    }

}
