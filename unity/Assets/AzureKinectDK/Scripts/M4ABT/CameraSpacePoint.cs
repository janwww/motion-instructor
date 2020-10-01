using System;
using System.Runtime.InteropServices;

namespace Microsoft.Azure.Kinect.Sensor.BodyTracking {

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraSpacePoint {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }
}
