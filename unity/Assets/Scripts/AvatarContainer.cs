using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;
using System.Text;

namespace PoseTeacher
{
    // Label for various containers
    public enum AvatarType
    {
        CUBE, STICK, ROBOT, SMPL
    }

    // Base interface for different type of containers (cube, stick etc.)
    public interface IAvatarSubContainer
    {
        // CONSIDER moving GameObject
        GameObject SubContainerObject { get; set; }

        // Activates/Deactivates the contained GameObject object
        void SetActive(bool active);
        // Move the contained GameObject object based on the input JointData
        void MovePerson(PoseData joint_data_list);

        Vector3 GetReferencePosition();
    }

    // Class that contains information about the cube contained in an AvatarContainer object
    public class CubeContainer : IAvatarSubContainer
    {
        public GameObject SubContainerObject { get; set; }

        // References to cubes of the cubes avatar
        public GameObject[] cubeObjects;


        public CubeContainer(GameObject container)
        {
            SubContainerObject = container;

            cubeObjects = new GameObject[(int)JointId.Count];

            for (var i = 0; i < (int)JointId.Count; i++)
            {
                // Find cube children, and insert refrences in same order as joints from the BT SDK
                // Note: cube objects in Scene have same name as the joints in the SDK
                Transform cubeTrI = SubContainerObject.transform.Find(Enum.GetName(typeof(JointId), i));
                if (cubeTrI == null)
                {
                    Debug.Log(Enum.GetName(typeof(JointId), i));
                    continue;
                }
                var cubeI = cubeTrI.gameObject;
                //TODO Add predefined scales for different cubes
                //cubeI.transform.localScale = Vector3.one * 0.4f;
                cubeI.transform.SetParent(SubContainerObject.transform);
                cubeObjects[i] = cubeI;
            }
        }

        public void MovePerson(PoseData joint_data_list)
        {
            //Place cubes at position and orietation of joints
            for (JointId jt = 0; jt < JointId.Count; jt++)
            {
                var joint = joint_data_list.data[(int)jt];
                var pos = joint.Position;
                var orientation = joint.Orientation;
                var v = new Vector3(pos[0], -pos[1], pos[2]) * 0.004f;
                var r = new Quaternion(orientation[0], orientation[1], orientation[2], orientation[3]);
                var obj = cubeObjects[(int)jt];
                obj.transform.localPosition = v;
                obj.transform.localRotation = r;
            }
        }

        public Vector3 GetReferencePosition()
        {
            return SubContainerObject.transform.position + new Vector3(0,-0.4f,0);
        }

        public void SetActive(bool active)
        {
            SubContainerObject.SetActive(active);
        }
    }

    // Class that contains information about the stick figure contained in an AvatarContainer object
    public class StickContainer : IAvatarSubContainer
    {
        //public GameObject stick;
        public GameObject SubContainerObject { get; set; }

        // References to parts of the stick person avatar
        public GameObject LeftLowerLeg, RightLowerLeg, LeftUpperArm, RightUpperArm, LeftUpperLeg, RightUpperLeg, TorsoLeft,
           TorsoRight, HipStick, LeftLowerArm, RightLowerArm, LeftEye, RightEye, Shoulders, MouthStick, NoseStick, LeftEar, RightEar;
        public GameObject LeftShoulderStick, RightShoulderStick, LeftHipStick, RightHipStick, LeftElbowStick, RightElbowStick, LeftWristStick, RightWristStick,
            LeftKneeStick, RightKneeStick, LeftAnkleStick, RightAnkleStick;

        // List of all references
        public List<GameObject> StickList;


