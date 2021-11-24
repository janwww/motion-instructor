using UnityEngine;
using Microsoft.Azure.Kinect.BodyTracking;
using Microsoft.Azure.Kinect.Sensor;
using UnityEngine.UI;
using System.IO;
using System;

namespace PoseTeacher
{
    public class KinectPoseGetter : PoseGetter
    {
        private bool recording;
        public bool Recording {
            get
            {
                return recording;
            }
            set {
                if (value) StartRecording();
                recording = value;
            }
        }

        string WriteDataPath;
        // Azure Kinect variables
        Device device;
        Tracker tracker;

        // Used for displaying RGB Kinect video
        private Renderer videoRenderer;
        public GameObject streamCanvas;
        Texture2D tex;

        public GameObject VideoCube { set { if (value != null) videoRenderer = value.GetComponent<MeshRenderer>(); } }

        public KinectPoseGetter()
        {
            StartAzureKinect();
        }

        public override PoseData GetNextPose()
        {
            if (device != null)
            {
                using (Capture capture = device.GetCapture())
                {
                    // Make tracker estimate body
                    tracker.EnqueueCapture(capture);

                    // Code for getting RGB image from camera
                    Microsoft.Azure.Kinect.Sensor.Image color = capture.Color;
                    if (color != null && color.WidthPixels > 0 && (streamCanvas != null || videoRenderer != null))
                    {
                        UnityEngine.Object.Destroy(tex);
                        tex = new Texture2D(color.WidthPixels, color.HeightPixels, TextureFormat.BGRA32, false);
                        tex.LoadRawTextureData(color.Memory.ToArray());
                        tex.Apply();

                        //Fetch the RawImage component from the GameObject
                        if (tex != null)
                        {
                            if (streamCanvas != null)
                            {
                                streamCanvas.GetComponent<RawImage>().texture = tex;
                            }
                            if (videoRenderer != null)
                            {
                                videoRenderer.material.mainTexture = tex;
                            }
                        }
                    }

                }

                // Get pose estimate from tracker
                using (Frame frame = tracker.PopResult())
                {
                    //  At least one body found by Body Tracking
                    if (frame.NumberOfBodies > 0)
                    {
                        // Use first estimated person, if mutiple are in the image
                        // !!! There are (probably) no guarantees on consisitent ordering between estimates
                        //var bodies = frame.Bodies;
                        var body = frame.GetBody(0);

                        // Apply pose to user avatar(s)
                        PoseData live_data = PoseDataUtils.Body2PoseData(body);

                        if (Recording) // recording
                        {
                            PoseDataJSON jdl = PoseDataUtils.Body2PoseDataJSON(body);
                            AppendRecordedFrame(jdl);
                        }
                        CurrentPose = live_data;
                    }
                }
            }
            else
            {
                Debug.Log("device is null!");
            }
            return CurrentPose;
        }
        public override void Dispose()
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

        void StartAzureKinect()
        {
            device = Device.Open(0);

            var config = new DeviceConfiguration
            {
                CameraFPS = FPS.FPS30,
                ColorResolution = ColorResolution.R720p,
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_Unbinned,
                WiredSyncMode = WiredSyncMode.Standalone,
            };
            device.StartCameras(config);
            Debug.Log("Open K4A device successful. sn:" + device.SerialNum);

            //var calibration = device.GetCalibration(config.DepthMode, config.ColorResolution);
            var calibration = device.GetCalibration();

            var trackerConfiguration = new TrackerConfiguration
            {
                ProcessingMode = TrackerProcessingMode.Gpu,
                SensorOrientation = SensorOrientation.Default
            };

            this.tracker = Tracker.Create(calibration, trackerConfiguration);
            Debug.Log("Body tracker created.");
        }

        // Appends the passed pose (PoseDataJSON format) to the file as JSON
        void AppendRecordedFrame(PoseDataJSON jdl)
        {
            string json = JsonUtility.ToJson(jdl) + Environment.NewLine;
            File.AppendAllText(WriteDataPath, json);
        }

        // reset recording file
        public void ResetRecording()
        {
            File.WriteAllText(WriteDataPath, "");
            Debug.Log("Reset recording file");
        }

        void StartRecording()
        {
            string timestamp = DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
            WriteDataPath = "jsondata/" + timestamp + ".txt";
        }

    }
}

