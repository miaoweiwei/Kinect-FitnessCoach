using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessCoach.BoneNode;
using Microsoft.Kinect;

namespace FitnessCoach.Core
{
    /// <inheritdoc />
    /// <summary>
    /// 一个姿态模型帧
    /// </summary>
    public class AttitudeModel : IDisposable
    {
        /// <summary>
        /// 该动姿态的名称
        /// </summary>
        public string AttitudeName { get; set; }

        /// <summary>
        /// 姿态保持时间
        /// </summary>
        public int Duration { get; set; } = 3000;

        /// <summary>
        /// 允许的关节角度误差，默认10度
        /// </summary>
        public float AllowableAngularError { get; set; } = 20;

        /// <summary>
        /// 关键骨骼向量与坐标轴的三个角度的允许误差，默认10度
        /// </summary>
        public float AllowableKeyBoneError { get; set; } = 20;

        /// <summary>
        /// 关节列表,该动作的主要的关节角度，不是全身所有的关节
        /// 比如右手抬起，只需要记录右胳膊的主要关节就可以
        /// </summary>
        public List<JointAngle> JointAngles { get; set; }

        /// <summary>
        /// 该动作关键的骨骼
        /// </summary>
        public List<KeyBone> KeyBones { get; set; }

        /// <summary>
        /// 模型帧与目标帧做对比
        /// </summary>
        /// <param name="jointAngleList">要对比的骨骼帧关节角度列表</param>
        /// <param name="keyBoneList">要对比的骨骼帧关键骨骼列表</param>
        /// <param name="msgInfo">对比的提示信息</param>
        /// <returns></returns>
        public bool Compared(List<JointAngle> jointAngleList, List<KeyBone> keyBoneList,out string msgInfo)
        {
            msgInfo = "";
            return Compared(jointAngleList, keyBoneList, this.AllowableAngularError, this.AllowableKeyBoneError, out msgInfo);
        }

        /// <summary>
        /// 模型帧与目标帧做对比
        /// </summary>
        /// <param name="jointAngleList">要对比的骨骼帧关节角度列表</param>
        /// <param name="keyBoneList">要对比的骨骼帧关键骨骼列表</param>
        /// <param name="allowableAngularError">允许的关节角度误差</param>
        /// <param name="allowableKeyBoneError">关键骨骼向量与坐标轴的三个角度的允许误差</param>
        /// <param name="msgInfo">对比的提示信息</param>
        /// <returns></returns>
        public bool Compared(List<JointAngle> jointAngleList, List<KeyBone> keyBoneList, float allowableAngularError,float allowableKeyBoneError, out string msgInfo)
        {
            //TODO 设计对比结果提示信息
            msgInfo = "";
            if (JointAngles != null && JointAngles.Count > 0)
            {
                if (jointAngleList == null || jointAngleList.Count <= 0)
                    return false;
                foreach (JointAngle jointAngle in JointAngles)
                    if (Math.Abs(jointAngle.Angle - jointAngleList.First(o => o.Name == jointAngle.Name).Angle) > allowableAngularError)
                        return false;
            }

            if (KeyBones == null || KeyBones.Count <= 0)
                return true;
            if (keyBoneList == null || keyBoneList.Count <= 0)
                return false;
            //对比关键骨骼的数据
            foreach (var keyBone in KeyBones)
            {
                KeyBone key = keyBoneList.First(o => o.Name == keyBone.Name);
                if (Math.Abs(keyBone.AngleX - key.AngleX) > allowableKeyBoneError)
                    return false;
                if (Math.Abs(keyBone.AngleY - key.AngleY) > allowableKeyBoneError)
                    return false;
                if (Math.Abs(keyBone.AngleZ - key.AngleZ) > allowableKeyBoneError)
                    return false;
            }
            return true;
        }

        public void Dispose()
        {
            this.JointAngles.Clear();
            this.JointAngles = null;
            this.KeyBones.Clear();
            this.KeyBones = null;
        }
    }
}