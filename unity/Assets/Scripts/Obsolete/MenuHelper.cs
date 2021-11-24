using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PoseTeacher;
namespace PoseTeacherOld
{
    // All possible Menus that can be displayed in the main menu
    public enum Menus
    {
        TITLE, DANCE, COURSES, SPECCOURSE, FREEPLAY, SETTINGS, AVATARSETTINGS,
        DIFFICULTYSETTINGS, FEEDSETTINGS, FEEDBACKSETTINGS, RECORDMENU, COREOMENU
    }
    // State of what type of "main" menu the user is in
    public enum MenuState
    {
        INMAIN, PAUSED
    }
    // Enum to help recover ingame state
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

        private PauseType pauseType = PauseType.TRAINING;

        // Sets the state (and pausetype) of the new menu and displays it.
        // Saves the current state to allow going back to the old menu state on unpause
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
            menus.Add(Menus.FREEPLAY, MenuHolderTr.Find("SpecCourseMenu").gameObject);
            menus.Add(Menus.SPECCOURSE, MenuHolderTr.Find("SpecCourseMenu").gameObject);
            menus.Add(Menus.SETTINGS, MenuHolderTr.Find("SettingsMenu").gameObject);
            menus.Add(Menus.AVATARSETTINGS, MenuObject.transform.Find("AvatarSettings").gameObject);
            menus.Add(Menus.DIFFICULTYSETTINGS, MenuHolderTr.Find("DifficultySettingsMenu").gameObject);
            menus.Add(Menus.FEEDSETTINGS, MenuHolderTr.Find("PosefeedSettingsMenu").gameObject);
            menus.Add(Menus.RECORDMENU, MenuHolderTr.Find("RecordMenu").gameObject);
            menus.Add(Menus.FEEDBACKSETTINGS, MenuHolderTr.Find("FeedbackDisplaySettingsMenu").gameObject);

            CurrentMenu = Menus.TITLE;

            courseMenuHelper = MenuObject.transform.Find("CourseMenuHelper").gameObject.GetComponent<CourseMenuHelper>();
            courseMenuHelper.LoadAllCourseInfos();
            courseMenuHelper.GenerateCoursesMenu(menus[Menus.COURSES].transform.Find("CourseMenuButtonCollection").gameObject);

