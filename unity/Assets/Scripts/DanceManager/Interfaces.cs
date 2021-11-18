using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher {
    public interface IAvatarDisplay {
        public void SetPose(DancePose dancePose);

        public GameObject gameObject { get; }

    }

    public interface IGoalDisplay {
        public void showGoal(DanceData danceData, IDanceGoal goal, float songTime);
        public void showNothing();

        public GameObject gameObject { get; }
    }

    public interface IDancePoseSource {
        public DancePose GetDancePose();

        public GameObject gameObject { get; }
    }

}
