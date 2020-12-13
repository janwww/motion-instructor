using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.IO;
using System.Linq;
using System.Globalization;


namespace PoseTeacher
{
    class VisualisationSimilarity //: MonoBehaviour
    {
        public AvatarContainer self;
        public List<string> stickNames;
        public List<GameObject> stickparts;
        public List<double> weightsTop;
        public List<double> weightsMiddle;
        public List<double> weightsBottom;
        public List<double> weightsTotal;
        public int stickNumber; // number of sticks existing in avatar
        public GameObject bodypart;
        public Color tempcolor;
        int index;
        double weight;
        public string file_weights;
        public Dictionary<string, List<double>> weights_dictionary;
        // AvatarSimilarity avatarSimilarity;

        public VisualisationSimilarity(AvatarContainer selfIn)
        {
            string file_weights = "jsondata/joints.csv";
            weights_dictionary = set_weights(file_weights);
            self = selfIn;
            //stickNumber = stickNames.Count;
            stickparts = new List<GameObject>(new GameObject[]
            {   //list of body joints
                self.stickContainer.LeftLowerLeg,
                self.stickContainer.RightLowerLeg,
                self.stickContainer.LeftUpperArm,
                self.stickContainer.RightUpperArm,
                self.stickContainer.LeftUpperLeg,
                self.stickContainer.RightUpperLeg,
                self.stickContainer.TorsoLeft,
                self.stickContainer.TorsoRight,
                self.stickContainer.HipStick,
                self.stickContainer.LeftLowerArm,
                self.stickContainer.RightLowerArm,
                self.stickContainer.LeftEye,
                self.stickContainer.RightEye,
                self.stickContainer.Shoulders,
                self.stickContainer.MouthStick,
                self.stickContainer.NoseStick,
                self.stickContainer.LeftEar,
                self.stickContainer.RightEar,
                self.stickContainer.LeftShoulderStick,
                self.stickContainer.RightShoulderStick,
                self.stickContainer.LeftHipStick,
                self.stickContainer.RightHipStick,
                self.stickContainer.LeftElbowStick,
                self.stickContainer.RightElbowStick,
                self.stickContainer.LeftWristStick,
                self.stickContainer.RightWristStick,
                self.stickContainer.LeftKneeStick,
                self.stickContainer.RightKneeStick,
                self.stickContainer.LeftAnkleStick,
                self.stickContainer.RightAnkleStick
        });
     
            setColor(self);
            // avatarSimilarity = new AvatarSimilarity(self); //if we want to inherit from AvatarSimilarity
        }
         

        public void Update(List<double> weights)
        {
            // set gradient color according to Similarity [weights] to all body parts
            int index = 0;
            foreach (var body_part in stickparts)
            {
                set_grad_color(body_part, Color.red, Color.green, weights[index]);
                index++;
            }
        }
        public void UpdatePart(string bodyNrIn, List<double> weights_score)
        {
            // update a color for a specific body part bodyNrIn. Sets grad color to "1" and blue to "0"
            List<double> weights = get_weights(bodyNrIn);
            for (var i = 0; i < weights.Count; i++)
            {
                if (weights[i] == 0)
                {
                    set_color(stickparts[i], Color.blue);
                }
                if (weights[i] == 1)
                {
                    set_grad_color(stickparts[i], Color.red, Color.green, weights_score[i]);
                }
            }
        }

        public void setColor(AvatarContainer selfin)
        {   
            //set permanent color to all body parts
           
            foreach (var body_part in stickparts)
            {
                body_part.GetComponent<Renderer>().material.color = Color.green;
            }

        }

        public void set_grad_color(GameObject body, Color begin, Color end, double weight)
        {
            //set a color as liner interpolation between colors begin and end.
            tempcolor = body.GetComponent<Renderer>().material.color;
            tempcolor.r = (float)(begin.r + (end.r - begin.r) * weight);
            tempcolor.g = (float)(begin.g + (end.g - begin.g) * weight);
            tempcolor.b = (float)(begin.b + (end.b - begin.b) * weight);
            //tempcolor.r = (float)(1 - weight); // just with red sub color
            body.GetComponent<Renderer>().material.color = tempcolor;
        }

        public void set_color(GameObject body, Color c)
        {
            //set a permanent color c to the body part body
            body.GetComponent<Renderer>().material.color = c;
        }

        List<double> get_weights(string name)
        {
            //returns binary mask for body parts. Takes name as a body part description 
            //weights can be retrieved from stickweight field in the class AvatarSimilarity

            // List<double> return_weights= weightsTotal;
            // initially all similarities are 1 which corresponds to green
            List<double> return_weights = Enumerable.Repeat(1.0, 30).ToList();  
            if (name.Equals("top"))
            {
               return_weights = weights_dictionary["arms"];
               //return_weights = avatarSimilarity.stickWeight;
            }
            if (name.Equals("middle"))
            {
                return_weights = weights_dictionary["middle"];
                
            }
            if (name.Equals("bottom"))
            {
                return_weights = weights_dictionary["bottom"];
                
            }
            if (name.Equals("main"))
            {
                return_weights = weights_dictionary["main"];
            }
            if (name.Equals("legs"))
            {
                return_weights = weights_dictionary["legs"];
                
            }
            if (name.Equals("head"))
            {
                return_weights = weights_dictionary["head"];
                
            }

            return return_weights;

        }

        Dictionary<string, List<double>> set_weights(string file)
        {
            //reads binary mask for body parts from csv file. Saves space!
            Dictionary<string, List<double>> weights_d = new Dictionary<string, List<double>>();

            using (var rd = new StreamReader(file))
            {
                var names = rd.ReadLine().Split(',');
                while (!rd.EndOfStream)
                {
                    var splits = rd.ReadLine().Split(',');
                    
                    for (int index = 0; index < splits.Count(); index++)
                    {
                        weight = Double.Parse(splits[index]);
                        if (weights_d.ContainsKey(names[index]))
                        {
                            weights_d[names[index]].Add(weight);
                        }
                        else
                        {
                            weights_d.Add(names[index], new List<double> { weight });
                        }
                    }
                }

            }
            return weights_d;
        }
    }

}


