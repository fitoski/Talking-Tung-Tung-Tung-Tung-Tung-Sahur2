using UnityEngine;

public class DeviceIDLogger : MonoBehaviour
{
    void Start()
    {
        Debug.Log("Device ID: " + SystemInfo.deviceUniqueIdentifier);
    }
}
