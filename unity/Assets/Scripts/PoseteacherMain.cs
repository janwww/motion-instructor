﻿using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using NativeWebSocket;
using System.Security.Permissions;

// Base interface for different type of containers (cube, stick etc.)
interface Container
{
    // Activates/Deactivates the contained GameObject object
    void SetActive(bool active);
    // Move the contained GameObject object based on the input JointData
    void MovePerson(JointDataListLive joint_data_list);
}

// Class that contains information about the cube contained in an AvatarGo object
// TODO move debugObject functionalities from AvatarGo to CubeContainer
class CubeContainer : Container
{
    // CONSIDER moving GameObject to interface? Change the interface to abstract base class with SetActive function there
    public GameObject cube;

    // References to cubes of the cubes avatar
    public GameObject[] debugObjects;

    public CubeContainer(GameObject container)
    {
        cube = container;
    }

    public void MovePerson(JointDataListLive joint_data_list)
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

    public void SetActive(bool active)
    {
        cube.SetActive(active);
    }
}

// Class that contains information about the stick figure contained in an AvatarGo object
class StickContainer : Container
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

    public void MovePerson(JointDataListLive joint_data_list)
    {
        /************************************Joints**************************************/
        JointDataLive stickJoint = joint_data_list.data[5];
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

    public void SetActive(bool active)
    {
        stick.SetActive(active);
    }
}

// Class that contains information about the robot and joints contained in an AvatarGo object
class RobotContainer : Container
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

    public void MovePerson(JointDataListLive joint_data_list)
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

    public void SetActive(bool active)
    {
        robot.SetActive(active);
    }
}

// Class that contains information about the skinned multi-person linear body model contained in an AvatarGo object
class SmplContainer : Container
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

    public void MovePerson(JointDataListLive joint_data_list)
    {
        // CONSIDER: depending on how moving the various containers are invoked, this might be redundant
        //      if joint_data shows delta movements, invoking this function redundantly breaks it 
        //      (right now I think we are good)
        stickContainer.MovePerson(joint_data_list);


        // Below the orientation of the stick figure parts is applied to the SMPL avatar, ith some additional calculations
        // TODO: <check TODO moveRobotPerson, same as there>

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

    public void SetActive(bool active)
    {
        smpl.SetActive(active);
    }
}

// Label for various containers
enum AvatarType
{
    CUBE, STICK, ROBOT, SMPL
}

