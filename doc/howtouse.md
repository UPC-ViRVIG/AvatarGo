## How to Use
- Take a look at the demo scenes: AvatarGo/Scenes/
- Drop into the scene one of the following prefabs:
  - SimulatorAvatarGo (use this to test the avatars without HMD)
  - SteamVRAvatarGo
- 'AvatarGo' is the first child the prefabs. It contains an AvatarGo component with the following properties:
  - Driver Type: Use SteamVR (HTC VIVE + Trackers) or Simulator
  - Driver Setup: Reference to [SteamVR Setup] or [Simulator Setup]
  - Avatar Prefab: Prefab of the avatar to use, it should have an AvatarProperties component
  - Show Mirror: Whether to display a mirror to help the user with the Walk-In-Place step or not
  - Controllers Style:
    * OpenHandFreeControllers: Show controllers in the real position with no Finger IK
    * ClosedHandFreeControllers: Show controllers in the real position but with Finger IK
    * ClosedHandAttachedControllers: Show controllers attached to the hands with Finger IK
    * OpenHandHiddenControllers: No controllers
  - OnCalibrationFinished: Scripts can suscribe to this event, it will be called once the calibration is completed and the user is inside the avatar
- Add a ground (for example a plane) with position y=0 and with a collider so the avatar can be correctly placed on top
- Press Play!

## Hardware
When using SteamVRAvatarGo prefab, the user will need to place the controllers and HTC VIVE trackers as in the following picture:
<p align="center">
  <img 
    width="960"
    height="216"
    src="https://github.com/UPC-ViRVIG/AvatarGo/blob/main/.github/media/hardware_setup.png"
  >
</p>