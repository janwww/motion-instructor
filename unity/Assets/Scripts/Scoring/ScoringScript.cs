using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Microsoft.Azure.Kinect.BodyTracking;



namespace PoseTeacher
{
    public enum Scores
    {
        BAD, GOOD, GREAT
    }

    public enum GoalType
    {
        MOTION, POSE
    }

    class ScoringScript
    {
        List<Scores> scores;

        // filter
        public double kalmanQ = 0.0001;
        public double kalmanR = 1.0;
        bool activateKalman;
        public List<KalmanFilter> kalmanFilter;

        //Goals
        bool currentlyScoring = false;
        GoalType currentGoalType;
        List<PoseData> currentGoal;
        int goalCounter;
        int goalLength;
        List<double> currentScores;

        //constants
        int numberOfComparisons = 8;

        GameObject scoreDisplay;


        public ScoringScript(GameObject _scoreDisplay = null, bool activateKalmanIn = true)
        {
            scoreDisplay = _scoreDisplay;

            activateKalman = activateKalmanIn;

            // generate kalman filters
            kalmanFilter = new List<KalmanFilter>();
            for (int i = 0; i < numberOfComparisons; i++)
            {
                kalmanFilter.Add(new KalmanFilter(kalmanQ, kalmanR));
                kalmanFilter[i].Reset(1.0);
            }


            scores = new List<Scores>();
        }

        public void StartNewGoal(GoalType type, List<PoseData> goal)
        {
            if (currentlyScoring) return;
            currentlyScoring = true;
            currentScores = new List<double>();
            currentGoalType = type;
            currentGoal = goal;
            goalCounter = 0;
            if (currentGoalType == GoalType.POSE)
            {
                goalLength = 15;
            } else
            {
                goalLength = currentGoal.Count;
            }
        }

        public void Update(PoseData currentSelfPose)
        {
            if (currentlyScoring)
            {
                double similarityTotal = 0.0;
                List<Quaternion> selfList = PoseDataToOrientation(currentSelfPose);
                List<Quaternion> goalList;
                if (currentGoalType == GoalType.POSE)
                {
                    goalList = PoseDataToOrientation(currentGoal[0]);
                }
                else
                {
                    goalList = PoseDataToOrientation(currentGoal[goalCounter]);
                }


                for (int i = 0; i < numberOfComparisons; i++)
                {
                    double similarity = cosineSimilarity(selfList[i], goalList[i]);
                    if (activateKalman)
                    {
                        similarity = kalmanFilter[i].Update(similarity);
                        if (similarity > 1.0) similarity = 1.0;
                        if (similarity < 0.0) similarity = 0.0;
                    }

                    similarityTotal += similarity;
                }
                currentScores.Add(similarityTotal / numberOfComparisons);
                goalCounter += 1;
                if (goalCounter == goalLength) {
                    double tempScore;
                    if (currentGoalType == GoalType.POSE)
                    {
                        tempScore = currentScores.Max();

                    } else
                    {
                        tempScore = currentScores.OrderByDescending(list => list).Take(Mathf.RoundToInt(currentScores.Count * 0.75f)).Average();
                    }

                    if (tempScore > 0.8)
                    {
                        scores.Add(Scores.GREAT);
                    }
                    else if (tempScore > 0.4)
                    {
                        scores.Add(Scores.GOOD);
                    }
                    else
                    {
                        scores.Add(Scores.BAD);
                    }

                    Debug.Log(scores[scores.Count - 1]);
                    //TODO: Maybe add an event that fires when a new score is reached
                    currentlyScoring = false;

                    if (scoreDisplay != null)
                    {
                        scoreDisplay.SendMessage("addScore", scores[scores.Count - 1]);
                    }
                }
            }
        }

        double cosineSimilarity(Quaternion a, Quaternion b)
        {
            // get cosine similarity from quaternion 
            // background: https://www.researchgate.net/publication/316447858_Similarity_analysis_of_motion_based_on_motion_capture_technology
            // background: https://gdalgorithms-list.narkive.com/9TaVDT9G/quaternion-similarity-measure
            return Mathf.Abs(a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z);
        }

