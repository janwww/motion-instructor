// Copyright (c) Takahiro Horikawa. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Microsoft.Azure.Kinect.Sensor.BodyTracking
{

    public struct Body {

        public UInt32 TrackingId;

        public Skeleton Skeleton;
    }

    [StructLayout(LayoutKind.Sequential)]
    [Native.NativeReference("k4abt_skeleton_t")]
    public struct Skeleton
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.Struct, SizeConst = (int)JointType.Count)]
        public Joint[] Joints;
    }


    [Native.NativeReference("k4abt_joint_confidence_level_t")]
    public enum JointConfidenceLevel
    {
        [Native.NativeReference("K4ABT_JOINT_CONFIDENCE_NONE")]
        /// <summary>
        /// The joint is out of range (too far from depth camera)
        /// </summary>
        None = 0,

        [Native.NativeReference("K4ABT_JOINT_CONFIDENCE_LOW")]
        /// <summary>
        /// The joint is not observed (likely due to occlusion), predicted joint pose.
        /// </summary>
        Low,

        [Native.NativeReference("K4ABT_JOINT_CONFIDENCE_MEDIUM")]
        /// <summary>
        /// Medium confidence in joint pose.
        /// 
        /// Current SDK will only provide joints up to this confidence level
        /// </summary>
        Medium,

        /// <summary>
        /// High confidence in joint pose.
        /// 
        /// Placeholder for future SDK
        /// </summary>
        High,

        /// <summary>
        /// The total number of confidence levels.
        /// </summary>
        Count
    }

    [StructLayout(LayoutKind.Sequential)]
    [Native.NativeReference("k4abt_joint_t")]
    public struct Joint
    {
        public CameraSpacePoint Position;

        public JointOrientation Orientation;

        public JointConfidenceLevel confidence_level;
    }
}
