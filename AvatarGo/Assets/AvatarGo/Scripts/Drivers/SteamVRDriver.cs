using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


public class SteamVRDriver : AvatarDriver
{
    // Global constants
    private static float HEAD_COSINE_DEVIATION_THRESHOLD = 0.5f;
    private static float MAX_HEAD_TO_WAIST_DISTANCE = 0.8f;
    private static uint maxDeviceCount = 16;

    // Aliases GameObjects for SteamVR devices
    private GameObject HMD;
    private GameObject ControllerLeft;
    private GameObject ControllerRight;
    private GameObject TrackerRoot;
    private GameObject TrackerLeft;
    private GameObject TrackerRight;

    private int[] TrackerIndices = new int[3];


    // How many devices detected
    private uint numConnectedControllers;
    private uint numConnectedTrackers;

    // Constructor
    public SteamVRDriver(GameObject obj) : base(obj)
    {
        type = AvatarDriver.AvatarDriverType.SteamVR;

        // Get references to GameObjects
        HMD = head;
        ControllerLeft = handLeft;
        ControllerRight = handRight;
        TrackerRoot = pelvis;
        TrackerLeft = footLeft;
        TrackerRight = footRight;

#if UNITY_EDITOR
        // Create a layer for everything that should not be seen from the HMD
        LayerUtils.CreateLayer("NotHMD");
        if (HMD)
        {
            LayerUtils.HideLayerInCamera("NotHMD", HMD.GetComponentInChildren<Camera>());
        }

        // Create a layer for everything that should only be seen from the HMD
        LayerUtils.CreateLayer("OnlyHMD");
        if (HMD)
        {
            Camera[] activeCameras = Camera.allCameras; // FIXME: should loop over all cameras, not just active ones
            foreach (Camera cam in activeCameras)
            {
                if (cam != HMD.GetComponentInChildren<Camera>())
                {
                    LayerUtils.HideLayerInCamera("OnlyHMD", cam);
                }
            }
        }
#endif

        // No device detected yet
        numConnectedControllers = 0;
        numConnectedTrackers = 0;

        // Create devices models
        setupDevicesModels();

        // TODO: mask sphere to hide head from user
        // for now, disable it, as it is not working
        if (HMD)
        {
            if (Utils.FindDescendants(HMD.transform, "MaskHead"))
            {
                Utils.FindDescendants(HMD.transform, "MaskHead").gameObject.SetActive(false);
            }
        }

        // Devices have not been identified
        ready = false;
    }

    // Devices
    private void setupDevicesModels()
    {
        // TODO: remove any child GameObject with name "Model"
        // Add models from file ourselves (this ensures models will be present,
        // and removes possible conflits with SteamVR_RenderModel)

#if UNITY_EDITOR
        // Hide HMD model from HMD
        if (HMD)
        {
            if (HMD.transform.Find("Model"))
            {
                LayerUtils.MoveToLayer(HMD.transform.Find("Model").gameObject, "NotHMD");
            }
        }
#endif

        // Colour devices
        colourDevices();
    }

    // Colour devices according to stickers
    public void colourDevices()
    {
        GameObject cameraHeadModel = HMD.transform.Find("Model").gameObject;
        if (cameraHeadModel)
        {
            if (cameraHeadModel.GetComponentInChildren<MeshRenderer>())
            {
                cameraHeadModel.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
            }
        }
        GameObject controllerLeftModel = ControllerLeft.transform.Find("Controller").gameObject;
        if (controllerLeftModel == null) controllerLeftModel = ControllerLeft.transform.Find("Model").gameObject;
        if (controllerLeftModel)
        {
            if (controllerLeftModel.GetComponentInChildren<MeshRenderer>())
            {
                controllerLeftModel.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            }
        }
        GameObject controllerRightModel = ControllerRight.transform.Find("Controller").gameObject;
        if (controllerRightModel == null) controllerRightModel = ControllerRight.transform.Find("Model").gameObject;
        if (controllerRightModel)
        {
            if (controllerRightModel.GetComponentInChildren<MeshRenderer>())
            {
                controllerRightModel.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta;
            }
        }
        GameObject trackerRootModel = TrackerRoot.transform.Find("Model").gameObject;
        if (trackerRootModel)
        {
            if (trackerRootModel.GetComponentInChildren<MeshRenderer>())
            {
                trackerRootModel.GetComponentInChildren<MeshRenderer>().material.color = Color.green;
            }
        }
        GameObject trackerLeftModel = TrackerLeft.transform.Find("Model").gameObject;
        if (trackerLeftModel)
        {
            if (trackerLeftModel.GetComponentInChildren<MeshRenderer>())
            {
                trackerLeftModel.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
            }
        }
        GameObject trackerRightModel = TrackerRight.transform.Find("Model").gameObject;
        if (trackerRightModel)
        {
            if (trackerRightModel.GetComponentInChildren<MeshRenderer>())
            {
                trackerRightModel.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
            }
        }
    }

