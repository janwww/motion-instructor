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
    public enum PlaybackSpeed
    {
        s1_00, s0_50, s0_25
    }

    public enum Difficulty
    {
        EASY, MEDIUM, HARD
    }

    // Main script
    [CLSCompliant(false)]
    public class PoseteacherMain : MonoBehaviour
    {
        PoseInputGetter SelfPoseInputGetter;
        PoseInputGetter TeacherPoseInputGetter;
        PoseInputGetter RecordedPoseInputGetter;

        // Used for displaying RGB Kinect video
        GameObject streamCanvas;
        GameObject videoCube;

        // Refrences to containers in scene
        List<AvatarContainer> avatarListSelf;
        List<AvatarContainer> avatarListTeacher;
        public AvatarContainer recordedAvatar;
        public GameObject avatarPrefab;
        public GameObject avatarTPrefab;

        // State of Main
        //public int recording_mode = 0; // 0: not recording, 1: recording, 2: playback, 3: load_file, 4: reset_recording
        bool isrecording = false;
        public bool pauseSelf = false;
        public bool pauseTeacher = true;
        public bool isChoreography = false;

        // Playback speed variables
        //private int playback_speed = 1; // 1: x1 , 2: x0.5, 4: x0.25
        public PlaybackSpeed playbackSpeed = PlaybackSpeed.s1_00;
        private int playback_counter = 0;
        private int record_counter = 0;
        private bool show_recording = false;

        // For fake data when emulating input from file
        // "jsondata/2020_05_26-21_23_28.txt"  "jsondata/2020_05_26-22_55_32.txt"  "jsondata/2020_05_27-00_01_59.txt"
        private readonly string fake_file = "jsondata/2020_05_27-00_01_59.txt";
        public PoseInputSource SelfPoseInputSource = PoseInputSource.KINECT;
        public bool mirroring = true; // can probably be changed to private (if no UI elements use it)


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

        // Used for showing similarity
        VisualisationSimilarity avatarVisualisationSimilarity;
        Graphtest graphtest;
        public static double similarityScoreExtern = 0.0; // similarity value between 0 and 1 for defined body part (extern global variable for plot)
        public static double similarityTotalScoreExtern = 0.0; // Total score (extern global variable for plot)

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
                AvatarContainer newAvatarCont = new AvatarContainer(newAvatar);
                avatarListSelf.Add(newAvatarCont);
                newAvatar.SetActive(true);
                newAvatarCont.ChangeActiveType(avatarListSelf[0].activeType);
                //newAvatar.transform.position = avatarListSelf[avatarListSelf.Count - 1].avatarContainer.transform.position;
                //newAvatar.transform.position = newAvatar.transform.position + new Vector3(1,0,0);
                //newAvatar.transform.position = new Vector3(1, 0, 0);
            } else
            {
            //    GameObject avatarContainer = GameObject.Find("AvatarContainerT");
                GameObject newAvatar = Instantiate(avatarTPrefab);
                AvatarContainer newAvatarCont = new AvatarContainer(newAvatar);
                avatarListTeacher.Add(newAvatarCont);
                newAvatar.SetActive(true);
                newAvatarCont.ChangeActiveType(avatarListTeacher[0].activeType);
            }
        }
        public void DeleteAvatar(bool self)
        {
            if (self && avatarListSelf.Count > 1)
            {
                AvatarContainer avatar = avatarListSelf[avatarListSelf.Count - 1];
                avatar.avatarContainer.SetActive(false);
                avatarListSelf.Remove(avatar);
            } else if (!self && avatarListTeacher.Count > 1)
            {
                AvatarContainer avatar = avatarListTeacher[avatarListTeacher.Count - 1];
                avatar.avatarContainer.SetActive(false);
                avatarListTeacher.Remove(avatar);
            }
        }
        

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
        public void set_recording_mode(int rec)
        {
            // 0: not recording, 1: recording, 2: playback, 3: load_file, 4: reset_recording
            switch (rec)
            {
                case 0:
                    pauseSelf = false;
                    pauseTeacher = true;
                    break;
                case 2:
                    pauseSelf = false;
                    pauseTeacher = false;
                    break;
                default:
                    Debug.Log("Assigned unused recording_mode");
                    break;
            }
        }
        public void set_playback_speed(int a)
        {
            switch (a)
            {
                case 1:
                    playbackSpeed = PlaybackSpeed.s1_00;
                    break;
                case 2:
                    playbackSpeed = PlaybackSpeed.s0_50;
                    break;
                case 4:
                    playbackSpeed = PlaybackSpeed.s0_25;
                    break;
                default:
                    Debug.Log("Assigned invalid playback speed");
                    break;
            }
        }
        public void set_playback_speed(PlaybackSpeed s)
        {
            playbackSpeed = s;
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
          //  avatarListSelf.Add(new AvatarContainer(avatarContainer2));
            avatarListTeacher.Add(new AvatarContainer(avatarContainerT));
            //  avatarListTeacher.Add(new AvatarContainer(avatarContainerT2));


            GameObject newAvatar = Instantiate(avatarTPrefab);
            recordedAvatar = new AvatarContainer(newAvatar);
            newAvatar.SetActive(false);

            // Set unused containers to inactive
            avatarListTeacher[0].avatarContainer.gameObject.SetActive(false);
            avatarContainer2.SetActive(false);
            avatarContainerT2.SetActive(false);
            // avatarListSelf[1].avatarContainer.gameObject.SetActive(false);
            // avatarListTeacher[1].avatarContainer.gameObject.SetActive(false);

            SelfPoseInputGetter = new PoseInputGetter(SelfPoseInputSource) { ReadDataPath = fake_file };
            TeacherPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE){ ReadDataPath = fake_file };

            SelfPoseInputGetter.streamCanvas = streamCanvas;
            SelfPoseInputGetter.VideoCube = videoCube;

            // initialize similarity calculation instance and assign selected avatars
            avatarSimilarity = new AvatarSimilarity(avatarListSelf[similaritySelfNr], avatarListTeacher[similarityTeacherNr], similarityBodyNr, similarityPenalty, similarityActivateKalman, similarityKalmanQ, similarityKalmanR);
            avatarVisualisationSimilarity = new VisualisationSimilarity(avatarListSelf[similaritySelfNr]);
            graphtest = new Graphtest((float)similarityScoreExtern);
        }

        private int temporaryCounter = 0;
        public bool temporaryBool = false;
        // Done at each application update
        void Update()
        {

            if (temporaryCounter == 1000)
                CoreoEnded();
            if (temporaryBool) 
                temporaryCounter++;

            checkKeyInput();

            if (!pauseSelf)
            {
                AnimateSelf(SelfPoseInputGetter.GetNextPose());
            }

            int playlimit = 1;
            switch (playbackSpeed)
            {
                case PlaybackSpeed.s0_25:
                    playlimit = 4;
                    break;
                case PlaybackSpeed.s0_50:
                    playlimit = 2;
                    break;
            }

            // Playback for teacher avatar(s)
            if (!pauseTeacher) // playback
            {
                playback_counter++;
                if (playback_counter >= playlimit)
                {
                    AnimateTeacher(TeacherPoseInputGetter.GetNextPose());
                    playback_counter = 0;
                }

                // Get pose similarity
                avatarSimilarity.Update(); // update similarity calculation with each update loop step
                similarityScore = avatarSimilarity.similarityBodypart; // get single similarity score for selected body part
                similarityScoreRaw = avatarSimilarity.similarityStick; // get similarity score for each stick element
                similarityWeightRaw = avatarSimilarity.stickWeight; // get similarity score for each stick element
                similarityTotalScore = avatarSimilarity.totalScore; // get total Score
                similarityScoreExtern = similarityScore; // global
                similarityTotalScoreExtern = similarityTotalScore; // global

                avatarVisualisationSimilarity.UpdatePart(similarityBodyNr, similarityScoreRaw);
                graphtest.Update_plot(similarityScoreExtern);
            }

            if (show_recording)
            {
                record_counter++;
                if (record_counter >= playlimit)
                {
                    recordedAvatar.MovePerson(RecordedPoseInputGetter.GetNextPose());
                    record_counter = 0;
                }
            }

            UpdateIndicators();
        }


        // Scale score linearly, only for testing
        private float scaleScore(float score)
        {
            float min = 0.5F;

            float scaled_score = (float)(score - min) /(1-min);
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
                        //float progress = scaleScore((float)similarityScore);
                        float progress = (float)similarityScore;
                        scoreIndicator.GetComponent<ProgressIndicator>().SetProgress(progress);

                        if (isChoreography)
                            scoreIndicator.GetComponent<ProgressIndicator>().pTotal.SetActive(true);
                        else
                            scoreIndicator.GetComponent<ProgressIndicator>().pTotal.SetActive(false);

                        if (isChoreography)
                            scoreIndicator.GetComponent<ProgressIndicator>().SetTotalScore((int)similarityTotalScore);

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
            RecordedPoseInputGetter.Dispose();
        }

        // Change recording mode via keyboard input for debugging and to not need menu
        void checkKeyInput()
        {
            if (Input.GetKey(KeyCode.X))
            {
                Debug.Log("X - set recording_mode to 0 (not recording)");
                //recording_mode = 0;
            }
            else if (Input.GetKey(KeyCode.Y))
            {
                Debug.Log("Y - set recording_mode to 1 (recording)");
                //recording_mode = 1;
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                Debug.Log("Z - set recording_mode to 2 (playback)");
                //recording_mode = 2;
            }
            else if (Input.GetKey(KeyCode.L))
            {
                Debug.Log("L - set recording_mode to 3 (load_file)");
                //recording_mode = 3;
            }
            else if (Input.GetKey(KeyCode.R))
            {
                Debug.Log("R - set recording_mode to 4 (reset_recording)");
                //recording_mode = 4;
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
            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                avatar.avatarContainer.gameObject.SetActive(true);
            }
            //avatarListTeacher[0].avatarContainer.gameObject.SetActive(true);
            set_recording_mode(2);
        }

        
        public void StartRecordingMode(bool temporary)
        {
            Debug.Log("Start recording mode");
            if (temporary)
            {
                //SelfPoseInputGetter.WriteDataPath = "jsondata/temporary.txt";
                //SelfPoseInputGetter.ResetRecording();
                SelfPoseInputGetter.GenNewFilename();
            }
            else
            {
                // Generate new filename with timestamp
                SelfPoseInputGetter.GenNewFilename();
            }
            
            SelfPoseInputGetter.recording = true;
            isrecording = true;
        }

        public void StopRecordingMode(bool abort = false)
        {
            Debug.Log("Stop recording mode");
            if (isrecording)
            {
                if (abort)
                {
                    SelfPoseInputGetter.recording = false;
                    isrecording = false;
                }
                else
                {
                    SelfPoseInputGetter.recording = false;
                    recordedAvatar.avatarContainer.SetActive(true);
                    RecordedPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE) { ReadDataPath = SelfPoseInputGetter.WriteDataPath };
                    show_recording = true;
                    isrecording = false;
                }
            }
        }

        public void PauseRecordingMode()
        {
            if (isrecording)
                SelfPoseInputGetter.recording = false;
        }
        public void ResumeRecordingMode()
        {
            if (isrecording)
                SelfPoseInputGetter.recording = true;
        }

        public void StopShowingRecording()
        {
            recordedAvatar.avatarContainer.SetActive(false);
            show_recording = false;
        }

        public void StartStopShowingRecording()
        {
            if (show_recording)
            {
                recordedAvatar.avatarContainer.SetActive(false);
                show_recording = false;
            } else
            {
                recordedAvatar.avatarContainer.SetActive(true);
                show_recording = true;
            }
        }

        public void RestartShowRecording()
        {
            //RecordedPoseInputGetter.RestartRecording();
        }

        public void PauseShowingRecording()
        {
            show_recording = !show_recording;
        }

        private bool graphAllowed = true;
        public void ChangeGraphVisibilityAllowed()
        {
            graphAllowed = !graphAllowed;
            if (graphtest != null)
                graphtest.graphContainer.SetActive(graphAllowed);
        }


        private bool videoCubeAllowed = true;
        public void ChangeVideoCubeVisibilityAllowed()
        {
            videoCubeAllowed = !videoCubeAllowed;
            if (videoCube != null)
                videoCube.SetActive(videoCubeAllowed);
        }

        public GameObject EndCoreoScreen;
        public GameObject CourseHelper;

        public void CoreoEnded()
        {
            // Show CoreoEndScreen with proper data
            StopRecordingMode();
            pauseTeacher = true;
            CoreoEndScreen endScreenHelper = EndCoreoScreen.GetComponent<CoreoEndScreen>();
            CourseMenuHelper courseHelper = CourseHelper.GetComponent<CourseMenuHelper>();
            endScreenHelper.SetCoreoName(courseHelper.CurrentStepName());
            endScreenHelper.SetScore((int)similarityTotalScore);
            EndCoreoScreen.SetActive(true);
        }

        public void RestartCoreo()
        {
            ResetTotalScore();
            // it should be RestartRecording instead of Reset...
            //TeacherPoseInputGetter.RestartRecording();
            RestartShowRecording(); // We might will have a naming error, consider using GenNewFilename for "temporary" recordings too...
            pauseTeacher = false;
        }

        public void LoopStepMovement()
        {

        }

        public void RestartStep()
        {
            //TeacherPoseInputGetter.RestartRecording();
        }

        public void CoreoShowRestartRecording()
        {
            //TeacherPoseInputGetter.RestartRecording();
            RestartShowRecording();
        }

        public void ResetTotalScore()
        {
            avatarSimilarity.ResetTotalScore();
        }
    }
}
