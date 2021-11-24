using System;
using UnityEngine;

using PoseTeacher;
namespace PoseTeacherOld
{
    public class AvatarSettings : MonoBehaviour
    {
        // The Main GameObject. Set in Unity Editor.
        public GameObject MainObject;
        private PoseteacherMain poseteacher;

        // Start is called before the first frame update
        void Start()
        {
            poseteacher = MainObject.GetComponent<PoseteacherMain>();
            // TODO persistency
        }

        // Adds an Avatar to the scene.
        // Param self (in): if true self avatar, if false teacher avatar is added.
        public void AddAvatar(bool self)
        {
            poseteacher.AddAvatar(self);
        }

        // Deletes an Avatar from the scene. Every time the Avatar that was added last is deleted. The first Avatar can not be deleted.
        // Param self (in): if true self Avatar, if false teacher Avatar is deleted.
        public void RemoveAvatar(bool self)
        {
            poseteacher.DeleteAvatar(self);
        }

        // Changes the avatar type for all Avatars.
        // Param typeString (in): string representation of the TYPE for Unity Editor compatibility
        public void ChangeAvatarType(string typeString)
        {
            // Convert string to enum
            AvatarType aType = (AvatarType)Enum.Parse(typeof(AvatarType), typeString);

            poseteacher.SetAvatarTypes(aType);

        }

        // Mirrors all Avatars. 
        public void MirrorAvatars()
        {
            poseteacher.do_mirror();
        }
    }
}