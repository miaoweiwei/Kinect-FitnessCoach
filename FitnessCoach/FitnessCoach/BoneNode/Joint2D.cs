using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Kinect;

namespace FitnessCoach.BoneNode
{
    /// <summary>
    /// 二维空间里的骨骼节点
    /// </summary>
    public class Joint2D : IEquatable<Joint2D>
    {
        /// <summary>
        /// 骨骼节点的类型
        /// </summary>
        public JointType Joint2DType;

        /// <summary>
        /// 二维空间的坐标
        /// </summary>
        public Point Position;

        /// <summary>
        /// <summary>
        /// 跟踪状态,这个点是推断的还是跟踪的或者识别不到的
        /// </summary>
        public TrackingState TrackingState;

        public bool Equals(Joint2D joint)
        {
            if (this.Joint2DType == joint.Joint2DType)
            {
                Point position = joint.Position;
                Point point2 = this.Position;
                if (point2.Equals(position) && (this.TrackingState == joint.TrackingState))
                {
                    return true;
                }
            }

            return false;
        }

        public sealed override bool Equals(object obj)
        {
            return obj is Joint2D o && Equals(o);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Joint2D a, Joint2D b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Joint2D a, Joint2D b)
        {
            return !(a.Equals(b));
        }
    }
}