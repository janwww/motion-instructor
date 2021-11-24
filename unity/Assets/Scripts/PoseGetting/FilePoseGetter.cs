using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace PoseTeacher
{
    public class FilePoseGetter : PoseGetter
    {
        bool loop = false;

        IEnumerator<string> SequenceEnum;
        private string _ReadDataPath;
        public string ReadDataPath
        {
            get { return _ReadDataPath; }
            set { _ReadDataPath = value; LoadData(); }
        }

        public FilePoseGetter(bool _loop)
        {
            loop = _loop;
        }

        public override PoseData GetNextPose()
        {
            if (!SequenceEnum.MoveNext())
            {
                // Quick and dirty way to loop (by reloading file)
                if (SequenceEnum == null || loop)
                {
                    LoadData();
                    SequenceEnum.MoveNext();
                }
            }

            string frame_json = SequenceEnum.Current;
            PoseData fake_live_data = PoseDataUtils.JSONstring2PoseData(frame_json);
            CurrentPose = fake_live_data;
            return CurrentPose;
        }

        public override void Dispose(){}

        public void RestartFile()
        {
            LoadData();
        }

        void LoadData()
        {
            SequenceEnum = File.ReadLines(ReadDataPath).GetEnumerator();
        }
    }
}

