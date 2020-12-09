using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using NativeWebSocket;
using System.Security.Permissions;

namespace PoseTeacher
{

    // Label for various containers
    public enum PoseInputSource
    {
        KINECT, WEBSOCKET, FILE
    }

    public enum Difficulty
    {
        EASY, MEDIUM, HARD
    }

    [CLSCompliant(false)]
    public class PoseInputGetter
    {

        PoseInputSource CurrentPoseInputSource;
        bool recording = false;
        PoseData CurrentPose { get; set; }

        // Azure Kinect variables
        Device device;
        BodyTracker tracker;

        // Used for displaying RGB Kinect video
        public Renderer videoRenderer;
        RawImage m_RawImage;
        GameObject streamCanvas;
        GameObject videoCube;

        //Select a Texture in the Inspector to change it (unused)
        public Texture m_Texture;
        Texture2D tex;

        //Websocket variables
        WebSocket websocket;
        public string WS_ip = "ws://localhost:2567";
        private PoseData poseLiveWS = null; // newest pose data from WS TODO: merge with CurrentPose

        //File variables
        IEnumerator<string> SequenceEnum;
        private string _ReadDataPath;
        public string ReadDataPath {
            get { return _ReadDataPath; }
            set { _ReadDataPath = value; GetTotalPoseNumber(); LoadData(); } 
        }

        private int _TotalFilePoseNumber;
         public int TotalFilePoseNumber
        {
            get { return _TotalFilePoseNumber; }
        }
        private int _CurrentFilePoseNumber;
        public int CurrentFilePoseNumber
        {
            get { return _CurrentFilePoseNumber; }
        }

        public string WriteDataPath { get; set; }

        public PoseInputGetter(PoseInputSource InitPoseInputSource)
        {
            CurrentPoseInputSource = InitPoseInputSource;

            switch (CurrentPoseInputSource)
            {
                case PoseInputSource.KINECT:
                    StartAzureKinect();
                    break;
                
                case PoseInputSource.WEBSOCKET:
                    StartWebsocket();
                    break;

                case PoseInputSource.FILE:
                    break;
            }
        }

        private async void StartWebsocket()
        {
            // Contains code to instantiate a websocket to obtain pose data
            // Web socket is currently used only for Kinect Alternative

            websocket = new WebSocket(WS_ip);

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
                // If joint information is recieved, set poseLiveWS
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("WS message received: " + message);
                var remote_joints = PoseDataUtils.DeserializeRJL(message);
                Debug.Log(remote_joints);
                poseLiveWS = PoseDataUtils.Remote2PoseData(remote_joints);
            };

            // Keep sending messages at every 0.3s
            // TODO: if using websocket, make this work
            //InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

            await websocket.Connect();
        }
        // For testing websocket
        async void SendWebSocketMessage()
        {
            if (websocket.State == WebSocketState.Open)
            {
                // Sending text
                await websocket.SendText("Test message");
            }
        }

        private void StartAzureKinect()
        {
            // Print to log
            Debug.Log("Try loading device");
            this.device = Device.Open(0);
            var config = new DeviceConfiguration
            {
                ColorResolution = ColorResolution.r720p,
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_Unbinned
            };
            device.StartCameras(config);

            if (device != null)
            {
                Debug.Log("Found non-null device");
                Debug.Log(device);
            }

            var calibration = device.GetCalibration(config.DepthMode, config.ColorResolution);

            var trackerConfiguration = new TrackerConfiguration
            {
                SensorOrientation = SensorOrientation.OrientationDefault,
                CpuOnlyMode = false
            };
            this.tracker = BodyTracker.Create(calibration, trackerConfiguration);
            Debug.Log("Device loading finished");
        }

        private void GetTotalPoseNumber()
        {
            LoadData();
            int count = 0;
            while (SequenceEnum.MoveNext())
            {
                count++;
            }
            _TotalFilePoseNumber = count;
            _CurrentFilePoseNumber = 0;
        }

