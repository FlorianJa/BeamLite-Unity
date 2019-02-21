using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using UnityEngine.Networking.Match;

public class HoloViveNetworkManager : NetworkManager
{
    public class CustomMsgType
    {
        public static short AddHybridPlayer = MsgType.Highest + 1;
    };

    public class AddHybridPlayerMessage : AddPlayerMessage
    {
        public Utils.PlayerType PlayerType;
        public Vector3 AlignmentPosition;
        public float AlignmentRotation;
    }

    public GameObject networkPlayer;
    //public GameObject vrPlayerPrefab;
    //public GameObject[] holoLensPlayerPrefab;
    //public AlignmentManager alignmentManager;
    public EventAnnouncer eventAnnouncer;
    public GameObject playersContainer;

    public delegate void AttemptingConnectionHandler();
    public delegate void ConnectionEstablishedHandler();
    public delegate void ConnectionLostHandler(bool willRetry);

    public event AttemptingConnectionHandler AttemptingConnection;
    public event ConnectionEstablishedHandler ConnectionEstablished;
    public event ConnectionLostHandler ConnectionLost;

    //private bool triggeredAttemptingConnection = false;
    //private bool triggeredConnectionLost = false;

    private static string DEFAULT_MATCH_NAME = "default";
    private static string DEFAULT_MATCH_PASSWORD = "";
    private static uint MATCH_SIZE = 5;

    private short playerCounter = 0;

    private Transform markerTransform;
    private float alignmentYRotation;
    private Vector3 markerOffset;

    public void Start()
    {
        StartMatchMaker();
        eventAnnouncer.gameObject.SetActive(true);

        if (Utils.IsVR)
        {
            CheckForExsitingMatches();
        }
        else if (Utils.IsHoloLens)
        {
            VuMarkHandler.Instance.VuMarkDetected += VuMarkHandler_VuMarkDetected; 
        }
    }

    private void CheckForExsitingMatches()
    {
        matchMaker.ListMatches(0, 5, DEFAULT_MATCH_NAME, false, 0, 0, this.OnMatchList);
    }

    /// <summary>
    /// Callback function for VuMarkDetection
    /// </summary>
    /// <param name="markerTransform"></param>
    /// <param name="headsetTransform"></param>
    /// <param name="playerRotation"></param>
    private void VuMarkHandler_VuMarkDetected(Transform markerTransform, Transform headsetTransform, float playerRotation)
    {
        this.markerTransform = markerTransform;
        alignmentYRotation = playerRotation;
        JoinDefaultMatch();
    }

