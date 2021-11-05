using System.Collections.Generic;
using UnityEngine;

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
    public class PoseteacherMain : MonoBehaviour
    {
        PoseInputGetter SelfPoseInputGetter;
        PoseInputGetter TeacherPoseInputGetter;
        PoseInputGetter RecordedPoseInputGetter;

        // Used for displaying RGB Kinect video
        //public GameObject streamCanvas;
        public GameObject videoCube;

        // Refrences to containers in scene
        public GameObject avatarContainer; // Only used to get reference from editor Inspector
        public GameObject avatarContainerT; // Only used to get reference from editor Inspector
        List<AvatarContainer> avatarListSelf;
        List<AvatarContainer> avatarListTeacher;
        public AvatarContainer recordedAvatar;
        public GameObject avatarPrefab;
        public GameObject avatarTPrefab;

        // Refrences to other objects in scene
        GameObject scoreIndicator;
        GameObject pulseObject;
        GameObject progressIndicator;
        public GameObject trainingElements;
        GameObject trainHandMenuContent;
        public GameObject choreographyElements;
        GameObject choreoHandMenuContent;
        public GameObject recordElements;
        GameObject recordHandMenuContent;
        public GameObject EndCoreoScreen;
        public GameObject CourseHelper;
        public GameObject GraphContainer;


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
        private bool pauseRecordingAnimation = true;

        // For fake data when emulating input from file
        private readonly string fake_file = "jsondata/2020_05_27-00_01_59.txt";
        public PoseInputSource SelfPoseInputSource = PoseInputSource.KINECT;
        public bool mirroring = false; // can probably be changed to private (if no UI elements use it)


        // Used for pose similarity calculation
        public BodyWeightsType similarityBodyNr = BodyWeightsType.TOTAL;
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

        // Debugging and testing variables
        private bool forceSimilarityUpdate = false;

        // Used for showing similarity
        VisualisationSimilarity avatarVisualisationSimilarity;
        Graph graph;

        // Duplicates for recording
        AvatarSimilarity recordedAvatarSimilarity;
        VisualisationSimilarity recordedAvatarVisualisationSimilarity;

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
                    avatarSimilarity.penalty = 1.5;
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

        //Add copy of self or techer to scene
        public void AddAvatar(bool self)
        {
            if (self)
            {
                GameObject newAvatar = Instantiate(avatarPrefab);
                AvatarContainer newAvatarCont = new AvatarContainer(newAvatar, mirroring);
                avatarListSelf.Add(newAvatarCont);
                newAvatar.SetActive(true);
                newAvatarCont.ChangeActiveType(avatarListSelf[0].activeType);
                newAvatarCont.MovePerson(SelfPoseInputGetter.CurrentPose);
                //newAvatar.transform.position = avatarListSelf[avatarListSelf.Count - 1].avatarContainer.transform.position;
                //newAvatar.transform.position = newAvatar.transform.position + new Vector3(1,0,0);
                //newAvatar.transform.position = new Vector3(1, 0, 0);
            }
            else
            {
                GameObject newAvatar = Instantiate(avatarTPrefab);
                AvatarContainer newAvatarCont = new AvatarContainer(newAvatar, mirroring);
                avatarListTeacher.Add(newAvatarCont);
                newAvatar.SetActive(true);
                newAvatarCont.ChangeActiveType(avatarListTeacher[0].activeType);
                if (TeacherPoseInputGetter.CurrentPose != null)
                {
                    newAvatarCont.MovePerson(TeacherPoseInputGetter.CurrentPose);
                }

            }
        }
        public void DeleteAvatar(bool self)
        {
            if (self && avatarListSelf.Count > 1)
            {
                AvatarContainer avatar = avatarListSelf[avatarListSelf.Count - 1];
                avatar.avatarContainer.SetActive(false);
                avatarListSelf.Remove(avatar);
                Destroy(avatar.avatarContainer.gameObject);
            }
            else if (!self && avatarListTeacher.Count > 1)
            {
                AvatarContainer avatar = avatarListTeacher[avatarListTeacher.Count - 1];
                avatar.avatarContainer.SetActive(false);
                avatarListTeacher.Remove(avatar);
                Destroy(avatar.avatarContainer.gameObject);
            }
        }


        // Mirror all avatar containers
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
                //streamCanvas.transform.localScale = new Vector3(32f, 16f, 1f);
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
                //streamCanvas.transform.localScale = new Vector3(-32f, 16f, 1f);
            }
            recordedAvatar.Mirror(mirroring);
        }

        // Setters used in UI, configured in Inspector tab of respective buttons

        public void set_recording_mode(int rec)
        {
            // 0: pause,  2: play
            // code using this should be refactored, numbers were used in older verison of project

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
        // Should be replaced by override with enum bellow. Is bound in handmenu object
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

        // Set the avatar type for all AvatarGos
        public void SetAvatarTypes(AvatarType avatarType)
        {
            foreach (AvatarContainer avatar in avatarListSelf)
            {
                avatar.ChangeActiveType(avatarType);
            }

            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                avatar.ChangeActiveType(avatarType);
            }
            recordedAvatar.ChangeActiveType(avatarType);
        }

        // Do once on scene startup
        private void Start()
        {
            // Initialize the respective AvatarContainer classes
            avatarListSelf = new List<AvatarContainer>();
            avatarListTeacher = new List<AvatarContainer>();
            avatarListSelf.Add(new AvatarContainer(avatarContainer));
            avatarListTeacher.Add(new AvatarContainer(avatarContainerT));


            // Instantiate objects for showing recordings
            GameObject newAvatar = Instantiate(avatarTPrefab);
            recordedAvatar = new AvatarContainer(newAvatar);
            newAvatar.SetActive(false);

            // Set teacher container to inactive at start
            avatarListTeacher[0].avatarContainer.gameObject.SetActive(false);

            SelfPoseInputGetter = new PoseInputGetter(SelfPoseInputSource) { ReadDataPath = fake_file };
            TeacherPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE) { ReadDataPath = fake_file };
            SelfPoseInputGetter.loop = true;
            TeacherPoseInputGetter.loop = true;
            RecordedPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE) { ReadDataPath = fake_file };

            //SelfPoseInputGetter.streamCanvas = streamCanvas;
            SelfPoseInputGetter.VideoCube = videoCube;

            // initialize similarity calculation instance and assign selected avatars
            avatarSimilarity = new AvatarSimilarity(avatarListSelf[similaritySelfNr], avatarListTeacher[similarityTeacherNr], similarityBodyNr, similarityPenalty, similarityActivateKalman, similarityKalmanQ, similarityKalmanR);
            avatarVisualisationSimilarity = new VisualisationSimilarity(avatarListSelf[similaritySelfNr]);
            graph = new Graph(GraphContainer, 0.0F);

            recordedAvatarSimilarity = new AvatarSimilarity(recordedAvatar, avatarListTeacher[similarityTeacherNr], similarityBodyNr, similarityPenalty, similarityActivateKalman, similarityKalmanQ, similarityKalmanR);
            recordedAvatarVisualisationSimilarity = new VisualisationSimilarity(recordedAvatar);

            // Find attached objects in scene
            scoreIndicator = avatarContainer.transform.Find("ScoreIndicator").gameObject;
            pulseObject = avatarContainer.transform.Find("PulsingCube").gameObject;
            progressIndicator = avatarContainerT.transform.Find("ProgressIndicator").gameObject;
            trainHandMenuContent = trainingElements.transform.Find("HandMenu_Training_HideOnHandDrop").Find("MenuContent").gameObject;
            choreoHandMenuContent = choreographyElements.transform.Find("HandMenu_Coreo_HideOnHandDrop").Find("MenuContent").gameObject;
            recordHandMenuContent = recordElements.transform.Find("HandMenu_Recording_HideOnHandDrop").Find("MenuContent").gameObject;

            // Default is to have a mirrored view
            do_mirror();

        }

        private void Update()
        {
            if (!pauseSelf)
            {
                AnimateSelf(SelfPoseInputGetter.GetNextPose());
            }

        }

        // Done at each fixed Frame Update
        void FixedUpdate()
        {
            CheckKeyInput();

            if (isChoreography && TeacherPoseInputGetter.CurrentFilePoseNumber >= TeacherPoseInputGetter.TotalFilePoseNumber)
            {
                CoreoEnded();
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

                // Only update total score if doing a choreography and paying attention
                if (isChoreography && TeacherPoseInputGetter.CurrentFilePoseNumber < TeacherPoseInputGetter.TotalFilePoseNumber && pauseRecordingAnimation)
                {
                    similarityTotalScore = avatarSimilarity.totalScore;
                }

                avatarVisualisationSimilarity.UpdatePart(similarityBodyNr, similarityScoreRaw);
                graph.Update_plot(similarityScore);
            }
            else if (forceSimilarityUpdate)
            {
                // Used only when testing in editor
                // Force score and visualisation update, even if teacher did not move
                avatarSimilarity.Update(); // update similarity calculation with each update loop step
                similarityScore = avatarSimilarity.similarityBodypart; // get single similarity score for selected body part
                similarityScoreRaw = avatarSimilarity.similarityStick; // get similarity score for each stick element
                similarityWeightRaw = avatarSimilarity.stickWeight;
                avatarVisualisationSimilarity.UpdatePart(similarityBodyNr, similarityScoreRaw);
                graph.Update_plot(similarityScore);
            }

            if (!pauseRecordingAnimation)
            {
                record_counter++;
                if (record_counter >= playlimit)
                {
                    recordedAvatar.MovePerson(RecordedPoseInputGetter.GetNextPose());
                    record_counter = 0;
                }

                recordedAvatarSimilarity.Update();
                recordedAvatarVisualisationSimilarity.UpdatePart(similarityBodyNr, recordedAvatarSimilarity.similarityStick);

            }

            UpdateIndicators();
        }


        // Actions to do before quitting application
        private void OnApplicationQuit()
        {
            SelfPoseInputGetter.Dispose();
            TeacherPoseInputGetter.Dispose();
            if (RecordedPoseInputGetter != null)
                RecordedPoseInputGetter.Dispose();
        }

        // Change recording mode via keyboard input for debugging and to not need menu
        void CheckKeyInput()
        {
            if (Input.GetKeyDown(KeyCode.H))
            {
                // Toggle hand menu (in training/choreography)
                if (pulseObject != null)
                {
                    GameObject toggleObject = null;
                    if (trainingElements.activeSelf)
                        toggleObject = trainHandMenuContent;
                    if (choreographyElements.activeSelf)
                        toggleObject = choreoHandMenuContent;
                    if (recordElements.activeSelf)
                        toggleObject = recordHandMenuContent;
                    if (toggleObject == null)
                        return;

                    ScorePulse sp = pulseObject.GetComponent<ScorePulse>();
                    if (!toggleObject.activeSelf)
                    {
                        Debug.Log("H - Toggle Hand Menu to active");
                        toggleObject.SetActive(true);
                        set_recording_mode(0);
                        sp.SetPause(true);
                    }
                    else
                    {
                        Debug.Log("H - Toggle Hand Menu to inactive");
                        toggleObject.SetActive(false);
                        set_recording_mode(2);
                        sp.SetPause(false);
                    }
                }
            }
            else if (Input.GetKeyDown(KeyCode.O))
            {
                Debug.Log("O - toggle teacher pause");
                pauseTeacher = !pauseTeacher;
            }
            else if (Input.GetKeyDown(KeyCode.P))
            {
                Debug.Log("P - toggle self pause");
                pauseSelf = !pauseSelf;
            }
            else if (Input.GetKeyDown(KeyCode.U))
            {
                Debug.Log("U - toggle force similarity update");
                forceSimilarityUpdate = !forceSimilarityUpdate;
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

        private void UpdateIndicators()
        {
            if (scoreIndicator.activeSelf)
            {
                if (isChoreography)
                {
                    scoreIndicator.GetComponent<ProgressIndicator>().pTotal.SetActive(true);
                    float progress = (float)similarityTotalScore / TeacherPoseInputGetter.TotalFilePoseNumber;
                    scoreIndicator.GetComponent<ProgressIndicator>().SetProgress(progress);
                    scoreIndicator.GetComponent<ProgressIndicator>().SetTotalScore((int)similarityTotalScore, TeacherPoseInputGetter.TotalFilePoseNumber);
                }
                else
                {
                    scoreIndicator.GetComponent<ProgressIndicator>().pTotal.SetActive(false);
                    scoreIndicator.GetComponent<ProgressIndicator>().SetProgress((float)similarityScore);
                }
            }
            if (pulseObject.activeSelf)
            {
                // TODO make cube pulse depending on parameters of dance
            }
            if (progressIndicator.activeSelf)
            {
                float progress = (float)TeacherPoseInputGetter.CurrentFilePoseNumber / TeacherPoseInputGetter.TotalFilePoseNumber;
                progressIndicator.GetComponent<ProgressIndicator>().SetProgress(progress);
            }
        }


        #region Functions for external changes

        public void ActivateIndicators()
        {
            if (scoreIndicator != null)
                scoreIndicator.SetActive(true);

            if (pulseObject != null)
                pulseObject.SetActive(true);

            if (progressIndicator != null)
                progressIndicator.SetActive(true);

            foreach (AvatarContainer avatar in avatarListSelf)
            {
                avatar.MoveIndicators(true);
            }
            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                avatar.MoveIndicators(true);
            }
        }

        public void DeActivateIndicators()
        {
            if (scoreIndicator != null)
                scoreIndicator.SetActive(false);

            if (pulseObject != null)
                pulseObject.SetActive(false);

            if (progressIndicator != null)
                progressIndicator.SetActive(false);
        }

        public void SetTeacherFile(string path)
        {
            TeacherPoseInputGetter.ReadDataPath = path;
        }

        public void ShowTeacher()
        {
            foreach (AvatarContainer avatar in avatarListTeacher)
            {
                avatar.avatarContainer.gameObject.SetActive(true);
            }
            set_recording_mode(2);
        }

        public void SetIsChoreography(bool newIsChoreogaphy)
        {
            if (newIsChoreogaphy)
            {
                isChoreography = true;
                TeacherPoseInputGetter.loop = false;

                // Mute cube
                ScorePulse sp = pulseObject.GetComponent<ScorePulse>();
                sp.isMuted = true;
            }
            else
            {
                isChoreography = false;
                TeacherPoseInputGetter.loop = true;

                // Unmute cube
                ScorePulse sp = pulseObject.GetComponent<ScorePulse>();
                sp.isMuted = false;
            }
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
                    RecordedPoseInputGetter.ReadDataPath = SelfPoseInputGetter.WriteDataPath;

                    isrecording = false;

                    if (!isChoreography)
                    {
                        pauseRecordingAnimation = false;
                    }

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
            pauseRecordingAnimation = true;
        }

        public void StartStopShowingRecording()
        {
            if (!pauseRecordingAnimation)
            {
                recordedAvatar.avatarContainer.SetActive(false);
                pauseRecordingAnimation = false;
            }
            else
            {
                recordedAvatar.avatarContainer.SetActive(true);
                pauseRecordingAnimation = true;
            }
        }

        public void RestartShowRecording()
        {
            RecordedPoseInputGetter.RestartFile();
        }

        public void PauseShowingRecording()
        {
            pauseRecordingAnimation = !pauseRecordingAnimation;
        }

        private bool graphAllowed = true;
        public void ChangeGraphVisibilityAllowed()
        {
            graphAllowed = !graphAllowed;
            if (graph != null)
                graph.graphContainer.SetActive(graphAllowed);
        }

        private bool videoCubeAllowed = true;
        public void ChangeVideoCubeVisibilityAllowed()
        {
            videoCubeAllowed = !videoCubeAllowed;
            if (videoCube != null)
                videoCube.SetActive(videoCubeAllowed);
        }

        public void CoreoEnded()
        {
            // Show CoreoEndScreen with proper data
            StopRecordingMode();

            EndCoreoScreen.SetActive(true);
            CoreoEndScreen endScreenHelper = EndCoreoScreen.GetComponent<CoreoEndScreen>();
            CourseMenuHelper courseHelper = CourseHelper.GetComponent<CourseMenuHelper>();
            endScreenHelper.SetCoreoName(courseHelper.CurrentStepName());
            endScreenHelper.SetScore((int)similarityTotalScore, TeacherPoseInputGetter.TotalFilePoseNumber);

        }

        public void RestartCoreo()
        {
            ResetTotalScore();
            // RestartRecording instead of Reset...
            TeacherPoseInputGetter.RestartFile();
            RestartShowRecording(); // We might will have a naming error, consider using GenNewFilename for "temporary" recordings too...
        }

        public void LoopStepMovement()
        {
            TeacherPoseInputGetter.loop = !TeacherPoseInputGetter.loop;
        }

        public void RestartStep()
        {
            TeacherPoseInputGetter.RestartFile();
        }

        public void CoreoShowRestartRecording()
        {
            TeacherPoseInputGetter.RestartFile();
            RestartShowRecording();
            pauseRecordingAnimation = false;

        }

        public void ResetTotalScore()
        {
            avatarSimilarity.ResetTotalScore();
        }
    }

    #endregion
}
