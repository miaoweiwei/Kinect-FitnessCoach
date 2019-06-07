using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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

        /// <summary> 当前应该对比第几帧 </summary>
        [XmlIgnore]
        public int Index { get; private set; } = 0;

        public bool Compared(IReadOnlyDictionary<JointType, Joint> joints3, out RecognitionResult result)
        {
            return Compared(joints3, AllowableAngularError, AllowableKeyBoneError, out result);
        }

        public bool Compared(IReadOnlyDictionary<JointType, Joint> joints3, float angularError, float keyBoneError,
            out RecognitionResult result)
        {
            result = new RecognitionResult() {AttitudeName = this.ActionName, InfoMessages = new List<string>()};
            if (Index == 0)
            {
                //Dictionary<JointType, Joint> joints3 = ActionFrames[Index].Joints.ToDictionary(o => o.JointType, value => value);
                //joints3 = Skeleton.CoordinateTransformation3D(joints3, JointType.SpineMid);
                List<KeyBone> keyBones = Skeleton.GetBodyAllKeyBones(joints3);
                List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(joints3);

                return ActionFrames[Index].Compared(joints3, JointAngles, KeyBones, angularError, keyBoneError, out result.InfoMessages);
            }

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