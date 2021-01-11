using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PoseTeacher
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
            // TODO: consider using centralised way for converting (adding new avatar should not break this code...)
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

            // Change all Avatars
            List<AvatarContainer> selfs = poseteacher.GetSelfAvatarContainers();
            foreach (AvatarContainer avatar in selfs)
            {
                avatar.ChangeActiveType(type);
            }

            List<AvatarContainer> teachers = poseteacher.GetTeacherAvatarContainers();
            foreach (AvatarContainer avatar in teachers)
            {
                avatar.ChangeActiveType(type);
            }
            
        }

        // Mirrors all Avatars. 
        public void MirrorAvatars()
        {
            poseteacher.do_mirror();
        }
    }
}