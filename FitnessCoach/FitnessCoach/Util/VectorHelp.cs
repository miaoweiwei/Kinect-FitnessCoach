using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Kinect;

namespace FitnessCoach.Util
{
    /// <summary>
    /// 三维向量(用 <see cref="CameraSpacePoint"/> 表示三维向量的类型)帮助类，
    /// 进行向量之间的计算
    /// </summary>
    internal static class VectorHelp
    {
        /// <summary>
        /// 获取两个三维的点组成的向量,
        /// 如向量AB
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static CameraSpacePoint GetVector(CameraSpacePoint A, CameraSpacePoint B)
        {
            return VectorHelp.GetSubtract(B, A);
        }

        /// <summary>
        /// 获取三维空间中两个点的之差，
        /// 例如 A-B（表示
        /// X = A.X - B.X,
        /// Y = A.Y - B.Y,
        /// Z = A.Z - B.Z）
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        /// <returns></returns>
        public static CameraSpacePoint GetSubtract(CameraSpacePoint A, CameraSpacePoint B)
        {
            return new CameraSpacePoint
            {
                X = A.X - B.X,
                Y = A.Y - B.Y,
                Z = A.Z - B.Z
            };
        }

        /// <summary>
        /// 返回向量的长度
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static float GetVectorLength(CameraSpacePoint vector)
        {
            float length = 0;
            double datX = vector.X * vector.X;
            double datY = Math.Pow((double) vector.Y, 2);
            double datZ = Math.Pow((double) vector.Z, 2);
            length = (float) Math.Sqrt(datX + datY + datZ);
            return length;
        }

        /// <summary>
        /// 计算向量之间的夹角
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="isAngle"></param>
        /// <returns></returns>
        public static float GetVectorAngle(CameraSpacePoint vector1, CameraSpacePoint vector2, bool isAngle = true)
        {
            double pointMultiply = vector1.X * vector2.X + vector1.Y * vector2.Y + vector1.Z * vector2.Z;
            double lengthMultiply = GetVectorLength(vector1) * GetVectorLength(vector2);
            double cosAngle = pointMultiply / lengthMultiply;

            double angle = Math.Acos(cosAngle);
            angle = isAngle ? angle * 180 / Math.PI : angle;
            return (float) angle;
        }

        /// <summary>
        /// 获取向量的单位向量
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        public static CameraSpacePoint GetUnitVector(CameraSpacePoint vector)
        {
            CameraSpacePoint unitVector = new CameraSpacePoint();
            float vectorLength = VectorHelp.GetVectorLength(vector);
            if (vectorLength > 0)
            {
                unitVector.X = vector.X / vectorLength;
                unitVector.Y = vector.Y / vectorLength;
                unitVector.Z = vector.Z / vectorLength;
            }

            return unitVector;
        }

        /// <summary> 获取两点之间的距离 </summary>
        public static double GetDistance(CameraSpacePoint a, CameraSpacePoint b)
        {
            var array = new double[]
            {
                b.X - a.X,
                b.Y - a.Y,
                b.Z - a.Z
            };
            return Math.Sqrt(array[0] * array[0] + array[1] * array[1] + array[2] * array[2]);
        }
    }
}