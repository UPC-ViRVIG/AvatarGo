using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.IO;
using UnityEngine.Events;

public class AvatarGo : MonoBehaviour
{
    // Driver options
    public AvatarDriver.AvatarDriverType driverType = AvatarDriver.AvatarDriverType.SteamVR;
    public GameObject driverSetup;

    private GameObject head { get { return driver.head; } }
    private GameObject handLeft { get { return driver.handLeft; } }
    private GameObject handRight { get { return driver.handRight; } }
    private GameObject pelvis { get { return driver.pelvis; } }
    private GameObject footLeft { get { return driver.footLeft; } }
    private GameObject footRight { get { return driver.footRight; } }

    public GameObject head_model { get; private set; }
    public GameObject handLeft_model { get; private set; }
    public GameObject handRight_model { get; private set; }
    public GameObject pelvis_model { get; private set; }
    public GameObject footLeft_model { get; private set; }
    public GameObject footRight_model { get; private set; }

    // Global options
    public GameObject AvatarPrefab;
    public bool ShowMirror = true;
    [Tooltip("Only for OpenHandHiddenControllers or OpenHandStretchArm")] public bool FingerIKObjects = false;

    public enum ControllersStyle
    {
        OpenHandFreeControllers, // No Finger IK... controllers are free to move
        ClosedHandFreeControllers, // Finger IK... controllers are free to move
        ClosedHandAttachedControllers, // Finger IK... controllers remain attached to the hand
        OpenHandHiddenControllers, // Finger IK (objects)... controllers are not displayed
        CloseHandStretchArm, // Finger IK... arm is stretch to reach the controller
        OpenHandStretchArm // Finger IK (objects)... controllers are not displayer... arm is stretch to reach the controller
    }
    public ControllersStyle controllersStyle = ControllersStyle.ClosedHandAttachedControllers;

    public UnityEvent OnCalibrationFinished;
    public bool IsCalibrationFinished { get; private set; }

    public void OnCalibrationFinishedInvoke()
    {
        IsCalibrationFinished = true;
        if (OnCalibrationFinished != null) OnCalibrationFinished.Invoke();
    }

