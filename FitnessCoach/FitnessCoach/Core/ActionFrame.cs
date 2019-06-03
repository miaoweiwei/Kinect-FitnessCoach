using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using FitnessCoach.BoneNode;
using Microsoft.Kinect;

namespace FitnessCoach.Core
{
    public class ActionFrame : IDisposable
    {
        /// <summary>
        /// 动作帧的序号
        /// </summary>
        public int Index;
        /// <summary>
        /// 骨骼节点序列
        /// </summary>
        public List<Joint> Joints;
        /// <summary>
        /// 是否已经正确对比
        /// </summary>
        [XmlIgnore] //不序列化
        public bool IsCompared;

        public bool Compared(ActionFrame action, float angularError, float keyBoneError,
            List<JointAngle> jointAngleList, List<KeyBone> keyBoneList, out List<string> result)
        {
            IsCompared = Compared(action, angularError, keyBoneError, jointAngleList, out result);
            if (keyBoneList == null)
                return IsCompared;

            Dictionary<JointType, Joint> joints3 = GetJointDictionary(action.Joints);
            List<KeyBone> keyBones = Skeleton.GetBodyAllKeyBones(joints3);

            if (IsCompared) //对比关键骨骼
            {
                foreach (JointAngle jointAngle in jointAngleList)
                {
                }
            }

            return IsCompared;
        }

        public bool Compared(ActionFrame action, float angularError, float keyBoneError,
            List<JointAngle> jointAngleList, out List<string> result)
        {
            result = new List<string>();
            Dictionary<JointType, Joint> joints3 = GetJointDictionary(action.Joints);
            //关节的角度
            List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(joints3);
            
            bool IsCompared = true;
            return IsCompared;
        }

        private Dictionary<JointType, Joint> GetJointDictionary(List<Joint> joints)
        {
            Dictionary<JointType, Joint> newJoints = new Dictionary<JointType, Joint>();
            foreach (Joint joint in joints)
                newJoints.Add(joint.JointType, joint);
            newJoints = Skeleton.CoordinateTransformation3D(newJoints, JointType.SpineMid);
            return newJoints;
        }

        public void Dispose()
        {
            Joints.Clear();
            Joints = null;
        }
    }
}