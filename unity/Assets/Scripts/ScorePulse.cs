using UnityEngine;

namespace PoseTeacher
{
    public enum ScoreRating
    {
        MISS, OK, GOOD, PERFECT, UNRATED
    }

    // Script for Pulsing object based on musical beats and scores
    public class ScorePulse : MonoBehaviour
    {
        // State to display the change of the object graduatly
        private int FramesSinceMayorBeat;
        private int FramesSinceMinorBeat;
        private bool EnabledMinorBeat = true;

        public GameObject PulsingObject; // Object that this script is assigned to

        // Audiofiles and objects. Consider moving them away from the pulsing object to their own script. (bring SetMute() function too)
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
        // TODO sync with music (use the beat and bar subscriber properly)
        // TODO consider using fixed rate update for scaling changes instead of frame based?
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

                FramesSinceMayorBeat += 3;
                FramesSinceMinorBeat += 3;

                // TODO remove this from here and call Beat and BarSubscriber from the correct place
                if (FramesSinceMinorBeat == 30)
                    BeatSubscriber(ScoreRating.OK);

                if (FramesSinceMayorBeat == 120)
                    BarSubscriber(ScoreRating.PERFECT);
            }
        }

        // pause the pulsing effect
        public void SetPause(bool isPaused)
        {
            this.isPaused = isPaused;
        }

        // mute the sound effects. If they are moved, move this function too.
        public void SetMute(bool isMuted)
        {
            this.isMuted = isMuted;
        }

        // Notify this object of a Bar thats happening.
        public void BarSubscriber(ScoreRating rating = ScoreRating.UNRATED)
        {
            currentRating = rating;
            FramesSinceMayorBeat = 0;
            if (!isMuted)
            {
                audioSource.PlayOneShot(conga_bar);
            }

        }

        // Notfiy this object of a Beat thats happening
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

        // Enable or Disable Beats (only Bars)
        public void EnableMinorBeat(bool enable)
        {
            EnabledMinorBeat = enable;
        }

        // Sets the color of the object based on the current rating
        private void SetMaterialColor()
        {
            switch (currentRating)
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