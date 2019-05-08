using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace FitnessCoach.BoneNode
{
    /// <summary>
    /// 关节角度
    /// </summary>
    public class JointAngle
    {
        /// <summary>
        /// 关节类型名字
        /// </summary>
        public JointType Name { get; set; }
        /// <summary>
        /// 该关节的角度
        /// </summary>
        public float Angle { get; set; }
    }
}