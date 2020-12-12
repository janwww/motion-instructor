using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;

using Microsoft.MixedReality.Toolkit.UI;

namespace PoseTeacher
{
    [Serializable]
    public class BlockContainer
    {
        public string FileLocation;
    }
        [Serializable]
    public class StepContainer
    {
        public int Position;
        public string Name;
        public string Description;
        public BlockContainer[] Blocks;
    }
    [Serializable]
    public class CourseJSON
    {
        public int CourseID;
        public string CourseTitle;
        public string CourseShortDescription;
        public string CourseDescription;
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

    public class CourseInfoHolder
    {
        public int CourseID { get; set; }
        public string CourseTitle { get; set; }
        public string CourseShortDescription { get; set; }
        public string CourseDescription { get; set; }
    }

    public class CoursesHolder
    {
        public List<CourseInfoHolder> Courses = new List<CourseInfoHolder>();
        public int CurrentCourse = 0;
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
        public GameObject CourseDescription;

        private Dictionary<string, string> courseToPath =
            new Dictionary<string, string>()
            {
                { "salsa", "jsondata/courses/salsa_male.txt" }
            };

        private Dictionary<int, StepContainer> steps = new Dictionary<int, StepContainer>();
        private Dictionary<int, CoreographieContainer> coreographies = new Dictionary<int, CoreographieContainer>();

        private CoursesHolder courses = new CoursesHolder();

        public void LoadAllCourseInfos()
        {
            foreach (KeyValuePair<string, string> entry in courseToPath)
            {
                string json_data = File.ReadAllText(entry.Value);
                CourseJSON course_steps = JsonUtility.FromJson<CourseJSON>(json_data);
                CourseInfoHolder course = new CourseInfoHolder();
                course.CourseID = course_steps.CourseID;
                course.CourseTitle = course_steps.CourseTitle;
                course.CourseShortDescription = course_steps.CourseShortDescription;
                course.CourseDescription = course_steps.CourseDescription;
                courses.Courses.Add(course);
            }
        }

        public void GenerateCoursesMenu(GameObject coursesMenuButtonCollection)
        {
            foreach (CourseInfoHolder info in courses.Courses)
            {
                GameObject newButton = Instantiate(buttonPrefab);
                newButton.transform.localScale += new Vector3(2, 2, 0);
                newButton.name = info.CourseTitle;
                var buttonConfigHelper = newButton.GetComponent<ButtonConfigHelper>();

                buttonConfigHelper.MainLabelText = info.CourseTitle;

                var onFocusReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnFocusReceiver>();
                if (onFocusReciever == null)
                {
                    onFocusReciever = newButton.GetComponent<Interactable>().AddReceiver<InteractableOnFocusReceiver>();
                }
                onFocusReciever.OnFocusOn.AddListener(() => CourseDetails.SetActive(true));
                onFocusReciever.OnFocusOn.AddListener(() => SetCourseDetails(info.CourseID));
                onFocusReciever.OnFocusOff.AddListener(() => CourseDetails.SetActive(false));

                var onClickReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>();
                if (onClickReciever == null)
                {
                    onClickReciever = newButton.GetComponent<Interactable>().AddReceiver<InteractableOnPressReceiver>();
                }
                onClickReciever.OnPress.AddListener(() => MenuObject.GetComponent<MenuHelper>().SelectedMenuOption(info.CourseID));

                newButton.SetActive(true);
                newButton.transform.SetParent(coursesMenuButtonCollection.transform);
            }

            Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection objCollectionComponent =
                coursesMenuButtonCollection.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();

            objCollectionComponent.UpdateCollection();
        }

        public CourseInfoHolder GetCourseInfo(int courseID)
        {
            return courses.Courses[courseID];
        }

        public CourseInfoHolder GetCurrentCourseInfo()
        {
            return courses.Courses[courses.CurrentCourse];
        }

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
            CourseJSON course_steps = JsonUtility.FromJson<CourseJSON>(json_data);
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
                    movementLocation = steps[i].Blocks[0].FileLocation;
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

                var onClickReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>();
                var onFocusReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnFocusReceiver>();
                if (onFocusReciever == null)
                {
                    onFocusReciever = newButton.GetComponent<Interactable>().AddReceiver<InteractableOnFocusReceiver>();
                }
                if (onClickReciever == null)
                {
                    onClickReciever = newButton.GetComponent<Interactable>().AddReceiver<InteractableOnPressReceiver>();
                }
                onFocusReciever.OnFocusOn.AddListener(() => CourseDetails.SetActive(true));
                onFocusReciever.OnFocusOn.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().SetMoveDetails(pos));
                onFocusReciever.OnFocusOn.AddListener(() => MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(movementLocation));
                //onFocusReciever.OnFocusOn.AddListener(() => MainObject.GetComponent<PoseteacherMain>().loadRecording(movementLocation));
                //onFocusReciever.OnFocusOn.AddListener(() => MainObject.GetComponent<PoseteacherMain>().fake_file = movementLocation);
                onFocusReciever.OnFocusOff.AddListener(() => CourseDetails.SetActive(false));
                
                if (steps.ContainsKey(i))
                {
                    onClickReciever.OnPress.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().StartStep());
                } 
                else if (coreographies.ContainsKey(i))
                {
                    onClickReciever.OnPress.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().StartCoreography());
                }
                onClickReciever.OnPress.AddListener(() => CourseDetails.SetActive(false));

                newButton.SetActive(true);
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

        public void SetCourseDetails(int courseID)
        {
            
            //GameObject CourseDescription = GameObject.Find("CourseDescription");
           // GameObject CourseDescription = CourseDetails.transform.Find("DescriptionPanel").Find("Panel1").Find("TextContext").Find("CourseDescription").gameObject;
            UnityEngine.UI.Text DescriptionText = CourseDescription.GetComponent<UnityEngine.UI.Text>();
            CourseInfoHolder info = courses.Courses[courseID]; 
            Debug.Log("<size=30>" + info.CourseTitle + "</size>\n<size=20>" + info.CourseDescription + "</size>");
            DescriptionText.text = "<size=30>" + info.CourseTitle + "</size>\n<size=20>" + info.CourseDescription + "</size>";
        }

        public void SetMoveDetails(int position)
        {
            //GameObject CourseDetails = GameObject.Find("CourseDetails");
            //GameObject CourseDescription = GameObject.Find("CourseDescription"); // This is the text-holding object of CourseDetails
           // GameObject CourseDescription = CourseDetails.transform.Find("DescriptionPanel").Find("Panel1").Find("TextContext").Find("CourseDescription").gameObject;

            UnityEngine.UI.Text DescriptionText = CourseDescription.GetComponent<UnityEngine.UI.Text>();

            if (steps.ContainsKey(position))
            {
                DescriptionText.text = "<size=30>" + steps[position].Name + "</size>\n" + steps[position].Description;
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
            PoseteacherMain main = MainObject.GetComponent<PoseteacherMain>();
            main.ShowTeacher();
            main.isChoreography = false;
            main.ActivateIndicators();
        }

        public void StartCoreography()
        {
            MenuObject.SetActive(false);
            CoreographyHolder.SetActive(true);
            PoseteacherMain main = MainObject.GetComponent<PoseteacherMain>();
            main.ShowTeacher();
            main.isChoreography = true;
            main.ActivateIndicators();
        }
    }
}