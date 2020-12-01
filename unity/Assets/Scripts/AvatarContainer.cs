using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;

namespace PoseTeacher
{
    // Label for various containers
    public enum AvatarType
    {
        CUBE, STICK, ROBOT, SMPL
    }

    // Base interface for different type of containers (cube, stick etc.)
    public interface IContainer
    {
        // Activates/Deactivates the contained GameObject object
        void SetActive(bool active);
        // Move the contained GameObject object based on the input JointData
        void MovePerson(PoseData joint_data_list);

        Vector3 GetReferencePosition();
    }

    // Class that contains information about the cube contained in an AvatarContainer object
    // TODO move debugObject functionalities from AvatarContainer to CubeContainer
    public class CubeContainer : IContainer
    {
        // CONSIDER moving GameObject to interface? Change the interface to abstract base class with SetActive function there
        public GameObject cube;

        // References to cubes of the cubes avatar
        public GameObject[] debugObjects;

        public CubeContainer(GameObject container)
        {
            cube = container;
        }

        public void MovePerson(PoseData joint_data_list)
        {
            //Place cubes at position and orietation of joints
            for (JointType jt = 0; jt < JointType.Count; jt++)
            {
                var joint = joint_data_list.data[(int)jt];
                var pos = joint.Position;
                var orientation = joint.Orientation;
                var v = new Vector3(pos[0], -pos[1], pos[2]) * 0.004f;
                var r = new Quaternion(orientation[0], orientation[1], orientation[2], orientation[3]);
                var obj = debugObjects[(int)jt];
                obj.transform.localPosition = v;
                obj.transform.localRotation = r;
                obj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            }
        }

        public Vector3 GetReferencePosition()
        {
            return cube.transform.position;
        }

        public void SetActive(bool active)
        {
            cube.SetActive(active);
        }
    }

    // Class that contains information about the stick figure contained in an AvatarContainer object
    public class StickContainer : IContainer
    {
        public GameObject stick;

        // References to parts of the stick person avatar
        public GameObject LeftLowerLeg, RightLowerLeg, LeftUpperArm, RightUpperArm, LeftUpperLeg, RightUpperLeg, TorsoLeft,
           TorsoRight, HipStick, LeftLowerArm, RightLowerArm, LeftEye, RightEye, Shoulders, MouthStick, NoseStick, LeftEar, RightEar;
        public GameObject LeftShoulderStick, RightShoulderStick, LeftHipStick, RightHipStick, LeftElbowStick, RightElbowStick, LeftWristStick, RightWristStick,
            LeftKneeStick, RightKneeStick, LeftAnkleStick, RightAnkleStick;

        public StickContainer(GameObject container)
        {
            stick = container;

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
            stick.SetActive(active);
        }


    }

    // Class that contains information about the robot and joints contained in an AvatarContainer object
    public class RobotContainer : IContainer
    {
        // stick needed for Move calculations
        // CONSIDER: own stick or global stick for avatar?
        //      is this realistic enough or should MovePerson be changed to container?
        //      "If way to apply pose to a humanoid rig is discovered, that would work directly and be simpler"
        public StickContainer stickContainer;
        public GameObject robot;

        // CONSIDER references to parts ( ?= stick person parts)

        public RobotContainer(GameObject container, StickContainer stickSkeleton)
        {
            robot = container;
            stickContainer = stickSkeleton;
        }

