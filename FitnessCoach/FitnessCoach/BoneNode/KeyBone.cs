using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace FitnessCoach.BoneNode
{
    /// <summary>
    /// 关键骨骼
    /// </summary>
    public class KeyBone
    {
        /// <summary>
        /// 关键骨骼的名字
        /// </summary>
        public JointType Name { get; set; }
        /// <summary>
        /// 关键骨骼的向量
        /// </summary>
        public CameraSpacePoint Vector { get; set; }
        /// <summary>
        /// 关键骨骼与X轴的角度
        /// </summary>
        public float AngleX { get; set; }
        /// <summary>
        /// 关键骨骼与Y轴的角度
        /// </summary>
        public float AngleY { get; set; }
        /// <summary>
        /// 关键骨骼与Z轴的角度
        /// </summary>
        public float AngleZ { get; set; }
    }
}
