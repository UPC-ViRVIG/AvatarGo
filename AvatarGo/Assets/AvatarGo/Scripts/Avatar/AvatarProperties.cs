using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(Animator))]
public class AvatarProperties : MonoBehaviour
{
    [Header("General")]
    public float EyesHeight;
    public Avatar DefaultAvatar;
    [Tooltip("Root Bone of the Avatar's skeleton")]
    public Transform Root;
    [Header("Head Clipping")]
    [Tooltip("Center of the head used by the clipping shader.")]
    public Vector3 HeadCenter = new Vector3(0.0f, 1.65f, 0.0f);
    [Tooltip("Radius of the head used by the clipping shader.")]
    public float HeadRadius = 0.2f;

    [Header("Closed Hand & Finger IK")]
    [Tooltip("Default rotation of the left hand to apply when applying Finger IK. Default values should work for most avatars. Recommendation: adjust with the Simulator in Play Mode (so changes can be seen in real time)")]
    public Vector3 LeftHandRotation = new Vector3(-2.033f, 79.846f, 60.0f);
    [Tooltip("Default rotation of the right hand to apply when applying Finger IK. Default values should work for most avatars. Recommendation: adjust with the Simulator in Play Mode (so changes can be seen in real time)")]
    public Vector3 RightHandRotation = new Vector3(-0.467f, -80.019f, -60.0f);
    [Tooltip("Unity's animation system does not add a bone at the end of each finger. This is needed for the Finger IK. This value controls the position of an artificial last finger bone. Adjust it in the editor with the red spheres (they should be at the end of each finger approximately).")]
    [Range(0.0f, 1.0f)] public float LastFingerJointOffset = 0.65f;
    [Tooltip("Left Hand Palm. Used to position the controller on the left hand.")]
    public Vector3 LeftGrabPosition = new Vector3(0.005f, 0.05f, 0.03f);
    [Tooltip("Right Hand Palm. Used to position the controller on the right hand.")]
    public Vector3 RightGrabPosition = new Vector3(-0.005f, 0.05f, 0.03f);

    private Animator Animator;

    private Camera Camera;
    private List<Material> Materials;
    private int ClippingSpherePositionKey;
    private int ClippingSphereRadiusKey;
    private Transform HeadJoint;

    private void Awake()
    {
        if (DefaultAvatar == null)
        {
            Debug.LogError("DefaultAvatar cannot be null. Mark the model as humanoid and create an Avatar.");
        }
        if (Root == null)
        {
            Debug.LogError("Root cannot be null. Reference the root bone.");
        }

        Camera = Camera.main;

        // Check Materials
        ClippingSpherePositionKey = Shader.PropertyToID("_ClippingSpherePosition");
        ClippingSphereRadiusKey = Shader.PropertyToID("_ClippingSphereRadius");
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        Materials = new List<Material>();
        foreach (Renderer renderer in renderers)
        {
            foreach (Material material in renderer.materials)
            {
                if (material.shader.name.Contains("AvatarShader"))
                {
                    Materials.Add(material);
                }
                else
                {
                    Debug.LogWarning("Material " + material.name + " does not have the required properties to perform head clipping. Make sure to assign the shader 'AvatarShader' or 'AvatarTransparentShader' to the material.");
                }
            }
        }
    }

    private void Update()
    {
        Vector3 headCenter = GetHeadCenter();
        // Materials
        foreach (Material material in Materials)
        {
            material.SetVector(ClippingSpherePositionKey, headCenter);
            material.SetFloat(ClippingSphereRadiusKey, HeadRadius);
        }
    }

    public Vector3 GetHeadCenter()
    {
        if (Animator == null || HeadJoint == null)
        {
            Animator = GetComponent<Animator>();
            HeadJoint = Animator.GetBoneTransform(HumanBodyBones.Head);
            if (HeadJoint == null)
            {
                Debug.LogError("Animator has no reference to the head joint.");
            }
        }
        return HeadJoint.TransformPoint(HeadCenter);
    }

    public Vector3 GetEyesHeight()
    {
        if (Animator == null || HeadJoint == null)
        {
            Animator = GetComponent<Animator>();
            HeadJoint = Animator.GetBoneTransform(HumanBodyBones.Head);
            if (HeadJoint == null)
            {
                Debug.LogError("Animator has no reference to the head joint.");
            }
        }
        return HeadJoint.TransformPoint(new Vector3(0.0f, EyesHeight, 0.0f));
    }

    public Vector3 GetLeftGrabHandWorldPosition()
    {
        if (Animator == null)
        {
            Animator = GetComponent<Animator>();
        }
        Transform leftHand = Animator.GetBoneTransform(HumanBodyBones.LeftHand);
        return leftHand.TransformPoint(LeftGrabPosition);
    }

    public Vector3 GetRightGrabHandWorldPosition()
    {
        if (Animator == null)
        {
            Animator = GetComponent<Animator>();
        }
        Transform rightHand = Animator.GetBoneTransform(HumanBodyBones.RightHand);
        return rightHand.TransformPoint(RightGrabPosition);
    }

