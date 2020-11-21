using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.Sensor.BodyTracking;
using UnityEngine.UI;
using System.IO;
using NativeWebSocket;

namespace PoseTeacher
{
    // Label for various containers
    public enum PoseInputSource
    {
        KINECT, WEBSOCKET, FILE
    }

    [CLSCompliant(false)]
    public class PoseInputGetter
    {
        private PoseInputSource currentPoseInputSource;
        public PoseInputSource CurrentPoseInputSource
        {
            get { return currentPoseInputSource; }
            set 
            {
                switch (value)
                {
                    case PoseInputSource.KINECT:
                        if(!startedAK)
                            StartAzureKinect();
                        break;

                    case PoseInputSource.WEBSOCKET:
                        if (!startedWS)
                            StartWebsocket();
                        break;

                    case PoseInputSource.FILE:
                        break;
                }
                currentPoseInputSource = value; 
            }
        }

        public bool Recording { get; set; } = false;
        public PoseData CurrentPose { get; set; }

        // Azure Kinect variables
        private bool startedAK = false;
        private Device device;
        private BodyTracker tracker;

        // Used for displaying RGB Kinect video
        public Renderer videoRenderer;
        RawImage m_RawImage;
        GameObject streamCanvas;
        GameObject videoCube;

        //Select a Texture in the Inspector to change it (unused)
        public Texture m_Texture;
        Texture2D tex;

        //Websocket variables
        private bool startedWS = false;
        WebSocket websocket;
        public string WS_ip = "ws://localhost:2567";
        private PoseData poseLiveWS = null; // newest pose data from WS TODO: merge with CurrentPose

        //File variables
        IEnumerator<string> SequenceEnum;
        private string readDataPath;
        public string ReadDataPath {
            get { return readDataPath; }
            set { readDataPath = value; LoadData(); } 
        }
        public string WriteDataPath { get; set; }

        public PoseInputGetter(PoseInputSource InitPoseInputSource)
        {
            // Settig property initializes corresponding input
            CurrentPoseInputSource = InitPoseInputSource;
        }

        private async void StartWebsocket()
        {
            // Contains code to instantiate a websocket to obtain pose data
            // Web socket is currently used only for Kinect Alternative

            websocket = new WebSocket(WS_ip);

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
                // If joint information is recieved, set poseLiveWS
                var message = System.Text.Encoding.UTF8.GetString(bytes);
                Debug.Log("WS message received: " + message);
                var remote_joints = PoseDataUtils.DeserializeRJL(message);
                Debug.Log(remote_joints);
                poseLiveWS = PoseDataUtils.Remote2PoseData(remote_joints);
            };

            // Keep sending messages at every 0.3s
            // TODO: if using websocket, make this work
            //InvokeRepeating("SendWebSocketMessage", 0.0f, 0.3f);

            await websocket.Connect();
            startedWS = true;
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

        private void StartAzureKinect()
        {
            // Print to log
            Debug.Log("Try loading device");
            this.device = Device.Open(0);
            var config = new DeviceConfiguration
            {
                ColorResolution = ColorResolution.r720p,
                ColorFormat = ImageFormat.ColorBGRA32,
                DepthMode = DepthMode.NFOV_Unbinned
            };
            device.StartCameras(config);

            if (device != null)
            {
                Debug.Log("Found non-null device");
                Debug.Log(device);
            }

            var calibration = device.GetCalibration(config.DepthMode, config.ColorResolution);

            var trackerConfiguration = new TrackerConfiguration
            {
                SensorOrientation = SensorOrientation.OrientationDefault,
                CpuOnlyMode = false
            };
            this.tracker = BodyTracker.Create(calibration, trackerConfiguration);
            Debug.Log("Device loading finished");
            startedAK = true;
        }

        private void LoadData()
        {
            //loadedData = true;
            SequenceEnum = File.ReadLines(ReadDataPath).GetEnumerator();
        }

        // Appends the passed pose (PoseDataJSON format) to the file as JSON
        void AppendRecordedFrame(PoseDataJSON jdl)
        {
            string json = JsonUtility.ToJson(jdl) + Environment.NewLine;
            File.AppendAllText(WriteDataPath, json);
        }

        public void NewWriteFile()
        {
            string timestamp = DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
            WriteDataPath = "jsondata/" + timestamp + ".txt";
        }

        // reset recording file
        void ResetRecording()
        {
            File.WriteAllText(WriteDataPath, "");
            Debug.Log("Reset recording file");
        }


        public PoseData GetNextPose()
        {
            switch (CurrentPoseInputSource)
            {
                case PoseInputSource.WEBSOCKET:
                    #if !UNITY_WEBGL || UNITY_EDITOR
                    websocket.DispatchMessageQueue();
                    #endif
                    // poseLiveWS is non-null if alternative is sending pose data over websocket
                    if (poseLiveWS != null)
                    {
                        // Assign last pose from websocket
                        // TODO: maybe animate in websocket code directly upon recieveing?
                        CurrentPose = poseLiveWS;
                    }
                    else
                    {
                        Debug.Log("No pose recieved from WebSocket!");
                        
                    }
                    break;

                case PoseInputSource.FILE:
                    // TODO: Only load once
                    // Quick and dirty way to loop (by reloading file)
                    if (SequenceEnum == null || !SequenceEnum.MoveNext())
                    {
                        LoadData();
                        SequenceEnum.MoveNext();
                    }

                    string frame_json = SequenceEnum.Current;
                    PoseData fake_live_data = PoseDataUtils.JSONstring2PoseData(frame_json);
                    CurrentPose = fake_live_data;
                    break;

                case PoseInputSource.KINECT:
                    if (device != null)
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
                                        videoRenderer.material.mainTexture = tex;
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
                    break;
                    
            }
            return CurrentPose;
        }

        public async void Dispose()
        {
            if (websocket != null)
            {
                await websocket.Close();
            }
            if (tracker != null)
            {
                tracker.Dispose();
            }
            if (device != null)
            {
                device.Dispose();
            }
        }

    }
}
