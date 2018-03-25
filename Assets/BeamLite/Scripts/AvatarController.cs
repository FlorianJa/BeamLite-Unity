using Dissonance;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AvatarController : MonoBehaviour {


    public GameObject NetworkPlayerGameObject;
    public Vector3 LocalAlignmentTranslation;
    public float LocalAlignmentRotation;
    public GameObject HoloLensAvatar;
    public GameObject HoloLensAvatar2;
    public GameObject ViveAvatar;
    public GameObject BodyPrefab;
    public Utils.PlayerType PlayerType;

    
    public GameObject HandPrefabL;
    public GameObject HandPrefabR;

    public GameObject HandPrefabHololens;

    public Text TextFieldForPlayerName;

    public Color NotSpeakingColor;
    public Color SpeakingColor;

    private NetworkPlayer _networkPlayer;
    private GameObject _body;
    private GameObject _rightHand;
    private GameObject _leftHand;
    private Vector3 MarkerOffset;



    // Use this for initialization
    void Start ()
    {

    }

    public void SetAvatar(int playerCounter, Utils.PlayerType playerType, string playerName, bool showHLAvatar = false)
    {
        PlayerType = playerType;
        _networkPlayer = NetworkPlayerGameObject.GetComponent<NetworkPlayer>();
        MarkerOffset = _networkPlayer.MarkerOffset;
        TextFieldForPlayerName.text = playerName;

        if (PlayerType == Utils.PlayerType.VR)
        {
            if(showHLAvatar)
            {
                Instantiate(ViveAvatar, gameObject.transform);
                _body = Instantiate(BodyPrefab, gameObject.transform);

                _rightHand = Instantiate(HandPrefabR);
                _rightHand.SetActive(false);

                _leftHand = Instantiate(HandPrefabL);
                _leftHand.SetActive(false);
            }
        }
        else
        {
            if (showHLAvatar)
            {
                if (playerCounter % 2 == 0)
                {
                    Instantiate(HoloLensAvatar, gameObject.transform);
                }
                else
                {
                    Instantiate(HoloLensAvatar2, gameObject.transform);
                }
                _body = Instantiate(BodyPrefab, gameObject.transform);

                _rightHand = Instantiate(HandPrefabHololens);
                _rightHand.SetActive(false);

                _leftHand = Instantiate(HandPrefabHololens);
                _leftHand.SetActive(false);

            }
            else
            {
                TextFieldForPlayerName.gameObject.transform.parent.gameObject.SetActive(false);
            }
        }


        //var comms = FindObjectOfType<DissonanceComms>();
        //comms.OnPlayerJoinedSession += Comms_OnPlayerJoinedSession;
    }

    //private void Comms_OnPlayerJoinedSession(VoicePlayerState obj)
    //{
    //    var BeamLiteHlapiPlayer = NetworkPlayerGameObject.GetComponent<BeamLiteHlapiPlayer>();

    //    var comms = FindObjectOfType<DissonanceComms>();

    //    if (obj.Name == BeamLiteHlapiPlayer.PlayerId)
    //    {
    //        //Get a specific player
    //        VoicePlayerState player = comms.FindPlayer(BeamLiteHlapiPlayer.PlayerId);
    //        player.OnStartedSpeaking += Player_OnStartedSpeaking;
    //        player.OnStoppedSpeaking += Player_OnStoppedSpeaking;
    //    }
    //}

    public void Player_OnStoppedSpeaking(VoicePlayerState obj)
    {
        TextFieldForPlayerName.color = NotSpeakingColor;
    }

    public void Player_OnStartedSpeaking(VoicePlayerState obj)
    {
        TextFieldForPlayerName.color = SpeakingColor;
    }

	
    // Update is called once per frame
	void Update ()
    {
        if (Utils.IsHoloLens)
        {
            //View on HoloLens Side
            if (_networkPlayer)
            {
                this.transform.position = LocalAlignmentTranslation + (Quaternion.Euler(0, -LocalAlignmentRotation, 0) * (NetworkPlayerGameObject.transform.position - MarkerOffset));
                this.transform.rotation = Quaternion.Euler(0, -LocalAlignmentRotation, 0) * NetworkPlayerGameObject.transform.rotation;

                if (_body != null)
                {
                    this._body.transform.rotation = Quaternion.Euler(BodyPrefab.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y, BodyPrefab.transform.rotation.eulerAngles.z);
                    this._body.transform.position = this.transform.position + BodyPrefab.transform.localPosition;
                }

                if (_rightHand)
                {
                    if (_networkPlayer.RightHandVisible)
                    {

                        _rightHand.SetActive(true);
                        _rightHand.transform.position = LocalAlignmentTranslation + (Quaternion.Euler(0, -LocalAlignmentRotation, 0) * (_networkPlayer.Hand1.transform.position - MarkerOffset));
                        _rightHand.transform.rotation = Quaternion.Euler(0, -LocalAlignmentRotation, 0) * _networkPlayer.Hand1.transform.rotation;
                    }
                    else
                    {
                        _rightHand.SetActive(false);
                    }
                }

                if (_leftHand)
                {
                    if (_networkPlayer.LeftHandVisible)
                    {
                        _leftHand.SetActive(true);
                        _leftHand.transform.position = LocalAlignmentTranslation + (Quaternion.Euler(0, -LocalAlignmentRotation, 0) * (_networkPlayer.Hand2.transform.position - MarkerOffset));
                        _leftHand.transform.rotation = Quaternion.Euler(0, -LocalAlignmentRotation, 0) * _networkPlayer.Hand2.transform.rotation;
                    }
                    else
                    {
                        _leftHand.SetActive(false);
                    }
                }

            }
        }
        else
        {
            //View on VR Side
            this.transform.position = NetworkPlayerGameObject.transform.position;
            this.transform.rotation = NetworkPlayerGameObject.transform.rotation;

            if (_body != null)
            {
                this._body.transform.rotation = Quaternion.Euler(BodyPrefab.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y, BodyPrefab.transform.rotation.eulerAngles.z);
                this._body.transform.position = this.transform.position + BodyPrefab.transform.localPosition;
            }
            if (_rightHand)
            {
                if(_networkPlayer.RightHandVisible)
                {
                    _rightHand.SetActive(true);
                    _rightHand.transform.position = _networkPlayer.Hand1.transform.position;
                    _rightHand.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
                }
                else
                {
                    _rightHand.SetActive(false);
                }
            }

            if (_leftHand)
            {
                if (_networkPlayer.LeftHandVisible)
                {
                    _leftHand.SetActive(true);
                    _leftHand.transform.position = _networkPlayer.Hand2.transform.position;
                    _leftHand.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
                }
                else
                {
                    _leftHand.SetActive(false);
                }
            }
        }
    }
}
