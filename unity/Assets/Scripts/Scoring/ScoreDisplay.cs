using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PoseTeacher {
    public class ScoreDisplay : MonoBehaviour
    {
        public UnityEngine.UI.Text text;

        // Start is called before the first frame update
        void Start()
        {
            text = gameObject.GetComponent<UnityEngine.UI.Text>();
            text.text = "";
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void addScore(Scores score)
        {
            switch (score) {
                case Scores.BAD:
                    text.text += "BAD\n";
                    break;
                case Scores.GOOD:
                    text.text += "GOOD\n";
                    break;
                case Scores.GREAT:
                    text.text += "GREAT\n";
                    break;
            }
        }
    }
}
