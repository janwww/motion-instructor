using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher {
    public class DancePoseSourceFromManager : MonoBehaviour, IDancePoseSource {
        private int currentPose = 0;

        DancePose IDancePoseSource.GetDancePose() {
            float timeOffset = DanceManager.SongTime - DanceManager.DanceData.poses[currentPose].timestamp;
            return DanceManager.DanceData.GetInterpolatedPose(currentPose, out currentPose, timeOffset);
        }
    }
}