        private void LoadData()
        {
            SequenceEnum = File.ReadLines(ReadDataPath).GetEnumerator();
        }

        // Appends the passed pose (PoseDataJSON format) to the file as JSON
        void AppendRecordedFrame(PoseDataJSON jdl)
        {
            string json = JsonUtility.ToJson(jdl) + Environment.NewLine;
            File.AppendAllText(WriteDataPath, json);
        }
        
        // reset recording file
        void ResetRecording()
        {
            File.WriteAllText(WriteDataPath, "");
            Debug.Log("Reset recording file");
        }


        public PoseData GetNextPose()
        {
            switch (CurrentPoseInputSource)
            {
                case PoseInputSource.WEBSOCKET:
                    #if !UNITY_WEBGL || UNITY_EDITOR
                    websocket.DispatchMessageQueue();
                    #endif
                    // poseLiveWS is non-null if alternative is sending pose data over websocket
                    if (poseLiveWS != null)
                    {
                        // Assign last pose from websocket
                        // TODO: maybe animate in websocket code directly upon recieveing?
                        CurrentPose = poseLiveWS;
                    }
                    else
                    {
                        Debug.Log("No pose recieved from WebSocket!");
                        
                    }
                    break;

                case PoseInputSource.FILE:

                    _CurrentFilePoseNumber++;
                    // TODO: Only load once
                    // Quick and dirty way to loop (by reloading file)
                    if (SequenceEnum == null || !SequenceEnum.MoveNext())
                    {
                        LoadData();
                        SequenceEnum.MoveNext();
                        _CurrentFilePoseNumber = 1;
                    }


                    string frame_json = SequenceEnum.Current;
                    PoseData fake_live_data = PoseDataUtils.JSONstring2PoseData(frame_json);
                    CurrentPose = fake_live_data;
                    break;

                case PoseInputSource.KINECT:
                    if (device != null)
                    {
                        // TODO: move to function?
                        // potentially simplify by not using "using" scopes
                        using (Capture capture = device.GetCapture())
                        {
                            // Make tracker estimate body
                            tracker.EnqueueCapture(capture);

                            // Code for getting RGB image from camera
                            var color = capture.Color;
                            if (color != null && color.WidthPixels > 0)
                            {
                                UnityEngine.Object.Destroy(tex);// required to not keep old images in memory
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
                                        videoRenderer.material.mainTexture = tex;
                                    }
                                }

                            }
                        }

                        // Get pose estimate from tracker
                        using (BodyFrame frame = tracker.PopResult())
                        {
                            Debug.LogFormat("{0} bodies found.", frame.BodyCount);

                            //  At least one body found by Body Tracking
                            if (frame.BodyCount > 0)
                            {
                                // Use first estimated person, if mutiple are in the image
                                // !!! There are (probably) no guarantees on consisitent ordering between estimates
                                var bodies = frame.Bodies;
                                var body = bodies[0];

                                // Apply pose to user avatar(s)
                                PoseData live_data = PoseDataUtils.Body2PoseData(body);

                                if (recording) // recording
                                {
                                    PoseDataJSON jdl = PoseDataUtils.Body2PoseDataJSON(body);
                                    AppendRecordedFrame(jdl);
                                }
                                CurrentPose = live_data;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("device is null!");
                    }
                    break;
                    
            }
            return CurrentPose;
        }

