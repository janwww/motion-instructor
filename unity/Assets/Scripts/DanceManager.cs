using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using UnityEngine.SceneManagement;

namespace PoseTeacher
{

    public class DanceManager : MonoBehaviour
    {
        public static DanceManager Instance;

        PoseGetter selfPoseInputGetter;

        public EndScoreScreen endScoreScreen;
        public GameObject videoCube;
        public DancePerformanceScriptableObject DancePerformanceObject;

        public GameObject avatarContainerSelf, avatarContainerTeacher;
        List<AvatarContainer> avatarListSelf, avatarListTeacher;

        private readonly string fake_file = "jsondata/2020_05_27-00_01_59.txt";
        public InputSource selfPoseInputSource = InputSource.KINECT;

        public bool paused = false;

        public PoseData currentSelfPose;

        private DanceData danceData;
        private AudioClip song;
        private AudioSource audioSource;

        List<(float, DanceData)> goals = new List<(float,DanceData)>();

        public float songTime => audioSource?.time ?? 0;

        bool finished = false;
        int currentId = 0;

        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

            // Start is called before the first frame update
        public void Start()
        {
            Setup();
            RestartSong();
        }

        // Update is called once per frame
        public void Update()
        {
            currentSelfPose = selfPoseInputGetter.GetNextPose();
            AnimateSelf(currentSelfPose);
            if (!finished)
            {
                float timeOffset = audioSource.time - danceData.poses[currentId].timestamp;
                if (goals.Count > 0 && audioSource.time >= goals[0].Item1)
                {
                    ScoringManager.Instance.StartNewGoal(goals[0].Item2.poses, 0f);
                    goals.RemoveAt(0);
                }
                AnimateTeacher(danceData.GetInterpolatedPose(currentId, out currentId, timeOffset).toPoseData());

                if (audioSource.time >= audioSource.clip.length)
                {
                    FinishSong();
                }
            }
            
        }

        public void OnApplicationQuit()
        {
            selfPoseInputGetter.Dispose();
            
        }

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

        PoseGetter getPoseGetter(InputSource src) {
            switch (src)
            {
                case InputSource.KINECT:
                    return new KinectPoseGetter() { VideoCube = videoCube};
                case InputSource.FILE:
                    return new FilePoseGetter(true) { ReadDataPath = fake_file };
                default:
                    return new FilePoseGetter(true) { ReadDataPath = fake_file };
            }
        }

        void FinishSong()
        {
            finished = true;
            audioSource.Stop();
            float totalScore = ScoringManager.Instance.getFinalScores().Item1;
            List<Scores> finalScores = ScoringManager.Instance.getFinalScores().Item2;

            Debug.Log(finalScores);
            //TODO: Add final score screen
            endScoreScreen.setValues(totalScore,
                finalScores.Where(element => element == Scores.GREAT).Count(),
                finalScores.Where(element => element == Scores.GOOD).Count(),
                finalScores.Where(element => element == Scores.BAD).Count());
            endScoreScreen.gameObject.SetActive(true);
        }

        void Setup()
        {
            if (PersistentData.Instance != null)
            {
                DancePerformanceObject = PersistentData.Instance.performance;
            }
            avatarListSelf = new List<AvatarContainer>();
            avatarListTeacher = new List<AvatarContainer>();
            avatarListSelf.Add(new AvatarContainer(avatarContainerSelf));
            avatarListTeacher.Add(new AvatarContainer(avatarContainerTeacher));


            audioSource = GetComponent<AudioSource>();
            song = DancePerformanceObject.SongObject.SongClip;
            audioSource.clip = song;
            danceData = DancePerformanceObject.danceData.LoadDanceDataFromScriptableObject();

            selfPoseInputGetter = getPoseGetter(selfPoseInputSource);

        }

        public void RestartSong()
        {
            endScoreScreen.gameObject.SetActive(false);

            goals = new List<(float, DanceData)>();
            for (int i = 0; i < DancePerformanceObject.goals.Count; i++)
            {
                goals.Add((DancePerformanceObject.goalStartTimestamps[i], DancePerformanceObject.goals[i]));
            }
            audioSource.time = 0;
            currentId = 0;
            finished = false;
            audioSource.PlayDelayed(0.5f);
        }

        public void QuitToMenu()
        {
            SceneManager.LoadScene("StartMenu", LoadSceneMode.Single);
        }
    }
}