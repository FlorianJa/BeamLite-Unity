using Vuforia;
using HoloToolkit.Unity;
using UnityEngine;
using System.Collections.Generic;

public class VuMarkHandler : Singleton<VuMarkHandler>
{

    #region EVENTS

    public delegate void VuMarkDetectedHandler(Transform Marker, Transform Headset, float alignmentRotation);
    public event VuMarkDetectedHandler VuMarkDetected;

    #endregion // EVENTS

    #region PRIVATE_MEMBER_VARIABLES

    private VuMarkManager _vuMarkManager;
    private bool _offsetSetted = false;
    private List<float> _rotationValues;

    #endregion // PRIVATE_MEMBER_VARIABLES

    #region PUBLIC_MEMBER_VARIABLES

    public GameObject HololensCamera;

    public Vector3 MarkerPosition;
    public float MarkerRotation;

    public Vector3 MarkerOffset;
    #endregion // PUBLIC_MEMBER_VARIABLES

    #region UNTIY_MONOBEHAVIOUR_METHODS

    void Start()
    {
        // register callbacks to VuMark Manager
        _vuMarkManager = TrackerManager.Instance.GetStateManager().GetVuMarkManager();
        _vuMarkManager.RegisterVuMarkDetectedCallback(OnVuMarkDetected);
        _vuMarkManager.RegisterVuMarkLostCallback(OnVuMarkLost);
        _rotationValues = new List<float>();
    }

    void Update()
    {
        foreach (var marker in _vuMarkManager.GetActiveBehaviours())
        {
            if (HololensCamera && _rotationValues.Count < 20 && !_offsetSetted)
            {
                _rotationValues.Add(marker.transform.rotation.eulerAngles.y);
                Debug.Log(marker.transform.rotation.eulerAngles.y + " " + HololensCamera.transform.rotation.eulerAngles.y);

            }
            else if(!_offsetSetted)
            {
                //use median instead of average
                _rotationValues.Sort();
                var playerRotation = _rotationValues[_rotationValues.Count / 2];
                playerRotation = 360 - playerRotation;
                MarkerRotation = playerRotation;
                MarkerPosition = marker.transform.position;

                if (VuMarkDetected != null) VuMarkDetected(marker.transform, HololensCamera.transform, playerRotation);
                _offsetSetted = true;

                //turn off the marker tracking/detecting
                VuforiaBehaviour.Instance.enabled = false;
            }
        }
    }

    new void OnDestroy()
    {
        base.OnDestroy();
        // unregister callbacks from VuMark Manager
        _vuMarkManager.UnregisterVuMarkDetectedCallback(OnVuMarkDetected);
        _vuMarkManager.UnregisterVuMarkLostCallback(OnVuMarkLost);
    }

    #endregion // UNTIY_MONOBEHAVIOUR_METHODS


    #region PUBLIC_METHODS

    /// <summary>
    /// This method will be called whenever a new VuMark is detected
    /// </summary>
    public void OnVuMarkDetected(VuMarkTarget target)
    {
        Debug.Log("New VuMark: " + GetVuMarkDataAsString(target));
    }

    /// <summary>
    /// This method will be called whenever a tracked VuMark is lost
    /// </summary>
    public void OnVuMarkLost(VuMarkTarget target)
    {
        Debug.Log("Lost VuMark: " + GetVuMarkDataAsString(target));
    }

    #endregion // PUBLIC_METHODS


    #region PRIVATE_METHODS

    /// <summary>
    /// Returns data typ of vumark
    /// </summary>
    /// <param name="vumark"></param>
    /// <returns></returns>
    private string GetVuMarkDataType(VuMarkTarget vumark)
    {
        switch (vumark.InstanceId.DataType)
        {
            case InstanceIdType.BYTES:
                return "Bytes";
            case InstanceIdType.STRING:
                return "String";
            case InstanceIdType.NUMERIC:
                return "Numeric";
        }
        return "";
    }

    /// <summary>
    /// Returns the data of a vumark as string.
    /// </summary>
    /// <param name="vumark"></param>
    /// <returns></returns>
    private string GetVuMarkDataAsString(VuMarkTarget vumark)
    {
        switch (vumark.InstanceId.DataType)
        {
            case InstanceIdType.BYTES:
                return vumark.InstanceId.HexStringValue;
            case InstanceIdType.STRING:
                return vumark.InstanceId.StringValue;
            case InstanceIdType.NUMERIC:
                return vumark.InstanceId.NumericValue.ToString();
        }
        return "";
    }

    #endregion // PRIVATE_METHODS
}

