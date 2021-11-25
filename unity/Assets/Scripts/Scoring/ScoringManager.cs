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

    class ScoringManager : MonoBehaviour
    {
        public static ScoringManager Instance;

        List<Scores> scores;

        //Goals
        bool currentlyScoring = false;
        GoalType currentGoalType;
        List<DancePose> currentGoal;
        float currentTimeStamp;
        float goalStartTimeStamp;
        int goalCounter;
        int goalLength;
        List<float> currentScores;

        //constants
        int numberOfComparisons = 8;
        readonly float constDeltaTime = 0.1f;
        public bool alternateDistanceMetric = false;

        public GameObject scoreDisplay;


        public void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            } else
            {
                Instance = this;
            }
            scores = new List<Scores>();
        }

        public void Update()
        {
            if (currentlyScoring && DanceManager.Instance.currentSelfPose!=null)
            {
                PoseData currentSelfPose = DanceManager.Instance.currentSelfPose;
                float danceTimeStamp = DanceManager.Instance.songTime;

                bool nextStep = false;

                switch (currentGoalType)
                {
                    case GoalType.POSE:
                        float deltaTime = danceTimeStamp - currentTimeStamp;
                        nextStep = deltaTime >= constDeltaTime;
                        break;
                    case GoalType.MOTION:
                        nextStep = danceTimeStamp >= currentGoal[goalCounter].timestamp + goalStartTimeStamp;
                        break;
                }


                if (nextStep)
                {
                    if (!alternateDistanceMetric)
                    {
                        currentScores.Add(quaternionDistanceScore(currentSelfPose));
                    } else
                    {
                        currentScores.Add(euclideanDistanceScore(currentSelfPose));
                    }

                    currentTimeStamp = danceTimeStamp;
                    goalCounter += 1;
                    if (goalCounter == goalLength)
                    {
                        finishGoal();
                    }
                }

            }
        }


        public void StartNewGoal( List<DancePose> goal, float startTimeStamp)
        {
            GoalType type = goal.Count == 1 ? GoalType.POSE : GoalType.MOTION;
            if (currentlyScoring) finishGoal();
            currentlyScoring = true;
            currentScores = new List<float>();
            currentGoalType = type;
            currentGoal = goal;
            goalCounter = 0;
            currentTimeStamp = startTimeStamp;
            goalStartTimeStamp = startTimeStamp;
            if (currentGoalType == GoalType.POSE)
            {
                goalLength = 15;
            }
            else
            {
                goalLength = currentGoal.Count;
            }
        }

        private float quaternionDistanceScore(PoseData currentSelfPose)
        {
            float distanceTotal = 0.0f;
            List<Quaternion> selfList = QuaternionUtils.PoseDataToOrientation(currentSelfPose);
            List<Quaternion> goalList;

            if (currentGoalType == GoalType.POSE)
            {
                goalList = QuaternionUtils.DancePoseToOrientation(currentGoal[0]);
            }
            else
            {
                goalList = QuaternionUtils.DancePoseToOrientation(currentGoal[goalCounter]);
            }

            for (int i = 0; i < numberOfComparisons; i++)
            {
                float distance = QuaternionUtils.quaternionDistance(selfList[i], goalList[i]);
                distanceTotal += Mathf.Pow(distance, 2) * QuaternionUtils.quaternionWeightsPrioritizeArms[i];
            }
            return Mathf.Sqrt(distanceTotal / ScoringUtils.TotalWeights(QuaternionUtils.quaternionWeightsPrioritizeArms));
        }

        private float euclideanDistanceScore(PoseData currentSelfPose)
        {
            List<Vector3> selfList = EuclideanUtils.PoseDataToVector3(currentSelfPose);
            List<Vector3> goalList;

            if (currentGoalType == GoalType.POSE)
            {
                goalList = EuclideanUtils.DancePoseToVector3(currentGoal[0]);
            }
            else // currentGoalType == goalType.MOTION
            {
                goalList = EuclideanUtils.DancePoseToVector3(currentGoal[goalCounter]);
            }

            double[,] scores = EuclideanUtils.Vector3ToScoreMatrix(goalList, selfList);
            float scaling = 0.01f;
            float average = 0f;
            foreach (double value in scores)
            {
                average += (float)value * scaling;
            }

            return average / scores.Length;
        }


        public List<Scores> getFinalScores()
        {
            if (currentlyScoring) finishGoal();
            return scores;
        }

        void finishGoal()
        {
            double tempScore;
            if (currentGoalType == GoalType.POSE)
            {
                //for a pose, take best score in evaluation period
                tempScore = currentScores.Max();

            }
            else
            {
                //for a move, take square average of scores
                tempScore = ScoringUtils.squaredAverage(currentScores);
            }
            Debug.Log(tempScore);

            if (!alternateDistanceMetric)
            {
                if (tempScore < 0.15)
                {
                    scores.Add(Scores.GREAT);
                }
                else if (tempScore < 0.4)
                {
                    scores.Add(Scores.GOOD);
                }
                else
                {
                    scores.Add(Scores.BAD);
                }
            }
            else
            {
                // NOTE: scores not yet tuned to optimal values
                if (tempScore < 0.2)
                {
                    scores.Add(Scores.GREAT);
                }
                else if (tempScore < 0.55)
                {
                    scores.Add(Scores.GOOD);
                }
                else
                {
                    scores.Add(Scores.BAD);
                }
            }

            if (scoreDisplay != null)
            {
                scoreDisplay.SendMessage("addScore", scores[scores.Count - 1]);
            }
            currentlyScoring = false;
        }
    }
}