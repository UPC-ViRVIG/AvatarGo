using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AvatarUtils
{
    // Imports a character from the Resources folder
    public static bool createCharacter(GameObject avatarPrefab, string controllerFile, AvatarBody body,
        out GameObject character, ref HumanDescription description, GameObject floorMarker, AvatarGo avatarVR)
    {
        // Import a FBX file from resources
        character = GameObject.Instantiate(avatarPrefab) as GameObject;
        if (!character)
        {
            Debug.LogError("File " + avatarPrefab.name + " not loaded correctly. Cannot make the character.");
            return false;
        }
        AvatarProperties avatarProperties = character.GetComponent<AvatarProperties>();
        if (avatarProperties == null)
        {
            Debug.LogError("File " + avatarPrefab.name + " does not contain an AvatarProperties component. Cannot make the character.");
            return false;
        }

        // Reset character position
        character.transform.position = Vector3.zero;

        // Add an animator component
        Animator animator;
        if (character.GetComponent<Animator>())
        {
            animator = character.GetComponent<Animator>();
        }
        else
        {
            animator = character.AddComponent<Animator>();
        }
        if (floorMarker != null)
        {
            Transform leftToesTransform = animator.GetBoneTransform(HumanBodyBones.LeftToes);
            if (leftToesTransform == null)
            {
                Debug.LogError("File " + avatarPrefab.name + " does not contain a LeftToes bone. Cannot make the character.");
                return false;
            }
            floorMarker.transform.SetParent(leftToesTransform, false);
            floorMarker.transform.position = character.transform.position;
        }
        // Set up the description for the humanoid
        bool success = setupHumanDescription(character, body, ref description, avatarVR, avatarProperties);
        if (!success)
        {
            Debug.LogError("Cannot create HumanDescription from " + avatarPrefab.name);
            return false;
        }

        // Tell Unity not to modify the positions of the joints
        description.upperArmTwist = 0.5f;
        description.lowerArmTwist = 0.5f;
        description.upperLegTwist = 0.5f;
        description.lowerLegTwist = 0.5f;
        description.armStretch = 0.0f;
        description.legStretch = 0.0f;
        description.feetSpacing = 0.0f;
        description.hasTranslationDoF = false;

        // Create the avatar using the GameObject and the HumanDescription
        Avatar a = AvatarBuilder.BuildHumanAvatar(character, description);
        if (!a)
        {
            Debug.LogError("Cannot build Avatar from " + avatarPrefab.name);
            return false;
        }

        // Set the avatar in the animator component
        animator.avatar = a;

        // Load the animator controller from resources if specified
        if (controllerFile != "")
        {
            RuntimeAnimatorController c = (RuntimeAnimatorController)RuntimeAnimatorController.Instantiate(Resources.Load(controllerFile));
            if (!c)
            {
                Debug.LogError("File " + controllerFile + " not loaded in from resources correctly. Cannot add animation controller.");
                return false;
            }

            // Set the controller in the animator component
            animator.runtimeAnimatorController = c;
        }

        return true;
    }

    /***********************************************************************************************************************/

    // Creates a HumanDescription given the parameters for your character
    private static bool setupHumanDescription(GameObject character, AvatarBody body,
                                              ref HumanDescription desc,
                                              AvatarGo avatarVR, AvatarProperties avatarProperties)
    {
        // Load in the bone mapping from file
        List<HumanBone> human = new List<HumanBone>();
        List<SkeletonBone> skeleton = new List<SkeletonBone>();
        if (!loadSkeleton(avatarProperties, human, skeleton))
        {
            return false;
        }

        // The human bone array is the list we've already composed
        desc.human = human.ToArray();

        // Apply overall characted scale (model/mesh, not skeleton)
        float avatarScaleY = 1.0f;

        // Uniform scale
        float avatarEye = avatarProperties.GetEyesHeight().y;
        avatarScaleY = body.bodyMeasures.eyesHeight / avatarEye;

        // Uniform scale
        character.transform.localScale = new Vector3(avatarScaleY, avatarScaleY, avatarScaleY);

        // Skeleton bones - this is where the pose transform is stored
        SkeletonBone[] sk = skeleton.ToArray();

        // For all the bones in the skeleton
        for (int i = 0; i < sk.Length; ++i)
        {
            // Get the default transform that comes from the FBX file
            Transform defaultTransform = Utils.FindDescendants(character.transform, sk[i].name);
            if (!defaultTransform)
            {
                Debug.Log("Did not find default bone transform " + sk[i].name + " in hierarchy. Defaulting to empty transform.");
            }

            sk[i].name = defaultTransform.name;
            sk[i].position = defaultTransform.localPosition;
            sk[i].rotation = defaultTransform.localRotation;
            sk[i].scale = Vector3.one;
        }

        // Set the skeleton definition
        desc.skeleton = sk;

        // Return
        return true;
    }

    private static bool loadSkeleton(AvatarProperties properties, List<HumanBone> human, List<SkeletonBone> skeleton)
    {
        // Add hips bone parent
        SkeletonBone sbParentHips = new SkeletonBone();
        sbParentHips.name = properties.Root.parent.name;
        skeleton.Add(sbParentHips);
        HumanDescription description = properties.DefaultAvatar.humanDescription;
        foreach (HumanBone hb in description.human)
        {
            HumanBone hb2 = new HumanBone();
            hb2.boneName = hb.boneName;
            hb2.humanName = hb.humanName;
            hb2.limit.useDefaultValues = true;
            human.Add(hb2);
            SkeletonBone sb = new SkeletonBone();
            sb.name = hb.boneName;
            skeleton.Add(sb);
        }
        return true;
    }
}