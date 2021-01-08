using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;
using System;

using Microsoft.MixedReality.Toolkit.UI;

namespace PoseTeacher
{
    // contains a single Block of a Step
    [Serializable]
    public class BlockContainer
    {
        public string FileLocation;
    }

    // contains a Step of a Course
    [Serializable]
    public class StepContainer
    {
        public int Position; // step position in the course
        public string Name;
        public string Description;
        public BlockContainer[] Blocks; // a step could contain multiple parts (blocks)
    }

    // contains a Course
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
    // contains a Coreography of a Course
    [Serializable]
    public class CoreographieContainer
    {
        public int Position;
        public string Name;
        public string Description;
        public int HighScore;
        public string FileLocation;
    }

    // Helper class to contain Course information without the steps
    public class CourseInfoHolder
    {
        public int CourseID { get; set; }
        public string CourseTitle { get; set; }
        public string CourseShortDescription { get; set; }
        public string CourseDescription { get; set; }
    }

    // Helper class to contain all information of all Courses without the steps
    public class CoursesHolder
    {
        public List<CourseInfoHolder> Courses = new List<CourseInfoHolder>();
        public int CurrentCourse = 0;
    }

    public class CourseMenuHelper : MonoBehaviour
    {
        public GameObject MainObject; // Main GameObject
        public GameObject MenuObject; // Menu new GameObject
        public GameObject buttonPrefab; // Button GameObject that will be instantiated in the course menus
        public GameObject CourseDetails; // CourseDetails gameObject in Menu new GO
        public GameObject CourseButtonCollection; // Button Collection GameObject for specific courses
        public GameObject CourseMenuHelperObject; // GameObject that this script is assigned to
        public GameObject CourseMenuHolder; // GameObject that contains the specific course Button Collection (can be deleted prob)
        public GameObject TrainingHolder; // TrainingElements GamObject
        public GameObject CoreographyHolder; // CoreograpyElements GameObject
        public GameObject CourseDescription; // The GameObject that containe the Text of Course Details

        // The list of courses. This has to be updated when a new course is added.
        private Dictionary<string, string> courseToPath =
            new Dictionary<string, string>()
            {
                { "salsa", "jsondata/courses/salsa_male.txt" },
                { "salsaf", "jsondata/courses/salsa_female.txt" }
            };

        private Dictionary<int, StepContainer> steps = new Dictionary<int, StepContainer>();
        private Dictionary<int, CoreographieContainer> coreographies = new Dictionary<int, CoreographieContainer>();

        private CoursesHolder courses = new CoursesHolder();

        // fields to store current state of the course
        private int CurrentStepOrCoreo = 0;
        private int CurrentBlock = 0;
        private bool isTraining = true;

        // Loads the Course info without storing the steps for all available courses
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

        // Dynamically generates a Menu where all Courses are listed
        // Have to call LoadAddCourseInfos first.
        public void GenerateCoursesMenu(GameObject coursesMenuButtonCollection)
        {
            foreach (CourseInfoHolder info in courses.Courses)
            {
                // Create Button for a specific course
                GameObject newButton = Instantiate(buttonPrefab);
                newButton.transform.localScale += new Vector3(2, 2, 0);
                newButton.name = info.CourseTitle;
                var buttonConfigHelper = newButton.GetComponent<ButtonConfigHelper>();

                buttonConfigHelper.MainLabelText = info.CourseTitle;

                // On Focus give user more information on the course
                var onFocusReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnFocusReceiver>();
                if (onFocusReciever == null)
                {
                    onFocusReciever = newButton.GetComponent<Interactable>().AddReceiver<InteractableOnFocusReceiver>();
                }
                onFocusReciever.OnFocusOn.AddListener(() => ActivateDetails());
                onFocusReciever.OnFocusOn.AddListener(() => SetCourseDetails(info.CourseID));
                onFocusReciever.OnFocusOff.AddListener(() => DeactivateDetails());

                // On Click open the menu of the particular course
                var onClickReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>();
                if (onClickReciever == null)
                {
                    onClickReciever = newButton.GetComponent<Interactable>().AddReceiver<InteractableOnPressReceiver>();
                }
                onClickReciever.OnPress.AddListener(() => MenuObject.GetComponent<MenuHelper>().SelectedMenuOption(info.CourseID));

                // Add Button to the Collection and display it
                newButton.SetActive(true);
                newButton.transform.SetParent(coursesMenuButtonCollection.transform);
            }

            Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection objCollectionComponent =
                coursesMenuButtonCollection.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();
            // Update, so Button are displayed on the grid instead of on top of each other
            objCollectionComponent.UpdateCollection();
        }

        // Returns the course infomarion without steps for a particular course.
        public CourseInfoHolder GetCourseInfo(int courseID)
        {
            return courses.Courses[courseID];
        }

        // Returns the course information without steps for the currently selected/played course.
        public CourseInfoHolder GetCurrentCourseInfo()
        {
            return courses.Courses[courses.CurrentCourse];
        }

