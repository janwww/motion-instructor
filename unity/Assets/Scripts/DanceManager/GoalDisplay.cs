using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

namespace PoseTeacher {
    public class GoalDisplay : MonoBehaviour, IGoalDisplay {
        public VisualEffect vfx;
        public float PoseGoalTestTime = 1.3f;
        public float slowMoFactor = 0.7f;

        private int currentPose = 0;
        private AvatarContainer avatarContainer;

        void Start() {
            avatarContainer = new AvatarContainer(gameObject);
            avatarContainer.ChangeActiveType(AvatarType.ROBOT);
            vfx.SetFloat("alpha", 0);
        }

        public void showGoal(DanceData danceData, IDanceGoal goal, float songTime) {
            if(goal.GetGoalType() == GoalType.MOTION) {
                DanceGoalMotion goalMotion = (DanceGoalMotion)goal;
                // Todo
                return;

            } else if(goal.GetGoalType() == GoalType.POSE) {
                DanceGoalPose goalPose = (DanceGoalPose)goal;

                // check if its time to do the goal pose
                if (danceData.poses[goalPose.id].timestamp - PoseGoalTestTime < songTime) {
                    float timedifference = danceData.poses[goalPose.id].timestamp - songTime;

                    // find opacity
                    float opacity = Mathf.Clamp01(Mathf.Pow(5, -Mathf.Abs(timedifference) / PoseGoalTestTime) - 0.22f);
                    int lel = 0;
                    avatarContainer.MovePerson(danceData.GetInterpolatedPose(currentPose, out lel, danceData.poses[goalPose.id].timestamp - slowMoFactor * timedifference).toPoseData());
                    vfx.SetFloat("alpha", opacity);

                } else {
                    vfx.SetFloat("alpha", 0f);
                }
            }
        }

        public void showNothing() {
            vfx.SetFloat("alpha", 0f);
        }
    }
}