    private void OnDestroy()
    {
        if (Materials != null)
        {
            foreach (Material material in Materials)
            {
                Destroy(material);
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(GetHeadCenter(), HeadRadius);
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(AvatarProperties))]
public class AvatarPropertiesEditor : Editor
{
    private Animator Animator;
    private float FingersSlider;

    public override void OnInspectorGUI()
    {
        const int space = 8;

        DrawDefaultInspector();
        EditorGUILayout.Space(space);

        AvatarProperties properties = (AvatarProperties)target;

        if (Animator == null) Animator = properties.GetComponent<Animator>();

        // FingersSlider = EditorGUILayout.Slider("Fingers", FingersSlider, 0, 1);
    }

    private void OnSceneGUI()
    {
        AvatarProperties properties = (AvatarProperties)target;

        if (Animator == null) Animator = properties.GetComponent<Animator>();

        Handles.color = Color.blue;
        Handles.SphereHandleCap(0, properties.GetEyesHeight() + properties.transform.forward * 0.25f, Quaternion.identity, 0.025f, EventType.Repaint);

        // Grab position
        Handles.color = Color.green;
        Handles.SphereHandleCap(0, properties.GetLeftGrabHandWorldPosition(), Quaternion.identity, 0.025f, EventType.Repaint);
        Handles.SphereHandleCap(0, properties.GetRightGrabHandWorldPosition(), Quaternion.identity, 0.025f, EventType.Repaint);

        Transform rightIndexDistal = Animator.GetBoneTransform(HumanBodyBones.RightIndexDistal);
        Transform rightMiddleDistal = Animator.GetBoneTransform(HumanBodyBones.RightMiddleDistal);
        Transform rightLittleDistal = Animator.GetBoneTransform(HumanBodyBones.RightLittleDistal);
        Transform rightRingDistal = Animator.GetBoneTransform(HumanBodyBones.RightRingDistal);
        Transform rightThumbDistal = Animator.GetBoneTransform(HumanBodyBones.RightThumbDistal);

        Transform rightIndexIntermediate = Animator.GetBoneTransform(HumanBodyBones.RightIndexIntermediate);
        Transform rightMiddleIntermediate = Animator.GetBoneTransform(HumanBodyBones.RightMiddleIntermediate);
        Transform rightLittleIntermediate = Animator.GetBoneTransform(HumanBodyBones.RightLittleIntermediate);
        Transform rightRingIntermediate = Animator.GetBoneTransform(HumanBodyBones.RightRingIntermediate);
        Transform rightThumbIntermediate = Animator.GetBoneTransform(HumanBodyBones.RightThumbIntermediate);

        Transform leftIndexDistal = Animator.GetBoneTransform(HumanBodyBones.LeftIndexDistal);
        Transform leftMiddleDistal = Animator.GetBoneTransform(HumanBodyBones.LeftMiddleDistal);
        Transform leftLittleDistal = Animator.GetBoneTransform(HumanBodyBones.LeftLittleDistal);
        Transform leftRingDistal = Animator.GetBoneTransform(HumanBodyBones.LeftRingDistal);
        Transform leftThumbDistal = Animator.GetBoneTransform(HumanBodyBones.LeftThumbDistal);

        Transform leftIndexIntermediate = Animator.GetBoneTransform(HumanBodyBones.LeftIndexIntermediate);
        Transform leftMiddleIntermediate = Animator.GetBoneTransform(HumanBodyBones.LeftMiddleIntermediate);
        Transform leftLittleIntermediate = Animator.GetBoneTransform(HumanBodyBones.LeftLittleIntermediate);
        Transform leftRingIntermediate = Animator.GetBoneTransform(HumanBodyBones.LeftRingIntermediate);
        Transform leftThumbIntermediate = Animator.GetBoneTransform(HumanBodyBones.LeftThumbIntermediate);

        Handles.color = Color.red;
        Handles.SphereHandleCap(0, rightIndexDistal.position + rightIndexDistal.up * (rightIndexDistal.position - rightIndexIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.SphereHandleCap(0, rightMiddleDistal.position + rightMiddleDistal.up * (rightMiddleDistal.position - rightMiddleIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.SphereHandleCap(0, rightLittleDistal.position + rightLittleDistal.up * (rightLittleDistal.position - rightLittleIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.SphereHandleCap(0, rightRingDistal.position + rightRingDistal.up * (rightRingDistal.position - rightRingIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.SphereHandleCap(0, rightThumbDistal.position + rightThumbDistal.up * (rightThumbDistal.position - rightThumbIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);

        Handles.SphereHandleCap(0, leftIndexDistal.position + leftIndexDistal.up * (leftIndexDistal.position - leftIndexIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.SphereHandleCap(0, leftMiddleDistal.position + leftMiddleDistal.up * (leftMiddleDistal.position - leftMiddleIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.SphereHandleCap(0, leftLittleDistal.position + leftLittleDistal.up * (leftLittleDistal.position - leftLittleIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.SphereHandleCap(0, leftRingDistal.position + leftRingDistal.up * (leftRingDistal.position - leftRingIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
        Handles.SphereHandleCap(0, leftThumbDistal.position + leftThumbDistal.up * (leftThumbDistal.position - leftThumbIntermediate.position).magnitude * properties.LastFingerJointOffset, Quaternion.identity, 0.01f, EventType.Repaint);
    }
}
#endif