        public StickContainer(GameObject container)
        {
            SubContainerObject = container;
            GameObject stick = container;

            // Find children of the stick person avatar in scene and save references in fields
            LeftLowerLeg = stick.transform.Find("LLLeg").gameObject;
            RightLowerLeg = stick.transform.Find("RLLeg").gameObject;
            LeftUpperArm = stick.transform.Find("LeftUpperArm").gameObject;
            RightUpperArm = stick.transform.Find("RightUpperArm").gameObject;
            LeftUpperLeg = stick.transform.Find("LeftUpperLeg").gameObject; // HipLeft 18 KneeLeft 19 AnkleLeft 20
            RightUpperLeg = stick.transform.Find("RightUpperLeg").gameObject; // HipRight 22 KneeRight 23 AnkleRight 24
            TorsoLeft = stick.transform.Find("TorsoLeft").gameObject; // ShoulderLeft 5 HipLeft 18
            TorsoRight = stick.transform.Find("TorsoRight").gameObject; // ShoulderRight 12 HipRight 22
            HipStick = stick.transform.Find("HipStick").gameObject; // HipLeft 18 HipRight 22
            LeftLowerArm = stick.transform.Find("LeftLowerArm").gameObject; // = ElbowLeft 6 WristLeft 7
            RightLowerArm = stick.transform.Find("RightLowerArm").gameObject; // = ElbowRight 13 WristRight 14
            LeftEye = stick.transform.Find("LeftEye").gameObject; // 28
            RightEye = stick.transform.Find("RightEye").gameObject; // 30
            Shoulders = stick.transform.Find("Shoulders").gameObject; // ShoulderLeft 5 ShoulderRight 12
            MouthStick = stick.transform.Find("MouthStick").gameObject; // = Neck 3
            NoseStick = stick.transform.Find("NoseStick").gameObject; // 27
            LeftEar = stick.transform.Find("LeftEar").gameObject; // 29
            RightEar = stick.transform.Find("RightEar").gameObject; // 31

            LeftShoulderStick = stick.transform.Find("LeftShoulderStick").gameObject;
            RightShoulderStick = stick.transform.Find("RightShoulderStick").gameObject;
            LeftHipStick = stick.transform.Find("LeftHipStick").gameObject;
            RightHipStick = stick.transform.Find("RightHipStick").gameObject;
            LeftElbowStick = stick.transform.Find("LeftElbowStick").gameObject;
            RightElbowStick = stick.transform.Find("RightElbowStick").gameObject;
            LeftWristStick = stick.transform.Find("LeftWristStick").gameObject;
            RightWristStick = stick.transform.Find("RightWristStick").gameObject;
            LeftKneeStick = stick.transform.Find("LeftKneeStick").gameObject;
            RightKneeStick = stick.transform.Find("RightKneeStick").gameObject;
            LeftAnkleStick = stick.transform.Find("LeftAnkleStick").gameObject;
            RightAnkleStick = stick.transform.Find("RightAnkleStick").gameObject;

            StickList = new List<GameObject>(new GameObject[]
                {   //list of body joints
                    LeftLowerLeg, RightLowerLeg, LeftUpperArm, RightUpperArm, LeftUpperLeg, RightUpperLeg, TorsoLeft,
                    TorsoRight, HipStick, LeftLowerArm, RightLowerArm, LeftEye, RightEye, Shoulders, MouthStick, NoseStick, LeftEar, RightEar,
                    LeftShoulderStick, RightShoulderStick, LeftHipStick, RightHipStick, LeftElbowStick, RightElbowStick, LeftWristStick, RightWristStick,
                    LeftKneeStick, RightKneeStick, LeftAnkleStick, RightAnkleStick
                });
        }

        public void MovePerson(PoseData joint_data_list)
        {
            /************************************Joints**************************************/
            JointData stickJoint = joint_data_list.data[5];
            var stickPos = stickJoint.Position;
            var stickOrientation = stickJoint.Orientation;
            var stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            var stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftShoulderStick.transform.localPosition = stickV;
            LeftShoulderStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[12];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightShoulderStick.transform.localPosition = stickV;
            RightShoulderStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[18];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftHipStick.transform.localPosition = stickV;
            LeftHipStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[22];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightHipStick.transform.localPosition = stickV;
            RightHipStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[6];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftElbowStick.transform.localPosition = stickV;
            LeftElbowStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[13];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightElbowStick.transform.localPosition = stickV;
            RightElbowStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[7];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftWristStick.transform.localPosition = stickV;
            LeftWristStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[14];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightWristStick.transform.localPosition = stickV;
            RightWristStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[19];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftKneeStick.transform.localPosition = stickV;
            LeftKneeStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[23];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightKneeStick.transform.localPosition = stickV;
            RightKneeStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[20];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftAnkleStick.transform.localPosition = stickV;
            LeftAnkleStick.transform.localRotation = stickR;

            stickJoint = joint_data_list.data[24];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightAnkleStick.transform.localPosition = stickV;
            RightAnkleStick.transform.localRotation = stickR;


            /************************************Head**************************************/
            stickJoint = joint_data_list.data[28];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftEye.transform.localPosition = stickV;
            LeftEye.transform.localRotation = stickR;
            LeftEye.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);

