using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;
using System.IO;
using System;

namespace PoseTeacher
{
    [System.Serializable]
    public class DanceData
    {
        public List<DancePose> poses = new List<DancePose>();

        // returns the interpolated pose for id and offsetTime and return id+1 in newId if offsetTime is larger than timestamp difference
        public DancePose GetInterpolatedPose(int id, out int newId, float offsetTime)
        {
            if (id >= poses.Count - 2)
            {
                newId = id;
                return poses[poses.Count - 1];
            }

            while (poses[id].timestamp + offsetTime > poses[id + 1].timestamp)
            {
                id += 1;
                if (id >= poses.Count - 2)
                {
                    newId = id;
                    return poses[poses.Count - 1];
                }
                offsetTime = offsetTime - (poses[id + 1].timestamp - poses[id].timestamp);
            }

            newId = id;
            float t = offsetTime / (poses[id + 1].timestamp - poses[id].timestamp);
            DancePose interPose = new DancePose();
            interPose.timestamp = poses[id].timestamp + offsetTime;
            for (int i = 0; i < interPose.positions.Length; i++)
            {
                interPose.positions[i] = Vector3.Slerp(poses[id].positions[i], poses[id + 1].positions[i], t);
                interPose.orientations[i] = Quaternion.Lerp(poses[id].orientations[i], poses[id + 1].orientations[i], t);
            }
            return interPose;
        }

        public string SaveToJSON()
        {
            foreach (var pose in poses)
            {
                pose.reducePrecision(5);
            }

            string jsonString = JsonUtility.ToJson(this);
            DateTime dt = DateTime.Now;
            File.WriteAllText("jsondata/" + dt.ToString("yyyy-MM-dd-HH-mm-ss") + "recording.json", jsonString);
            return "Saved as " + dt.ToString("yyyy-MM-dd-HH-mm-ss") + "recording.json";
        }

        public static DanceData LoadFromJSON(string fileName)
        {
            string jsonString = File.ReadAllText("jsondata/" + fileName);
            return JsonUtility.FromJson<DanceData>(jsonString);
        }
    }

    public enum DanceDifficulty
    {
        Easy,
        Medium,
        Hard,
        Expert
    }

    [System.Serializable]
    public class DancePose
    {
        public Vector3[] positions = new Vector3[(int)JointId.Count];
        public Quaternion[] orientations = new Quaternion[(int)JointId.Count];
        public float timestamp;

        public void reducePrecision(int digits)
        {
            for (int i = 0; i < positions.Length; i++)
            {
                var oldVector = positions[i];
                positions[i] = new Vector3(float.Parse(oldVector.x.ToString("N" + digits)), float.Parse(oldVector.y.ToString("N" + digits)), float.Parse(oldVector.z.ToString("N" + digits)));
            }

            for (int i = 0; i < orientations.Length; i++)
            {
                var oldRot = orientations[i];
                orientations[i] = new Quaternion(float.Parse(oldRot.x.ToString("N" + digits)), float.Parse(oldRot.y.ToString("N" + digits)), float.Parse(oldRot.z.ToString("N" + digits)), float.Parse(oldRot.w.ToString("N" + digits)));
            }
        }

        public PoseData toPoseData()
        {
            PoseData poseData = new PoseData();
            poseData.data = new JointData[(int)JointId.Count];

            for (int i = 0; i < (int)JointId.Count; i++)
            {
                JointData jointData = new JointData();
                jointData.Position = positions[i];
                jointData.Orientation = orientations[i];
                poseData.data[i] = jointData;
            }
            return poseData;
        }
    }

}