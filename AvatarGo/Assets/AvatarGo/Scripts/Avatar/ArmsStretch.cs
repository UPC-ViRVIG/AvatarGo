using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsStretch : MonoBehaviour
{
    private Transform RightEndEffector;
    private Transform LeftEndEffector;

    private Animator Animator;
    private Transform RLowerArm;
    private Transform RHand;
    private Transform LLowerArm;
    private Transform LHand;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
        Debug.Assert(Animator != null, "Animator is null");
        RLowerArm = Animator.GetBoneTransform(HumanBodyBones.RightLowerArm);
        RHand = Animator.GetBoneTransform(HumanBodyBones.RightHand);
        LLowerArm = Animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        LHand = Animator.GetBoneTransform(HumanBodyBones.LeftHand);
    }

    public void DoUpdate()
    {
        if (RightEndEffector != null)
        {
            Vector3 dir = (RHand.position - RLowerArm.position).normalized;
            float dist = Vector3.Distance(RHand.position, RightEndEffector.position);
            Debug.Log(dist);
            RLowerArm.position += dir * dist;
        }
        if (LeftEndEffector != null)
        {
            Vector3 dir = (LHand.position - LLowerArm.position).normalized;
            float dist = Vector3.Distance(LHand.position, LeftEndEffector.position);
            LLowerArm.position += dir * dist;
        }
    }

    public void SetRightEndEffector(Transform t)
    {
        RightEndEffector = t;
    }
    public void SetLeftEndEffector(Transform t)
    {
        LeftEndEffector = t;
    }
}