    // Assigns device indices and enables/disables GameObjects accordingly
    public bool setDevicesIndex()
    {
        if (numConnectedControllers >= 2)
        {
            ControllerLeft.SetActive(true);
        }
        else
        {
            ControllerLeft.SetActive(false);
        }

        if (numConnectedControllers >= 1)
        {
            ControllerRight.SetActive(true);
        }
        else
        {
            ControllerRight.SetActive(false);
        }

        if (numConnectedTrackers >= 1)
        {
            TrackerRoot.SetActive(true);
            TrackerRoot.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)TrackerIndices[0];
        }
        else
        {
            TrackerRoot.SetActive(false);
        }

        if (numConnectedTrackers >= 3)
        {
            TrackerLeft.SetActive(true);
            TrackerLeft.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)TrackerIndices[2];
        }
        else
        {
            TrackerLeft.SetActive(false);
        }

        if (numConnectedTrackers >= 2)
        {
            TrackerRight.SetActive(true);
            TrackerRight.GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)TrackerIndices[1];
        }
        else
        {
            TrackerRight.SetActive(false);
        }

        return true;
    }

    // Finds connected devices and sets temporal device indices
    public bool detectDevices(DisplayMirror displayMirror)
    {
        // Init global vars
        numConnectedControllers = 0;
        numConnectedTrackers = 0;

        // Get pose relative to the safe bounds defined by the user
        TrackedDevicePose_t[] trackedDevicePoses = new TrackedDevicePose_t[maxDeviceCount];
        if (OpenVR.Settings != null)
        {
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, trackedDevicePoses);
        }

        // Loop over connected devices
        for (uint i = 0; i < maxDeviceCount; ++i)
        {
            // deviceClass sometimes returns the wrong class for a device, hence we use a string
            /*ETrackedDeviceClass deviceClass = ETrackedDeviceClass.Invalid;
            if (OpenVR.Settings != null)
            {
                deviceClass = OpenVR.System.GetTrackedDeviceClass(i);
            }*/
            ETrackingResult status = trackedDevicePoses[i].eTrackingResult;
            var result = new System.Text.StringBuilder((int)64);
            var error = ETrackedPropertyError.TrackedProp_Success;
            if (OpenVR.System != null)
            {
                OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
            }
            // Handle HMD
            if (result.ToString().Contains("hmd") && status == ETrackingResult.Running_OK)
            //else if (deviceClass == ETrackedDeviceClass.HMD && status == ETrackingResult.Running_OK)
            {
                continue;
            }
            // Handle controllers
            else if (result.ToString().Contains("controller") && status == ETrackingResult.Running_OK)
            //else if (deviceClass == ETrackedDeviceClass.Controller && status == ETrackingResult.Running_OK)
            {
                numConnectedControllers++;
            }
            // Handle trackers
            else if (result.ToString().Contains("tracker_vive") && status == ETrackingResult.Running_OK)
            //else if (deviceClass == ETrackedDeviceClass.GenericTracker && status == ETrackingResult.Running_OK)
            {
                TrackerIndices[numConnectedTrackers] = (int)i;
                numConnectedTrackers++;
            }
        }

        string message = string.Format(PipelineUtils.progressMessageAt(PipelineUtils.Stage.DIRTY), numConnectedControllers, numConnectedTrackers);
        if (numConnectedControllers >= 2 && numConnectedTrackers >= 3)
        {
            string message2 = PipelineUtils.introMessageAt(PipelineUtils.Stage.DEVICES);
            if (displayMirror)
            {
                displayMirror.ShowTextAgain(message, new Color(1.0f, 1.0f, 1.0f, 0.5f), 2, message2, new Color(1.0f, 1.0f, 1.0f, 0.5f), 0, true);
            }
        }
        else
        {
            message = message + " " + PipelineUtils.failureMessageAt(PipelineUtils.Stage.DIRTY);
            if (displayMirror)
            {
                displayMirror.ShowText(message, new Color(1.0f, 0.0f, 0.0f, 0.5f), 0, true);
            }
        }

        // Not done
        ready = false;

        if (numConnectedControllers < 2 || numConnectedTrackers < 3) return false; // not enough devices

        // Asign correct indices
        return setDevicesIndex();
    }

    // Fixes indices of tracked devices
    public bool identifyDevices(DisplayMirror displayMirror)
    {
        displayMirror.avatarVR.singleInput.blockControllers(true);

        if (numConnectedControllers + numConnectedTrackers < 2)
        {
            string message = PipelineUtils.failureMessageAt(PipelineUtils.Stage.DEVICES, 0);
            Debug.Log(message + "\n");
            if (displayMirror)
            {
                displayMirror.ShowText(message, new Color(1.0f, 0.0f, 0.0f, 0.5f), 2, true);
            }
            //ViveInput.blockControllers(false); // no need, displayPanel will unblock them
            return false;
        }

        uint numPoints = 1 + numConnectedControllers + numConnectedTrackers;
        Vector3[] points = new Vector3[numPoints];
        GameObject[] deviceObjects = new GameObject[numPoints];
        //  points[0] = HMD position                deviceIndices[0] = HMD GameObject     
        //  points[1] = Controller 1 position       deviceIndices[1] = Controller 1 GameObject     
        //  points[2] = Controller 2 position       deviceIndices[2] = Controller 2 GameObject     
        // ...                                      ...
        //  points[n] = Controller n position       deviceIndices[n] = Controller n GameObject     
        //  points[n+1] = Tracker 1 position        deviceIndices[n+1] = Tracker 1 GameObject     
        //  points[n+2] = Tracker 2 position        deviceIndices[n+2] = Tracker 2 GameObject     
        //  points[n+3] = Tracker 3 position        deviceIndices[n+3] = Tracker 3 GameObject     
        // ...                                      ...
        uint controllerIndex0 = 1;
        uint trackerIndex0 = controllerIndex0 + numConnectedControllers;

        uint controllerIndex = controllerIndex0;
        uint trackerIndex = trackerIndex0;

        if (head.activeInHierarchy)
        {
            points[0] = head.transform.position;
            deviceObjects[0] = head;
        }

        if (handLeft.activeInHierarchy)
        {
            points[controllerIndex] = handLeft.transform.position;
            deviceObjects[controllerIndex] = handLeft;
            controllerIndex++;
        }
        if (handRight.activeInHierarchy)
        {
            points[controllerIndex] = handRight.transform.position;
            deviceObjects[controllerIndex] = handRight;
            controllerIndex++;
        }
        Debug.Assert(controllerIndex == numConnectedControllers + 1);

        if (pelvis.activeInHierarchy)
        {
            points[trackerIndex] = pelvis.transform.position;
            deviceObjects[trackerIndex] = pelvis;
            trackerIndex++;
        }
        if (footLeft.activeInHierarchy)
        {
            points[trackerIndex] = footLeft.transform.position;
            deviceObjects[trackerIndex] = footLeft;
            trackerIndex++;
        }
        if (footRight.activeInHierarchy)
        {
            points[trackerIndex] = footRight.transform.position;
            deviceObjects[trackerIndex] = footRight;
            trackerIndex++;
        }
        Debug.Assert(trackerIndex == numConnectedControllers + numConnectedTrackers + 1);

        // Fit plane to tracked objects locations
        float a = 0.0f, b = 0.0f, c = 0.0f, d = 0.0f;
        bool res = Utils.FitPlane(numPoints, points, ref a, ref b, ref c, ref d);
        if (!res)
        {
            string message = PipelineUtils.failureMessageAt(PipelineUtils.Stage.DEVICES, 1);
            Debug.Log(message + "\n");
            if (displayMirror)
            {
                displayMirror.ShowText(message, new Color(1.0f, 0.0f, 0.0f, 0.5f), 2, true);
            }
            //ViveInput.blockControllers(false); // no need, displayPanel will unblock them
            return false;
        }
        Vector3 n = new Vector3(a, b, c);
        n = Vector3.Normalize(n);

        // Get HMD forward vector
        Vector3 f = HMD.transform.forward;
        f = Vector3.Normalize(f);

        //  Compute deviation between plane normal and HMD forward
        float deviation = Vector3.Dot(n, f);

        // Make sure plane points in the same direction 
        if (System.Math.Abs(deviation) < HEAD_COSINE_DEVIATION_THRESHOLD)
        {
            string message = PipelineUtils.failureMessageAt(PipelineUtils.Stage.DEVICES, 2);
            Debug.Log(message + "\n");
            if (displayMirror)
            {
                displayMirror.ShowText(message, new Color(1.0f, 0.0f, 0.0f, 0.5f), 2, true);
            }
            //ViveInput.blockControllers(false); // no need, displayPanel will unblock them
            return false;
        }
        if (deviation < 0.0f)
        {
            n = -1.0f * n;
        }

        // Get a point on the plane
        Vector3 p = new Vector3(0.0f, 0.0f, -d / c);

        // Project points on plane
        Vector3[] projectedPoints = new Vector3[numPoints];
        for (uint i = 0; i < numPoints; ++i)
        {
            Vector3 t = points[i] - p;
            float dist = Vector3.Dot(t, n);
            projectedPoints[i] = points[i] - dist * n;
        }

        // Build u,v coordinate system
        Vector3 v = Vector3.up;
        Vector3 u = Vector3.Cross(v, n);
        float u0 = Vector3.Dot(projectedPoints[0], u); // HMD
        float v0 = Vector3.Dot(projectedPoints[0], v);

        // Get uv coordinates
        Vector2[] planePoints = new Vector2[numPoints];
        planePoints[0] = new Vector2(0.0f, 0.0f); // HMD will be origin of uv space
        for (uint i = 1; i < numPoints; ++i)
        {
            float u_coord = Vector3.Dot(projectedPoints[i], u) - u0;
            float v_coord = Vector3.Dot(projectedPoints[i], v) - v0;
            Vector2 uv = new Vector2(u_coord, v_coord);
            planePoints[i] = uv;
        }

        // Identify controllers/trackers according to uv coordinates
        for (uint i = 0; i < numConnectedControllers; ++i)
        {
            if (planePoints[controllerIndex0 + i].x < 0.0f)
            {
                handLeft = deviceObjects[controllerIndex0 + i];
            }
            else
            {
                handRight = deviceObjects[controllerIndex0 + i];
            }
        }
        for (uint i = 0; i < numConnectedTrackers; ++i)
        {
            if (System.Math.Abs(planePoints[trackerIndex0 + i].y) < MAX_HEAD_TO_WAIST_DISTANCE)
            {
                pelvis = deviceObjects[trackerIndex0 + i];
            }
            else if (planePoints[trackerIndex0 + i].x < 0.0f)
            {
                footLeft = deviceObjects[trackerIndex0 + i];
            }
            else
            {
                footRight = deviceObjects[trackerIndex0 + i];
            }
        }

        // Asign correct indices
        HMD = head;
        ControllerLeft = handLeft;
        ControllerRight = handRight;
        TrackerRoot = pelvis;
        TrackerLeft = footLeft;
        TrackerRight = footRight;
        // Update variables
        ready = true;
        displayMirror.avatarVR.singleInput.blockControllers(false);
        displayMirror.CleanText();
        colourDevices();
        return true;
    }
}
