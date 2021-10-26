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

    public class ScoringScript
    {
        //init
        List<double> StickWeights = new List<double>(new double[] {
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
        });
        AvatarContainer self;
        List<Scores> scores;

        // filter
        public double penalty = 0.0;
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


        public ScoringScript(AvatarContainer selfIn, bool activateKalmanIn = true)
        {

            self = selfIn;
            activateKalman = activateKalmanIn;

            // generate kalman filters
            if (activateKalman)
            {
                kalmanFilter = new List<KalmanFilter>(new KalmanFilter[StickWeights.Count]);
                for (int i = 0; i < StickWeights.Count; i++)
                {
                    kalmanFilter[i] = new KalmanFilter(kalmanQ, kalmanR);
                    kalmanFilter[i].Reset(1.0);
                }
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

        public void Update()
        {
            if (currentlyScoring)
            {
                double similarityTotal = 0.0;
                for ( int i = 0; i < StickWeights.Count; i++)
                {
                    if (StickWeights[i] == 0) continue;

                    Quaternion selfRotation = self.stickContainer.StickList[i].gameObject.transform.rotation;
                    Quaternion goalRotation;
                    if (currentGoalType == GoalType.POSE)
                    {
                        goalRotation = currentGoal[0].data[i].Orientation;
                    } else
                    {
                        goalRotation = currentGoal[i].data[i].Orientation;
                    }

                    // get cosine similarity from quaternion 
                    // background: https://www.researchgate.net/publication/316447858_Similarity_analysis_of_motion_based_on_motion_capture_technology
                    // background: https://gdalgorithms-list.narkive.com/9TaVDT9G/quaternion-similarity-measure
                    double similarity = Mathf.Abs(selfRotation.w * goalRotation.w + selfRotation.x * goalRotation.x + selfRotation.y * goalRotation.y + selfRotation.z * goalRotation.z);
                    
                    if (activateKalman)
                    {
                        similarity = kalmanFilter[i].Update(similarity);
                        if (similarity > 1.0) similarity = 1.0;
                        if (similarity < 1.0) similarity = 0.0;
                    }

                    similarityTotal += similarity * StickWeights[i];
                }
                currentScores.Add(similarityTotal / StickWeights.Sum());

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
                    //TODO: Maybe add an event that fires when a new score is reached
                    currentlyScoring = false;
                }
            }
        }
    }
}