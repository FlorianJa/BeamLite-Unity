using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class BeamLiteVRTK_Rotator : MonoBehaviour {

    public Transform CameraRigTransform;
    public Transform CameraHeadTransform;

    private bool _rotationAllowed = true;
    public float TouchPadThresholdTurn = 0.5f;
    public float TouchPadThresholdRelease = 0.1f;
    public float YDegreesToRotate = 30f;

    private float _fadeInTime = 0.2f;
    public Color blinkToColor = Color.black;
    public float blinkPause = 0.15f;

    private bool _isTouchpadPressed = false;

    // Use this for initialization
    void Start ()
    {
        if (GetComponent<VRTK_ControllerEvents>() == null)
        {
            VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTK_ControllerEvents_ListenerExample", "VRTK_ControllerEvents", "the same"));
            return;
        }

        GetComponent<VRTK_ControllerEvents>().TouchpadAxisChanged += new ControllerInteractionEventHandler(DoTouchpadAxisChanged);

        GetComponent<VRTK_ControllerEvents>().TouchpadPressed += new ControllerInteractionEventHandler(DoTouchpadPressed);

        GetComponent<VRTK_ControllerEvents>().TouchpadReleased += new ControllerInteractionEventHandler(DoTouchpadReleased);
    }

    private void DoTouchpadReleased(object sender, ControllerInteractionEventArgs e)
    {
        _isTouchpadPressed = false;
        _rotationAllowed = true;
    }

    private void DoTouchpadPressed(object sender, ControllerInteractionEventArgs e)
    {
        _isTouchpadPressed = true;
        if (_rotationAllowed && e.touchpadAxis.x > TouchPadThresholdTurn)
        {
            _rotationAllowed = false;
            Blink(_fadeInTime);
            CameraRigTransform.RotateAround(CameraHeadTransform.position, Vector3.up, YDegreesToRotate);

        }
        else if (_rotationAllowed && e.touchpadAxis.x < -TouchPadThresholdTurn)
        {
            _rotationAllowed = false;
            Blink(_fadeInTime);
            CameraRigTransform.RotateAround(CameraHeadTransform.position, Vector3.up, -YDegreesToRotate);
        }
    }

    private void DoTouchpadAxisChanged(object sender, ControllerInteractionEventArgs e)
    {
        //_isTouchpadPressed = true;
        //if (_rotationAllowed && e.touchpadAxis.x > TouchPadThresholdTurn)
        ////if (_isTouchpadPressed && _rotationAllowed && e.touchpadAxis.x > TouchPadThresholdTurn)
        //{
        //    _rotationAllowed = false;
        //    Blink(_fadeInTime);
        //    //CameraRigTransform.rotation = Quaternion.Euler(DegreesToRotate) * CameraRigTransform.rotation;
        //    CameraRigTransform.RotateAround(CameraHeadTransform.position, Vector3.up, YDegreesToRotate);

        //}
        //else if (_rotationAllowed && e.touchpadAxis.x < -TouchPadThresholdTurn)
        ////else if (_isTouchpadPressed && _rotationAllowed && e.touchpadAxis.x < -TouchPadThresholdTurn)
        //{
        //    _rotationAllowed = false;
        //    Blink(_fadeInTime);
        //    //CameraRigTransform.rotation = Quaternion.Euler(-DegreesToRotate) * CameraRigTransform.rotation;
        //    CameraRigTransform.RotateAround(CameraHeadTransform.position, Vector3.up, YDegreesToRotate);
        //}

        ////if (_rotationAllowed == false && e.touchpadAxis.x > -TouchPadThresholdRelease && e.touchpadAxis.x < TouchPadThresholdRelease)
        ////{
        ////    _rotationAllowed = true;
        ////}
        return;
    }

    protected virtual void Blink(float transitionSpeed)
    {
        _fadeInTime = transitionSpeed;
        if (transitionSpeed > 0f)
        {
            VRTK_SDK_Bridge.HeadsetFade(blinkToColor, 0);
        }
        Invoke("ReleaseBlink", blinkPause);
    }

    protected virtual void ReleaseBlink()
    {
        VRTK_SDK_Bridge.HeadsetFade(Color.clear, _fadeInTime);
    }

    private void DebugLogger(uint index, string button, string action, ControllerInteractionEventArgs e)
    {
        VRTK_Logger.Info("Controller on index '" + index + "' " + button + " has been " + action
                + " with a pressure of " + e.buttonPressure + " / trackpad axis at: " + e.touchpadAxis + " (" + e.touchpadAngle + " degrees)");
    }
}