        // Load a particular course with step information.
        // Param courseName (in): the name that represents the course in the courseToPath dictionary
        public void LoadCourse(string courseName)
        {
            // Delete the previous course that was loaded
            ClearState();

            // Find course to be loaded
            string jsonPath;
            bool courseFound = courseToPath.TryGetValue(courseName, out jsonPath);
            if (!courseFound)
            {
                Debug.Log("Course not found: " + courseName + ". Check that the Course Button, CourseMenuHelper.courseToPath and JSON filepath are sync.");
                return;
            }

            // Read the course
            string json_data = File.ReadAllText(jsonPath);
            CourseJSON course_steps = JsonUtility.FromJson<CourseJSON>(json_data);

            foreach (StepContainer step in course_steps.steps)
            {
                steps.Add((step.Position), step);
            }
            foreach (CoreographieContainer coreographie in course_steps.coreographies)
            {
                coreographies.Add(coreographie.Position, coreographie);
            }

            courses.CurrentCourse = course_steps.CourseID;
            isTraining = true;

            GenerateCourseMenu();
        }

        // Load all Coreographies.
        public void LoadFreeplay()
        {
            ClearState();

            int id = 0;
            foreach (KeyValuePair<string, string> entry in courseToPath)
            {
                string json_data = File.ReadAllText(entry.Value);
                CourseJSON course_steps = JsonUtility.FromJson<CourseJSON>(json_data);
                CourseInfoHolder course = new CourseInfoHolder();

                foreach (CoreographieContainer coreographie in course_steps.coreographies)
                {
                    coreographies.Add(id++, coreographie);
                }
            }
            isTraining = false;

            GenerateCourseMenu();
        }

        // Dynamically generates a Menu where all steps and coreographies of a course are listed or all coreographies of all courses are listed.
        private void GenerateCourseMenu()
        {
            for(int i = 0; i < steps.Count + coreographies.Count; i++)
            {
                // Create button for step/coreography
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
                // x is used in callback methods where the ref of i would be already changed and result in error
                int x = i;

                // On Focus give user more information on the step/coreo
                // On press starts the coreo or step
                var onPressReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnPressReceiver>();
                var onFocusReciever = newButton.GetComponent<Interactable>().GetReceiver<InteractableOnFocusReceiver>();
                if (onFocusReciever == null)
                {
                    onFocusReciever = newButton.GetComponent<Interactable>().AddReceiver<InteractableOnFocusReceiver>();
                }
                if (onPressReciever == null)
                {
                    onPressReciever = newButton.GetComponent<Interactable>().AddReceiver<InteractableOnPressReceiver>();
                }
                onFocusReciever.OnFocusOn.AddListener(() => ActivateDetails());
                if (isTraining)
                    onFocusReciever.OnFocusOn.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().SetMoveDetails(pos));
                else
                {                  
                    onFocusReciever.OnFocusOn.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().SetMoveDetails(x));
                }
                onPressReciever.OnPress.AddListener(() => MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(movementLocation));
                onFocusReciever.OnFocusOff.AddListener(() => DeactivateDetails());
                
