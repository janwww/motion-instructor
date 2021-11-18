using PoseTeacher;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class DanceEditor : MonoBehaviour {

    public Slider slider;
    public DancePerformanceScriptableObject DancePerformanceObject;
    public GameObject PoseIndicatorContainer;
    public GameObject PoseIndicatorPrefab;

    public GameObject avatarDisplayObject;
    //public GameObject goalDisplayObject;

    public float poseGoalTestTime;

    public bool insertGoalPose = false;

    private DanceData danceData;
    private AudioClip song;

    private AudioSource audioSource;

    private IAvatarDisplay avatarDisplay;
    //private IGoalDisplay goalDisplay;

    private int currentId = 0;
    private int currentGoal = 0;

    // Start is called before the first frame update
    void Start() {
        audioSource = GetComponent<AudioSource>();
        song = DancePerformanceObject.SongObject.SongClip;
        audioSource.clip = song;

        avatarDisplay = avatarDisplayObject.GetComponent<IAvatarDisplay>();
        //goalDisplay = goalDisplayObject.GetComponent<IGoalDisplay>();

        danceData = DancePerformanceObject.danceData.LoadDanceDataFromScriptableObject();
        foreach (var pose in danceData.poses) {
            GameObject indicator = Instantiate(PoseIndicatorPrefab, PoseIndicatorContainer.transform);
            RectTransform rt = indicator.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(pose.timestamp / song.length, 0, 0);

        }

        foreach (var poseTest in DancePerformanceObject.goals) {
            if(poseTest.GetGoalType() == GoalType.POSE) {
                DanceGoalPose goalPose = (DanceGoalPose)poseTest;
                DancePose pose = danceData.poses[goalPose.id];
                GameObject indicator = Instantiate(PoseIndicatorPrefab, PoseIndicatorContainer.transform);
                indicator.GetComponent<Image>().color = Color.white;
                RectTransform rt = indicator.GetComponent<RectTransform>();
                rt.localPosition = new Vector3(pose.timestamp / song.length, 20f, 0);
            } else if (poseTest.GetGoalType() == GoalType.MOTION) {
                DanceGoalMotion goalPose = (DanceGoalMotion)poseTest;

                DancePose pose = danceData.poses[goalPose.startId];
                GameObject indicator = Instantiate(PoseIndicatorPrefab, PoseIndicatorContainer.transform);
                indicator.GetComponent<Image>().color = Color.grey;
                RectTransform rt = indicator.GetComponent<RectTransform>();
                rt.localPosition = new Vector3(pose.timestamp / song.length, 20f, 0);

                pose = danceData.poses[goalPose.endId];
                indicator = Instantiate(PoseIndicatorPrefab, PoseIndicatorContainer.transform);
                indicator.GetComponent<Image>().color = Color.grey;
                rt = indicator.GetComponent<RectTransform>();
                rt.localPosition = new Vector3(pose.timestamp / song.length, 20f, 0);
            }
        }

        audioSource.Play();
    }

    // Update is called once per frame
    void Update() {
        // Todo pretty basic, we want to improve from this
        if (insertGoalPose) {
            insertGoalPose = false;
            DancePerformanceObject.danceGoalPoses.Add(new DanceGoalPose() {id = currentId});

            GameObject indicator = Instantiate(PoseIndicatorPrefab, PoseIndicatorContainer.transform);
            indicator.GetComponent<Image>().color = Color.white;
            RectTransform rt = indicator.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(danceData.poses[currentId].timestamp / song.length, 20f, 0);

            // sort goals by starting ID
            DancePerformanceObject.danceGoalPoses.Sort();

#if UNITY_EDITOR
            // need to mark as dirty to flush changes from scripableObject to disk
            EditorUtility.SetDirty(DancePerformanceObject);
#endif
        }

        slider.value = audioSource.time / song.length;
        float timeOffset = audioSource.time - danceData.poses[currentId].timestamp;

        avatarDisplay.SetPose(danceData.GetInterpolatedPose(currentId, out currentId, timeOffset));


        // find current goal and display it
        List<IDanceGoal> goals = DancePerformanceObject.goals;
        while (currentGoal < goals.Count) {
            IDanceGoal goal = goals[currentGoal];
            if (goal.GetGoalType() == GoalType.POSE) {
                DanceGoalPose goalPose = (DanceGoalPose)goal;
                if (audioSource.time > danceData.poses[goalPose.id].timestamp + poseGoalTestTime) {
                    currentGoal++;
                } else {
                    break; // the loop
                }
            } else if (goal.GetGoalType() == GoalType.MOTION) {
                DanceGoalMotion goalMotion = (DanceGoalMotion)goal;
                if (audioSource.time > danceData.poses[goalMotion.endId].timestamp) {
                    currentGoal++;
                } else {
                    break; // the loop
                }
            }
        }

        if (currentGoal < goals.Count) {
            //goalDisplay.showGoal(danceData, goals[currentGoal], audioSource.time);
        } else {
            //goalDisplay.showNothing();
        }

    }

    public void SliderChanged() {
        // also gets called when chaning via script, sight, so only do something if
        float time = slider.value;
        float songTime = audioSource.time / song.length;
        if (time < songTime + 0.01 && time > songTime - 0.01) {
            return;
        }
        Debug.Log("Reset");
        audioSource.time = time * song.length;
        currentId = 0;
        currentGoal = 0;
    }

    public void ChangePitch(float pitch) {
        audioSource.pitch = pitch;
    }
}
