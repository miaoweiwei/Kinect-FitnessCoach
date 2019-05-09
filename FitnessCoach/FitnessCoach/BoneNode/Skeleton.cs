using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
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

        private int displayWidth;
        private int displayHeight;

        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// 用于将相机空间点的Z值钳制为负的常数
        /// </summary>
        private const float InferredZPositionClamp = 0.1f;

        /// <summary>
        /// 用于绘制当前推断的骨骼的笔
        /// </summary>        
        private readonly Pen inferredBonePen = new Pen(Brushes.Gray, 1);

        /// <summary>
        /// 人体骨骼颜色列表，KInect最多识别6个人，所以共6中颜色
        /// </summary>
        public List<Pen> BodyColors = new List<Pen>()
        {
            new Pen(Brushes.Red, 3),
            new Pen(Brushes.Orange, 3),
            new Pen(Brushes.Green, 3),
            new Pen(Brushes.Blue, 3),
            new Pen(Brushes.Indigo, 3),
            new Pen(Brushes.Violet, 3),
        };

        public Skeleton(int displayWidth, int displayHeight, CoordinateMapper coordinateMapper)
        {
            this.displayWidth = displayWidth;
            this.displayHeight = displayHeight;
            this.coordinateMapper = coordinateMapper;
        }

        /// <summary>
        /// 返回骨骼字典，Key为骨骼的名字，value为骨骼的两端点
        /// </summary>
        /// <returns></returns>
        public static Dictionary<Bone, Tuple<JointType, JointType>> GetBoneDic()
        {
            Dictionary<Bone, Tuple<JointType, JointType>> dic = new Dictionary<Bone, Tuple<JointType, JointType>>()
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
            };
            return dic;
        }

        /// <summary>
        /// 返回关节节点所对应的两个骨骼的字典
        /// </summary>
        /// <returns></returns>
        public static Dictionary<JointType, Tuple<Bone, Bone>> GetJointDic()
        {
            Dictionary<JointType, Tuple<Bone, Bone>> dic = new Dictionary<JointType, Tuple<Bone, Bone>>()
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
            };
            return dic;
        }

        /// <summary>
        /// 画出多个人的骨骼框架
        /// </summary>
        public void DrawBodyArr(Body[] bodies, DrawingContext dc)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                Body body = bodies[i];
                //设置画笔
                Pen drawPen = this.BodyColors[i];
                this.DrawBody(body, dc, drawPen);
            }
        }

        /// <summary>
        /// 画出单个人的骨骼
        /// </summary>
        /// <param name="body"></param>
        /// <param name="dc"></param>
        /// <param name="pen"></param>
        public void DrawBody(Body body, DrawingContext dc, Pen pen)
        {
            if (body.IsTracked) //判断这个人是不是已经被跟踪到
            {
                this.DrawClippedEdges(body, dc); //画出边框
                //获取骨骼节点的三维坐标
                IReadOnlyDictionary<JointType, Joint> joints3 = body.Joints;
                //将关节点转换为2D深度（显示）空间
                Dictionary<JointType, Point> jointPoints = new Dictionary<JointType, Point>();
                foreach (JointType jointType in joints3.Keys) //将相机点映射到2D深度空间
                {
                    //有时，推断关节的深度（Z）可能显示为负数
                    // 限制到0.1f以防止坐标映射器返回（-Infinity，-Infinity）
                    CameraSpacePoint position = joints3[jointType].Position;
                    if (position.Z < 0) //深度为负值时
                        position.Z = InferredZPositionClamp;
                    DepthSpacePoint depthSpacePoint = this.coordinateMapper.MapCameraPointToDepthSpace(position);
                    jointPoints[jointType] = new Point(depthSpacePoint.X, depthSpacePoint.Y);
                }

                this.DrawBody(joints3, jointPoints, dc, pen);
                this.DrawBoneAngle(joints3, jointPoints, dc, Brushes.Purple);
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
                    new Rect(0, this.displayHeight - ClipBoundsThickness, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Top))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, this.displayWidth, ClipBoundsThickness));
            }

            if (clippedEdges.HasFlag(FrameEdges.Left))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(0, 0, ClipBoundsThickness, this.displayHeight));
            }

            if (clippedEdges.HasFlag(FrameEdges.Right))
            {
                drawingContext.DrawRectangle(
                    Brushes.Red,
                    null,
                    new Rect(this.displayWidth - ClipBoundsThickness, 0, ClipBoundsThickness, this.displayHeight));
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
            Dictionary<Bone, Tuple<JointType, JointType>> boneDic = GetBoneDic();
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
            //骨头字典
            Dictionary<Bone, Tuple<JointType, JointType>> boneDic = Skeleton.GetBoneDic();
            //关节字典
            Dictionary<JointType, Tuple<Bone, Bone>> jointDic = Skeleton.GetJointDic();
            //关节的角度
            Dictionary<JointType, float> bodyJointAngleDic = Skeleton.GetBodyJointAngleDic(joints3);
            foreach (KeyValuePair<JointType, float> pair in bodyJointAngleDic)
            {
                Point point = joints[pair.Key];
                // 基于设置的属性集创建格式化的文字。        
                FormattedText formattedText = new FormattedText(
                    pair.Value.ToString("f4"), //保留三位小数
                    CultureInfo.GetCultureInfo("zh-cn"), //en-us 英文 zh-cn 中文
                    FlowDirection.LeftToRight,
                    new Typeface("Verdana"),
                    5,
                    Brushes.Red);
                dc.DrawText(formattedText, point);
            }
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
            Dictionary<Bone, Tuple<JointType, JointType>> boneDic = Skeleton.GetBoneDic();
            //关节字典
            Dictionary<JointType, Tuple<Bone, Bone>> jointDic = Skeleton.GetJointDic();
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