using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;



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
                    goalList = PoseDataToOrientation( currentGoal[0]);
                }
                else
                {
                    goalList = PoseDataToOrientation (currentGoal[goalCounter]);
                }
                
                
                for ( int i = 0; i < numberOfComparisons; i++)
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
                currentScores.Add(similarityTotal/numberOfComparisons);
                goalCounter += 1;
                if (goalCounter == goalLength) {
                    double tempScore;
                    if (currentGoalType == GoalType.POSE)
                    {
                        tempScore = currentScores.Max();
                        
                    } else
                    {
                        tempScore = currentScores.OrderByDescending(list=>list).Take(Mathf.RoundToInt(currentScores.Count*0.75f)).Average();
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
            vector = pose.data[13].Position - pose.data[14].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //Shoulders (SHOULDER_LEFT - SHOULDER_RIGHT)
            vector = pose.data[5].Position - pose.data[12].Position;
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            return list;
        }

        
    }
}