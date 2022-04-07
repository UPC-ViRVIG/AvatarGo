using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SingleInput : MonoBehaviour
{
    private AvatarGo avatarVR;

    // SteamVR controllers
    private SteamVR_Behaviour_Pose SteamVRControllerLeft;
    private SteamVR_Behaviour_Pose SteamVRControllerRight;

    // Input actions
    private SteamVR_Action_Boolean SteamVRTrigger = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "GrabPinch");
    private SteamVR_Action_Boolean SteamVRGrip = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("default", "GrabGrip");

    // Whether the SteamVR controllers have been assigned
    private bool controllersStarted = false;

    // Whether the controllers events should not be processed
    private bool applicationBusy = false;
    private bool controllerRecent = false;

    // Stage in the Avatar Setup pipeline
    public PipelineUtils.Stage stage = PipelineUtils.Stage.DEVICES;
    private bool succeeded = false; // whether current stage is completed

    void Start()
    {
        avatarVR = transform.parent.GetComponent<AvatarGo>();
    }

    // Start controllers
    private void StartControllers()
    {
        SteamVRControllerLeft = null;
        SteamVRControllerRight = null;
        if (avatarVR.driver.handLeft.activeInHierarchy)
        {
            SteamVRControllerLeft = avatarVR.driver.handLeft.GetComponent<SteamVR_Behaviour_Pose>();
            controllersStarted = true;
        }
        if (avatarVR.driver.handRight.activeInHierarchy)
        {
            SteamVRControllerRight = avatarVR.driver.handRight.GetComponent<SteamVR_Behaviour_Pose>();
            controllersStarted = true;
        }
    }

    /*
        Pipeline STAGES:
          - DIRTY   -> Initial stage (This STAGE is NEVER set... it is only used for messaging)
          - DEVICES -> Devices are identified and correctly positioned (DEVICES and T_POSE are done at the same time)
          - T_POSE  -> Some static measurements
          - ROOT_AVATAR    -> Avatar is constructed using Body measurements, then is placed in the scene, finally the user enters inside to compute exact offsets
          - DONE           -> completed = true, the avatar is loaded
    */

    private GameObject floorMarker = null;
    private int floorMarkerFlag = 0;
    private GameObject avatarRootStep;
    private bool placeLastAvatar = false;

    void Update()
    {
        if (avatarVR.driver == null) return;

        StickAvatarToGround();

        // succeeded -> current stage has been completed
        if (succeeded) // Progress - no input handled here
        {
            PipelineUtils.Stage next = PipelineUtils.nextStage(avatarVR.driver, stage);
            PipelineUtils.displayInBetweenStagesMessage(avatarVR, avatarVR.displayMirror, stage, next);
            stage = next;
            if (stage == PipelineUtils.Stage.ROOT_AVATAR)
            {
                // after the neck has been set, we have computed all the measurements needed. Now we want to show the new rescaled avatar
                avatarVR.clearAvatar();
                floorMarker = avatarVR.loadAvatarMannequin(out avatarRootStep);
            }
            if (stage == PipelineUtils.Stage.DONE)
            {
                avatarVR.disableMirror();
                avatarVR.body.setCompleted(true);
                avatarVR.loadAvatar();
                avatarVR.OnCalibrationFinishedInvoke();
            }
            succeeded = false;
        }

        if (floorMarker != null) // Sticking avatar to the ground
        {
            return;
        }

        if (applicationBusy || controllerRecent) // Busy
        {
            return;
        }

        if (!controllersStarted) // controllersStarted -> false: STAGE is DIRTY
                                 //                       true: controllers detected but NOT identified by position
        {
            bool allDevicesConnected = false;
            if ((avatarVR.driver as SteamVRDriver) != null) // add/change this for different drivers
            {
                allDevicesConnected = (avatarVR.driver as SteamVRDriver).detectDevices(avatarVR.displayMirror);
            }
            if (allDevicesConnected) StartControllers();
        }

        // Don't start until at least one controller is available
        if ((avatarVR.driver as SteamVRDriver) != null &&
            SteamVRControllerLeft == null && SteamVRControllerRight == null) // add/change this for different drivers
        {
            return;
        }

        UpdateControllers();

        // Identify controllers, if needed
        if (IsTriggerDown() && !avatarVR.driver.isReady()) // isReady() -> false: waiting for devices to be identified
                                                           //              true: ready to obtain measures, STAGE is DEVICES
        {
            if ((avatarVR.driver as SteamVRDriver) != null) // add/change this for different drivers
            {
                (avatarVR.driver as SteamVRDriver).identifyDevices(avatarVR.displayMirror);
            }
            StartControllers();
            return;
        }

        // Start pipeline if ready
        if (avatarVR.driver.isReady() && !avatarVR.body.isStarted()) // isStarted() -> false: Pipeline still in STAGE 0 (DEVICES)
                                                                     //                true: if loadBodyMeasures then (STAGE is ROOT_AVATAR) else (STAGE is ANKLES)
        {
            stage = PipelineUtils.Stage.DEVICES;
            PipelineUtils.Stage next = PipelineUtils.nextStage(avatarVR.driver, PipelineUtils.Stage.DEVICES);
            if (avatarVR.driverType != AvatarDriver.AvatarDriverType.Simulation)
            {
                stage = next;
                if (stage == PipelineUtils.Stage.T_POSE)
                {
                    StartCoroutine(avatarVR.body.computeHeightWidthMeasure(result => succeeded = result, avatarVR.driver, avatarVR.displayMirror, avatarVR));
                    StartCoroutine(ControllersIdleForSecs(1));
                }
                else Debug.Assert(false, "Something went wrong. This stage should be T_Pose, but it is: " + stage.ToString());
            }
            else
            {
                stage = next;
                avatarVR.body.computeFeetMeasuresUsingRoot(avatarVR, avatarVR.driver.footLeft.transform.position, avatarVR.driver.footRight.transform.position, avatarVR.driver.pelvis.transform.position);
                succeeded = true;
            }

            avatarVR.body.setStarted(true);
            return;
        }

        // Application pipeline: forward
        if (IsTriggerDown() || (avatarVR.driverType == AvatarDriver.AvatarDriverType.Simulation))
        {
            ForwardPipeline(avatarVR.driver);
        }

        // Switch from Top Mirror to Front Mirror
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            avatarVR.switchActiveMirror();
        }
    }

    // Application pipeline: forward
    public void ForwardPipeline(AvatarDriver driver)
    {
        if (stage == PipelineUtils.Stage.ROOT_AVATAR)
        {
            avatarVR.body.setupLeftWristJoint(driver.handLeft, avatarVR);
            avatarVR.body.setupRightWristJoint(driver.handRight, avatarVR);
            avatarVR.body.setupNeckJoint(driver.head, avatarVR);

            StartCoroutine(avatarVR.body.computeRootMeasuresUsingAvatar(result => succeeded = result, driver, avatarVR.displayMirror,
               avatarVR.animator, avatarVR));
            StartCoroutine(ControllersIdleForSecs(1));
        }
    }

    private void UpdateControllers()
    {
        if (SteamVRControllerRight == null || SteamVRControllerLeft == null) StartControllers();
    }

    // Getters / Setters

    public void setApplicationBusy(bool flag)
    {
        applicationBusy = flag;
    }

    public void setControllerRecent(bool flag)
    {
        controllerRecent = flag;
    }

    // Helper functions to block controllers input

    private IEnumerator ControllersIdleForSecs(int secs)
    {
        setControllerRecent(true);
        if (secs > 0)
        {
            yield return new WaitForSeconds(secs);
            setControllerRecent(false);
        }
    }

    public void blockControllers(bool flag)
    {
        setApplicationBusy(flag);
    }

    // Grab inputs
    public bool IsTriggerDown()
    {
        return
            (SteamVRControllerLeft != null && SteamVRTrigger.GetLastStateDown(SteamVRControllerLeft.inputSource)) ||
            (SteamVRControllerRight != null && SteamVRTrigger.GetLastStateDown(SteamVRControllerRight.inputSource)) ||
            (Input.GetKeyDown(KeyCode.Space));
    }

    private void StickAvatarToGround()
    {
        // Correct loaded avatar in ROOT_AVATAR step, after 2 frame (to let the animator system to correctly place all bones... Unity :) )
        if (!placeLastAvatar && floorMarker != null && floorMarkerFlag++ == 2)
        {
            RaycastHit hit;
            float difference = 0.0f;
            Vector3 origin = floorMarker.transform.position;
            if (Physics.Raycast(origin, Vector3.down, out hit, 0.5f))
            {
                difference = hit.point.y - origin.y;
            }
            else if (Physics.Raycast(origin, Vector3.up, out hit, 0.5f))
            {
                difference = hit.point.y - origin.y;
            }

            avatarRootStep.transform.Translate(new Vector3(0.0f, difference, 0.0f), Space.World);
            floorMarker = null;
            floorMarkerFlag = 0;
        }
    }
}