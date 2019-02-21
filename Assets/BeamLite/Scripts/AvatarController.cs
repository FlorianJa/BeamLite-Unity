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
    public GameObject RiftAvatar;
    public GameObject ViveAvatar;
    public GameObject BodyPrefab;
    public Utils.PlayerType PlayerType;

    
    public GameObject HandPrefabL;
    public GameObject HandPrefabR;

    public GameObject HandPrefabHololens;

    public Text TextFieldForPlayerName;

    public Color NotSpeakingColor;
    public Color SpeakingColor;

    public Texture2D[] possible_textures;

    public int PoseL = -1, PoseR = -1;

    
    private Transform _HipTransform;
    private Transform _BellyTransform;
    private Transform _BreastTransform;
    private Transform _ShoulderTransform;
    private Transform _NeckTransform;

    private NetworkPlayer _networkPlayer;
    private GameObject _body;
    private Renderer _bodyRenderer;
    private GameObject _rightHand;
    private GameObject _leftHand;
    private Vector3 MarkerOffset;
    private int _currentTexture=0;


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


        if (PlayerType == Utils.PlayerType.Vive)
        {
            if (showHLAvatar)
            {
                //Instantiate(ViveAvatar, new Vector3(0,-0.4f,0),Quaternion.Euler(0,-90,0), gameObject.transform);
                //_body = Instantiate(BodyPrefab, new Vector3(0, -0.4f, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);
                Instantiate(ViveAvatar, new Vector3(0,0,0),Quaternion.Euler(0,-90,0), gameObject.transform);
                _body = Instantiate(BodyPrefab, new Vector3(0, 0, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);

                _rightHand = Instantiate(HandPrefabR);
                _rightHand.SetActive(false);

                _leftHand = Instantiate(HandPrefabL);
                _leftHand.SetActive(false);
            }
        } else if (PlayerType == Utils.PlayerType.Rift)
        {
            if (showHLAvatar)
            {
                //Instantiate(RiftAvatar, new Vector3(0, -0.4f, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);
                //_body = Instantiate(BodyPrefab, new Vector3(0, -0.4f, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);
                Instantiate(RiftAvatar, new Vector3(0, 0, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);
                _body = Instantiate(BodyPrefab, new Vector3(0,0, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);

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
                //Instantiate(HoloLensAvatar, new Vector3(0, -0.4f, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);
                //_body = Instantiate(BodyPrefab, new Vector3(0, -0.4f, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);
                Instantiate(HoloLensAvatar, new Vector3(0, 0, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);
                _body = Instantiate(BodyPrefab, new Vector3(0,0, 0), Quaternion.Euler(0, -90, 0), gameObject.transform);

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
        if (_body)
        {
            _HipTransform = _body.transform.Find("Character_Body/Armature/Beine/Hüfte").transform;
            _BellyTransform = _body.transform.Find("Character_Body/Armature/Beine/Hüfte/Bauch").transform;
            _BreastTransform = _body.transform.Find("Character_Body/Armature/Beine/Hüfte/Bauch/Brust").transform;
            _ShoulderTransform = _body.transform.Find("Character_Body/Armature/Beine/Hüfte/Bauch/Brust/Schulter").transform;
            _NeckTransform = _body.transform.Find("Character_Body/Armature/Beine/Hüfte/Bauch/Brust/Schulter/Hals").transform;
            _bodyRenderer = _body.transform.Find("Character_Body/Body").GetComponent<Renderer>();
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
                this.transform.position = LocalAlignmentTranslation + new Vector3(0, -0.1f, 0) + (Quaternion.Euler(0, -LocalAlignmentRotation, 0) * (NetworkPlayerGameObject.transform.position - MarkerOffset));
                this.transform.rotation = Quaternion.Euler(0, -LocalAlignmentRotation, 0) * NetworkPlayerGameObject.transform.rotation;

                if (_body != null)
                {
                    this._body.transform.rotation = Quaternion.Euler(BodyPrefab.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y - 90, BodyPrefab.transform.rotation.eulerAngles.z);
                    this._body.transform.position = this.transform.position + BodyPrefab.transform.localPosition;
                }

                if (_rightHand)
                {
                    if (_networkPlayer.RightHandVisible)
                    {

                        _rightHand.SetActive(true);
                        _rightHand.transform.position = LocalAlignmentTranslation + (Quaternion.Euler(0, -LocalAlignmentRotation, 0) * (_networkPlayer.Hand1.transform.position - MarkerOffset));
                        _rightHand.transform.rotation = Quaternion.Euler(0, -LocalAlignmentRotation, 0) * _networkPlayer.Hand1.transform.rotation;
                        switch (PoseR)
                        {
                            case 0:
                                _rightHand.transform.Find("Character_Righthand").GetComponent<Animator>().Play("RighthandArmature|RighthandIdle", 0, 0.0f);
                                _rightHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                                break;
                            case 1:
                                _rightHand.transform.Find("Character_Righthand").GetComponent<Animator>().Play("RighthandArmature|RighthandGrab", 0, 0.0f);
                                _rightHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                                break;
                            case 2:
                                _rightHand.transform.Find("Character_Righthand").GetComponent<Animator>().Play("RighthandArmature|RighthandPointing", 0, 0.0f);
                                _rightHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = true;
                                break;
                        }
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
                        switch (PoseL)
                        {
                            case 0:
                                _leftHand.transform.Find("Character_Lefthand").GetComponent<Animator>().Play("LefthandArmature|LefthandIdle", 0, 0.0f);
                                _leftHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                                break;
                            case 1:
                                _leftHand.transform.Find("Character_Lefthand").GetComponent<Animator>().Play("LefthandArmature|Lefthandgrab", 0, 0.0f);
                                _leftHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                                break;
                            case 2:
                                _leftHand.transform.Find("Character_Lefthand").GetComponent<Animator>().Play("LefthandArmature|LefthandPointing", 0, 0.0f);
                                _leftHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = true;
                                break;
                        }
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
            this.transform.position = NetworkPlayerGameObject.transform.position+new Vector3(0,-0.15f,0);
            this.transform.rotation = NetworkPlayerGameObject.transform.rotation;

            if (_body != null)
            {
                this._body.transform.rotation = Quaternion.Euler(BodyPrefab.transform.rotation.eulerAngles.x, this.transform.rotation.eulerAngles.y-90, BodyPrefab.transform.rotation.eulerAngles.z);
                //this._body.transform.position = this.transform.position + BodyPrefab.transform.localPosition + new Vector3(0, -0.4f, 0);
                this._body.transform.position = this.transform.position + BodyPrefab.transform.localPosition + new Vector3(0, 0, 0);
            }
            if (_rightHand)
            {
                if(_networkPlayer.RightHandVisible)
                {
                    _rightHand.SetActive(true);
                    _rightHand.transform.position = _networkPlayer.Hand1.transform.position;
                    _rightHand.transform.rotation = Quaternion.Euler(0, this.transform.rotation.eulerAngles.y, 0);
                    switch (PoseR)
                    {
                        case 0:
                            _rightHand.transform.Find("Character_Righthand").GetComponent<Animator>().Play("RighthandArmature|RighthandIdle", 0, 0.0f);
                            _rightHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                            break;
                        case 1:
                            _rightHand.transform.Find("Character_Righthand").GetComponent<Animator>().Play("RighthandArmature|RighthandGrab", 0, 0.0f);
                            _rightHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                            break;
                        case 2:
                            _rightHand.transform.Find("Character_Righthand").GetComponent<Animator>().Play("RighthandArmature|RighthandPointing", 0, 0.0f);
                            _rightHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = true;
                            break;
                    }
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
                    switch (PoseL)
                    {
                        case 0:
                            _leftHand.transform.Find("Character_Lefthand").GetComponent<Animator>().Play("LefthandArmature|LefthandIdle", 0, 0.0f);
                            _leftHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                            break;
                        case 1:
                            _leftHand.transform.Find("Character_Lefthand").GetComponent<Animator>().Play("LefthandArmature|Lefthandgrab", 0, 0.0f);
                            _leftHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = false;
                            break;
                        case 2:
                            _leftHand.transform.Find("Character_Lefthand").GetComponent<Animator>().Play("LefthandArmature|LefthandPointing", 0, 0.0f);
                            _leftHand.transform.Find("Cube").GetComponent<MeshRenderer>().enabled = true;
                            break;
                    }
                }
                else
                {
                    _leftHand.SetActive(false);
                }
            }
        }
        if (_body != null)
        {
            if (_networkPlayer.TextureNum != _currentTexture)
            {
                _currentTexture = _networkPlayer.TextureNum;
                _bodyRenderer.material.SetTexture("_MainTex", possible_textures[_currentTexture]);
            }
            _HipTransform.localScale = new Vector3(_networkPlayer.Hipsize, 1, _networkPlayer.Hipsize);
            if (_networkPlayer.Hipsize != 0)
            {
                _BellyTransform.localScale = new Vector3(_networkPlayer.Bellysize / _networkPlayer.Hipsize, 1, _networkPlayer.Bellysize / _networkPlayer.Hipsize);
                if (_networkPlayer.Bellysize != 0)
                {
                    _BreastTransform.localScale = new Vector3(_networkPlayer.Breastsize / _networkPlayer.Bellysize, 1, _networkPlayer.Breastsize / _networkPlayer.Bellysize);
                    if (_networkPlayer.Breastsize != 0)
                    {
                        _ShoulderTransform.localScale = new Vector3(_networkPlayer.Shouldersize / _networkPlayer.Breastsize, 1, _networkPlayer.Shouldersize / _networkPlayer.Breastsize);
                        if (_networkPlayer.Shouldersize != 0)
                        {
                            _NeckTransform.localScale = new Vector3(1 / _networkPlayer.Shouldersize, 1, 1 / _networkPlayer.Shouldersize);
                        }
                    }
                }
            }
        }
    }
}
