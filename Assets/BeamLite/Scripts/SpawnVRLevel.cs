using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnVRLevel : MonoBehaviour {


#if UNITY_STANDALONE
    public GameObject VRLevel;
#endif
	// Use this for initialization
	void Start () {

#if UNITY_STANDALONE

        if (VRLevel)
        {
            Instantiate(VRLevel);
        }

#endif
    }
}
