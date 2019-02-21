using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class ControllerAnimation : MonoBehaviour {
    
    public Animator HandAnimator;
    public bool isRightHand=true;
    public BeamLiteNetworkManager NetMan;
    public GameObject PointerObj;
    private bool isGrabbing = false;
    void Start () {

        if (GetComponent<VRTK_ControllerEvents>() == null)
        {
            VRTK_Logger.Error(VRTK_Logger.GetCommonMessage(VRTK_Logger.CommonMessageKeys.REQUIRED_COMPONENT_MISSING_FROM_GAMEOBJECT, "VRTK_ControllerEvents_ListenerExample", "VRTK_ControllerEvents", "the same"));
            return;
        }
        GetComponent<VRTK_ControllerEvents>().TriggerPressed += new ControllerInteractionEventHandler(DoTriggerTouchStart);
        GetComponent<VRTK_ControllerEvents>().TriggerReleased += new ControllerInteractionEventHandler(DoTriggerTouchEnd);
        GetComponent<VRTK_ControllerEvents>().GripTouchStart += new ControllerInteractionEventHandler(DoGripTouchStart);
        GetComponent<VRTK_ControllerEvents>().GripPressed += new ControllerInteractionEventHandler(DoGripTouchStart);
        GetComponent<VRTK_ControllerEvents>().GripReleased += new ControllerInteractionEventHandler(DoGripTouchEnd);
        GetComponent<VRTK_ControllerEvents>().GripTouchEnd += new ControllerInteractionEventHandler(DoGripTouchEnd);

    }

    private void DoGripTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        HandAnimator.speed = 0.0f;
        if (isRightHand)
        {
            HandAnimator.Play("RighthandArmature|RighthandIdle", 0, 0.0f);
            NetMan.sendAnim(true,0);
        }
        else
        {
            HandAnimator.Play("LefthandArmature|LefthandIdle", 0, 0.0f);
            NetMan.sendAnim(false, 0);
        }
        isGrabbing = false;
    }

    private void DoGripTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        HandAnimator.speed = 0.0f;
        if (isRightHand)
        {
            HandAnimator.Play("RighthandArmature|RighthandGrab", 0, 0.0f);
            NetMan.sendAnim(true, 1);
        }
        else
        {
            HandAnimator.Play("LefthandArmature|Lefthandgrab", 0, 0.0f);
            NetMan.sendAnim(false, 1);
        }
        isGrabbing = true;
    }

    private void DoTriggerTouchEnd(object sender, ControllerInteractionEventArgs e)
    {
        //PointerObj.SetActive(false);
        if (!isGrabbing)
        {
            HandAnimator.speed = 0.0f;
            if (isRightHand)
            {
                HandAnimator.Play("RighthandArmature|RighthandIdle", 0, 0.0f);
                NetMan.sendAnim(true, 0);
            }
            else
            {
                HandAnimator.Play("LefthandArmature|LefthandIdle", 0, 0.0f);
                NetMan.sendAnim(false, 0);
            }
        }
        else
        {
            DoGripTouchStart(sender, e);
        }
    }

    private void DoTriggerTouchStart(object sender, ControllerInteractionEventArgs e)
    {
        //PointerObj.SetActive(true);
        if (isRightHand)
        {
            HandAnimator.Play("RighthandArmature|RighthandPointing", 0, 0.0f);
            NetMan.sendAnim(true, 2);
        }
        else
        {
            HandAnimator.Play("LefthandArmature|LefthandPointing", 0, 0.0f);
            NetMan.sendAnim(false, 2);
        }
    }

    // Update is called once per frame
    void Update () {
    }
}
