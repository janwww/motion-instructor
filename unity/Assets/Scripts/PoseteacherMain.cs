using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using NativeWebSocket;
using System.Security.Permissions;

namespace PoseTeacher
{
    public enum MainState
    {
        PAUSE, PLAY_SELF, PLAY_ALL
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

        //GameObject spatialAwarenessSystem;

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



        // Default file to read from for techer. Should not actually be played
        private string current_file = "";

        // For fake data when emulating unput from file
        // "jsondata/2020_05_26-21_23_28.txt"  "jsondata/2020_05_26-22_55_32.txt"  "jsondata/2020_05_27-00_01_59.txt"
        private readonly string fake_file = "jsondata/2020_05_27-00_01_59.txt";

        // can be changed in the UI
        // TODO: probaly change to using functions to toggle
        public bool isMaleSMPL = true;
        public bool usingKinectAlternative = true;
        public PoseInputSource SelfPoseInputSource = PoseInputSource.KINECT;


        public bool mirroring = true; // can probably be changed to private (if no UI elements use it)

        // Used for showing if pose is correct
        public Material normal_material;
        public Material correct_material;
        public Material incorrect_material;
        public bool shouldCheckCorrectness = true;
        public float correctionThresh = 30.0f;


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
        public void set_recording_mode(int rec)
        {
            recording_mode = rec;
        }
        public void set_playback_speed(int a)
        {
            playback_speed = a;
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
            TeacherPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE){ ReadDataPath = fake_file };


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
