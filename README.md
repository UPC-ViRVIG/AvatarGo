**AvatarGo** is a Unity package for incorporating **self-avatars** in any **Virtual Reality** application with a low-cost set-up consisting of a **SteamVR-based HMD, 2 Controllers and 3 HTC VIVE Trackers**. It incorporates a custom calibration step in which users need to walk inside a virtual avatar. Using a custom Fingers Inverse Kinematics solution, the hands can automatically adjust to the controllers.

Demo video: https://youtu.be/DWU4p-a-uXo

## Contents
1. [Installation](#installation)
2. [How to Use](#how-to-use)
2. [Citation](#citation)
3. [Troubleshooting FAQ](#troubleshooting-faq)
4. [License](#license)

## Installation
- Download the last version from the [Release page](https://github.com/UPC-ViRVIG/AvatarGo/releases)
- Open the package or go to the Unity menu ‘Assets/Import Package/Custom Package’ and select the package
- From the Packages Window 'Window/Package Manager':
  - Enable preview packages:
    - Settings/gear button and select Advanced Project Settings
    - Check 'Enable Preview Packages'
- Install the following packages from the Packages Window:
  - Netcode for GameObjects: 
    - Click the + button
    - Add package from git URL
    - Type: com.unity.netcode.gameobjects and click Add
  - TextMeshPro (from Unity Registry) (if not already installed)
- Install SteamVR from the Asset Store: https://assetstore.unity.com/packages/tools/integration/steamvr-plugin-32647
(When installing SteamVR, Unity should automatically set up the project for its use in VR and install OpenVR)
- Delete all MainCameras in the scene (or change their MainCamera tag)

## How to Use

## Citation

## Troubleshooting FAQ

## License