            stickJoint = joint_data_list.data[30];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightEye.transform.localPosition = stickV;
            RightEye.transform.localRotation = stickR;
            RightEye.transform.localScale = new Vector3(0.3f, 0.2f, 0.2f);

            stickJoint = joint_data_list.data[27];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            NoseStick.transform.localPosition = stickV;
            NoseStick.transform.localRotation = stickR;
            NoseStick.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            stickJoint = joint_data_list.data[29];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftEar.transform.localPosition = stickV;
            LeftEar.transform.localRotation = stickR;
            LeftEar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            LeftEar.transform.LookAt(LeftShoulderStick.transform.position);
            LeftEar.transform.Rotate(90, 0, 0);

            stickJoint = joint_data_list.data[31];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightEar.transform.localPosition = stickV;
            RightEar.transform.localRotation = stickR;
            RightEar.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            RightEar.transform.LookAt(RightShoulderStick.transform.position);
            RightEar.transform.Rotate(90, 0, 0);

            stickJoint = joint_data_list.data[27];
            stickPos = stickJoint.Position;
            stickOrientation = stickJoint.Orientation;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.008f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            MouthStick.transform.localPosition = stickV;
            MouthStick.transform.localRotation = stickR;
            MouthStick.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);


            /************************************Body**************************************/
            stickJoint = joint_data_list.data[5];
            var stickJoint_b = joint_data_list.data[12];
            stickPos = stickJoint.Position;
            var stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            Vector3 stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            float stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            Shoulders.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            Shoulders.transform.LookAt(RightShoulderStick.transform.position);
            Shoulders.transform.Rotate(90, 0, 0);
            Shoulders.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

            stickJoint = joint_data_list.data[18];
            stickJoint_b = joint_data_list.data[22];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            HipStick.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            HipStick.transform.LookAt(RightHipStick.transform.position);
            HipStick.transform.Rotate(90, 0, 0);
            HipStick.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

