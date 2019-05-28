using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FitnessCoach.Properties;
using FitnessCoach.Util;
using Microsoft.Kinect;
using Brushes = System.Windows.Media.Brushes;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;

namespace FitnessCoach.BoneNode
{
    /// <summary>
    /// 骨骼
    /// </summary>
    internal class Skeleton
    {
        /// <summary>
        ///  边缘矩形的厚度
        /// </summary>
        private const double ClipBoundsThickness = 3;

        /// <summary>
        /// 用于将相机空间点的Z值钳制为负的常数
        /// </summary>
        private const float InferredZPositionClamp = 0.3f;

        /// <summary>
        /// 用于绘制当前推断的骨骼的笔
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// 显示骨骼时的页面宽度
        /// </summary>
        public double DisplayWidth { get; set; }

        /// <summary>
        /// 显示骨骼时的页面高度
        /// </summary>
        public double DisplayHeight { get; set; }

        /// <summary>
        /// 骨骼的坐标系
        /// </summary>
        public CoordinateMapper CoordinateMapper { get; set; } = null;

        /// <summary>
        /// 骨骼的最大宽度
        /// </summary>
        public double BonePenMaxSize { get; set; } = 20;

        /// <summary>
        /// 人体骨骼颜色列表，Kinect最多识别6个人，所以共6中颜色
        /// </summary>
        public SolidColorBrush[] BodyBrushes { get; set; } = new SolidColorBrush[]
        {
            Brushes.Red,
            Brushes.Orange,
            Brushes.Green,
            Brushes.Blue,
            Brushes.Indigo,
            Brushes.Violet,
        };

        public Skeleton(int displayWidth, int displayHeight, CoordinateMapper coordinateMapper)
        {
            this.DisplayWidth = displayWidth;
            this.DisplayHeight = displayHeight;
            this.CoordinateMapper = coordinateMapper;
        }

        /// <summary>
        /// 获取每个骨骼分别与坐标系的夹角（X,Y,Z轴的夹角）
        /// </summary>
        /// <param name="joints3"></param>
        /// <returns></returns>
        public static List<KeyBone> GetBodyAllKeyBones(IReadOnlyDictionary<JointType, Joint> joints3)
        {
            var boneDic = SkeletonDictionary.GetBoneDic();
            var jointDic = SkeletonDictionary.GetJointDic();
            List<KeyBone> keyBones = new List<KeyBone>();
            foreach (Bone bone in SkeletonDictionary.GetBoneDic().Keys)
            {
                CameraSpacePoint point1 = joints3[boneDic[bone].Item1].Position;
                CameraSpacePoint point2 = joints3[boneDic[bone].Item2].Position;
                CameraSpacePoint vector = VectorHelp.GetVector(point1, point2);
                KeyBone keyBone = new KeyBone
                {
                    Name = bone,
                    Vector = vector,
                    AngleX = VectorHelp.GetVectorAngle(vector, new CameraSpacePoint() {X = 1, Y = 0, Z = 0}),
                    AngleY = VectorHelp.GetVectorAngle(vector, new CameraSpacePoint() {X = 0, Y = 1, Z = 0}),
                    AngleZ = VectorHelp.GetVectorAngle(vector, new CameraSpacePoint() {X = 0, Y = 0, Z = 1})
                };
                keyBones.Add(keyBone);
            }

            return keyBones;
        }


