using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
#if UNITY_WSA
using UnityEngine.VR.WSA.Input;
#endif
using UnityEngine;
using cakeslice;
using VRTK;

public class Hololenshandler : MonoBehaviour
{
    public VRTK_Slider Slider;

    private bool IsPressed = false;
    public float YRotationOffset = 15;
    private bool _isHandVisible = false;
    private uint currentInputSourceId;
    private Vector3 _trackedHandPosition;
    private float _rotateToHandX = 90;
    private Vector3 _positionOffset;
    private float _rotateToHandY
    {
        get { return CameraCache.Main.transform.rotation.eulerAngles.y - YRotationOffset; }
    }
    private float _rotateToHandZ = 0;

    public Outline OutlineEffect;
    // Use this for initialization
    void Start () {

#if UNITY_WSA
        InteractionManager.SourceDetected += InteractionManager_SourceDetected;
        InteractionManager.SourceLost += InteractionManager_SourceLost;
        InteractionManager.SourcePressed += InteractionManager_SourcePressed;
        InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;
        InteractionManager.SourceReleased += InteractionManager_SourceReleased;
        
        OutlineEffect.color = 2;
        OutlineEffect.enabled = false;
#endif
    }


    // Update is called once per frame
    void Update () {
		
	}

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
                if (Physics.Raycast(_trackedHandPosition, direction, out hit, 2.0f))
                {
                    if (!(hit.collider.tag == "CharacterCreationSlider"))
                        return;
                    if (hit.collider.name != this.name && hit.collider.name != Slider.name)
                        return;

                    if (IsPressed)
                    {
                        Slider.transform.position = hit.point - (Quaternion.Euler(_rotateToHandX, hit.transform.rotation.eulerAngles.y, _rotateToHandZ) * (_positionOffset));
                    }
                }
                else
                {
                    OutlineEffect.enabled = false;
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
}