        public void MovePerson(PoseData joint_data_list)
        {
            // CONSIDER: depending on how moving the various containers are invoked, this might be redundant
            //      if joint_data shows delta movements, invoking this function redundantly breaks it 
            //      (right now I think we are good)
            stickContainer.MovePerson(joint_data_list);


            // Below the orientation of the stick figure parts is applied to the robot avatar, with some additional calculations
            // TODO: is there a simpler way to do this?
            // If way to apply pose to a humanoid rig is discovered, that would work directly and be simpler


            // Get Robot body parts references
            GameObject right_shoulder_joint, right_upper_arm_joint, right_forearm_joint, left_upper_arm_joint, left_forearm_joint;
            GameObject left_thigh_joint, left_knee_joint, right_thigh_joint, right_knee_joint;
            GameObject robotKyle = robot.transform.Find("Robot Kyle").gameObject;
            GameObject robotRoot = robotKyle.transform.Find("Root").gameObject;
            GameObject hip = robotRoot.transform.Find("Hip").gameObject;
            GameObject ribs = robotRoot.transform.Find("Ribs").gameObject;
            GameObject left_shoulder_joint = ribs.transform.Find("Left_Shoulder_Joint_01").gameObject;

            right_shoulder_joint = ribs.transform.Find("Right_Shoulder_Joint_01").gameObject;
            bool set = false;
            if (right_shoulder_joint.transform.childCount == 1)
            {
                set = true;
                right_upper_arm_joint = right_shoulder_joint.transform.Find("Right_Upper_Arm_Joint_01").gameObject;
                left_upper_arm_joint = left_shoulder_joint.transform.Find("Left_Upper_Arm_Joint_01").gameObject;
                right_thigh_joint = hip.transform.Find("Right_Thigh_Joint_01").gameObject;
                left_thigh_joint = hip.transform.Find("Left_Thigh_Joint_01").gameObject;
            }
            else
            {
                right_upper_arm_joint = robot.transform.Find("Right_Upper_Arm_Joint_01").gameObject;
                left_upper_arm_joint = robot.transform.Find("Left_Upper_Arm_Joint_01").gameObject;
                right_thigh_joint = robot.transform.Find("Right_Thigh_Joint_01").gameObject;
                left_thigh_joint = robot.transform.Find("Left_Thigh_Joint_01").gameObject;
            }
            right_forearm_joint = right_upper_arm_joint.transform.Find("Right_Forearm_Joint_01").gameObject;
            left_forearm_joint = left_upper_arm_joint.transform.Find("Left_Forearm_Joint_01").gameObject;
            right_knee_joint = right_thigh_joint.transform.Find("Right_Knee_Joint_01").gameObject;
            left_knee_joint = left_thigh_joint.transform.Find("Left_Knee_Joint_01").gameObject;

            // TODO were these unused? 
            /*Quaternion rootTransform = Quaternion.FromToRotation(robotRoot.transform.rotation.eulerAngles, stickContainer.stick.transform.rotation.eulerAngles);
            Quaternion relativeTransform = Quaternion.FromToRotation(transform.up, stickContainer.LeftUpperArm.transform.localRotation.eulerAngles);
            Vector3 ea = stickContainer.RightUpperArm.transform.rotation.eulerAngles;*/

            // Change parents of body part to all be in global coordinates of avatar
            if (set == true)
            {
                right_upper_arm_joint.transform.SetParent(robot.transform);
                left_upper_arm_joint.transform.SetParent(robot.transform);
                right_thigh_joint.transform.SetParent(robot.transform);
                left_thigh_joint.transform.SetParent(robot.transform);
            }

            // Some manually tested rotations are applied after the calculated pose, as there are offsets
            right_upper_arm_joint.transform.localRotation = stickContainer.RightUpperArm.transform.localRotation;
            right_upper_arm_joint.transform.Rotate(0, 0, 90);

            right_forearm_joint.transform.localRotation = Quaternion.Inverse(right_upper_arm_joint.transform.localRotation) * stickContainer.RightLowerArm.transform.localRotation;
            right_forearm_joint.transform.Rotate(0, 0, 90);

            left_upper_arm_joint.transform.localRotation = stickContainer.LeftUpperArm.transform.localRotation;
            left_upper_arm_joint.transform.Rotate(180, 0, 90);
            left_forearm_joint.transform.localRotation = Quaternion.Inverse(left_upper_arm_joint.transform.localRotation) * stickContainer.LeftLowerArm.transform.localRotation;
            left_forearm_joint.transform.Rotate(180, 90, 45);

            left_thigh_joint.transform.localRotation = stickContainer.LeftUpperLeg.transform.localRotation;
            left_thigh_joint.transform.Rotate(0, 0, 90);
            left_knee_joint.transform.localRotation = Quaternion.Inverse(left_thigh_joint.transform.localRotation) * stickContainer.LeftKneeStick.transform.localRotation;
            left_knee_joint.transform.Rotate(0, 0, 170);

            right_thigh_joint.transform.localRotation = stickContainer.RightUpperLeg.transform.localRotation;
            right_thigh_joint.transform.Rotate(0, 0, -90);
            right_knee_joint.transform.localRotation = Quaternion.Inverse(right_thigh_joint.transform.localRotation) * stickContainer.RightKneeStick.transform.localRotation;
            right_knee_joint.transform.Rotate(180, 0, -170);
        }

        public Vector3 GetReferencePosition()
        {
            return stickContainer.HipStick.transform.position;
        }

        public void SetActive(bool active)
        {
            robot.SetActive(active);
        }
    }

    // Class that contains information about the skinned multi-person linear body model contained in an AvatarContainer object
    // TODO: add changer function for male/female
    public class SmplContainer : IContainer
    {
        // stick needed for Move calculations
        // CONSIDER: see robot
        public StickContainer stickContainer;
        public GameObject smpl;

