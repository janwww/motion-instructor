using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher
{
    public class AvatarSettings : MonoBehaviour
    {
        public GameObject VerticalGrid;
        public GameObject SelfAvatarButtonCollection;
        public GameObject TeacherAvatarButtonCollection;
        public GameObject MainObject;
        // TODO in unity set buttonPrefab to to the actual Prefab. There is aissue with that for some reason...
        public GameObject buttonPrefab;
        public GameObject RadioButtonContainer;
        private PoseteacherMain poseteacher;

        private bool selectedAvatarSelf = true;
        private int selectedAvatarNum = -1;
        private int currentSelfCount = 0;
        private int currentTeacherCount = 0;

        // Start is called before the first frame update
        void Start()
        {

            // TODO persistency
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void PopulateAvatarButtons()
        {
            //  clear collections
          /*  while (SelfAvatarButtonCollection.transform.childCount > 0)
            {
                Transform b = SelfAvatarButtonCollection.transform.GetChild(0);
                b.transform.parent = null;
                b.gameObject.SetActive(false);
            }
            while (TeacherAvatarButtonCollection.transform.childCount > 0)
            {
                Transform b = TeacherAvatarButtonCollection.transform.GetChild(0).transform.parent = null;
            } */

            poseteacher = MainObject.GetComponent<PoseteacherMain>();

            List<AvatarContainer> selfs = poseteacher.GetSelfAvatarContainers();
            List<AvatarContainer> teachers = poseteacher.GetTeacherAvatarContainers();

            GameObject addSelfButton = Instantiate(buttonPrefab);
            GameObject addTeacherButton = Instantiate(buttonPrefab);

            addSelfButton.transform.localScale += new Vector3(2, 2, 0);
            addTeacherButton.transform.localScale += new Vector3(2, 2, 0);
            addSelfButton.name = "AddSelfButton";
            addTeacherButton.name = "AddTeacherButton";

            var buttonConfigHelper = addSelfButton.GetComponent<ButtonConfigHelper>();
            buttonConfigHelper.MainLabelText = "Add Self Avatar";

            buttonConfigHelper = addTeacherButton.GetComponent<ButtonConfigHelper>();
            buttonConfigHelper.MainLabelText = "Add Teacher Avatar";

            var onClickReciever = addSelfButton.GetComponent<Interactable>().GetReceiver<InteractableOnClickReceiver>();
            onClickReciever.OnClicked.AddListener(() => AddAvatar(true));

            onClickReciever = addTeacherButton.GetComponent<Interactable>().GetReceiver<InteractableOnClickReceiver>();
            onClickReciever.OnClicked.AddListener(() => AddAvatar(false));

            addSelfButton.transform.SetParent(SelfAvatarButtonCollection.transform);
            addTeacherButton.transform.SetParent(TeacherAvatarButtonCollection.transform);

         //   var radioButtonCollectionComponent = RadioButtonContainer.GetComponent<InteractableToggleCollection>();

            foreach (AvatarContainer avatar in selfs)
            {
                GameObject selfButton = Instantiate(buttonPrefab);
                selfButton.transform.localScale += new Vector3(2, 2, 0);
                selfButton.name = "SelfButton" + currentSelfCount;
                buttonConfigHelper = selfButton.GetComponent<ButtonConfigHelper>();
                buttonConfigHelper.MainLabelText = "Self Avatar " + currentSelfCount;

                onClickReciever = selfButton.GetComponent<Interactable>().GetReceiver<InteractableOnClickReceiver>();
                onClickReciever.OnClicked.AddListener(() => SetSelectedAvatarPos(currentSelfCount));
                onClickReciever.OnClicked.AddListener(() => SetSelectedAvatarSelf(true));

            //    radioButtonCollectionComponent.ToggleList.SetValue(selfButton.GetComponent<Interactable>(), currentSelfCount + currentTeacherCount);

                selfButton.transform.SetParent(SelfAvatarButtonCollection.transform);
                currentSelfCount++;
            }

            foreach (AvatarContainer avatar in teachers)
            {
                GameObject teacherButton = Instantiate(buttonPrefab);
                teacherButton.transform.localScale += new Vector3(2, 2, 0);
                teacherButton.name = "TeacherButton" + currentTeacherCount;
                buttonConfigHelper = teacherButton.GetComponent<ButtonConfigHelper>();
                buttonConfigHelper.MainLabelText = "Teacher Avatar " + currentTeacherCount;

                onClickReciever = teacherButton.GetComponent<Interactable>().GetReceiver<InteractableOnClickReceiver>();
                onClickReciever.OnClicked.AddListener(() => SetSelectedAvatarPos(currentTeacherCount));
                onClickReciever.OnClicked.AddListener(() => SetSelectedAvatarSelf(false));

                teacherButton.transform.SetParent(TeacherAvatarButtonCollection.transform);
            }

            Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection objCollectionComponent =
                SelfAvatarButtonCollection.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();
            objCollectionComponent.UpdateCollection();

            objCollectionComponent =
                TeacherAvatarButtonCollection.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();
            objCollectionComponent.UpdateCollection();

            objCollectionComponent =
                VerticalGrid.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();
            objCollectionComponent.UpdateCollection();
        }

        void AddAvatar(bool self)
        {
            poseteacher.AddAvatar(self);
            AddAvatarButton(self);
        }

        void AddAvatarButton(bool self)
        {
            if (self)
            {
                GameObject selfButton = Instantiate(buttonPrefab);
                selfButton.transform.localScale += new Vector3(2, 2, 0);
                selfButton.name = "SelfButton" + currentSelfCount;
                var buttonConfigHelper = selfButton.GetComponent<ButtonConfigHelper>();
                buttonConfigHelper.MainLabelText = "Self Avatar " + currentSelfCount;

                var onClickReciever = selfButton.GetComponent<Interactable>().GetReceiver<InteractableOnClickReceiver>();
                onClickReciever.OnClicked.AddListener(() => SetSelectedAvatarPos(currentSelfCount));
                onClickReciever.OnClicked.AddListener(() => SetSelectedAvatarSelf(true));

                selfButton.transform.SetParent(SelfAvatarButtonCollection.transform);

                Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection objCollectionComponent =
                    SelfAvatarButtonCollection.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();
                objCollectionComponent.UpdateCollection();
                currentSelfCount++;
            } else
            {
                GameObject teacherButton = Instantiate(buttonPrefab);
                teacherButton.transform.localScale += new Vector3(2, 2, 0);
                teacherButton.name = "TeacherButton" + currentTeacherCount;
                var buttonConfigHelper = teacherButton.GetComponent<ButtonConfigHelper>();
                buttonConfigHelper.MainLabelText = "Teacher Avatar " + currentTeacherCount;

                var onClickReciever = teacherButton.GetComponent<Interactable>().GetReceiver<InteractableOnClickReceiver>();
                onClickReciever.OnClicked.AddListener(() => SetSelectedAvatarPos(currentTeacherCount));
                onClickReciever.OnClicked.AddListener(() => SetSelectedAvatarSelf(false));

                teacherButton.transform.SetParent(TeacherAvatarButtonCollection.transform);

                Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection objCollectionComponent =
                    TeacherAvatarButtonCollection.GetComponent<Microsoft.MixedReality.Toolkit.Utilities.GridObjectCollection>();
                objCollectionComponent.UpdateCollection();
                currentTeacherCount++;
            }
        }

        public void SetSelectedAvatarSelf(bool self)
        {
            selectedAvatarSelf = self;
        }
        public void SetSelectedAvatarPos(int pos)
        {
            selectedAvatarNum = pos;
        }

        public void ChangeAvatarType(string typeString)
        {
            AvatarType type = AvatarType.CUBE;
            switch(typeString)
            {
                case "CUBE": type = AvatarType.CUBE; break;
                case "STICK": type = AvatarType.STICK; break;
                case "ROBOT": type = AvatarType.ROBOT; break;
                case "SMPL": type = AvatarType.SMPL; break;
                default: type = AvatarType.STICK; break;
            }

            poseteacher = MainObject.GetComponent<PoseteacherMain>();

            if (selectedAvatarSelf)
            {
                List<AvatarContainer> selfs = poseteacher.GetSelfAvatarContainers();
                selfs[selectedAvatarNum].ChangeActiveType(type);
            }
            else
            {
                List<AvatarContainer> teachers = poseteacher.GetTeacherAvatarContainers();
                teachers[selectedAvatarNum].ChangeActiveType(type);
            }
            
            
        }
    }
}