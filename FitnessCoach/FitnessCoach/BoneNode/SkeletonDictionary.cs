﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace FitnessCoach.BoneNode
{
    public static class SkeletonDictionary
    {
        private static Dictionary<Bone, Tuple<JointType, JointType>> _bondeDic;

        /// <summary>
        /// 获取骨骼字典，Key为骨骼的名字，value为骨骼的两端点，
        /// <see cref="JointType.SpineMid"/> 为身体中间部位
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Bone, Tuple<JointType, JointType>> GetBoneDic()
        {
            return _bondeDic ?? (_bondeDic = new Dictionary<Bone, Tuple<JointType, JointType>>()
            {
                /***********************左边上肢骨骼***********************/
                {Bone.FingerLeft, new Tuple<JointType, JointType>(JointType.HandTipLeft, JointType.HandLeft)},
                {Bone.PalmLeft, new Tuple<JointType, JointType>(JointType.HandLeft, JointType.WristLeft)},
                {Bone.ThumbLeft, new Tuple<JointType, JointType>(JointType.ThumbLeft, JointType.WristLeft)},
                {Bone.ArmLeft, new Tuple<JointType, JointType>(JointType.WristLeft, JointType.ElbowLeft)},
                {Bone.BigArmLeft, new Tuple<JointType, JointType>(JointType.ElbowLeft, JointType.ShoulderLeft)},
                {
                    Bone.ShoulderLeft,
                    new Tuple<JointType, JointType>(JointType.ShoulderLeft, JointType.SpineShoulder)
                },
                /***********************左边下肢骨骼***********************/
                {Bone.FootLeft, new Tuple<JointType, JointType>(JointType.FootLeft, JointType.AnkleLeft)},
                {Bone.LowerLegLeft, new Tuple<JointType, JointType>(JointType.AnkleLeft, JointType.KneeLeft)},
                {Bone.ThighLeft, new Tuple<JointType, JointType>(JointType.KneeLeft, JointType.HipLeft)},
                {Bone.HipLeft, new Tuple<JointType, JointType>(JointType.HipLeft, JointType.SpineBase)},

                /***********************右边上肢骨骼***********************/
                {Bone.FingerRight, new Tuple<JointType, JointType>(JointType.HandTipRight, JointType.HandRight)},
                {Bone.PalmRight, new Tuple<JointType, JointType>(JointType.HandRight, JointType.WristRight)},
                {Bone.ThumbRight, new Tuple<JointType, JointType>(JointType.ThumbRight, JointType.WristRight)},
                {Bone.ArmRight, new Tuple<JointType, JointType>(JointType.WristRight, JointType.ElbowRight)},
                {Bone.BigArmRight, new Tuple<JointType, JointType>(JointType.ElbowRight, JointType.ShoulderRight)},
                {
                    Bone.ShoulderRight,
                    new Tuple<JointType, JointType>(JointType.ShoulderRight, JointType.SpineShoulder)
                },
                /***********************右边下肢骨骼***********************/
                {Bone.FootRight, new Tuple<JointType, JointType>(JointType.FootRight, JointType.AnkleRight)},
                {Bone.LowerLegRight, new Tuple<JointType, JointType>(JointType.AnkleRight, JointType.KneeRight)},
                {Bone.ThighRight, new Tuple<JointType, JointType>(JointType.KneeRight, JointType.HipRight)},
                {Bone.HipRight, new Tuple<JointType, JointType>(JointType.HipRight, JointType.SpineBase)},

                /***********************躯干***********************/
                {Bone.Neck, new Tuple<JointType, JointType>(JointType.Head, JointType.Neck)},
                {Bone.NeckShoulder, new Tuple<JointType, JointType>(JointType.Neck, JointType.SpineShoulder)},
                {Bone.SpineUp, new Tuple<JointType, JointType>(JointType.SpineShoulder, JointType.SpineMid)},
                {Bone.SpineDown, new Tuple<JointType, JointType>(JointType.SpineMid, JointType.SpineBase)},
            });
        }

        private static Dictionary<JointType, Tuple<Bone, Bone>> _jointDic;

        /// <summary>
        /// 获取关节节点所对应的两个骨骼的字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<JointType, Tuple<Bone, Bone>> GetJointDic()
        {
            return _jointDic ?? (_jointDic = new Dictionary<JointType, Tuple<Bone, Bone>>()
            {
                //左臂的关节节点
                {JointType.HandLeft, new Tuple<Bone, Bone>(Bone.FingerLeft, Bone.PalmLeft)},
                {JointType.WristLeft, new Tuple<Bone, Bone>(Bone.PalmLeft, Bone.ArmLeft)},
                {JointType.ElbowLeft, new Tuple<Bone, Bone>(Bone.ArmLeft, Bone.BigArmLeft)},
                {JointType.ShoulderLeft, new Tuple<Bone, Bone>(Bone.BigArmLeft, Bone.ShoulderLeft)},
                //左腿的关节节点
                {JointType.AnkleLeft, new Tuple<Bone, Bone>(Bone.FootLeft, Bone.LowerLegLeft)},
                {JointType.KneeLeft, new Tuple<Bone, Bone>(Bone.LowerLegLeft, Bone.ThighLeft)},
                {JointType.HipLeft, new Tuple<Bone, Bone>(Bone.ThighLeft, Bone.HipLeft)},

                //右臂的关节节点
                {JointType.HandRight, new Tuple<Bone, Bone>(Bone.FingerRight, Bone.PalmRight)},
                {JointType.WristRight, new Tuple<Bone, Bone>(Bone.PalmRight, Bone.ArmRight)},
                {JointType.ElbowRight, new Tuple<Bone, Bone>(Bone.ArmRight, Bone.BigArmRight)},
                {JointType.ShoulderRight, new Tuple<Bone, Bone>(Bone.BigArmRight, Bone.ShoulderRight)},
                //右腿的关节节点
                {JointType.AnkleRight, new Tuple<Bone, Bone>(Bone.FootRight, Bone.LowerLegRight)},
                {JointType.KneeRight, new Tuple<Bone, Bone>(Bone.LowerLegRight, Bone.ThighRight)},
                {JointType.HipRight, new Tuple<Bone, Bone>(Bone.ThighRight, Bone.HipRight)},

                //躯干
                {JointType.Neck, new Tuple<Bone, Bone>(Bone.Neck, Bone.NeckShoulder)},
                {JointType.SpineMid, new Tuple<Bone, Bone>(Bone.SpineUp, Bone.SpineDown)},
            });
        }

        private static Dictionary<JointType, string> _jointNameDic;

        /// <summary>
        /// 获取关节类型的名字
        /// </summary>
        /// <returns></returns>
        public static Dictionary<JointType, string> GetJointNameDic()
        {
            return _jointNameDic ?? (_jointNameDic = new Dictionary<JointType, string>()
            {
                {JointType.SpineBase, "脊椎基地"},
                {JointType.SpineMid, "脊柱中间"},
                {JointType.Neck, "脖子"},
                {JointType.Head, "头"},
                {JointType.ShoulderLeft, "左肩膀"},
                {JointType.ElbowLeft, "左手肘"},
                {JointType.WristLeft, "左手腕"},
                {JointType.HandLeft, "左手"},
                {JointType.ShoulderRight, "右肩膀"},
                {JointType.ElbowRight, "右手肘"},
                {JointType.WristRight, "右手腕"},
                {JointType.HandRight, "右手"},
                {JointType.HipLeft, "左髋关节"},
                {JointType.KneeLeft, "左膝盖"},
                {JointType.AnkleLeft, "左踝关节"},
                {JointType.FootLeft, "左脚"},
                {JointType.HipRight, "右髋关节"},
                {JointType.KneeRight, "右膝盖"},
                {JointType.AnkleRight, "右踝关节"},
                {JointType.FootRight, "右脚"},
                {JointType.SpineShoulder, "脊椎肩膀"},
                {JointType.HandTipLeft, "左手指"},
                {JointType.ThumbLeft, "左拇指"},
                {JointType.HandTipRight, "右手指"},
                {JointType.ThumbRight, "右拇指"},
            });
        }
    }
}