        List<Quaternion> PoseDataToOrientation(PoseData pose)
        {
            List<Quaternion> list = new List<Quaternion>();
            Vector3 vector;

            //LeftUpperArm (SHOULDER_LEFT - ELBOW_LEFT)
            vector = pose.data[5].Position - pose.data[6].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //RightUpperArm (SHOULDER_RIGHT - ELBOW_RIGHT)
            vector = pose.data[12].Position - pose.data[13].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //TorsoLeft (SHOULDER_LEFT - HIP_LEFT
            vector = pose.data[5].Position - pose.data[18].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //TorsoRight (SHOULDER_RIGHT - HIP_RIGHT
            vector = pose.data[12].Position - pose.data[22].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //HipStick (HIP_LEFT - HIP_RIGHT)
            vector = pose.data[18].Position - pose.data[22].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //LeftLowerArm (ELBOW_LEFT - WRIST_LEFT)
            vector = pose.data[6].Position - pose.data[7].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //RightLowerArm (ELBOW_RIGHT - WRIST_RIGHT)
            vector = pose.data[13].Position - pose.data[17].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //Shoulders (SHOULDER_LEFT - SHOULDER_RIGHT)
            vector = pose.data[5].Position - pose.data[12].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            return list;
        }


    }


    public class ScoreEDD
    {
        List<PoseData> teacherPoses;
        List<double[,]> scoreList;
        public double eddScore;
        public int totalFrames;
        int previousFrames;

        public ScoreEDD(int pF = 5)
        {
            teacherPoses = new List<PoseData>();
            scoreList = new List<double[,]>();
            eddScore = 0;
            totalFrames = 0;
            previousFrames = pF;
        }


        // Euclidean Distance Differences 
        public double[,] ScoreMatrix(PoseData teacherPoseData, PoseData playerPoseData)
        {
            // scaling such that the score isnt to high
            double scaling = 1;

            // center the pose joints to the hip middle (this will be 0, 0, 0)
            // body centre is joint[2]
            Vector3 centreTeacher = teacherPoseData.data[2].Position;
            Vector3 centrePlayer = playerPoseData.data[2].Position;

            //Debug.Log("Number of teacher poses: " + teacherPoseData.data.Length);
            //Debug.Log("Number of player poses:  " + playerPoseData.data.Length);

            // iterate over all joints
            // and get their respective distance
            double[,] scoreMatrix = new double[teacherPoseData.data.Length, playerPoseData.data.Length];
            double[,] distanceTT = new double[teacherPoseData.data.Length, teacherPoseData.data.Length];
            double[,] distanceTP = new double[teacherPoseData.data.Length, playerPoseData.data.Length];

            // distance between teacher and player
            int i = 0, j = 0;
            foreach (JointData teacherJoint in teacherPoseData.data)
            {
                j = 0;
                foreach(JointData playerJoint in playerPoseData.data)
                {
                    double distance = Math.Abs(Vector3.Distance(teacherJoint.Position - centreTeacher, playerJoint.Position - centrePlayer)) * scaling;
                    distanceTP[i, j] = distance;
                    j++;
                }
                i++;
            }

            // distance between teacher and teacher
            // and difference between self distance and t-p distance
            i = 0; j = 0;
            foreach (JointData teacherJoint1 in teacherPoseData.data)
            {
                j = 0;
                foreach (JointData teacherJoint2 in teacherPoseData.data)
                {
                    // centering not needed here, relative distance is the same.
                    double distance = Math.Abs(Vector3.Distance(teacherJoint1.Position, teacherJoint2.Position)) * scaling;
                    distanceTT[i, j] = distance;
                    scoreMatrix[i, j] = Math.Abs(distance - distanceTP[i, j]);

                    j++;
                }
                i++;
            }

            // NOTE: introduce some weighting mechanism if wanted (or make it a own function)

            return scoreMatrix;
        }


        // Calculate the average Euclidean Distance Differences (EDD) of the player for the last numberPreviousFrames
        // 0 best score. if wrong higher, on average wrong movement around 100. on average good movement around 10?
        public double EDDOverTime(int numberPreviousFrames, PoseData player)
        {
            // scaling such that the score isnt to high
            double scaling = 0.001;
            double average = 0;

            int listLength = teacherPoses.Count;
            //Debug.Log("Number of teacher frames: " + listLength);
            if(listLength <= numberPreviousFrames)
            {
                foreach(PoseData teacher in teacherPoses)
                {
                    double[,] score = ScoreMatrix(teacher, player);
                    foreach(double value in score)
                    {
                        //Debug.Log("value in score matrix: " + value);
                        average += value*scaling;
                    }
                //Debug.Log("average after row: " + average);
                }
                average /= listLength;
            }

            else
            {
                for(int i = numberPreviousFrames; i > 0; --i)
                {
                    double[,] score = ScoreMatrix(teacherPoses[listLength - i], player);
                    foreach (double value in score)
                    {
                        //Debug.Log("value in score matrix: " + value);
                        average += value*scaling;
                    }
                    //Debug.Log("average after row: " + average);
                }
                average /= numberPreviousFrames;
            }

            return average;
        }

        public void Update(PoseData teacher, PoseData player)
        {
            teacherPoses.Add(teacher);
            eddScore += EDDOverTime(previousFrames, player);
            totalFrames++;
        }


    }

    

}

