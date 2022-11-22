using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AvatarController_UnityIK : AvatarController
{
    public AvatarGo AvatarVR;
    public Animator Animator;
    public AvatarProperties AvatarProperties;

    public bool CalibrationFinished
    {
        get { return Flag >= 2; }
    }

    private bool ControllersAttached = false;
    private bool ClampControllers = false;
    private bool ArmsStretchFlag = false;

    private int Flag = 0;

    private Quaternion InitialWorldNeckRotation;
    private Quaternion InitialWorldSpineRotation;
    private Quaternion InitialLocalHipsRotation;

    public Vector3 ReferenceVector;

    public HumanDescription Desc;
    private Vector3 HipsOffset;

    private Quaternion LastLeftHandWorldRotation;
    private Quaternion LastRightHandWorldRotation;

    private ArmsStretch ArmsStretch;

    // Fingers
    private BendFingersType FingersType;
    private float UniformBendT;
    private ControllerFingers LeftController, RightController;
    private List<Quaternion> FingersOpen = new List<Quaternion>();
    private List<Quaternion> FingersClosed = new List<Quaternion>();
    private List<Quaternion> FingersClosedObjects = new List<Quaternion>();
    private List<Transform> Fingers = new List<Transform>();
    private List<float> FingersLengths = new List<float>();
    private List<float> FingersTs = new List<float>();
    private float RightThumbX, LeftThumbX; // [0, 1]

    void Awake()
    {
        Animator = GetComponent<Animator>();
        AvatarProperties = GetComponent<AvatarProperties>();
    }

    void Start()
    {
        InitialWorldNeckRotation = Animator.GetBoneTransform(HumanBodyBones.Head).rotation;
        InitialWorldSpineRotation = Animator.GetBoneTransform(HumanBodyBones.Spine).rotation;
        InitialLocalHipsRotation = Animator.GetBoneTransform(HumanBodyBones.Hips).localRotation;
        InitFingers();
    }

    void OnAnimatorIK()
    {
        if (!Animator || !AvatarProperties)
        {
            return;
        }

        if (!ikActive)
        {
            return;
        }

        Vector3 headPos = driver.head.transform.position;
        Quaternion headRot = driver.head.transform.rotation;
        Vector3 headForward = driver.head.transform.forward;
        Vector3 headRight = driver.head.transform.right;

        if (body == null || body.jointWristLeft == null || body.jointWristRight == null ||
            body.jointRoot == null || body.jointAnkleLeft == null || body.jointAnkleRight == null
            || body.jointNeck == null)
        {
            return;
        }

        // WristLeft        
        Vector3 jointWristLeftPos = body.jointWristLeft.transform.position;
        Quaternion jointWristLeftRot = body.jointWristLeft.transform.rotation;
        // WristRight
        Vector3 jointWristRightPos = body.jointWristRight.transform.position;
        Quaternion jointWristRightRot = body.jointWristRight.transform.rotation;
        // Root
        Vector3 jointRootPos = body.jointRoot.transform.position;
        Quaternion jointRootRot = body.jointRoot.transform.rotation;
        // AnkleLeft
        Vector3 jointAnkleLeftPos = body.jointAnkleLeft.transform.position;
        Quaternion jointAnkleLeftRot = body.jointAnkleLeft.transform.rotation;
        // AnkleRight
        Vector3 jointAnkleRightPos = body.jointAnkleRight.transform.position;
        Quaternion jointAnkleRightRot = body.jointAnkleRight.transform.rotation;
        // Head
        Vector3 jointNeckPos = body.jointNeck.transform.position;
        Quaternion jointNeckRot = body.jointNeck.transform.rotation;

        if (Flag == 0) // Init
        {
            // Reference Vector to compute vector from spine to neck
            // Assuming T-POSE
            Vector3 centerHead = headPos + headForward * body.bodyMeasures.depthCenterHead + headRight * body.bodyMeasures.widthCenterHead;
            ReferenceVector = Quaternion.Inverse(Animator.bodyRotation) * (centerHead - AvatarVR.driver.pelvis.transform.position);
            ReferenceVector.Normalize();

            Flag = 1;
            return;
        }

        HipsOffset = jointRootPos - Animator.GetBoneTransform(HumanBodyBones.Hips).position;

        // Root end-effector - NOT REALLY AN END-EFFECTOR, INSTEAD WE CHANGE THE OVERALL BODY TRANSFORM
        if (body.jointRoot != null)
        {
            Transform hipsTransform = Animator.GetBoneTransform(HumanBodyBones.Hips);
            Animator.SetBoneLocalRotation(HumanBodyBones.Hips, (Quaternion.Inverse(hipsTransform.parent.rotation) * jointRootRot) * InitialLocalHipsRotation);
            //animator.bodyRotation = body.jointRoot.transform.rotation;
        }

        // Neck end-effector - NO END-EFFECTOR AVAILABLE -> WE DO THIS MANUALLY
        if (body.jointNeck != null)
        {
            // Spine
            Quaternion bodyRotation = jointRootRot;
            Vector3 centerHead = headPos + headForward * body.bodyMeasures.depthCenterHead + headRight * body.bodyMeasures.widthCenterHead;
            Vector3 rootToNeck = Quaternion.Inverse(bodyRotation) * (centerHead - AvatarVR.driver.pelvis.transform.position);
            rootToNeck.Normalize();

            Quaternion newSpineWorldRot = Quaternion.FromToRotation(ReferenceVector, rootToNeck) * InitialWorldSpineRotation;
            Animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Inverse(Animator.GetBoneTransform(HumanBodyBones.Spine).transform.parent.rotation) * newSpineWorldRot);

            // Neck
            // First add the headset rotation to the neck in world space
            Quaternion newNeckWorldRot = jointNeckRot * InitialWorldNeckRotation; // QResult = Q2 * Q1 // Q1 rotation is applied first, then Q2
            // as required by Unity, convert world space to local space by multiplying the inverse of the neck's parent rotation
            Animator.SetBoneLocalRotation(HumanBodyBones.Head, Quaternion.Inverse(bodyRotation * Animator.GetBoneTransform(HumanBodyBones.Head).transform.parent.rotation) * newNeckWorldRot);
        }

        // LeftHand end-effector
        if (body.jointWristLeft != null)
        {
            // Fake the final position and rotation of the ik to compute the LeftGrabHand position
            Transform leftHand = Animator.GetBoneTransform(HumanBodyBones.LeftHand);
            leftHand.rotation = LastLeftHandWorldRotation;
            leftHand.position = jointWristLeftPos - HipsOffset;

            Animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
            Animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
            Animator.SetIKPosition(AvatarIKGoal.LeftHand, ((jointWristLeftPos - HipsOffset) + ((jointWristLeftPos - HipsOffset) - AvatarProperties.GetLeftGrabHandWorldPosition())));
            Animator.SetIKRotation(AvatarIKGoal.LeftHand, jointWristLeftRot);
        }

        // RightHand end-effector
        if (body.jointWristRight != null)
        {
            // Fake the final position and rotation of the ik to compute the RightGrabHand position
            Transform rightHand = Animator.GetBoneTransform(HumanBodyBones.RightHand);
            rightHand.rotation = LastRightHandWorldRotation;
            rightHand.position = jointWristRightPos - HipsOffset;

            Animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
            Animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
            Animator.SetIKPosition(AvatarIKGoal.RightHand, ((jointWristRightPos - HipsOffset) + ((jointWristRightPos - HipsOffset) - AvatarProperties.GetRightGrabHandWorldPosition())));
            Animator.SetIKRotation(AvatarIKGoal.RightHand, jointWristRightRot);
        }

        // LeftFoot end-effector
        if (body.jointAnkleLeft != null)
        {
            Animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
            Animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1);
            Animator.SetIKPosition(AvatarIKGoal.LeftFoot, jointAnkleLeftPos - HipsOffset);
            Animator.SetIKRotation(AvatarIKGoal.LeftFoot, jointAnkleLeftRot);
        }

        // RightFoot end-effector
        if (body.jointAnkleRight != null)
        {
            Animator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
            Animator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1);
            Animator.SetIKPosition(AvatarIKGoal.RightFoot, jointAnkleRightPos - HipsOffset);
            Animator.SetIKRotation(AvatarIKGoal.RightFoot, jointAnkleRightRot);
        }
    }

    private void LateUpdate()
    {
        if (Flag == 0 || Flag == 1)
        {
            if (Flag == 1) Flag = 2;
            return;
        }

        Vector3 jointRootPos = body.jointRoot.transform.position;

        // Root end-effector
        if (body.jointRoot != null)
        {
            // Hips
            Animator.GetBoneTransform(HumanBodyBones.Hips).position = jointRootPos;
        }

        if (ControllersAttached)
        {
            // Right Hand
            Animator.GetBoneTransform(HumanBodyBones.RightHand).localRotation = Animator.GetBoneTransform(HumanBodyBones.RightHand).localRotation * Quaternion.Euler(AvatarProperties.RightHandRotation);
            // Left Hand
            Animator.GetBoneTransform(HumanBodyBones.LeftHand).localRotation = Animator.GetBoneTransform(HumanBodyBones.LeftHand).localRotation * Quaternion.Euler(AvatarProperties.LeftHandRotation);
        }

        // Arm Stretch
        if (ArmsStretchFlag)
        {
            ArmsStretch.DoUpdate();
        }

        // Fingers
        if (FingersType == BendFingersType.Automatic)
        {
            // Right Fingers
            FingersGradientDescent(0, Fingers.Count / 2, RightController);
            // Left Fingers
            FingersGradientDescent(Fingers.Count / 2, Fingers.Count, LeftController);
        }
        else if (FingersType == BendFingersType.Uniform)
        {
            for (int i = 0; i < Fingers.Count; i++)
            {
                Fingers[i].localRotation = Quaternion.Slerp(FingersOpen[i], FingersClosed[i], UniformBendT);
            }
        }
        else if (FingersType == BendFingersType.Open)
        {
            for (int i = 0; i < Fingers.Count; i++)
            {
                Fingers[i].localRotation = FingersOpen[i];
            }
        }
        else if (FingersType == BendFingersType.Objects)
        {
            // Right Fingers
            FingersObjectsGradientDescent(0, Fingers.Count / 2, false);
            // Left Fingers
            FingersObjectsGradientDescent(Fingers.Count / 2, Fingers.Count, true);
        }

        LastLeftHandWorldRotation = Animator.GetBoneTransform(HumanBodyBones.LeftHand).rotation;
        LastRightHandWorldRotation = Animator.GetBoneTransform(HumanBodyBones.RightHand).rotation;

        if (ClampControllers)
        {
            if (AvatarVR.handLeft_model != null) AvatarVR.handLeft_model.transform.position = AvatarProperties.GetLeftGrabHandWorldPosition() - AvatarVR.handLeft_model.transform.parent.TransformDirection(-0.1f * Vector3.forward);
            if (AvatarVR.handRight_model != null) AvatarVR.handRight_model.transform.position = AvatarProperties.GetRightGrabHandWorldPosition() - AvatarVR.handRight_model.transform.parent.TransformDirection(-0.1f * Vector3.forward);
        }
        else
        {
            if (AvatarVR.handLeft_model != null) AvatarVR.handLeft_model.transform.localPosition = Vector3.zero;
            if (AvatarVR.handRight_model != null) AvatarVR.handRight_model.transform.localPosition = Vector3.zero;
        }
    }

    public void SetArmStretch(ArmsStretch armsStretch)
    {
        ArmsStretch = armsStretch;
    }

    public void SetArmStretch(bool e)
    {
        ArmsStretchFlag = e;
    }

    public void SetControllersAttached(bool e)
    {
        ControllersAttached = e;
    }

    public void SetClampControllers(bool e)
    {
        ClampControllers = e;
    }

    public void SetAutomaticFingers(ControllerFingers rightController, ControllerFingers leftController)
    {
        FingersType = BendFingersType.Automatic;
        RightController = rightController;
        LeftController = leftController;
    }

    public void SetUniformFingers(float t)
    {
        Debug.Assert(t >= 0 && t <= 1, "t must be between 0 and 1");
        UniformBendT = t;
        FingersType = BendFingersType.Uniform;
    }

    public void SetObjectsFingers()
    {
        FingersType = BendFingersType.Objects;
    }

    public void SetOpenFingers()
    {
        FingersType = BendFingersType.Open;
    }

    private void InitFingers()
    {
        const float maxAngle = 110;
        const float maxAngleThumb = 10;
        if (Animator == null) Animator = GetComponent<Animator>();
        // Right Fingers
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightIndexProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightIndexDistal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightMiddleProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightLittleProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightLittleDistal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightRingProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightRingDistal));
        for (int i = 0; i < Fingers.Count; i++)
        {
            FingersOpen.Add(Fingers[i].localRotation);
            FingersClosed.Add(Fingers[i].localRotation * Quaternion.Euler(maxAngle, 0, 0));
            FingersClosedObjects.Add(FingersClosed[FingersClosed.Count - 1]);
        }
        int thumbIndex = Fingers.Count;
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightThumbProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.RightThumbDistal));
        for (int i = thumbIndex; i < Fingers.Count; i++)
        {
            FingersOpen.Add(Fingers[i].localRotation);
            FingersClosed.Add(Fingers[i].localRotation * Quaternion.Euler(0, 0, -maxAngleThumb));
            FingersClosedObjects.Add(Fingers[i].localRotation * Quaternion.Euler(maxAngle, 0, 0));
        }
        // Left Fingers
        int leftIndex = Fingers.Count;
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftIndexProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftMiddleProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftLittleProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftRingProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftRingDistal));
        for (int i = leftIndex; i < Fingers.Count; i++)
        {
            FingersOpen.Add(Fingers[i].localRotation);
            FingersClosed.Add(Fingers[i].localRotation * Quaternion.Euler(maxAngle, 0, 0));
            FingersClosedObjects.Add(FingersClosed[FingersClosed.Count - 1]);
        }
        thumbIndex = Fingers.Count;
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftThumbProximal));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate));
        Fingers.Add(Animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal));
        for (int i = thumbIndex; i < Fingers.Count; i++)
        {
            FingersOpen.Add(Fingers[i].localRotation);
            FingersClosed.Add(Fingers[i].localRotation * Quaternion.Euler(0, 0, maxAngleThumb));
            FingersClosedObjects.Add(Fingers[i].localRotation * Quaternion.Euler(maxAngle, 0, 0));
        }

        // Lengths
        for (int f = 0; f < Fingers.Count; f += 3)
        {
            FingersLengths.Add((Fingers[f].position - Fingers[f + 1].position).magnitude +
                               (Fingers[f + 1].position - Fingers[f + 2].position).magnitude +
                               ((Fingers[f + 2].position - Fingers[f + 1].position) * AvatarProperties.LastFingerJointOffset).magnitude);
        }

        // Ts
        for (int f = 0; f < Fingers.Count; f++)
        {
            FingersTs.Add(0.0f);
        }
    }

    private void FingersGradientDescent(int startIndex, int endIndex, ControllerFingers controller)
    {
        const float samplingDistance = 0.02f;
        const float learningRate = 0.1f;
        const float maxThumbXAngle = 10.0f;
        for (int f = startIndex; f < endIndex; f += 3)
        {
            bool isThumb = f - startIndex >= 12;
            // Find rotations with minimum angle
            for (int i = 0; i < 3; i++)
            {
                Transform finger = Fingers[f + i];
                finger.localRotation = Quaternion.SlerpUnclamped(FingersOpen[f + i], FingersClosed[f + i], FingersTs[f + i]);
                float d1 = DistanceTarget(f, controller, isThumb);
                // if (d1 < 0.001f) break;
                finger.localRotation = Quaternion.SlerpUnclamped(FingersOpen[f + i], FingersClosed[f + i], FingersTs[f + i] + samplingDistance);
                float d2 = DistanceTarget(f, controller, isThumb);
                float gradient = (d2 - d1) / samplingDistance;
                // Gradient descent
                if (isThumb)
                {
                    // Thumb
                    FingersTs[f + i] = Mathf.Clamp(FingersTs[f + i] - learningRate * gradient, -1.0f, 1.0f);
                }
                else
                {
                    FingersTs[f + i] = Mathf.Clamp01(FingersTs[f + i] - learningRate * gradient);
                }
                finger.localRotation = Quaternion.SlerpUnclamped(FingersOpen[f + i], FingersClosed[f + i], FingersTs[f + i]);

                // Thumb x angle
                if (isThumb && i == 0)
                {
                    bool right = f == 12;
                    Quaternion initRot = finger.localRotation;
                    finger.localRotation = initRot * Quaternion.Euler(maxThumbXAngle * (right ? RightThumbX : LeftThumbX), 0.0f, 0.0f);
                    d1 = DistancePlaneThumb(f, controller);
                    if (d1 > 0.001f)
                    {
                        finger.localRotation = initRot * Quaternion.Euler(maxThumbXAngle * ((right ? RightThumbX : LeftThumbX) + samplingDistance), 0.0f, 0.0f);
                        d2 = DistancePlaneThumb(f, controller);
                        gradient = (d2 - d1) / samplingDistance;
                        if (right)
                        {
                            RightThumbX = Mathf.Clamp01(RightThumbX - learningRate * gradient);
                        }
                        else
                        {
                            LeftThumbX = Mathf.Clamp01(LeftThumbX - learningRate * gradient);
                        }
                        finger.localRotation = initRot * Quaternion.Euler(maxThumbXAngle * (right ? RightThumbX : LeftThumbX), 0.0f, 0.0f);
                    }
                }

                // Early termination
                if (DistanceTarget(f, controller, isThumb) < 0.001f) break;
            }
        }
    }

    private Collider[] OverlapColliders = new Collider[16];
    private void FingersObjectsGradientDescent(int startIndex, int endIndex, bool isLeft)
    {
        const float samplingDistance = 0.02f;
        const float learningRate = 0.005f;
        Vector3 palm = isLeft ? AvatarProperties.GetLeftGrabHandWorldPosition() : AvatarProperties.GetRightGrabHandWorldPosition();
        const float radius = 0.005f;
        int nColliders = Physics.OverlapSphereNonAlloc(palm, radius, OverlapColliders);
        if (nColliders == 0)
        {
            for (int f = startIndex; f < endIndex; f += 3)
            {
                bool isThumb = f - startIndex >= 12; 
                for (int i = 0; i < 3; i++)
                {
                    Transform finger = Fingers[f + i]; 
                    if (isThumb)
                    {
                        // Thumb
                        if (FingersTs[f + i] > 0.1f)
                        {   
                            FingersTs[f + i] = Mathf.Clamp(FingersTs[f + i] - learningRate * 0.25f, 0.1f, 1.0f);
                        }
                        else
                        {
                            FingersTs[f + i] = Mathf.Clamp(FingersTs[f + i] + learningRate * 0.25f, -0.9f, 0.1f);
                        }
                    }
                    else
                    {
                        FingersTs[f + i] = Mathf.Clamp(FingersTs[f + i] - learningRate * 0.25f, 0.1f, 1.0f);
                    }
                    finger.localRotation = Quaternion.SlerpUnclamped(FingersOpen[f + i], FingersClosed[f + i], FingersTs[f + i]);
                }
            }
            return;
        }
        ISDF sdf = OverlapColliders[0].GetComponentInChildren<ISDF>();
        for (int f = startIndex; f < endIndex; f += 3)
        {
            bool isThumb = f - startIndex >= 12;
            // Find rotations with minimum angle
            for (int i = 0; i < 3; i++)
            {
                Transform finger = Fingers[f + i];
                finger.localRotation = Quaternion.SlerpUnclamped(FingersOpen[f + i], FingersClosed[f + i], FingersTs[f + i]);
                float d1 = DistanceObjectTarget(f, sdf);
                finger.localRotation = Quaternion.SlerpUnclamped(FingersOpen[f + i], FingersClosed[f + i], FingersTs[f + i] + samplingDistance);
                float d2 = DistanceObjectTarget(f, sdf);
                float gradient = (d2 - d1) / samplingDistance;
                // Gradient descent
                if (isThumb)
                {
                    // Thumb
                    FingersTs[f + i] = Mathf.Clamp(FingersTs[f + i] - learningRate * gradient * 20.0f, -1.0f, 1.0f);
                }
                else
                {
                    FingersTs[f + i] = Mathf.Clamp(FingersTs[f + i] - learningRate * gradient, 0.0f, 0.75f);
                }
                finger.localRotation = Quaternion.SlerpUnclamped(FingersOpen[f + i], FingersClosed[f + i], FingersTs[f + i]);

                // Early termination
                if (DistanceObjectTarget(f, sdf) < 0.001f) break;
            }
        }
    }

    private float DistanceTarget(int f, ControllerFingers controller, bool isThumb)
    {
        float negativePenalization = 2.0f; // avoid fingers inside the controller
        Transform joint1 = Fingers[f + 1];
        Transform joint2 = Fingers[f + 2];
        Vector3 joint3 = joint2.position + joint2.up * (joint2.position - joint1.position).magnitude * AvatarProperties.LastFingerJointOffset;
        float d1 = controller.GetControllerSDF(joint1.position);
        float d2 = controller.GetControllerSDF(joint2.position);
        float d3 = controller.GetControllerSDF(joint3);
        // negative part of separator is the good part of the space
        float separator1 = isThumb ? 0.0f : controller.GetSDFSeparatorPlane(joint1.position);
        if (separator1 < 0.0f) separator1 = 0.0f;
        float separator2 = isThumb ? 0.0f : controller.GetSDFSeparatorPlane(joint2.position);
        if (separator2 < 0.0f) separator2 = 0.0f;
        float separator3 = isThumb ? 0.0f : controller.GetSDFSeparatorPlane(joint3);
        if (separator3 < 0.0f) separator3 = 0.0f;
        return (d1 < 0 ? d1 * -negativePenalization : d1) + separator1 +
               (d2 < 0 ? d2 * -negativePenalization : d2) + separator2 +
               (d3 < 0 ? d3 * -negativePenalization : d3) + separator3;
    }

    private float DistanceObjectTarget(int f, ISDF sdf)
    {
        float negativePenalization = 2.0f; // avoid fingers inside the controller
        Transform joint1 = Fingers[f + 1];
        Transform joint2 = Fingers[f + 2];
        Vector3 joint3 = joint2.position + joint2.up * (joint2.position - joint1.position).magnitude * AvatarProperties.LastFingerJointOffset;
        float d1 = sdf.Distance(joint1.position);
        float d2 = sdf.Distance(joint2.position);
        float d3 = sdf.Distance(joint3);
        return (d1 < 0 ? d1 * -negativePenalization : d1) +
               (d2 < 0 ? d2 * -negativePenalization : d2) +
               (d3 < 0 ? d3 * -negativePenalization : d3);
    }

    private float DistancePlaneThumb(int f, ControllerFingers controller)
    {
        Transform joint1 = Fingers[f + 1];
        Transform joint2 = Fingers[f + 2];
        Vector3 joint3 = joint2.position + joint2.up * (joint2.position - joint1.position).magnitude * AvatarProperties.LastFingerJointOffset;
        return controller.GetThumbPlaneDistance(joint1.position) + controller.GetThumbPlaneDistance(joint2.position) + controller.GetThumbPlaneDistance(joint3);
    }

    public enum BendFingersType
    {
        Open,
        Uniform,
        Automatic,
        Objects
    }
}
