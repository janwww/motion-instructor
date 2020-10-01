using System;
using System.Runtime.InteropServices;


namespace Microsoft.Azure.Kinect.Sensor.BodyTracking {

    [StructLayout(LayoutKind.Sequential)]
    public struct JointOrientation {
        public float W { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
