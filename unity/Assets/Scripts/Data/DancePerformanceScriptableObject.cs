using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PoseTeacher;
using System;
using UnityEditor;

[CreateAssetMenu(fileName = "Dance", menuName = "ScriptableObjects/DancePerformanceScriptableObject", order = 3)]
public class DancePerformanceScriptableObject : ScriptableObject {
    public DanceDataScriptableObject danceData;
    [Tooltip("If True, Goals are created dynamically based on timestamps")]
    public bool dynamicGoals;
    public List<float> goalStartTimestamps;
    [Tooltip("Leave empty if dynamicGoals is true")]
    [SerializeField] List<DanceDataScriptableObject> explicitGoals;
    [Tooltip("Leave empty if dynamicGoals is false")]
    [SerializeField] List<float> goalDuration;
    public MusicDataScriptableObject SongObject;
    public DanceDifficulty Difficulty;

    public List<DanceData> goals {
        get
        {
            List<DanceData> intermediateGoals = new List<DanceData>();
            if (dynamicGoals)
            {
                DanceData decompressedDancedata = danceData.LoadDanceDataFromScriptableObject();
                for (int i = 0; i < goalStartTimestamps.Count; i++)
                {
                    DanceData newDanceData = new DanceData();
                    int id = 0;
                    if (goalDuration[i] > 0)
                    {
                        for (float timestamp = 0f; timestamp <= goalDuration[i]; timestamp += 0.1f)
                        {
                            newDanceData.poses.Add(decompressedDancedata.GetInterpolatedPose(0, out id, goalStartTimestamps[i] + timestamp));
                            newDanceData.poses[newDanceData.poses.Count - 1].timestamp = timestamp;
                        }
                    }
                    else
                    {
                        newDanceData.poses.Add(decompressedDancedata.GetInterpolatedPose(0, out id, goalStartTimestamps[i]));
                    }
                    intermediateGoals.Add(newDanceData);
                }
            } else
            {
                foreach(DanceDataScriptableObject obj in explicitGoals)
                {
                    intermediateGoals.Add(obj.LoadDanceDataFromScriptableObject());
                }
            }
            return intermediateGoals;
        }
    }
}


