using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace PoseTeacher
{
    public class ScoringUtils
    {
        


        public static int TotalWeights(List<int> weights)
        {
            int total = 0;
            foreach (int i in weights)
            {
                total += i;
            }
            return total;
        }

        public static float squaredAverage(List<float> values)
        {
            float squaredTotal = 0f;
            foreach (float f in values)
            {
                squaredTotal += Mathf.Pow(f, 2);
            }
            return Mathf.Sqrt(squaredTotal / values.Count);
        }

        
    }

    public class QuaternionUtils
    {
        public static List<int> quaternionWeightsPrioritizeArms = new List<int> { 3, 3, 1, 1, 1, 3, 3, 1 };


        public static float quaternionDistance(Quaternion a, Quaternion b)
        {
            return 1 - Mathf.Pow(a.w * b.w + a.x * b.x + a.y * b.y + a.z * b.z, 2);
        }

        public static List<Quaternion> PoseDataToOrientation(PoseData pose)
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

        public static List<Quaternion> DancePoseToOrientation(DancePose pose)
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
    }

    public class EuclideanUtils {
        public static List<Vector3> PoseDataToVector3(PoseData pose)
        {
            List<Vector3> list = new List<Vector3>();

            Vector3 pos = new Vector3();
            foreach (JointData joint in pose.data)
            {
                pos = joint.Position;
                list.Add(pos);
            }
            return list;
        }

        public static List<Vector3> DancePoseToVector3(DancePose pose)
        {
            List<Vector3> list = new List<Vector3>();

            foreach (Vector3 pos in pose.positions)
            {
                list.Add(pos);
            }
            return list;
        }


        public static double[,] CreateEuclideanWeightMatrix()
        {
            /*
            More infos about joint indexes can be found in PoseDataUtils.cs
            Or in  lightweight-human-pose-estimation-3d-demo.pytorch from the link bellow.
            Lightweight human pose estimation (https://github.com/Daniil-Osokin/lightweight-human-pose-estimation-3d-demo.pytorch) (Apache-2.0 License)
            */

            // after values from kinect body tracking 
            double[,] weightMatrix = new double[32, 32];

            // symetric matrix 
            // code used to generate weightMatrix
            for (int i = 0; i < 32; ++i)
            {
                for (int j = i; j < 32; ++j)
                {
                    // self weight
                    if (i == j) { weightMatrix[i, j] = 1.0; }
                    // ellbow or wrist
                    else if (new double[] { 6, 7, 13, 14 }.Contains(i)) { weightMatrix[i, j] = 1.0; weightMatrix[j, i] = 1.0; }
                    // knee and ankle
                    else if (new double[] { 19, 20, 23, 24 }.Contains(i)) { weightMatrix[i, j] = 1.0; weightMatrix[j, i] = 1.0; }
                    // 
                    else if (new double[] { 5, 12 }.Contains(i)) { weightMatrix[i, j] = 1.0; weightMatrix[j, i] = 1.0; }
                    //hips
                    else if (new double[] { 18, 22 }.Contains(i)) { weightMatrix[i, j] = 1.0; weightMatrix[j, i] = 1.0; }
                }
            }

            return weightMatrix;
        }

        // pre generated for faster updates
        public static double[,] euclideanWeightMatrix = {
            { 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 1, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 1, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 1, 0, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 1, 0, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 1, 0, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 1, 0 },
            { 0, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 0, 1, 1, 1, 0, 0, 0, 1, 1, 1, 0, 1, 1, 1, 0, 0, 0, 0, 0, 0, 1 } };

        // Euclidean Distance Differences 
        public static double[,] Vector3ToScoreMatrix(List<Vector3> teacherPositions, List<Vector3> playerPositions)
        {
            // scaling such that the score isnt to high
            double scaling = 1;

            // center the pose joints to the hip middle (this will be 0, 0, 0)
            // body centre is joint[2]
            Vector3 centreTeacher = teacherPositions[2];
            Vector3 centrePlayer = playerPositions[2];

            //Debug.Log("Number of teacher poses: " + teacherPoseData.data.Length);
            //Debug.Log("Number of player poses:  " + playerPoseData.data.Length);

            // iterate over all joints
            // and get their respective distance
            double[,] scoreMatrix = new double[teacherPositions.Count, playerPositions.Count];
            double[,] distanceTT = new double[teacherPositions.Count, teacherPositions.Count];
            double[,] distanceTP = new double[teacherPositions.Count, playerPositions.Count];

            // distance between teacher and player
            int i = 0, j = 0;
            foreach (Vector3 posT in teacherPositions)
            {
                j = 0;
                foreach (Vector3 posP in playerPositions)
                {
                    double distance = System.Math.Abs(Vector3.Distance(posT - centreTeacher, posP - centrePlayer)) * scaling;
                    distanceTP[i, j] = distance;
                    j++;
                }
                i++;
            }

            // distance between teacher and teacher
            // and difference between self distance and t-p distance
            i = 0; j = 0;
            foreach (Vector3 posT1 in teacherPositions)
            {
                j = 0;
                foreach (Vector3 posT2 in teacherPositions)
                {
                    // centering not needed here, relative distance is the same.
                    double distance = System.Math.Abs(Vector3.Distance(posT1, posT2)) * scaling;
                    distanceTT[i, j] = distance;
                    scoreMatrix[i, j] = System.Math.Abs(distance - distanceTP[i, j]);

                    // weighting mechanism. use already initialized matrix, otherwise too expensive
                    scoreMatrix[i, j] = scoreMatrix[i, j] * euclideanWeightMatrix[i, j];

                    j++;
                }
                i++;
            }

            return scoreMatrix;
        }
    }
}

