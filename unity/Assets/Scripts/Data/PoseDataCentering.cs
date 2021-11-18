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
public class PoseDataCentering : MonoBehaviour {
    public DanceDataScriptableObject inputDance;
    public string outputName;
    public bool compressed;
    public bool convert;

    void Update() {
        if (convert) {
            convert = false;
            DanceData danceDataInput = inputDance.LoadDanceDataFromScriptableObject();

            // Normalize all pelvis positions by subtracting the starting pelvis position from all pelvis positions
            Vector3 startingPosition = danceDataInput.poses[0].positions[0];
            foreach (var poses in danceDataInput.poses) {
                poses.positions[0] -= startingPosition;
            }
            DanceDataScriptableObject.SaveDanceDataToScriptableObject(danceDataInput, outputName, compressed);
        }
    }
}


#endif