﻿using UnityEngine;
using UnityEngine.UI;

namespace PoseTeacherOld
{
    public class CoreoEndScreen : MonoBehaviour
    {

        private string CoreoName = "Coreo placeholder";
        private int Score = -1;
        private int MaxScore = -1;

        // The EndCoreoScreen GameObject
        public GameObject EndCoreoScreen;
        private GameObject _CoreoScoreDescription;
        private GameObject CoreoScoreDescription
        {
            get
            {
                if (_CoreoScoreDescription == null)
                    _CoreoScoreDescription = EndCoreoScreen.transform.Find("TextContent").Find("CoreoScoreDescription").gameObject;
                return _CoreoScoreDescription;
            }
        }
        // The CourseMenuHelper GameObject in Menu new
        public GameObject CourseHelper;
        // The Main GameObject
        public GameObject Main;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Code that runs when the Show Recording button is pressed on the Coreo End Screen.
        public void ShowRecordingPressed()
        {
            Main.GetComponent<PoseteacherMain>().CoreoShowRestartRecording();
        }

        // Code that runs when the Restart Coreography button is pressed on the Coreo End Screen.
        public void RestartCoreoPressed()
        {
            Main.GetComponent<PoseteacherMain>().RestartCoreo();
            EndCoreoScreen.SetActive(false);
        }

        // Code that runs when the Next Move button is pressed on the Coreo End Screen.
        public void NextMovePressed()
        {
            CourseHelper.GetComponent<CourseMenuHelper>().StartNextStep();
            EndCoreoScreen.SetActive(false);
        }

        // Sets and updates the name of the Coreography to the input string.
        public void SetCoreoName(string coreoName)
        {
            CoreoName = coreoName;
            UpdateText();
        }

        // Sets and updates the current and max score of the Coreography according to the input values.
        public void SetScore(int score, int maxscore)
        {
            Score = score;
            MaxScore = maxscore;
            UpdateText();
        }

        // TODO Sets the rating (1 to 5 stars) of the currentyl played Coreography.
        public void SetRating()
        {

        }

        // Updates the displayed text on the Coreo End Screen with the Coreography name and achieved score.
        public void UpdateText()
        {
            Text text = CoreoScoreDescription.GetComponent<Text>();
            text.text = CoreoName + "\nScore: <color=yellow>" + Score + "/" + MaxScore + "</color>";
        }
    }
}