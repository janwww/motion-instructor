using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher
{ 	
	[CLSCompliant(false)]
	public class AvatarSimilarity : MonoBehaviour
	{
		// get similarity of pose between 2 avatars
        // integrate selection which part is weighted how much e.g. 3 setups
		double GetSimilarity(AvatarContainer object1, AvatarContainer object2)
		{
			// define all stick names (should be actually moved to a parameter file)
			List<string> stickNames = new List<string>(new string[] {
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
			List<double> stickWeights = new List<double>(new double[] {
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
			double stickWeightSum = 0.0;
			foreach (double stickWeight in stickWeights)
			{
				stickWeightSum += stickWeight;
			}

			// get similarity (between 0 and 1) of orientation between all sticks
			double similarity = 0.0;
			for (int i = 0; i < stickNumber; i++)
			{
				// get position and orientation of relevant game objects
				object1.smplContainer.smpl.gameObject.SetActive(true);
				Vector3 vec1Position = GameObject.Find(stickNames[i]).transform.position;
				Quaternion vec1Rotation = GameObject.Find(stickNames[i]).transform.rotation;
				object1.smplContainer.smpl.gameObject.SetActive(false);
				object2.smplContainer.smpl.gameObject.SetActive(true);
				Vector3 vec2Position = GameObject.Find(stickNames[i]).transform.position;
				Quaternion vec2Rotation = GameObject.Find(stickNames[i]).transform.rotation;
				object2.smplContainer.smpl.gameObject.SetActive(false);

				// get cosine similarity from quaternion 
				// background: https://www.researchgate.net/publication/316447858_Similarity_analysis_of_motion_based_on_motion_capture_technology
				// background: https://gdalgorithms-list.narkive.com/9TaVDT9G/quaternion-similarity-measure
				double cos_angle = vec1Rotation.w * vec2Rotation.w + vec1Rotation.x * vec2Rotation.x + vec1Rotation.y * vec2Rotation.y + vec1Rotation.z * vec2Rotation.z;
				cos_angle = Math.Abs(cos_angle);
				similarity += cos_angle * stickWeights[i];
			}
			similarity /= stickWeightSum;
			return similarity;
		}
	}
}