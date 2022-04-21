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
Follow the steps in the installation documentation in [doc/installation.md](doc/installation.md)

## How to Use
All documentation can be found in [doc/howtouse.md](doc/howtouse.md)

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