// Class that keeps references to sub-objects in the scene of a avatar container
// Probably use this when refactoring as the base class for the script attached to avatar containers
// TODO: 
//      implement functions for changing the currently used avatar (done), 
//      and keep state to not have to setActive on all avatars. (Use enum for current avatar?) (done)
//      transfer functions for moving the avatars into here (moved to Containers, done)
//   !  rename class (include comments, use VS rename function!)
class AvatarGo
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
    public Dictionary<AvatarType, Container> containers;

    // state flags for the contained avatar objects
    // CONSIDER: public/private
    public bool isMirrored = true;  // PREVIOUS COMMENT "can probably be changed to private (if no UI elements use it)"
    public AvatarType activeType = AvatarType.STICK;


    // Initialization of class object
    public AvatarGo(GameObject avatarContainer)
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

        containers = new Dictionary<AvatarType, Container>();
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
    public void MovePerson(JointDataListLive live_data)
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
    }

    // Change the currently active container-type of the avatar. (Change between stick, robot etc.)
    public void ChangeActiveType(AvatarType type)
    {
        containers[activeType].SetActive(false);
        containers[type].SetActive(true);
        activeType = type;
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

// Main script
// TODO:
// Reorder functions (need to define placeholders first for somoe)
public class PoseteacherMain : MonoBehaviour
{

    Device device;
    BodyTracker tracker;

    // Used for displaying RGB Kinect video
    public Renderer renderer;
    RawImage m_RawImage;
    GameObject streamCanvas;
    GameObject videoCube;

    // Refrences to containers in scene
    // TODO: change it to list(s) or other flexible datastructure to allow more (partially done)
    // TODO find good way to reference objects. @Unity might do this for us
    AvatarGo avatarSelf;
    AvatarGo avatarTeacher;
    AvatarGo avatarSelf2;
    AvatarGo avatarTeacher2;
    List<AvatarGo> avatarListSelf;
    List<AvatarGo> avatarListTeacher;
    
    GameObject spatialAwarenessSystem;
    

    //Select a Texture in the Inspector to change it (unused)
    public Texture m_Texture;
    Texture2D tex;

    // Decides what action is being done with joints
    // TODO: 
    // Change to enum (!!! making this changes makes buttons in scene unusable)
    // Make setter functions? For loading file and reset recording, should be rewritten as separate functions, not values here
    // 3 and 4 are currently not used/implemented
    // 3 not necessary as load happens automatically at playback
    // 4 disabled to not accidentaly delete current file contents
    public int recording_mode = 0; // 0: not recording, 1: recording, 2: playback, 3: load_file, 4: reset_recording
    // 
    // TODO:
    // Change to enum? (!!! making this changes makes button playback speed change in scene unusable)
    public int playback_speed = 1; // 1: x1 , 2: x0.5, 4: x0.25
    private int playback_counter = 0;
    
    IEnumerable<string> read_recording_data;
    IEnumerator<string> sequenceEnum;
    JointDataList saved_joint_data;
    JointDataListLive remote_joints_live = null; // Live (newest) joint data

    // File that is being read from
    string current_file = "jsondata.txt";

    // can be changed in the UI
    // TODO: probaly change to using functions to toggle
    public bool isMaleSMPL = true; 
    public bool usingKinectAlternative = true;
    

    public bool mirroring = true; // can probably be changed to private (if no UI elements use it)
    public bool debugMode = true;
    private bool loadedFakeData = false;

    // Web socket is currently used only for Kinect Alternative
    WebSocket websocket;
    public string WS_ip = "ws://localhost:2567";

    // Used for showing if pose is correct
    public Material normal_material;
    public Material correct_material;
    public Material incorrect_material;
    public bool shouldCheckCorrectness = true;
    public float correctionThresh = 30.0f;

    // Mirror all avatar containers
    // TODO: Move code to AvatarGo class (partial done)
    //      Conform to naming conventions
    // Consider: what to do with videoCube and streamCanvas?
    //      Do we really need this function, or we will just set this one-by-one?
    public void do_mirror()
    {

        if (mirroring == true)
        {
            mirroring = false;
            // TODO figure out how to handle selection of mirrored avatar
            foreach (AvatarGo avatar in avatarListSelf)
            {
                avatar.Mirror();
            }
            foreach (AvatarGo avatar in avatarListTeacher)
            {
                avatar.Mirror();
            }

            videoCube.transform.localScale = new Vector3(-1f, 0.6f, 0.1f);
            streamCanvas.transform.localScale = new Vector3(32f, 16f, 1f);
        }
        else
        {
            mirroring = true;
            foreach (AvatarGo avatar in avatarListSelf)
            {
                avatar.Mirror();
            }
            foreach (AvatarGo avatar in avatarListTeacher)
            {
                avatar.Mirror();
            }

            videoCube.transform.localScale = new Vector3(1f, 0.6f, 0.1f);
            streamCanvas.transform.localScale = new Vector3(-32f, 16f, 1f);
        }
    }

    // Setters used in UI, configured in Inspector tab of respective buttons
    // Changing the names could (shouldn't) break the link to UI
    // Changing function signatures (parameters) WILL break the link to UI
    public void set_SMPL(bool value)
    {
        isMaleSMPL = value;
    }
    public void set_OpenPose(bool value)
    {
        usingKinectAlternative = value;
    }
    public void set_recording_mode(int rec)
    {
        recording_mode = rec;
    }
    public void set_correctionThresh(float thresh)
    {
        correctionThresh = thresh;
    }
    public void set_playback_speed(int a)
    {
        playback_speed = a;
    }
    public void set_current_file(string curr)
    {
        current_file = curr;
    }

    // Generate new filename with timestamp and set as file to write to
    public void gen_new_current_file()
    {
        string timestamp = DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
        current_file = "jsondata/" + timestamp + ".txt";
    }


    // Class reperesenting a pose, only used for getting pose from websocket 
    // in format of lightweight-human-pose-estimation-3d-demo.pytorch (mentioned in README)
    [Serializable]
    public class RemoteJointList
    {
        public List<List<double>> values { get; set; }
    }

    // Do once on scene startup
    async void Start()
    {
        // Contains code to instantiate a websocket to obtain pose data
        // Web socket is currently used only for Kinect Alternative


        // Unity apparently cannot deserialize arrays, so we had to do it manually
        // TODO: Function only used once in Start, but maybe should be outside
        RemoteJointList DeserializeRJL(string message)
        {
            string[] joints3D = message.Split(new string[] { "], [" }, StringSplitOptions.None);
            joints3D[0] = joints3D[0].Substring(3);
            joints3D[18] = joints3D[18].Split(']')[0];
            
            List<List<double>> joints = new List<List<double>>();
            for (int i = 0; i < 19; i++)
            {   
                string [] joint = joints3D[i].Split(',');
                List<double> dbl = new List<double> { Convert.ToDouble(joint[0]), Convert.ToDouble(joint[1]), Convert.ToDouble(joint[2]) };
                joints.Add(dbl);
            }
            RemoteJointList rjl = new RemoteJointList { values = joints };
            return rjl;
        }
        
        // TODO: remove static socket ip , replace from field
        websocket = new WebSocket("ws://localhost:2567");

        websocket.OnOpen += () =>
        {
            Debug.Log("WS connection open!");
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WS connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            // If joint information is recieved, set remote_joints_live
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("WS message received: " + message);
            var remote_joints = DeserializeRJL(message);
            Debug.Log(remote_joints);
            remote_joints_live = Remote_to_JDL_Live(remote_joints);
        };

        // Keep sending messages at every 0.3s
        InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

        await websocket.Connect();
    }

    // For testing websocket
    async void SendWebSocketMessage()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending text
            await websocket.SendText("Test message");
        }
    }

    // Actions to do before quitting application
    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }


    // Activated once, on enable (at the start)
    // TODO: Merge with Start function as this script is never disabled
    private void OnEnable()
    {
        Debug.Log("OnEnable");

        // Get refrences to objects used to show RGB video (Kinect)
        streamCanvas = GameObject.Find("RawImage");
        videoCube = GameObject.Find("VideoCube");

        // Find avatar container objects, and initialize their respective AvatarGo classes
        GameObject avatarContainer = GameObject.Find("AvatarContainer");
        GameObject avatarContainerT = GameObject.Find("AvatarContainerT");
        GameObject avatarContainer2 = GameObject.Find("AvatarContainer2");
        GameObject avatarContainerT2 = GameObject.Find("AvatarContainerT2");

        avatarListSelf = new List<AvatarGo>();
        avatarListTeacher = new List<AvatarGo>();
        avatarListSelf.Add(new AvatarGo(avatarContainer));
        avatarListSelf.Add(new AvatarGo(avatarContainer2));
        avatarListTeacher.Add(new AvatarGo(avatarContainerT));
        avatarListTeacher.Add(new AvatarGo(avatarContainerT2));
        /*avatarSelf = new AvatarGo(avatarContainer);
        avatarTeacher = new AvatarGo(avatarContainerT);
        avatarSelf2 = new AvatarGo(avatarContainer2);
        avatarTeacher2 = new AvatarGo(avatarContainerT2);*/

        // Set unused containers to inactive
        avatarListTeacher[0].avatarContainer.gameObject.SetActive(false);
        avatarListSelf[1].avatarContainer.gameObject.SetActive(false);
        avatarListTeacher[1].avatarContainer.gameObject.SetActive(false);

        // Test print to log
        Debug.Log(streamCanvas);

        // Setup device and tracker for Azure Kinect Body Tracking
        if (usingKinectAlternative == false)
        {
            this.device = Device.Open(0);
            var config = new DeviceConfiguration
            {
                ColorResolution = ColorResolution.r720p,
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_Unbinned
            };
            device.StartCameras(config);

            var calibration = device.GetCalibration(config.DepthMode, config.ColorResolution);

            var trackerConfiguration = new TrackerConfiguration
            {
                SensorOrientation = SensorOrientation.OrientationDefault,
                CpuOnlyMode = false
            };
            this.tracker = BodyTracker.Create(calibration, trackerConfiguration);
        }

        
    }

    // If script onbject is disabled, (normally this never happens)
    // when usingKinectAlternative == false the varables are unitialized i.e. ==null
    private void OnDisable()
    {
        if (tracker != null)
        {
            tracker.Dispose();
        }
        if (device != null)
        {
            device.Dispose();
        }
    }

    // Change recording mode via keyboard input for debugging and to not need menu
    void checkKeyInput()
    {
        if (Input.GetKey(KeyCode.X))
        {
            Debug.Log("X - set recording_mode to 0 (not recording)");
            recording_mode = 0;
        }
        else if (Input.GetKey(KeyCode.Y))
        {
            Debug.Log("Y - set recording_mode to 1 (recording)");
            recording_mode = 1;
        }
        else if (Input.GetKey(KeyCode.Z))
        {
            Debug.Log("Z - set recording_mode to 2 (playback)");
            recording_mode = 2;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            Debug.Log("L - set recording_mode to 3 (load_file)");
            recording_mode = 3;
        }
        else if (Input.GetKey(KeyCode.R))
        {
            Debug.Log("R - set recording_mode to 4 (reset_recording)");
            recording_mode = 4;
        }
    }

    // (REMOVED) Functions for moving different avatars to a JointDataListLive pose
    // TODO: move functions to AvatarGo class (done)
    //      remove code duplication (done)
    //   !  remove unused lines of code -> see various container classes



    // Change material of stickContainer avatar if not close enough to teacher pose
    // CONSIDER: move (part of it) to AvatarGo or Containers
    void showCorrection(AvatarGo avatarSelf, AvatarGo avatarTeacher)
    {
        float LeftUpperArm_difference = Quaternion.Angle(avatarSelf.stickContainer.LeftUpperArm.transform.localRotation, avatarTeacher.stickContainer.LeftUpperArm.transform.localRotation);
        float LeftLowerArm_difference = Quaternion.Angle(avatarSelf.stickContainer.LeftLowerArm.transform.localRotation, avatarTeacher.stickContainer.LeftLowerArm.transform.localRotation);
        float RightUpperArm_difference = Quaternion.Angle(avatarSelf.stickContainer.RightUpperArm.transform.localRotation, avatarTeacher.stickContainer.RightUpperArm.transform.localRotation);
        float RightLowerArm_difference = Quaternion.Angle(avatarSelf.stickContainer.RightLowerArm.transform.localRotation, avatarTeacher.stickContainer.RightLowerArm.transform.localRotation);
        float LeftUpperLeg_difference = Quaternion.Angle(avatarSelf.stickContainer.LeftUpperLeg.transform.localRotation, avatarTeacher.stickContainer.LeftUpperLeg.transform.localRotation);
        float LeftLowerLeg_difference = Quaternion.Angle(avatarSelf.stickContainer.LeftLowerLeg.transform.localRotation, avatarTeacher.stickContainer.LeftLowerLeg.transform.localRotation);
        float RightUpperLeg_difference = Quaternion.Angle(avatarSelf.stickContainer.RightUpperLeg.transform.localRotation, avatarTeacher.stickContainer.RightUpperLeg.transform.localRotation);
        float RightLowerLeg_difference = Quaternion.Angle(avatarSelf.stickContainer.RightLowerLeg.transform.localRotation, avatarTeacher.stickContainer.RightLowerLeg.transform.localRotation);
        float thresh = correctionThresh;

        if (avatarSelf.stickContainer.stick.activeSelf)
        {
            avatarSelf.stickContainer.LeftUpperArm.GetComponent<Renderer>().material = (LeftUpperArm_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.stickContainer.LeftLowerArm.GetComponent<Renderer>().material = (LeftLowerArm_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.stickContainer.RightUpperArm.GetComponent<Renderer>().material = (RightUpperArm_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.stickContainer.RightLowerArm.GetComponent<Renderer>().material = (RightLowerArm_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.stickContainer.LeftUpperLeg.GetComponent<Renderer>().material = (LeftUpperLeg_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.stickContainer.LeftLowerLeg.GetComponent<Renderer>().material = (LeftLowerLeg_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.stickContainer.RightUpperLeg.GetComponent<Renderer>().material = (RightUpperLeg_difference > thresh) ? incorrect_material : correct_material;
            avatarSelf.stickContainer.RightLowerLeg.GetComponent<Renderer>().material = (RightLowerLeg_difference > thresh) ? incorrect_material : correct_material;
        }
    }

    // Converts Azure Kinect SDK BT Body to JointDataList (serializable for JSON) pose
    JointDataList convertBodyToJDL(Body body)
    {
        JointData[] joint_data_array = new JointData[(int)JointType.Count];
        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            // write recorded poses to file
            var joint = body.Skeleton.Joints[(int)jt];
            var pos = joint.Position;
            var orientation = joint.Orientation;
            // save raw data
            var v = new Vector3(pos.X, pos.Y, pos.Z);
            var r = new Quaternion(orientation.X, orientation.Y, orientation.Z, orientation.W);
            var joint_data = new JointData { position = JsonUtility.ToJson(v), rotation = JsonUtility.ToJson(r) };
            joint_data_array[(int)jt] = joint_data;
        }
        JointDataList jdl = new JointDataList { data = joint_data_array };
        return jdl;
    }

    // Converts JSON string to JointDataListLive pose
    JointDataListLive JSON_to_JDL_Live(string frame_json)
    {
        JointDataLive[] joint_data_recorded_array = new JointDataLive[(int)JointType.Count];
        JointDataListLive recorded_data = new JointDataListLive { data = { } };

        Debug.Log(frame_json);
        saved_joint_data = JsonUtility.FromJson<JointDataList>(frame_json);
        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            // play recording
            JointData jd = saved_joint_data.data[(int)jt];
            Vector3 v_saved = JsonUtility.FromJson<Vector3>(jd.position);
            Quaternion r_saved = JsonUtility.FromJson<Quaternion>(jd.rotation);
            var joint_data_live = new JointDataLive
            {
                Position = v_saved,
                Orientation = r_saved
            };
            joint_data_recorded_array[(int)jt] = joint_data_live;
        }
        recorded_data = new JointDataListLive { data = joint_data_recorded_array };
        return recorded_data;
    }

    // Converts Azure Kinect SDK BT Body to JointDataListLive pose
    JointDataListLive Body_to_JDL_Live(Body body)
    {
        JointDataLive[] joint_data_live_array = new JointDataLive[(int)JointType.Count];
        JointDataListLive live_data = new JointDataListLive { data = { } };

        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            var stickJoint = body.Skeleton.Joints[(int)jt];
            var stickPos = stickJoint.Position;
            var stickOrientation = stickJoint.Orientation;
            var joint_data_live = new JointDataLive
            {
                Position = new Vector3(stickPos.X, stickPos.Y, stickPos.Z),
                Orientation = new Quaternion(stickOrientation.X, stickOrientation.Y, stickOrientation.Z, stickOrientation.W)
            };
            joint_data_live_array[(int)jt] = joint_data_live;
        }
        live_data = new JointDataListLive { data = joint_data_live_array };
        return live_data;
    }

    // Converts RemoteJointList pose to JointDataListLive pose
    JointDataListLive Remote_to_JDL_Live(RemoteJointList rjl)
    {
        // format in  lightweight-human-pose-estimation-3d-demo.pytorch
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
        JointDataLive[] joint_data_received_array = new JointDataLive[(int)JointType.Count];

        Debug.Log(JsonUtility.ToJson(rjl));
        List<List<double>> joint_data = rjl.values;
        Debug.Log(joint_data);
        
        float scaling = 10.0f;
        for (JointType jt = 0; jt < JointType.Count; jt++)
        {
            if (dict.ContainsKey((int)jt))
            {
                int pytorch_index = dict[(int)jt];
                List<double> jtd = joint_data[pytorch_index];
                Debug.Log(jtd);
                Vector3 v_rcv = new Vector3(scaling * (float)jtd[0], scaling * (float)jtd[1], scaling * (float)jtd[2]);
                Quaternion r_rcv = new Quaternion(0, 0, 0, 0);
                var joint_data_live = new JointDataLive
                {
                    Position = v_rcv,
                    Orientation = r_rcv
                };
                joint_data_received_array[(int)jt] = joint_data_live;
            }
            else
            {   // just fill empty value as placeholder
                Vector3 v_rcv = new Vector3(0,0,0);
                Quaternion r_rcv = new Quaternion(0, 0, 0, 0);
                var joint_data_live = new JointDataLive
                {
                    Position = v_rcv,
                    Orientation = r_rcv
                };
                joint_data_received_array[(int)jt] = joint_data_live;
            }
           
        }
        JointDataListLive received_data = new JointDataListLive { data = joint_data_received_array };
        return received_data;
    }

    // Appends the passed pose (JointDataList format) to the file as JSON
    // TODO: remove unused parameter avatarSelf
    void appendRecordedFrame(JointDataList jdl, AvatarGo avatarSelf, string filename)
    {
        string json = JsonUtility.ToJson(jdl) + Environment.NewLine;
        File.AppendAllText(filename, json);
    }

    // Loads passed filename into read_recording_data and sequenceEnum fields
    // If no filename is passed, loads current_file
    public void loadRecording(string filename = "")
    {
        // read recording file
        if (filename == "")
        {
            filename = current_file;
        }
        read_recording_data = File.ReadLines(filename);
        sequenceEnum = read_recording_data.GetEnumerator();
        Debug.Log(read_recording_data);
    }

    // !!! Deletes contents of the passed filename to remove all appended poses
    // If no filename is passed current_file content is deleted
    // Currently not used
    void resetRecording(string filename = "")
    {
        // reset recording file
        if (filename == "")
        {
            filename = current_file;
        }
        File.WriteAllText(filename, "");
        Debug.Log("reset recording file");
    }

    // TODO: see inside
    //      comment function when TODO resolved
    void animateAvatars(AvatarGo avatarSelf, JointDataListLive live_data)
    {
        // MovePerson() considers which container to move
        foreach (AvatarGo avatar in avatarListSelf)
        {
            avatar.MovePerson(live_data);
        }
        // TODO: can we remove this / move it to AvatarGo or containers?
        //      using avatarSelf(2), avatarTeacher(2) is deprecated!
        if (avatarSelf.stickContainer.stick.activeSelf)
        {
            //moveStickPerson(live_data, avatarSelf); (all moveXXPerson() done at the begininng of function)
            //moveStickPerson(live_data, avatarSelf2);
            

            avatarSelf2.stickContainer.stick.gameObject.SetActive(true);
            avatarSelf2.cubeContainer.cube.gameObject.SetActive(false);
            avatarSelf2.robotContainer.robot.gameObject.SetActive(false);
            avatarSelf2.smplContainer.smpl.gameObject.SetActive(false);

            avatarTeacher.stickContainer.stick.gameObject.SetActive(true);
            avatarTeacher.cubeContainer.cube.gameObject.SetActive(false);
            avatarTeacher.robotContainer.robot.gameObject.SetActive(false);
            avatarTeacher.smplContainer.smpl.gameObject.SetActive(false);

            avatarTeacher2.stickContainer.stick.gameObject.SetActive(true);
            avatarTeacher2.cubeContainer.cube.gameObject.SetActive(false);
            avatarTeacher2.robotContainer.robot.gameObject.SetActive(false);
            avatarTeacher2.smplContainer.smpl.gameObject.SetActive(false);

        }
        else if (avatarSelf.cubeContainer.cube.activeSelf)
        {
            //moveCubePerson(live_data, avatarSelf);
            //moveCubePerson(live_data, avatarSelf2);

            avatarSelf2.stickContainer.stick.gameObject.SetActive(false);
            avatarSelf2.cubeContainer.cube.gameObject.SetActive(true);
            avatarSelf2.robotContainer.robot.gameObject.SetActive(false);
            avatarSelf2.smplContainer.smpl.gameObject.SetActive(false);

            avatarTeacher.stickContainer.stick.gameObject.SetActive(false);
            avatarTeacher.cubeContainer.cube.gameObject.SetActive(true);
            avatarTeacher.robotContainer.robot.gameObject.SetActive(false);
            avatarTeacher.smplContainer.smpl.gameObject.SetActive(false);

            avatarTeacher2.stickContainer.stick.gameObject.SetActive(false);
            avatarTeacher2.cubeContainer.cube.gameObject.SetActive(true);
            avatarTeacher2.robotContainer.robot.gameObject.SetActive(false);
            avatarTeacher2.smplContainer.smpl.gameObject.SetActive(false);

        }
        else if (avatarSelf.robotContainer.robot.activeSelf)
        {
            //moveRobotPerson(live_data, avatarSelf);
            //moveRobotPerson(live_data, avatarSelf2);

            avatarSelf2.stickContainer.stick.gameObject.SetActive(false);
            avatarSelf2.cubeContainer.cube.gameObject.SetActive(false);
            avatarSelf2.robotContainer.robot.gameObject.SetActive(true);
            avatarSelf2.smplContainer.smpl.gameObject.SetActive(false);

            avatarTeacher.stickContainer.stick.gameObject.SetActive(false);
            avatarTeacher.cubeContainer.cube.gameObject.SetActive(false);
            avatarTeacher.robotContainer.robot.gameObject.SetActive(true);
            avatarTeacher.smplContainer.smpl.gameObject.SetActive(false);

            avatarTeacher2.stickContainer.stick.gameObject.SetActive(false);
            avatarTeacher2.cubeContainer.cube.gameObject.SetActive(false);
            avatarTeacher2.robotContainer.robot.gameObject.SetActive(true);
            avatarTeacher2.smplContainer.smpl.gameObject.SetActive(false);

        }
        else if (avatarSelf.smplContainer.smpl.activeSelf)
        {
            //moveSMPLPerson(live_data, avatarSelf);
            //moveSMPLPerson(live_data, avatarSelf2);

            GameObject SMPL_male = avatarSelf.smplContainer.smpl.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_female = avatarSelf.smplContainer.smpl.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_male2 = avatarSelf2.smplContainer.smpl.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_female2 = avatarSelf2.smplContainer.smpl.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_maleT = avatarTeacher.smplContainer.smpl.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_femaleT = avatarTeacher.smplContainer.smpl.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_maleT2 = avatarTeacher2.smplContainer.smpl.transform.Find("SMPL_m_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            GameObject SMPL_femaleT2 = avatarTeacher2.smplContainer.smpl.transform.Find("SMPL_f_unityDoubleBlends_lbs_10_scale5_207_v1.0.0").gameObject;
            if (isMaleSMPL)
            // It checks here whether male or female is selected in the settings
            // note: the surrounding section only gets executed if a body is found
            {
                SMPL_male.SetActive(true);
                SMPL_male2.SetActive(true);
                SMPL_maleT.SetActive(true);
                SMPL_maleT2.SetActive(true);
                SMPL_female.SetActive(false);
                SMPL_female2.SetActive(false);
                SMPL_femaleT.SetActive(false);
                SMPL_femaleT2.SetActive(false);


            }
            else
            {
                SMPL_male.SetActive(false);
                SMPL_male2.SetActive(false);
                SMPL_maleT.SetActive(false);
                SMPL_maleT2.SetActive(false);
                SMPL_female.SetActive(true);
                SMPL_female2.SetActive(true);
                SMPL_femaleT.SetActive(true);
                SMPL_femaleT2.SetActive(true);
            }

            avatarSelf2.stickContainer.stick.gameObject.SetActive(false);
            avatarSelf2.cubeContainer.cube.gameObject.SetActive(false);
            avatarSelf2.robotContainer.robot.gameObject.SetActive(false);
            avatarSelf2.smplContainer.smpl.gameObject.SetActive(true);

            avatarTeacher.stickContainer.stick.gameObject.SetActive(false);
            avatarTeacher.cubeContainer.cube.gameObject.SetActive(false);
            avatarTeacher.robotContainer.robot.gameObject.SetActive(false);
            avatarTeacher.smplContainer.smpl.gameObject.SetActive(true);

            avatarTeacher2.stickContainer.stick.gameObject.SetActive(false);
            avatarTeacher2.cubeContainer.cube.gameObject.SetActive(false);
            avatarTeacher2.robotContainer.robot.gameObject.SetActive(false);
            avatarTeacher2.smplContainer.smpl.gameObject.SetActive(true);

        }
    }
    // Animates all teacher avatars based on the JointData provided
    void animateTeacher(AvatarGo avatarTeacher, JointDataListLive recorded_data)
    {
        foreach (AvatarGo avatar in avatarListTeacher)
        {
            avatar.MovePerson(recorded_data);
        }
    }


    // Done at each application update
    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
                websocket.DispatchMessageQueue();
        #endif

        checkKeyInput();

        // Get new pose
        if (usingKinectAlternative)
        {
            // remote_joints_live is non-null if alternative is sending pose data over websocket
            if (remote_joints_live != null)
            {
                JointDataListLive live_data = remote_joints_live;
                animateAvatars(avatarSelf, live_data);
            }
            // no data being recieved over websocket and debug mode
            else if(debugMode == true)
            {
                // load fake data once
                // TODO: should probably be in Start function
                if (loadedFakeData == false)
                {
                    loadedFakeData = true;
                    loadRecording();
                }

                // use fake data from JSON file for testing
                // TODO: have different sequenceEnum for fake input not same as used for playback teacher
                // otherwise advances pose also for teacher
                sequenceEnum.MoveNext();
                string frame_json = sequenceEnum.Current;
                JointDataListLive fake_live_data = JSON_to_JDL_Live(frame_json);
                Debug.Log(avatarSelf);
                animateAvatars(avatarSelf, fake_live_data);
            }

        }
        else
        {
            // TODO: move to function?
            // potentially simplify by not using "using" scopes
            using (Capture capture = device.GetCapture())
            {
                // Make tracker estimate body
                tracker.EnqueueCapture(capture);

                // Code for getting RGB image from camera
                var color = capture.Color;
                if (color != null && color.WidthPixels > 0)
                {
                    UnityEngine.Object.Destroy(tex);// required to not keep old images in memory
                    tex = new Texture2D(color.WidthPixels, color.HeightPixels, TextureFormat.BGRA32, false);
                    tex.LoadRawTextureData(color.GetBufferCopy());
                    tex.Apply();

                    //Fetch the RawImage component from the GameObject
                    if (streamCanvas != null)
                    {
                        m_RawImage = streamCanvas.GetComponent<RawImage>();
                        if (m_RawImage != null)
                        {
                            m_RawImage.texture = tex;
                            renderer.material.mainTexture = tex;
                        }
                    }

                }
            }

            // Get pose estimate from tracker
            using (BodyFrame frame = tracker.PopResult())
            {
                Debug.LogFormat("{0} bodies found.", frame.BodyCount);

                //  At least one body found by Body Tracking
                if (frame.BodyCount > 0)
                {
                    // Use first estimated person, if mutiple are in the image
                    // !!! There are (probably) no guarantees on consisitent ordering between estimates
                    var bodies = frame.Bodies;
                    var body = bodies[0];

                    // Apply pose to user avatar(s)
                    JointDataListLive live_data = Body_to_JDL_Live(body);
                    animateAvatars(avatarSelf, live_data);

                    // if recording pose, append to current_file file
                    if (recording_mode == 1) // recording
                    {
                        JointDataList jdl = convertBodyToJDL(body);
                        appendRecordedFrame(jdl, avatarSelf, current_file);
                    }
                }
            }
        }


        // Playback for teacher avatar(s)
        if (recording_mode == 2) // playback
        {
            // load if not yet loaded
            if (sequenceEnum == null)
            {
                loadRecording();
            }

            // play recording at different speeds
            // skip pose update if counter isn't big enought
            // TODO: maybe rewrite...
            if (playback_speed == 1) // x1
            {
                sequenceEnum.MoveNext();
            }
            else if (playback_speed == 2) // x0.5
            {
                playback_counter++;
                if (playback_counter >= 2)
                {
                    sequenceEnum.MoveNext();
                    playback_counter = 0;
                }
            }
            else //(playback_speed == 3) // x0.25
            {
                playback_counter++;
                if (playback_counter >= 4)
                {
                    sequenceEnum.MoveNext();
                    playback_counter = 0;
                }
            }

            // Get current pose and apply it to teacher avatars
            string frame_json = sequenceEnum.Current;
            JointDataListLive recorded_data = JSON_to_JDL_Live(frame_json);
            animateTeacher(avatarTeacher, recorded_data);

            // TODO: Put this in animate function?
            if (shouldCheckCorrectness)
            {
                showCorrection(avatarSelf, avatarTeacher);
            }

        }


    }

}

// TODO: The following classes should be renamed with more intuitive naming

// Class with position and orientation of one joint in string format
// Used for conversion to JSON
[Serializable]
class JointData
{
    public string position;
    public string rotation;
}

// Class with position and orientation of all joints (one pose) in string format
// Used for conversion to JSON
[Serializable]
class JointDataList
{
    public JointData[] data;
}

// Class with position and orientation of one joint
class JointDataLive
{
    public Vector3 Position;
    public Quaternion Orientation;
}

// Class with position and orientation of all joints (one pose)
// Used for applying poses to avatars
[Serializable]
class JointDataListLive
{
    public JointDataLive[] data;
}
