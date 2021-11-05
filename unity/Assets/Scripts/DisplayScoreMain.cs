using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DisplayScoreMain : MonoBehaviour
{

    public TextMeshPro text;
    public float score;
    public bool displaying;
    private Animation animation;
    private AudioSource audioData;

    // Start is called before the first frame update
    void Start()
    {
        text.text = "Start :)";
        score = 0;
        displaying = false;
        animation = text.GetComponent<Animation>();
        audioData = text.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (displaying)
        {
            text.enabled = true;
            if (!animation.isPlaying)
            {
                animation.Play("textAppear");
                audioData.Play(0);
            }

            if (score == 0)
            {
                text.text = "BAD";
                text.color = new Color(1.0f, 0.0f, 0.0f);
            }
            if (score > 0 && score < 1)
            {
                text.text = "GOOD";
                text.color = new Color(0.0f, 1.0f, 0.0f);
            }
            if (score >= 1 && score < 2)
            {
                text.text = "GREAT";
                text.color = new Color(0.22f, 0.56f, 0.22f);
            }

            displaying = false;
        }

        if(animation.isPlaying)
        {
            text.enabled = true;
        }
        else
        {
            text.enabled = false;
        }
    }

}
