using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace PoseTeacher
{
    public enum Menus
    {
        TITLE, DANCE, COURSES, SPECCOURSE, SETTINGS, AVATARSETTINGS,
        DIFFICULTYSETTINGS, FEEDSETTINGS, RECORDMENU, COREOMENU
    }
    public enum MenuState
    {
        INMAIN, PAUSED
    }
    public enum PauseType
    {
        TRAINING, COREO, RECORD
    }

    public class MenuHelper : MonoBehaviour
    {

        public GameObject MenuObject;
        public GameObject MainObject;
        private Dictionary<Menus, GameObject> menus = new Dictionary<Menus, GameObject>();
        private Menus CurrentMenu;
        private Menus PreviousMainMenu;
        private CourseMenuHelper courseMenuHelper;
        private TextMeshPro TitleText;
        public MenuState state = MenuState.INMAIN;

        public GameObject TrainingElements;
        public GameObject CoreoElements;
        public GameObject RecordElements;
       // private bool isTrainingPause = true;
        private PauseType pauseType = PauseType.TRAINING;

        public void setState(string state)
        {
            switch (state)
            {
                case "PAUSED_training":
                    this.state = MenuState.PAUSED;
                    // Set the menu to settings, but save state
                    PreviousMainMenu = CurrentMenu;
                    CurrentMenu = Menus.SETTINGS;
                    UpdateMenu();
                    // Show menu
                    MenuObject.SetActive(true);
                    // Change Buttons
                    MenuObject.transform.Find("TitleBarHolder").Find("Buttons").Find("MainButtons").gameObject.SetActive(false);
                    MenuObject.transform.Find("TitleBarHolder").Find("Buttons").Find("PausedButtons").gameObject.SetActive(true);
                    TrainingElements.SetActive(false);
                    pauseType = PauseType.TRAINING;
                    break;
                case "PAUSED_coreo":
                    this.state = MenuState.PAUSED;
                    // Set the menu to settings, but save state
                    PreviousMainMenu = CurrentMenu;
                    CurrentMenu = Menus.SETTINGS;
                    UpdateMenu();
                    // Show menu
                    MenuObject.SetActive(true);
                    // Change Buttons
                    MenuObject.transform.Find("TitleBarHolder").Find("Buttons").Find("MainButtons").gameObject.SetActive(false);
                    MenuObject.transform.Find("TitleBarHolder").Find("Buttons").Find("PausedButtons").gameObject.SetActive(true);
                    CoreoElements.SetActive(false);
                    pauseType = PauseType.COREO;
                    break;
                case "PAUSED_record":
                    this.state = MenuState.PAUSED;
                    PreviousMainMenu = CurrentMenu;
                    CurrentMenu = Menus.SETTINGS;
                    UpdateMenu();
                    // Show menu
                    MenuObject.SetActive(true);
                    // Change Buttons
                    MenuObject.transform.Find("TitleBarHolder").Find("Buttons").Find("MainButtons").gameObject.SetActive(false);
                    MenuObject.transform.Find("TitleBarHolder").Find("Buttons").Find("PausedButtons").gameObject.SetActive(true);
                    RecordElements.SetActive(false);
                    pauseType = PauseType.RECORD;
                    break;
                case "INMAIN":
                    this.state = MenuState.INMAIN;
                    // Revert Menu state
                    CurrentMenu = PreviousMainMenu;
                    UpdateMenu();
                    // Hide Menu
                    MenuObject.SetActive(false);
                    // change buttins
                    MenuObject.transform.Find("TitleBarHolder").Find("Buttons").Find("MainButtons").gameObject.SetActive(true);
                    MenuObject.transform.Find("TitleBarHolder").Find("Buttons").Find("PausedButtons").gameObject.SetActive(false);
                    
                    switch (pauseType)
                    {
                        case PauseType.TRAINING:
                            TrainingElements.SetActive(true);
                            break;
                        case PauseType.COREO:
                            CoreoElements.SetActive(true);
                            break;
                        case PauseType.RECORD:
                            RecordElements.SetActive(true);
                            break;
                        default:
                            break;
                    }

                    break;
            }
        }

        private void Start()
        {
            Transform MenuHolderTr = MenuObject.transform.Find("NearMenu");
            menus.Add(Menus.TITLE, MenuHolderTr.Find("TitleMenu").gameObject);
            menus.Add(Menus.DANCE, MenuHolderTr.Find("DanceMenu").gameObject);
            menus.Add(Menus.COURSES, MenuHolderTr.Find("CourseMenu").gameObject);
            menus.Add(Menus.SPECCOURSE, MenuHolderTr.Find("SpecCourseMenu").gameObject);
            menus.Add(Menus.SETTINGS, MenuHolderTr.Find("SettingsMenu").gameObject);
            menus.Add(Menus.AVATARSETTINGS, MenuObject.transform.Find("AvatarSettings").gameObject);
            menus.Add(Menus.DIFFICULTYSETTINGS, MenuHolderTr.Find("DifficultySettingsMenu").gameObject);
            menus.Add(Menus.FEEDSETTINGS, MenuHolderTr.Find("PosefeedSettingsMenu").gameObject);
            menus.Add(Menus.RECORDMENU, MenuHolderTr.Find("RecordMenu").gameObject);

            CurrentMenu = Menus.TITLE;

            courseMenuHelper = MenuObject.transform.Find("CourseMenuHelper").gameObject.GetComponent<CourseMenuHelper>();
            courseMenuHelper.LoadAllCourseInfos();
            courseMenuHelper.GenerateCoursesMenu(menus[Menus.COURSES].transform.Find("CourseMenuButtonCollection").gameObject);
            // TODO: create the buttons from code

            TitleText = MenuObject.transform.Find("TitleBarHolder").Find("TitleBar").Find("Title").gameObject.GetComponent<TextMeshPro>();
            SetTitleBarText();
        }

        public void BackToHome()
        {
            if (CurrentMenu != Menus.TITLE)
            {
                // Set current Menu to Inactive
                menus[CurrentMenu].SetActive(false);
                // Set Title Menu to Active
                CurrentMenu = Menus.TITLE;
                DeactivateTeachers();
                menus[CurrentMenu].SetActive(true); 
            }

            SetTitleBarText();
        }

        public void BackToPreviousMenu()
        {
            // Set current Menu to Inactive
            if (CurrentMenu != Menus.TITLE)
                menus[CurrentMenu].SetActive(false);

            // Set Upper level Menu to Active
            switch (CurrentMenu)
            {
                case Menus.TITLE:
                    break;
                case Menus.DANCE:
                    CurrentMenu = Menus.TITLE;
                    menus[CurrentMenu].SetActive(true);
                    break;
                case Menus.COURSES:
                    CurrentMenu = Menus.DANCE;
                    menus[CurrentMenu].SetActive(true);
                    break;
                case Menus.SPECCOURSE:
                    CurrentMenu = Menus.COURSES;
                    menus[CurrentMenu].SetActive(true);
                    break;
                case Menus.SETTINGS:
                    CurrentMenu = Menus.TITLE;
                    DeactivateTeachers();
                    menus[CurrentMenu].SetActive(true);
                    break;
                case Menus.AVATARSETTINGS:
                    CurrentMenu = Menus.SETTINGS;
                    menus[CurrentMenu].SetActive(true);
                    break;
                case Menus.DIFFICULTYSETTINGS:
                    CurrentMenu = Menus.SETTINGS;
                    menus[CurrentMenu].SetActive(true);
                    break;
                case Menus.FEEDSETTINGS:
                    CurrentMenu = Menus.SETTINGS;
                    menus[CurrentMenu].SetActive(true);
                    break;
                case Menus.RECORDMENU:
                    CurrentMenu = Menus.TITLE;
                    menus[CurrentMenu].SetActive(true);
                    break;
                default:
                    break;
            }

            SetTitleBarText();
        }

        public void BackToGame()
        {
            setState("INMAIN");
        }

        public void BackToSetting()
        {
            menus[CurrentMenu].SetActive(false);
            CurrentMenu = Menus.SETTINGS;
            menus[CurrentMenu].SetActive(true);
        }

        private void UpdateMenu()
        {
            foreach (KeyValuePair<Menus, GameObject> menu in menus)
            {
                menu.Value.SetActive(false);
            }
            menus[CurrentMenu].SetActive(true);
            SetTitleBarText();
        }

        public void SelectedMenuOption(int selectedMenuOption)
        {

            switch (CurrentMenu)
            {
                case Menus.TITLE:
                    SelectedInTitleMenu(selectedMenuOption);
                    break;
                case Menus.DANCE:
                    SelectedInDanceMenu(selectedMenuOption);
                    break;
                case Menus.COURSES:
                    SelectedInCoursesMenu(selectedMenuOption);
                    break;
                case Menus.SPECCOURSE:
                    // This is ccurently done in CourseMenuHelper StartStep/StartCoreography function
                    break;
                case Menus.SETTINGS:
                    SelectedInSettingsMenu(selectedMenuOption);
                    break;
                case Menus.DIFFICULTYSETTINGS:
                    SelectedInDifficultySettingsMenu(selectedMenuOption);
                    break;
                case Menus.FEEDSETTINGS:
                    SelectedInFeedSettingsMenu(selectedMenuOption);
                    break;
                case Menus.RECORDMENU:
                    SelectedInRecordMovementMenu(selectedMenuOption);
                    break;
                default:
                    break;
            }

            SetTitleBarText();
        }

        private void SelectedInTitleMenu(int selectedMenuOption)
        {
            switch (selectedMenuOption)
            {
                // Title -> Dance Menu
                case 0:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.DANCE;
                    menus[CurrentMenu].SetActive(true);
                    break;
                // Title -> Workout Menu
                case 1:
                    /*   menus[CurrentMenu].SetActive(false);
                       CurrentMenu = Menus.WORKOUT;
                       menus[CurrentMenu].SetActive(true); */
                    break;
                // Title -> Settings Menu
                case 2:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.SETTINGS;
                    ActivateTeachers();
                    menus[CurrentMenu].SetActive(true);
                    break;
                // Title -> Record movement Menu
                case 3:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.RECORDMENU;
                    menus[CurrentMenu].SetActive(true);
                    break;
                default:
                    break;
            }
        }

        private void SelectedInDanceMenu(int selectedMenuOption)
        {
            switch (selectedMenuOption)
            {
                // Dance -> Courses Menu
                case 0:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.COURSES;
                    menus[CurrentMenu].SetActive(true);
                    break;
                // Dance -> Freeplay Menu
                case 1:
                    /*   menus[CurrentMenu].SetActive(false);
                       CurrentMenu = Menus.FREEPLAY;
                       menus[CurrentMenu].SetActive(true); */
                    break;
                default:
                    break;
            }
        }

        // TODO make this dynamic too
        private void SelectedInCoursesMenu(int selectedMenuOption)
        {
            switch (selectedMenuOption)
            {
                // Courses -> Salsa male Menu
                case 0:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.SPECCOURSE;
                    courseMenuHelper.LoadCourse("salsa");
                    menus[CurrentMenu].SetActive(true);
                    break;
                // Courses -> Salsa female Menu
                case 1:
                    break;
                default:
                    break;
            }
        }

        private void SelectedInSettingsMenu(int selectedMenuOption)
        {
            switch (selectedMenuOption)
            {
                // Settings -> AvatarSettings Menu
                case 0:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.AVATARSETTINGS;
                    HighlightSelectedAvatarType();
                    menus[CurrentMenu].SetActive(true);
                    break;
                // Settings -> RGBFeedSettings Menu
                case 1:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.FEEDSETTINGS;
                    HighlightSelectedFeed();
                    menus[CurrentMenu].SetActive(true);
                    break;
                // Settings -> DifficultySettings Menu
                case 2:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.DIFFICULTYSETTINGS;
                    HighlightSelectedDifficulty();
                    menus[CurrentMenu].SetActive(true);
                    break;
                default:
                    break;
            }
        }

        private void SelectedInDifficultySettingsMenu(int selectedMenuOption)
        {
            switch (selectedMenuOption)
            {
                // Easy Difficutly selected
                case 0:
                    MainObject.GetComponent<PoseteacherMain>().SetDifficulty(Difficulty.EASY);
                    break;
                // Medium Difficulty Selected
                case 1:
                    MainObject.GetComponent<PoseteacherMain>().SetDifficulty(Difficulty.MEDIUM);
                    break;
                // Hard Difficulty Selected
                case 2:
                    MainObject.GetComponent<PoseteacherMain>().SetDifficulty(Difficulty.HARD);
                    break;
                default:
                    break;
            }
            HighlightSelectedDifficulty();
        }

        private void SelectedInFeedSettingsMenu(int selectedMenuOption)
        {
            switch (selectedMenuOption)
            {
                // Kinect input selected
                case 0:
                    MainObject.GetComponent<PoseteacherMain>().SelfPoseInputSource = PoseInputSource.KINECT;
                    break;
                // Websocket input Selected
                case 1:
                    MainObject.GetComponent<PoseteacherMain>().SelfPoseInputSource = PoseInputSource.WEBSOCKET;
                    break;
                // File input Selected
                case 2:
                    MainObject.GetComponent<PoseteacherMain>().SelfPoseInputSource = PoseInputSource.FILE;
                    break;
                default:
                    break;
            }
            HighlightSelectedFeed();
        }

        private void SelectedInRecordMovementMenu(int selectedMenuOption)
        {
            switch (selectedMenuOption)
            {
                // Start recording
                case 0:
                    // TODO
                    StartRecording();
                    break;
                // Placeholder for recorded movements
                case 1:
                    // TODO
                    break;
                default:
                    break;
            }
        }


        private void HighlightSelectedDifficulty()
        {
            menus[CurrentMenu].transform.Find("DifficultySettingsMenuButtonCollection").Find("EasyDifficultySettingsButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);
            menus[CurrentMenu].transform.Find("DifficultySettingsMenuButtonCollection").Find("MediumDifficultySettingsButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);
            menus[CurrentMenu].transform.Find("DifficultySettingsMenuButtonCollection").Find("HardDifficultySettingsButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);

            switch (MainObject.GetComponent<PoseteacherMain>().difficulty)
            {
                case Difficulty.EASY:
                    menus[CurrentMenu].transform.Find("DifficultySettingsMenuButtonCollection").Find("EasyDifficultySettingsButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255,255,0);
                    break;
                case Difficulty.MEDIUM:
                    menus[CurrentMenu].transform.Find("DifficultySettingsMenuButtonCollection").Find("MediumDifficultySettingsButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0); 
                    break;
                case Difficulty.HARD:
                    menus[CurrentMenu].transform.Find("DifficultySettingsMenuButtonCollection").Find("HardDifficultySettingsButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0); 
                    break;
            }
        }

        private void HighlightSelectedFeed()
        {
            menus[CurrentMenu].transform.Find("PosefeedSettingsMenuButtonCollection").Find("KinectFeedButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);
            menus[CurrentMenu].transform.Find("PosefeedSettingsMenuButtonCollection").Find("RGBCameraFeedButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);
            menus[CurrentMenu].transform.Find("PosefeedSettingsMenuButtonCollection").Find("JSONFeedButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);

            switch (MainObject.GetComponent<PoseteacherMain>().SelfPoseInputSource)
            {
                case PoseInputSource.KINECT:
                    menus[CurrentMenu].transform.Find("PosefeedSettingsMenuButtonCollection").Find("KinectFeedButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0);
                    break;
                case PoseInputSource.WEBSOCKET:
                    menus[CurrentMenu].transform.Find("PosefeedSettingsMenuButtonCollection").Find("RGBCameraFeedButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0);
                    break;
                case PoseInputSource.FILE:
                    menus[CurrentMenu].transform.Find("PosefeedSettingsMenuButtonCollection").Find("JSONFeedButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0);
                    break;
            }
        }

        public void HighlightSelectedAvatarType()
        {
            menus[CurrentMenu].transform.Find("VerticalGrid").Find("AvatarTypes").Find("AvatarTypesButtonCollection").Find("CubeAvatarButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);
            menus[CurrentMenu].transform.Find("VerticalGrid").Find("AvatarTypes").Find("AvatarTypesButtonCollection").Find("StickAvatarButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);
            menus[CurrentMenu].transform.Find("VerticalGrid").Find("AvatarTypes").Find("AvatarTypesButtonCollection").Find("RobotAvatarButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);
            menus[CurrentMenu].transform.Find("VerticalGrid").Find("AvatarTypes").Find("AvatarTypesButtonCollection").Find("SMPLAvatarButton").
                Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 255);

            switch (MainObject.GetComponent<PoseteacherMain>().GetSelfAvatarContainers()[0].activeType)
            {
                case AvatarType.CUBE:
                    menus[CurrentMenu].transform.Find("VerticalGrid").Find("AvatarTypes").Find("AvatarTypesButtonCollection").Find("CubeAvatarButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0);
                    break;
                case AvatarType.STICK:
                    menus[CurrentMenu].transform.Find("VerticalGrid").Find("AvatarTypes").Find("AvatarTypesButtonCollection").Find("StickAvatarButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0);
                    break;
                case AvatarType.ROBOT:
                    menus[CurrentMenu].transform.Find("VerticalGrid").Find("AvatarTypes").Find("AvatarTypesButtonCollection").Find("RobotAvatarButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0);
                    break;
                case AvatarType.SMPL:
                    menus[CurrentMenu].transform.Find("VerticalGrid").Find("AvatarTypes").Find("AvatarTypesButtonCollection").Find("SMPLAvatarButton").
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0);
                    break;
            }
        }

        private void SetTitleBarText()
        {
            
            switch (CurrentMenu)
            {
                case Menus.TITLE:
                    TitleText.text = "<size=120%>Motion Instructor</size>\nWhat are we going to do today?";
                    break;
                case Menus.DANCE:
                    TitleText.text = "<size=120%>Dance</size>\nLearn a new dance style, or challange yourself in one that you already know!";
                    break;
                case Menus.COURSES:
                    TitleText.text = "<size=120%>Courses</size>\nSelect the dance style you want to learn!";
                    break;
                case Menus.SPECCOURSE:
                    CourseInfoHolder info = courseMenuHelper.GetCurrentCourseInfo();
                    TitleText.text = "<size=120%>" + info.CourseTitle + "</size>\n" + info.CourseShortDescription;
                    break;
                case Menus.SETTINGS:
                    TitleText.text = "<size=120%>Settings</size>";
                    break;
                default:
                    break;
            }
        }

        public void SetCourseDetailsText(int courseID)
        {
            courseMenuHelper.SetCourseDetails(courseID);
        }

        private void ActivateTeachers()
        {
            foreach (AvatarContainer avatar in MainObject.GetComponent<PoseteacherMain>().GetTeacherAvatarContainers())
            {
                avatar.avatarContainer.SetActive(true);
            }
        }

        private void DeactivateTeachers()
        {
            foreach (AvatarContainer avatar in MainObject.GetComponent<PoseteacherMain>().GetTeacherAvatarContainers())
            {
                avatar.avatarContainer.SetActive(false);
            }
        }

        private void StartRecording()
        {
            MenuObject.SetActive(false);
            PoseteacherMain main = MainObject.GetComponent<PoseteacherMain>();
            main.StartRecordingMode(false);
            RecordElements.SetActive(true);
        }
    }
}
