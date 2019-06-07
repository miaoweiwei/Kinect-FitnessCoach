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

        public bool Compared(IReadOnlyDictionary<JointType, Joint> joints3, List<JointType> jointAngleList,
            List<Bone> keyBoneList, float angularError, float keyBoneError, out List<string> result)
        {
            IsCompared = Compared(joints3,jointAngleList, angularError, keyBoneError, out result);
            return IsCompared;
        }

        public bool Compared(IReadOnlyDictionary<JointType, Joint> joints3,List<JointType> jointAngleList, float angularError, float keyBoneError,
            out List<string> result)
        {
            result = new List<string>();

            //模型当前帧的有关信息
            Dictionary<JointType, Joint> modelJoints = Joints.ToDictionary(o => o.JointType, value => value);
            modelJoints = Skeleton.CoordinateTransformation3D(modelJoints, JointType.SpineMid);
            List<KeyBone> modelKeyBones = Skeleton.GetBodyAllKeyBones(modelJoints);
            List<JointAngle> modelJointAngles = Skeleton.GetBodyJointAngleList(modelJoints);

            //待检测的
            joints3 = Skeleton.CoordinateTransformation3D(joints3, JointType.SpineMid);
            List<KeyBone> keyBones = Skeleton.GetBodyAllKeyBones(joints3);
            List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(joints3);


            foreach (JointType jointType in jointAngleList)
            {
                
            }

            
            return IsCompared;
        }

        public void Dispose()
        {
            Joints.Clear();
            Joints = null;
        }
    }
}