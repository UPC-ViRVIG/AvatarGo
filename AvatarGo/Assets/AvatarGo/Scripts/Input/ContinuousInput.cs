using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// It captures a point and can be read from another class.
/// This version is simple, but it can be useful to test if the given gameobject/controller is valid
/// and don't return any value until it is valid again. (For example, check state of SteamVR controller/tracker)
/// </summary>
public class ContinuousInput : MonoBehaviour
{
    private AvatarGo avatarVR;

    // Used for capturing points
    public bool capturing = false;
    public Vector3 capturedPoint = new Vector3();
    private GameObject capturingGameObject = null;

    void Start()
    {
        avatarVR = transform.parent.GetComponent<AvatarGo>();
    }

    // Only used for caputuring devices location
    void Update()
    {
        if (capturing)
        {
            capturedPoint = getPointFirst();
            capturing = false;
        }
    }

    private Vector3 getPointFirst()
    {
        return capturingGameObject.transform.position;
    }

    public void captureInstantPosition(GameObject gO)
    {
        if (gO == null)
        {
            return;
        }
        capturing = true;
        capturingGameObject = gO;
    }

    public Vector3 getInstantPosition()
    {
        Debug.Assert(!capturing, "getInstantPosition() cannot be called until ContinousInput has finished capturing points");
        return capturedPoint;
    }
}