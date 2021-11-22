using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PoseTeacher;
using System;


[CreateAssetMenu(fileName = "Dance", menuName = "ScriptableObjects/DancePerformanceScriptableObject", order = 3)]
public class DancePerformanceScriptableObject : ScriptableObject {
    public DanceDataScriptableObject danceData;
    public List<DanceDataScriptableObject> goals;
    public List<float> goalTimestamps;
    public MusicDataScriptableObject SongObject;
    public DanceDifficulty Difficulty;
}
