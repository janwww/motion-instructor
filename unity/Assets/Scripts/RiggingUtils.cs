using Microsoft.Azure.Kinect.BodyTracking;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PoseTeacher
{
    public static class RiggingUtils
    {
        // Copied and modified code from:
        // https://github.com/microsoft/Azure-Kinect-Samples/tree/master/body-tracking-samples/sample_unity_bodytracking

        public static Dictionary<JointId, Quaternion> basisJointMap;
        static Quaternion Y_180_FLIP = new Quaternion(0.0f, 1.0f, 0.0f, 0.0f);

        static RiggingUtils()
        {
            Vector3 zpositive = Vector3.forward;
            Vector3 xpositive = Vector3.right;
            Vector3 ypositive = Vector3.up;
            // spine and left hip are the same
            Quaternion leftHipBasis = Quaternion.LookRotation(xpositive, -zpositive);
            Quaternion spineHipBasis = Quaternion.LookRotation(xpositive, -zpositive);
            Quaternion rightHipBasis = Quaternion.LookRotation(xpositive, zpositive);
            // arms and thumbs share the same basis
            Quaternion leftArmBasis = Quaternion.LookRotation(ypositive, -zpositive);
            Quaternion rightArmBasis = Quaternion.LookRotation(-ypositive, zpositive);
            Quaternion leftHandBasis = Quaternion.LookRotation(-zpositive, -ypositive);
            Quaternion rightHandBasis = Quaternion.identity;
            Quaternion leftFootBasis = Quaternion.LookRotation(xpositive, ypositive);
            Quaternion rightFootBasis = Quaternion.LookRotation(xpositive, -ypositive);

            basisJointMap = new Dictionary<JointId, Quaternion>
            {

                // pelvis has no parent so set to count
                [JointId.Pelvis] = spineHipBasis,
                [JointId.SpineNavel] = spineHipBasis,
                [JointId.SpineChest] = spineHipBasis,
                [JointId.Neck] = spineHipBasis,
                [JointId.ClavicleLeft] = leftArmBasis,
                [JointId.ShoulderLeft] = leftArmBasis,
                [JointId.ElbowLeft] = leftArmBasis,
                [JointId.WristLeft] = leftHandBasis,
                [JointId.HandLeft] = leftHandBasis,
                [JointId.HandTipLeft] = leftHandBasis,
                [JointId.ThumbLeft] = leftArmBasis,
                [JointId.ClavicleRight] = rightArmBasis,
                [JointId.ShoulderRight] = rightArmBasis,
                [JointId.ElbowRight] = rightArmBasis,
                [JointId.WristRight] = rightHandBasis,
                [JointId.HandRight] = rightHandBasis,
                [JointId.HandTipRight] = rightHandBasis,
                [JointId.ThumbRight] = rightArmBasis,
                [JointId.HipLeft] = leftHipBasis,
                [JointId.KneeLeft] = leftHipBasis,
                [JointId.AnkleLeft] = leftHipBasis,
                [JointId.FootLeft] = leftFootBasis,
                [JointId.HipRight] = rightHipBasis,
                [JointId.KneeRight] = rightHipBasis,
                [JointId.AnkleRight] = rightHipBasis,
                [JointId.FootRight] = rightFootBasis,
                [JointId.Head] = spineHipBasis,
                [JointId.Nose] = spineHipBasis,
                [JointId.EyeLeft] = spineHipBasis,
                [JointId.EarLeft] = spineHipBasis,
                [JointId.EyeRight] = spineHipBasis,
                [JointId.EarRight] = spineHipBasis
            };
        }

        public static HumanBodyBones MapKinectJoint(JointId joint)
        {
            // https://docs.microsoft.com/en-us/azure/Kinect-dk/body-joints
            switch (joint)
            {
                case JointId.Pelvis: return HumanBodyBones.Hips;
                case JointId.SpineNavel: return HumanBodyBones.Spine;
                case JointId.SpineChest: return HumanBodyBones.Chest;
                case JointId.Neck: return HumanBodyBones.Neck;
                case JointId.Head: return HumanBodyBones.Head;
                case JointId.HipLeft: return HumanBodyBones.LeftUpperLeg;
                case JointId.KneeLeft: return HumanBodyBones.LeftLowerLeg;
                case JointId.AnkleLeft: return HumanBodyBones.LeftFoot;
                case JointId.FootLeft: return HumanBodyBones.LeftToes;
                case JointId.HipRight: return HumanBodyBones.RightUpperLeg;
                case JointId.KneeRight: return HumanBodyBones.RightLowerLeg;
                case JointId.AnkleRight: return HumanBodyBones.RightFoot;
                case JointId.FootRight: return HumanBodyBones.RightToes;
                case JointId.ClavicleLeft: return HumanBodyBones.LeftShoulder;
                case JointId.ShoulderLeft: return HumanBodyBones.LeftUpperArm;
                case JointId.ElbowLeft: return HumanBodyBones.LeftLowerArm;
                case JointId.WristLeft: return HumanBodyBones.LeftHand;
                case JointId.ClavicleRight: return HumanBodyBones.RightShoulder;
                case JointId.ShoulderRight: return HumanBodyBones.RightUpperArm;
                case JointId.ElbowRight: return HumanBodyBones.RightLowerArm;
                case JointId.WristRight: return HumanBodyBones.RightHand;
                default: return HumanBodyBones.LastBone;
            }
        }

        public static SkeletonBone GetSkeletonBone(Animator animator, string boneName)
        {
            int count = 0;
            StringBuilder cloneName = new StringBuilder(boneName);
            cloneName.Append("(Clone)");
            foreach (SkeletonBone sb in animator.avatar.humanDescription.skeleton)
            {
                if (sb.name == boneName || sb.name == cloneName.ToString())
                {
                    return animator.avatar.humanDescription.skeleton[count];
                }
                count++;
            }
            return new SkeletonBone();
        }


        public static Quaternion AbsoluteJointRotations(Quaternion jointRot, JointId jointId)
        {
            // Change rotations from local coordinate system of Kinect Joints to absolute rotations
            // https://docs.microsoft.com/en-us/azure/kinect-dk/body-joints
            return Y_180_FLIP * jointRot * Quaternion.Inverse(basisJointMap[jointId]);
        }

        public static void MoveRiggedAvatar(Animator animator, Dictionary<JointId, Quaternion> absoluteOffsetMap, PoseData poseData, Transform CharacterRootTransform, float OffsetY, float OffsetZ, float scale = 0.001f)
        {
            for (int j = 0; j < (int)JointId.Count; j++)
            {
                if (MapKinectJoint((JointId)j) != HumanBodyBones.LastBone && absoluteOffsetMap.ContainsKey((JointId)j))
                {
                    // get the absolute offset
                    Quaternion absOffset = absoluteOffsetMap[(JointId)j];
                    Transform finalJoint = animator.GetBoneTransform(MapKinectJoint((JointId)j));

                    Quaternion jointRot = AbsoluteJointRotations(poseData.data[j].Orientation, (JointId)j);

                    finalJoint.rotation = absOffset * Quaternion.Inverse(absOffset) * jointRot * absOffset;
                    if (j == 0)
                    {
                        // character root plus translation reading from the kinect, plus the offset from the script public variables
                        Vector3 root_data_pos = poseData.data[j].Position * scale;
                        finalJoint.position = CharacterRootTransform.position + new Vector3(root_data_pos.x, root_data_pos.y + OffsetY, root_data_pos.z - OffsetZ);
                    }
                }
            }
        }

        public static Dictionary<JointId, Quaternion> CreateOffsetMap(Animator animator, Transform rootJointTransform)
        {
            Dictionary<JointId, Quaternion> absoluteOffsetMap = new Dictionary<JointId, Quaternion>();
            for (int i = 0; i < (int)JointId.Count; i++)
            {
                HumanBodyBones hbb = MapKinectJoint((JointId)i);
                if (hbb != HumanBodyBones.LastBone)
                {
                    Transform transform = animator.GetBoneTransform(hbb);
                    if (transform == null)
                    {
                        Debug.Log("Skipped: " + hbb);
                        continue;
                    }

                    Quaternion absOffset = GetSkeletonBone(animator, transform.name).rotation;
                    // find the absolute offset for the tpose
                    while (!ReferenceEquals(transform.parent, rootJointTransform))
                    {
                        transform = transform.parent;
                        absOffset = GetSkeletonBone(animator, transform.name).rotation * absOffset;
                    }
                    absoluteOffsetMap[(JointId)i] = absOffset;
                }
            }
            return absoluteOffsetMap;
        }
    }
}
