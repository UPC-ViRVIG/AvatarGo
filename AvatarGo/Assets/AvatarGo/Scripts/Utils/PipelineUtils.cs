using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PipelineUtils
{
    // Stage in the Avatar Setup pipeline

    /*
        Pipeline STAGES:
          - DIRTY   -> Initial stage (This STAGE is NEVER set... it is only used for messaging)
          - DEVICES -> Devices are identified and correctly positioned (DEVICES and T_POSE are done at the same time)
          - T_POSE  -> Some static measurements
          - ROOT_AVATAR    -> Avatar is constructed using Body measurements, then is placed in the scene, finally the user enters inside to compute exact offsets
          - DONE           -> completed = true, the avatar is loaded
    */

    public enum Stage
    {
        DIRTY = -1,
        DEVICES = 0,
        T_POSE = 1,
        ROOT_AVATAR = 10,
        DONE = 11
    }

    // Returns the next stage in the pipeline given the current stage, and present devices
    public static PipelineUtils.Stage nextStage(AvatarDriver driver, PipelineUtils.Stage current)
    {
        if (current == PipelineUtils.Stage.DEVICES)
        {
            return PipelineUtils.Stage.T_POSE;
        }
        if (current == PipelineUtils.Stage.T_POSE)
        {
            return PipelineUtils.Stage.ROOT_AVATAR;
        }
        if (current == PipelineUtils.Stage.ROOT_AVATAR)
        {
            return PipelineUtils.Stage.DONE;
        }
        if (current == PipelineUtils.Stage.DONE)
        {
            return PipelineUtils.Stage.DONE;
        }
        return PipelineUtils.Stage.DONE;
    }

    // Returns message to be shown before stage: i.e. instructions to user
    public static string introMessageAt(Stage current)
    {
        if (current == Stage.DIRTY)
        {
            return "";
        }
        else if (current == Stage.DEVICES)
        {
            return "Setting up device indices and taking some measures... Please, stand on a T-pose. Press TRIGGER when ready!";
        }
        else if (current == Stage.T_POSE)
        {
            return "Taking some measures... Please, stand on a T-pose. Press TRIGGER when ready!";
        }
        else if (current == Stage.ROOT_AVATAR)
        {
            return "\n\n\n\n\nSetting up root... Please, stand on a T-pose inside the avatar shown. Press TRIGGER when ready!";
        }
        else //if (current == Stage.DONE)
        {
            return "Avatar setup completed successfully.";
        }
    }

    // Returns message to be shown after completed stage: i.e. well done messages
    public static string successMessageAt(Stage current)
    {
        if (current == Stage.DIRTY)
        {
            return "";
        }
        else if (current == Stage.DEVICES)
        {
            return "Devices were identified correctly!";

        }
        else if (current == Stage.T_POSE)
        {
            return "Measures were correctly captured!";
        }
        else //if (current == Stage.DONE)
        {
            return "";
        }
    }

    // Returns message to be shown after failed stage: i.e. error messages
    public static string failureMessageAt(Stage current, int code = -1)
    {
        if (current == Stage.DIRTY)
        {
            return "Please, connect more controllers and/or trackers.";
        }
        else if (current == Stage.DEVICES)
        {
            if (code == 0)
            {
                return "Not enough devices! Need at least two controllers and/or trackers.";
            }
            else if (code == 1)
            {
                return "Could not identify tracked objects! Make sure you're standing on a T-pose.";
            }
            else if (code == 2)
            {
                return "Your head is not aligned with the rest of your body! Make sure you're standing on a T-pose.";
            }
            return "";
        }
        else if (current == Stage.T_POSE)
        {
            return "";
        }
        else //if (current == Stage.DONE)
        {
            return "";
        }
    }

    // Returns message to be shown while executing stages: i.e. progress messages
    public static string progressMessageAt(Stage current)
    {
        if (current == Stage.DIRTY)
        {
            return "Found {0} controller(s) and {1} tracker(s).";
        }
        else if (current == Stage.DEVICES)
        {
            return "";
        }
        else if (current == Stage.T_POSE)
        {
            return "Taking some measures... Please, stand on a T-pose.";
        }
        else if (current == Stage.ROOT_AVATAR)
        {
            return "Setting up root... Please, stand on a T-pose inside the avatar shown.";
        }
        else //if (current == Stage.DONE)
        {
            return "";
        }
    }

    // Displays success message followed by intro message for current transition
    public static void displayInBetweenStagesMessage(AvatarGo avatarVR, DisplayMirror displayMirror, PipelineUtils.Stage current, PipelineUtils.Stage next)
    {
        if (displayMirror == null) return;

        string message1 = PipelineUtils.successMessageAt(current);
        Color color1 = new Color(0.0f, 1.0f, 0.0f, 0.5f);
        int secs1 = 2;
        string message2 = PipelineUtils.introMessageAt(next);
        Color color2 = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        int secs2 = 0;
        if (next == PipelineUtils.Stage.DONE)
        {
            displayMirror.CleanText();
            color2 = new Color(0.0f, 1.0f, 0.0f, 0.5f);
            secs2 = 2;
        }
        //Debug.Log(message1 + "\n");
        //Debug.Log(message2 + "\n");
        if (displayMirror && displayMirror.isActiveAndEnabled)
        {
            displayMirror.ShowTextAgain(message1, color1, secs1, message2, color2, secs2, true);
        }
    }
}
