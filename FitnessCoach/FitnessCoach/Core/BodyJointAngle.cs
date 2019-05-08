using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitnessCoach.BoneNode;

namespace FitnessCoach.Core
{
    /// <summary>
    /// 骨骼关节角度模型帧
    /// </summary>
    internal class BodyJointAngle
    {
        /// <summary>
        /// 该动作帧的名称
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// 允许的关节角度误差
        /// </summary>
        public float AllowableAngularError { get; set; }

        /// <summary>
        /// 关键骨骼向量与坐标轴的三个角度的允许误差
        /// </summary>
        public float AllowableKeyAngularError { get; set; }

        /// <summary>
        /// 关节列表
        /// </summary>
        public List<JointAngle> JointAngles { get; set; }

        /// <summary>
        /// 关键骨骼
        /// </summary>
        public List<KeyBone> KeyBones { get; set; }

        public bool Compared(List<JointAngle> jointAngleList, List<KeyBone> keyBoneList)
        {
            for (int i = 0; i < this.JointAngles.Count; i++)
            {
                float angle = jointAngleList.First(o => o.Name == JointAngles[i].Name).Angle;
                if (JointAngles[i].Angle - angle >= this.AllowableAngularError)
                    return false;
            }

            //对比关键骨骼的数据
            for (int i = 0; i < this.KeyBones.Count; i++)
            {
                float anglex = keyBoneList.First(o => o.Name == KeyBones[i].Name).AngleX;
                float angley = keyBoneList.First(o => o.Name == KeyBones[i].Name).AngleY;
                float anglez = keyBoneList.First(o => o.Name == KeyBones[i].Name).AngleZ;

                if (KeyBones[i].AngleX - anglex >= this.AllowableKeyAngularError)
                    return false;
                if (KeyBones[i].AngleY - angley >= this.AllowableKeyAngularError)
                    return false;
                if (KeyBones[i].AngleZ - anglez >= this.AllowableKeyAngularError)
                    return false;
            }

            return true;
        }
    }
}