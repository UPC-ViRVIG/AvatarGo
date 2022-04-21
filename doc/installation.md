## Unity Version
AvatarGo has been tested in **Unity 2020.3 LTS** and **2021.2**

We **recommend using 2020.3+** since previous Unity version may have different dependencies/packages manager and AvatarGo might be more difficult to install. However, if you manage to install all dependencies (SteamVR, Netcode for GameObjects and TextMeshPro) it should work in older Unity versions.

## Installation
There are two ways to install AvatarGo: using an existing project and adding AvatarGo as a package or creating a new project by cloning this repository.
### First option: Package
- Download the last version of the .unitypackage from the [Release page](https://github.com/UPC-ViRVIG/AvatarGo/releases)
- Open the package or go to the Unity menu ‘Assets/Import Package/Custom Package’ and select the package
- At this point some errors will appear until all dependencies are installed:
- From the Packages Window 'Window/Package Manager':
  - Enable preview packages:
    - Settings/gear button and select Advanced Project Settings
    - Check 'Enable Preview Packages' or 'Enable Pre-release Packages' (depending on the Unity version)
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
