using HoloToolkit.UI.Keyboard;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;


public class BeamLiteNetworkManager : NetworkManager
{

    public class CustomMsgType
    {
        public static short AddHybridPlayer = MsgType.Highest + 1;
    };

    public class AddHybridPlayerMessage : AddPlayerMessage
    {
        public Utils.PlayerType PlayerType;
        public Vector3 AlignmentTranslation;
        public float AlignmentRotation;
        public string localPlayerName;
    }

    public string LocalPlayerName = string.Empty;

    public GameObject networkPlayer;

    public delegate void AttemptingConnectionHandler();
    public delegate void ConnectionEstablishedHandler();
    public delegate void ConnectionLostHandler(bool willRetry);

    public event AttemptingConnectionHandler AttemptingConnection;
    public event ConnectionEstablishedHandler ConnectionEstablished;
    public event ConnectionLostHandler ConnectionLost;

    public GameObject playersContainer;
    public Transform MarkerOffset;

    public bool ConnectHoloLensWithoutMarkerScanning;
    public BeamLiteNetworkDiscovery NetworkDiscovery;
    public bool ServerAddressIsSet = false;


    private short playerCounter = 0;
    private Vector3 _alignmentTranslation;
    private float _alignmentYRotation;
    private bool _alignmentIsSet = false;

    // Use this for initialization
    void Start ()
    {
        if (Utils.IsHoloLens)
        {
            if (ConnectHoloLensWithoutMarkerScanning)
            {
                _alignmentTranslation = new Vector3();
                _alignmentYRotation = 0;
            }
            else
            {

                VuMarkHandler.Instance.VuMarkDetected += VuMarkHandler_VuMarkDetected;
            }

            Keyboard.Instance.OnTextSubmitted += Instance_OnTextSubmitted;
        }
    }

    private void Instance_OnTextSubmitted(object sender, EventArgs e)
    {
        var keyboard = sender as Keyboard;

        this.LocalPlayerName = keyboard.InputField.text;
        JoinMatch();
    }

    public void JoinMatch()
    {
        if (ServerAddressIsSet)
        {
            if (Utils.IsVR)
            {
                StartClient(); 
            }
            else
            {
                if (_alignmentIsSet || ConnectHoloLensWithoutMarkerScanning)
                {
                    StartClient();
                }
            }
        }
    }

    public void CreateMatch()
    {
        StartHost();
        RegisterServerMessageHandlers();

    }

    private void VuMarkHandler_VuMarkDetected(Transform Marker, Transform Headset, float playerRotation)
    {
        _alignmentTranslation = Marker.position;
        _alignmentYRotation = playerRotation;
        _alignmentIsSet = true;
        if (ServerAddressIsSet)
        {
            StartClient();
        }
    }


    /// <summary>
    /// Register handler for AddHybridPlayer Custommessage.
    /// </summary>
    private void RegisterServerMessageHandlers()
    {
        NetworkServer.RegisterHandler(CustomMsgType.AddHybridPlayer, OnAddHybridPlayerMessage);
    }


    /// <summary>
    /// Handler for AddHybridPlayer Custommessage
    /// </summary>
    /// <param name="netMsg">NetworkMessage</param>
    private void OnAddHybridPlayerMessage(NetworkMessage netMsg)
    {
        var msg = netMsg.ReadMessage<AddHybridPlayerMessage>();
        SpawnPlayer(netMsg.conn, msg.playerControllerId, msg.PlayerType, msg.AlignmentTranslation, msg.AlignmentRotation, msg.localPlayerName);
    }


    /// <summary>
    /// Spawns a new network player and sets him up (Runs on server side)
    /// </summary>
    /// <param name="conn">Connection from where the original message came from</param>
    /// <param name="playerControllerId">ID for identifying the playerController</param>
    /// <param name="playerType">Type of the player (VR or HoloLens)</param>
    /// <param name="alignmentPosition">Offset Position for Hololens (equals the Markerposition in the HoloLens coordinate system)</param>
    /// <param name="alignmentRotation">Offset Rotation for HoloLens (equals the Markerrotation in the HoloLens coordinate system)</param>
    public void SpawnPlayer(NetworkConnection conn, short playerControllerId, Utils.PlayerType playerType, Vector3 alignmentPosition, float alignmentRotation, string localPlayerName)
    {
        Debug.Log("Spawning a " + playerType.ToString() + " player.");

        var _alignmentTranslation = playerType == Utils.PlayerType.HoloLens ? alignmentPosition : playersContainer.transform.position;
        var _alignmentRotation = playerType == Utils.PlayerType.HoloLens ? alignmentRotation : playersContainer.transform.rotation.eulerAngles.y;

        GameObject player = Instantiate(networkPlayer, playersContainer.transform);

        NetworkServer.AddPlayerForConnection(conn, player, playerCounter);

        player.GetComponent<NetworkPlayer>().RpcSetupPlayer(_alignmentTranslation, _alignmentRotation, playerType, playerCounter, MarkerOffset.position, localPlayerName);

        player.GetComponent<NetworkPlayer>().PlayerName = localPlayerName;

        if (playerType == Utils.PlayerType.HoloLens) playerCounter++;
    }



    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        Debug.Log("Connected to server!");
        if (ConnectionEstablished != null) ConnectionEstablished();

        AddHybridPlayerMessage msg = new AddHybridPlayerMessage();
        msg.playerControllerId = 1;
        msg.PlayerType = Utils.CurrentPlayerType;
        msg.localPlayerName = LocalPlayerName;
        if (msg.PlayerType == Utils.PlayerType.HoloLens)
        {
            msg.AlignmentTranslation = _alignmentTranslation;
            msg.AlignmentRotation = _alignmentYRotation;
        }

        conn.Send(CustomMsgType.AddHybridPlayer, msg);
    }
}
