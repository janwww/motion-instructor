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

        public GameObject avatarContainerSelf, avatarContainerTeacher;
        List<AvatarContainer> avatarListSelf, avatarListTeacher;

        private readonly string dance_file = "jsondata/salsa_m/2020_12_14-15_46_29.txt";
        private readonly string move_file = "jsondata/move1.txt";
        private readonly string fake_file = "jsondata/2020_05_27-00_01_59.txt";
        public PoseInputSource selfPoseInputSource = PoseInputSource.KINECT;

        ScoringScript scoringUtil;

        public bool paused = true;

        PoseData currentSelfPose;

        List<PoseData> move = new List<PoseData>();

        int currentframe = 0;

        // Start is called before the first frame update
        void Start()
        {
            avatarListSelf = new List<AvatarContainer>();
            avatarListTeacher = new List<AvatarContainer>();
            avatarListSelf.Add(new AvatarContainer(avatarContainerSelf));
            avatarListTeacher.Add(new AvatarContainer(avatarContainerTeacher));

            selfPoseInputGetter = new PoseInputGetter(selfPoseInputSource) { ReadDataPath = dance_file };
            teacherPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE) { ReadDataPath = dance_file };
            selfPoseInputGetter.loop = true;
            teacherPoseInputGetter.loop = false;

            selfPoseInputGetter.VideoCube = videoCube;
            
            scoringUtil = new ScoringScript(scoreDisplay);

            List<string> sequence = File.ReadLines(move_file).ToList();
            for (int i = 0; i<sequence.Count;i+=10)
            {
                move.Add(PoseDataUtils.JSONstring2PoseData(sequence[i]));
            }
            scoringUtil.StartNewGoal(GoalType.MOTION, move);
        }

        // Update is called once per frame
        private void Update()
        {
            
        }

        private void FixedUpdate()
        {
            
            if (!paused)
            {
                currentframe += 1;
                currentSelfPose = selfPoseInputGetter.GetNextPose();
                AnimateSelf(currentSelfPose);
                AnimateTeacher(teacherPoseInputGetter.GetNextPose());

                if (currentframe >= 10)
                {
                    currentframe = 0;
                    scoringUtil.Update(currentSelfPose);
                }
            }
        }

        private void OnApplicationQuit()
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