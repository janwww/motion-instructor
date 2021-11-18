using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PoseTeacher;
using System;


[CreateAssetMenu(fileName = "Dance", menuName = "ScriptableObjects/DancePerformanceScriptableObject", order = 3)]
public class DancePerformanceScriptableObject : ScriptableObject {
    [SerializeField]
    public DanceDataScriptableObject danceData;
    [SerializeField]
    public MusicDataScriptableObject SongObject;
    [SerializeField]
    public DanceDifficulty Difficulty;
    [SerializeField]
    public List<DanceGoalPose> danceGoalPoses = new List<DanceGoalPose>();
    [SerializeField]
    public List<DanceGoalMotion> danceGoalMotions = new List<DanceGoalMotion>();

    public List<IDanceGoal> goals => GetDanceGoals();

    // get all goals in a sorted list
    public List<IDanceGoal> GetDanceGoals() {
        List<IDanceGoal> goals = new List<IDanceGoal>();
        goals.AddRange(danceGoalMotions);
        goals.AddRange(danceGoalPoses);
        goals.Sort();
        return goals;
    }
}
