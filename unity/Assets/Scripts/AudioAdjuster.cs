using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAdjuster : MonoBehaviour
{
    // Storage for non-muted sound level to restore after unmute
    private float soundLevel = 0;

    // Mutes if Audio is currently unmuted, unmutes otherwise.
    // Mutes all AudioSource in the scene.
    public void MuteOrUnmute()
    {
        if (AudioListener.volume != 0)
        {
            soundLevel = AudioListener.volume;
            AudioListener.volume = 0;
        } else
        {
            AudioListener.volume = soundLevel;
        }
    }
}
