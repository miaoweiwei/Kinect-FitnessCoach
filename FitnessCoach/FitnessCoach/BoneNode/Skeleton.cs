using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    public class Skeleton
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

        private Pen _joinDefaultPen = new Pen(Brushes.Aqua, 1);
        private Pen _joinSelectPen = new Pen(Brushes.Yellow, 1);

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
        /// 获取所有关节的角度
        /// </summary>
        /// <param name="joints3">关节的三维坐标</param>
        /// <returns></returns>
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
                //angle = (float) Math.Round(angle, 2);
                
                jointAngles.Add(new JointAngle(pair.Key, angle));
            }

            return jointAngles;
        }

        /// <summary>
        /// 三维空间里的坐标转换
        /// </summary>
        /// <param name="bodyJoints"></param>
        /// <param name="jointType">指定原点坐标类型</param>
        /// <returns></returns>
        public static Dictionary<JointType, Joint> CoordinateTransformation3D(
            IReadOnlyDictionary<JointType, Joint> bodyJoints, JointType jointType)
        {
            CameraSpacePoint origin = bodyJoints[jointType].Position;
            Dictionary<JointType, Joint> joints3 = new Dictionary<JointType, Joint>();
            foreach (KeyValuePair<JointType, Joint> pair in bodyJoints)
            {
                joints3[pair.Key] = new Joint()
                {
                    JointType = pair.Value.JointType,
                    TrackingState = pair.Value.TrackingState,
                    //变换指定原点坐标，以JointType.SpineMid为坐标原点
                    Position = VectorHelp.GetSubtract(pair.Value.Position, origin),
                };
            }

            return joints3;
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
            if (!body.IsTracked)
                return;
            this.DrawClippedEdges(body, dc); //画出边框
            //获取骨骼节点的三维坐标
            IReadOnlyDictionary<JointType, Joint> bodyJoints = body.Joints;
            //var bodyJointOrientations = body.JointOrientations;
            CameraSpacePoint positionSpineMid = bodyJoints[JointType.SpineMid].Position;
            double num = positionSpineMid.Z;
            double thickness = BonePenMaxSize / num;
            //设置骨骼的画笔
            Pen boneDrawPen = new Pen(this.BodyBrushes[bodyIndex], thickness);
            //将关节点转换为2D深度（显示）空间
            Dictionary<JointType, Joint2D> joints2 = JointToJoint2Ds(bodyJoints);

            //List<Joint2D> jointList = joints2.Values.ToList();
            //string str = XmlUtil.Serializer(jointList);

            this.DrawBody(joints2, dc, boneDrawPen);

            // 指定 JointType.SpineMid 为坐标原点的三维空间坐标
            Dictionary<JointType, Joint> joints3 = CoordinateTransformation3D(bodyJoints, JointType.SpineMid);
            ControlCommand command = ControlCommand.GetControlCommand();
            command.SwitchLeader(body);
            command.HandClosedEvent(body);
            if (command.LeaderId == body.TrackingId)
            {
                dc.DrawEllipse(Brushes.White, new Pen(Brushes.Yellow, 2), joints2[JointType.Head].Position, 10,
                    10);
                this.DrawBoneAngle(joints3, joints2, dc, Brushes.White);
            }

            ////TODO 显示手
            //Size s = new Size(Resources.hand.Width, Resources.hand.Height);
            //Point p = new Point(joints2[JointType.WristRight].Position.X - s.Width / 2,
            //    joints2[JointType.WristRight].Position.Y - s.Height / 2);
            //Rect r = new Rect(p, s);
            //ImageSource image = Tools.BitmapToBitmapImage(Resources.hand);
            //dc.DrawImage(image, r);
        }

        /// <summary>
        ///把相机空间里的点映射到指定的彩色或深度空间
        /// </summary>
        /// <param name="bodyJoints"></param>
        /// <param name="isMapColor"></param>
        /// <returns></returns>
        public Dictionary<JointType, Joint2D> JointToJoint2Ds(IReadOnlyDictionary<JointType, Joint> bodyJoints,
            bool isMapColor = true)
        {
            //将关节点转换为2D深度（显示）空间
            Dictionary<JointType, Joint2D> joints2 = new Dictionary<JointType, Joint2D>();
            foreach (KeyValuePair<JointType, Joint> pair in bodyJoints)
            {
                //有时，推断关节的深度（Z）可能显示为负数
                CameraSpacePoint position = pair.Value.Position;
                if (position.Z < 0) //深度为负值时
                    position.Z = InferredZPositionClamp;
                // 将点从相机空间映射到颜色空间。
                ColorSpacePoint colorSpacePoint = CoordinateMapper.MapCameraPointToColorSpace(position);
                // 将点从相机空间映射到深度空间。
                DepthSpacePoint depthSpacePoint = CoordinateMapper.MapCameraPointToDepthSpace(position);
                Point point = new Point
                {
                    X = isMapColor ? colorSpacePoint.X : depthSpacePoint.X,
                    Y = isMapColor ? colorSpacePoint.Y : depthSpacePoint.Y
                };

                joints2[pair.Key] = new Joint2D()
                {
                    Joint2DType = pair.Key,
                    Position = point,
                    TrackingState = pair.Value.TrackingState
                };
            }

            return joints2;
        }

        public void DrawJointCircle(DrawingContext dc, Dictionary<JointType, Joint2D> joints2,
            List<JointType> selectJoint, Pen joinDefaultPen, Pen joinSelectPen, double defaultRadius,
            double selectRadius)
        {
            foreach (KeyValuePair<JointType, Joint2D> pair in joints2)
            {
                if (selectJoint != null && selectJoint.Contains(pair.Key)) //选中的关节
                    dc.DrawEllipse(joinSelectPen.Brush, joinSelectPen, pair.Value.Position, selectRadius, selectRadius);
                else //没有选中的关节
                    dc.DrawEllipse(joinDefaultPen.Brush, joinDefaultPen, pair.Value.Position, defaultRadius,
                        defaultRadius);
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
        public void DrawBody(Dictionary<JointType, Joint2D> jointPoints, DrawingContext drawingContext, Pen drawingPen)
        {
            //画出骨头
            Dictionary<Bone, Tuple<JointType, JointType>> boneDic = SkeletonDictionary.GetBoneDic();
            foreach (KeyValuePair<Bone, Tuple<JointType, JointType>> boneValuePair in boneDic)
            {
                Tuple<JointType, JointType> bone = boneValuePair.Value;
                this.DrawBone(jointPoints, bone, drawingContext, drawingPen);
            }
        }

        //画一个骨头
        private void DrawBone(Dictionary<JointType, Joint2D> jointPoints, Tuple<JointType, JointType> bone,
            DrawingContext drawingContext, Pen drawingPen)
        {
            Joint2D joint0 = jointPoints[bone.Item1];
            Joint2D joint1 = jointPoints[bone.Item2];

            // 有可能某个关节点不在相机的拍摄范围里，如果我们找不到这些关节中的任何一个，就退出
            if (joint0.TrackingState == TrackingState.NotTracked ||
                joint1.TrackingState == TrackingState.NotTracked) return;

            // 默认的画笔为用于画推断的节点的画笔
            Pen drawPen = this.inferredBonePen;
            if ((joint0.TrackingState == TrackingState.Tracked) && (joint1.TrackingState == TrackingState.Tracked))
                drawPen = drawingPen; //跟踪到了就用当前的画笔
            //连接两个节点
            drawingContext.DrawLine(drawPen, joint0.Position, joint1.Position);
        }

        /// <summary>
        /// 显示角度
        /// </summary>
        public void DrawBoneAngle(IReadOnlyDictionary<JointType, Joint> joints3, Dictionary<JointType, Joint2D> joints,
            DrawingContext dc, Brush foreground)
        {
            //关节的角度
            List<JointAngle> jointAngles = Skeleton.GetBodyJointAngleList(joints3);

            foreach (JointAngle jointAngle in jointAngles)
            {
                Point point = joints[jointAngle.Name].Position;
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
    }
}