        /// <summary>
        /// 画出多个人的骨骼框架
        /// </summary>
        public void DrawBodyArr(Body[] bodies, DrawingContext dc)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                Body body = bodies[i];
                this.DrawBody(body, i, dc);
            }
        }

        /// <summary>
        /// 画出单个人的骨骼
        /// </summary>
        /// <param name="body"></param>
        /// <param name="bodyIndex"></param>
        /// <param name="dc"></param>
        public void DrawBody(Body body, int bodyIndex, DrawingContext dc)
        {
            if (body.IsTracked) //判断这个人是不是已经被跟踪到
            {
                this.DrawClippedEdges(body, dc); //画出边框
                //获取骨骼节点的三维坐标
                IReadOnlyDictionary<JointType, Joint> bodyJoints = body.Joints;
                //var bodyJointOrientations = body.JointOrientations;
                CameraSpacePoint positionSpineMid = bodyJoints[JointType.SpineMid].Position;
                double num = positionSpineMid.Z;
                //设置骨骼的画笔
                Pen boneDrawPen = new Pen(this.BodyBrushes[bodyIndex], this.BonePenMaxSize / num);

                //将关节点转换为2D深度（显示）空间
                Dictionary<JointType, Point> joints2 = new Dictionary<JointType, Point>();
                // 指定 JointType.SpineMid 为坐标原点的三维空间坐标
                Dictionary<JointType, Joint> joints3 = new Dictionary<JointType, Joint>();

                foreach (KeyValuePair<JointType, Joint> pair in bodyJoints)
                {
                    //有时，推断关节的深度（Z）可能显示为负数
                    CameraSpacePoint position = pair.Value.Position;
                    if (position.Z < 0) //深度为负值时
                        position.Z = InferredZPositionClamp;
                    ColorSpacePoint colorSpacePoint = CoordinateMapper.MapCameraPointToColorSpace(position);
                    joints2[pair.Key] = new Point(colorSpacePoint.X, colorSpacePoint.Y);
                    Joint jointTemp = new Joint()
                    {
                        JointType = pair.Value.JointType,
                        TrackingState = pair.Value.TrackingState,
                        //变换指定原点坐标，以JointType.SpineMid为坐标原点
                        Position = VectorHelp.GetSubtract(pair.Value.Position, positionSpineMid),
                    };
                    joints3.Add(pair.Key, jointTemp);
                }

                this.DrawBody(joints3, joints2, dc, boneDrawPen);
                this.DrawBoneAngle(joints3, joints2, dc, Brushes.White);

                //TODO 显示手
                Size s = new Size(Resources.hand.Width, Resources.hand.Height);
                Point p = new Point(joints2[JointType.WristRight].X - s.Width / 2, joints2[JointType.WristRight].Y - s.Height / 2);
                Rect r = new Rect(p, s);
                ImageSource image = Tools.BitmapToBitmapImage(Resources.hand);
                dc.DrawImage(image, r);
            }
        }
        
        /// <summary>
        /// 画边框
        /// </summary>
        /// <param name="body"></param>
        /// <param name="drawingContext"></param>
        public void DrawClippedEdges(Body body, DrawingContext drawingContext)
        {
            //获取剪切正文的视野边缘。
            FrameEdges clippedEdges = body.ClippedEdges;

            //框架边框的边缘类型。
            if (clippedEdges.HasFlag(FrameEdges.Bottom))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, this.DisplayHeight - ClipBoundsThickness, this.DisplayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.DisplayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.DisplayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.DisplayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.DisplayHeight));
            }
        }

        /// <summary>
        /// 画一个人的骨架
        /// </summary>
        /// <param name="joints3"></param>
        /// <param name="jointPoints"></param>
        /// <param name="drawingContext"></param>
        /// <param name="drawingPen"></param>
        private void DrawBody(IReadOnlyDictionary<JointType, Joint> joints3, IDictionary<JointType, Point> jointPoints,
            DrawingContext drawingContext, Pen drawingPen)
        {
            //画出骨头
            Dictionary<Bone, Tuple<JointType, JointType>> boneDic = SkeletonDictionary.GetBoneDic();
            foreach (KeyValuePair<Bone, Tuple<JointType, JointType>> boneValuePair in boneDic)
            {
                Tuple<JointType, JointType> bone = boneValuePair.Value;
                this.DrawBone(joints3, jointPoints, bone.Item1, bone.Item2, drawingContext, drawingPen);
            }
        }

        //画一个骨头
        private void DrawBone(IReadOnlyDictionary<JointType, Joint> joints3, IDictionary<JointType, Point> jointPoints,
            JointType jointType0, JointType jointType1, DrawingContext drawingContext, Pen drawingPen)
        {
            Joint joint0 = joints3[jointType0];
            Joint joint1 = joints3[jointType1];

            // 有可能某个关节点不在相机的拍摄范围里，如果我们找不到这些关节中的任何一个，就退出
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked) return;

            // 默认的画笔为用于画推断的节点的画笔
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
                drawPen = drawingPen; //跟踪到了就用当前的画笔
            //连接两个节点
            drawingContext.DrawLine(drawPen, jointPoints[jointType0], jointPoints[jointType1]);
        }

        /// <summary>
        /// 显示角度
        /// </summary>
        public void DrawBoneAngle(IReadOnlyDictionary<JointType, Joint> joints3, IDictionary<JointType, Point> joints,
            DrawingContext dc, Brush foreground)
        {
            //关节的角度
            List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(joints3);

            foreach (JointAngle jointAngle in jointAngles)
            {
                Point point = joints[jointAngle.Name];
                // 基于设置的属性集创建格式化的文字。        
                FormattedText formattedText = new FormattedText(
                    jointAngle.Angle.ToString("f4"), //保留三位小数
                    CultureInfo.GetCultureInfo("zh-cn"), //en-us 英文 zh-cn 中文
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    10,
                    foreground);
                dc.DrawText(formattedText, point);
            }
        }

        public static List<JointAngle> GetBodyJointAngleList(IReadOnlyDictionary<JointType, Joint> joints3)
        {
            List<JointAngle> jointAngles = new List<JointAngle>();
            //骨头字典
            Dictionary<Bone, Tuple<JointType, JointType>> boneDic = SkeletonDictionary.GetBoneDic();
            //关节字典
            Dictionary<JointType, Tuple<Bone, Bone>> jointDic = SkeletonDictionary.GetJointDic();

            foreach (KeyValuePair<JointType, Tuple<Bone, Bone>> pair in jointDic)
            {
                Tuple<JointType, JointType> bondVectorTuple1 = boneDic[pair.Value.Item1];
                Tuple<JointType, JointType> bondVectorTuple2 = boneDic[pair.Value.Item2];

                CameraSpacePoint vector1 = VectorHelp.GetVector(joints3[bondVectorTuple1.Item1].Position,
                    joints3[bondVectorTuple1.Item2].Position);
                CameraSpacePoint vector2 = VectorHelp.GetVector(joints3[bondVectorTuple2.Item2].Position,
                    joints3[bondVectorTuple2.Item1].Position);

                float angle = VectorHelp.GetVectorAngle(vector1, vector2);
                jointAngles.Add(new JointAngle(pair.Key, angle));
            }

            return jointAngles;
        }

        /// <summary>
        /// 计算关节的角度
        /// </summary>
        /// <param name="joints3">关节的三维坐标</param>
        /// <returns></returns>
        public static Dictionary<JointType, float> GetBodyJointAngleDic(IReadOnlyDictionary<JointType, Joint> joints3)
        {
            Dictionary<JointType, float> jointAngleDic = new Dictionary<JointType, float>();
            //骨头字典
            Dictionary<Bone, Tuple<JointType, JointType>> boneDic = SkeletonDictionary.GetBoneDic();
            //关节字典
            Dictionary<JointType, Tuple<Bone, Bone>> jointDic = SkeletonDictionary.GetJointDic();
            foreach (KeyValuePair<JointType, Tuple<Bone, Bone>> pair in jointDic)
            {
                Tuple<JointType, JointType> bondVectorTuple1 = boneDic[pair.Value.Item1];
                Tuple<JointType, JointType> bondVectorTuple2 = boneDic[pair.Value.Item2];

                CameraSpacePoint vector1 = VectorHelp.GetVector(joints3[bondVectorTuple1.Item1].Position,
                    joints3[bondVectorTuple1.Item2].Position);
                CameraSpacePoint vector2 = VectorHelp.GetVector(joints3[bondVectorTuple2.Item2].Position,
                    joints3[bondVectorTuple2.Item1].Position);

                float angle = VectorHelp.GetVectorAngle(vector1, vector2);
                jointAngleDic.Add(pair.Key, angle);
            }

            return jointAngleDic;
        }
    }
}