                if (steps.ContainsKey(i))
                {
                    onPressReciever.OnPress.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().StartStep(pos));
                } 
                else if (coreographies.ContainsKey(i))
                {
                    if (isTraining)
                        onPressReciever.OnPress.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().StartCoreography(pos));
                    else
                        onPressReciever.OnPress.AddListener(() => CourseMenuHelperObject.GetComponent<CourseMenuHelper>().StartCoreography(x));
                }
                onPressReciever.OnPress.AddListener(() => DeactivateDetails(true));

                // Add Button to the Collection and display it
                newButton.SetActive(true);
                newButton.transform.SetParent(CourseButtonCollection.transform);
            }

            Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection objCollectionComponent = 
                CourseButtonCollection.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();
            // Update, so Button are displayed on the grid instead of on top of each other
            objCollectionComponent.UpdateCollection();
        }

        // Removes all Steps and Coreographies currently stored. Removes all butons from the specific course list.
        private void ClearState()
        {
            steps.Clear();
            coreographies.Clear();

            for (int i = CourseButtonCollection.transform.childCount - 1; i>=0; i--)
            {
                GameObject.DestroyImmediate(CourseButtonCollection.transform.GetChild(i).gameObject);
            }
        }

        // Sets the course's detail on the CourseDescription panel under the Menu
        public void SetCourseDetails(int courseID)
        {
            
            UnityEngine.UI.Text DescriptionText = CourseDescription.GetComponent<UnityEngine.UI.Text>();
            CourseInfoHolder info = courses.Courses[courseID]; 
            Debug.Log("<size=30>" + info.CourseTitle + "</size>\n<size=20>" + info.CourseDescription + "</size>");
            DescriptionText.text = "<size=30>" + info.CourseTitle + "</size>\n<size=20>" + info.CourseDescription + "</size>";
        }

        // Sets the step's or coreo's detail on the CourseDescription panel under the Menu
        public void SetMoveDetails(int position)
        {
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

        // Starts a step
        // TODO: consider one function in PoseteacherMain instead of a list of methods that have to be called (move responsibility)
        public void StartStep(int stepid)
        {
            CurrentStepOrCoreo = stepid;
            CurrentBlock = 0;
            MenuObject.SetActive(false);
            CoreographyHolder.SetActive(false);
            TrainingHolder.SetActive(true);
            PoseteacherMain main = MainObject.GetComponent<PoseteacherMain>();
            main.ShowTeacher();
            main.SetIsChoreography(false);
            main.ActivateIndicators();
        }

        // Starts a coreography
        // TODO: consider one function in PoseteacherMain instead of a list of methods that have to be called (move responsibility)
        public void StartCoreography(int coreoid)
        {
            CurrentStepOrCoreo = coreoid;
            CurrentBlock = 0;
            MenuObject.SetActive(false);
            TrainingHolder.SetActive(false);
            CoreographyHolder.SetActive(true);
            PoseteacherMain main = MainObject.GetComponent<PoseteacherMain>();
            main.ShowTeacher();
            main.SetIsChoreography(true);
            main.ActivateIndicators();
            main.ResetTotalScore();
            main.StartRecordingMode(true); // These main calls would probably be better of in a single call when next refactored...
            main.pauseTeacher = false;
        }

        // Returns with the name of the currently played Step or Coreography
        public string CurrentStepName()
        {
            if (steps.ContainsKey(CurrentStepOrCoreo))
            {
                return steps[CurrentStepOrCoreo].Name;
            }
            else if (coreographies.ContainsKey(CurrentStepOrCoreo))
            {
                return coreographies[CurrentStepOrCoreo].Name;
            }

            return "No Name Found";
        }

        // Loads the next step (or coreography) and starts it
        public void StartNextStep()
        {
            if (isTraining)
            {
                if (steps.ContainsKey(CurrentStepOrCoreo + 1)) {
                    MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(steps[CurrentStepOrCoreo + 1].Blocks[0].FileLocation);
                    StartStep(CurrentStepOrCoreo + 1);

                } else if (coreographies.ContainsKey(CurrentStepOrCoreo + 1))
                {
                    MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(coreographies[CurrentStepOrCoreo + 1].FileLocation);
                    StartCoreography(CurrentStepOrCoreo + 1);
                } else
                {
                    // TODO: No other step available, handle ending everything
                }



            } else
            {
                // TODO if we are doing only coreographies (Free play)
                if (coreographies.ContainsKey(CurrentStepOrCoreo + 1))
                {
                    MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(coreographies[CurrentStepOrCoreo + 1].FileLocation);
                    StartCoreography(CurrentStepOrCoreo + 1);
                }
            }
        }

        // Loads the previous step (or coreography) and starts it
        public void StartPreviousStep()
        {
            if (isTraining)
            {
                if (steps.ContainsKey(CurrentStepOrCoreo - 1))
                {
                    MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(steps[CurrentStepOrCoreo - 1].Blocks[0].FileLocation);
                    StartStep(CurrentStepOrCoreo - 1);

                }
                else if (coreographies.ContainsKey(CurrentStepOrCoreo - 1))
                {
                    MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(coreographies[CurrentStepOrCoreo - 1].FileLocation);
                    StartCoreography(CurrentStepOrCoreo - 1);
                }
                else
                {
                    // TODO: No other step available, handle ending everything
                }



            }
            else
            {
                // TODO if we are doing only coreographies (Free play)
            }
            MainObject.GetComponent<PoseteacherMain>().StopShowingRecording();
        }

        // Loads the next block of the current step or the next step if no more blocks are available.
        public void StartNextBlock()
        {
            if (coreographies.ContainsKey(CurrentStepOrCoreo))
            {
                // Move to next Step (most likely we will never end up here
                StartNextStep();
            }
            CurrentBlock++;
            if (CurrentBlock < steps[CurrentStepOrCoreo].Blocks.Length)
                // Move to next block
                MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(steps[CurrentStepOrCoreo].Blocks[CurrentBlock].FileLocation);
            else
                // Move to next step, consider notifying the user...
                StartNextStep();

        }

        // Loads the previous block of the current step or the next step if no more blocks are available.
        public void StartPreviousBlock()
        {
            CurrentBlock--;
            if (CurrentBlock >= 0)
                MainObject.GetComponent<PoseteacherMain>().SetTeacherFile(steps[CurrentStepOrCoreo].Blocks[CurrentBlock].FileLocation);
            else
                StartPreviousStep();
            // TODO else no previous block available (msg user?) move to previous step?
        }

        private int activeFocuses = 0;
        private void ActivateDetails()
        {
            if (activeFocuses < 0) activeFocuses = 0;
            activeFocuses++;
            if (activeFocuses > 0)
            {
                CourseDetails.SetActive(true);
            }
        }

        private void DeactivateDetails(bool forcezero = false)
        {
            activeFocuses--;
            if (forcezero) activeFocuses = 0;
            if (activeFocuses == 0)
            {
                CourseDetails.SetActive(false);
            }
        }
    }
}