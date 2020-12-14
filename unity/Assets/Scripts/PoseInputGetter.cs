using System;
using System.Collections.Generic;
using UnityEngine;
using Microsoft.Azure.Kinect.Sensor;
using Microsoft.Azure.Kinect.BodyTracking;
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

    public class PoseInputGetter
    {

        PoseInputSource CurrentPoseInputSource;
        public bool recording = false;
        public bool loop = false;
        PoseData CurrentPose { get; set; }

        // Azure Kinect variables
        Device device;
        Tracker tracker;

        // Used for displaying RGB Kinect video
        private Renderer videoRenderer;
        private RawImage m_RawImage;
        public GameObject streamCanvas;

        public GameObject VideoCube { set { if (value != null) videoRenderer = value.GetComponent<MeshRenderer>(); } }

        //Select a Texture in the Inspector to change it (unused)
        public Texture m_Texture;
        Texture2D tex;

        //Websocket variables
        WebSocket websocket;
        public string WS_ip = "ws://localhost:2567";
        private PoseData poseLiveWS = null; // newest pose data from WS TODO: merge with CurrentPose

        //File variables
        IEnumerator<string> SequenceEnum;
        private string _ReadDataPath;
        public string ReadDataPath {
            get { return _ReadDataPath; }
            set { _ReadDataPath = value; GetTotalPoseNumber(); LoadData(); }
        }

        private int _TotalFilePoseNumber;
        public int TotalFilePoseNumber
        {
            get { return _TotalFilePoseNumber; }
        }
        private int _CurrentFilePoseNumber;
        public int CurrentFilePoseNumber
        {
            get { return _CurrentFilePoseNumber; }
        }

        public string WriteDataPath { get; set; }

        public PoseInputGetter(PoseInputSource InitPoseInputSource)
        {
            CurrentPoseInputSource = InitPoseInputSource;

            switch (CurrentPoseInputSource)
            {
                case PoseInputSource.KINECT:
                    StartAzureKinect();
                    break;

                case PoseInputSource.WEBSOCKET:
                    StartWebsocket();
                    break;

                case PoseInputSource.FILE:
                    break;
            }
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

        private void GetTotalPoseNumber()
        {
            LoadData();
            int count = 0;
            while (SequenceEnum.MoveNext())
            {
                count++;
            }
            _TotalFilePoseNumber = count;
            
        }

        private void LoadData()
        {
            SequenceEnum = File.ReadLines(ReadDataPath).GetEnumerator();
            _CurrentFilePoseNumber = 0;
        }

        public void RestartFile()
        {
            LoadData();
        }

        // Appends the passed pose (PoseDataJSON format) to the file as JSON
        void AppendRecordedFrame(PoseDataJSON jdl)
        {
            string json = JsonUtility.ToJson(jdl) + Environment.NewLine;
            File.AppendAllText(WriteDataPath, json);
        }

        // Generate new filename with timestamp and set as file to write to
        public void GenNewFilename()
        {
            string timestamp = DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss");
            WriteDataPath = "jsondata/" + timestamp + ".txt";
        }

        // reset recording file
        public void ResetRecording()
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

                    _CurrentFilePoseNumber++;
                    // TODO: Only load once
                    // Quick and dirty way to loop (by reloading file)
                    if (SequenceEnum == null || !SequenceEnum.MoveNext())
                    {
                        if (!loop)
                            break;
                        LoadData();
                        SequenceEnum.MoveNext();
                        _CurrentFilePoseNumber = 1;
                    }

                                        string frame_json = SequenceEnum.Current;
                    PoseData fake_live_data = PoseDataUtils.JSONstring2PoseData(frame_json);
                    CurrentPose = fake_live_data;

                    if (recording) // recording
                    {
                        File.AppendAllText(WriteDataPath, frame_json + Environment.NewLine);
                    }
                    break;

                case PoseInputSource.KINECT:
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
                                UnityEngine.Object.Destroy(tex);// required to not keep old images in memory
                                tex = new Texture2D(color.WidthPixels, color.HeightPixels, TextureFormat.BGRA32, false);
                                tex.LoadRawTextureData(color.Memory.ToArray());
                                tex.Apply();

                                //Fetch the RawImage component from the GameObject
                                if (tex != null)
                                {
                                    if (streamCanvas != null)
                                    {
                                        m_RawImage = streamCanvas.GetComponent<RawImage>();
                                        m_RawImage.texture = tex;
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

                            //Debug.LogFormat("{0} bodies found.", frame.NumberOfBodies);

                            //  At least one body found by Body Tracking
                            if (frame.NumberOfBodies > 0)
                            {
                                // Use first estimated person, if mutiple are in the image
                                // !!! There are (probably) no guarantees on consisitent ordering between estimates
                                //var bodies = frame.Bodies;
                                var body = frame.GetBody(0);

                                // Apply pose to user avatar(s)
                                PoseData live_data = PoseDataUtils.Body2PoseData(body);

                                if (recording) // recording
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
