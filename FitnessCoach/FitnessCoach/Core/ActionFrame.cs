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
            result = new List<string>();
            //模型当前帧的有关信息
            Dictionary<JointType, Joint> modelJoints = Joints.ToDictionary(o => o.JointType, value => value);
            modelJoints = Skeleton.CoordinateTransformation3D(modelJoints, JointType.SpineMid);
            //待检测的
            joints3 = Skeleton.CoordinateTransformation3D(joints3, JointType.SpineMid);
            if (jointAngleList != null) //对比角度
            {
                List<JointAngle> modelJointAngles = Skeleton.GetBodyJointAngleList(modelJoints);
                List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(joints3);
                foreach (JointType jointType in jointAngleList)
                {
                    JointAngle modelAngle = modelJointAngles.First(o => o.Name == jointType);
                    JointAngle jointAngle = jointAngles.First(o => o.Name == jointType);
                    if (Math.Abs(modelAngle.Angle - jointAngle.Angle) > angularError)
                    {
                        result.Add(
                            $"请使{BoneNode.SkeletonDictionary.GetJointNameDic()[jointType]}活动到{modelAngle.Angle:###}度");
                        return IsCompared = false;
                    }
                }
            }

            if (keyBoneList != null) //对比关键骨骼
            {
                List<KeyBone> modelKeyBones = Skeleton.GetBodyAllKeyBones(modelJoints);
                List<KeyBone> keyBones = Skeleton.GetBodyAllKeyBones(joints3);
                foreach (Bone bone in keyBoneList)
                {
                    KeyBone modelKeyBone = modelKeyBones.First(o => o.Name == bone);
                    KeyBone keyBone = keyBones.First(o => o.Name == bone);
                    if (Math.Abs(modelKeyBone.AngleX - keyBone.AngleX) > keyBoneError)
                        return IsCompared = false;
                    if (Math.Abs(modelKeyBone.AngleY - keyBone.AngleY) > keyBoneError)
                        return IsCompared = false;
                    if (Math.Abs(modelKeyBone.AngleZ - keyBone.AngleZ) > keyBoneError)
                        return IsCompared = false;
                }
            }

            return IsCompared = true;
        }

        public void Dispose()
        {
            Joints.Clear();
            Joints = null;
        }
    }
}