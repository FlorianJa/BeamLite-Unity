using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SizeController : MonoBehaviour {

    public GameObject WhiteboardPen;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    internal void updown(bool up)
    {
        WhiteboardPen.GetComponent<NewWhiteboardPen>().UpDownPenSize(up);
        GetComponent<TextMesh>().text=""+WhiteboardPen.GetComponent<NewWhiteboardPen>().PenSize;
    }
}
