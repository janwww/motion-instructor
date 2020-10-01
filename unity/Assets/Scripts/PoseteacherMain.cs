﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using NativeWebSocket;

public class AvatarGo
{
    void prepareGameObjects(GameObject avatarContainer, ref GameObject[] debugObjects)
    {
        GameObject cubeC = avatarContainer.transform.Find("CubeContainer").gameObject;
        debugObjects = new GameObject[(int)JointType.Count];
        for (var i = 0; i < (int)JointType.Count; i++)
        {
            var cube = cubeC.transform.Find(Enum.GetName(typeof(JointType), i)).gameObject;
            cube.transform.localScale = Vector3.one * 0.4f;
            cube.transform.SetParent(cubeC.transform);
            debugObjects[i] = cube;
        }
        
    }

    public GameObject LeftLowerLeg, RightLowerLeg, LeftUpperArm, RightUpperArm, LeftUpperLeg, RightUpperLeg, TorsoLeft,
       TorsoRight, HipStick, LeftLowerArm, RightLowerArm, LeftEye, RightEye, Shoulders, MouthStick, NoseStick, LeftEar, RightEar;

    public GameObject LeftShoulderStick, RightShoulderStick, LeftHipStick, RightHipStick, LeftElbowStick, RightElbowStick, LeftWristStick, RightWristStick,
        LeftKneeStick, RightKneeStick, LeftAnkleStick, RightAnkleStick;

    public GameObject avatarContainer, cubeContainer, stickContainer, robotContainer, smplContainer;
    public GameObject[] debugObjects;

    public AvatarGo(GameObject avatarContainer)
    {
        this.avatarContainer = avatarContainer;
        GameObject[] debugObjects = {};
        prepareGameObjects(avatarContainer, ref debugObjects);
        this.debugObjects = debugObjects;

        GameObject cubeC = avatarContainer.transform.Find("CubeContainer").gameObject;
        GameObject stickC = avatarContainer.transform.Find("StickContainer").gameObject;
        GameObject robotC = avatarContainer.transform.Find("RobotContainer").gameObject;
        GameObject smplC = avatarContainer.transform.Find("SMPLContainer").gameObject;
        cubeContainer = cubeC;
        stickContainer = stickC;
        robotContainer = robotC;
        smplContainer = smplC;

        
        LeftLowerLeg = stickC.transform.Find("LLLeg").gameObject;
        RightLowerLeg = stickC.transform.Find("RLLeg").gameObject;
        LeftUpperArm = stickC.transform.Find("LeftUpperArm").gameObject;
        RightUpperArm = stickC.transform.Find("RightUpperArm").gameObject;
        LeftUpperLeg = stickC.transform.Find("LeftUpperLeg").gameObject; // HipLeft 18 KneeLeft 19 AnkleLeft 20
        RightUpperLeg = stickC.transform.Find("RightUpperLeg").gameObject; // HipRight 22 KneeRight 23 AnkleRight 24
        TorsoLeft = stickC.transform.Find("TorsoLeft").gameObject; // ShoulderLeft 5 HipLeft 18
        TorsoRight = stickC.transform.Find("TorsoRight").gameObject; // ShoulderRight 12 HipRight 22
        HipStick = stickC.transform.Find("HipStick").gameObject; // HipLeft 18 HipRight 22
        LeftLowerArm = stickC.transform.Find("LeftLowerArm").gameObject; // = ElbowLeft 6 WristLeft 7
        RightLowerArm = stickC.transform.Find("RightLowerArm").gameObject; // = ElbowRight 13 WristRight 14
        LeftEye = stickC.transform.Find("LeftEye").gameObject; // 28
        RightEye = stickC.transform.Find("RightEye").gameObject; // 30
        Shoulders = stickC.transform.Find("Shoulders").gameObject; // ShoulderLeft 5 ShoulderRight 12
        MouthStick = stickC.transform.Find("MouthStick").gameObject; // = Neck 3
        NoseStick = stickC.transform.Find("NoseStick").gameObject; // 27
        LeftEar = stickC.transform.Find("LeftEar").gameObject; // 29
        RightEar = stickC.transform.Find("RightEar").gameObject; // 31

        LeftShoulderStick = stickC.transform.Find("LeftShoulderStick").gameObject;
        RightShoulderStick = stickC.transform.Find("RightShoulderStick").gameObject;
        LeftHipStick = stickC.transform.Find("LeftHipStick").gameObject;
        RightHipStick = stickC.transform.Find("RightHipStick").gameObject;
        LeftElbowStick = stickC.transform.Find("LeftElbowStick").gameObject;
        RightElbowStick = stickC.transform.Find("RightElbowStick").gameObject;
        LeftWristStick = stickC.transform.Find("LeftWristStick").gameObject;
        RightWristStick = stickC.transform.Find("RightWristStick").gameObject;
        LeftKneeStick = stickC.transform.Find("LeftKneeStick").gameObject;
        RightKneeStick = stickC.transform.Find("RightKneeStick").gameObject;
        LeftAnkleStick = stickC.transform.Find("LeftAnkleStick").gameObject;
        RightAnkleStick = stickC.transform.Find("RightAnkleStick").gameObject;

        stickContainer.SetActive(true);
        cubeContainer.SetActive(false);
        robotContainer.SetActive(false);
        smplContainer.SetActive(false);
    }
}

public class PoseteacherMain : MonoBehaviour
{
    Device device;
    BodyTracker tracker;

    public Renderer renderer;
    GameObject streamCanvas;
    GameObject videoCube;

    AvatarGo avatarSelf;
    AvatarGo avatarTeacher;
    AvatarGo avatarSelf2;
    AvatarGo avatarTeacher2;

    public bool mirroring = true;
    GameObject spatialAwarenessSystem;
    RawImage m_RawImage;
    //Select a Texture in the Inspector to change to
    public Texture m_Texture;
    Texture2D tex;
    public int recording_mode = 0; // 0: not recording, 1: recording, 2: playback, 3: load_file, 4: reset_recording
    public int playback_speed = 1; // 1: x1 , 2: x0.5, 4: x0.25
    private int playback_counter = 0;
    IEnumerable<string> read_recording_data;
    IEnumerator<string> sequenceEnum;
    JointDataList saved_joint_data;
    JointDataListLive remote_joints_live = null;

    string current_file = "jsondata.txt";

