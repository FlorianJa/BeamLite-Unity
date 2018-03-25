using HoloToolkit.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WSA
using UnityEngine.VR.WSA.Input;
#endif
public class HoloLensHandTrackingController : Singleton<HoloLensHandTrackingController>
{


    public bool Hand1Tracked;
    public Vector3 TrackedHandPosition1;

    public bool Hand2Tracked;
    public Vector3 TrackedHandPosition2;

    private HashSet<uint> _trackedHands = new HashSet<uint>();
    private Dictionary<uint, string> trackingObject = new Dictionary<uint, string>();

#if UNITY_WSA
    protected override void Awake()
    {
        base.Awake();
        InteractionManager.SourceDetected += InteractionManager_SourceDetected;
        InteractionManager.SourceLost += InteractionManager_SourceLost;
        InteractionManager.SourceUpdated += InteractionManager_SourceUpdated;
    }

    private void InteractionManager_SourceUpdated(InteractionSourceState state)
    {
        uint id = state.source.id;
        Vector3 pos;

        if (state.source.kind == InteractionSourceKind.Hand)
        {
            if (trackingObject.ContainsKey(state.source.id))
            {
                if (state.properties.location.TryGetPosition(out pos))
                {
                    var hand = trackingObject[id];
                    if (hand.Equals("Hand1"))
                    {
                        TrackedHandPosition1 = pos;
                    }
                    else if (hand.Equals("Hand2"))
                    {
                        TrackedHandPosition2 = pos;
                    }
                }
            }
        }
    }

    private void InteractionManager_SourceLost(InteractionSourceState state)
    {
        uint id = state.source.id;
        // Check to see that the source is a hand.
        if (state.source.kind != InteractionSourceKind.Hand)
        {
            return;
        }

        if (_trackedHands.Contains(id))
        {
            _trackedHands.Remove(id);
        }

        if (trackingObject.ContainsKey(id))
        {
            var hand = trackingObject[id];
            trackingObject.Remove(id);
            if (hand.Equals("Hand1"))
            {
                Hand1Tracked = false;
            }
            else if (hand.Equals("Hand2"))
            {
                Hand2Tracked = false;
            }
        }
        
    }

    private void InteractionManager_SourceDetected(InteractionSourceState state)
    {
        uint id = state.source.id;
        // Check to see that the source is a hand.
        if (state.source.kind != InteractionSourceKind.Hand)
        {
            return;
        }
        _trackedHands.Add(id);

        if (!Hand1Tracked)
        { 
            Vector3 pos;
            Hand1Tracked = true;
            if (state.properties.location.TryGetPosition(out pos))
            {
                TrackedHandPosition1 = pos;

            }
            trackingObject.Add(id, "Hand1");
        }
        else
        {
            Vector3 pos;
            Hand2Tracked = true;
            if (state.properties.location.TryGetPosition(out pos))
            {
                TrackedHandPosition2 = pos;

            }
            trackingObject.Add(id, "Hand2");
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        InteractionManager.SourceDetected -= InteractionManager_SourceDetected;
        InteractionManager.SourceLost -= InteractionManager_SourceLost;
        InteractionManager.SourceUpdated -= InteractionManager_SourceUpdated;
    }
#endif
}
