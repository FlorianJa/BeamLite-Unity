using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkPen : NetworkBehaviour {

    public GameObject Pen;
    public Transform MarkerOffsetTransform;

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        if(hasAuthority)
        {
            if (Utils.IsVR)
            {
                this.transform.position = Pen.transform.position;
                //this.transform.rotation = Pen.transform.rotation;
            }
            else
            {
                this.transform.position = Quaternion.Euler(0, VuMarkHandler.Instance.MarkerRotation, 0) * (Pen.transform.position - VuMarkHandler.Instance.MarkerPosition) + MarkerOffsetTransform.position;
               // this.transform.rotation = Quaternion.Euler(0, VuMarkHandler.Instance.MarkerRotation, 0) * Pen.transform.rotation;
            }
            this.transform.rotation = Pen.transform.rotation;
        }
	}
}
