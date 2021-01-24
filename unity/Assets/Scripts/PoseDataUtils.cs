using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher
{
    #region Joint and Pose classes
    // Class with position and orientation of one joint
    public class JointData
    {
        public Vector3 Position;
        public Quaternion Orientation;
    }

    // Class with position and orientation of all joints (one pose)
    // Used for applying poses to avatars
    [Serializable]
    public class PoseData
    {
        public JointData[] data;
    }


    // Class with position and orientation of one joint in string format
    // Used for conversion to JSON
    [Serializable]
    public class JointDataJSON
    {
        public string position;
        public string rotation;
    }

    // Class with position and orientation of all joints (one pose) in string format
    // Used for conversion to JSON
    [Serializable]
    public class PoseDataJSON
    {
        public JointDataJSON[] data;
    }


    // Class reperesenting a pose, only used for getting pose from websocket 
    // in format of lightweight-human-pose-estimation-3d-demo.pytorch (mentioned in README)
    [Serializable]
    public class RemoteJointList
    {
        public List<List<double>> values { get; set; }
    }
    #endregion

    #region Utils static functions
    public static class PoseDataUtils
    {
        // Converts Azure Kinect SDK BT Body to PoseDataJSON (serializable for JSON) pose
        public static PoseDataJSON Body2PoseDataJSON(Body body)
        {
            JointDataJSON[] joint_data_array = new JointDataJSON[(int)JointId.Count];
            for (JointId jt = 0; jt < JointId.Count; jt++)
            {
                // write recorded poses to file
                Microsoft.Azure.Kinect.BodyTracking.Joint joint = body.Skeleton.GetJoint(jt);
                var pos = joint.Position;
                var orientation = joint.Quaternion;
                // save raw data
                var v = new Vector3(pos.X, pos.Y, pos.Z);
                var r = new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);
                var joint_data = new JointDataJSON { position = JsonUtility.ToJson(v), rotation = JsonUtility.ToJson(r) };
                joint_data_array[(int)jt] = joint_data;
            }
            PoseDataJSON jdl = new PoseDataJSON { data = joint_data_array };
            return jdl;
        }

        // Converts JSON string to PoseData pose
        public static PoseData JSONstring2PoseData(string frame_json)
        {
            JointData[] joint_data_recorded_array = new JointData[(int)JointId.Count];
            PoseData recorded_data = new PoseData { data = { } };

            //Debug.Log(frame_json);
            PoseDataJSON saved_joint_data = JsonUtility.FromJson<PoseDataJSON>(frame_json);
            for (JointId jt = 0; jt < JointId.Count; jt++)
            {
                // play recording
                JointDataJSON jd = saved_joint_data.data[(int)jt];
                Vector3 v_saved = JsonUtility.FromJson<Vector3>(jd.position);
                Quaternion r_saved = JsonUtility.FromJson<Quaternion>(jd.rotation);
                var joint_data_live = new JointData
                {
                    Position = v_saved,
                    Orientation = r_saved
                };
                joint_data_recorded_array[(int)jt] = joint_data_live;
            }
            recorded_data = new PoseData { data = joint_data_recorded_array };
            return recorded_data;
        }

        // Converts Azure Kinect SDK BT Body to PoseData pose
        public static PoseData Body2PoseData(Body body)
        {
            JointData[] joint_data_live_array = new JointData[(int)JointId.Count];
            PoseData live_data = new PoseData { data = { } };

            for (JointId jt = 0; jt < JointId.Count; jt++)
            {
                Microsoft.Azure.Kinect.BodyTracking.Joint stickJoint = body.Skeleton.GetJoint(jt);
                var stickPos = stickJoint.Position;
                var stickOrientation = stickJoint.Quaternion;
                JointData joint_data_live = new JointData
                {
                    Position = new Vector3(stickPos.X, stickPos.Y, stickPos.Z),
                    Orientation = new Quaternion(stickOrientation.X, stickOrientation.Y, stickOrientation.Z, stickOrientation.W)
                };
                joint_data_live_array[(int)jt] = joint_data_live;
            }
            live_data = new PoseData { data = joint_data_live_array };
            return live_data;
        }

        // Converts RemoteJointList pose to PoseData pose
        public static PoseData Remote2PoseData(RemoteJointList rjl)
        {
            // format in  lightweight-human-pose-estimation-3d-demo.pytorch
            // Lightweight human pose estimation (https://github.com/Daniil-Osokin/lightweight-human-pose-estimation-3d-demo.pytorch) (Apache-2.0 License)
            /*
            0: Neck
            1: Nose
            2: BodyCenter(center of hips)
            3: lShoulder
            4: lElbow
            5: lWrist,
            6: lHip
            7: lKnee
            8: lAnkle
            9: rShoulder
            10: rElbow
            11: rWrist
            12: rHip
            13: rKnee
            14: rAnkle
            15: rEye
            16: lEye
            17: rEar
            18: lEar
            */

            // format in Kinect Body Tracking SDK
            // HipLeft 18 KneeLeft 19 AnkleLeft 20
            // HipRight 22 KneeRight 23 AnkleRight 24
            // ShoulderLeft 5
            // ShoulderRight 12
            // HipLeft 18 HipRight 22
            // ElbowLeft 6 WristLeft 7
            // ElbowRight 13 WristRight 14
            // LeftEye 28
            // RightEye 30
            // Neck 3
            // Nose 27
            // Left Ear 29
            // Right Ear 31 

            IDictionary<int, int> dict = new Dictionary<int, int>();
            // map kinect keys to Pytorch keys
            // in total 31 kinect and 18 Pytorch keys
            dict.Add(18, 6);
            dict.Add(19, 7);
            dict.Add(20, 8);
            dict.Add(22, 12);
            dict.Add(23, 13);
            dict.Add(24, 14);
            dict.Add(5, 3);
            dict.Add(12, 9);
            dict.Add(6, 4);
            dict.Add(7, 5);
            dict.Add(13, 10);
            dict.Add(14, 11);
            dict.Add(28, 16);
            dict.Add(30, 15);
            dict.Add(3, 0);
            dict.Add(27, 1);
            dict.Add(29, 18);
            dict.Add(31, 17);
            JointData[] joint_data_received_array = new JointData[(int)JointId.Count];

            Debug.Log(JsonUtility.ToJson(rjl));
            List<List<double>> joint_data = rjl.values;
            Debug.Log(joint_data);

            float scaling = 10.0f;
            for (JointId jt = 0; jt < JointId.Count; jt++)
            {
                if (dict.ContainsKey((int)jt))
                {
                    int pytorch_index = dict[(int)jt];
                    List<double> jtd = joint_data[pytorch_index];
                    Debug.Log(jtd);
                    Vector3 v_rcv = new Vector3(scaling * (float)jtd[0], scaling * (float)jtd[1], scaling * (float)jtd[2]);
                    Quaternion r_rcv = new Quaternion(0, 0, 0, 0);
                    var joint_data_live = new JointData
                    {
                        Position = v_rcv,
                        Orientation = r_rcv
                    };
                    joint_data_received_array[(int)jt] = joint_data_live;
                }
                else
                {   // just fill empty value as placeholder
                    Vector3 v_rcv = new Vector3(0, 0, 0);
                    Quaternion r_rcv = new Quaternion(0, 0, 0, 0);
                    var joint_data_live = new JointData
                    {
                        Position = v_rcv,
                        Orientation = r_rcv
                    };
                    joint_data_received_array[(int)jt] = joint_data_live;
                }

            }
            PoseData received_data = new PoseData { data = joint_data_received_array };
            return received_data;
        }

        // Unity apparently cannot deserialize arrays, so we had to do it manually
        // TODO: Function only used once in Start, but maybe should be outside
        public static RemoteJointList DeserializeRJL(string message)
        {
            string[] joints3D = message.Split(new string[] { "], [" }, StringSplitOptions.None);
            joints3D[0] = joints3D[0].Substring(3);
            joints3D[18] = joints3D[18].Split(']')[0];

            List<List<double>> joints = new List<List<double>>();
            for (int i = 0; i < 19; i++)
            {
                string[] joint = joints3D[i].Split(',');
                List<double> dbl = new List<double> { Convert.ToDouble(joint[0]), Convert.ToDouble(joint[1]), Convert.ToDouble(joint[2]) };
                joints.Add(dbl);
            }
            RemoteJointList rjl = new RemoteJointList { values = joints };
            return rjl;
        }
    }
    #endregion
}