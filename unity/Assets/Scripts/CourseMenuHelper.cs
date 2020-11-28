using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;

using Microsoft.MixedReality.Toolkit.UI;

namespace PoseTeacher
{
    [Serializable]
    public class StepContainer
    {
        public int Position;
        public string Name;
        public string Description;
        public string FileLocation;
    }
    [Serializable]
    public class StepsJSON
    {
        public StepContainer[] steps;
        public CoreographieContainer[] coreographies;
    }
    [Serializable]
    public class CoreographieContainer
    {
        public int Position;
        public string Name;
        public string Description;
        public int HighScore;
        public string FileLocation;
    }
    public class CoreographiesJSON
    {
        public CoreographieContainer[] coreographies;
    }


    [CLSCompliant(false)]
    public class CourseMenuHelper : MonoBehaviour
    {
        public GameObject MainObject;
        public GameObject MenuObject;
        public GameObject buttonPrefab;
        public GameObject CourseDetails;
        public GameObject CourseButtonCollection;
        public GameObject CourseMenuHelperObject;
        public GameObject CourseMenuHolder;
        public GameObject TrainingHolder;
        public GameObject CoreographyHolder;

        private Dictionary<string, string> courseToPath =
            new Dictionary<string, string>()
            {
                { "salsa", "jsondata/courses/salsa.txt" }
            };

        private Dictionary<int, StepContainer> steps = new Dictionary<int, StepContainer>();
        private Dictionary<int, CoreographieContainer> coreographies = new Dictionary<int, CoreographieContainer>();

        public void LoadCourse(string courseName)
        {
            ClearState();

            string jsonPath;
            bool courseFound = courseToPath.TryGetValue(courseName, out jsonPath);
            if (!courseFound)
            {
                Debug.Log("Course not found: " + courseName + ". Check that the Course Button, CourseMenuHelper.courseToPath and JSON filepath are sync.");
                return;
            }

            string json_data = File.ReadAllText(jsonPath);
            Debug.Log(json_data);
            StepsJSON course_steps = JsonUtility.FromJson<StepsJSON>(json_data);
            Debug.Log(course_steps.ToString());
            foreach (StepContainer step in course_steps.steps)
            {
                steps.Add((step.Position), step);
            }
            foreach (CoreographieContainer coreographie in course_steps.coreographies)
            {
                coreographies.Add(coreographie.Position, coreographie);
            }

            GenerateCourseMenu();
        }

        private void GenerateCourseMenu()
        {
            for(int i = 0; i < steps.Count + coreographies.Count; i++)
            {
                GameObject newButton = Instantiate(buttonPrefab);

                newButton.transform.localScale += new Vector3(2, 2, 0);

                newButton.name = "Button" + i;
                var buttonConfigHelper = newButton.GetComponent<ButtonConfigHelper>();
                int pos;
                string movementLocation;
                if (steps.ContainsKey(i))
                {
                    buttonConfigHelper.MainLabelText = steps[i].Name;
                    pos = steps[i].Position;
                    movementLocation = steps[i].FileLocation;
                }
                else if (coreographies.ContainsKey(i))
                {
                    buttonConfigHelper.MainLabelText = "<color=\"yellow\">" + coreographies[i].Name + "</color>";
                    pos = coreographies[i].Position;
                    movementLocation = coreographies[i].FileLocation;
                }
                else
                {
                    Debug.Log("Step/coreography not found.");
                    return;
                }
                

                var onClickReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnClickReceiver>();
                var onFocusReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnFocusReceiver>();
                
                onFocusReciever.OnFocusOn.AddListener(() => CourseDetails.SetActive(true));
                onFocusReciever.OnFocusOn.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().SetDetails(pos));
                onFocusReciever.OnFocusOn.AddListener(() => MainObject.GetComponent<PoseteacherMain>().loadRecording(movementLocation));
                onFocusReciever.OnFocusOn.AddListener(() => MainObject.GetComponent<PoseteacherMain>().fake_file = movementLocation);
                onFocusReciever.OnFocusOff.AddListener(() => CourseDetails.SetActive(false));
                
                if (steps.ContainsKey(i))
                {
                    onClickReciever.OnClicked.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().StartStep());
                } 
                else if (coreographies.ContainsKey(i))
                {
                    onClickReciever.OnClicked.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().StartCoreography());
                }
                
                // Vector3 pos = newButton.transform.position;
                // Vector3 pos2 = CourseButtonCollection.transform.position;
                newButton.transform.SetParent(CourseButtonCollection.transform);
            }




                Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection objCollectionComponent = 
                CourseButtonCollection.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();

            objCollectionComponent.UpdateCollection();
        }

        private void ClearState()
        {
            steps.Clear();
            coreographies.Clear();

            for (int i = CourseButtonCollection.transform.childCount - 1; i>=0; i--)
            {
                GameObject.DestroyImmediate(CourseButtonCollection.transform.GetChild(i).gameObject);
            }
        }

        public void SetDetails(int position)
        {
            GameObject CourseDetails = GameObject.Find("CourseDetails");
            GameObject CourseDescription = GameObject.Find("CourseDescription");
            UnityEngine.UI.Text DescriptionText = CourseDescription.GetComponent<UnityEngine.UI.Text>();

            if (steps.ContainsKey(position))
            {
                DescriptionText.text = steps[position].Description;
            } else if (coreographies.ContainsKey(position))
            {
                DescriptionText.text = coreographies[position].Description;
            } else
            {
                Debug.Log("Step/coreography not found: " + position);
            }
            

        }

        public void StartStep()
        {
            MenuObject.SetActive(false);
            TrainingHolder.SetActive(true);
        }

        public void StartCoreography()
        {
            MenuObject.SetActive(false);
            CoreographyHolder.SetActive(true);
        }
    }
}