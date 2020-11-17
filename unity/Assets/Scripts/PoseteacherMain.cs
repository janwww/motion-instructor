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
    // Main script
    // TODO:
    // Reorder functions (need to define placeholders first for somoe)
    [CLSCompliant(false)]
    public class PoseteacherMain : MonoBehaviour
    {

        Device device;
        BodyTracker tracker;

        // Used for displaying RGB Kinect video
        public Renderer videoRenderer;
        RawImage m_RawImage;
        GameObject streamCanvas;
        GameObject videoCube;

        // Refrences to containers in scene
        // TODO: change it to list(s) or other flexible datastructure to allow more (partially done)
        // TODO find good way to reference objects. @Unity might do this for us
        //AvatarContainer avatarSelf;
        //AvatarContainer avatarTeacher;
        //AvatarContainer avatarSelf2;
        //AvatarContainer avatarTeacher2;
        List<AvatarContainer> avatarListSelf;
        List<AvatarContainer> avatarListTeacher;

        GameObject spatialAwarenessSystem;


        //Select a Texture in the Inspector to change it (unused)
        public Texture m_Texture;
        Texture2D tex;

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

        IEnumerable<string> read_recording_data;
        IEnumerator<string> sequenceEnum;

        PoseData poseLiveWS = null; // newest pose data from WS

        // File that is being read from
        string current_file = "jsondata/2020_05_26-22_55_32.txt";

        //For fake data 
        IEnumerable<string> fake_data;
        IEnumerator<string> fake_sequenceEnum;
        string fake_file = "jsondata/2020_05_26-22_55_32.txt";

        // can be changed in the UI
        // TODO: probaly change to using functions to toggle
        public bool isMaleSMPL = true;
        public bool usingKinectAlternative = true;


        public bool mirroring = true; // can probably be changed to private (if no UI elements use it)
        public bool debugMode = true;
        private bool loadedFakeData = false;

        // Web socket is currently used only for Kinect Alternative
        WebSocket websocket;
        public string WS_ip = "ws://localhost:2567";

        // Used for showing if pose is correct
        public Material normal_material;
        public Material correct_material;
        public Material incorrect_material;
        public bool shouldCheckCorrectness = true;
        public float correctionThresh = 30.0f;

        // Used for pose similarity calculation
        public int similarity_body_part = 1; // 1: top, 2: middle, 3: bottom, 4: total
        public int similarity_self_element = 0; // self list element to compare
        public int similarity_teacher_element = 0; // teacher list element to compare
        public double similarity_pose; // similarity value between 0 and 1 for defined body part
        AvatarSimilarity avatarSimilarity;


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
        async void Start()
        {
            // Contains code to instantiate a websocket to obtain pose data
            // Web socket is currently used only for Kinect Alternative


            // TODO: remove static socket ip , replace from field
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
                // If joint information is recieved, set poseLiveWS
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("WS message received: " + message);
                var remote_joints = PoseDataUtils.DeserializeRJL(message);
                Debug.Log(remote_joints);
                poseLiveWS = PoseDataUtils.Remote2PoseData(remote_joints);
            };

            // Keep sending messages at every 0.3s
            InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

            await websocket.Connect();
        }


        // Activated once, on enable (at the start)
        // TODO: Merge with Start function as this script is never disabled
        private void OnEnable()
        {
            Debug.Log("OnEnable");

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
            /*avatarSelf = new AvatarContainer(avatarContainer);
            avatarTeacher = new AvatarContainer(avatarContainerT);
            avatarSelf2 = new AvatarContainer(avatarContainer2);
            avatarTeacher2 = new AvatarContainer(avatarContainerT2);*/

            // Set unused containers to inactive
            avatarListTeacher[0].avatarContainer.gameObject.SetActive(false);
            avatarListSelf[1].avatarContainer.gameObject.SetActive(false);
            avatarListTeacher[1].avatarContainer.gameObject.SetActive(false);

            // Setup device and tracker for Azure Kinect Body Tracking
            if (usingKinectAlternative == false)
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

            // initialize similarity calculation instance
            avatarSimilarity = new AvatarSimilarity(avatarListSelf[similarity_self_element], avatarListTeacher[similarity_teacher_element]);

        }


        // Done at each application update
        void Update()
        {
#if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
#endif

            checkKeyInput();

            // Get new pose
            if (usingKinectAlternative)
            {
                // poseLiveWS is non-null if alternative is sending pose data over websocket
                if (poseLiveWS != null)
                {
                    PoseData live_data = poseLiveWS;
                    AnimateSelf(live_data);
                }
                // no data being recieved over websocket and debug mode
                else if (debugMode == true)
                {
                    // load fake data once
                    // TODO: should probably be in Start function
                    if (loadedFakeData == false)
                    {
                        loadedFakeData = true;
                        //loadRecording();
                        fake_data = File.ReadLines(fake_file);
                        fake_sequenceEnum = fake_data.GetEnumerator();
                    }

                    // use fake data from JSON file for testing
                    // TODO: have different sequenceEnum for fake input not same as used for playback teacher
                    // otherwise advances pose also for teacher
                    // Quick and dirty way to loop (by reloading file)
                    if (!fake_sequenceEnum.MoveNext())
                    {
                        fake_data = File.ReadLines(fake_file);
                        fake_sequenceEnum = fake_data.GetEnumerator();
                        fake_sequenceEnum.MoveNext();
                    }


                    string frame_json = fake_sequenceEnum.Current;
                    PoseData fake_live_data = PoseDataUtils.JSONstring2PoseData(frame_json);
                    //Debug.Log(avatarSelf);
                    AnimateSelf(fake_live_data);
                }

            }
            else if (device != null)
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
                        AnimateSelf(live_data);

                        // if recording pose, append to current_file file
                        if (recording_mode == 1) // recording
                        {
                            PoseDataJSON jdl = PoseDataUtils.Body2PoseDataJSON(body);
                            appendRecordedFrame(jdl, current_file);
                        }
                    }
                }
            }
            else
            {
                Debug.Log("device is null!");
            }

            // Get pose similarity
            similarity_pose = avatarSimilarity.GetSimilarity();

            // Playback for teacher avatar(s)
            if (recording_mode == 2) // playback
            {
                // load if not yet loaded
                if (sequenceEnum == null)
                {
                    loadRecording();
                }

                // play recording at different speeds
                // skip pose update if counter isn't big enought
                // TODO: maybe rewrite...
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

                // Get current pose and apply it to teacher avatars
                string frame_json = sequenceEnum.Current;
                PoseData recorded_data = PoseDataUtils.JSONstring2PoseData(frame_json);
                AnimateTeacher(recorded_data);

                // TODO: Put this in animate function?
                if (shouldCheckCorrectness)
                {
                    showCorrection(avatarListSelf[0], avatarListTeacher[0]);
                }



            }


        }

        // Actions to do before quitting application
        private async void OnApplicationQuit()
        {
            await websocket.Close();
        }


        // If script onbject is disabled, (normally this never happens)
        // when usingKinectAlternative == false the varables are unitialized i.e. ==null
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

        // For testing websocket
        async void SendWebSocketMessage()
        {
            if (websocket.State == WebSocketState.Open)
            {
                // Sending text
                await websocket.SendText("Test message");
            }
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

        // (REMOVED) Functions for moving different avatars to a PoseData pose
        // TODO: move functions to AvatarContainer class (done)
        //      remove code duplication (done)
        //   !  remove unused lines of code -> see various container classes



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


        // Appends the passed pose (PoseDataJSON format) to the file as JSON
        // TODO: remove unused parameter avatarSelf
        void appendRecordedFrame(PoseDataJSON jdl, string filename)
        {
            string json = JsonUtility.ToJson(jdl) + Environment.NewLine;
            File.AppendAllText(filename, json);
        }

        // Loads passed filename into read_recording_data and sequenceEnum fields
        // If no filename is passed, loads current_file
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

        // !!! Deletes contents of the passed filename to remove all appended poses
        // If no filename is passed current_file content is deleted
        // Currently not used
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
      
    }
}