    public bool isMaleSMPL = true; // can be changed in the UI
    public bool usingKinectAlternative = true;
    public bool debugMode = true;
    private bool loadedFakeData = false;
    WebSocket websocket;
    public string WS_ip = "ws://localhost:2567";
    public Material normal_material;
    public Material correct_material;
    public Material incorrect_material;
    public bool shouldCheckCorrectness = true;
    public float correctionThresh = 30.0f;
    public void do_mirror()
    {

        if (mirroring == true)
        {
            mirroring = false;
            avatarSelf.avatarContainer.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            avatarTeacher.avatarContainer.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            avatarSelf2.avatarContainer.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            avatarTeacher2.avatarContainer.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            videoCube.transform.localScale = new Vector3(-1f, 0.6f, 0.1f);
            streamCanvas.transform.localScale = new Vector3(32f, 16f, 1f);
        }
        else
        {
            mirroring = true;
            avatarSelf.avatarContainer.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            avatarTeacher.avatarContainer.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            avatarSelf2.avatarContainer.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            avatarTeacher2.avatarContainer.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);

            videoCube.transform.localScale = new Vector3(1f, 0.6f, 0.1f);
            streamCanvas.transform.localScale = new Vector3(-32f, 16f, 1f);
        }
    }

    public void set_SMPL(bool value)
    {
        isMaleSMPL = value;
    }
    public void set_OpenPose(bool value)
    {
        usingKinectAlternative = value;
    }
    public void set_recording_mode(int rec)
    {
        recording_mode = rec;
    }
    public void set_correctionThresh(float thresh)
    {
        correctionThresh = thresh;
    }

    public void set_playback_speed(int a)
    {
        playback_speed = a;
    }

    public void set_current_file(string curr)
    {
        current_file = curr;
    }

    public void gen_new_current_file()
    {
        string timestamp = DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
        current_file = "jsondata/" + timestamp + ".txt";
    }

    [Serializable]
    public class RemoteJointList
    {
        public List<List<double>> values { get; set; }
    }
    async void Start()
    {

        // Unity apparently cannot deserialize arrays, so we had to do it manually
        RemoteJointList DeserializeRJL(string message)
        {
            string[] joints3D = message.Split(new string[] { "], [" }, StringSplitOptions.None);
            joints3D[0] = joints3D[0].Substring(3);
            joints3D[18] = joints3D[18].Split(']')[0];
            
            List<List<double>> joints = new List<List<double>>();
            for (int i = 0; i < 19; i++)
            {   
                string [] joint = joints3D[i].Split(',');
                List<double> dbl = new List<double> { Convert.ToDouble(joint[0]), Convert.ToDouble(joint[1]), Convert.ToDouble(joint[2]) };
                joints.Add(dbl);
            }
            RemoteJointList rjl = new RemoteJointList { values = joints };
            return rjl;
        }

        websocket = new WebSocket("ws://localhost:2567");

        websocket.OnOpen += () =>
        {
            Debug.Log("WS connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WS connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("WS message received: " + message);
            var remote_joints = DeserializeRJL(message);
            Debug.Log(remote_joints);
            remote_joints_live = Remote_to_JDL_Live(remote_joints);
        };

        // Keep sending messages at every 0.3s
        InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        await websocket.Connect();
    }


    async void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending text
            await websocket.SendText("Test message");
        }
    }

    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

    private void OnEnable()
    {
        Debug.Log("OnEnable");

        // This returns the GameObject named Hand.
        streamCanvas = GameObject.Find("RawImage");
        videoCube = GameObject.Find("VideoCube");

        GameObject avatarContainer = GameObject.Find("AvatarContainer");
        GameObject avatarContainerT = GameObject.Find("AvatarContainerT");
        GameObject avatarContainer2 = GameObject.Find("AvatarContainer2");
        GameObject avatarContainerT2 = GameObject.Find("AvatarContainerT2");

        Debug.Log(streamCanvas);

        if(usingKinectAlternative == false)
        {
            this.device = Device.Open(0);
            var config = new DeviceConfiguration
            {
                ColorResolution = ColorResolution.r720p,
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_Unbinned
            };
            device.StartCameras(config);

            var calibration = device.GetCalibration(config.DepthMode, config.ColorResolution);

            var trackerConfiguration = new TrackerConfiguration
            {
                SensorOrientation = SensorOrientation.OrientationDefault,
                CpuOnlyMode = false
            };
            this.tracker = BodyTracker.Create(calibration, trackerConfiguration);
        }

        avatarSelf = new AvatarGo(avatarContainer);
        avatarTeacher = new AvatarGo(avatarContainerT);
        avatarSelf2 = new AvatarGo(avatarContainer2);
        avatarTeacher2 = new AvatarGo(avatarContainerT2);

        avatarTeacher.avatarContainer.gameObject.SetActive(false);
        avatarSelf2.avatarContainer.gameObject.SetActive(false);
        avatarTeacher2.avatarContainer.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        if (tracker != null)
        {
            tracker.Dispose();
        }
        if (device != null)
        {
            device.Dispose();
        }
    }


    void checkKeyInput()
    {
        if (Input.GetKey(KeyCode.X))
        {
            Debug.Log("X - set recording_mode to 0 (not recording)");
            recording_mode = 0;
        }
        else if (Input.GetKey(KeyCode.Y))
        {
            Debug.Log("Y - set recording_mode to 1 (recording)");
            recording_mode = 1;
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            Debug.Log("Z - set recording_mode to 2 (playback)");
            recording_mode = 2;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            Debug.Log("L - set recording_mode to 3 (load_file)");
            recording_mode = 3;
        }
        else if (Input.GetKey(KeyCode.R))
        {
            Debug.Log("R - set recording_mode to 4 (reset_recording)");
            recording_mode = 4;
        }
    }


    void moveStickPerson(JointDataListLive joint_data_list, AvatarGo avatarSelf)
    {
        /************************************Joints**************************************/
        JointDataLive stickJoint = joint_data_list.data[5];
        var stickPos = stickJoint.Position;
        var stickOrientation = stickJoint.Orientation;
        var stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        var stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftShoulderStick.transform.localPosition = stickV;
        avatarSelf.LeftShoulderStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[12];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightShoulderStick.transform.localPosition = stickV;
        avatarSelf.RightShoulderStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[18];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftHipStick.transform.localPosition = stickV;
        avatarSelf.LeftHipStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightHipStick.transform.localPosition = stickV;
        avatarSelf.RightHipStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[6];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftElbowStick.transform.localPosition = stickV;
        avatarSelf.LeftElbowStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[13];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightElbowStick.transform.localPosition = stickV;
        avatarSelf.RightElbowStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[7];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftWristStick.transform.localPosition = stickV;
        avatarSelf.LeftWristStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[14];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightWristStick.transform.localPosition = stickV;
        avatarSelf.RightWristStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[19];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftKneeStick.transform.localPosition = stickV;
        avatarSelf.LeftKneeStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[23];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightKneeStick.transform.localPosition = stickV;
        avatarSelf.RightKneeStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[20];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftAnkleStick.transform.localPosition = stickV;
        avatarSelf.LeftAnkleStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[24];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightAnkleStick.transform.localPosition = stickV;
        avatarSelf.RightAnkleStick.transform.localRotation = stickR;


        /************************************Head**************************************/
        stickJoint = joint_data_list.data[28];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftEye.transform.localPosition = stickV;
        avatarSelf.LeftEye.transform.localRotation = stickR;
        avatarSelf.LeftEye.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);

        stickJoint = joint_data_list.data[30];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightEye.transform.localPosition = stickV;
        avatarSelf.RightEye.transform.localRotation = stickR;
        avatarSelf.RightEye.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);

        stickJoint = joint_data_list.data[27];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.NoseStick.transform.localPosition = stickV;
        avatarSelf.NoseStick.transform.localRotation = stickR;
        avatarSelf.NoseStick.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        stickJoint = joint_data_list.data[29];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftEar.transform.localPosition = stickV;
        avatarSelf.LeftEar.transform.localRotation = stickR;
        avatarSelf.LeftEar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        avatarSelf.LeftEar.transform.LookAt(avatarSelf.LeftShoulderStick.transform.position);
        avatarSelf.LeftEar.transform.Rotate(90, 0, 0);

        stickJoint = joint_data_list.data[31];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightEar.transform.localPosition = stickV;
        avatarSelf.RightEar.transform.localRotation = stickR;
        avatarSelf.RightEar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        avatarSelf.RightEar.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.RightEar.transform.Rotate(90, 0, 0);

        stickJoint = joint_data_list.data[27]; 
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.MouthStick.transform.localPosition = stickV;
        avatarSelf.MouthStick.transform.localRotation = stickR;
        avatarSelf.MouthStick.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);


        /************************************Body**************************************/
        stickJoint = joint_data_list.data[5];
        var stickJoint_b = joint_data_list.data[12];
        stickPos = stickJoint.Position;
        var stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        Vector3 stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        float stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.Shoulders.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.Shoulders.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.Shoulders.transform.Rotate(90, 0, 0);
        avatarSelf.Shoulders.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.HipStick.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.HipStick.transform.LookAt(avatarSelf.RightHipStick.transform.position);
        avatarSelf.HipStick.transform.Rotate(90, 0, 0);
        avatarSelf.HipStick.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[5];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.TorsoLeft.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.TorsoLeft.transform.LookAt(avatarSelf.LeftShoulderStick.transform.position);
        avatarSelf.TorsoLeft.transform.Rotate(90, 0, 0);
        avatarSelf.TorsoLeft.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[12];
        stickJoint_b = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.TorsoRight.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.TorsoRight.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.TorsoRight.transform.Rotate(90, 0, 0);
        avatarSelf.TorsoRight.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);


        /************************************Arms**************************************/
        stickJoint = joint_data_list.data[5];
        stickJoint_b = joint_data_list.data[6];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftUpperArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftUpperArm.transform.LookAt(avatarSelf.LeftElbowStick.transform.position);
        avatarSelf.LeftUpperArm.transform.Rotate(90, 0, 0);
        avatarSelf.LeftUpperArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[12];
        stickJoint_b = joint_data_list.data[13];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightUpperArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightUpperArm.transform.LookAt(avatarSelf.RightElbowStick.transform.position);
        avatarSelf.RightUpperArm.transform.Rotate(90, 0, 0);
        avatarSelf.RightUpperArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[6];
        stickJoint_b = joint_data_list.data[7];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftLowerArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftLowerArm.transform.LookAt(avatarSelf.LeftWristStick.transform.position);
        avatarSelf.LeftLowerArm.transform.Rotate(90, 0, 0);
        avatarSelf.LeftLowerArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[13];
        stickJoint_b = joint_data_list.data[14];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightLowerArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightLowerArm.transform.LookAt(avatarSelf.RightWristStick.transform.position);
        avatarSelf.RightLowerArm.transform.Rotate(90, 0, 0);
        avatarSelf.RightLowerArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);


        /************************************Legs**************************************/
        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[19];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftUpperLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftUpperLeg.transform.LookAt(avatarSelf.LeftHipStick.transform.position);
        avatarSelf.LeftUpperLeg.transform.Rotate(90, 0, 0);
        avatarSelf.LeftUpperLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[22];
        stickJoint_b = joint_data_list.data[23];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightUpperLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightUpperLeg.transform.LookAt(avatarSelf.RightHipStick.transform.position);
        avatarSelf.RightUpperLeg.transform.Rotate(90, 0, 0);
        avatarSelf.RightUpperLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[19];
        stickJoint_b = joint_data_list.data[20];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftLowerLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftLowerLeg.transform.LookAt(avatarSelf.LeftKneeStick.transform.position);
        avatarSelf.LeftLowerLeg.transform.Rotate(90, 0, 0);
        avatarSelf.LeftLowerLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[23];
        stickJoint_b = joint_data_list.data[24];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightLowerLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightLowerLeg.transform.LookAt(avatarSelf.RightKneeStick.transform.position);
        avatarSelf.RightLowerLeg.transform.Rotate(90, 0, 0);
        avatarSelf.RightLowerLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);
    }

    void moveCubePerson(JointDataListLive joint_data_list, AvatarGo avatarSelf)
    {
        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            var joint = joint_data_list.data[(int)jt];
            var pos = joint.Position;
            var orientation = joint.Orientation;
            var v = new Vector3(pos[0], -pos[1], pos[2]) * 0.004f;
            var r = new Quaternion(orientation[0], orientation[1], orientation[2], orientation[3]);
            var obj = avatarSelf.debugObjects[(int)jt];
            obj.transform.localPosition = v;
            obj.transform.localRotation = r;
            obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }

    void moveRobotPerson(JointDataListLive joint_data_list, AvatarGo avatarSelf)
    {
        JointDataLive stickJoint = joint_data_list.data[5];
        var stickPos = stickJoint.Position;
        var stickOrientation = stickJoint.Orientation;
        var stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        var stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftShoulderStick.transform.localPosition = stickV;
        avatarSelf.LeftShoulderStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[12];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightShoulderStick.transform.localPosition = stickV;
        avatarSelf.RightShoulderStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[18];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftHipStick.transform.localPosition = stickV;
        avatarSelf.LeftHipStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightHipStick.transform.localPosition = stickV;
        avatarSelf.RightHipStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[6];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftElbowStick.transform.localPosition = stickV;
        avatarSelf.LeftElbowStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[13];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightElbowStick.transform.localPosition = stickV;
        avatarSelf.RightElbowStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[7];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftWristStick.transform.localPosition = stickV;
        avatarSelf.LeftWristStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[14];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightWristStick.transform.localPosition = stickV;
        avatarSelf.RightWristStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[19];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftKneeStick.transform.localPosition = stickV;
        avatarSelf.LeftKneeStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[23];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightKneeStick.transform.localPosition = stickV;
        avatarSelf.RightKneeStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[20];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftAnkleStick.transform.localPosition = stickV;
        avatarSelf.LeftAnkleStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[24];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightAnkleStick.transform.localPosition = stickV;
        avatarSelf.RightAnkleStick.transform.localRotation = stickR;


        /************************************Head**************************************/
        stickJoint = joint_data_list.data[28];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftEye.transform.localPosition = stickV;
        avatarSelf.LeftEye.transform.localRotation = stickR;
        avatarSelf.LeftEye.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);

        stickJoint = joint_data_list.data[30];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightEye.transform.localPosition = stickV;
        avatarSelf.RightEye.transform.localRotation = stickR;
        avatarSelf.RightEye.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);

        stickJoint = joint_data_list.data[27];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.NoseStick.transform.localPosition = stickV;
        avatarSelf.NoseStick.transform.localRotation = stickR;
        avatarSelf.NoseStick.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        stickJoint = joint_data_list.data[29];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftEar.transform.localPosition = stickV;
        avatarSelf.LeftEar.transform.localRotation = stickR;
        avatarSelf.LeftEar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        avatarSelf.LeftEar.transform.LookAt(avatarSelf.LeftShoulderStick.transform.position);
        avatarSelf.LeftEar.transform.Rotate(90, 0, 0);

        stickJoint = joint_data_list.data[31];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightEar.transform.localPosition = stickV;
        avatarSelf.RightEar.transform.localRotation = stickR;
        avatarSelf.RightEar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        avatarSelf.RightEar.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.RightEar.transform.Rotate(90, 0, 0);

        stickJoint = joint_data_list.data[27];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.MouthStick.transform.localPosition = stickV;
        avatarSelf.MouthStick.transform.localRotation = stickR;
        avatarSelf.MouthStick.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);


        /************************************Body**************************************/
        stickJoint = joint_data_list.data[5];
        var stickJoint_b = joint_data_list.data[12];
        stickPos = stickJoint.Position;
        var stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        Vector3 stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        float stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.Shoulders.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.Shoulders.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.Shoulders.transform.Rotate(90, 0, 0);
        avatarSelf.Shoulders.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.HipStick.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.HipStick.transform.LookAt(avatarSelf.RightHipStick.transform.position);
        avatarSelf.HipStick.transform.Rotate(90, 0, 0);
        avatarSelf.HipStick.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[5];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.TorsoLeft.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.TorsoLeft.transform.LookAt(avatarSelf.LeftShoulderStick.transform.position);
        avatarSelf.TorsoLeft.transform.Rotate(90, 0, 0);
        avatarSelf.TorsoLeft.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[12];
        stickJoint_b = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.TorsoRight.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.TorsoRight.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.TorsoRight.transform.Rotate(90, 0, 0);
        avatarSelf.TorsoRight.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);


        /************************************Arms**************************************/
        stickJoint = joint_data_list.data[5];
        stickJoint_b = joint_data_list.data[6];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftUpperArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftUpperArm.transform.LookAt(avatarSelf.LeftElbowStick.transform.position);
        avatarSelf.LeftUpperArm.transform.Rotate(90, 0, 0);
        avatarSelf.LeftUpperArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[12];
        stickJoint_b = joint_data_list.data[13];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightUpperArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightUpperArm.transform.LookAt(avatarSelf.RightElbowStick.transform.position);
        avatarSelf.RightUpperArm.transform.Rotate(90, 0, 0);
        avatarSelf.RightUpperArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[6];
        stickJoint_b = joint_data_list.data[7];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftLowerArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftLowerArm.transform.LookAt(avatarSelf.LeftWristStick.transform.position);
        avatarSelf.LeftLowerArm.transform.Rotate(90, 0, 0);
        avatarSelf.LeftLowerArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[13];
        stickJoint_b = joint_data_list.data[14];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightLowerArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightLowerArm.transform.LookAt(avatarSelf.RightWristStick.transform.position);
        avatarSelf.RightLowerArm.transform.Rotate(90, 0, 0);
        avatarSelf.RightLowerArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);


        /************************************Legs**************************************/
        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[19];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftUpperLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftUpperLeg.transform.LookAt(avatarSelf.LeftHipStick.transform.position);
        avatarSelf.LeftUpperLeg.transform.Rotate(90, 0, 0);
        avatarSelf.LeftUpperLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[22];
        stickJoint_b = joint_data_list.data[23];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightUpperLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightUpperLeg.transform.LookAt(avatarSelf.RightHipStick.transform.position);
        avatarSelf.RightUpperLeg.transform.Rotate(90, 0, 0);
        avatarSelf.RightUpperLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[19];
        stickJoint_b = joint_data_list.data[20];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftLowerLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftLowerLeg.transform.LookAt(avatarSelf.LeftKneeStick.transform.position);
        avatarSelf.LeftLowerLeg.transform.Rotate(90, 0, 0);
        avatarSelf.LeftLowerLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[23];
        stickJoint_b = joint_data_list.data[24];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightLowerLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightLowerLeg.transform.LookAt(avatarSelf.RightKneeStick.transform.position);
        avatarSelf.RightLowerLeg.transform.Rotate(90, 0, 0);
        avatarSelf.RightLowerLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        // get Robot body parts
        GameObject right_shoulder_joint, right_upper_arm_joint, right_forearm_joint, left_upper_arm_joint, left_forearm_joint;
        GameObject left_thigh_joint, left_knee_joint, right_thigh_joint, right_knee_joint;
        GameObject robotKyle = avatarSelf.robotContainer.transform.Find("Robot Kyle").gameObject;
        GameObject robotRoot = robotKyle.transform.Find("Root").gameObject;
        GameObject hip = robotRoot.transform.Find("Hip").gameObject;
        GameObject ribs = robotRoot.transform.Find("Ribs").gameObject;
        GameObject left_shoulder_joint = ribs.transform.Find("Left_Shoulder_Joint_01").gameObject;

        right_shoulder_joint = ribs.transform.Find("Right_Shoulder_Joint_01").gameObject;
        bool set = false;
        if (right_shoulder_joint.transform.childCount == 1)
        {
            set = true;
            right_upper_arm_joint = right_shoulder_joint.transform.Find("Right_Upper_Arm_Joint_01").gameObject;
            left_upper_arm_joint = left_shoulder_joint.transform.Find("Left_Upper_Arm_Joint_01").gameObject;
            right_thigh_joint = hip.transform.Find("Right_Thigh_Joint_01").gameObject;
            left_thigh_joint = hip.transform.Find("Left_Thigh_Joint_01").gameObject;
        }
        else
        {
            right_upper_arm_joint = avatarSelf.robotContainer.transform.Find("Right_Upper_Arm_Joint_01").gameObject;
            left_upper_arm_joint = avatarSelf.robotContainer.transform.Find("Left_Upper_Arm_Joint_01").gameObject;
            right_thigh_joint = avatarSelf.robotContainer.transform.Find("Right_Thigh_Joint_01").gameObject;
            left_thigh_joint = avatarSelf.robotContainer.transform.Find("Left_Thigh_Joint_01").gameObject;
        }
        right_forearm_joint = right_upper_arm_joint.transform.Find("Right_Forearm_Joint_01").gameObject;
        left_forearm_joint = left_upper_arm_joint.transform.Find("Left_Forearm_Joint_01").gameObject;
        right_knee_joint = right_thigh_joint.transform.Find("Right_Knee_Joint_01").gameObject;
        left_knee_joint = left_thigh_joint.transform.Find("Left_Knee_Joint_01").gameObject;

        Quaternion rootTransform = Quaternion.FromToRotation(robotRoot.transform.rotation.eulerAngles, avatarSelf.stickContainer.transform.rotation.eulerAngles);
        Quaternion relativeTransform = Quaternion.FromToRotation(transform.up, avatarSelf.LeftUpperArm.transform.localRotation.eulerAngles);
        Vector3 ea = avatarSelf.RightUpperArm.transform.rotation.eulerAngles;

        if (set == true)
        {
            right_upper_arm_joint.transform.SetParent(avatarSelf.robotContainer.transform);
            left_upper_arm_joint.transform.SetParent(avatarSelf.robotContainer.transform);
            right_thigh_joint.transform.SetParent(avatarSelf.robotContainer.transform);
            left_thigh_joint.transform.SetParent(avatarSelf.robotContainer.transform);
        }
        right_upper_arm_joint.transform.localRotation = avatarSelf.RightUpperArm.transform.localRotation;
        right_upper_arm_joint.transform.Rotate(0, 0, 90);

        right_forearm_joint.transform.localRotation = Quaternion.Inverse(right_upper_arm_joint.transform.localRotation) * avatarSelf.RightLowerArm.transform.localRotation;
        right_forearm_joint.transform.Rotate(0, 0, 90);

        left_upper_arm_joint.transform.localRotation = avatarSelf.LeftUpperArm.transform.localRotation;
        left_upper_arm_joint.transform.Rotate(180, 0, 90);
        left_forearm_joint.transform.localRotation = Quaternion.Inverse(left_upper_arm_joint.transform.localRotation) * avatarSelf.LeftLowerArm.transform.localRotation;
        left_forearm_joint.transform.Rotate(180, 90, 45);

        left_thigh_joint.transform.localRotation = avatarSelf.LeftUpperLeg.transform.localRotation;
        left_thigh_joint.transform.Rotate(0, 0, 90);
        left_knee_joint.transform.localRotation = Quaternion.Inverse(left_thigh_joint.transform.localRotation) * avatarSelf.LeftKneeStick.transform.localRotation;
        left_knee_joint.transform.Rotate(0, 0, 170);

        right_thigh_joint.transform.localRotation = avatarSelf.RightUpperLeg.transform.localRotation;
        right_thigh_joint.transform.Rotate(0, 0, -90);
        right_knee_joint.transform.localRotation = Quaternion.Inverse(right_thigh_joint.transform.localRotation) * avatarSelf.RightKneeStick.transform.localRotation;
        right_knee_joint.transform.Rotate(180, 0, -170);
    }

    void moveSMPLPerson(JointDataListLive joint_data_list, AvatarGo avatarSelf)
    {
        JointDataLive stickJoint = joint_data_list.data[5];
        var stickPos = stickJoint.Position;
        var stickOrientation = stickJoint.Orientation;
        var stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        var stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftShoulderStick.transform.localPosition = stickV;
        avatarSelf.LeftShoulderStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[12];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightShoulderStick.transform.localPosition = stickV;
        avatarSelf.RightShoulderStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[18];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftHipStick.transform.localPosition = stickV;
        avatarSelf.LeftHipStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightHipStick.transform.localPosition = stickV;
        avatarSelf.RightHipStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[6];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftElbowStick.transform.localPosition = stickV;
        avatarSelf.LeftElbowStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[13];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightElbowStick.transform.localPosition = stickV;
        avatarSelf.RightElbowStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[7];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftWristStick.transform.localPosition = stickV;
        avatarSelf.LeftWristStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[14];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightWristStick.transform.localPosition = stickV;
        avatarSelf.RightWristStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[19];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftKneeStick.transform.localPosition = stickV;
        avatarSelf.LeftKneeStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[23];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightKneeStick.transform.localPosition = stickV;
        avatarSelf.RightKneeStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[20];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftAnkleStick.transform.localPosition = stickV;
        avatarSelf.LeftAnkleStick.transform.localRotation = stickR;

        stickJoint = joint_data_list.data[24];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightAnkleStick.transform.localPosition = stickV;
        avatarSelf.RightAnkleStick.transform.localRotation = stickR;


        /************************************Head**************************************/
        stickJoint = joint_data_list.data[28];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftEye.transform.localPosition = stickV;
        avatarSelf.LeftEye.transform.localRotation = stickR;
        avatarSelf.LeftEye.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);
        //LeftEye.transform.Rotate(90, 90, 0);

        stickJoint = joint_data_list.data[30];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightEye.transform.localPosition = stickV;
        avatarSelf.RightEye.transform.localRotation = stickR;
        avatarSelf.RightEye.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);
        //RightEye.transform.Rotate(90, 90, 0);

        stickJoint = joint_data_list.data[27];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.NoseStick.transform.localPosition = stickV;
        avatarSelf.NoseStick.transform.localRotation = stickR;
        avatarSelf.NoseStick.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        stickJoint = joint_data_list.data[29];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftEar.transform.localPosition = stickV;
        avatarSelf.LeftEar.transform.localRotation = stickR;
        avatarSelf.LeftEar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        avatarSelf.LeftEar.transform.LookAt(avatarSelf.LeftShoulderStick.transform.position);
        avatarSelf.LeftEar.transform.Rotate(90, 0, 0);

        stickJoint = joint_data_list.data[31];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightEar.transform.localPosition = stickV;
        avatarSelf.RightEar.transform.localRotation = stickR;
        avatarSelf.RightEar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        avatarSelf.RightEar.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.RightEar.transform.Rotate(90, 0, 0);

        stickJoint = joint_data_list.data[27];
        stickPos = stickJoint.Position;
        stickOrientation = stickJoint.Orientation;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.MouthStick.transform.localPosition = stickV;
        avatarSelf.MouthStick.transform.localRotation = stickR;
        avatarSelf.MouthStick.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);


        /************************************Body**************************************/
        stickJoint = joint_data_list.data[5];
        var stickJoint_b = joint_data_list.data[12];
        stickPos = stickJoint.Position;
        var stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        Vector3 stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        float stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.Shoulders.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.Shoulders.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.Shoulders.transform.Rotate(90, 0, 0);
        avatarSelf.Shoulders.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.HipStick.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.HipStick.transform.LookAt(avatarSelf.RightHipStick.transform.position);
        avatarSelf.HipStick.transform.Rotate(90, 0, 0);
        avatarSelf.HipStick.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[5];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.TorsoLeft.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.TorsoLeft.transform.LookAt(avatarSelf.LeftShoulderStick.transform.position);
        avatarSelf.TorsoLeft.transform.Rotate(90, 0, 0);
        avatarSelf.TorsoLeft.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

        stickJoint = joint_data_list.data[12];
        stickJoint_b = joint_data_list.data[22];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.TorsoRight.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.TorsoRight.transform.LookAt(avatarSelf.RightShoulderStick.transform.position);
        avatarSelf.TorsoRight.transform.Rotate(90, 0, 0);
        avatarSelf.TorsoRight.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);


        /************************************Arms**************************************/
        stickJoint = joint_data_list.data[5];
        stickJoint_b = joint_data_list.data[6];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftUpperArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftUpperArm.transform.LookAt(avatarSelf.LeftElbowStick.transform.position);
        avatarSelf.LeftUpperArm.transform.Rotate(90, 0, 0);
        avatarSelf.LeftUpperArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[12];
        stickJoint_b = joint_data_list.data[13];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightUpperArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightUpperArm.transform.LookAt(avatarSelf.RightElbowStick.transform.position);
        avatarSelf.RightUpperArm.transform.Rotate(90, 0, 0);
        avatarSelf.RightUpperArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[6];
        stickJoint_b = joint_data_list.data[7];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftLowerArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        //LeftLowerArm.transform.LookAt(stickV);
        avatarSelf.LeftLowerArm.transform.LookAt(avatarSelf.LeftWristStick.transform.position);
        avatarSelf.LeftLowerArm.transform.Rotate(90, 0, 0);
        avatarSelf.LeftLowerArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[13];
        stickJoint_b = joint_data_list.data[14];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightLowerArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightLowerArm.transform.LookAt(avatarSelf.RightWristStick.transform.position);
        avatarSelf.RightLowerArm.transform.Rotate(90, 0, 0);
        avatarSelf.RightLowerArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);


        /************************************Legs**************************************/
        stickJoint = joint_data_list.data[18];
        stickJoint_b = joint_data_list.data[19];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftUpperLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftUpperLeg.transform.LookAt(avatarSelf.LeftHipStick.transform.position);
        avatarSelf.LeftUpperLeg.transform.Rotate(90, 0, 0);
        avatarSelf.LeftUpperLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[22];
        stickJoint_b = joint_data_list.data[23];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightUpperLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightUpperLeg.transform.LookAt(avatarSelf.RightHipStick.transform.position);
        avatarSelf.RightUpperLeg.transform.Rotate(90, 0, 0);
        avatarSelf.RightUpperLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[19];
        stickJoint_b = joint_data_list.data[20];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.LeftLowerLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.LeftLowerLeg.transform.LookAt(avatarSelf.LeftKneeStick.transform.position);
        avatarSelf.LeftLowerLeg.transform.Rotate(90, 0, 0);
        avatarSelf.LeftLowerLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

        stickJoint = joint_data_list.data[23];
        stickJoint_b = joint_data_list.data[24];
        stickPos = stickJoint.Position;
        stickPos_b = stickJoint_b.Position;
        stickOrientation = stickJoint.Orientation;
        stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
        stick_length = stick.magnitude * 0.002f;
        stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
        stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
        avatarSelf.RightLowerLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
        avatarSelf.RightLowerLeg.transform.LookAt(avatarSelf.RightKneeStick.transform.position);
        avatarSelf.RightLowerLeg.transform.Rotate(90, 0, 0);
        avatarSelf.RightLowerLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);


        // get SMPL body parts
        GameObject smpl_body;
        GameObject smpl_male = avatarSelf.smplContainer.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
        GameObject smpl_female = avatarSelf.smplContainer.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
        GameObject L_hip, R_hip, L_Shoulder, R_Shoulder, R_Elbow, L_Elbow, L_Knee, R_Knee;
        bool set = false;
        if (smpl_male.activeSelf == true)
        {
            smpl_body = smpl_male;
            //GameObject L_hip, R_hip, L_Shoulder, R_Shoulder;
            GameObject SMPLRoot = smpl_body.transform.Find("m_avg_root").gameObject;
            GameObject pelvis = SMPLRoot.transform.Find("m_avg_Pelvis").gameObject;

            GameObject Spine1 = pelvis.transform.Find("m_avg_Spine1").gameObject;
            GameObject Spine2 = Spine1.transform.Find("m_avg_Spine2").gameObject;
            GameObject Spine3 = Spine2.transform.Find("m_avg_Spine3").gameObject;

            // we don't need the spine
            GameObject L_Collar = Spine3.transform.Find("m_avg_L_Collar").gameObject;

            // we don't need the hand
            GameObject R_Collar = Spine3.transform.Find("m_avg_R_Collar").gameObject;

            GameObject Neck = Spine3.transform.Find("m_avg_Neck").gameObject;

            if (L_Collar.transform.childCount == 1)
            {
                set = true;
                L_Shoulder = L_Collar.transform.Find("m_avg_L_Shoulder").gameObject;
                R_Shoulder = R_Collar.transform.Find("m_avg_R_Shoulder").gameObject;
                L_hip = pelvis.transform.Find("m_avg_L_Hip").gameObject;
                R_hip = pelvis.transform.Find("m_avg_R_Hip").gameObject;
            }
            else
            {
                L_Shoulder = avatarSelf.smplContainer.transform.Find("m_avg_L_Shoulder").gameObject;
                R_Shoulder = avatarSelf.smplContainer.transform.Find("m_avg_R_Shoulder").gameObject;
                L_hip = avatarSelf.smplContainer.transform.Find("m_avg_L_Hip").gameObject;
                R_hip = avatarSelf.smplContainer.transform.Find("m_avg_R_Hip").gameObject;
            }

            L_Knee = L_hip.transform.Find("m_avg_L_Knee").gameObject;
            GameObject L_Ankle = L_Knee.transform.Find("m_avg_L_Ankle").gameObject;
            // we don't need the foot and the ankle not much either

            R_Knee = R_hip.transform.Find("m_avg_R_Knee").gameObject;
            GameObject R_Ankle = R_Knee.transform.Find("m_avg_R_Ankle").gameObject;

            L_Elbow = L_Shoulder.transform.Find("m_avg_L_Elbow").gameObject;
            GameObject L_Wrist = L_Elbow.transform.Find("m_avg_L_Wrist").gameObject;
            R_Elbow = R_Shoulder.transform.Find("m_avg_R_Elbow").gameObject;
            GameObject R_Wrist = R_Elbow.transform.Find("m_avg_R_Wrist").gameObject;

        }
        else
        {
            smpl_body = smpl_female;
            //GameObject L_hip, R_hip, L_Shoulder, R_Shoulder;
            GameObject SMPLRoot = smpl_body.transform.Find("f_avg_root").gameObject;
            GameObject pelvis = SMPLRoot.transform.Find("f_avg_Pelvis").gameObject;

            GameObject Spine1 = pelvis.transform.Find("f_avg_Spine1").gameObject;
            GameObject Spine2 = Spine1.transform.Find("f_avg_Spine2").gameObject;
            GameObject Spine3 = Spine2.transform.Find("f_avg_Spine3").gameObject;

            // we don't need the spine
            GameObject L_Collar = Spine3.transform.Find("f_avg_L_Collar").gameObject;

            // we don't need the hand
            GameObject R_Collar = Spine3.transform.Find("f_avg_R_Collar").gameObject;

            GameObject Neck = Spine3.transform.Find("f_avg_Neck").gameObject;

            //right_shoulder_joint = ribs.transform.Find("Right_Shoulder_Joint_01").gameObject;
            //bool set = false;
            if (L_Collar.transform.childCount == 1)
            {
                set = true;
                L_Shoulder = L_Collar.transform.Find("f_avg_L_Shoulder").gameObject;
                R_Shoulder = R_Collar.transform.Find("f_avg_R_Shoulder").gameObject;
                L_hip = pelvis.transform.Find("f_avg_L_Hip").gameObject;
                R_hip = pelvis.transform.Find("f_avg_R_Hip").gameObject;
            }
            else
            {
                L_Shoulder = avatarSelf.smplContainer.transform.Find("f_avg_L_Shoulder").gameObject;
                R_Shoulder = avatarSelf.smplContainer.transform.Find("f_avg_R_Shoulder").gameObject;
                L_hip = avatarSelf.smplContainer.transform.Find("f_avg_L_Hip").gameObject;
                R_hip = avatarSelf.smplContainer.transform.Find("f_avg_R_Hip").gameObject;
            }
            L_Knee = L_hip.transform.Find("f_avg_L_Knee").gameObject;
            GameObject L_Ankle = L_Knee.transform.Find("f_avg_L_Ankle").gameObject;
            // we don't need the foot and the ankle not much either

            R_Knee = R_hip.transform.Find("f_avg_R_Knee").gameObject;
            GameObject R_Ankle = R_Knee.transform.Find("f_avg_R_Ankle").gameObject;

            L_Elbow = L_Shoulder.transform.Find("f_avg_L_Elbow").gameObject;
            GameObject L_Wrist = L_Elbow.transform.Find("f_avg_L_Wrist").gameObject;
            R_Elbow = R_Shoulder.transform.Find("f_avg_R_Elbow").gameObject;
            GameObject R_Wrist = R_Elbow.transform.Find("f_avg_R_Wrist").gameObject;
        }


        if (set == true)
        {
            L_Shoulder.transform.SetParent(avatarSelf.smplContainer.transform);
            R_Shoulder.transform.SetParent(avatarSelf.smplContainer.transform);
            L_hip.transform.SetParent(avatarSelf.smplContainer.transform);
            R_hip.transform.SetParent(avatarSelf.smplContainer.transform);
        }
        R_Shoulder.transform.localRotation = avatarSelf.RightUpperArm.transform.localRotation;
        R_Shoulder.transform.Rotate(0, 180, 90);
        R_Elbow.transform.localRotation = Quaternion.Inverse(R_Shoulder.transform.localRotation) * avatarSelf.RightLowerArm.transform.localRotation;
        R_Elbow.transform.Rotate(0, 180, 90);

        L_Shoulder.transform.localRotation = avatarSelf.LeftUpperArm.transform.localRotation;
        L_Shoulder.transform.Rotate(180, 0, 90);
        L_Elbow.transform.localRotation = Quaternion.Inverse(L_Shoulder.transform.localRotation) * avatarSelf.LeftLowerArm.transform.localRotation;
        L_Elbow.transform.Rotate(180, 0, 90);

        L_hip.transform.localRotation = avatarSelf.LeftUpperLeg.transform.localRotation;
        L_hip.transform.Rotate(0, 90, 0);
        L_Knee.transform.localRotation = Quaternion.Inverse(L_hip.transform.localRotation) * avatarSelf.LeftKneeStick.transform.localRotation;
        L_Knee.transform.Rotate(-90, 180, -90);

        R_hip.transform.localRotation = avatarSelf.RightUpperLeg.transform.localRotation;
        R_hip.transform.Rotate(0, 90, 0);
        R_Knee.transform.localRotation = Quaternion.Inverse(R_hip.transform.localRotation) * avatarSelf.RightKneeStick.transform.localRotation;
        R_Knee.transform.Rotate(90, 180, 90);
    }

    void showCorrection(AvatarGo avatarSelf, AvatarGo avatarTeacher)
    {
        float LeftUpperArm_difference = Quaternion.Angle(avatarSelf.LeftUpperArm.transform.localRotation, avatarTeacher.LeftUpperArm.transform.localRotation);
        float LeftLowerArm_difference = Quaternion.Angle(avatarSelf.LeftLowerArm.transform.localRotation, avatarTeacher.LeftLowerArm.transform.localRotation);
        float RightUpperArm_difference = Quaternion.Angle(avatarSelf.RightUpperArm.transform.localRotation, avatarTeacher.RightUpperArm.transform.localRotation);
        float RightLowerArm_difference = Quaternion.Angle(avatarSelf.RightLowerArm.transform.localRotation, avatarTeacher.RightLowerArm.transform.localRotation);
        float LeftUpperLeg_difference = Quaternion.Angle(avatarSelf.LeftUpperLeg.transform.localRotation, avatarTeacher.LeftUpperLeg.transform.localRotation);
        float LeftLowerLeg_difference = Quaternion.Angle(avatarSelf.LeftLowerLeg.transform.localRotation, avatarTeacher.LeftLowerLeg.transform.localRotation);
        float RightUpperLeg_difference = Quaternion.Angle(avatarSelf.RightUpperLeg.transform.localRotation, avatarTeacher.RightUpperLeg.transform.localRotation);
        float RightLowerLeg_difference = Quaternion.Angle(avatarSelf.RightLowerLeg.transform.localRotation, avatarTeacher.RightLowerLeg.transform.localRotation);
        float thresh = correctionThresh;

        if (avatarSelf.stickContainer.activeSelf)
        {
            avatarSelf.LeftUpperArm.GetComponent<Renderer>().material = (LeftUpperArm_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.LeftLowerArm.GetComponent<Renderer>().material = (LeftLowerArm_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.RightUpperArm.GetComponent<Renderer>().material = (RightUpperArm_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.RightLowerArm.GetComponent<Renderer>().material = (RightLowerArm_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.LeftUpperLeg.GetComponent<Renderer>().material = (LeftUpperLeg_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.LeftLowerLeg.GetComponent<Renderer>().material = (LeftLowerLeg_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.RightUpperLeg.GetComponent<Renderer>().material = (RightUpperLeg_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.RightLowerLeg.GetComponent<Renderer>().material = (RightLowerLeg_difference > thresh) ? incorrect_material : correct_material;
        }
    }

    JointDataList convertBodyToJDL(Body body)
    {
        JointData[] joint_data_array = new JointData[(int)JointType.Count];
        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            // write recorded poses to file
            var joint = body.Skeleton.Joints[(int)jt];
            var pos = joint.Position;
            var orientation = joint.Orientation;
            // save raw data
            var v = new Vector3(pos.X, pos.Y, pos.Z);
            var r = new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);
            var joint_data = new JointData { position = JsonUtility.ToJson(v), rotation = JsonUtility.ToJson(r) };
            joint_data_array[(int)jt] = joint_data;
        }
        JointDataList jdl = new JointDataList { data = joint_data_array };
        return jdl;
    }

    JointDataListLive JSON_to_JDL_Live(string frame_json)
    {
        JointDataLive[] joint_data_recorded_array = new JointDataLive[(int)JointType.Count];
        JointDataListLive recorded_data = new JointDataListLive { data = { } };

        Debug.Log(frame_json);
        saved_joint_data = JsonUtility.FromJson<JointDataList>(frame_json);
        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            // play recording
            JointData jd = saved_joint_data.data[(int)jt];
            Vector3 v_saved = JsonUtility.FromJson<Vector3>(jd.position);
            Quaternion r_saved = JsonUtility.FromJson<Quaternion>(jd.rotation);
            var joint_data_live = new JointDataLive
            {
                Position = v_saved,
                Orientation = r_saved
            };
            joint_data_recorded_array[(int)jt] = joint_data_live;
        }
        recorded_data = new JointDataListLive { data = joint_data_recorded_array };
        return recorded_data;
    }

    JointDataListLive Body_to_JDL_Live(Body body)
    {
        JointDataLive[] joint_data_live_array = new JointDataLive[(int)JointType.Count];
        JointDataListLive live_data = new JointDataListLive { data = { } };

        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            var stickJoint = body.Skeleton.Joints[(int)jt];
            var stickPos = stickJoint.Position;
            var stickOrientation = stickJoint.Orientation;
            var joint_data_live = new JointDataLive
            {
                Position = new Vector3(stickPos.X, stickPos.Y, stickPos.Z),
                Orientation = new Quaternion(stickOrientation.X, stickOrientation.Y, stickOrientation.Z, stickOrientation.W)
            };
            joint_data_live_array[(int)jt] = joint_data_live;
        }
        live_data = new JointDataListLive { data = joint_data_live_array };
        return live_data;
    }

    JointDataListLive Remote_to_JDL_Live(RemoteJointList rjl)
    {
        // format in  lightweight-human-pose-estimation-3d-demo.pytorch
        /*
        0: Neck
        1: Nose
        2: BodyCenter(center of hips)
        3: lShoulder
        4: lElbow
        5: lWrist,
        6: lHip
        7: lKnee
        8: lAnkle
        9: rShoulder
        10: rElbow
        11: rWrist
        12: rHip
        13: rKnee
        14: rAnkle
        15: rEye
        16: lEye
        17: rEar
        18: lEar
        */

        // format in Kinect Body Tracking SDK
        // HipLeft 18 KneeLeft 19 AnkleLeft 20
        // HipRight 22 KneeRight 23 AnkleRight 24
        // ShoulderLeft 5
        // ShoulderRight 12
        // HipLeft 18 HipRight 22
        // ElbowLeft 6 WristLeft 7
        // ElbowRight 13 WristRight 14
        // LeftEye 28
        // RightEye 30
        // Neck 3
        // Nose 27
        // Left Ear 29
        // Right Ear 31 

        IDictionary<int, int> dict = new Dictionary<int, int>();
        // map kinect keys to Pytorch keys
        // in total 31 kinect and 18 Pytorch keys
        dict.Add(18, 6);
        dict.Add(19, 7);
        dict.Add(20, 8);
        dict.Add(22, 12);
        dict.Add(23, 13);
        dict.Add(24, 14);
        dict.Add(5, 3);
        dict.Add(12, 9);
        dict.Add(6, 4);
        dict.Add(7, 5);
        dict.Add(13, 10);
        dict.Add(14, 11);
        dict.Add(28, 16);
        dict.Add(30, 15);
        dict.Add(3, 0);
        dict.Add(27, 1);
        dict.Add(29, 18);
        dict.Add(31, 17);
        JointDataLive[] joint_data_received_array = new JointDataLive[(int)JointType.Count];

        Debug.Log(JsonUtility.ToJson(rjl));
        List<List<double>> joint_data = rjl.values;
        Debug.Log(joint_data);
        
        float scaling = 10.0f;
        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            if (dict.ContainsKey((int)jt))
            {
                int pytorch_index = dict[(int)jt];
                List<double> jtd = joint_data[pytorch_index];
                Debug.Log(jtd);
                Vector3 v_rcv = new Vector3(scaling * (float)jtd[0], scaling * (float)jtd[1], scaling * (float)jtd[2]);
                Quaternion r_rcv = new Quaternion(0, 0, 0, 0);
                var joint_data_live = new JointDataLive
                {
                    Position = v_rcv,
                    Orientation = r_rcv
                };
                joint_data_received_array[(int)jt] = joint_data_live;
            }
            else
            {   // just fill empty value as placeholder
                Vector3 v_rcv = new Vector3(0,0,0);
                Quaternion r_rcv = new Quaternion(0, 0, 0, 0);
                var joint_data_live = new JointDataLive
                {
                    Position = v_rcv,
                    Orientation = r_rcv
                };
                joint_data_received_array[(int)jt] = joint_data_live;
            }
           
        }
        JointDataListLive received_data = new JointDataListLive { data = joint_data_received_array };
        return received_data;
    }

    void appendRecordedFrame(JointDataList jdl, AvatarGo avatarSelf, string filename)
    {
        string json = JsonUtility.ToJson(jdl) + Environment.NewLine;
        File.AppendAllText(filename, json);
    }

    public void loadRecording(string filename = "")
    {
        // read recording file
        if (filename == "")
        {
            filename = current_file;
        }
        read_recording_data = File.ReadLines(filename);
        sequenceEnum = read_recording_data.GetEnumerator();
        Debug.Log(read_recording_data);
    }

    void resetRecording(string filename = "")
    {
        // reset recording file
        if (filename == "")
        {
            filename = current_file;
        }
        File.WriteAllText(filename, "");
        Debug.Log("reset recording file");
    }

    void animateAvatars(AvatarGo avatarSelf, JointDataListLive live_data)
    {
        if (avatarSelf.stickContainer.activeSelf)
        {
            moveStickPerson(live_data, avatarSelf);
            moveStickPerson(live_data, avatarSelf2);

            avatarSelf2.stickContainer.gameObject.SetActive(true);
            avatarSelf2.cubeContainer.gameObject.SetActive(false);
            avatarSelf2.robotContainer.gameObject.SetActive(false);
            avatarSelf2.smplContainer.gameObject.SetActive(false);

            avatarTeacher.stickContainer.gameObject.SetActive(true);
            avatarTeacher.cubeContainer.gameObject.SetActive(false);
            avatarTeacher.robotContainer.gameObject.SetActive(false);
            avatarTeacher.smplContainer.gameObject.SetActive(false);

            avatarTeacher2.stickContainer.gameObject.SetActive(true);
            avatarTeacher2.cubeContainer.gameObject.SetActive(false);
            avatarTeacher2.robotContainer.gameObject.SetActive(false);
            avatarTeacher2.smplContainer.gameObject.SetActive(false);

        }
        else if (avatarSelf.cubeContainer.activeSelf)
        {
            moveCubePerson(live_data, avatarSelf);
            moveCubePerson(live_data, avatarSelf2);

            avatarSelf2.stickContainer.gameObject.SetActive(false);
            avatarSelf2.cubeContainer.gameObject.SetActive(true);
            avatarSelf2.robotContainer.gameObject.SetActive(false);
            avatarSelf2.smplContainer.gameObject.SetActive(false);

            avatarTeacher.stickContainer.gameObject.SetActive(false);
            avatarTeacher.cubeContainer.gameObject.SetActive(true);
            avatarTeacher.robotContainer.gameObject.SetActive(false);
            avatarTeacher.smplContainer.gameObject.SetActive(false);

            avatarTeacher2.stickContainer.gameObject.SetActive(false);
            avatarTeacher2.cubeContainer.gameObject.SetActive(true);
            avatarTeacher2.robotContainer.gameObject.SetActive(false);
            avatarTeacher2.smplContainer.gameObject.SetActive(false);

        }
        else if (avatarSelf.robotContainer.activeSelf)
        {
            moveRobotPerson(live_data, avatarSelf);
            moveRobotPerson(live_data, avatarSelf2);

            avatarSelf2.stickContainer.gameObject.SetActive(false);
            avatarSelf2.cubeContainer.gameObject.SetActive(false);
            avatarSelf2.robotContainer.gameObject.SetActive(true);
            avatarSelf2.smplContainer.gameObject.SetActive(false);

            avatarTeacher.stickContainer.gameObject.SetActive(false);
            avatarTeacher.cubeContainer.gameObject.SetActive(false);
            avatarTeacher.robotContainer.gameObject.SetActive(true);
            avatarTeacher.smplContainer.gameObject.SetActive(false);

            avatarTeacher2.stickContainer.gameObject.SetActive(false);
            avatarTeacher2.cubeContainer.gameObject.SetActive(false);
            avatarTeacher2.robotContainer.gameObject.SetActive(true);
            avatarTeacher2.smplContainer.gameObject.SetActive(false);

        }
        else if (avatarSelf.smplContainer.activeSelf)
        {
            moveSMPLPerson(live_data, avatarSelf);
            moveSMPLPerson(live_data, avatarSelf2);

            GameObject SMPL_male = avatarSelf.smplContainer.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_female = avatarSelf.smplContainer.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_male2 = avatarSelf2.smplContainer.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_female2 = avatarSelf2.smplContainer.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_maleT = avatarTeacher.smplContainer.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_femaleT = avatarTeacher.smplContainer.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_maleT2 = avatarTeacher2.smplContainer.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_femaleT2 = avatarTeacher2.smplContainer.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            if (isMaleSMPL)
            // It checks here whether male or female is selected in the settings
            // note: the surrounding section only gets executed if a body is found
            {
                SMPL_male.SetActive(true);
                SMPL_male2.SetActive(true);
                SMPL_maleT.SetActive(true);
                SMPL_maleT2.SetActive(true);
                SMPL_female.SetActive(false);
                SMPL_female2.SetActive(false);
                SMPL_femaleT.SetActive(false);
                SMPL_femaleT2.SetActive(false);


            }
            else
            {
                SMPL_male.SetActive(false);
                SMPL_male2.SetActive(false);
                SMPL_maleT.SetActive(false);
                SMPL_maleT2.SetActive(false);
                SMPL_female.SetActive(true);
                SMPL_female2.SetActive(true);
                SMPL_femaleT.SetActive(true);
                SMPL_femaleT2.SetActive(true);
            }

            avatarSelf2.stickContainer.gameObject.SetActive(false);
            avatarSelf2.cubeContainer.gameObject.SetActive(false);
            avatarSelf2.robotContainer.gameObject.SetActive(false);
            avatarSelf2.smplContainer.gameObject.SetActive(true);

            avatarTeacher.stickContainer.gameObject.SetActive(false);
            avatarTeacher.cubeContainer.gameObject.SetActive(false);
            avatarTeacher.robotContainer.gameObject.SetActive(false);
            avatarTeacher.smplContainer.gameObject.SetActive(true);

            avatarTeacher2.stickContainer.gameObject.SetActive(false);
            avatarTeacher2.cubeContainer.gameObject.SetActive(false);
            avatarTeacher2.robotContainer.gameObject.SetActive(false);
            avatarTeacher2.smplContainer.gameObject.SetActive(true);

        }
    }
    void animateTeacher(AvatarGo avatarTeacher, JointDataListLive recorded_data)
    {
        if (avatarTeacher.stickContainer.activeSelf)
        {
            moveStickPerson(recorded_data, avatarTeacher);
            moveStickPerson(recorded_data, avatarTeacher2);
        }
        else if (avatarTeacher.cubeContainer.activeSelf)
        {
            moveCubePerson(recorded_data, avatarTeacher);
            moveCubePerson(recorded_data, avatarTeacher2);
        }
        else if (avatarTeacher.robotContainer.activeSelf)
        {
            moveRobotPerson(recorded_data, avatarTeacher);
            moveRobotPerson(recorded_data, avatarTeacher2);
        }
        else if (avatarTeacher.smplContainer.activeSelf)
        {
            moveSMPLPerson(recorded_data, avatarTeacher);
            moveSMPLPerson(recorded_data, avatarTeacher2);
        }
    }


    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
                websocket.DispatchMessageQueue();
        #endif

        checkKeyInput();

        if (usingKinectAlternative)
        {
            if (remote_joints_live != null)
            {
                JointDataListLive live_data = remote_joints_live;
                animateAvatars(avatarSelf, live_data);
            }
            else if(debugMode == true)
            {
                // use fake input for testing
                if (loadedFakeData == false)
                {
                    loadedFakeData = true;
                    loadRecording();
                }

                // use fake data
                sequenceEnum.MoveNext();
                string frame_json = sequenceEnum.Current;
                JointDataListLive fake_live_data = JSON_to_JDL_Live(frame_json);
                Debug.Log(avatarSelf);
                animateAvatars(avatarSelf, fake_live_data);
            }

        }
        else
        {
            using (Capture capture = device.GetCapture())
            {
                tracker.EnqueueCapture(capture);
                var color = capture.Color;
                if (color != null && color.WidthPixels > 0)
                {
                    UnityEngine.Object.Destroy(tex);
                    tex = new Texture2D(color.WidthPixels, color.HeightPixels, TextureFormat.BGRA32, false);
                    tex.LoadRawTextureData(color.GetBufferCopy());
                    tex.Apply();

                    //Fetch the RawImage component from the GameObject
                    if (streamCanvas != null)
                    {
                        m_RawImage = streamCanvas.GetComponent<RawImage>();
                        if (m_RawImage != null)
                        {
                            m_RawImage.texture = tex;
                            renderer.material.mainTexture = tex;
                        }
                    }

                }
            }

            using (BodyFrame frame = tracker.PopResult())
            {
                Debug.LogFormat("{0} bodies found.", frame.BodyCount);
                if (frame.BodyCount > 0)
                {
                    var bodies = frame.Bodies;
                    var body = bodies[0];

                    JointDataListLive live_data = Body_to_JDL_Live(body);
                    animateAvatars(avatarSelf, live_data);

                    if (recording_mode == 1) // recording
                    {
                        JointDataList jdl = convertBodyToJDL(body);
                        appendRecordedFrame(jdl, avatarSelf, current_file);
                    }
                }
            }
        }


        if (recording_mode == 2) // playback
        {

            if(sequenceEnum == null)
            {
                loadRecording();
            }

            // play recording
            // needs to be loaded with recording_mode 3 first
            if (playback_speed == 1) // x1
            {
                sequenceEnum.MoveNext();
            }
            else if (playback_speed == 2) // x0.5
            {
                playback_counter++;
                if (playback_counter >= 2)
                {
                    sequenceEnum.MoveNext();
                    playback_counter = 0;
                }

            }
            else //(playback_speed == 3) // x0.25
            {
                playback_counter++;
                if (playback_counter >= 4)
                {
                    sequenceEnum.MoveNext();
                    playback_counter = 0;
                }
            }

            string frame_json = sequenceEnum.Current;
            JointDataListLive recorded_data = JSON_to_JDL_Live(frame_json);
            animateTeacher(avatarTeacher, recorded_data);

            if (shouldCheckCorrectness)
            {
                showCorrection(avatarSelf, avatarTeacher);
            }

        }


    }

}

[Serializable]
class JointData
{
    public string position;
    public string rotation;
}

[Serializable]
class JointDataList
{
    public JointData[] data;
}

class JointDataLive
{
    public Vector3 Position;
    public Quaternion Orientation;
}

[Serializable]
class JointDataListLive
{
    public JointDataLive[] data;
}
