using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher
{ 
    public enum InputSource { KINECT, FILE }
    public abstract class PoseGetter
    {
        public PoseData CurrentPose { get; protected set; }
        public abstract PoseData GetNextPose();
        public abstract void Dispose(); 
    }


}

