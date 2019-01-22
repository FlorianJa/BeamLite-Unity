using Dissonance.Integrations.UNet_HLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPlayer : NetworkBehaviour
{

    [SyncVar]
    public Utils.PlayerType PlayerType;

    [SyncVar]
    public Vector3 AlignmentTranslation;

   

    [SyncVar]
    public float AlignmentRotation;

    [SyncVar]
    public bool RightHandVisible;

    [SyncVar]
    public bool LeftHandVisible;


    [SyncVar]
    public int PlayerCounter;

    [SyncVar]
    public Vector3 MarkerOffset;


    [SyncVar]
    public string PlayerName;

    public GameObject Hand1;
    public GameObject Hand2;


    public GameObject AvatarPrefab;
    public GameObject Avatar;

    private GameObject _hmd;

    private GameObject _handTrackerGameObject;

    private Transform _viveControllerLeft;
    private Transform _viveControllerRight;

    private HoloLensHandTrackingController _holoLensHandTracker;

    private bool _lastLeftHandVisibility;



    private bool _lastRightHandVisibility;
    private GameObject[] Pens;
    public GameObject[] Whiteboards;
    private GameObject WhiteBoardButtons;
    private GameObject Agenda;
    public GlobalSyncingManager GlobalSyncingManager;
    private GameObject Clock;



    /// <summary>
    /// Use this for initialization
    /// </summary>
    void Start()
    {
        GetWhiteboards();
        GetWhiteboardButtons();
        GetAgenda();
        GetClock();
        GlobalSyncingManager = GameObject.Find("GlobalSyncingManager").GetComponent<GlobalSyncingManager>();

        //on vr spawn for every remote player an avatar
        if (Utils.CurrentPlayerType == Utils.PlayerType.VR && !isLocalPlayer)
        {
            SpawnAvatar(true);
        }
        else
        {
            if(Utils.CurrentPlayerType == Utils.PlayerType.HoloLens && this.PlayerType == Utils.PlayerType.VR)
            {
                SpawnAvatar(true);
            }
            else
            {
                SpawnAvatar(false);
            }
            

            if (PlayerType == Utils.PlayerType.HoloLens && isLocalPlayer)
            {
                SetWhiteboardTransforms();
                SetWhiteboardButtonTransforms();
                SetAgendaTransform();
                SetClockTransform();
            }

        }

        if (!isServer && isLocalPlayer)
        {
            //get the whiteboard data from server and apply them to the local whiteboard
        }

    }

    

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Get the HMD for updating the player position
        _hmd = GameObject.FindGameObjectWithTag("MainCamera");

        _handTrackerGameObject = GameObject.FindGameObjectWithTag("HoloLensHandTracker");

        GameObject _vrCameraRig = GameObject.Find("VR CameraRig");

        if (_vrCameraRig)
        {
            _viveControllerLeft = _vrCameraRig.GetComponent<SteamVR_ControllerManager>().left.transform.Find("tatzeRaccoonL");
            _viveControllerRight = _vrCameraRig.GetComponent<SteamVR_ControllerManager>().right.transform.Find("tatzeRaccoonR");

            if (_viveControllerRight) CmdUpdateHandVisibilityRight(true);
            if (_viveControllerLeft) CmdUpdateHandVisibilityLeft(true);
        }

        if (_handTrackerGameObject)
        {
            _holoLensHandTracker = _handTrackerGameObject.GetComponent<HoloLensHandTrackingController>();
        }

        Pens = GameObject.FindGameObjectsWithTag("PenVisual");

        foreach (var pen in Pens)
        {
            pen.GetComponent<NewWhiteboardPen>().NetworkPlayer = this;
            foreach(var renderer in pen.GetComponentsInChildren<MeshRenderer>())
            {
                renderer.enabled = true;
            }
        }
        GlobalSyncingManager = GameObject.Find("GlobalSyncingManager").GetComponent<GlobalSyncingManager>();
        GameObject.Find("WhiteboardButtons").GetComponentInChildren<WhiteboardClearButtonScript>().NetworkPlayer = this;
        GameObject.Find("Agenda").GetComponent<CheckBoxStateHandler>().NetworkPlayer = this;
    }



    public void SetTodo1State(bool state)
    {
        CmdSetTodo1State(state);
    }

    [Command]
    private void CmdSetTodo1State(bool state)
    {
        GlobalSyncingManager.Todo1State = state;
    }

    public void SetTodo2State(bool state)
    {
        CmdSetTodo2State(state);
    }

    [Command]
    private void CmdSetTodo2State(bool state)
    {
        GlobalSyncingManager.Todo2State = state;
    }

    public void SetTodo3State(bool state)
    {
        CmdSetTodo3State(state);
    }

    [Command]
    private void CmdSetTodo3State(bool state)
    {
        GlobalSyncingManager.Todo3State = state;
    }

    public void SetTouch(bool isTouching)
    {
        CmdSetTouch(isTouching, this.netId);
    }

    [Command]
    private void CmdSetTouch(bool isTouching, NetworkInstanceId networkInstanceId)
    {
        if (GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer <= 0 && isTouching == true)
        {
            GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer = (int)networkInstanceId.Value;
            RpcSetTouch(isTouching, (int)networkInstanceId.Value);
        }
        else if (isTouching == false && GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer == networkInstanceId.Value)
        {
            GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer = -1;
            RpcSetTouch(isTouching, -1);
        }

    }

    [ClientRpc]
    private void RpcSetTouch(bool isTouching, int networkInstanceId)
    {
        Whiteboards[0].GetComponent<Whiteboard>().SetTouching(isTouching);
    }

    public void SetTouchPosition(float x, float y, Color color, int penSize)
    {
        CmdSetTouchPosition(x, y, color, penSize);
    }

    [Command]
    private void CmdSetTouchPosition(float x, float y, Color color, int penSize)
    {
        RpcSetTouchPosition(x, y, color, penSize);
    }

    [ClientRpc]
    private void RpcSetTouchPosition(float x, float y, Color color, int penSize)
    {
        Whiteboards[0].GetComponent<Whiteboard>().SetTouchPosition(x, y, color, penSize);
    }

    public void ClearWhiteboard()
    {
        CmdClearWhiteboard();
    }

    [Command]
    private void CmdClearWhiteboard()
    {
        RpcClearWhiteboard();
    }

    [ClientRpc]
    private void RpcClearWhiteboard()
    {
        Whiteboards[0].GetComponent<Whiteboard>().Clear();
    }

    private void NetworkPlayer_PenWantToHaveAuthority(GameObject sender)
    {
        CmdNetworkPlayer_PenWantToHaveAuthority(sender);
    }

    private void NetworkPlayer_PenWantToRemoveAuthority(GameObject sender)
    {
        CmdNetworkPlayer_PenWantToRemoveAuthority(sender);
    }

    [Command]
    private void CmdNetworkPlayer_PenWantToHaveAuthority(GameObject sender)
    {
        sender.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }
    [Command]
    private void CmdNetworkPlayer_PenWantToRemoveAuthority(GameObject sender)
    {
        sender.GetComponent<NetworkIdentity>().RemoveClientAuthority(connectionToClient);
    }

    /// <summary>
    ///  Update is called once per frame
    /// </summary>
    void Update()
    {
        // transform the real player position into the VR coordinate system. 
        // If local player is VR user, alignment translation and rotation is zero. 
        // If local player is HoloLens user, AlignmentTranslation equals the position of the real marker in the HoloLens coordinate system
        // AlignmentRotation equals the rotation of the real Marker in the HoloLens coordinate system.
        // The real Marker is aligned with the VR coordinate system

        if (isLocalPlayer && _hmd)
        {
            //to get the position of the HoloLens Player in the VR coordinate system we have to subtract the AlignmentTranslation from the position of the Headset and rotate this vector by the AlignmentRotation
            if (PlayerType == Utils.PlayerType.HoloLens)
            {
                this.transform.position = Quaternion.Euler(0, AlignmentRotation, 0) * (_hmd.transform.position - AlignmentTranslation) + MarkerOffset;
            }
            else
            {
                this.transform.position = Quaternion.Euler(0, AlignmentRotation, 0) * (_hmd.transform.position - AlignmentTranslation);
            }
            this.transform.rotation = Quaternion.Euler(0, AlignmentRotation, 0) * _hmd.transform.rotation;

#warning comment missing
            if (_holoLensHandTracker)
            {
                if (_holoLensHandTracker.Hand1Tracked)
                {
                    if (_lastRightHandVisibility == false) CmdUpdateHandVisibilityRight(true);
                    Hand1.transform.position = Quaternion.Euler(0, AlignmentRotation, 0) * (_holoLensHandTracker.TrackedHandPosition1 - AlignmentTranslation) + MarkerOffset;
                    Hand1.transform.rotation = this.transform.rotation;
                    _lastRightHandVisibility = _holoLensHandTracker.Hand1Tracked;
                }
                else
                {
                    if (_lastRightHandVisibility == true) CmdUpdateHandVisibilityRight(false);
                    _lastRightHandVisibility = _holoLensHandTracker.Hand1Tracked;
                }


                if (_holoLensHandTracker.Hand2Tracked)
                {
                    if (_lastLeftHandVisibility == false) CmdUpdateHandVisibilityLeft(true);
                    Hand2.transform.position = Quaternion.Euler(0, AlignmentRotation, 0) * (_holoLensHandTracker.TrackedHandPosition2 - AlignmentTranslation) + MarkerOffset;
                    Hand2.transform.rotation = this.transform.rotation;
                    _lastLeftHandVisibility = _holoLensHandTracker.Hand2Tracked;
                }
                else
                {
                    if (_lastLeftHandVisibility == true) CmdUpdateHandVisibilityLeft(false);
                    _lastLeftHandVisibility = _holoLensHandTracker.Hand2Tracked;
                }
            }

            if (_viveControllerRight)
            {
                Hand1.transform.position = _viveControllerRight.transform.position;
                Hand1.transform.rotation = _viveControllerRight.transform.rotation;
            }

            if (_viveControllerLeft)
            {
                Hand2.transform.position = _viveControllerRight.transform.position;
                Hand2.transform.rotation = _viveControllerRight.transform.rotation;
            }
        }
    }


    [Command]
    private void CmdUpdateHandVisibilityRight(bool handTracked)
    {
        RightHandVisible = handTracked;
    }

    [Command]
    private void CmdUpdateHandVisibilityLeft(bool handTracked)
    {
        LeftHandVisible = handTracked;
    }

    /// <summary>
    /// Get called from the Server to setup the parameters for the player
    /// </summary>
    /// <param name="alignmentTranslation"></param>
    /// <param name="alignmentYRotation"></param>
    /// <param name="playerType"></param>
    /// <param name="playerCounter"></param>
    [ClientRpc]
    public void RpcSetupPlayer(Vector3 alignmentTranslation, float alignmentYRotation, Utils.PlayerType playerType, int playerCounter, Vector3 markerOffset, string playerName)
    {
        SetupPlayer(alignmentTranslation, alignmentYRotation, playerType, playerCounter, markerOffset, playerName);
    }

    [Client]
    public void SetupPlayer(Vector3 alignmentTranslation, float alignmentYRotation, Utils.PlayerType playerType, int playerCounter, Vector3 markerOffset, string playerName)
    {
        AlignmentTranslation = alignmentTranslation;
        AlignmentRotation = alignmentYRotation;
        PlayerType = playerType;
        PlayerCounter = playerCounter;
        MarkerOffset = markerOffset;
        PlayerName = playerName;
    }

    [Client]
    public void SpawnAvatar(bool showAvatar)
    {
        Avatar = Instantiate(AvatarPrefab);
        Avatar.GetComponent<AvatarController>().NetworkPlayerGameObject = gameObject;
        Avatar.GetComponent<AvatarController>().SetAvatar(PlayerCounter, PlayerType, PlayerName, showAvatar);

        var beamLiteHlapiPlayer = this.GetComponent<BeamLiteHlapiPlayer>();
        if (beamLiteHlapiPlayer)
        {
            beamLiteHlapiPlayer.Avatar = Avatar;
        }

        // set local alignment translation and rotation only for HoloLens User
        // local alignment translation and rotation is used to transform the position in VR into AR coordinate system
        // local alignment translation and rotation equals the position and rotation of the real Marker 
        if (Utils.CurrentPlayerType == Utils.PlayerType.HoloLens)
        {
            Avatar.GetComponent<AvatarController>().LocalAlignmentTranslation = VuMarkHandler.Instance.MarkerPosition;
            Avatar.GetComponent<AvatarController>().LocalAlignmentRotation = VuMarkHandler.Instance.MarkerRotation;
        }



    }

    private void GetWhiteboards()
    {
        Whiteboards = GameObject.FindGameObjectsWithTag("Whiteboard");
    }

    private void GetWhiteboardButtons()
    {
        WhiteBoardButtons = GameObject.FindGameObjectWithTag("WhiteBoardButtons");
    }

    private void GetAgenda()
    {
        Agenda = GameObject.Find("Agenda");
    }

    private void GetClock()
    {
        Clock = GameObject.Find("Clock");
    }
    private void SetWhiteboardTransforms()
    {
        if (Whiteboards.Length > 0)
        {
            foreach (var whiteboard in Whiteboards)
            {
                whiteboard.SetActive(true);
                whiteboard.transform.position = VuMarkHandler.Instance.MarkerPosition + (Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * (whiteboard.transform.position - MarkerOffset));
                whiteboard.transform.rotation = Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * whiteboard.transform.rotation;

                foreach(var renderer in whiteboard.GetComponents<MeshRenderer>())
                {
                    renderer.enabled = true;
                }
            }

        }
    }

    private void SetWhiteboardButtonTransforms()
    {
        if (WhiteBoardButtons != null)
        {
            foreach (Transform child in WhiteBoardButtons.transform)
            {
                child.gameObject.SetActive(true);
            }

            WhiteBoardButtons.transform.position = VuMarkHandler.Instance.MarkerPosition + (Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * (WhiteBoardButtons.transform.position - MarkerOffset));
            WhiteBoardButtons.transform.rotation = Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * WhiteBoardButtons.transform.rotation;

        }
    }

    private void SetAgendaTransform()
    {
        if (Agenda != null)
        {
            foreach (Transform child in Agenda.transform)
            {
                child.gameObject.SetActive(true);
            }

            Agenda.transform.position = VuMarkHandler.Instance.MarkerPosition + (Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * (Agenda.transform.position - MarkerOffset));
            Agenda.transform.rotation = Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * Agenda.transform.rotation;
        }
    }

    private void SetClockTransform()
    {
        if (Clock != null)
        {
            Clock.transform.position = VuMarkHandler.Instance.MarkerPosition + (Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * (Clock.transform.position - MarkerOffset));
            Clock.transform.rotation = Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * Clock.transform.rotation;
        }
    }

    public override void OnNetworkDestroy()
    {
        base.OnNetworkDestroy();
        // when network player is destroyed, the avatar will be removed too
        Destroy(Avatar);
    }

    public void AssignPenAuthority(GameObject gameObject)
    {
        CmdGivePenAuthority(gameObject);
    }

    [Command]
    public void CmdGivePenAuthority(GameObject gameObject)
    {
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    public void RemovePenAuthority(GameObject gameObject)
    {
        CmdRemovePenAuthority(gameObject);
    }

    [Command]
    public void CmdRemovePenAuthority(GameObject gameObject)
    {
        if (gameObject.GetComponent<NetworkIdentity>().hasAuthority)
        {
            gameObject.GetComponent<NetworkIdentity>().RemoveClientAuthority(connectionToClient);
        }
    }
}
