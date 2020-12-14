using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PoseTeacher
{ 	
	class AvatarSimilarity //: MonoBehaviour
	{
        // input
        public AvatarContainer self; // object containing self avatar
        public AvatarContainer teacher; // object containing teacher avatar
        public String bodyNr; // body part we want to show similarity for
        public List<string> stickNames; // all names of sticks existing in avatar
        public List<double> stickWeightBody; // weight to each stick in avatar based on body variable
        public List<double> stickWeightAdapt; // static correction weight to each stick in avatar
        public List<double> stickWeight; // temporary weight value for all body sticks
        public int stickNumber; // number of sticks existing in avatar
        public double stickWeightTotal; // sum of all stick weights
        public double totalScore; // total score

        // output
        public double similarityBodypart; // similarity for defined bodypart
        public List<double> similarityStick; // similarity for each stick in stickNames

        // filter
        public double penalty = 0.0;
        public double kalmanQ = 0.0001;
        public double kalmanR = 1.0;
        bool activateKalman = true;
        public List<KalmanFilter> kalmanFilter;

        // constructor
        public AvatarSimilarity(AvatarContainer selfIn, AvatarContainer teacherIn, String bodyNrIn, double penaltyIn, bool activateKalmanIn, double kalmanQIn, double kalmanRIn)
        {

            // assign
            ///////////////////////////////////////////////////////////////////////////////////
            self = selfIn;
            teacher = teacherIn;
            bodyNr = bodyNrIn;
            penalty = penaltyIn;
            kalmanQ = kalmanQIn;
            kalmanR = kalmanRIn;
            stickNumber = 0;
            activateKalman = activateKalmanIn;

            // initialize
            ///////////////////////////////////////////////////////////////////////////////////
            SetBody(bodyNr, penaltyIn, kalmanQIn, kalmanRIn);
        }

        public void SetBody(String bodyNrIn, double penaltyIn, double kalmanQIn, double kalmanRIn)
        {
            // assign
            ///////////////////////////////////////////////////////////////////////////////////
            bodyNr = bodyNrIn;
            penalty = penaltyIn;
            kalmanQ = kalmanQIn;
            kalmanR = kalmanRIn;

            // parameters
            ///////////////////////////////////////////////////////////////////////////////////
            // define all stick names (should be actually moved to a parameter file)
            stickNames = new List<string>(new string[] {
                "LeftLowerLeg",
                "RightLowerLeg",
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
            stickNumber = stickNames.Count;

            // weight each stick
            stickWeight = new List<double>(new double[] {
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

            if (bodyNr.Equals("total")) {
                // weight each stick
                stickWeightBody = new List<double>(new double[] {
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
            };
            if (bodyNr.Equals("top"))
            {
                // weight each stick
                stickWeightBody = new List<double>(new double[] {
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
            });
            };
            if (bodyNr.Equals("middle"))
            {
                // weight each stick
                stickWeightBody = new List<double>(new double[] {
                0.0,
                0.0,
                1.0,
                1.0,
                0.0,
                0.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
            });
            };
            if (bodyNr.Equals("bottom"))
            {
                // weight each stick
                stickWeightBody = new List<double>(new double[] {
                1.0,
                1.0,
                0.0,
                0.0,
                1.0,
                1.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                1.0,
                1.0,
                1.0,
                1.0
            });
            };

            // initialize
            ///////////////////////////////////////////////////////////////////////////////////

            // correct each default stick weight
            stickWeightAdapt = new List<double>(new double[] {
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

            // calculate final stick weight
            stickWeightTotal = 0.0;
            for (int i = 0; i < stickNumber; i++)
            {
                stickWeightTotal += stickWeightBody[i] * stickWeightAdapt[i];
            }

            // set default similarity each stick
            similarityStick = new List<double>(new double[] {
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0,
                0.0
            });

            // generate kalman filters
            kalmanFilter = new List<KalmanFilter>(new KalmanFilter[stickNumber]);
            for (int i = 0; i < stickNumber; i++)
            {
                kalmanFilter[i] = new KalmanFilter(kalmanQ, kalmanR);
                kalmanFilter[i].Reset(1.0);
            }


            // total score
            totalScore = 0.0;
        }

        // get similarity of pose between 2 avatars
        // integrate selection which part is weighted how much e.g. 3 setups
        public void Update()
		{
			// get similarity (between 0 and 1) of orientation between all sticks
			double similarityTotal = 0.0;
            double stickWeightPenalty = 1.0;
            stickWeightTotal = 0.0;
            for (int i = 0; i < stickNumber; i++)
			{
                // get position and orientation of relevant game objects
                Vector3 selfPosition;
                Quaternion selfRotation;
                Vector3 teacherPosition;
                Quaternion teacherRotation;

                switch (i)
                {
                    case 0:
                        selfPosition = self.stickContainer.LeftLowerLeg.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftLowerLeg.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftLowerLeg.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftLowerLeg.gameObject.transform.rotation;
                        break;
                    case 1:
                        selfPosition = self.stickContainer.RightLowerLeg.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightLowerLeg.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightLowerLeg.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightLowerLeg.gameObject.transform.rotation;
                        break;
                    case 2:
                        selfPosition = self.stickContainer.LeftUpperArm.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftUpperArm.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftUpperArm.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftUpperArm.gameObject.transform.rotation;
                        break;
                    case 3:
                        selfPosition = self.stickContainer.RightUpperArm.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightUpperArm.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightUpperArm.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightUpperArm.gameObject.transform.rotation;
                        break;
                    case 4:
                        selfPosition = self.stickContainer.LeftUpperLeg.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftUpperLeg.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftUpperLeg.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftUpperLeg.gameObject.transform.rotation;
                        break;
                    case 5:
                        selfPosition = self.stickContainer.RightUpperLeg.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightUpperLeg.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightUpperLeg.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightUpperLeg.gameObject.transform.rotation;
                        break;
                    case 6:
                        selfPosition = self.stickContainer.TorsoLeft.gameObject.transform.position;
                        selfRotation = self.stickContainer.TorsoLeft.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.TorsoLeft.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.TorsoLeft.gameObject.transform.rotation;
                        break;
                    case 7:
                        selfPosition = self.stickContainer.TorsoRight.gameObject.transform.position;
                        selfRotation = self.stickContainer.TorsoRight.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.TorsoRight.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.TorsoRight.gameObject.transform.rotation;
                        break;
                    case 8:
                        selfPosition = self.stickContainer.HipStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.HipStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.HipStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.HipStick.gameObject.transform.rotation;
                        break;
                    case 9:
                        selfPosition = self.stickContainer.LeftLowerArm.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftLowerArm.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftLowerArm.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftLowerArm.gameObject.transform.rotation;
                        break;
                    case 10:
                        selfPosition = self.stickContainer.RightLowerArm.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightLowerArm.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightLowerArm.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightLowerArm.gameObject.transform.rotation;
                        break;
                    case 11:
                        selfPosition = self.stickContainer.LeftEye.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftEye.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftEye.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftEye.gameObject.transform.rotation;
                        break;
                    case 12:
                        selfPosition = self.stickContainer.RightEye.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightEye.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightEye.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightEye.gameObject.transform.rotation;
                        break;
                    case 13:
                        selfPosition = self.stickContainer.Shoulders.gameObject.transform.position;
                        selfRotation = self.stickContainer.Shoulders.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.Shoulders.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.Shoulders.gameObject.transform.rotation;
                        break;
                    case 14:
                        selfPosition = self.stickContainer.MouthStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.MouthStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.MouthStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.MouthStick.gameObject.transform.rotation;
                        break;
                    case 15:
                        selfPosition = self.stickContainer.NoseStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.NoseStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.NoseStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.NoseStick.gameObject.transform.rotation;
                        break;
                    case 16:
                        selfPosition = self.stickContainer.LeftEar.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftEar.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftEar.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftEar.gameObject.transform.rotation;
                        break;
                    case 17:
                        selfPosition = self.stickContainer.RightEar.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightEar.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightEar.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightEar.gameObject.transform.rotation;
                        break;
                    case 18:
                        selfPosition = self.stickContainer.LeftShoulderStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftShoulderStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftShoulderStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftShoulderStick.gameObject.transform.rotation;
                        break;
                    case 19:
                        selfPosition = self.stickContainer.RightShoulderStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightShoulderStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightShoulderStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightShoulderStick.gameObject.transform.rotation;
                        break;
                    case 20:
                        selfPosition = self.stickContainer.LeftHipStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftHipStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftHipStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftHipStick.gameObject.transform.rotation;
                        break;
                    case 21:
                        selfPosition = self.stickContainer.RightHipStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightHipStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightHipStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightHipStick.gameObject.transform.rotation;
                        break;
                    case 22:
                        selfPosition = self.stickContainer.LeftElbowStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftElbowStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftElbowStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftElbowStick.gameObject.transform.rotation;
                        break;
                    case 23:
                        selfPosition = self.stickContainer.RightElbowStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightElbowStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightElbowStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightElbowStick.gameObject.transform.rotation;
                        break;
                    case 24:
                        selfPosition = self.stickContainer.LeftWristStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftWristStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftWristStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftWristStick.gameObject.transform.rotation;
                        break;
                    case 25:
                        selfPosition = self.stickContainer.RightWristStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightWristStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightWristStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightWristStick.gameObject.transform.rotation;
                        break;
                    case 26:
                        selfPosition = self.stickContainer.LeftKneeStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftKneeStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftKneeStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftKneeStick.gameObject.transform.rotation;
                        break;
                    case 27:
                        selfPosition = self.stickContainer.RightKneeStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightKneeStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightKneeStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightKneeStick.gameObject.transform.rotation;
                        break;
                    case 28:
                        selfPosition = self.stickContainer.LeftAnkleStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.LeftAnkleStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.LeftAnkleStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.LeftAnkleStick.gameObject.transform.rotation;
                        break;
                    case 29:
                        selfPosition = self.stickContainer.RightAnkleStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightAnkleStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightAnkleStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightAnkleStick.gameObject.transform.rotation;
                        break;
                    default:
                        selfPosition = self.stickContainer.RightAnkleStick.gameObject.transform.position;
                        selfRotation = self.stickContainer.RightAnkleStick.gameObject.transform.rotation;
                        teacherPosition = teacher.stickContainer.RightAnkleStick.gameObject.transform.position;
                        teacherRotation = teacher.stickContainer.RightAnkleStick.gameObject.transform.rotation;
                        break;
                }



                // get cosine similarity from quaternion 
                // background: https://www.researchgate.net/publication/316447858_Similarity_analysis_of_motion_based_on_motion_capture_technology
                // background: https://gdalgorithms-list.narkive.com/9TaVDT9G/quaternion-similarity-measure
                double cos_angle = selfRotation.w * teacherRotation.w + selfRotation.x * teacherRotation.x + selfRotation.y * teacherRotation.y + selfRotation.z * teacherRotation.z;
				cos_angle = Math.Abs(cos_angle);

                // similarity after filter
                similarityStick[i] = cos_angle;
                if (activateKalman)
                {
                    similarityStick[i] = kalmanFilter[i].Update(similarityStick[i]);
                    if(similarityStick[i] > 1.0)
                    {
                        similarityStick[i] = 1.0;
                    }
                    if (similarityStick[i] < 0.0)
                    {
                        similarityStick[i] = 0.0;
                    }
                }
                similarityTotal += similarityStick[i];

                // penalty
                stickWeightPenalty = 1.0/(Math.Pow(similarityStick[i], penalty));

                // weight
                stickWeight[i] = stickWeightBody[i] * stickWeightAdapt[i] * stickWeightPenalty;
                stickWeightTotal += stickWeight[i];

            }
            // normalization
            similarityBodypart = similarityTotal / stickWeightTotal;
            // timewise total score
            totalScore = totalScore + similarityBodypart;

        }

        public void ResetTotalScore()
        {
            totalScore = 0;
        }
	}
}