using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class AvatarBody
{

    // Global GameObjects for joints
    public GameObject jointNeck;
    public GameObject jointWristLeft, jointWristRight;
    public GameObject jointRoot;
    public GameObject jointAnkleLeft, jointAnkleRight;

    // Body measures obtained from the the setup process
    [Serializable]
    public struct BodyMeasures
    {
        public float footToAnkleLeft;
        public float footToAnkleRight;
        public float eyesHeight;
        public float depthCenterHead;
        public float widthCenterHead;
        public Vector3 leftFootRootStep, rightFootRootStep, pelvisRootStep, headRootStep, leftHandRootStep, rightHandRootStep;
        public Quaternion leftFootRotRootStep, rightFootRotRootStep, pelvisRotRootStep, headRotRootStep, leftHandRotRootStep, rightHandRotRootStep;
    }
    public BodyMeasures bodyMeasures;

    // Whether all the joints have been set
    public bool completed = false;

    // Whether any joint has been set
    private bool started = false;

    // Constructor
    public AvatarBody()
    {
        jointNeck = null;
        jointWristLeft = null;
        jointWristRight = null;
        jointRoot = null;
        jointAnkleLeft = null;
        jointAnkleRight = null;

        bodyMeasures.footToAnkleLeft = 0.2769778f;
        bodyMeasures.footToAnkleRight = 0.2772783f;
        bodyMeasures.eyesHeight = 1.68952f;
        bodyMeasures.depthCenterHead = -0.07f;
        bodyMeasures.widthCenterHead = -0.01000001f;

        completed = false;
    }

    // Computing body measures

    // Step 1: height - measure taken from HMD && first aproximation to foot size using root
    public IEnumerator computeHeightWidthMeasure(System.Action<bool> success, AvatarDriver driver, DisplayMirror displayMirror, AvatarGo avatarVR)
    {
        avatarVR.singleInput.blockControllers(true);

        Vector3 headPosition = new Vector3();
        Vector3 pelvisPosition = new Vector3();

        if (driver.head && driver.head.activeInHierarchy)
        {
            avatarVR.continuousInput.captureInstantPosition(driver.head);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            headPosition = avatarVR.continuousInput.getInstantPosition();
            bodyMeasures.eyesHeight = headPosition.y;
        }

        // This is the only place where we are sure the user is standing in T-pose
        // Compute the vector from the root to the neck here
        if (driver.head && driver.head.activeInHierarchy && driver.pelvis && driver.pelvis.activeInHierarchy)
        {
            // Capture pelvis
            avatarVR.continuousInput.captureInstantPosition(driver.pelvis);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            pelvisPosition = avatarVR.continuousInput.getInstantPosition();
        }

        if (driver.footLeft && driver.footLeft.activeInHierarchy &&
            driver.footRight && driver.footRight.activeInHierarchy)
        {
            // Capture footLeft
            avatarVR.continuousInput.captureInstantPosition(driver.footLeft);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 footLeftPosition = avatarVR.continuousInput.getInstantPosition();
            // Capture footRight
            avatarVR.continuousInput.captureInstantPosition(driver.footRight);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 footRightPosition = avatarVR.continuousInput.getInstantPosition();

            computeFeetMeasuresUsingRoot(avatarVR, footLeftPosition, footRightPosition, pelvisPosition);
        }

        avatarVR.singleInput.blockControllers(false);
        success(true);
        yield break;
    }

    public IEnumerator computeRootMeasuresUsingAvatar(System.Action<bool> success, AvatarDriver driver, DisplayMirror displayMirror, Animator animator, AvatarGo avatarVR)
    {
        avatarVR.singleInput.blockControllers(true);

        if (driver.pelvis && driver.pelvis.activeInHierarchy)
        {
            // Capture pelvis
            avatarVR.continuousInput.captureInstantPosition(driver.pelvis);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 pelvisPosition = avatarVR.continuousInput.getInstantPosition();
            // Capture left foot
            avatarVR.continuousInput.captureInstantPosition(driver.footLeft);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 leftFoot = avatarVR.continuousInput.getInstantPosition();
            // Capture right foot
            avatarVR.continuousInput.captureInstantPosition(driver.footRight);
            yield return new WaitUntil(() => !avatarVR.continuousInput.capturing);
            Vector3 rightFoot = avatarVR.continuousInput.getInstantPosition();

            setupRootJoint(driver.pelvis, pelvisPosition, driver.pelvis.transform.rotation, avatarVR);
            correctLeftAnkleOffset(avatarVR, leftFoot, driver.footLeft.transform.rotation);
            correctRightAnkleOffset(avatarVR, rightFoot, driver.footRight.transform.rotation);

            saveRootStepData(driver);
        }

        avatarVR.singleInput.blockControllers(false);
        success(true);
        yield break;
    }

    public void setupLeftAnkleJoint(GameObject parent, AvatarGo avatarVR)
    {
        // Create left ankle joint
        jointAnkleLeft = new GameObject();
        jointAnkleLeft.transform.SetParent(parent.transform, false);
        jointAnkleLeft.transform.localScale = Vector3.one;
        jointAnkleLeft.name = "jointAnkleLeft";

        // Transform left ankle joint
        Quaternion inverseStart = Quaternion.Inverse(parent.transform.rotation);
        jointAnkleLeft.transform.localRotation = inverseStart;
        jointAnkleLeft.transform.localPosition = jointAnkleLeft.transform.parent.worldToLocalMatrix * new Vector4(0.0f, 0.0f, -bodyMeasures.footToAnkleLeft, 0.0f);   // forward
    }

    public void setupRightAnkleJoint(GameObject parent, AvatarGo avatarVR)
    {
        // Create right ankle joint
        jointAnkleRight = new GameObject();
        jointAnkleRight.transform.SetParent(parent.transform, false);
        jointAnkleRight.transform.localScale = Vector3.one;
        jointAnkleRight.name = "jointAnkleRight";

        // Transform right ankle joint
        Quaternion inverseStart = Quaternion.Inverse(parent.transform.rotation);
        jointAnkleRight.transform.localRotation = inverseStart;
        jointAnkleRight.transform.localPosition = jointAnkleRight.transform.parent.worldToLocalMatrix * new Vector4(0.0f, 0.0f, -bodyMeasures.footToAnkleRight, 0.0f);   // forward
    }

    public void setupRootJoint(GameObject parent, Vector3 pelvisPosition, Quaternion pelvisRotation, AvatarGo avatar)
    {
        // Create root joint
        jointRoot = new GameObject();
        jointRoot.transform.SetParent(parent.transform, false);
        jointRoot.transform.localScale = Vector3.one;
        jointRoot.name = "jointRoot";

        // Transform root joint
        Quaternion inverseStart = Quaternion.Inverse(pelvisRotation);
        jointRoot.transform.localRotation = inverseStart;
        // tracker -> hips
        Vector3 offset = avatar.queryJointPositionAvatar(HumanBodyBones.Hips) - pelvisPosition;
        jointRoot.transform.localPosition = jointRoot.transform.parent.worldToLocalMatrix * new Vector4(offset.x, offset.y, offset.z, 0.0f);
        // jointRoot.transform.localPosition = Vector3.zero;
    }

    public void correctLeftAnkleOffset(AvatarGo avatar, Vector3 leftFootPosition, Quaternion leftFootRotation)
    {
        // tracker -> leftFoot
        Vector3 offset = avatar.queryJointPositionAvatar(HumanBodyBones.LeftFoot) - leftFootPosition;
        Vector3 offsetLocal = jointAnkleLeft.transform.parent.worldToLocalMatrix * new Vector4(offset.x, offset.y, offset.z, 0.0f);
        jointAnkleLeft.transform.localPosition = offsetLocal;
        // jointAnkleLeft.transform.localPosition = jointAnkleLeft.transform.parent.worldToLocalMatrix * new Vector4(0.0f, 0.0f, -0.20f, 0.0f); ;
    }

    public void correctRightAnkleOffset(AvatarGo avatar, Vector3 rightFootPosition, Quaternion rightFootRotation)
    {
        // tracker -> rightFoot
        Vector3 offset = avatar.queryJointPositionAvatar(HumanBodyBones.RightFoot) - rightFootPosition;
        Vector3 offsetLocal = jointAnkleRight.transform.parent.worldToLocalMatrix * new Vector4(offset.x, offset.y, offset.z, 0.0f);
        jointAnkleRight.transform.localPosition = offsetLocal;
        // jointAnkleRight.transform.localPosition = jointAnkleRight.transform.parent.worldToLocalMatrix * new Vector4(0.0f, 0.0f, -0.20f, 0.0f); ;
    }

    public void setupLeftWristJoint(GameObject parent, AvatarGo avatarVR)
    {
        // Create left wrist joint
        jointWristLeft = new GameObject();
        jointWristLeft.transform.SetParent(parent.transform);
        jointWristLeft.name = "jointWristLeft";

        // Transform left wrist joint
        jointWristLeft.transform.localRotation = Quaternion.identity;
        jointWristLeft.transform.localPosition = -0.175f * Vector3.forward;
    }

    public void setupRightWristJoint(GameObject parent, AvatarGo avatarVR)
    {
        // Create right wrist joint
        jointWristRight = new GameObject();
        jointWristRight.transform.SetParent(parent.transform);
        jointWristRight.name = "jointWristRight";

        // Transform right wrist joint
        jointWristRight.transform.localRotation = Quaternion.identity;
        jointWristRight.transform.localPosition = -0.175f * Vector3.forward;
    }

    public void setupNeckJoint(GameObject parent, AvatarGo avatarVR)
    {
        // Create neck joint
        jointNeck = new GameObject();
        jointNeck.transform.SetParent(parent.transform);
        jointNeck.name = "jointNeck";

        // Transform neck joint
        jointNeck.transform.localRotation = Quaternion.identity;
        jointNeck.transform.localPosition = Vector3.zero;
    }

    private void saveRootStepData(AvatarDriver driver)
    {
        bodyMeasures.pelvisRootStep = driver.pelvis.transform.position;
        bodyMeasures.pelvisRotRootStep = driver.pelvis.transform.rotation;
        bodyMeasures.leftFootRootStep = driver.footLeft.transform.position;
        bodyMeasures.leftFootRotRootStep = driver.footLeft.transform.rotation;
        bodyMeasures.rightFootRootStep = driver.footRight.transform.position;
        bodyMeasures.rightFootRotRootStep = driver.footRight.transform.rotation;
        bodyMeasures.headRootStep = driver.head.transform.position;
        bodyMeasures.headRotRootStep = driver.head.transform.rotation;
        bodyMeasures.leftHandRootStep = driver.handLeft.transform.position;
        bodyMeasures.leftHandRotRootStep = driver.handLeft.transform.rotation;
        bodyMeasures.rightHandRootStep = driver.handRight.transform.position;
        bodyMeasures.rightHandRotRootStep = driver.handRight.transform.rotation;
    }

    // Completion flag

    public bool isCompleted()
    {
        return completed;
    }

    public void setCompleted(bool flag)
    {
        completed = flag;
    }

    // Start flag

    public bool isStarted()
    {
        return started;
    }

    public void setStarted(bool flag)
    {
        started = flag;
    }

    public void computeFeetMeasuresUsingRoot(AvatarGo avatarVR, Vector3 footLeft, Vector3 footRight, Vector3 pelvis)
    {
        AvatarDriver driver = avatarVR.driver;
        if (driver.footLeft && driver.footLeft.activeInHierarchy && driver.pelvis && driver.pelvis.activeInHierarchy)
        {
            //assuming Z is Forward. otherwise compute both Z projection and X projection and take the bigger one (other is the sideways offset from the bodycenter)
            bodyMeasures.footToAnkleLeft = Math.Abs(footLeft.z - pelvis.z);
            setupLeftAnkleJoint(driver.footLeft, avatarVR);
        }
        if (driver.footRight && driver.footRight.activeInHierarchy && driver.pelvis && driver.pelvis.activeInHierarchy)
        {
            bodyMeasures.footToAnkleRight = Math.Abs(footRight.z - pelvis.z);
            setupRightAnkleJoint(driver.footRight, avatarVR);
        }
    }
}