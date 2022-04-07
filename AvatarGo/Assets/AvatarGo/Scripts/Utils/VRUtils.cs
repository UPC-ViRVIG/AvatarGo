using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


public class VRUtils
{
    // Enable/disable SteamVR tracking on descendant GameObjects
    public static void EnableTracking(GameObject obj, bool flag)
    {
        EnableTrackingRecursive(obj, flag);

        // HMD
        if (obj.GetComponent<Camera>())
        {
            obj.GetComponent<Camera>().enabled = flag;
        }
    }

    private static void EnableTrackingRecursive(GameObject obj, bool flag)
    {
        if (obj.GetComponent<SteamVR_TrackedObject>())
        {
            obj.GetComponent<SteamVR_TrackedObject>().enabled = flag;
        }
        Transform current = obj.transform;
        for (int i = 0; i < current.childCount; ++i)
        {
            EnableTrackingRecursive(current.GetChild(i).gameObject, flag);
        }
    }
}