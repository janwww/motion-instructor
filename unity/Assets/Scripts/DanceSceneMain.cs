using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher
{

    public class DanceSceneMain : MonoBehaviour
    {
        PoseInputGetter selfPoseInputGetter, teacherPoseInputGetter;

        public GameObject videoCube;

        public GameObject avatarContainerSelf, avatarContainerTeacher;
        List<AvatarContainer> avatarListSelf, avatarListTeacher;

        private readonly string fake_file = "jsondata/2020_05_27-00_01_59.txt";
        public PoseInputSource selfPoseInputSource = PoseInputSource.KINECT;

        ScoringScript scoringUtil;

        public bool paused = true;

        PoseData currentSelfPose;

        // Start is called before the first frame update
        void Start()
        {
            avatarListSelf = new List<AvatarContainer>();
            avatarListTeacher = new List<AvatarContainer>();
            avatarListSelf.Add(new AvatarContainer(avatarContainerSelf));
            avatarListTeacher.Add(new AvatarContainer(avatarContainerTeacher));

            selfPoseInputGetter = new PoseInputGetter(selfPoseInputSource) { ReadDataPath = fake_file };
            teacherPoseInputGetter = new PoseInputGetter(PoseInputSource.FILE) { ReadDataPath = fake_file };
            selfPoseInputGetter.loop = true;
            teacherPoseInputGetter.loop = true;

            selfPoseInputGetter.VideoCube = videoCube;
            
            scoringUtil = new ScoringScript(avatarListSelf[0]);
        }

        // Update is called once per frame
        private void Update()
        {
            if (!paused)
            {
                currentSelfPose = selfPoseInputGetter.GetNextPose();
                AnimateSelf(currentSelfPose);
                AnimateTeacher(teacherPoseInputGetter.GetNextPose());
            }
        }

        private void FixedUpdate()
        {

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