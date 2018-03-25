using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoloLensAlignment : MonoBehaviour {

    public BeamLiteNetworkManager NetworkManager;
    public Whiteboard whiteboard;
    // Use this for initialization
    void Start()
    {
        if (Utils.IsHoloLens)
        {
            VuMarkHandler.Instance.VuMarkDetected += Instance_VuMarkDetected;

        }
    }

    private void Instance_VuMarkDetected(Transform Marker, Transform Headset, float alignmentRotation)
    {
        //whiteboard.transform.position = VuMarkHandler.Instance.MarkerPosition + (Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * (whiteboard.transform.position - NetworkManager.MarkerOffset));
        //whiteboard.transform.rotation = Quaternion.Euler(0, -VuMarkHandler.Instance.MarkerRotation, 0) * whiteboard.transform.rotation;

        //this.transform.position = LocalAlignmentTranslation + (Quaternion.Euler(0, -LocalAlignmentRotation, 0) * (NetworkPlayerGameObject.transform.position - MarkerOffset));
        //this.transform.rotation = Quaternion.Euler(0, -LocalAlignmentRotation, 0) * NetworkPlayerGameObject.transform.rotation;


        //whiteboard.transform.position = Quaternion.Euler(0, alignmentRotation, 0) * (whiteboard.transform.position - Marker.position) + NetworkManager.MarkerOffset;
        //whiteboard.transform.rotation = Quaternion.Euler(0, alignmentRotation, 0) * whiteboard.transform.rotation;
    }

}
