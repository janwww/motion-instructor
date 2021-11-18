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
public class PoseDataCompressor : MonoBehaviour {
    public DanceDataScriptableObject inputDance;
    public string outputName;
    public bool outputCompressed;
    public bool convert;

    void Update() {
        if (convert) {
            convert = false;
            DanceData danceDataInput = inputDance.LoadDanceDataFromScriptableObject();
            DanceDataScriptableObject.SaveDanceDataToScriptableObject(danceDataInput, outputName, outputCompressed);
        }
    }
}


#endif