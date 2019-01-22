using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using VRTK;

[Obsolete("OldWhiteboardPen is deprecated, please use NewWhiteboardPen instead.", true)]
public class OldWhiteboardPen : MonoBehaviour
{
    /// <summary>
    /// Object to write on
    /// </summary>
    public Whiteboard Whiteboard;

    /// <summary>
    /// color of pen
    /// </summary>
    public Color Color = Color.cyan;

    public GameObject Tip;
    
    /// <summary>
    /// max. length between tip and whiteboard
    /// </summary>
    public float RaycastLenght = 0.05f;

    /// <summary>
    /// Networkplayer controlling this pen
    /// </summary>
    public NetworkPlayer NetworkPlayer;

    private RaycastHit hit;
    private bool lastTouch = false;
    private Quaternion lastAngle;
    private float _lastX = -1;
    private float _lastY = -1;

    // Use this for initialization
    void Start()
    {
        if(NetworkPlayer == null)
        {
            Debug.LogError("NetworkPlayer is not set in WhiteboardPen script");
        }
        if(Whiteboard == null)
        {
            Debug.LogError("Whiteboard is not set in WhiteboardPen script");
        }

        GetComponent<VRTK_InteractableObject>().InteractableObjectGrabbed += InteractableObjectGrabbed;
        GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += InteractableObjectUngrabbed;
    }

    private void InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    private void InteractableObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {
        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
    
    // Update is called once per frame
    void Update()
    {
        Vector3 tip = Tip.transform.position;

        if (NetworkPlayer != null)
        {
            if (NetworkPlayer.GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer <= 0 || NetworkPlayer.GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer == NetworkPlayer.netId.Value)
            {

                if (Physics.Raycast(tip, transform.up, out hit, RaycastLenght))
                {
                    if (!(hit.collider.tag == "Whiteboard"))
                        return;

                    //tip is touching whiteboard
                    this.Whiteboard = hit.collider.GetComponent<Whiteboard>();

                    // set touch position only if touch position change to previous frane
                    if (_lastX != hit.textureCoord.x || _lastY != hit.textureCoord.y)
                    {
                        NetworkPlayer.SetTouchPosition(hit.textureCoord.x, hit.textureCoord.y, Color,1);
                    }

                    // store touch position
                    _lastX = hit.textureCoord.x;
                    _lastY = hit.textureCoord.y;

                    // if in previous frame the tip didnt touch, set touch to true
                    if (!lastTouch)
                    {
                        NetworkPlayer.SetTouch(true);
                        lastTouch = true;
                        lastAngle = transform.rotation;
                    }
                }
                // tip is not touching
                else
                {
                    // if in previous frame the tip touched, set touch to true
                    if (lastTouch)
                    {
                        NetworkPlayer.SetTouch(false);
                    }
                    lastTouch = false;
                }

                if (lastTouch)
                {
                    transform.rotation = lastAngle;
                }
            }
        }
    }
}
