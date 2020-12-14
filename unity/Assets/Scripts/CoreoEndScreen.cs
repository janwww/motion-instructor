using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PoseTeacher {
    public class CoreoEndScreen : MonoBehaviour
    {

        private string CoreoName = "Coreo placeholder";
        private int Score = -1;

        public GameObject EndCoreoScreen;
        private GameObject CoreoScoreDescription;
        public GameObject CourseHelper;
        public GameObject Main;

        // Start is called before the first frame update
        void Start()
        {
            CoreoScoreDescription = EndCoreoScreen.transform.Find("TextContent").Find("CoreoScoreDescription").gameObject;
            EndCoreoScreen.SetActive(false);
        }

        public void ShowRecordingPressed()
        {
            Main.GetComponent<PoseteacherMain>().CoreoShowRestartRecording();
            //EndCoreoScreen.SetActive(false);
        }

        public void RestartCoreoPressed()
        {
            Main.GetComponent<PoseteacherMain>().RestartCoreo();
            EndCoreoScreen.SetActive(false);
        }

        public void NextMovePressed()
        {
            CourseHelper.GetComponent<CourseMenuHelper>().StartNextStep();
            EndCoreoScreen.SetActive(false);
        }

        public void SetCoreoName(string coreoName)
        {
            CoreoName = coreoName;
            UpdateText();
        }

        public void SetScore(int score)
        {
            Score = score;
            UpdateText();
        }

        public void SetRating()
        {

        }

        public void UpdateText()
        {
            Text text = CoreoScoreDescription.GetComponent<Text>();
            text.text = CoreoName + "\nScore: <color=yellow>" + Score + "</color>";
        }
    }
}
