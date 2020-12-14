using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher
{
    public enum ScoreRating
    {
        MISS, OK, GOOD, PERFECT, UNRATED
    }

    public class ScorePulse : MonoBehaviour
    {
        private int FramesSinceMayorBeat;
        private int FramesSinceMinorBeat;
        private bool EnabledMinorBeat = true;

        public GameObject PulsingObject;

        private AudioSource audioSource;
        public AudioClip clave_beat1;
        public AudioClip clave_beat2;
        public AudioClip clave_beat3;
        public AudioClip clave_beat4;
        public AudioClip conga_bar;
        private int beat_counter = 0;
        
        private bool isPaused = false;
        public bool isMuted = false;

        private Color BaseColor = new Color(1, 1, 1);
        private Color PerfectColor = new Color(0, 1, 0);
        private Color GoodColor = new Color(1, 1, 0);
        private Color OkColor = new Color(1, 0, 0);
        private Color MissColor = new Color(0.3f, 0.3f, 0.3f);
        private ScoreRating currentRating = ScoreRating.UNRATED;
        Material material;

        // Start is called before the first frame update
        void Start()
        {
            FramesSinceMayorBeat = 0;
            FramesSinceMinorBeat = 0;
            material = PulsingObject.GetComponent<Renderer>().material;
            audioSource = PulsingObject.GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {
            if (!isPaused)
            {
                if (FramesSinceMinorBeat < 5)
                {
                    PulsingObject.transform.localScale += new Vector3(0.01f, 0.01f, 0.01f);
                    SetMaterialColor();
                }
                else if (FramesSinceMinorBeat < 10)
                {
                    PulsingObject.transform.localScale -= new Vector3(0.01f, 0.01f, 0.01f);
                    currentRating = ScoreRating.UNRATED;
                    SetMaterialColor();
                }

                if (FramesSinceMayorBeat < 5)
                {
                    PulsingObject.transform.localScale += new Vector3(0.02f, 0.02f, 0.02f);
                    SetMaterialColor();
                }
                else if (FramesSinceMayorBeat < 10)
                {
                    PulsingObject.transform.localScale -= new Vector3(0.02f, 0.02f, 0.02f);
                    currentRating = ScoreRating.UNRATED;
                    SetMaterialColor();
                }

                FramesSinceMayorBeat++;
                FramesSinceMinorBeat++;

                if (FramesSinceMinorBeat == 30)
                    BeatSubscriber(ScoreRating.OK);

                if (FramesSinceMayorBeat == 120)
                    BarSubscriber(ScoreRating.PERFECT);
            }
        }

        public void SetPause(bool isPaused)
        {
            this.isPaused = isPaused;
        }

        public void SetMute(bool isMuted)
        {
            this.isMuted = isMuted;
        }

        public void BarSubscriber(ScoreRating rating = ScoreRating.UNRATED)
        {
            currentRating = rating;
            FramesSinceMayorBeat = 0;
            //beat_counter = 0;
            if (!isMuted)
            {
                audioSource.PlayOneShot(conga_bar);
            }
                
        }

        public void BeatSubscriber(ScoreRating rating = ScoreRating.UNRATED)
        {
            currentRating = rating;
            FramesSinceMinorBeat = 0;
            beat_counter = (beat_counter + 1) % 4;
            if (!isMuted)
            {
                switch (beat_counter)
                {
                    case 0: audioSource.PlayOneShot(clave_beat1); break;
                    case 1: audioSource.PlayOneShot(clave_beat2); break;
                    case 2: audioSource.PlayOneShot(clave_beat3); break;
                    case 3: audioSource.PlayOneShot(clave_beat4); break;
                    default: break;
                }
            }  
        }

        public void EnableMinorBeat(bool enable)
        {
            EnabledMinorBeat = enable;
        }

        private void SetMaterialColor()
        {
            switch(currentRating)
            {
                case ScoreRating.UNRATED: material.color = BaseColor; break;
                case ScoreRating.MISS: material.color = MissColor; break;
                case ScoreRating.OK: material.color = OkColor; break;
                case ScoreRating.GOOD: material.color = GoodColor; break;
                case ScoreRating.PERFECT: material.color = PerfectColor; break;
            }
        }
    }
}