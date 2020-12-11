using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAdjuster : MonoBehaviour
{
    //AudioListener listener;
    //public GameObject holder;
    private float soundLevel = 0;
    // Start is called before the first frame update
    void Start()
    {
      //  listener = holder.GetComponent<AudioListener>();    
    }

    // Update is called once per frame
    void Update()
    {
        
    }

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
