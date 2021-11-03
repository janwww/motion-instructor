# MR Motion Instructor
[**Report**][report-link] | [**Presentation**][presentation-link] | [**Video**][video-link]


[report-link]: https://drive.google.com/file/d/14RkPbbcBlSm9PFg2t-S5-5DiDakiR77F/view?usp=sharing
[presentation-link]: https://drive.google.com/file/d/1Tdgr7TpSHI3s6lD8M-23Ves3paCuMY5g/view?usp=sharing
[video-link]: https://drive.google.com/file/d/12SufbJpmwGuYedoX1JAJ5AfUv7IJO9HS/view?usp=sharing


Authors:
[Jan Wiegner](https://github.com/janwww), 
[Rudolf Varga](https://github.com/ketjatekos), 
[Felix Pfreundtner](https://github.com/felixpfreundtner), 
[Daniil Emtsev](https://github.com/daniil-777) and 
[Utkarsh Bajpai](https://github.com/Utkarsh-Bajpai)

Project for the course Mixed Reality Lab 2020 at ETH.

Check out the report and video linked above for more detailed information.


### Abstract
Learning complex movements can be a time-consuming process, but it is necessary for the mastery of activities like Karate Kata, Yoga and dance choreographies. 
It is important to have a teacher to demonstrate the correct pose sequences step by step and correct errors in the student’s body postures.
In-person sessions can be impractical due to epidemics or travel distance, while videos make it hard to see the 3D postures of the teacher and of the students. 
As an alternative, we propose the teaching of poses in Augmented Reality (AR) with a virtual teacher and 3D avatars.
The focus of our project was on dancing, but it can easily be adapted for other activities.

<p align="center">
<img src="images/train.gif">
</p>


### Architecture

<p align="center">
<img src="images/architecture_main.png">
</p>

Our main method of pose estimation is using the Azure Kinect with the Sensor SDK and Body tracking SDK 
and we use the official C# wrapper to connect it to Unity.
We rely on Microsoft’s 
[Holographic Remoting Player](https://docs.microsoft.com/en-us/windows/mixed-reality/develop/platform-capabilities-and-apis/holographic-remoting-player) 
to display content on the HoloLens 2 which is being run and rendered on a separate computer.


### Learning Motion in MR
<p align="center">
<img width=380 src="images/menu.gif">
<img width=380 src="images/handmenu.gif">
</p>

The user has an option of following a guided course, which consists of repeating basic steps to perfect them and testing their skills on choreographies.
They can also use freeplay mode to beat their previous highest score.

There are a multitude of visualization options, so the user can change the environment to their own needs and accelerates the learning process.
This includes creating multiple instances of his avatar and the one of the teacher, mirroring them, showing a graph of the score, a live RGB feed from the Kinect and others.
Changes can be either done in the main menu or trough the hand menu for smaller changes.



<p align="center">
<img src="images/choreography.gif">
</p>

Our scoring mechanism is explained in our [report][report-link].

### Avatars

<p align="center">
<img src="images/avatars.gif">
</p>

There are 4 avatar options to choose from in our project: 
- Cube avatar (position and orientation of all estimated joints)
- Stick figure avatar (body parts used for score calculation, changing color depending on correctness)
- Robot avatar (rigged model)
- SMPL avatar models (parametrized rigged model)




## Environment
As of January 24, 2021, this repository has been tested under the following environment:
- Windows 10 Education (10.0.19042 Build 19042)
- [All tools required to develop on the HoloLens](https://docs.microsoft.com/en-us/windows/mixed-reality/install-the-tools)
- Unity 2019.4.13f1 // Unity 2019 LTS version

 A dedicated CUDA compatible graphics card is necessary, NVIDIA GEFORCE GTX 1070 or better. For more information consult the 
 [official BT SDK hardware requirements](https://docs.microsoft.com/en-us/azure/kinect-dk/system-requirements). 
 We used a GTX 1070 for development and testing.

## Get Started
1. Clone this repository.
2. Open the `unity` folder as a Unity project, with `Universal Windows Platform` as the build platform. It might take a while to fetch all packages.
3. Use the Mixed Reality Feature Tool to install:
    - Mixed Reality Toolkit Examples v2.7.2
    - Mixed Reality Toolkit Extensions v2.7.2
    - Mixed Reality Toolkit Foundation v2.7.2
    - Mixed Reality Toolkit Standard Assets v2.7.2
    - Mixed Reality Toolkit Tools v2.7.2
    - Mixed Reality OpenXR Plugin v1.1.1
5. Setup the Azure Kinect Libraries: (same as [Sample Unity Body Tracking Application](https://github.com/microsoft/Azure-Kinect-Samples/tree/master/body-tracking-samples/sample_unity_bodytracking))
    1. Upgrade Microsoft.Azure.Kinect.Sensor Package to the newest Version (v1.4.1)
    2. Manually install the Azure Kinect Body Tracking SDK v1.1.0 [Link](https://docs.microsoft.com/en-us/azure/kinect-dk/body-sdk-download)
    4. Get the NuGet packages of libraries:
        - Open the Visual Studio Solution (.sln) associated with this project. You can create one by opening a csharp file in the Unity Editor.
        - In Visual Studio open: Tools->NuGet Package Manager-> Package Manager Console
        - When Prompted to get missing packages, click confirm
    5. Move libraries to correct folders:
        - Execute the file `unity/MoveLibraryFile.bat`. You should now have library files in `unity/` and in the newly created `unity/Assets/Plugins`.
    6. Add these libraries to the root directory (contains the assets folder)
       From Azure Kinect Body Tracking SDK\tools\
        - cudnn64_8.dll
        - cudnn64_cnn_infer64_8.dll
        - cudnn64_ops_infer64_8.dll
        - cudart64_110.dll
        - cublas64_11.dll
        - cublasLt64_11.dll
        - onxxruntime.dll
        - dnn_model_2_0_op11.onnx
6. Open `Assets/PoseteacherScene` in the Unity Editor.
7. When prompted to import TextMesh Pro, select `Import TMP Essentials`. You will need to reopen the scene to fix visual glitches.
8. (Optional) Connect to the HoloLens with [Holographic Remoting](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Tools/HolographicRemoting.html#connecting-to-the-hololens-with-wi-fi) using the `Windows XR Plugin Remoting` in Unity.
Otherwise the scene will only play in the editor.
7. (Optional) In the `Main` object in `PoseteacherScene` set `Self Pose Input Source` to `KINECT`.
Otherwise the input of the user is simulated from a file.
8. Click play inside the Unity editor.



### Notes
- Most of the application logic is inside of the `PoseteacherMain.cs` script which is attached to the `Main` game object.
- If updating from an old version of the project, be sure to delete the `Library` folder generated by Unity, so that packages are handled correctly. 
- The project uses MRTK 2.5.1 from the Unity Package Manager, not imported into the `Assets` folder: 
   - MRTK assets have to be searched inside `Packages` in the editor.
   - The only MRTK files in `Assets` should be in folders `MixedRealityToolkit.Generated` and `MRTK/Shaders`. 
   - Only exception is the `Samples/Mixed Reality Toolkit Examples` if [MRTK examples are imported](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/usingupm.html#using-mixed-reality-toolkit-examples)
   - If there are other MRTK folders, they are from an old version of the project (or were imported manually) and [should be removed like when updating](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/Updating.html). 
     Remeber to delete the `Library` folder after doing this.
- We use the newer XR SDK pipeline instead of the Legacy XR pipeline (which is depreciated)

## How to use
Use the UI to navigate in the application. This can also be done in the editor, consult the 
[MRTK In-Editor Input Simulation](https://microsoft.github.io/MixedRealityToolkit-Unity/Documentation/InputSimulation/InputSimulationService.html)
page to see how.

For debugging we added the following keyboard shortcuts:
 - H for toggling Hand menu in training/choreography/recording (for use in editor testing)
 - O for toggling pause of teacher avatar updates
 - P for toggling pause of self avatar updates
 - U for toggling force similarity update (even if teacher updates are paused)


## License

All our code and modifications are licensed under the attached MIT License. 

We use some code and assets from:
- [This fork](https://github.com/Aviscii/azure-kinect-dk-unity) of the [azure-kinect-dk-unity repository](https://github.com/curiosity-inc/azure-kinect-dk-unity) (MIT License).
- [NativeWebSocket](https://github.com/endel/NativeWebSocket) (Apache-2.0 License). 
- [SMPL](https://smpl.is.tue.mpg.de/) (Creative Commons Attribution 4.0 International License). 
- [Space Robot Kyle](https://assetstore.unity.com/packages/3d/characters/robots/space-robot-kyle-4696) (Unity Extension Asset License). 
- [Lightweight human pose estimation](https://github.com/Daniil-Osokin/lightweight-human-pose-estimation-3d-demo.pytorch) (Apache-2.0 License). 
- [Official Sample Unity Body Tracking Application](https://github.com/microsoft/Azure-Kinect-Samples/tree/master/body-tracking-samples/sample_unity_bodytracking) (MIT License)


## Alternative pose estimation (experimental)

We show an example of using the Websockets for obtaining the pose combined with the [Lightweight human pose estimation](https://github.com/Daniil-Osokin/lightweight-human-pose-estimation-3d-demo.pytorch) repository. If you do not have an Azure Kinect or GPU you can use this, but it will be very slow. 

Clone the repository and copy `alt_pose_estimation/demo_ws.py` into it. 
Install the required packages according to the repository and run `demo_ws.py`. 
Beware that Pytorch still has issues with Python 3.8, so we recommend using Python 3.7. 
It should now be sending pose data over a local Websocket, which can be used if the `SelfPoseInputSource` value is set to `WEBSOCKET` for the `Main` object in the Unity Editor. 
Depending on the version of the project, some changes might need to be made in `PoseInputGetter.cs` to correctly setup the Websocket.