        public async void Dispose()
        {
            if (websocket != null)
            {
                await websocket.Close();
            }
            if (tracker != null)
            {
                tracker.Dispose();
            }
            if (device != null)
            {
                device.Dispose();
            }
        }

    }

    // Main script
    [CLSCompliant(false)]
    public class PoseteacherMain : MonoBehaviour
    {

        PoseInputGetter SelfPoseInputGetter;
        PoseInputGetter TeacherPoseInputGetter;

        // Used for displaying RGB Kinect video
        public Renderer videoRenderer;
        GameObject streamCanvas;
        GameObject videoCube;

        // Refrences to containers in scene
        // TODO find good way to reference objects. @Unity might do this for us
        List<AvatarContainer> avatarListSelf;
        List<AvatarContainer> avatarListTeacher;
        public GameObject avatarPrefab;
        public GameObject avatarTPrefab;

        GameObject spatialAwarenessSystem;

        // Decides what action is being done with joints
        // TODO: 
        // Change to enum (!!! making this changes makes buttons in scene unusable)
        // Make setter functions? For loading file and reset recording, should be rewritten as separate functions, not values here
        // 3 and 4 are currently not used/implemented
        // 3 not necessary as load happens automatically at playback
        // 4 disabled to not accidentaly delete current file contents
        public int recording_mode = 0; // 0: not recording, 1: recording, 2: playback, 3: load_file, 4: reset_recording
        // 
        // TODO:
        // Change to enum? (!!! making this changes makes button playback speed change in scene unusable)
        public int playback_speed = 1; // 1: x1 , 2: x0.5, 4: x0.25
        private int playback_counter = 0;



        // File that is being read from
        string current_file = "jsondata/2020_05_26-22_55_32.txt";

        //For fake data 
        //string fake_file = "jsondata/2020_05_26-21_23_28.txt";
        //string fake_file = "jsondata/2020_05_26-22_55_32.txt";
        string fake_file = "jsondata/2020_05_27-00_01_59.txt";

        // can be changed in the UI
        // TODO: probaly change to using functions to toggle
        public bool isMaleSMPL = true;
        public bool usingKinectAlternative = true;
        public PoseInputSource SelfPoseInputSource = PoseInputSource.FILE;


        public bool mirroring = true; // can probably be changed to private (if no UI elements use it)

        // Used for showing if pose is correct
        public Material normal_material;
        public Material correct_material;
        public Material incorrect_material;
        public bool shouldCheckCorrectness = true;
        public float correctionThresh = 30.0f;

        public Difficulty difficulty = Difficulty.EASY;
        public void SetDifficulty(Difficulty newDifficulty)
        {
            difficulty = newDifficulty;
            // TODO change penalty and other stuff...
            switch (difficulty)
            {
                case Difficulty.EASY:
                    avatarSimilarity.penalty = 0.5;
                    break;
                case Difficulty.MEDIUM:
                    avatarSimilarity.penalty = 1.0;
                    break;
                case Difficulty.HARD:
                    avatarSimilarity.penalty = 1.5f;
                    break;
                default:
                    break;
            }
        }

        public List<AvatarContainer> GetSelfAvatarContainers()
        {
            return avatarListSelf;
        }
        public List<AvatarContainer> GetTeacherAvatarContainers()
        {
            return avatarListTeacher;
        }

        public void AddAvatar(bool self)
        {
            if (self)
            {
            //    GameObject avatarContainer = GameObject.Find("AvatarContainer");
                GameObject newAvatar = Instantiate(avatarPrefab);
                avatarListSelf.Add(new AvatarContainer(newAvatar));
            } else
            {
            //    GameObject avatarContainer = GameObject.Find("AvatarContainerT");
                GameObject newAvatar = Instantiate(avatarTPrefab);
                avatarListTeacher.Add(new AvatarContainer(newAvatar));
            }
        }
        // Used for pose similarity calculation
        public String similarityBodyNr = "total"; // "total", "top", "middle", "bottom"
        public int similaritySelfNr = 0; // self list element to compare
        public int similarityTeacherNr = 0; // teacher list element to compare
        public double similarityScore = 0; // similarity output value between 0 and 1 for defined body part
        public double similarityPenalty = 1.0; // exponential penalty coefficient to be applied when similarity of body part not optimal fit [0....inf] -> 0 every body part has same weight
        public bool similarityActivateKalman = true; // activate kalman 
        public double similarityKalmanQ = 0.000001; // Kalman process noise covariance (model strength)
        public double similarityKalmanR = 1.0; // Kalman sensor noise covariance (observation strength)
        public double similarityTotalScore = 0; // Total score
        public List<double> similarityScoreRaw; // similarity value for all body sticks
        public List<double> similarityWeightRaw; // weight value for all body sticks
        AvatarSimilarity avatarSimilarity;
        VisualisationSimilarity avatarVisualisationSimilarity;
        // RandomGraph randomGraph;


        // Mirror all avatar containers
        // TODO: Move code to AvatarContainer class (partial done)
        //      Conform to naming conventions
        // Consider: what to do with videoCube and streamCanvas?
        //      Do we really need this function, or we will just set this one-by-one?
        public void do_mirror()
        {

            if (mirroring == true)
            {
                mirroring = false;
                // TODO figure out how to handle selection of mirrored avatar
                foreach (AvatarContainer avatar in avatarListSelf)
                {
                    avatar.Mirror();
                }
                foreach (AvatarContainer avatar in avatarListTeacher)
                {
                    avatar.Mirror();
                }

                videoCube.transform.localScale = new Vector3(-1f, 0.6f, 0.1f);
                streamCanvas.transform.localScale = new Vector3(32f, 16f, 1f);
            }
            else
            {
                mirroring = true;
                foreach (AvatarContainer avatar in avatarListSelf)
                {
                    avatar.Mirror();
                }
                foreach (AvatarContainer avatar in avatarListTeacher)
                {
                    avatar.Mirror();
                }

                videoCube.transform.localScale = new Vector3(1f, 0.6f, 0.1f);
                streamCanvas.transform.localScale = new Vector3(-32f, 16f, 1f);
            }
        }

        // Setters used in UI, configured in Inspector tab of respective buttons
        // Changing the names could (shouldn't) break the link to UI
        // Changing function signatures (parameters) WILL break the link to UI
        public void set_SMPL(bool value)
        {
            isMaleSMPL = value;
        }
        //public void set_OpenPose(bool value)
        //{
        //    usingKinectAlternative = value;
        //}
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

        // For setting in menu
        // TODO maybe organize wrapper functions in separate class for menu?
        public void SetAvatarTypesCube() => SetAvatarTypes(AvatarType.CUBE);
        public void SetAvatarTypesStick() => SetAvatarTypes(AvatarType.STICK);
        public void SetAvatarTypesRobot() => SetAvatarTypes(AvatarType.ROBOT);
        public void SetAvatarTypesSMPL() => SetAvatarTypes(AvatarType.SMPL);

        // Set the avatar type for all AvatarGos
        public void SetAvatarTypes(AvatarType type)
        {
            foreach (AvatarContainer avatar in avatarListSelf)
            {
                avatar.ChangeActiveType(type);
            }

            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                avatar.ChangeActiveType(type);
            }
        }

        // Generate new filename with timestamp and set as file to write to
        public void gen_new_current_file()
        {
            string timestamp = DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
            current_file = "jsondata/" + timestamp + ".txt";
        }



        // Do once on scene startup
        private void Start()
        {
            Debug.Log("OnEnable");

            //StartWebsocket();

            // Get refrences to objects used to show RGB video (Kinect)
            streamCanvas = GameObject.Find("RawImage");
            videoCube = GameObject.Find("VideoCube");

            // Find avatar container objects, and initialize their respective AvatarContainer classes
            GameObject avatarContainer = GameObject.Find("AvatarContainer");
            GameObject avatarContainerT = GameObject.Find("AvatarContainerT");
            GameObject avatarContainer2 = GameObject.Find("AvatarContainer2");
            GameObject avatarContainerT2 = GameObject.Find("AvatarContainerT2");

            avatarListSelf = new List<AvatarContainer>();
            avatarListTeacher = new List<AvatarContainer>();
            avatarListSelf.Add(new AvatarContainer(avatarContainer));
            avatarListSelf.Add(new AvatarContainer(avatarContainer2));
            avatarListTeacher.Add(new AvatarContainer(avatarContainerT));
            avatarListTeacher.Add(new AvatarContainer(avatarContainerT2));

            // Set unused containers to inactive
            avatarListTeacher[0].avatarContainer.gameObject.SetActive(false);
            avatarListSelf[1].avatarContainer.gameObject.SetActive(false);
            avatarListTeacher[1].avatarContainer.gameObject.SetActive(false);

            SelfPoseInputGetter = new PoseInputGetter(SelfPoseInputSource){ ReadDataPath = fake_file };
            TeacherPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE){ ReadDataPath = current_file};


            // initialize similarity calculation instance and assign selected avatars
            avatarSimilarity = new AvatarSimilarity(avatarListSelf[similaritySelfNr], avatarListTeacher[similarityTeacherNr], similarityBodyNr, similarityPenalty, similarityActivateKalman, similarityKalmanQ, similarityKalmanR);
            avatarVisualisationSimilarity = new VisualisationSimilarity(avatarListSelf[similaritySelfNr]);
           //  randomGraph = new RandomGraph();
        }


        // Done at each application update
        void Update()
        {
            checkKeyInput();

            AnimateSelf(SelfPoseInputGetter.GetNextPose());
       //avatarSelf.stickContainer.stick.activeSelf
            // avatarListSelf[0].stickContainer.LeftUpperArm.GetComponent<Renderer>().material.color = Color.red;
           // AnimateSelf(TeacherPoseInputGetter.GetNextPose());
            // Get pose similarity
            avatarSimilarity.Update(); // update similarity calculation with each update loop step
            similarityScore = avatarSimilarity.similarityBodypart; // get single similarity score for selected body part
            similarityScoreRaw = avatarSimilarity.similarityStick; // get similarity score for each stick element
            similarityWeightRaw = avatarSimilarity.stickWeight; // get similarity score for each stick element
            similarityTotalScore = (int)avatarSimilarity.totalScore; // get total Score

            //avatarVisualisationSimilarity.Update(similarityScoreRaw);// avatarVisualisationSimilarity.Update(similarityScore);
            avatarVisualisationSimilarity.UpdatePart(similarityBodyNr, similarityScore);
            // randomGraph.Update();
            // Playback for teacher avatar(s)
            if (recording_mode == 2) // playback
            {

                // play recording at different speeds
                // skip pose update if counter isn't big enought
                // TODO: maybe rewrite...
                if (playback_speed == 1) // x1
                {
                    AnimateTeacher(TeacherPoseInputGetter.GetNextPose());
                }
                else if (playback_speed == 2) // x0.5
                {
                    playback_counter++;
                    if (playback_counter >= 2)
                    {
                        AnimateTeacher(TeacherPoseInputGetter.GetNextPose());
                        playback_counter = 0;
                    }
                }
                else //(playback_speed == 3) // x0.25
                {
                    playback_counter++;
                    if (playback_counter >= 4)
                    {
                        AnimateTeacher(TeacherPoseInputGetter.GetNextPose());
                        playback_counter = 0;
                    }
                }

                // TODO: Put this in AvatarContainer?
                if (shouldCheckCorrectness)
                {
                    showCorrection(avatarListSelf[0], avatarListTeacher[0]);
                }

            }

            UpdateIndicators();
            
        }


        private float scaleScore(float score)
        {
            float min = 0.5F;

            float scaled_score = (float)(similarityScore - min) /(1-min);
            scaled_score = scaled_score < 0 ? 0 : scaled_score;
            scaled_score = scaled_score > 1 ? 1 : scaled_score;
            return scaled_score;
        }

        public void ActivateIndicators()
        {
            foreach (AvatarContainer avatar in avatarListSelf)
            {
                Transform scoreIndicatorTr = avatar.avatarContainer.transform.Find("ScoreIndicator");
                if (scoreIndicatorTr != null)
                {
                    GameObject scoreIndicator = scoreIndicatorTr.gameObject;
                    scoreIndicator.SetActive(true);

                }

                Transform pulsingObjectTr = avatar.avatarContainer.transform.Find("PulsingCube");
                if (pulsingObjectTr != null)
                {
                    GameObject pulseObject = pulsingObjectTr.gameObject;
                    pulseObject.SetActive(true);
                }

            }
            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                Transform progressIndicatorTr = avatar.avatarContainer.transform.Find("ProgressIndicator");
                if (progressIndicatorTr != null)
                {
                    GameObject progressIndicator = progressIndicatorTr.gameObject;
                    progressIndicator.SetActive(true);
                }

            }
        }

        public void DeActivateIndicators()
        {
            foreach (AvatarContainer avatar in avatarListSelf)
            {
                Transform scoreIndicatorTr = avatar.avatarContainer.transform.Find("ScoreIndicator");
                if (scoreIndicatorTr != null)
                {
                    GameObject scoreIndicator = scoreIndicatorTr.gameObject;
                    scoreIndicator.SetActive(false);

                }

                Transform pulsingObjectTr = avatar.avatarContainer.transform.Find("PulsingCube");
                if (pulsingObjectTr != null)
                {
                    GameObject pulseObject = pulsingObjectTr.gameObject;
                    pulseObject.SetActive(false);
                }

            }
            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                Transform progressIndicatorTr = avatar.avatarContainer.transform.Find("ProgressIndicator");
                if (progressIndicatorTr != null)
                {
                    GameObject progressIndicator = progressIndicatorTr.gameObject;
                    progressIndicator.SetActive(false);
                }

            }
        }

        private void UpdateIndicators()
        {
            foreach (AvatarContainer avatar in avatarListSelf)
            {
                Transform scoreIndicatorTr = avatar.avatarContainer.transform.Find("ScoreIndicator");
                if (scoreIndicatorTr != null)
                {
                    GameObject scoreIndicator = scoreIndicatorTr.gameObject;
                    if (scoreIndicator.activeSelf)
                    {
                        float progress = scaleScore((float)similarityScore);
                        scoreIndicator.GetComponent<ProgressIndicator>().SetProgress(progress);
                    }
                }

                Transform pulsingObjectTr = avatar.avatarContainer.transform.Find("PulsingCube");
                if (pulsingObjectTr != null)
                {
                    GameObject pulseObject = pulsingObjectTr.gameObject;
                    if (pulseObject.activeSelf)
                    {
                        // TODO make cube pulse depending on frames and stuff...
                    }
                }

            }
            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                Transform progressIndicatorTr = avatar.avatarContainer.transform.Find("ProgressIndicator");
                if (progressIndicatorTr != null)
                {
                    GameObject progressIndicator = progressIndicatorTr.gameObject;
                    if (progressIndicator.activeSelf)
                    {
                        // TODO use correct progress...
                        float progress = (float)TeacherPoseInputGetter.CurrentFilePoseNumber / TeacherPoseInputGetter.TotalFilePoseNumber;
                        progressIndicator.GetComponent<ProgressIndicator>().SetProgress(progress);
                    }
                }

            }
            
            // TODO update other indicators too
        }

        // Actions to do before quitting application
        private void OnApplicationQuit()
        {
            SelfPoseInputGetter.Dispose();
            TeacherPoseInputGetter.Dispose();
        }

        // Change recording mode via keyboard input for debugging and to not need menu
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


        // Change material of stickContainer avatar if not close enough to teacher pose
        // CONSIDER: move (part of it) to AvatarContainer or Containers
        void showCorrection(AvatarContainer avatarSelf, AvatarContainer avatarTeacher)
        {
            float LeftUpperArm_difference = Quaternion.Angle(avatarSelf.stickContainer.LeftUpperArm.transform.localRotation, avatarTeacher.stickContainer.LeftUpperArm.transform.localRotation);
            float LeftLowerArm_difference = Quaternion.Angle(avatarSelf.stickContainer.LeftLowerArm.transform.localRotation, avatarTeacher.stickContainer.LeftLowerArm.transform.localRotation);
            float RightUpperArm_difference = Quaternion.Angle(avatarSelf.stickContainer.RightUpperArm.transform.localRotation, avatarTeacher.stickContainer.RightUpperArm.transform.localRotation);
            float RightLowerArm_difference = Quaternion.Angle(avatarSelf.stickContainer.RightLowerArm.transform.localRotation, avatarTeacher.stickContainer.RightLowerArm.transform.localRotation);
            float LeftUpperLeg_difference = Quaternion.Angle(avatarSelf.stickContainer.LeftUpperLeg.transform.localRotation, avatarTeacher.stickContainer.LeftUpperLeg.transform.localRotation);
            float LeftLowerLeg_difference = Quaternion.Angle(avatarSelf.stickContainer.LeftLowerLeg.transform.localRotation, avatarTeacher.stickContainer.LeftLowerLeg.transform.localRotation);
            float RightUpperLeg_difference = Quaternion.Angle(avatarSelf.stickContainer.RightUpperLeg.transform.localRotation, avatarTeacher.stickContainer.RightUpperLeg.transform.localRotation);
            float RightLowerLeg_difference = Quaternion.Angle(avatarSelf.stickContainer.RightLowerLeg.transform.localRotation, avatarTeacher.stickContainer.RightLowerLeg.transform.localRotation);
            float thresh = correctionThresh;

            if (avatarSelf.stickContainer.stick.activeSelf)
            {
                avatarSelf.stickContainer.LeftUpperArm.GetComponent<Renderer>().material = (LeftUpperArm_difference > thresh) ? incorrect_material : correct_material;
                avatarSelf.stickContainer.LeftLowerArm.GetComponent<Renderer>().material = (LeftLowerArm_difference > thresh) ? incorrect_material : correct_material;
                avatarSelf.stickContainer.RightUpperArm.GetComponent<Renderer>().material = (RightUpperArm_difference > thresh) ? incorrect_material : correct_material;
                avatarSelf.stickContainer.RightLowerArm.GetComponent<Renderer>().material = (RightLowerArm_difference > thresh) ? incorrect_material : correct_material;
                avatarSelf.stickContainer.LeftUpperLeg.GetComponent<Renderer>().material = (LeftUpperLeg_difference > thresh) ? incorrect_material : correct_material;
                avatarSelf.stickContainer.LeftLowerLeg.GetComponent<Renderer>().material = (LeftLowerLeg_difference > thresh) ? incorrect_material : correct_material;
                avatarSelf.stickContainer.RightUpperLeg.GetComponent<Renderer>().material = (RightUpperLeg_difference > thresh) ? incorrect_material : correct_material;
                avatarSelf.stickContainer.RightLowerLeg.GetComponent<Renderer>().material = (RightLowerLeg_difference > thresh) ? incorrect_material : correct_material;
            }

        }


        // Animates all self avatars based on the JointData provided
        void AnimateSelf(PoseData live_data)
        {
            // MovePerson() considers which container to move
            foreach (AvatarContainer avatar in avatarListSelf)
            {
                avatar.MovePerson(live_data);
            }
        }
        // Animates all teacher avatars based on the JointData provided
        void AnimateTeacher(PoseData recorded_data)
        {
            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                avatar.MovePerson(recorded_data);
            }
        }

        public void SetTeacherFile(string path)
        {
            TeacherPoseInputGetter.ReadDataPath = path;
            //SelfPoseInputGetter.ReadDataPath = path;
        }

        public void ShowTeacher()
        {
            avatarListTeacher[0].avatarContainer.gameObject.SetActive(true);
            set_recording_mode(2);
        }
    }
}
