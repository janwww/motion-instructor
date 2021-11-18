#if UNITY_EDITOR
// Only used in Editor

using Microsoft.Azure.Kinect.BodyTracking;
using PoseTeacher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class PoseDataConverter : MonoBehaviour {
    public DanceDataScriptableObject danceToLoadTest;
    public bool testLoadTime = false;
    public bool save = false;
    // compressing saved about 50%
    public bool saveCompressed = false;
    public string loadingFilename;
    public string savingFilename;

    // use default 50HZ
    public float conversionTimestep = 0.02f;

    void Update() {

        // if save is ticked, load old JSON and save it as a new scriptable object
        if (save) {
            save = false;
            DanceData newDanceData = new DanceData();

            float time = 0;

            List<PoseData> oldPoseData = new List<PoseData>();
            List<string> sequence = File.ReadLines(loadingFilename).ToList();
            for (int i = 0; i < sequence.Count; i += 10) {
                oldPoseData.Add(PoseDataUtils.JSONstring2PoseData(sequence[i]));
            }

            foreach (var pose in oldPoseData) {
                DancePose newPose = new DancePose();
                newPose.timestamp = time;
                time += conversionTimestep;
                for (int i = 0; i < (int) JointId.Count; i++) {
                    newPose.positions[i] = pose.data[i].Position;
                    newPose.orientations[i] = pose.data[i].Orientation;
                }
                newPose.reducePrecision(5);
                newDanceData.poses.Add(newPose);
            }
            DanceDataScriptableObject.SaveDanceDataToScriptableObject(newDanceData, savingFilename, saveCompressed);
        }

        if (testLoadTime) {
            testLoadTime = false;
            DateTime before = DateTime.Now;
            DanceData danceData = danceToLoadTest.LoadDanceDataFromScriptableObject();
            DateTime after = DateTime.Now;
            TimeSpan duration = after.Subtract(before);
            Debug.Log("Duration in milliseconds: " + duration.Milliseconds);
        }
    }
}




#endif
