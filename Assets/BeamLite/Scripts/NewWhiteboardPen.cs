using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.InputModule;
#if UNITY_WSA
using UnityEngine.VR.WSA.Input;
#endif
using System;
using HoloToolkit.Unity;
using cakeslice;
using VRTK;

public class NewWhiteboardPen : MonoBehaviour
{
    public bool IsPressed = false;
    public float YRotationOffset = 15;
    public float AR_DistanceBetweenHandAndWhiteboard = 0.75f;
    public Whiteboard Whiteboard;

    private Color _color = Color.black;
    public Outline OutlineEffect;
    public Transform MarkerOffsetTransform;
    public NetworkPlayer NetworkPlayer;

    private uint currentInputSourceId;

    private Vector3 _trackedHandPosition;
    private Vector3 _positionOffset;
    private float _rotateToHandX = 90;

    private bool _isHandVisible;
    private float _rotateToHandY
    {
        get { return CameraCache.Main.transform.rotation.eulerAngles.y - YRotationOffset; }
    }
    private float _rotateToHandZ = 0;
    private bool _lastTouch = false;
    private float _lastX = -1;
    private float _lastY = -1;
    private Quaternion lastAngle;

    public GameObject Tip;
    public int PenSize = 5;

    /// <summary>
    /// max. length between tip and whiteboard
    /// Controlls how far the pen can move away from the whiteboard and is still writing
    /// </summary>
    public float VR_DistanceBetweenPenAndWhiteboad = 0.03f;
    private RaycastHit hit;



    // Use this for initialization
    void Start()
    {
        if (Whiteboard == null)
        {
            Debug.LogError("Whiteboard is not set in WhiteboardPen script");
        }
        #region HOLOLENS_ONLY
#if UNITY_WSA
        InteractionManager.SourceDetected += InteractionManager_SourceDetected;
        InteractionManager.SourceLost += InteractionManager_SourceLost;
        InteractionManager.SourcePressed += InteractionManager_SourcePressed;
        InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;
        InteractionManager.SourceReleased += InteractionManager_SourceReleased;

        _positionOffset = Tip.transform.localPosition;
        this.Whiteboard.SetColor(_color,PenSize);
        OutlineEffect.color = 2;
        OutlineEffect.enabled = false;
#endif
        #endregion

        #region VR_ONLY
#if UNITY_STANDALONE
        GetComponent<VRTK_InteractableObject>().InteractableObjectGrabbed += InteractableObjectGrabbed;
        GetComponent<VRTK_InteractableObject>().InteractableObjectUngrabbed += InteractableObjectUngrabbed;
#endif
        #endregion

    }


    public void SetColorAndPenSize(Color color, int penSize)
    {
        _color = color;
        PenSize = penSize;
    }

    #region VR_ONLY
#if UNITY_STANDALONE
    private void InteractableObjectUngrabbed(object sender, InteractableObjectEventArgs e)
    {
        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
    }

