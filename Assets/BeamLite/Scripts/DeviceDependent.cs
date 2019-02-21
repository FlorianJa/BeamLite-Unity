using UnityEngine;
using System;
public class DeviceDependent : MonoBehaviour
{
    public Utils.PlayerType[] requiredPlayerType;

    void Awake()
    {
        gameObject.SetActive(Array.IndexOf(requiredPlayerType,Utils.CurrentPlayerType)>-1);
    }
}