    public void resetControllersStyle()
    {
        bool active = false;
        bool activeControllers = false;
        if (controllersStyle == ControllersStyle.OpenHandFreeControllers || singleInput == null || singleInput.stage != PipelineUtils.Stage.DONE)
        {
            active = singleInput == null || singleInput.stage != PipelineUtils.Stage.DONE;
            activeControllers = true;

            if (controller != null && controller is AvatarController_UnityIK)
            {
                AvatarController_UnityIK controllerUnity = (AvatarController_UnityIK)controller;
                controllerUnity.SetControllersAttached(false);
                controllerUnity.SetOpenFingers();
                controllerUnity.SetClampControllers(false);
                controllerUnity.SetArmStretch(false);
            }
        }
        else if (controllersStyle == ControllersStyle.ClosedHandFreeControllers)
        {
            active = singleInput == null || singleInput.stage != PipelineUtils.Stage.DONE;
            activeControllers = true;

            if (controller != null && controller is AvatarController_UnityIK)
            {
                AvatarController_UnityIK controllerUnity = (AvatarController_UnityIK)controller;
                controllerUnity.SetControllersAttached(true);
                controllerUnity.SetAutomaticFingers(handRight.GetComponentInChildren<ControllerFingers>(true), handLeft.GetComponentInChildren<ControllerFingers>(true));
                controllerUnity.SetClampControllers(false);
                controllerUnity.SetArmStretch(false);
            }
        }
        else if (controllersStyle == ControllersStyle.ClosedHandAttachedControllers)
        {
            active = singleInput == null || singleInput.stage != PipelineUtils.Stage.DONE;
            activeControllers = true;

            if (controller != null && controller is AvatarController_UnityIK)
            {
                AvatarController_UnityIK controllerUnity = (AvatarController_UnityIK)controller;
                controllerUnity.SetControllersAttached(true);
                controllerUnity.SetAutomaticFingers(handRight.GetComponentInChildren<ControllerFingers>(true), handLeft.GetComponentInChildren<ControllerFingers>(true));
                controllerUnity.SetClampControllers(true);
                controllerUnity.SetArmStretch(false);
            }
        }
        else if (controllersStyle == ControllersStyle.OpenHandHiddenControllers)
        {
            active = false;
            activeControllers = false;

            if (controller != null && controller is AvatarController_UnityIK)
            {
                AvatarController_UnityIK controllerUnity = (AvatarController_UnityIK)controller;
                controllerUnity.SetControllersAttached(false);
                if (FingerIKObjects)
                {
                    controllerUnity.SetObjectsFingers();
                }
                else
                {
                    controllerUnity.SetUniformFingers(0.1f);
                }
                controllerUnity.SetClampControllers(false);
                controllerUnity.SetArmStretch(false);
            }
        }
        else if (controllersStyle == ControllersStyle.CloseHandStretchArm)
        {
            active = singleInput == null || singleInput.stage != PipelineUtils.Stage.DONE;
            activeControllers = true;

            if (controller != null && controller is AvatarController_UnityIK)
            {
                AvatarController_UnityIK controllerUnity = (AvatarController_UnityIK)controller;
                controllerUnity.SetControllersAttached(true);
                controllerUnity.SetAutomaticFingers(handRight.GetComponentInChildren<ControllerFingers>(true), handLeft.GetComponentInChildren<ControllerFingers>(true));
                controllerUnity.SetClampControllers(true);
                controllerUnity.SetArmStretch(true);
            }
        }
        else if (controllersStyle == ControllersStyle.OpenHandStretchArm)
        {
            active = false;
            activeControllers = false;

            if (controller != null && controller is AvatarController_UnityIK)
            {
                AvatarController_UnityIK controllerUnity = (AvatarController_UnityIK)controller;
                controllerUnity.SetControllersAttached(false);
                if (FingerIKObjects)
                {
                    controllerUnity.SetObjectsFingers();
                }
                else
                {
                    controllerUnity.SetUniformFingers(0.1f);
                }
                controllerUnity.SetClampControllers(false);
                controllerUnity.SetArmStretch(true);
            }
        }

        if (body != null && body.jointWristLeft && body.jointWristRight)
        {
            //     body.jointWristLeft.transform.localPosition = -0.15f * Vector3.forward - 0.025f * Vector3.right + 0.01f * Vector3.up;
            //     body.jointWristRight.transform.localPosition = -0.15f * Vector3.forward + 0.025f * Vector3.right + 0.01f * Vector3.up;
            body.jointWristLeft.transform.localPosition = -0.1f * Vector3.forward; // center controller
            body.jointWristRight.transform.localPosition = -0.1f * Vector3.forward;
        }

        if (head_model != null) head_model.SetActive(active);
        if (handLeft_model != null) handLeft_model.SetActive(activeControllers);
        if (handRight_model != null) handRight_model.SetActive(activeControllers);
        if (pelvis_model != null) pelvis_model.SetActive(active);
        if (footLeft_model != null) footLeft_model.SetActive(active);
        if (footRight_model != null) footRight_model.SetActive(active);
    }

    /*********************************************************/

    // Avatar main elements
    [HideInInspector] public AvatarDriver driver;
    [HideInInspector] public AvatarBody body;
    [HideInInspector] public AvatarController controller;
    private GameObject inputSystem;
    [HideInInspector] public SingleInput singleInput;
    [HideInInspector] public ContinuousInput continuousInput;

    // Scene elements
    private GameObject displayMirrorObj;
    private GameObject displayMirrorObj2;
    [HideInInspector] public DisplayMirror displayMirror;
    [HideInInspector] public GameObject avatar;
    [HideInInspector] public GameObject skeleton;
    [HideInInspector] public Animator animator;

    void Start()
    {
        // Create the avatar driver
        setupDriver(driverType, driverSetup);

        // Create the abstraction of the body
        if (body == null) body = new AvatarBody();

        // No IK controller yet
        controller = null;

        // Create scene elements
        setupScene();

        // Our scripts to control Vive input
        if (inputSystem == null)
        {
            inputSystem = new GameObject("[Input System]");
            singleInput = inputSystem.AddComponent<SingleInput>();
            continuousInput = inputSystem.AddComponent<ContinuousInput>();
            inputSystem.transform.SetParent(transform);
        }
    }

