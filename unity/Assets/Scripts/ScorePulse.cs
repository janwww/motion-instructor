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
        }

        // Update is called once per frame
        void Update()
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
                MinorBeatSubscriber(ScoreRating.MISS);

            if (FramesSinceMayorBeat == 120)
                MayorBeatSubscriber(ScoreRating.GOOD);

        }

        public void MayorBeatSubscriber(ScoreRating rating = ScoreRating.UNRATED)
        {
            currentRating = rating;
            FramesSinceMayorBeat = 0;
        }

        public void MinorBeatSubscriber(ScoreRating rating = ScoreRating.UNRATED)
        {
            currentRating = rating;
            FramesSinceMinorBeat = 0;
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