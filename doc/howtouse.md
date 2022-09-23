## Contents
1. [How to Use](#how-to-use)
2. [Hardware](#hardware-setup)
3. [Add new avatars](#add-new-avatars)
4. [Collaborative](#collaborative)

## How to Use
- Take a look at the demo scenes: AvatarGo/Scenes/
- Drop into the scene one of the following prefabs:
  - SimulatorAvatarGo (use this to test the avatars without HMD)
  - SteamVRAvatarGo
- 'AvatarGo' is the first child of the prefabs. It contains an AvatarGo component with the following properties:
  - **Driver Type**: Use SteamVR (HTC VIVE + Trackers) or Simulator
  - **Driver Setup**: Reference to [SteamVR Setup] or [Simulator Setup]
  - **Avatar Prefab**: Prefab of the avatar to use, it should have an AvatarProperties component
  - **Show Mirror**: Whether to display a mirror to help the user with the Walk-In-Place step or not
  - **Controllers Style**:
    * OpenHandFreeControllers: Show controllers in their real position with no Finger IK
    * ClosedHandFreeControllers: Show controllers in their real position but with Finger IK
    * ClosedHandAttachedControllers: Show controllers attached to the hands with Finger IK
    * OpenHandHiddenControllers: No controllers
  - **OnCalibrationFinished**: Scripts can subscribe to this event; it will be called once the calibration is completed and the user is inside the avatar
- Add a ground (e.g., a plane) with position y=0 and with a collider so the avatar can be correctly placed on top
- Press Play!

## Hardware setup
When using SteamVRAvatarGo prefab, the user will need to place the controllers and HTC VIVE trackers as in the following picture:
<p align="center">
  <img 
    width="1089"
    height="600"
    src="https://github.com/UPC-ViRVIG/AvatarGo/blob/main/.github/media/hardware_setup.PNG"
  >
</p>

## Add new avatars
Please, look at the avatars (prefabs) already incorporated with AvatarGo. They can be found in 'Assets/AvatarGo/Resources/Avatars/'

- Add into the assets folder any mesh with a humanoid skeleton recognized by Unity
- Open the mesh import panel (click on the mesh and in the Inspector view) and go to the Rig menu. Select Animation Type = 'Humanoid'
- Apply changes
- Drop the imported mesh into the scene and add an AvatarProperties component (same GameObject as the Animator component), fill the following properties (make sure you have Gizmos enabled in the Scene view):
  - **Eyes Height**: A blue sphere will appear in front of the avatar's head. It indicated the eyes' height with respect to the head bone. Adjust this value until the blue sphere is at the eyes' height.
  - **Default Avatar**: Reference to the Avatar field in the Animator component. It is the avatar definition Unity created when importing the avatar.
  - **Root**: Reference to the root of the avatar's skeleton. In Unity, this is a GameObject child of the GameObject containing the Animator.
  - **Head Center**: Start with (0, 0, 0) and a cyan/light blue sphere centered on the head bone will appear. Adjust it until it covers the whole head (and ideally none of the rest of the body). It is used when clipping the head in first person view.
  - **Head Radius**: The radius of the previous sphere.
  - **Closed Hand & Finger IK**: These are advanced attributes. They should work with most avatars by default. Hovering the mouse over their name will show a brief description of how to adjust them.
- Create a Prefab of the avatar. Move it to any folder in the Project view to create a Prefab. To use the avatar with the built-in collaborative, it should be placed in the 'Assets/AvatarGo/Resources/Avatars/' directory.
- Ensure all materials of the avatar use the Shader: 'Custom/AvatarShader' or 'Custom/AvatarShaderTransparent' to perform clipping correctly.
- That's it! The prefab is ready to be used in the AvatarGo component.

## Collaborative
Documentation under construction... :)
