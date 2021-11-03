using PoseTeacher;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DanceEditor : MonoBehaviour {

    public Slider slider;
    public DancePerformanceScriptableObject DancePerformanceObject;
    public GameObject PoseIndicatorContainer;
    public GameObject PoseIndicatorPrefab;
    public GameObject AvatarContainerObject;

    private DanceData danceData;
    private AudioClip song;
    private AvatarContainer avatar;

    private AudioSource audioSource;

    private int currentId = 0;

    // Start is called before the first frame update
    void Start() {
        audioSource = GetComponent<AudioSource>();
        song = DancePerformanceObject.SongObject.SongClip;
        audioSource.clip = song;

        danceData = DancePerformanceObject.danceData.LoadDanceDataFromScriptableObject();
        foreach (var pose in danceData.poses) {
            GameObject indicator = Instantiate(PoseIndicatorPrefab, PoseIndicatorContainer.transform);
            RectTransform rt = indicator.GetComponent<RectTransform>();
            rt.localPosition = new Vector3(pose.timestamp / song.length, 0, 0);

        }

        avatar = new AvatarContainer(AvatarContainerObject);
        avatar.ChangeActiveType(AvatarType.ROBOT);

        audioSource.Play();
    }

    // Update is called once per frame
    void Update() {
        slider.value = audioSource.time / song.length;
        float timeOffset = audioSource.time - danceData.poses[currentId].timestamp;
        avatar.MovePerson(danceData.GetInterpolatedPose(currentId, out currentId, timeOffset).toPoseData());
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
    }

    public void ChangePitch(float pitch) {
        audioSource.pitch = pitch;
    }
}
