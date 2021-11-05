using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PoseTeacher
{
    public enum BodyWeightsType
    {
        TOTAL, TOP, MIDDLE, BOTTOM
    }

    public static class SimilarityConst
    {
        // all names of sticks existing in avatar
        public static List<string> StickNames
        {
            get
            {
                return new List<string>(new string[] {
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
            }
        }

        public const int StickNumber = 30;

        public static List<double> GetStickWeights(BodyWeightsType weightType)
        {
            switch (weightType)
            {
                case BodyWeightsType.TOTAL:
                    return Enumerable.Repeat(1.0, StickNumber).ToList();

                case BodyWeightsType.TOP:
                    return new List<double>(new double[] {
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
                                1.0, //LeftEye
                                1.0, //RightEye
                                1.0, //Shoulders
                                1.0, //MouthStick
                                1.0, //NoseStick
                                1.0, //LeftEar
                                1.0, //RightEar
                                1.0, //LeftShoulderStick
                                1.0, //RightShoulderStick
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

                case BodyWeightsType.MIDDLE:
                    return new List<double>(new double[] {
                        0.0,
                        0.0,
                        1.0, //LeftUpperArm
                        1.0, //RightUpperArm
                        0.0,
                        0.0,
                        1.0, //TorsoLeft
                        1.0, //TorsoRight
                        1.0, //HipStick
                        1.0, //LeftLowerArm
                        1.0, //RightLowerArm
                        0.0,
                        0.0,
                        1.0, //Shoulders
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                        1.0, //LeftShoulderStick
                        1.0, //RightShoulderStick
                        1.0, //LeftHipStick
                        1.0, //RightHipStick
                        1.0, //LeftElbowStick
                        1.0, //RightElbowStick
                        1.0, //LeftWristStick
                        1.0, //RightWristStick
                        0.0,
                        0.0,
                        0.0,
                        0.0,
                    });

                case BodyWeightsType.BOTTOM:
                    return new List<double>(new double[] {
                        1.0, //LeftLowerLeg
                        1.0, //RightLowerLeg
                        0.0,
                        0.0,
                        1.0, //LeftUpperLeg
                        1.0,//RightUpperLeg
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
                        1.0, //LeftKneeStick
                        1.0, //RightKneeStick
                        1.0, //LeftAnkleStick
                        1.0, //RightAnkleStick
                    });

                default:
                    return Enumerable.Repeat(0.0, StickNumber).ToList();
            }
        }
    }

    class AvatarSimilarity
    {
        // input
        public AvatarContainer self; // object containing self avatar
        public AvatarContainer teacher; // object containing teacher avatar
        public BodyWeightsType bodyNr; // body part we want to show similarity for
        //public List<string> stickNames; // all names of sticks existing in avatar
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
        public AvatarSimilarity(AvatarContainer selfIn, AvatarContainer teacherIn, BodyWeightsType bodyNrIn, double penaltyIn, bool activateKalmanIn, double kalmanQIn, double kalmanRIn)
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

            // define all stick names (should be actually moved to a parameter file)
            //stickNames = SimilarityConst.StickNames;
            stickNumber = SimilarityConst.StickNumber;

            // Initialize adaptive weighting
            stickWeight = Enumerable.Repeat(1.0, stickNumber).ToList();

            // weight of each stick
            stickWeightBody = SimilarityConst.GetStickWeights(bodyNr);


            // correct each default stick weight
            stickWeightAdapt = Enumerable.Repeat(1.0, stickNumber).ToList();

            // calculate final stick weight
            stickWeightTotal = 0.0;
            for (int i = 0; i < stickNumber; i++)
            {
                stickWeightTotal += stickWeightBody[i] * stickWeightAdapt[i];
            }

            // set default similarity each stick
            similarityStick = Enumerable.Repeat(0.0, stickNumber).ToList();

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
                // get orientation of relevant game objects
                Quaternion selfRotation = self.stickContainer.StickList[i].gameObject.transform.rotation;
                Quaternion teacherRotation = teacher.stickContainer.StickList[i].gameObject.transform.rotation;

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
                    if (similarityStick[i] > 1.0)
                    {
                        similarityStick[i] = 1.0;
                    }
                    if (similarityStick[i] < 0.0)
                    {
                        similarityStick[i] = 0.0;
                    }
                }

                // Add similarity depending on weight
                similarityTotal += similarityStick[i] * stickWeightBody[i];

                // penalty
                stickWeightPenalty = 1.0 / (Math.Pow(similarityStick[i], penalty));

                // weight
                stickWeight[i] = stickWeightBody[i] * stickWeightAdapt[i] * stickWeightPenalty;
                stickWeightTotal += stickWeight[i];

            }
            // normalization
            similarityBodypart = similarityTotal / stickWeightTotal;
            // timewise total score
            totalScore += similarityBodypart;
        }

        public void ResetTotalScore()
        {
            totalScore = 0;
        }
    }
}