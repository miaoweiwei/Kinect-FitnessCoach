using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessCoach.BoneNode;
using Microsoft.Kinect;

namespace FitnessCoach.Core
{
    /// <summary>
    /// 动作模型
    /// </summary>
    public class ActionModel : IDisposable
    {
        /// <summary>
        /// 动作名称
        /// </summary>
        public string ActionName;

        /// <summary>
        /// 一个动作的连续帧
        /// </summary>
        public List<ActionFrame> ActionFrames;

        /// <summary>
        /// 允许的关节角度误差，默认20度
        /// </summary>
        public float AllowableAngularError { get; set; } = 20;

        /// <summary>
        /// 关键骨骼向量与坐标轴的三个角度的允许误差，默认20度
        /// </summary>
        public float AllowableKeyBoneError { get; set; } = 20;

        /// <summary>
        /// 关节列表,该动作的主要的关节角度，不是全身所有的关节
        /// 比如右手抬起，只需要记录右胳膊的主要关节就可以
        /// </summary>
        public List<JointType> JointAngles { get; set; }

        /// <summary>
        /// 该动作关键的骨骼
        /// </summary>
        public List<Bone> KeyBones { get; set; }

        public bool Compared(List<JointAngle> jointAngleList, List<KeyBone> keyBoneList, out RecognitionResult result)
        {
            result = new RecognitionResult();

            return true;
        }

        public void Dispose()
        {
            ActionFrames?.Clear();
            ActionFrames = null;
            JointAngles?.Clear();
            JointAngles = null;
            KeyBones?.Clear();
            KeyBones = null;
        }
    }
}