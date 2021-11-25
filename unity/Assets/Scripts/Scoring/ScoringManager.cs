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
        List<PoseData> currentMotion;
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
                    PoseData currentSelfPose = DanceManager.Instance.currentSelfPose;
                    DancePose goalPose;

                    if (currentGoalType == GoalType.POSE)
                    {
                        goalPose = currentGoal[0];
                    }
                    else
                    {
                        goalPose = currentGoal[goalCounter];
                        currentMotion.Add(currentSelfPose);
                    }

                    if (!alternateDistanceMetric)
                    {
                        currentScores.Add(quaternionDistanceScore(goalPose,currentSelfPose));
                    } else
                    {
                        currentScores.Add(euclideanDistanceScore(goalPose,currentSelfPose));
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
            currentMotion = new List<PoseData>();
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

        private float quaternionDistanceScore(DancePose goalPose, PoseData currentSelfPose)
        {
            float distanceTotal = 0.0f;
            List<Quaternion> selfList = QuaternionUtils.PoseDataToOrientation(currentSelfPose);
            List<Quaternion> goalList = QuaternionUtils.DancePoseToOrientation(goalPose);

            for (int i = 0; i < numberOfComparisons; i++)
            {
                float distance = QuaternionUtils.quaternionDistance(selfList[i], goalList[i]);
                distanceTotal += Mathf.Pow(distance, 2) * QuaternionUtils.quaternionWeightsPrioritizeArms[i];
            }
            return Mathf.Sqrt(distanceTotal / ScoringUtils.TotalWeights(QuaternionUtils.quaternionWeightsPrioritizeArms));
        }

        private float euclideanDistanceScore(DancePose goalPose, PoseData currentSelfPose)
        {
            List<Vector3> selfList = EuclideanUtils.PoseDataToVector3(currentSelfPose);
            List<Vector3> goalList = EuclideanUtils.DancePoseToVector3(goalPose);

            double[,] scores = EuclideanUtils.Vector3ToScoreMatrix(goalList, selfList);
            float scaling = 0.01f;
            float average = 0f;
            foreach (double value in scores)
            {
                average += (float)value * scaling;
            }

            return average / scores.Length;
        }

        private float DTWDistance(List<DancePose> goals, List<PoseData> playerPoses, int windowLeft, int windowRight)
        {
            float[,] DTW = new float[goals.Count + 1, playerPoses.Count + 1];
            for(int i = 0; i <= goals.Count; i++)
            {
                for (int j = 0; j <= playerPoses.Count; j++)
                {
                    DTW[i, j] = Mathf.Infinity;
                }
            }
            DTW[0, 0] = 0;
            for (int i = 1; i <= goals.Count; i++)
            {
                for (int j = Mathf.Max(1, i - windowLeft); j <= Mathf.Min(playerPoses.Count, i + windowRight); j++)
                {
                    float cost = quaternionDistanceScore(goals[i-1], playerPoses[j-1]);
                    DTW[i, j] = cost + Mathf.Min(DTW[i - 1, j], DTW[i, j - 1], DTW[i - 1, j - 1]);
                }
            }
            return DTW[goals.Count, playerPoses.Count] / playerPoses.Count;
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
            Debug.Log("DTW: " + DTWDistance(currentGoal, currentMotion, 3, 10));

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