        // CONSIDER references to parts ( ?= stick person parts)

        public SmplContainer(GameObject container, StickContainer stickSkeleton)
        {
            smpl = container;
            stickContainer = stickSkeleton;
        }

        public void MovePerson(PoseData joint_data_list)
        {
            // CONSIDER: depending on how moving the various containers are invoked, this might be redundant
            //      if joint_data shows delta movements, invoking this function redundantly breaks it 
            //      (right now I think we are good)
            stickContainer.MovePerson(joint_data_list);

            // Below the orientation of the stick figure parts is applied to the SMPL avatar, ith some additional calculations
            // TODO: Rewriite <check TODO moveRobotPerson, same as there>

            // get SMPL body parts
            GameObject smpl_body;
            GameObject smpl_male = smpl.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject smpl_female = smpl.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject L_hip, R_hip, L_Shoulder, R_Shoulder, R_Elbow, L_Elbow, L_Knee, R_Knee;
            bool set = false;
            if (smpl_male.activeSelf == true)
            {
                smpl_body = smpl_male;
                //GameObject L_hip, R_hip, L_Shoulder, R_Shoulder;
                GameObject SMPLRoot = smpl_body.transform.Find("m_avg_root").gameObject;
                GameObject pelvis = SMPLRoot.transform.Find("m_avg_Pelvis").gameObject;

                GameObject Spine1 = pelvis.transform.Find("m_avg_Spine1").gameObject;
                GameObject Spine2 = Spine1.transform.Find("m_avg_Spine2").gameObject;
                GameObject Spine3 = Spine2.transform.Find("m_avg_Spine3").gameObject;

                // we don't need the spine
                GameObject L_Collar = Spine3.transform.Find("m_avg_L_Collar").gameObject;

                // we don't need the hand
                GameObject R_Collar = Spine3.transform.Find("m_avg_R_Collar").gameObject;

                GameObject Neck = Spine3.transform.Find("m_avg_Neck").gameObject;

                if (L_Collar.transform.childCount == 1)
                {
                    set = true;
                    L_Shoulder = L_Collar.transform.Find("m_avg_L_Shoulder").gameObject;
                    R_Shoulder = R_Collar.transform.Find("m_avg_R_Shoulder").gameObject;
                    L_hip = pelvis.transform.Find("m_avg_L_Hip").gameObject;
                    R_hip = pelvis.transform.Find("m_avg_R_Hip").gameObject;
                }
                else
                {
                    L_Shoulder = smpl.transform.Find("m_avg_L_Shoulder").gameObject;
                    R_Shoulder = smpl.transform.Find("m_avg_R_Shoulder").gameObject;
                    L_hip = smpl.transform.Find("m_avg_L_Hip").gameObject;
                    R_hip = smpl.transform.Find("m_avg_R_Hip").gameObject;
                }

                L_Knee = L_hip.transform.Find("m_avg_L_Knee").gameObject;
                GameObject L_Ankle = L_Knee.transform.Find("m_avg_L_Ankle").gameObject;
                // we don't need the foot and the ankle not much either

                R_Knee = R_hip.transform.Find("m_avg_R_Knee").gameObject;
                GameObject R_Ankle = R_Knee.transform.Find("m_avg_R_Ankle").gameObject;

                L_Elbow = L_Shoulder.transform.Find("m_avg_L_Elbow").gameObject;
                GameObject L_Wrist = L_Elbow.transform.Find("m_avg_L_Wrist").gameObject;
                R_Elbow = R_Shoulder.transform.Find("m_avg_R_Elbow").gameObject;
                GameObject R_Wrist = R_Elbow.transform.Find("m_avg_R_Wrist").gameObject;

            }
            else
            {
                smpl_body = smpl_female;
                //GameObject L_hip, R_hip, L_Shoulder, R_Shoulder;
                GameObject SMPLRoot = smpl_body.transform.Find("f_avg_root").gameObject;
                GameObject pelvis = SMPLRoot.transform.Find("f_avg_Pelvis").gameObject;

                GameObject Spine1 = pelvis.transform.Find("f_avg_Spine1").gameObject;
                GameObject Spine2 = Spine1.transform.Find("f_avg_Spine2").gameObject;
                GameObject Spine3 = Spine2.transform.Find("f_avg_Spine3").gameObject;

                // we don't need the spine
                GameObject L_Collar = Spine3.transform.Find("f_avg_L_Collar").gameObject;

                // we don't need the hand
                GameObject R_Collar = Spine3.transform.Find("f_avg_R_Collar").gameObject;

                GameObject Neck = Spine3.transform.Find("f_avg_Neck").gameObject;

                //right_shoulder_joint = ribs.transform.Find("Right_Shoulder_Joint_01").gameObject;
                //bool set = false;
                if (L_Collar.transform.childCount == 1)
                {
                    set = true;
                    L_Shoulder = L_Collar.transform.Find("f_avg_L_Shoulder").gameObject;
                    R_Shoulder = R_Collar.transform.Find("f_avg_R_Shoulder").gameObject;
                    L_hip = pelvis.transform.Find("f_avg_L_Hip").gameObject;
                    R_hip = pelvis.transform.Find("f_avg_R_Hip").gameObject;
                }
                else
                {
                    L_Shoulder = smpl.transform.Find("f_avg_L_Shoulder").gameObject;
                    R_Shoulder = smpl.transform.Find("f_avg_R_Shoulder").gameObject;
                    L_hip = smpl.transform.Find("f_avg_L_Hip").gameObject;
                    R_hip = smpl.transform.Find("f_avg_R_Hip").gameObject;
                }
                L_Knee = L_hip.transform.Find("f_avg_L_Knee").gameObject;
                GameObject L_Ankle = L_Knee.transform.Find("f_avg_L_Ankle").gameObject;
                // we don't need the foot and the ankle not much either

                R_Knee = R_hip.transform.Find("f_avg_R_Knee").gameObject;
                GameObject R_Ankle = R_Knee.transform.Find("f_avg_R_Ankle").gameObject;

                L_Elbow = L_Shoulder.transform.Find("f_avg_L_Elbow").gameObject;
                GameObject L_Wrist = L_Elbow.transform.Find("f_avg_L_Wrist").gameObject;
                R_Elbow = R_Shoulder.transform.Find("f_avg_R_Elbow").gameObject;
                GameObject R_Wrist = R_Elbow.transform.Find("f_avg_R_Wrist").gameObject;
            }


            if (set == true)
            {
                L_Shoulder.transform.SetParent(smpl.transform);
                R_Shoulder.transform.SetParent(smpl.transform);
                L_hip.transform.SetParent(smpl.transform);
                R_hip.transform.SetParent(smpl.transform);
            }

            // Some manually tested rotations are applied after the calculated pose, as there are offsets
            R_Shoulder.transform.localRotation = stickContainer.RightUpperArm.transform.localRotation;
            R_Shoulder.transform.Rotate(0, 180, 90);
            R_Elbow.transform.localRotation = Quaternion.Inverse(R_Shoulder.transform.localRotation) * stickContainer.RightLowerArm.transform.localRotation;
            R_Elbow.transform.Rotate(0, 180, 90);

            L_Shoulder.transform.localRotation = stickContainer.LeftUpperArm.transform.localRotation;
            L_Shoulder.transform.Rotate(180, 0, 90);
            L_Elbow.transform.localRotation = Quaternion.Inverse(L_Shoulder.transform.localRotation) * stickContainer.LeftLowerArm.transform.localRotation;
            L_Elbow.transform.Rotate(180, 0, 90);

            L_hip.transform.localRotation = stickContainer.LeftUpperLeg.transform.localRotation;
            L_hip.transform.Rotate(0, 90, 0);
            L_Knee.transform.localRotation = Quaternion.Inverse(L_hip.transform.localRotation) * stickContainer.LeftKneeStick.transform.localRotation;
            L_Knee.transform.Rotate(-90, 180, -90);

            R_hip.transform.localRotation = stickContainer.RightUpperLeg.transform.localRotation;
            R_hip.transform.Rotate(0, 90, 0);
            R_Knee.transform.localRotation = Quaternion.Inverse(R_hip.transform.localRotation) * stickContainer.RightKneeStick.transform.localRotation;
            R_Knee.transform.Rotate(90, 180, 90);
        }