    private void InteractableObjectGrabbed(object sender, InteractableObjectEventArgs e)
    {
        this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
#endif
    #endregion

    #region HOLOLENS_ONLY
#if UNITY_WSA
    private void InteractionManager_SourcePressed(InteractionSourceState state)
    {
        if (currentInputSourceId == state.source.id)
        {
            IsPressed = true;
            OutlineEffect.enabled = true;
        }
    }

    private void InteractionManager_SourceDetected(InteractionSourceState state)
    {
        _isHandVisible = true;
        currentInputSourceId = state.source.id;
    }

    private void InteractionManager_SourceReleased(InteractionSourceState state)
    {
        if (currentInputSourceId == state.source.id)
        {
            IsPressed = false;
            OutlineEffect.enabled = false;
        }
    }

    private void InteractionManager_SourceUpdated(InteractionSourceState state)
    {
        if (currentInputSourceId == state.source.id)
        {
            state.properties.location.TryGetPosition(out _trackedHandPosition);

            //if local player has authority of this pen
            Vector3 direction = CameraCache.Main.transform.rotation * Vector3.forward;
            RaycastHit hit;
            if (_isHandVisible)
            {
                if (NetworkPlayer != null)
                {
                    if (NetworkPlayer.GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer <= 0 || NetworkPlayer.GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer == NetworkPlayer.netId.Value)
                    {
                        if (Physics.Raycast(_trackedHandPosition, direction, out hit, AR_DistanceBetweenHandAndWhiteboard))
                        {
                            if (!(hit.collider.tag == "Whiteboard"))
                                return;

                            this.transform.position = hit.point - (Quaternion.Euler(_rotateToHandX, hit.transform.rotation.eulerAngles.y, _rotateToHandZ) * (_positionOffset));
                            this.transform.rotation = Quaternion.Euler(_rotateToHandX, hit.transform.rotation.eulerAngles.y, _rotateToHandZ);

                            if (IsPressed)
                            {
                                this.Whiteboard = hit.collider.GetComponent<Whiteboard>();

                                if (_lastX != hit.textureCoord.x || _lastY != hit.textureCoord.y)
                                {
                                    NetworkPlayer.SetTouchPosition(hit.textureCoord.x, hit.textureCoord.y, _color, PenSize);
                                }
                                _lastX = hit.textureCoord.x;
                                _lastY = hit.textureCoord.y;

                                if (!_lastTouch)
                                {
                                    NetworkPlayer.ToggleTouch(true);
                                    _lastTouch = true;
                                }
                            }
                            else
                            {
                                if (_lastTouch)
                                {
                                    NetworkPlayer.ToggleTouch(false);
                                }
                                _lastTouch = false;
                            }
                        }
                        else
                        {
                            NetworkPlayer.ToggleTouch(false);
                            OutlineEffect.enabled = false;
                        }
                    }
                }
            }
        }
    }

    private void InteractionManager_SourceLost(InteractionSourceState state)
    {
        if (currentInputSourceId == state.source.id)
        {
            IsPressed = false;
            _isHandVisible = false;
        }
    }
#endif
    #endregion

    public void Update()
    {
        #region HOLOLENS_ONLY
        if (Utils.IsHoloLens)
        {
            ////if local player has authority of this pen
            //Vector3 direction = CameraCache.Main.transform.rotation * Vector3.forward;
            //RaycastHit hit;
            //if (_isHandVisible)
            //{
            //    if (NetworkPlayer != null)
            //    {
            //        if (NetworkPlayer.WhiteboardPermissionController.NetworkPlayerIdOfWritingPlayer <= 0 || NetworkPlayer.WhiteboardPermissionController.NetworkPlayerIdOfWritingPlayer == NetworkPlayer.netId.Value)
            //        {
            //            if (Physics.Raycast(_trackedHandPosition, direction, out hit, MaxDistanceToWhiteboard))
            //            {
            //                if (!(hit.collider.tag == "Whiteboard"))
            //                    return;

            //                this.transform.position = hit.point - (Quaternion.Euler(_rotateToHandX, hit.transform.rotation.eulerAngles.y, _rotateToHandZ) * (_positionOffset));
            //                this.transform.rotation = Quaternion.Euler(_rotateToHandX, hit.transform.rotation.eulerAngles.y, _rotateToHandZ);

            //                if (IsPressed)
            //                {
            //                    this.Whiteboard = hit.collider.GetComponent<Whiteboard>();

            //                    //if (_lastX != hit.textureCoord.x || _lastY != hit.textureCoord.y)
            //                    {
            //                        NetworkPlayer.SetTouchPosition(hit.textureCoord.x, hit.textureCoord.y, Color);
            //                    }
            //                    _lastX = hit.textureCoord.x;
            //                    _lastY = hit.textureCoord.y;

            //                    if (!_lastTouch)
            //                    {
            //                        NetworkPlayer.ToggleTouch(true);
            //                        _lastTouch = true;
            //                    }
            //                }
            //                else
            //                {
            //                    if (_lastTouch)
            //                    {
            //                        NetworkPlayer.ToggleTouch(false);
            //                    }
            //                    _lastTouch = false;
            //                }
            //            }
            //            else
            //            {
            //                NetworkPlayer.ToggleTouch(false);
            //                OutlineEffect.enabled = false;
            //            }
            //        }
            //    }
            //}
        }
        #endregion
        #region VR_ONLY
        else
        {
            Vector3 tip = Tip.transform.position;

            if (NetworkPlayer != null)
            {
                if (NetworkPlayer.GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer <= 0 || NetworkPlayer.GlobalSyncingManager.NetworkPlayerIdOfWritingPlayer == NetworkPlayer.netId.Value)
                {

                    if (Physics.Raycast(tip, transform.up, out hit, VR_DistanceBetweenPenAndWhiteboad))
                    {
                        if (!(hit.collider.tag == "Whiteboard"))
                            return;

                        //tip is touching whiteboard
                        this.Whiteboard = hit.collider.GetComponent<Whiteboard>();

                        // set touch position only if touch position change to previous frane
                        if (_lastX != hit.textureCoord.x || _lastY != hit.textureCoord.y)
                        {
                            NetworkPlayer.SetTouchPosition(hit.textureCoord.x, hit.textureCoord.y, _color, PenSize);
                        }

                        // store touch position
                        _lastX = hit.textureCoord.x;
                        _lastY = hit.textureCoord.y;

                        // if in previous frame the tip didnt touch, set touch to true
                        if (!_lastTouch)
                        {
                            NetworkPlayer.ToggleTouch(true);
                            _lastTouch = true;
                            lastAngle = transform.rotation;
                        }
                    }
                    // tip is not touching
                    else
                    {
                        // if in previous frame the tip touched, set touch to true
                        if (_lastTouch)
                        {
                            NetworkPlayer.ToggleTouch(false);
                        }
                        _lastTouch = false;
                    }

                    if (_lastTouch)
                    {
                        transform.rotation = lastAngle;
                    }
                }
            }
        }
#endregion

    }
}