<p align="center">
  <img 
    width="960"
    height="216"
    src="https://github.com/UPC-ViRVIG/AvatarGo/blob/main/.github/media/Cover.png"
  >
</p>

**AvatarGo** is a Unity package for incorporating **self-avatars** in any **Virtual Reality** application with a low-cost set-up consisting of a **SteamVR-based HMD, 2 Controllers and 3 HTC VIVE Trackers**. It incorporates a custom calibration step in which users need to walk inside a virtual avatar. Using a custom Fingers Inverse Kinematics solution, the hands can automatically adjust to the controllers.

Demo video: https://youtu.be/DWU4p-a-uXo

## Contents
1. [Installation](#installation)
2. [How to Use](#how-to-use)
2. [Citation](#citation)
3. [Troubleshooting FAQ](#troubleshooting-faq)
4. [License](#license)

## Installation
There are two ways to install AvatarGo: creating a new project by cloning this repository or using an existing project and adding AvatarGo as a package.
### First option: Package
- Download the last version from the [Release page](https://github.com/UPC-ViRVIG/AvatarGo/releases)
- Open the package or go to the Unity menu ‘Assets/Import Package/Custom Package’ and select the package
- At this point some errors will appear until all dependencies are installed:
- From the Packages Window 'Window/Package Manager':
  - Enable preview packages:
    - Settings/gear button and select Advanced Project Settings
    - Check 'Enable Preview Packages'
- Install the following packages (if not already installed) from the Packages Window:
  - Netcode for GameObjects: 
    - Click the + button
    - Add package from git URL: com.unity.netcode.gameobjects and click Add
  - TextMeshPro (from Unity Registry) (after installation make sure the TMP Essential Resources are imported from the TextMeshPro window)
- Install SteamVR from the Asset Store: https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647
(When installing SteamVR, Unity should automatically set up the project for its use in VR and install OpenVR)
- Delete all MainCameras in the scene (or change their MainCamera tag)
### Second option: Clone
- Clone this repository with git
- Open the project with Unity (it will warn you that there are errors in the project, press continue and ignore)
- When the project is cloned the Package Manager dependencies should be handled automatically by Unity, if that is not the case, please follow the steps for installing dependencies in the First option
- Install SteamVR from the Asset Store: https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647
(When installing SteamVR, Unity should automatically set up the project for its use in VR and install OpenVR)
- Delete all MainCameras in the scene (or change their MainCamera tag)


## How to Use
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

## Citation
Please consider citing this paper in your publications if it helps your research:
```
@article{ponton2022avatargo,
  author = {Jose Luis Ponton and Eva Monclús and Nuria Pelechano},
  title = {AvatarGo: Plug and Play self-avatars for VR},
  booktitle = {Eurographics 2022 Short Papers},
  year = {2022}
}
```

Link to the paper: TBD

## Troubleshooting FAQ
**Q: The project cannot be opened because Unity warns that there are errors to be solved:**

Some dependencies might not be installed yet. Press continue and open the project, then follow the installation instructions to add all dependencies.

**Q: The mirror is not displaying any text:**

Probably the TMP Essential Resources are not imported. Go to: 'Window/TextMeshPro/Import TMP Essential Resources' and import all files.

**

## License
This work is licensed under CC BY-NC-SA 4.0.
AvatarGo is freely available for free non-commercial use, and may be redistributed under these conditions.  Please, see the [license](https://github.com/UPC-ViRVIG/AvatarGo/blob/main/LICENSE) for further details.