    #region Server

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
        SpawnPlayer(netMsg.conn, msg.playerControllerId, msg.PlayerType, msg.AlignmentPosition, msg.AlignmentRotation);
    }

    /// <summary>
    /// Spawns a new network player and sets him up (Runs on server side)
    /// </summary>
    /// <param name="conn">Connection from where the original message came from</param>
    /// <param name="playerControllerId">ID for identifying the playerController</param>
    /// <param name="playerType">Type of the player (VR or HoloLens)</param>
    /// <param name="alignmentPosition">Offset Position for Hololens (equals the Markerposition in the HoloLens coordinate system)</param>
    /// <param name="alignmentRotation">Offset Rotation for HoloLens (equals the Markerrotation in the HoloLens coordinate system)</param>
    public void SpawnPlayer(NetworkConnection conn, short playerControllerId, Utils.PlayerType playerType, Vector3 alignmentPosition, float alignmentRotation)
    {
        Debug.Log("Spawning a " + playerType.ToString() + " player.");

        var _alignmentPosition = playerType == Utils.PlayerType.HoloLens ? alignmentPosition : playersContainer.transform.position;
        var _alignmentRotation = playerType == Utils.PlayerType.HoloLens ? alignmentRotation : playersContainer.transform.rotation.eulerAngles.y;

        GameObject player = Instantiate(networkPlayer, playersContainer.transform);

        NetworkServer.AddPlayerForConnection(conn, player, playerCounter);

        //player.GetComponent<NetworkPlayer>().alignmentTranslation = _alignmentPosition;
        //player.GetComponent<NetworkPlayer>().alignmentRotation = _alignmentRotation;
        //player.GetComponent<NetworkPlayer>().playerType = playerType;
        //player.GetComponent<NetworkPlayer>().AvatarIndex = playerCounter;

        //player.GetComponent<NetworkPlayer>().RpcSetupPlayer(_alignmentPosition, _alignmentRotation, playerType, playerCounter, markerOffset);

        if (playerType == Utils.PlayerType.HoloLens) playerCounter++;
    }

    private void CreateDefaultMatch()
    {
        matchMaker.CreateMatch(DEFAULT_MATCH_NAME, MATCH_SIZE, true, "", "", "", 0, 0, this.OnMatchCreate);
    }

    public override void OnMatchCreate(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchCreate(success, extendedInfo, matchInfo);
        if (success)
        {
            Debug.Log("Match created: " + extendedInfo);
        }
        else
        {
            Debug.LogError("Failed to create match.");
        }
    }

    public override void OnServerError(NetworkConnection conn, int errorCode)
    {
        base.OnServerError(conn, errorCode);
        Debug.LogError("A server error occurred: " + errorCode);
    }

    #endregion

    #region Client

    /// <summary>
    /// Joins default match, Matchname = DEFAULT_MATCH_NAME, no password
    /// </summary>
    private void JoinDefaultMatch()
    {
        if (!matchMaker)
        {
            StartMatchMaker();
        }
        //Get all Matches, filter by name
        matchMaker.ListMatches(0, 5, DEFAULT_MATCH_NAME, false, 0, 0, this.OnMatchList);
    }

    /// <summary>
    /// Gets the MatchList from MatchMakingServers. 
    /// If there is no match and current Player is a VR Player, create a new Match. 
    /// If there is no match and current Player is a HoloLens Player, start over.
    /// </summary>
    /// <param name="success"></param>
    /// <param name="extendedInfo"></param>
    /// <param name="matchList"></param>
    public override void OnMatchList(bool success, string extendedInfo, List<MatchInfoSnapshot> matchList)
    {
        base.OnMatchList(success, extendedInfo, matchList);

        if (success)
        {
            if (matchList.Count > 0)
            {
                Debug.Log("Found " + matchList.Count + " matches:");
                foreach (MatchInfoSnapshot match in matchList)
                {
                    Debug.Log("Match: " + match.name + " (" + match.currentSize + ")");
                }

                // Join the first match found
                matchMaker.JoinMatch(matchList[0].networkId, DEFAULT_MATCH_PASSWORD, "", "", 0, 0, OnMatchJoined);
            }
            //no Matches found
            else
            {
                if (Utils.IsVR)
                {
                    //If VR cant find a match, create default Match
                    CreateDefaultMatch();
                    RegisterServerMessageHandlers();
                }
                else
                {
                    //try joining again
                    Debug.LogWarning("Found no matches. Refreshing match list...");
                    JoinDefaultMatch();
                }
            }
        }
        else
        {
            Debug.LogError("Failed to list matches: " + extendedInfo);
        }
    }

    public override void OnMatchJoined(bool success, string extendedInfo, MatchInfo matchInfo)
    {
        base.OnMatchJoined(success, extendedInfo, matchInfo);
        if (success)
        {
            Debug.Log("Joined match: " + extendedInfo);
        }
        else
        {
            Debug.LogError("Failed to join match.");
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        Debug.Log("Connected to server!");
        if (ConnectionEstablished != null) ConnectionEstablished();

        AddHybridPlayerMessage msg = new AddHybridPlayerMessage();
        msg.playerControllerId = 0;
        msg.PlayerType = Utils.CurrentPlayerType;
        if (msg.PlayerType == Utils.PlayerType.HoloLens)
        {
            msg.AlignmentPosition = markerTransform.position;
            msg.AlignmentRotation = alignmentYRotation;
        }

        conn.Send(CustomMsgType.AddHybridPlayer, msg);
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        Debug.LogWarning("Disconnected from server! Rejoining default match...");

        JoinDefaultMatch();
    }

    public override void OnClientError(NetworkConnection conn, int errorCode)
    {
        base.OnClientError(conn, errorCode);
        Debug.LogError("Client error: " + errorCode);
    }

    #endregion
}