        public Vector3 GetReferencePosition()
        {
            return stickContainer.HipStick.transform.position;
        }

        public void SetActive(bool active)
        {
            smpl.SetActive(active);
        }
    }


    // Class that keeps references to sub-objects in the scene of a avatar container
    // Probably use this when refactoring as the base class for the script attached to avatar containers
    // TODO: 
    //   !  rename class (include comments, use VS rename function!)
    public class AvatarContainer
    {

        // TODO: if cube avatar class is created, migrate this to there. Otherwise move the getting of other refrences from init to here
        // if the function stays, it should assign direcly to the field of the class
        // create function declaration and move this function under initializer
        void prepareGameObjects(GameObject avatarContainer, ref GameObject[] debugObjects)
        {
            // Gets refernces to the cube children of the cube container
            // TODO: do this after finding cubeContainer and setting to field is done in init, to not run Find again
            GameObject cubeC = avatarContainer.transform.Find("CubeContainer").gameObject;
            debugObjects = new GameObject[(int)JointType.Count];
            for (var i = 0; i < (int)JointType.Count; i++)
            {
                // Find cube children, and insert refrences in same order as joints from the BT SDK
                // Note: cube objects have same name as the joints in the SDK
                var cube = cubeC.transform.Find(Enum.GetName(typeof(JointType), i)).gameObject;
                cube.transform.localScale = Vector3.one * 0.4f;
                cube.transform.SetParent(cubeC.transform);
                debugObjects[i] = cube;
            }

        }

