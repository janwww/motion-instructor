// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace Microsoft.Azure.Kinect.Sensor
{
    [Native.NativeReference("k4abt_sensor_orientation_t")]
    public enum SensorOrientation
    {
        [Native.NativeReference("K4ABT_SENSOR_ORIENTATION_DEFAULT")]
        /// <summary>
        /// Mount the sensor at its default orientation.
        /// </summary>
        OrientationDefault = 0,

        [Native.NativeReference("K4ABT_SENSOR_ORIENTATION_CLOCKWISE90")]
        /// <summary>
        /// Clockwisely rotate the sensor 90 degree.
        /// </summary>
        OrientationClockwise90,

        [Native.NativeReference("K4ABT_SENSOR_ORIENTATION_COUNTERCLOCKWISE90")]
        /// <summary>
        /// Counter-clockwisely rotate the sensor 90 degrees.
        /// </summary>
        OrientationCounterClockwise90,

        [Native.NativeReference("K4ABT_SENSOR_ORIENTATION_FLIP180")]
        /// <summary>
        /// Mount the sensor upside-down.
        /// </summary>
        OrientationFlip180,

    }
}
