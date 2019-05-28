using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessCoach.BoneNode;

namespace FitnessCoach.Core
{
    /// <summary>
    /// 动作模型
    /// </summary>
    public class ActionModel
    {
        /// <summary>
        /// 动作名称
        /// </summary>
        public string ActionName;

        /// <summary>
        /// 关键帧
        /// </summary>
        public List<AttitudeModel> KeyFrameAttitudeModelList;

        public bool Compared(List<JointAngle> jointAngleList, List<KeyBone> keyBoneList, out RecognitionResult result)
        {
            result = new RecognitionResult();
            for (int i = 0; i < KeyFrameAttitudeModelList.Count; i++)
            {
                if (!KeyFrameAttitudeModelList[i].IsCompared) //前面需要对比的帧还没有对比的话就去对比，然后返回false
                {
                    AttitudeModel attitude = KeyFrameAttitudeModelList[i];
                    attitude.Compared(jointAngleList, keyBoneList, out result);
                    return false;
                }
            }
            return true;
        }
    }
}