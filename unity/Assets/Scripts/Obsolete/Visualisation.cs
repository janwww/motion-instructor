using System.Collections.Generic;
using UnityEngine;
using PoseTeacher;
namespace PoseTeacherOld
{
    class VisualisationSimilarity
    {   //Class for the visualisation of score similarity
        public AvatarContainer selfAvatarContainer;
        public List<GameObject> stickparts;
        private Color correctColor = Color.green;
        private Color incorrectColor = Color.red;
        private Color neutralColor = Color.blue;

        public VisualisationSimilarity(AvatarContainer selfIn)
        {
            selfAvatarContainer = selfIn;
            stickparts = selfAvatarContainer.stickContainer.StickList;
            SetColor();
        }

        public void Update(List<double> weights)
        {
            // set gradient color according to Similarity [weights] to all body parts
            int index = 0;
            foreach (var body_part in stickparts)
            {
                set_grad_color(body_part, incorrectColor, correctColor, weights[index]);
                index++;
            }
        }

        public void UpdatePart(BodyWeightsType bodyNrIn, List<double> weights_score)
        {
            // update a color for a specific body part bodyNrIn. Sets grad color to "1" and blue to "0"
            List<double> weights = SimilarityConst.GetStickWeights(bodyNrIn);

            for (var i = 0; i < weights.Count; i++)
            {
                if (weights[i] == 0)
                {
                    SetColor(stickparts[i], neutralColor);
                }
                if (weights[i] == 1)
                {
                    set_grad_color(stickparts[i], incorrectColor, correctColor, weights_score[i]);
                }
            }
        }

        public void SetColor()
        {
            //set color to all body parts
            foreach (var body_part in stickparts)
            {
                body_part.GetComponent<Renderer>().material.color = correctColor;
            }
        }

        public void set_grad_color(GameObject body, Color begin, Color end, double weight)
        {
            //set a color as liner interpolation between colors begin and end.
            Color tempcolor = body.GetComponent<Renderer>().material.color;
            tempcolor.r = (float)(begin.r + (end.r - begin.r) * weight);
            tempcolor.g = (float)(begin.g + (end.g - begin.g) * weight);
            tempcolor.b = (float)(begin.b + (end.b - begin.b) * weight);

            body.GetComponent<Renderer>().material.color = tempcolor;
        }

        public void SetColor(GameObject body, Color c)
        {
            //set a permanent color c to the body part body
            body.GetComponent<Renderer>().material.color = c;
        }
    }
}


