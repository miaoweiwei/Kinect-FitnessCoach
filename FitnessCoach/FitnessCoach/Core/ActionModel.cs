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
        /// 最后一帧的保持时间
        /// </summary>
        public int LastFrameDurationTime { get; set; } = 5;

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

        /// <summary>
        /// 是否已经正确对比动作
        /// </summary>
        [XmlIgnore] //不序列化
        public bool IsCompared { get; private set; } = false;

        /// <summary>
        /// 恢复动作模型对比结果
        /// </summary>
        public void ResetDefault()
        {
            Index = 0;
            IsCompared = false;
            ActionFrames?.ForEach(o => o.IsCompared = false);
        }

        public bool Compared(IReadOnlyDictionary<JointType, Joint> joints3, out RecognitionResult result)
        {
            return Compared(joints3, AllowableAngularError, AllowableKeyBoneError, out result);
        }

        private DateTime starTime;

        public bool Compared(IReadOnlyDictionary<JointType, Joint> joints3, float angularError, float keyBoneError,
            out RecognitionResult result)
        {
            result = new RecognitionResult() {AttitudeName = this.ActionName, InfoMessages = new List<string>()};
            if (0 < Index && Index < ActionFrames.Count - 1)
            {
                bool temp = ActionFrames[Index].Compared(joints3, JointAngles, KeyBones, angularError, keyBoneError,
                    out result.InfoMessages);
                if (Index % 20 == 0)
                {
                    result.InfoMessages.Clear();
                    result.InfoMessages.Add($"动作已完成{(Index * 0.1 / ActionFrames.Count * 100).ToString("####")}%,加油！");
                }

                Index++;
                return temp;
            }

            if (Index == 0 && !ActionFrames[Index].IsCompared)
            {
                bool temp = ActionFrames[Index].Compared(joints3, JointAngles, KeyBones, angularError, keyBoneError,
                    out result.InfoMessages);
                if (temp)
                {
                    Index++;
                    return true;
                }

                result.InfoMessages.Clear();
                result.InfoMessages.Add("请做好准备动作");
                return false;
            }

            if (Index == ActionFrames.Count - 1)
            {
                if (!ActionFrames[Index].IsCompared)
                {
                    bool temp = ActionFrames[Index].Compared(joints3, JointAngles, KeyBones, angularError, keyBoneError,
                        out result.InfoMessages);
                    if (temp)
                    {
                        starTime = DateTime.Now;
                    }

                    return temp;
                }
                else
                {
                    bool temp = ActionFrames[Index].Compared(joints3, JointAngles, KeyBones, angularError, keyBoneError,
                        out result.InfoMessages);
                    if (temp)
                    {
                        if ((DateTime.Now - starTime).Milliseconds >= 5000)
                        {
                            result.InfoMessages.Clear();
                            result.InfoMessages.Add("动作已经对比完成");
                            IsCompared = true;
                            return true;
                        }

                        result.InfoMessages.Clear();
                        result.InfoMessages.Add("请保持5秒，坚持住！");
                        return false;
                    }
                    else
                    {
                        starTime = DateTime.Now;
                        return false;
                    }
                }
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