using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class CharactercreationScreen : MonoBehaviour {

    public Texture2D[] possible_textures;

    public GameObject HoloLensAvatar;
    public GameObject ViveAvatar;
    public GameObject RiftAvatar;
    public GameObject BodyPrefab;
    public VRTK_Slider Hipslider;
    public VRTK_Slider Bellyslider;
    public VRTK_Slider Breastslider;
    public VRTK_Slider Shoulderslider;
    public VRTK_InteractableObject Okbutton;
    public VRTK_InteractableObject ButtonLeft;
    public VRTK_InteractableObject ButtonRight;

    public BeamLiteNetworkManager NetworkManager;
    

    private bool isvisible=false;
    private GameObject _body;
    private Renderer _bodyRenderer;
    private GameObject _head;
    private Transform _HipTransform;
    private Transform _BellyTransform;
    private Transform _BreastTransform;
    private Transform _ShoulderTransform;
    private Transform _NeckTransform;

    // Use this for initialization
    void Start () {

        Okbutton.InteractableObjectTouched += OKpressed;
        Okbutton.InteractableObjectUsed += OKpressed;
        ButtonLeft.InteractableObjectTouched += Leftpressed;
        ButtonLeft.InteractableObjectUsed += Leftpressed;
        ButtonRight.InteractableObjectTouched += Rightpressed;
        ButtonRight.InteractableObjectUsed += Rightpressed;

        Hipslider.ValueChanged += SomeSliderValueChanged;
        Bellyslider.ValueChanged += SomeSliderValueChanged;
        Breastslider.ValueChanged += SomeSliderValueChanged;
        Shoulderslider.ValueChanged += SomeSliderValueChanged;
        NetworkManager.LocalplayerHipsize = Hipslider.GetValue();
        NetworkManager.LocalplayerBellysize = Bellyslider.GetValue();
        NetworkManager.LocalplayerBreastsize = Breastslider.GetValue();
        NetworkManager.LocalplayerShouldersize = Shoulderslider.GetValue();
    }

    private void SomeSliderValueChanged(object sender, Control3DEventArgs e)
    {
        NetworkManager.LocalplayerHipsize = Hipslider.GetValue();
        NetworkManager.LocalplayerBellysize = Bellyslider.GetValue();
        NetworkManager.LocalplayerBreastsize = Breastslider.GetValue();
        NetworkManager.LocalplayerShouldersize = Shoulderslider.GetValue();
        if (_body != null)
        {
            _HipTransform.localScale = new Vector3(NetworkManager.LocalplayerHipsize, 1, NetworkManager.LocalplayerHipsize);
            if (NetworkManager.LocalplayerHipsize != 0)
            {
                _BellyTransform.localScale = new Vector3(NetworkManager.LocalplayerBellysize / NetworkManager.LocalplayerHipsize, 1, NetworkManager.LocalplayerBellysize / NetworkManager.LocalplayerHipsize);
                if (NetworkManager.LocalplayerBellysize != 0)
                {
                    _BreastTransform.localScale = new Vector3(NetworkManager.LocalplayerBreastsize / NetworkManager.LocalplayerBellysize, 1, NetworkManager.LocalplayerBreastsize / NetworkManager.LocalplayerBellysize);
                    if (NetworkManager.LocalplayerBreastsize != 0)
                    {
                        _ShoulderTransform.localScale = new Vector3(NetworkManager.LocalplayerShouldersize / NetworkManager.LocalplayerBreastsize, 1, NetworkManager.LocalplayerShouldersize / NetworkManager.LocalplayerBreastsize);
                        if (NetworkManager.LocalplayerShouldersize != 0)
                        {
                            _NeckTransform.localScale = new Vector3(1 / NetworkManager.LocalplayerShouldersize, 1, 1 / NetworkManager.LocalplayerShouldersize);
                        }
                    }
                }
            }
        }
        NetworkManager.UpdateplayerSizes();
    }

    // Update is called once per frame
    void Update ()
    {
        if (!isvisible && (Utils.CurrentPlayerType==Utils.PlayerType.Rift || Utils.CurrentPlayerType == Utils.PlayerType.Vive || Utils.CurrentPlayerType == Utils.PlayerType.HoloLens))
        {

            //this.transform.rotation = Quaternion.Euler(0, 90, 0);
            switch (Utils.CurrentPlayerType)
            {
                case Utils.PlayerType.HoloLens:
                    _head=Instantiate(HoloLensAvatar, gameObject.transform);
                    break;
                case Utils.PlayerType.Vive:
                    _head = Instantiate(ViveAvatar, gameObject.transform);
                    break;
                case Utils.PlayerType.Rift:
                    _head = Instantiate(RiftAvatar, gameObject.transform);
                    break;
            }
            _body = Instantiate(BodyPrefab, gameObject.transform);
            _body.transform.rotation = Quaternion.Euler(0, 90, 0);
            _body.transform.Translate(new Vector3(0, -0.3f, 0));
            _head.transform.rotation = Quaternion.Euler(0, 90, 0);
            _head.transform.Translate(new Vector3(0, -0.3f, 0));
            isvisible = true;
            _HipTransform = _body.transform.Find("Armature/Beine/Hüfte").transform;
            _BellyTransform = _body.transform.Find("Armature/Beine/Hüfte/Bauch").transform;
            _BreastTransform = _body.transform.Find("Armature/Beine/Hüfte/Bauch/Brust").transform;
            _ShoulderTransform = _body.transform.Find("Armature/Beine/Hüfte/Bauch/Brust/Schulter").transform;
            _NeckTransform = _body.transform.Find("Armature/Beine/Hüfte/Bauch/Brust/Schulter/Hals").transform;
            _bodyRenderer = _body.transform.Find("Body").GetComponent<Renderer>();
        }


    }

    private void Buttonpress(string name)
    {
        if (name == "OK") {
            this.gameObject.SetActive(false);
            NetworkManager.JoinMatch();
        }
        else if (name == "Left")
        {
            NetworkManager.LocalplayerTexture = (NetworkManager.LocalplayerTexture - 1) % (possible_textures.Length);
            if (_body)
            {
                _bodyRenderer.material.SetTexture("_MainTex", possible_textures[NetworkManager.LocalplayerTexture]);
            }
            NetworkManager.UpdateplayerTexture();

        }
        else if(name== "Right")
        {
            NetworkManager.LocalplayerTexture = (NetworkManager.LocalplayerTexture + 1) % (possible_textures.Length);
            if (_body)
            {
                _bodyRenderer.material.SetTexture("_MainTex", possible_textures[NetworkManager.LocalplayerTexture]);
            }
            NetworkManager.UpdateplayerTexture();

        }
    }

    public void ButtonClickedHololens(string name)
    {
        Buttonpress(name);
    }
    private void OKpressed(object sender, InteractableObjectEventArgs e)
    {
        Buttonpress("OK");
    }
    private void Leftpressed(object sender, InteractableObjectEventArgs e)
    {
        Buttonpress("Left");
    }
    private void Rightpressed(object sender, InteractableObjectEventArgs e)
    {
        Buttonpress("Right");
    }
}
