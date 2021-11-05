using Microsoft.Azure.Kinect.BodyTracking;
using System;
using System.Collections.Generic;
using UnityEngine;

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
            return SubContainerObject.transform.position + new Vector3(0, -0.4f, 0);
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

            Vector3 jointWeight = new Vector3(1, -1, 1) * 0.008f;

            LeftShoulderStick.transform.localPosition = Vector3.Scale(joint_data_list.data[5].Position, jointWeight);
            LeftShoulderStick.transform.localRotation = joint_data_list.data[5].Orientation;

            RightShoulderStick.transform.localPosition = Vector3.Scale(joint_data_list.data[12].Position, jointWeight);
            RightShoulderStick.transform.localRotation = joint_data_list.data[12].Orientation;

            LeftHipStick.transform.localPosition = Vector3.Scale(joint_data_list.data[18].Position, jointWeight);
            LeftHipStick.transform.localRotation = joint_data_list.data[12].Orientation;

            RightHipStick.transform.localPosition = Vector3.Scale(joint_data_list.data[22].Position, jointWeight);
            RightHipStick.transform.localRotation = joint_data_list.data[22].Orientation;

            LeftElbowStick.transform.localPosition = Vector3.Scale(joint_data_list.data[6].Position, jointWeight);
            LeftElbowStick.transform.localRotation = joint_data_list.data[6].Orientation;

            RightElbowStick.transform.localPosition = Vector3.Scale(joint_data_list.data[13].Position, jointWeight);
            RightElbowStick.transform.localRotation = joint_data_list.data[13].Orientation;

            LeftWristStick.transform.localPosition = Vector3.Scale(joint_data_list.data[7].Position, jointWeight);
            LeftWristStick.transform.localRotation = joint_data_list.data[13].Orientation;

            RightWristStick.transform.localPosition = Vector3.Scale(joint_data_list.data[14].Position, jointWeight);
            RightWristStick.transform.localRotation = joint_data_list.data[14].Orientation;

            LeftKneeStick.transform.localPosition = Vector3.Scale(joint_data_list.data[19].Position, jointWeight);
            LeftKneeStick.transform.localRotation = joint_data_list.data[19].Orientation;

            RightKneeStick.transform.localPosition = Vector3.Scale(joint_data_list.data[23].Position, jointWeight);
            RightKneeStick.transform.localRotation = joint_data_list.data[23].Orientation;

            LeftAnkleStick.transform.localPosition = Vector3.Scale(joint_data_list.data[20].Position, jointWeight);
            LeftAnkleStick.transform.localRotation = joint_data_list.data[20].Orientation;

            RightAnkleStick.transform.localPosition = Vector3.Scale(joint_data_list.data[24].Position, jointWeight);
            RightAnkleStick.transform.localRotation = joint_data_list.data[24].Orientation;


            /************************************Head**************************************/
            Vector3 eyeScale = new Vector3(0.3f, 0.2f, 0.2f);
            Vector3 otherScale = new Vector3(0.2f, 0.2f, 0.2f);

            LeftEye.transform.localPosition = Vector3.Scale(joint_data_list.data[28].Position, jointWeight);
            LeftEye.transform.localRotation = joint_data_list.data[28].Orientation;
            LeftEye.transform.localScale = eyeScale;

            RightEye.transform.localPosition = Vector3.Scale(joint_data_list.data[30].Position, jointWeight);
            RightEye.transform.localRotation = joint_data_list.data[30].Orientation;
            RightEye.transform.localScale = eyeScale;

            NoseStick.transform.localPosition = Vector3.Scale(joint_data_list.data[27].Position, jointWeight);
            NoseStick.transform.localRotation = joint_data_list.data[27].Orientation;
            NoseStick.transform.localScale = otherScale;

            LeftEar.transform.localPosition = Vector3.Scale(joint_data_list.data[29].Position, jointWeight);
            LeftEar.transform.localScale = otherScale;
            LeftEar.transform.LookAt(LeftShoulderStick.transform.position);
            LeftEar.transform.Rotate(90, 0, 0);

            RightEar.transform.localPosition = Vector3.Scale(joint_data_list.data[31].Position, jointWeight);
            RightEar.transform.localScale = otherScale;
            RightEar.transform.LookAt(RightShoulderStick.transform.position);
            RightEar.transform.Rotate(90, 0, 0);

            MouthStick.transform.localPosition = Vector3.Scale(joint_data_list.data[27].Position, jointWeight);
            MouthStick.transform.localRotation = joint_data_list.data[27].Orientation;
            MouthStick.transform.localScale = otherScale;


            /************************************Body**************************************/
            float stickWeight = 0.002f;

            Shoulders.transform.localPosition = Vector3.Scale((joint_data_list.data[5].Position + joint_data_list.data[12].Position) * 0.5f, jointWeight);
            Shoulders.transform.LookAt(RightShoulderStick.transform.position);
            Shoulders.transform.Rotate(90, 0, 0);
            Shoulders.transform.localScale = new Vector3(0.2f, Vector3.Distance(joint_data_list.data[5].Position, joint_data_list.data[12].Position) * stickWeight, 0.2f);

            HipStick.transform.localPosition = Vector3.Scale((joint_data_list.data[18].Position + joint_data_list.data[22].Position) * 0.5f, jointWeight);
            HipStick.transform.LookAt(RightHipStick.transform.position);
            HipStick.transform.Rotate(90, 0, 0);
            HipStick.transform.localScale = new Vector3(0.2f, Vector3.Distance(joint_data_list.data[18].Position, joint_data_list.data[22].Position) * stickWeight, 0.2f);

            TorsoLeft.transform.localPosition = Vector3.Scale((joint_data_list.data[18].Position + joint_data_list.data[5].Position) * 0.5f, jointWeight);
            TorsoLeft.transform.LookAt(LeftShoulderStick.transform.position);
            TorsoLeft.transform.Rotate(90, 0, 0);
            TorsoLeft.transform.localScale = new Vector3(0.2f, Vector3.Distance(joint_data_list.data[18].Position, joint_data_list.data[5].Position) * stickWeight, 0.2f);

            TorsoRight.transform.localPosition = Vector3.Scale((joint_data_list.data[12].Position + joint_data_list.data[22].Position) * 0.5f, jointWeight);
            TorsoRight.transform.LookAt(RightShoulderStick.transform.position);
            TorsoRight.transform.Rotate(90, 0, 0);
            TorsoRight.transform.localScale = new Vector3(0.2f, Vector3.Distance(joint_data_list.data[12].Position, joint_data_list.data[22].Position) * stickWeight, 0.2f);


            /************************************Arms**************************************/
            Vector3 limbScale = new Vector3(0.2f, 1.2f, 0.2f);

            LeftUpperArm.transform.localPosition = Vector3.Scale((joint_data_list.data[5].Position + joint_data_list.data[6].Position) * 0.5f, jointWeight);
            LeftUpperArm.transform.LookAt(LeftElbowStick.transform.position);
            LeftUpperArm.transform.Rotate(90, 0, 0);
            LeftUpperArm.transform.localScale = limbScale;
            
            RightUpperArm.transform.localPosition = Vector3.Scale((joint_data_list.data[12].Position + joint_data_list.data[13].Position) * 0.5f, jointWeight);
            RightUpperArm.transform.LookAt(RightElbowStick.transform.position);
            RightUpperArm.transform.Rotate(90, 0, 0);
            RightUpperArm.transform.localScale = limbScale;

            LeftLowerArm.transform.localPosition = Vector3.Scale((joint_data_list.data[6].Position + joint_data_list.data[7].Position) * 0.5f, jointWeight);
            LeftLowerArm.transform.LookAt(LeftWristStick.transform.position);
            LeftLowerArm.transform.Rotate(90, 0, 0);
            LeftLowerArm.transform.localScale = limbScale;

            RightLowerArm.transform.localPosition = Vector3.Scale((joint_data_list.data[13].Position + joint_data_list.data[14].Position) * 0.5f, jointWeight);
            RightLowerArm.transform.LookAt(RightWristStick.transform.position);
            RightLowerArm.transform.Rotate(90, 0, 0);
            RightLowerArm.transform.localScale = limbScale;


            /************************************Legs**************************************/
            LeftUpperLeg.transform.localPosition = Vector3.Scale((joint_data_list.data[18].Position + joint_data_list.data[19].Position) * 0.5f, jointWeight);
            LeftUpperLeg.transform.LookAt(LeftHipStick.transform.position);
            LeftUpperLeg.transform.Rotate(90, 0, 0);
            LeftUpperLeg.transform.localScale = limbScale;

            RightUpperLeg.transform.localPosition = Vector3.Scale((joint_data_list.data[22].Position + joint_data_list.data[23].Position) * 0.5f, jointWeight);
            RightUpperLeg.transform.LookAt(RightHipStick.transform.position);
            RightUpperLeg.transform.Rotate(90, 0, 0);
            RightUpperLeg.transform.localScale = limbScale;

            LeftLowerLeg.transform.localPosition = Vector3.Scale((joint_data_list.data[19].Position + joint_data_list.data[20].Position) * 0.5f, jointWeight);
            LeftLowerLeg.transform.LookAt(LeftKneeStick.transform.position);
            LeftLowerLeg.transform.Rotate(90, 0, 0);
            LeftLowerLeg.transform.localScale = limbScale;

            RightLowerLeg.transform.localPosition = Vector3.Scale((joint_data_list.data[23].Position + joint_data_list.data[24].Position) * 0.5f, jointWeight);
            RightLowerLeg.transform.LookAt(RightKneeStick.transform.position);
            RightLowerLeg.transform.Rotate(90, 0, 0);
            RightLowerLeg.transform.localScale = limbScale;
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
            return hip.transform.position + new Vector3(0, -0.1f, 0);
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
            return Spine1.transform.position + new Vector3(0, 0.1f, 0);
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
            if (live_data == null)
                return;

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
            Vector3 indicatorPos = new Vector3(0, 0, 0), cubePos = new Vector3(0, 0, 0);
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
            if (mirror == null)
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
