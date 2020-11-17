using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PoseTeacher
{ 	
	class AvatarSimilarity : MonoBehaviour
	{
        public AvatarContainer self; // object containing self avatar
        public AvatarContainer teacher; // object containing teacher avatar
        public int body_part; // body part we want to show similarity for
        public List<string> stickNames; // all names of sticks existing in avatar
        public List<double> stickWeights; // weight to each stick in avatar
        public int stickNumber; // number of sticks existing in avatar
        public double stickWeightSum; // sum of all stick weights

        // constructor
        public AvatarSimilarity()
        {
            // define all stick names (should be actually moved to a parameter file)
            stickNames = new List<string>(new string[] {
                "LLLeg",
                "RLLeg",
                "LeftUpperArm",
                "RightUpperArm",
                "LeftUpperLeg",
                "RightUpperLeg",
                "TorsoLeft",
                "TorsoRight",
                "HipStick",
                "LeftLowerArm",
                "RightLowerArm",
                "LeftEye",
                "RightEye",
                "Shoulders",
                "MouthStick",
                "NoseStick",
                "LeftEar",
                "RightEar",
                "LeftShoulderStick",
                "RightShoulderStick",
                "LeftHipStick",
                "RightHipStick",
                "LeftElbowStick",
                "RightElbowStick",
                "LeftWristStick",
                "RightWristStick",
                "LeftKneeStick",
                "RightKneeStick",
                "LeftAnkleStick",
                "RightAnkleStick"
            });
            int stickNumber = stickNames.Count;

            // weight each stick
            stickWeights = new List<double>(new double[] {
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0
            });
            stickWeightSum = 0.0;
            foreach (double stickWeight in stickWeights)
            {
                stickWeightSum += stickWeight;
            }
        }

        // get similarity of pose between 2 avatars
        // integrate selection which part is weighted how much e.g. 3 setups
        public double GetSimilarity()
		{
			// get similarity (between 0 and 1) of orientation between all sticks
			double similarity = 0.0;
			for (int i = 0; i < stickNumber; i++)
			{
                // get position and orientation of relevant game objects
                Vector3 selfPosition = self.avatarContainer.gameObject.transform.position;
				Quaternion selfRotation = self.avatarContainer.gameObject.transform.rotation;
                Vector3 teacherPosition = teacher.avatarContainer.gameObject.transform.position;
                Quaternion teacherRotation = teacher.avatarContainer.gameObject.transform.rotation;

                // get cosine similarity from quaternion 
                // background: https://www.researchgate.net/publication/316447858_Similarity_analysis_of_motion_based_on_motion_capture_technology
                // background: https://gdalgorithms-list.narkive.com/9TaVDT9G/quaternion-similarity-measure
                double cos_angle = selfRotation.w * teacherRotation.w + selfRotation.x * teacherRotation.x + selfRotation.y * teacherRotation.y + selfRotation.z * teacherRotation.z;
				cos_angle = Math.Abs(cos_angle);
				similarity += cos_angle * stickWeights[i];
			}
			similarity /= stickWeightSum;
			return similarity;
		}
	}
}