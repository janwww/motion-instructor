using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Azure.Kinect.BodyTracking;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using PoseTeacher;

[CreateAssetMenu(fileName = "Dance", menuName = "ScriptableObjects/DanceDataScriptableObject", order = 2)]
public class DanceDataScriptableObject : ScriptableObject {
    public DanceData DanceDataUncompressed;
    public byte[] DanceDataCompressed;

#if UNITY_EDITOR
    public static void SaveDanceDataToScriptableObject(DanceData danceData, string name, bool compressed) {
        DanceDataScriptableObject danceDataScriptableObject = DanceDataScriptableObject.CreateInstance<DanceDataScriptableObject>();
        if (compressed) {
            byte[] byteDanceData = null;

            // Convert DanceData to serializable cheapDancedata
            cheapDanceData cheapDanceData = new cheapDanceData();
            cheapDanceData.cheapPoses = new List<cheapDancePose>();
            foreach (var pose in danceData.poses) {
                cheapDancePose cheapDancePose = new cheapDancePose();
                cheapDancePose.positions = new float[(int)JointId.Count * 3];
                cheapDancePose.orientations = new float[(int)JointId.Count * 4];
                cheapDancePose.timestamp = pose.timestamp;

                for (int i = 0; i < (int)JointId.Count; i++) {
                    Vector3 currPos = pose.positions[i];
                    Quaternion currQuat = pose.orientations[i];
                    cheapDancePose.positions[i * 3 + 0] = currPos.x;
                    cheapDancePose.positions[i * 3 + 1] = currPos.y;
                    cheapDancePose.positions[i * 3 + 2] = currPos.z;
                    cheapDancePose.orientations[i * 4 + 0] = currQuat.x;
                    cheapDancePose.orientations[i * 4 + 1] = currQuat.y;
                    cheapDancePose.orientations[i * 4 + 2] = currQuat.z;
                    cheapDancePose.orientations[i * 4 + 3] = currQuat.w;
                }
                cheapDanceData.cheapPoses.Add(cheapDancePose);
            }

            using (System.IO.MemoryStream ms = new MemoryStream()) {
                var bf = new BinaryFormatter();
                bf.Serialize(ms, cheapDanceData);
                byteDanceData = ms.ToArray();
            }
            danceDataScriptableObject.DanceDataCompressed = CLZF2.Compress(byteDanceData);
        } else {
            danceDataScriptableObject.DanceDataUncompressed = danceData;
        }
        AssetDatabase.CreateAsset(danceDataScriptableObject, "Assets/Dances/" + name + ".asset");
        AssetDatabase.SaveAssets();
        Debug.Log("Saved Dance to " + "Assets/Dances/" + name + ".asset");
    }
#endif

    public DanceData LoadDanceDataFromScriptableObject() {
        if (DanceDataCompressed != null && DanceDataCompressed.Length > 0) {
            cheapDanceData cheapDanceData;
            byte[] byteDanceData = CLZF2.Decompress(DanceDataCompressed);
            using (MemoryStream ms = new MemoryStream(byteDanceData)) {
                var bf = new BinaryFormatter();
                cheapDanceData = (cheapDanceData)bf.Deserialize(ms);
            }

            // Convert cheapDanceData to Dancedata
            DanceData danceData = new DanceData();

            foreach (var pose in cheapDanceData.cheapPoses) {
                DancePose dancePose = new DancePose();
                dancePose.timestamp = pose.timestamp;
                for (int i = 0; i < (int)JointId.Count; i++) {
                    dancePose.positions[i] = new Vector3(pose.positions[i * 3 + 0], pose.positions[i * 3 + 1], pose.positions[i * 3 + 2]);
                    dancePose.orientations[i] = new Quaternion(pose.orientations[i * 4 + 0], pose.orientations[i * 4 + 1], pose.orientations[i * 4 + 2], pose.orientations[i * 4 + 3]);
                }
                danceData.poses.Add(dancePose);
            }

            return danceData;
        } else {
            return DanceDataUncompressed;
        }
    }

    [System.Serializable]
    public struct cheapDanceData {
        public List<cheapDancePose> cheapPoses;
    }

    [System.Serializable]
    public struct cheapDancePose {
        public float[] positions;
        public float[] orientations;
        public float timestamp;
    }
}