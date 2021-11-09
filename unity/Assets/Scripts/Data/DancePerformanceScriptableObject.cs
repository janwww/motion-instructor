using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dance", menuName = "ScriptableObjects/DancePerformanceScriptableObject", order = 3)]
    public class DancePerformanceScriptableObject : ScriptableObject {
    public DanceDataScriptableObject danceData;
    public MusicDataScriptableObject SongObject;
    public DanceDifficulty Difficulty;
}
