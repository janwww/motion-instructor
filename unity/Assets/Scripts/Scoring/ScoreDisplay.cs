using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace PoseTeacher {
    public class ScoreDisplay : MonoBehaviour
    {
        public TextMeshPro text2;
        private TextMeshPro textMesh;
        private Animation animation;
        private AudioSource audioData;

        // Start is called before the first frame update
        void Start()
        {
            textMesh = gameObject.GetComponent<TextMeshPro>();
            textMesh.text = "";
            animation = gameObject.GetComponent<Animation>();
            audioData = gameObject.GetComponent<AudioSource>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void addScore(Scores score)
        {
            animation.Play("textAppear");
            audioData.Play(0);
            switch (score) {
                case Scores.BAD:
                    textMesh.text = "BAD";
                    textMesh.color = new Color(1.0f, 0.0f, 0.0f);
                    break;
                case Scores.GOOD:
                    textMesh.text = "GOOD";
                    textMesh.color = new Color(0.0f, 1.0f, 0.0f);
                    break;
                case Scores.GREAT:
                    textMesh.text = "GREAT";
                    textMesh.color = new Color(0.22f, 0.56f, 0.22f);
                    break;
            }
            StartCoroutine(TextDisappear(1));
        }

        private IEnumerator TextDisappear(float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            textMesh.text = "";
        }
    }
}