    public void InitBody(AvatarBody.BodyMeasures bodyMeasures)
    {
        body = new AvatarBody();
        body.bodyMeasures = bodyMeasures;
    }

    public void disableMirror()
    {
        if (displayMirrorObj != null) displayMirrorObj.SetActive(false);
        if (displayMirrorObj2 != null) displayMirrorObj2.SetActive(false);
    }

    public void setActiveMirror2(bool enabled)
    {
        if (displayMirrorObj2 != null) displayMirrorObj2.SetActive(enabled);
        if (displayMirrorObj != null) displayMirrorObj.SetActive(!enabled);
    }

    public void switchActiveMirror()
    {
        setActiveMirror2(!displayMirrorObj2.activeSelf);
    }

    // Scene elements
    public void setupScene()
    {
        if (driverType == AvatarDriver.AvatarDriverType.Simulation) return;

        if (ShowMirror)
        {
            // Create and place mirror
            displayMirrorObj = GameObject.Instantiate(Resources.Load("Prefabs/DisplayMirror"), this.transform) as GameObject;
            displayMirrorObj.name = "DisplayMirror";
            displayMirrorObj.transform.localPosition = new Vector3(0.0f, 1.5f, 2.0f);
            displayMirrorObj.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
            displayMirrorObj.transform.localScale = new Vector3(0.25f, 0.25f, 0.1f);
            setDisplayMirrorVisible(true);
            displayMirror = displayMirrorObj.transform.Find("Mirror").GetComponent<DisplayMirror>();

            // Create and place second mirror
            displayMirrorObj2 = GameObject.Instantiate(Resources.Load("Prefabs/DisplayMirrorTop"), this.transform) as GameObject;
            displayMirrorObj2.transform.localPosition = new Vector3(0.0f, 1.5f, 2.0f);
            displayMirrorObj2.transform.localEulerAngles = new Vector3(0.0f, 180.0f, 0.0f);
            displayMirrorObj2.transform.localScale = new Vector3(0.25f, 0.25f, 0.1f);
            Camera cameraMirror2 = displayMirrorObj2.GetComponentInChildren<Camera>();
            cameraMirror2.transform.localPosition = new Vector3(0.0f, 14.0f, 27.3f);
            cameraMirror2.transform.localEulerAngles = new Vector3(90.0f, 0.0f, 180.0f);
            cameraMirror2.transform.localScale = new Vector3(1.225f, 1.225f, 1.0f);
            setActiveMirror2(false);
        }
    }

    // Avatar driver
    public void setupDriver(AvatarDriver.AvatarDriverType driverType, GameObject driverSetup)
    {
        // We must have an instance of a driver
        if (!driverSetup)
        {
            throw new System.NullReferenceException("Could not find a VR Setup! Make sure there is one instance in the scene, " +
                "and assign it to SetupMaster via the editor.");
        }

        // The driver must be active
        if (!driverSetup.activeInHierarchy)
        {
            driverSetup.SetActive(true);
        }

        // Create the driver object
        if (driverType == AvatarDriver.AvatarDriverType.Simulation)
        {
            driver = new SimulatorDriver(driverSetup);
            VRUtils.EnableTracking(driverSetup, false);
            driver.SetActive(true);
        }
        else if (driverType == AvatarDriver.AvatarDriverType.SteamVR)
        {
            driver = new SteamVRDriver(driverSetup);
            VRUtils.EnableTracking(driverSetup, true);
        }

        Transform headModelTransform = head.transform.Find("Model");
        head_model = headModelTransform == null ? null : headModelTransform.gameObject;
        Transform handLeftTransform = handLeft.transform.Find("Controller");
        if (handLeftTransform == null) handLeftTransform = handLeft.transform.Find("Model");
        handLeft_model = handLeftTransform == null ? null : handLeftTransform.gameObject;
        Transform handRightTransform = handRight.transform.Find("Controller");
        if (handRightTransform == null) handRightTransform = handRight.transform.Find("Model");
        handRight_model = handRightTransform == null ? null : handRightTransform.gameObject;
        Transform pelvisTransform = pelvis.transform.Find("Model");
        pelvis_model = pelvisTransform == null ? null : pelvisTransform.gameObject;
        Transform footLeftTransform = footLeft.transform.Find("Model");
        footLeft_model = footLeftTransform == null ? null : footLeftTransform.gameObject;
        Transform footRightTransform = footRight.transform.Find("Model");
        footRight_model = footRightTransform == null ? null : footRightTransform.gameObject;

        resetControllersStyle();
    }