            stickJoint = joint_data_list.data[18];
            stickJoint_b = joint_data_list.data[5];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            TorsoLeft.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            TorsoLeft.transform.LookAt(LeftShoulderStick.transform.position);
            TorsoLeft.transform.Rotate(90, 0, 0);
            TorsoLeft.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);

            stickJoint = joint_data_list.data[12];
            stickJoint_b = joint_data_list.data[22];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            TorsoRight.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            TorsoRight.transform.LookAt(RightShoulderStick.transform.position);
            TorsoRight.transform.Rotate(90, 0, 0);
            TorsoRight.transform.localScale = new Vector3(0.2f, stick_length, 0.2f);


            /************************************Arms**************************************/
            stickJoint = joint_data_list.data[5];
            stickJoint_b = joint_data_list.data[6];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftUpperArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            LeftUpperArm.transform.LookAt(LeftElbowStick.transform.position);
            LeftUpperArm.transform.Rotate(90, 0, 0);
            LeftUpperArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

            stickJoint = joint_data_list.data[12];
            stickJoint_b = joint_data_list.data[13];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightUpperArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            RightUpperArm.transform.LookAt(RightElbowStick.transform.position);
            RightUpperArm.transform.Rotate(90, 0, 0);
            RightUpperArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

            stickJoint = joint_data_list.data[6];
            stickJoint_b = joint_data_list.data[7];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftLowerArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            LeftLowerArm.transform.LookAt(LeftWristStick.transform.position);
            LeftLowerArm.transform.Rotate(90, 0, 0);
            LeftLowerArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

            stickJoint = joint_data_list.data[13];
            stickJoint_b = joint_data_list.data[14];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightLowerArm.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            RightLowerArm.transform.LookAt(RightWristStick.transform.position);
            RightLowerArm.transform.Rotate(90, 0, 0);
            RightLowerArm.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);


            /************************************Legs**************************************/
            stickJoint = joint_data_list.data[18];
            stickJoint_b = joint_data_list.data[19];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftUpperLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            LeftUpperLeg.transform.LookAt(LeftHipStick.transform.position);
            LeftUpperLeg.transform.Rotate(90, 0, 0);
            LeftUpperLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

            stickJoint = joint_data_list.data[22];
            stickJoint_b = joint_data_list.data[23];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightUpperLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            RightUpperLeg.transform.LookAt(RightHipStick.transform.position);
            RightUpperLeg.transform.Rotate(90, 0, 0);
            RightUpperLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

            stickJoint = joint_data_list.data[19];
            stickJoint_b = joint_data_list.data[20];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            LeftLowerLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            LeftLowerLeg.transform.LookAt(LeftKneeStick.transform.position);
            LeftLowerLeg.transform.Rotate(90, 0, 0);
            LeftLowerLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);

            stickJoint = joint_data_list.data[23];
            stickJoint_b = joint_data_list.data[24];
            stickPos = stickJoint.Position;
            stickPos_b = stickJoint_b.Position;
            stickOrientation = stickJoint.Orientation;
            stick = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) - new Vector3(stickPos_b[0], -stickPos_b[1], stickPos_b[2]);
            stick_length = stick.magnitude * 0.002f;
            stickV = new Vector3(stickPos[0], -stickPos[1], stickPos[2]) * 0.004f;
            stickR = new Quaternion(stickOrientation[0], stickOrientation[1], stickOrientation[2], stickOrientation[3]);
            RightLowerLeg.transform.localPosition = new Vector3((stickPos[0] + stickPos_b[0]) * 0.5f, (-stickPos[1] - stickPos_b[1]) * 0.5f, (stickPos[2] + stickPos_b[2]) * 0.5f) * 0.008f;
            RightLowerLeg.transform.LookAt(RightKneeStick.transform.position);
            RightLowerLeg.transform.Rotate(90, 0, 0);
            RightLowerLeg.transform.localScale = new Vector3(0.2f, 1.2f, 0.2f);
        }

        public Vector3 GetReferencePosition()
        {
            return HipStick.transform.position;
        }

        public void SetActive(bool active)
        {
            SubContainerObject.SetActive(active);
        }


    }

    // Class that contains information about the robot and joints contained in an AvatarContainer object
    public class RobotContainer : IAvatarSubContainer
    {
        // stick needed for Move calculations
        // CONSIDER: own stick or global stick for avatar?
        //      is this realistic enough or should MovePerson be changed to container?
        //      "If way to apply pose to a humanoid rig is discovered, that would work directly and be simpler"

        public StickContainer stickContainer;
        //public GameObject robot;
        public GameObject SubContainerObject { get; set; }

        // CONSIDER references to parts ( ?= stick person parts)

        Animator animator;
        public Transform CharacterRootTransform;
        Dictionary<JointId, Quaternion> absoluteOffsetMap;
        public float OffsetY = 0.5f;
        public float OffsetZ = -3.0f;

        public RobotContainer(GameObject container, StickContainer stickSkeleton)
        {
            SubContainerObject = container;
            stickContainer = stickSkeleton;

            GameObject robotKyle = SubContainerObject.transform.Find("Robot Kyle").gameObject;
            CharacterRootTransform = robotKyle.transform;
            animator = robotKyle.GetComponent<Animator>();

            absoluteOffsetMap = RiggingUtils.CreateOffsetMap(animator, CharacterRootTransform);
        }

        public void MovePerson(PoseData joint_data_list)
        {
            // Remove mirroring before applying pose and readd it afterwards
            // Necesary because MoveRiggedAvatar function works in global coordinates
            Vector3 prevScale = SubContainerObject.transform.localScale;
            SubContainerObject.transform.localScale = new Vector3(Math.Abs(prevScale.x), prevScale.y, prevScale.z);

            RiggingUtils.MoveRiggedAvatar(animator, absoluteOffsetMap, joint_data_list, CharacterRootTransform, OffsetY, OffsetZ);

            SubContainerObject.transform.localScale = prevScale;
        }

        public Vector3 GetReferencePosition()
        {
            GameObject robotRoot = CharacterRootTransform.Find("Root").gameObject;
            GameObject hip = robotRoot.transform.Find("Hip").gameObject;
            return hip.transform.position + new Vector3(0,-0.1f,0);
        }

        public void SetActive(bool active)
        {
            SubContainerObject.SetActive(active);
        }
    }

    // Class that contains information about the skinned multi-person linear body model contained in an AvatarContainer object
    public class SmplContainer : IAvatarSubContainer
    {
        // stick needed for Move calculations
        public StickContainer stickContainer;
        public GameObject SubContainerObject { get; set; }

        Animator animator;
        public Transform CharacterRootTransform;
        Dictionary<JointId, Quaternion> absoluteOffsetMap;
        public float OffsetY = 1.0f;
        public float OffsetZ = 1.0f;

        public SmplContainer(GameObject container, StickContainer stickSkeleton)
        {
            SubContainerObject = container;
            stickContainer = stickSkeleton;

            GameObject smpl_male = SubContainerObject.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            animator = smpl_male.GetComponent<Animator>();
            CharacterRootTransform = smpl_male.transform.Find("m_avg_root");

            absoluteOffsetMap = RiggingUtils.CreateOffsetMap(animator, CharacterRootTransform);
        }

        public void MovePerson(PoseData joint_data_list)
        {
            // Remove mirroring before applying pose and readd it afterwards
            // Necesary because MoveRiggedAvatar function works in global coordinates
            Vector3 prevScale = SubContainerObject.transform.localScale;
            SubContainerObject.transform.localScale = new Vector3(Math.Abs(prevScale.x), prevScale.y, prevScale.z);

            RiggingUtils.MoveRiggedAvatar(animator, absoluteOffsetMap, joint_data_list, CharacterRootTransform, OffsetY, OffsetZ);

            SubContainerObject.transform.localScale = prevScale;
        }

        public Vector3 GetReferencePosition()
        {
            GameObject pelvis = CharacterRootTransform.Find("m_avg_Pelvis").gameObject;
            GameObject Spine1 = pelvis.transform.Find("m_avg_Spine1").gameObject;
            return Spine1.transform.position + new Vector3(0,0.1f,0);
            //return stickContainer.HipStick.transform.position;
        }

        public void SetActive(bool active)
        {
            SubContainerObject.SetActive(active);
        }
    }


    // Class that keeps references to sub-objects in the scene of a avatar container
    public class AvatarContainer
    {

        // References to avatar objects in scene part of the container
        public GameObject avatarContainer;
        public CubeContainer cubeContainer;
        public StickContainer stickContainer;
        public RobotContainer robotContainer;
        public SmplContainer smplContainer;
        public Dictionary<AvatarType, IAvatarSubContainer> containers;

        // state flags for the contained avatar objects
        // CONSIDER: public/private
        public bool isMirrored = false;  // PREVIOUS COMMENT "can probably be changed to private (if no UI elements use it)"
        public AvatarType activeType = AvatarType.STICK;


        // Initialization of class object
        public AvatarContainer(GameObject avatarContainer, bool mirror = false)
        {
            this.avatarContainer = avatarContainer;

            // Find child object avatars in scene and save references in fields
            GameObject cubeC = avatarContainer.transform.Find("CubeContainer").gameObject;
            GameObject stickC = avatarContainer.transform.Find("StickContainer").gameObject;
            GameObject robotC = avatarContainer.transform.Find("RobotContainer").gameObject;
            GameObject smplC = avatarContainer.transform.Find("SMPLContainer").gameObject;
            cubeContainer = new CubeContainer(cubeC);
            stickContainer = new StickContainer(stickC);
            robotContainer = new RobotContainer(robotC, stickContainer);
            smplContainer = new SmplContainer(smplC, stickContainer);

            containers = new Dictionary<AvatarType, IAvatarSubContainer>();
            containers.Add(AvatarType.CUBE, cubeContainer);
            containers.Add(AvatarType.STICK, stickContainer);
            containers.Add(AvatarType.ROBOT, robotContainer);
            containers.Add(AvatarType.SMPL, smplContainer);
            

            // Deactivate all other avatars except stickContainer
            // Note: it is necessary to do this after getting references, otherwise objects can't be found in scene
            stickContainer.SetActive(true);
            cubeContainer.SetActive(false);
            robotContainer.SetActive(false);
            smplContainer.SetActive(false);

            Mirror(mirror);
            
        }

        // Move active avatar based on the input JointData
        public void MovePerson(PoseData live_data)
        {
            // stickContainer needs to always be updated, because score calculation relies on it
            switch (activeType)
            {
                case AvatarType.CUBE:
                    stickContainer.MovePerson(live_data);
                    cubeContainer.MovePerson(live_data);
                    break;

                case AvatarType.STICK:
                    stickContainer.MovePerson(live_data);
                    break;

                case AvatarType.ROBOT:
                    stickContainer.MovePerson(live_data);
                    robotContainer.MovePerson(live_data);
                    break;

                case AvatarType.SMPL:
                    stickContainer.MovePerson(live_data);
                    smplContainer.MovePerson(live_data);
                    break;
            }

           MoveIndicators();
        }

        public void MoveIndicators(bool forceMove = false)
        {
            Vector3 indicatorPos = new Vector3(0,0,0), cubePos = new Vector3(0, 0, 0);
            bool moveIndicators = forceMove;
            Transform scoreIndicatorTr = avatarContainer.transform.Find("ScoreIndicator");
            if (scoreIndicatorTr != null)
            {
                GameObject scoreIndicator = scoreIndicatorTr.gameObject;
                if (scoreIndicator.activeSelf)
                {
                    Vector3 newPosition = containers[activeType].GetReferencePosition();
                    indicatorPos = new Vector3(newPosition.x, newPosition.y + 0.9f, newPosition.z);
                    if ((scoreIndicator.transform.position - indicatorPos).magnitude > 1)
                        moveIndicators = true;
                        
                }
            }

            Transform pulsingObjectTr = avatarContainer.transform.Find("PulsingCube");
            if (pulsingObjectTr != null)
            {
                GameObject pulseObject = pulsingObjectTr.gameObject;
                if (pulseObject.activeSelf)
                {
                    Vector3 newPosition = containers[activeType].GetReferencePosition();
                    cubePos = new Vector3(newPosition.x, newPosition.y + 1.4f, newPosition.z);
                    if ((pulseObject.transform.position - cubePos).magnitude > 1)
                        moveIndicators = true;
                    //pulseObject.transform.position = new Vector3(newPosition.x, 1.7f, newPosition.z);
                }
            }

            Transform progressIndicatorTr = avatarContainer.transform.Find("ProgressIndicator");
            if (progressIndicatorTr != null)
            {
                GameObject progressIndicator = progressIndicatorTr.gameObject;
                if (progressIndicator.activeSelf)
                {
                    Vector3 newPosition = containers[activeType].GetReferencePosition();
                    indicatorPos = new Vector3(newPosition.x, newPosition.y + 0.9f, newPosition.z);
                    if ((progressIndicator.transform.position - indicatorPos).magnitude > 1)
                        moveIndicators = true;
                }
            }

            if (moveIndicators)
            {
                if (scoreIndicatorTr != null)
                {
                    GameObject scoreIndicator = scoreIndicatorTr.gameObject;
                    if (scoreIndicator.activeSelf)
                    {
                        scoreIndicator.transform.position = indicatorPos;

                    }
                }
                if (pulsingObjectTr != null)
                {
                    GameObject pulseObject = pulsingObjectTr.gameObject;
                    if (pulseObject.activeSelf)
                    {
                        pulseObject.transform.position = cubePos;
                    }
                }
                if (progressIndicatorTr != null)
                {
                    GameObject progressIndicator = progressIndicatorTr.gameObject;
                    if (progressIndicator.activeSelf)
                    {
                        progressIndicator.transform.position = indicatorPos;
                    }
                }
            }
        }

        // Change the currently active container-type of the avatar. (Change between stick, robot etc.)
        public void ChangeActiveType(AvatarType type)
        {
            containers[activeType].SetActive(false);
            containers[type].SetActive(true);
            activeType = type;

            MoveIndicators(true);
        }

        // Sets the mirroring of the avatar, toggles mirror if default parameter
        // Needs to mirror SubContainers, because negative scale in the main container interferes with moving object in MRTK
        public void Mirror(bool? mirror = null)
        {
            if(mirror == null)
            {
                mirror = !isMirrored;
            }

            if (mirror == false)
            {
                isMirrored = false;
                foreach (IAvatarSubContainer container in containers.Values)
                {
                    Vector3 prevScale = container.SubContainerObject.transform.localScale;
                    container.SubContainerObject.transform.localScale = new Vector3(Math.Abs(prevScale.x), prevScale.y, prevScale.z);
                }
            }
            else
            {
                isMirrored = true;
                foreach (IAvatarSubContainer container in containers.Values)
                {
                    Vector3 prevScale = container.SubContainerObject.transform.localScale;
                    container.SubContainerObject.transform.localScale = new Vector3(-Math.Abs(prevScale.x), prevScale.y, prevScale.z);
                }
            }
        }
    }
}
