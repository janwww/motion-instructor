using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

namespace PoseTeacher
{
    public static class OldMenuInitializer
    {
        private static GameObject GetSubmenu(GameObject menu, string subMenuName)
        {
            // Navigate 2 levels down
            return menu.transform.Find(subMenuName).transform.GetChild(0).gameObject;
        }

        private static void AddListener(GameObject subMenu, string buttonName, UnityEngine.Events.UnityAction call)
        {
            GameObject button = subMenu.transform.Find(buttonName).gameObject;
            InteractableOnClickReceiver onClickReceiver = button.GetComponent<Interactable>().GetReceiver<InteractableOnClickReceiver>();
            onClickReceiver.OnClicked.AddListener(call);
        }

        public static void AddAllListeners()
        {
            PoseteacherMain mainScript = GameObject.Find("Main").GetComponent<PoseteacherMain>();
            GameObject menu = GameObject.Find("Menus (old)/NearMenu");
            GameObject submenu;

            // Choose Algorithm
            submenu = GetSubmenu(menu, "Menu MainAlgo");
            AddListener(submenu, "ButtonAlternative", () => mainScript.SelfPoseInputSource = PoseInputSource.FILE);
            AddListener(submenu, "ButtonKinect", () => mainScript.SelfPoseInputSource = PoseInputSource.KINECT);

        }
    }
}