    // Avatar
    public bool loadAvatar()
    {
        if (!body.isCompleted())
        {
            return false;
        }

        clearAvatar();

        // Create the character
        HumanDescription humanDescription = new HumanDescription();
        bool success = AvatarUtils.createCharacter(
            avatarPrefab: AvatarPrefab,
            controllerFile: "Controllers/IKController",
            body: body,
            character: out avatar,
            description: ref humanDescription,
            floorMarker: null,
            avatarVR: this
        );
        if (!success)
        {
            return false;
        }
        avatar.name = AvatarPrefab.name;
        avatar.transform.parent = transform;
        avatar.SetActive(true);

        // AvatarController_UnityGood
        controller = avatar.AddComponent<AvatarController_UnityIK>();
        ((AvatarController_UnityIK)controller).AvatarVR = this;
        controller.driver = driver;
        controller.body = body;
        (controller as AvatarController_UnityIK).Desc = humanDescription;

        // Arm Stretch
        //AvatarProperties avatarProperties = avatar.GetComponent<AvatarProperties>();
        ArmsStretch armsStretch = avatar.AddComponent<ArmsStretch>();
        armsStretch.SetLeftEndEffector(body.jointWristLeft.transform);
        armsStretch.SetRightEndEffector(body.jointWristRight.transform);
        ((AvatarController_UnityIK)controller).SetArmStretch(armsStretch);

        setIKActive(true);

        resetControllersStyle();

        return true;
    }

    public GameObject loadAvatarMannequin(out GameObject avatarRootStep)
    {
        avatarRootStep = null;
        // Choose the name of the selected avatar
        bool success;

        HumanDescription humanDescription = new HumanDescription();

        // Create the character
        GameObject floorMarker = new GameObject("Floor Marker");

        success = AvatarUtils.createCharacter(
            avatarPrefab: AvatarPrefab,
            controllerFile: "",
            body: body,
            character: out avatar,
            description: ref humanDescription,
            floorMarker: floorMarker,
            avatarVR: this
        );
        if (!success)
        {
            return null;
        }
        avatar.name = AvatarPrefab.name;
        avatar.transform.SetParent(transform, false);
        SetAnimationControllers();

        avatarRootStep = avatar;
        return floorMarker;
    }

    public void clearAvatar()
    {
        if (handRight_model != null)
        {
            // Reset RightHand Model before destroying the avatar
            handRight_model.transform.SetParent(handRight.transform);
            handRight_model.transform.localPosition = Vector3.zero;
            handRight_model.transform.localRotation = Quaternion.identity;
        }

        if (handLeft_model != null)
        {
            handLeft_model.transform.SetParent(handLeft.transform);
            handLeft_model.transform.localPosition = Vector3.zero;
            handLeft_model.transform.localRotation = Quaternion.identity;
        }

        if (avatar)
        {
            Destroy(avatar);
        }
        if (skeleton)
        {
            Destroy(skeleton);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (UnityEditor.EditorApplication.isPlaying)
        {
            resetControllersStyle();
        }
    }
#endif

    /************************* UI ****************************/

    private void setDisplayMirrorVisible(bool flag)
    {
        if (displayMirrorObj)
        {
            displayMirrorObj.SetActive(flag);
        }
    }

    private void setIKActive(bool flag)
    {
        if (controller != null)
        {
            controller.ikActive = flag;
        }
    }

    public Vector3 queryJointPositionAvatar(HumanBodyBones queryJoint)
    {
        Animator animator = avatar.GetComponent<Animator>();
        Transform t = animator.GetBoneTransform(queryJoint);
        return t.position;
    }

    void SetAnimationControllers()
    {
        animator = avatar.GetComponent<Animator>();
        animator.runtimeAnimatorController = Resources.Load("Internal/TPoseController") as RuntimeAnimatorController;
        resetControllersStyle();
        avatar.SetActive(true);
    }
}