**AvatarGo** is a Unity package for incorporating **self-avatars** in any **Virtual Reality** application with a low-cost set-up consisting of a **SteamVR-based HMD, 2 Controllers and 3 HTC VIVE Trackers**. It incorporates a custom calibration step in which users need to walk inside a virtual avatar. Using a custom Fingers Inverse Kinematics solution, the hands can automatically adjust to the controllers.

Demo video: https://youtu.be/DWU4p-a-uXo

## Contents
1. [Installation](#installation)
2. [How to Use](#how-to-use)
2. [Citation](#citation)
3. [Troubleshooting FAQ](#troubleshooting-faq)
4. [License](#license)

## Installation
- Download the last version from the [Release page](https://github.com/UPC-ViRVIG/AvatarGo/releases) or clone this repository
- Skip this step if the repository was cloned: Open the package or go to the Unity menu ‘Assets/Import Package/Custom Package’ and select the package
- At this point some errors will appear until all dependencies are installed:
- From the Packages Window 'Window/Package Manager':
  - Enable preview packages:
    - Settings/gear button and select Advanced Project Settings
    - Check 'Enable Preview Packages'
- Install the following packages (if not already installed) from the Packages Window:
  - Netcode for GameObjects: 
    - Click the + button
    - Add package from git URL: com.unity.netcode.gameobjects and click Add
  - TextMeshPro (from Unity Registry)
- Install SteamVR from the Asset Store: https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647
(When installing SteamVR, Unity should automatically set up the project for its use in VR and install OpenVR)
- Delete all MainCameras in the scene (or change their MainCamera tag)

## How to Use
- Drop into the scene one of the following prefabs:
  - SimulatorAvatarGo (use this to test the avatars without HMD)
  - SteamVRAvatarGo
- 'AvatarGo' is the first GameObject child of the prefabs,  TODO

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
**Q: I cannot open the project because Unity says there are errors to be solved:**

There are errors because some dependencies are not installed yet. Press continue and open the project, then follow the installation instructions to add all dependencies.

## License
