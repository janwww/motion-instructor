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
        List<DancePose> currentGoal;
        float currentTimeStamp;
        int goalCounter;
        int goalLength;
        List<double> currentScores;

        //constants
        int numberOfComparisons = 8;
        readonly float constDeltaTime = 0.1f;

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

        public void StartNewGoal(GoalType type, List<DancePose> goal, float startTimeStamp)
        {
            if (currentlyScoring) return;
            currentlyScoring = true;
            currentScores = new List<double>();
            currentGoalType = type;
            currentGoal = goal;
            goalCounter = 0;
            currentTimeStamp = startTimeStamp;
            if (currentGoalType == GoalType.POSE)
            {
                goalLength = 15;
            } else
            {
                goalLength = currentGoal.Count;
            }
        }

        public void Update(PoseData currentSelfPose, float danceTimeStamp)
        {
            if (currentlyScoring)
            {

                bool nextStep = false;

                switch (currentGoalType)
                {
                    case GoalType.POSE:
                        float deltaTime = danceTimeStamp - currentTimeStamp;
                        nextStep = deltaTime >= constDeltaTime;
                        break;
                    case GoalType.MOTION:
                        nextStep = danceTimeStamp >= currentGoal[goalCounter].timestamp;
                        break;
                }


                if (nextStep)
                {
                    double similarityTotal = 0.0;
                    List<Quaternion> selfList = PoseDataToOrientation(currentSelfPose);
                    List<Quaternion> goalList;
                    if (currentGoalType == GoalType.POSE)
                    {
                        goalList = DancePoseToOrientation(currentGoal[0]);
                    }
                    else
                    {
                        goalList = DancePoseToOrientation(currentGoal[goalCounter]);
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

                        similarityTotal += similarity*scoringWeightsPrioritizeArms[i];
                    }
                    Debug.Log(similarityTotal / TotalWeights(scoringWeightsPrioritizeArms));
                    currentScores.Add(similarityTotal / TotalWeights(scoringWeightsPrioritizeArms));

                    currentTimeStamp = danceTimeStamp;
                    goalCounter += 1;
                    if (goalCounter == goalLength)
                    {
                        double tempScore;
                        if (currentGoalType == GoalType.POSE)
                        {
                            //for a pose, take best score in evaluation period
                            tempScore = currentScores.Max();

                        }
                        else
                        {
                            //for a move, take best 75% of scores and average them 
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

        List<Quaternion> DancePoseToOrientation(DancePose pose)
        {
            List<Quaternion> list = new List<Quaternion>();
            Vector3 vector;

            //LeftUpperArm (SHOULDER_LEFT - ELBOW_LEFT)
            vector = pose.positions[5] - pose.positions[6];
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //RightUpperArm (SHOULDER_RIGHT - ELBOW_RIGHT)
            vector = pose.positions[12] - pose.positions[13];
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //TorsoLeft (SHOULDER_LEFT - HIP_LEFT
            vector = pose.positions[5] - pose.positions[18];
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //TorsoRight (SHOULDER_RIGHT - HIP_RIGHT
            vector = pose.positions[12] - pose.positions[22];
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //HipStick (HIP_LEFT - HIP_RIGHT)
            vector = pose.positions[18] - pose.positions[22];
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //LeftLowerArm (ELBOW_LEFT - WRIST_LEFT)
            vector = pose.positions[6] - pose.positions[7];
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //RightLowerArm (ELBOW_RIGHT - WRIST_RIGHT)
            vector = pose.positions[13] - pose.positions[14];
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            //Shoulders (SHOULDER_LEFT - SHOULDER_RIGHT)
            vector = pose.positions[5] - pose.positions[12];
            list.Add(Quaternion.LookRotation(vector, Vector3.up));

            return list;
        }

        List<int> scoringWeightsPrioritizeArms = new List<int>{3, 3, 1, 1, 1, 3, 3, 1};

        int TotalWeights(List<int> weights)
        {
            int total = 0;
            foreach (int i in weights)
            {
                total += i;
            }
            return total;
        }
    }
}