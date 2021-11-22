using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Song", menuName = "ScriptableObjects/MusicDataScriptableObject", order = 1)]
public class MusicDataScriptableObject : ScriptableObject {
    public string SongName;
    public AudioClip SongClip;
    public float Volume = 1;
    public string Attribution;
}
