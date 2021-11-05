using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

namespace PoseTeacher
{

    public class DanceSceneMain : MonoBehaviour
    {
        PoseInputGetter selfPoseInputGetter, teacherPoseInputGetter;

        public GameObject videoCube;
        public GameObject scoreDisplay;
        public DancePerformanceScriptableObject DancePerformanceObject;

        public GameObject avatarContainerSelf, avatarContainerTeacher;
        List<AvatarContainer> avatarListSelf, avatarListTeacher;

        private readonly string dance_file = "jsondata/salsa_m/2020_12_14-15_46_29.txt";
        private readonly string move_file = "jsondata/move1.txt";
        private readonly string fake_file = "jsondata/2020_05_27-00_01_59.txt";
        public PoseInputSource selfPoseInputSource = PoseInputSource.KINECT;

        ScoringScript scoringUtil;

        public bool paused = false;

        PoseData currentSelfPose;

        private DanceData danceData;
        private AudioClip song;
        private AudioSource audioSource;


        readonly List<DancePose> move = new List<DancePose>();

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

            selfPoseInputGetter = new PoseInputGetter(selfPoseInputSource) { ReadDataPath = fake_file };
            teacherPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE) { ReadDataPath = dance_file };
            selfPoseInputGetter.loop = true;
            teacherPoseInputGetter.loop = false;

            selfPoseInputGetter.VideoCube = videoCube;
            
            scoringUtil = new ScoringScript(scoreDisplay);

            Debug.Log(danceData.poses.Count);
            for (int i = 0; i<danceData.poses.Count;i+=10)
            {
                move.Add(danceData.poses[i]);
            }

            scoringUtil.StartNewGoal(GoalType.MOTION, move, 0f);
            audioSource.Play();
        }

        // Update is called once per frame
        public void Update()
        {
            float timeOffset = audioSource.time - danceData.poses[currentId].timestamp;
            currentSelfPose = selfPoseInputGetter.GetNextPose();
            AnimateSelf(currentSelfPose);
            AnimateTeacher(danceData.GetInterpolatedPose(currentId, out currentId, timeOffset).toPoseData());

            scoringUtil.Update(currentSelfPose, audioSource.time);
        }

        public void FixedUpdate()
        {
           
        }

        public void OnApplicationQuit()
        {
            selfPoseInputGetter.Dispose();
            teacherPoseInputGetter.Dispose();
            
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