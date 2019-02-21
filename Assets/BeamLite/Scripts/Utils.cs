using System;
using UnityEngine.VR;

public class Utils
{
    private static PlayerType playerType = PlayerType.Undetermined;

    public enum PlayerType
    {
        Undetermined,
        Unknown,
        HoloLens,
        VR,
        Vive,
        Rift,
    }

    public static PlayerType CurrentPlayerType
    {
        get
        {
            if (playerType == PlayerType.Undetermined || playerType == PlayerType.VR)
            {
                switch (VRSettings.loadedDeviceName)
                {
                    case "HoloLens":
                        playerType = PlayerType.HoloLens;
                        break;
                    case "OpenVR":
                        //playerType = PlayerType.VR;
                        if (VRDevice.model != "")
                        {
                            if (VRDevice.model.Contains("Rift"))
                            {
                                playerType = PlayerType.Rift;
                            }
                            else if (VRDevice.model.Contains("Vive"))
                            {
                                playerType = PlayerType.Vive;
                            }
                        }
                        break;
                    default:
#if UNITY_EDITOR
                        playerType = PlayerType.HoloLens;
#else
                        playerType = PlayerType.VR;
#endif
                        break;
                }
            }

            return playerType;
        }
    }

    public static bool IsHoloLens
    {
        get
        {
            return CurrentPlayerType == PlayerType.HoloLens;
        }
    }

    public static bool IsVR
    {
        get
        {
            return (CurrentPlayerType == PlayerType.Rift || CurrentPlayerType == PlayerType.Vive || CurrentPlayerType == PlayerType.VR );
        }
    }
}