        // References to avatar objects in scene part of the container
        public GameObject avatarContainer;
        public CubeContainer cubeContainer;
        public StickContainer stickContainer;
        public RobotContainer robotContainer;
        public SmplContainer smplContainer;
        public Dictionary<AvatarType, IContainer> containers;

        // state flags for the contained avatar objects
        // CONSIDER: public/private
        public bool isMirrored = true;  // PREVIOUS COMMENT "can probably be changed to private (if no UI elements use it)"
        public AvatarType activeType = AvatarType.STICK;


        // Initialization of class object
        public AvatarContainer(GameObject avatarContainer)
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

            containers = new Dictionary<AvatarType, IContainer>();
            containers.Add(AvatarType.CUBE, cubeContainer);
            containers.Add(AvatarType.STICK, stickContainer);
            containers.Add(AvatarType.ROBOT, robotContainer);
            containers.Add(AvatarType.SMPL, smplContainer);

            // TODO: already mentionned above:
            // if the function stays, it should assign direcly to the field of the class
            GameObject[] debugObjects = { };
            prepareGameObjects(avatarContainer, ref debugObjects);
            this.cubeContainer.debugObjects = debugObjects;

            // Deactivate all other avatars except stickContainer
            // Note: it is necessary to do this after getting references, otherwise objects can't be found in scene
            stickContainer.SetActive(true);
            cubeContainer.SetActive(false);
            robotContainer.SetActive(false);
            smplContainer.SetActive(false);
        }

        // Move active avatar based on the input JointData
        public void MovePerson(PoseData live_data)
        {
            switch (activeType)
            {
                case AvatarType.CUBE:
                    cubeContainer.MovePerson(live_data);
                    break;

                case AvatarType.STICK:
                    stickContainer.MovePerson(live_data);
                    break;

                case AvatarType.ROBOT:
                    robotContainer.MovePerson(live_data);
                    break;

                case AvatarType.SMPL:
                    smplContainer.MovePerson(live_data);
                    break;
            }

           MoveIndicators();

        }

        private void MoveIndicators()
        {
            Transform scoreIndicatorTr = avatarContainer.transform.Find("ScoreIndicator");
            if (scoreIndicatorTr != null)
            {
                GameObject scoreIndicator = scoreIndicatorTr.gameObject;
                if (scoreIndicator.activeSelf)
                {
                    Vector3 newPosition = containers[activeType].GetReferencePosition();
                    newPosition = new Vector3(newPosition.x, 1.2f, newPosition.z);
                    if ((scoreIndicator.transform.position - newPosition).magnitude > 1)
                        scoreIndicator.transform.position = newPosition;
                }
            }

            Transform pulsingObjectTr = avatarContainer.transform.Find("PulsingCube");
            if (pulsingObjectTr != null)
            {
                GameObject pulseObject = pulsingObjectTr.gameObject;
                if (pulseObject.activeSelf)
                {
                    Vector3 newPosition = containers[activeType].GetReferencePosition();
                    newPosition = new Vector3(newPosition.x, 1.7f, newPosition.z);
                    if ((pulseObject.transform.position - newPosition).magnitude > 1)
                        pulseObject.transform.position = newPosition;
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
                    newPosition = new Vector3(newPosition.x, 1.2f, newPosition.z);
                    if ((progressIndicator.transform.position - newPosition).magnitude > 1)
                        progressIndicator.transform.position = newPosition;
                }
            }
        }

        // Change the currently active container-type of the avatar. (Change between stick, robot etc.)
        public void ChangeActiveType(AvatarType type)
        {
            containers[activeType].SetActive(false);
            containers[type].SetActive(true);
            activeType = type;

            MoveIndicators();
        }

        // Mirrors the avatar
        public void Mirror()
        {
            if (isMirrored == true)
            {
                isMirrored = false;
                avatarContainer.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
            }
            else
            {
                isMirrored = true;
                avatarContainer.transform.localScale = new Vector3(-1.0f, 1.0f, 1.0f);
            }
        }
    }
}
