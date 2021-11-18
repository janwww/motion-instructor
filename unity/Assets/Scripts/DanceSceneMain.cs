using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace PoseTeacher
{

    public class DanceSceneMain : MonoBehaviour
    {
        PoseInputGetter selfPoseInputGetter;

        public GameObject videoCube;
        public GameObject scoreDisplay;
        public DancePerformanceScriptableObject DancePerformanceObject;

        public GameObject avatarContainerSelf, avatarContainerTeacher;
        List<AvatarContainer> avatarListSelf, avatarListTeacher;

        private readonly string fake_file = "jsondata/2020_05_27-00_01_59.txt";
        public PoseInputSource selfPoseInputSource = PoseInputSource.KINECT;

        ScoringScript scoringUtil;

        public bool paused = false;

        PoseData currentSelfPose;

        private DanceData danceData;
        private AudioClip song;
        private AudioSource audioSource;

        readonly List<(float, DanceData)> goals = new List<(float,DanceData)>();

        int currentId = 0;

        // Start is called before the first frame update
        public void Start()
        {
            avatarListSelf = new List<AvatarContainer>();
            avatarListTeacher = new List<AvatarContainer>();
            avatarListSelf.Add(new AvatarContainer(avatarContainerSelf));
            avatarListTeacher.Add(new AvatarContainer(avatarContainerTeacher));

            audioSource = GetComponent<AudioSource>();
            song = DancePerformanceObject.SongObject.SongClip;
            audioSource.clip = song;
            danceData = DancePerformanceObject.danceData.LoadDanceDataFromScriptableObject();

            //for(int i = 0; i < DancePerformanceObject.goals.Count; i++)
            //{
            //    goals.Add((DancePerformanceObject.goalTimestamps[i], DancePerformanceObject.goals[i].LoadDanceDataFromScriptableObject()));
            //}


            selfPoseInputGetter = new PoseInputGetter(selfPoseInputSource) { ReadDataPath = fake_file };
            selfPoseInputGetter.loop = true;
            selfPoseInputGetter.VideoCube = videoCube;
            
            scoringUtil = new ScoringScript(scoreDisplay);

            scoringUtil.StartNewGoal(GoalType.MOTION, goals[0].Item2.poses, 0f);
            goals.RemoveAt(0);
            audioSource.Play();
        }

        // Update is called once per frame
        public void Update()
        {
            float timeOffset = audioSource.time - danceData.poses[currentId].timestamp;
            currentSelfPose = selfPoseInputGetter.GetNextPose();
            AnimateSelf(currentSelfPose);
            if (goals.Count > 0 && audioSource.time >= goals[0].Item1)
            {
                scoringUtil.StartNewGoal(GoalType.MOTION, goals[0].Item2.poses, 0f);
                goals.RemoveAt(0);
            }
            AnimateTeacher(danceData.GetInterpolatedPose(currentId, out currentId, timeOffset).toPoseData());

            scoringUtil.Update(currentSelfPose, audioSource.time);

            if (audioSource.time > danceData.poses[danceData.poses.Count - 1].timestamp)
            {
                audioSource.Stop();
                List<Scores> finalScores = scoringUtil.getFinalScores();
                Debug.Log(finalScores);
                //TODO: Add final score screen
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
    }
}