            TitleText = MenuObject.transform.Find("TitleBarHolder").Find("TitleBar").Find("Title").gameObject.GetComponent<TextMeshPro>();
            SetTitleBarText();
        }

        // Sets the menu to the TITLE menu screen (home)
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
                courseMenuHelper.DeactivateDetails(true);
            }

            SetTitleBarText();
        }

        // Sets the menu to the parent menu screen of the current menu screen
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
                case Menus.FREEPLAY:
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
                case Menus.FEEDBACKSETTINGS:
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

        // Resumes to the step/coreo/recording from the menu
        public void BackToGame()
        {
            // consider refactoring code from the setState function to here?
            setState("INMAIN");
        }

        // Sets the menu to the main Settings menu screen (mainly for paused state)
        public void BackToSetting()
        {
            menus[CurrentMenu].SetActive(false);
            CurrentMenu = Menus.SETTINGS;
            menus[CurrentMenu].SetActive(true);
        }

        // Inactivates the previous menu and activates the current (new) menu
        private void UpdateMenu()
        {
            foreach (KeyValuePair<Menus, GameObject> menu in menus)
            {
                menu.Value.SetActive(false);
            }
            menus[CurrentMenu].SetActive(true);
            SetTitleBarText();
        }

        // Handles Menu option selection (e.g. progressing menu to other parts...)
        // For actions to be taken see SelectedIn<ScreenName>Menu() functions
        // param selectedmenuOption (in): an id of the selected option, unique only up to a single menu screen. (multiple screen can have the same ids)
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
                case Menus.FREEPLAY:
                    // This is ccurently done in CourseMenuHelper StartStep/StartCoreography function
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
                case Menus.FEEDBACKSETTINGS:
                    SelectedInFeedbackSettingsMenu(selectedMenuOption);
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
                // Title -> Workout Menu (currently inactive)
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
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.FREEPLAY;
                    courseMenuHelper.LoadFreeplay();
                    menus[CurrentMenu].SetActive(true);
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
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.SPECCOURSE;
                    courseMenuHelper.LoadCourse("salsaf");
                    menus[CurrentMenu].SetActive(true);
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
                case 3:
                    menus[CurrentMenu].SetActive(false);
                    CurrentMenu = Menus.FEEDBACKSETTINGS;
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

        private void SelectedInFeedbackSettingsMenu(int selectedMenuOption)
        {
            switch (selectedMenuOption)
            {
                // VideoCube toggle pressed
                case 0:
                    MainObject.GetComponent<PoseteacherMain>().ChangeVideoCubeVisibilityAllowed();
                    break;
                // Graph toggle pressed
                case 1:
                    MainObject.GetComponent<PoseteacherMain>().ChangeGraphVisibilityAllowed();
                    break;
                default:
                    break;
            }
        }

        // Modify the currently selected difficulty's button to stand out
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
                        Find("IconAndText").Find("TextMeshPro").gameObject.GetComponent<TextMeshPro>().color = new Color(255, 255, 0);
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

        // Modify the currently selected Feed's button to stand out
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

        // Modify the currently selected avatar type's button to stand out
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

        // Modify the TitleBar text based on the currently active menu screen.
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
                case Menus.FREEPLAY:
                    TitleText.text = "<size=120%>Free play</size>\nChallenge yourself in your favourite coreographies!";
                    break;
                case Menus.SPECCOURSE:
                    CourseInfoHolder info = courseMenuHelper.GetCurrentCourseInfo();
                    TitleText.text = "<size=120%>" + info.CourseTitle + "</size>\n" + info.CourseShortDescription;
                    break;
                case Menus.SETTINGS:
                    TitleText.text = "<size=120%>Settings</size>";
                    break;
                case Menus.AVATARSETTINGS:
                    TitleText.text = "<size=120%>Avatar Settings</size>\nChange the avatar type and add more to the scene!";
                    break;
                case Menus.DIFFICULTYSETTINGS:
                    TitleText.text = "<size=120%>Difficulty Settings</size>\nHarder settings mean more precise movements!";
                    break;
                case Menus.FEEDSETTINGS:
                    TitleText.text = "<size=120%>Pose Input Settings</size>\nWhat channel do you want to send your poses?";
                    break;
                case Menus.FEEDBACKSETTINGS:
                    TitleText.text = "<size=120%>Visualization Settings</size>\nAdd additional objects that shows how close you follow!";
                    break;
                case Menus.RECORDMENU:
                    TitleText.text = "<size=120%>Record</size>\nRecord your own movements!";
                    break;
                case Menus.COREOMENU:
                    TitleText.text = "<size=120%>Free Play!</size>\nTry your favourite coreographies!";
                    break;
                default:
                    break;
            }
        }

        // Sets the course's detail on the CourseDescription panel under the Menu
        public void SetCourseDetailsText(int courseID)
        {
            courseMenuHelper.SetCourseDetails(courseID);
        }

        // Activate all teacher avatars
        private void ActivateTeachers()
        {
            foreach (AvatarContainer avatar in MainObject.GetComponent<PoseteacherMain>().GetTeacherAvatarContainers())
            {
                avatar.avatarContainer.SetActive(true);
            }
        }
        // Inactivate all teacher avatars
        private void DeactivateTeachers()
        {
            foreach (AvatarContainer avatar in MainObject.GetComponent<PoseteacherMain>().GetTeacherAvatarContainers())
            {
                avatar.avatarContainer.SetActive(false);
            }
        }
        // Starts the recording
        private void StartRecording()
        {
            MenuObject.SetActive(false);
            PoseteacherMain main = MainObject.GetComponent<PoseteacherMain>();
            main.StartRecordingMode(false);
            RecordElements.SetActive(true);
        }
    }
}
