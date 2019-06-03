﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using FitnessCoach.BoneNode;
using FitnessCoach.Config;
using FitnessCoach.Core;
using FitnessCoach.Util;
using Microsoft.Kinect;
using Microsoft.Win32;
using Timer = System.Timers.Timer;

namespace ActionRecording
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(MainWindow).FullName);
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private List<Joint2D> templateJoint2Ds;

        private ActionModel actionModel;

        private List<JointType> recordingJointTypes; //记录用于对比的关节
        private List<Bone> recordingBones; //记录用于对比的骨骼

        private bool isRecording; //是否开始录制
        private ulong tracked;

        private Timer timer;
        private int triggerCount = 5;
        private string infomsg = "";

        #region UI有关的属性

        private DrawingGroup templateDrawingGroup;

        /// <summary> 绘制模板骨骼图像 </summary>
        private DrawingImage _templateImageSource;

        /// <summary>
        /// 用于体渲染输出的绘图组
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary> 绘制骨骼图像 </summary>
        private DrawingImage _bodyImageSource;

        /// <summary> 模板骨骼图 </summary>
        public DrawingImage TemplateImageSource
        {
            get => _templateImageSource;
        }

        /// <summary> 绘制骨骼图 </summary>
        public DrawingImage BodyImageSource
        {
            get => _bodyImageSource;
        }

        private Brush _bodyBackGroundColor = Brushes.Transparent;
        private Brush _templateBackGroundColor = Brushes.Transparent;
        private Pen _bonePen = new Pen(Brushes.Red, 3);
        private Pen _boneSelectPen = new Pen(Brushes.BlueViolet, 4);

        private Pen _joinDefaultPen = new Pen(Brushes.Aqua, 1);
        private Pen _joinSelectPen = new Pen(Brushes.Yellow, 1);

        #endregion

        #region 有关Kinect的字段

        private KinectSensor kinectSensor;

        /// <summary>
        /// 显示宽度（深度空间）
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// 显示高度（深度空间）
        /// </summary>
        private int displayHeight;


        /// <summary>
        /// 坐标转化器，可以把三维坐标转成二维的坐标
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// 骨骼框架阅读器
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// 人体数组
        /// </summary>
        private Body[] bodies = null;

        private Skeleton skeleton;

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            InitUi();
            InitKinect();
            DataContext = this;
        }

        private void InitUi()
        {
            templateDrawingGroup = new DrawingGroup();
            _templateImageSource = new DrawingImage(templateDrawingGroup);

            drawingGroup = new DrawingGroup();
            _bodyImageSource = new DrawingImage(drawingGroup);

            templateJoint2Ds = LoadTemplateModel(GlobalConfig.TemplateModelPath);
            saveFileDialog = new SaveFileDialog
            {
                Title = @"保存工件的配置",
                Filter = @"模型文件（*.model）|*.model|XML文件（*.xml）|*.xml|文本文件（*.txt）|*.txt|所有文件（*.*）|*.*",
                DefaultExt = @"模型文件（*.model）|*.model",
                InitialDirectory = Environment.CurrentDirectory,
                RestoreDirectory = true,
                OverwritePrompt = true,
                AddExtension = true,
            };
            openFileDialog = new OpenFileDialog
            {
                Title = @"加载工件的配置",
                Filter = @"模型文件（*.model）|*.model|XML文件（*.xml）|*.xml|文本文件（*.txt）|*.txt|所有文件（*.*）|*.*",
                DefaultExt = @"模型文件（*.model）|*.model",
                CheckFileExists = true,
                Multiselect = false,
                InitialDirectory = Environment.CurrentDirectory,
                RestoreDirectory = true,
                CheckPathExists = true,
            };

            timer = new Timer {Interval = 1000, AutoReset = true};
            timer.Elapsed += Timer_Elapsed;
        }

        private void InitKinect()
        {
            kinectSensor = GlobalConfig.Sensor;

            //获取坐标映射器
            coordinateMapper = kinectSensor.CoordinateMapper;
            //FrameDescription frameDescription = kinectSensor.BodyIndexFrameSource.FrameDescription;
            FrameDescription coloDescription = kinectSensor.ColorFrameSource.FrameDescription; //获取彩色图的分辨率
            //得到关节空间的大小
            displayWidth = coloDescription.Width;
            displayHeight = coloDescription.Height;
            //设置骨骼空间的大小与彩色图像的一致，避免骨骼和彩色任务图像存在偏差
            skeleton = new Skeleton(displayWidth, displayHeight, coordinateMapper);
            //为身体骨骼打开阅读器
            bodyFrameReader = kinectSensor.BodyFrameSource.OpenReader();
        }

        private void KinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            string status = kinectSensor.IsAvailable
                ? "正在运行"
                : "Kinect不可用!";
            SetStateText(status);
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null) //重新初始化身体数组
                {
                    if (bodies == null || bodies.Length < bodyFrame.BodyCount)
                        bodies = new Body[bodyFrame.BodyCount];
                    bodyFrame.GetAndRefreshBodyData(bodies); //接收身体骨骼信息
                    dataReceived = true; //数据接收成功
                }
            }

            if (!dataReceived) return;
            using (DrawingContext dc = drawingGroup.Open())
            {
                Rect r = new Rect(0.0, 0.0, displayWidth, displayHeight);
                dc.DrawRectangle(_bodyBackGroundColor, null, r);
                for (int i = 0; i < bodies.Length; i++)
                {
                    Body body = bodies[i];
                    skeleton.DrawBody(body, i, dc);
                    if (tracked == 0)
                        tracked = body.IsTracked ? body.TrackingId : 0;

                    if (body.IsTracked && body.TrackingId == tracked && actionModel != null && isRecording)
                    {
                        if (actionModel.ActionFrames == null)
                            actionModel.ActionFrames = new List<ActionFrame>();
                        actionModel.ActionFrames.Add(new ActionFrame()
                        {
                            Index = actionModel.ActionFrames.Count,
                            Joints = body.Joints.Values.ToList()
                        });
                    }

                    Dictionary<JointType, Joint2D> joint2Ds = skeleton.JointToJoint2Ds(body.Joints);
                    //画关节点的圆
                    foreach (KeyValuePair<JointType, Joint2D> pair in joint2Ds)
                    {
                        if (recordingJointTypes != null && recordingJointTypes.Contains(pair.Value.Joint2DType)) //选中的关节
                            dc.DrawEllipse(_joinSelectPen.Brush, _joinSelectPen, pair.Value.Position, 6, 6);
                        else //没有选中的关节
                            dc.DrawEllipse(_joinDefaultPen.Brush, _joinDefaultPen, pair.Value.Position, 5, 5);
                    }
                }

                //画出骨头
                Dictionary<Bone, Tuple<JointType, JointType>> boneDic = SkeletonDictionary.GetBoneDic();
                foreach (KeyValuePair<Bone, Tuple<JointType, JointType>> boneValuePair in boneDic)
                {
                    Tuple<JointType, JointType> bone = boneValuePair.Value;
                    Joint2D joint0 = templateJoint2Ds.First(o => o.Joint2DType == bone.Item1);
                    Joint2D joint1 = templateJoint2Ds.First(o => o.Joint2DType == bone.Item2);
                    //连接两个节点
                    if (recordingJointTypes != null && recordingBones.Contains(boneValuePair.Key))
                        dc.DrawLine(_boneSelectPen, joint0.Position, joint1.Position);
                    else
                        dc.DrawLine(_bonePen, joint0.Position, joint1.Position);
                }

                foreach (Joint2D joint2D in templateJoint2Ds)
                {
                    if (recordingJointTypes != null && recordingJointTypes.Contains(joint2D.Joint2DType)) //选中的关节
                        dc.DrawEllipse(_joinSelectPen.Brush, _joinSelectPen, joint2D.Position, 6, 6);
                    else //没有选中的关节
                        dc.DrawEllipse(_joinDefaultPen.Brush, _joinDefaultPen, joint2D.Position, 5, 5);
                }

                if (!string.IsNullOrEmpty(infomsg))
                    SetPromptInfo(dc, infomsg);
                drawingGroup.ClipGeometry = new RectangleGeometry(r);
            }
        }

        private void KinectEventBind()
        {
            kinectSensor.IsAvailableChanged += KinectSensor_IsAvailableChanged;
            if (bodyFrameReader != null)
                bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
        }

        private void KinectEventCancelBind()
        {
            kinectSensor.IsAvailableChanged -= KinectSensor_IsAvailableChanged;
            if (bodyFrameReader != null)
                bodyFrameReader.FrameArrived -= BodyFrameReader_FrameArrived;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            KinectEventCancelBind();
            if (kinectSensor == null) return;
            kinectSensor.Close();
            kinectSensor = null;
        }

        #region 设置UI

        private void SetStatusBarText(TextBlock textBlock, string msg)
        {
            if (!AppStatusBar.IsVisible)
                AppStatusBar.Visibility = Visibility.Visible;
            textBlock.Text = msg;
        }

        private void SetStateText(string msg)
        {
            SetStatusBarText(StateText, msg);
        }

        private void SetDurationText(string time)
        {
            SetStatusBarText(DurationText, $"时长：{time}");
        }

        private void SetFrameCountText(string frameCount)
        {
            SetStatusBarText(FrameCountText, $"帧数：{frameCount}");
        }

        private void LabBtnModel_OnMouseEnter(object sender, MouseEventArgs e)
        {
            ((Control) sender).Background = Resources["SelectedBackGroundColor"] as Brush;
        }

        private void LabBtnModel_OnMouseLeave(object sender, MouseEventArgs e)
        {
            ((Control) sender).Background = Resources["LabBtnBackGroundColor"] as Brush;
        }

        /// <summary>
        /// 切换显示图层
        /// </summary>
        private void SwitchShowGrid(params UIElement[] elements)
        {
            SavePage.Visibility = Visibility.Collapsed;
            BodyViewBox.Visibility = Visibility.Collapsed;
            StartPageBorder.Visibility = Visibility.Collapsed;
            TemplateViewBox.Visibility = Visibility.Collapsed;
            ModelToolBar.Visibility = Visibility.Collapsed;
            SaveResultPage.Visibility = Visibility.Collapsed;
            foreach (UIElement element in elements)
            {
                element.Visibility = Visibility.Visible;
            }
        }

        private void BackStartPage()
        {
            KinectEventCancelBind();
            kinectSensor.Close();
            recordingJointTypes?.Clear();
            recordingBones?.Clear();
            actionModel?.Dispose();
            actionModel = null;
            BtnRecording.Content = "开始录制";
            isRecording = false;
            SwitchShowGrid(StartPageBorder);
        }

        #endregion

        private void LabBtnOpenModel_OnClick(object sender, RoutedEventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if (result != true) return;
            string filePath = openFileDialog.FileName;
            //TODO 打开模型
            ModelToolBar.Visibility = Visibility.Visible;
        }

        #region 新建模型

        //加载模型
        private void OpenTemplate()
        {
            using (DrawingContext dc = templateDrawingGroup.Open())
            {
                Rect r = new Rect(0.0, 0.0, 400, 500);
                dc.DrawRectangle(_templateBackGroundColor, null, r);
                //画出骨头
                Dictionary<Bone, Tuple<JointType, JointType>> boneDic = SkeletonDictionary.GetBoneDic();
                foreach (KeyValuePair<Bone, Tuple<JointType, JointType>> boneValuePair in boneDic)
                {
                    Tuple<JointType, JointType> bone = boneValuePair.Value;
                    Joint2D joint0 = templateJoint2Ds.First(o => o.Joint2DType == bone.Item1);
                    Joint2D joint1 = templateJoint2Ds.First(o => o.Joint2DType == bone.Item2);
                    //连接两个节点
                    if (recordingJointTypes != null && recordingBones.Contains(boneValuePair.Key))
                        dc.DrawLine(_boneSelectPen, joint0.Position, joint1.Position);
                    else
                        dc.DrawLine(_bonePen, joint0.Position, joint1.Position);
                }

                //画关节点的圆
                foreach (Joint2D joint2D in templateJoint2Ds)
                {
                    if (recordingJointTypes != null && recordingJointTypes.Contains(joint2D.Joint2DType)) //选中的关节
                        dc.DrawEllipse(_joinSelectPen.Brush, _joinSelectPen, joint2D.Position, 5, 5);
                    else //没有选中的关节
                        dc.DrawEllipse(_joinDefaultPen.Brush, _joinDefaultPen, joint2D.Position, 4, 4);
                }

                templateDrawingGroup.ClipGeometry = new RectangleGeometry(r);
            }
        }

        #region 通过点击模板骨骼图形来获取那些是关键的骨骼和关键的关节

        private void BodyTemplateImage_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image control = sender as Image;
            Point point = e.GetPosition(control); //获取鼠标点击的位置
            if (recordingJointTypes == null)
                recordingJointTypes = new List<JointType>();
            if (recordingBones == null)
                recordingBones = new List<Bone>();

            bool idClickJoint = false;

            #region 计算点击的是哪个关节点

            foreach (var joint2D in templateJoint2Ds)
            {
                if (JudgeChoiceJoint(joint2D.Position, point, 5))
                {
                    idClickJoint = true;
                    if (recordingJointTypes.Contains(joint2D.Joint2DType))
                        recordingJointTypes.Remove(joint2D.Joint2DType);
                    else
                        recordingJointTypes.Add(joint2D.Joint2DType);
                    break;
                }
            }

            #endregion

            if (idClickJoint)
            {
                OpenTemplate();
                return;
            }

            #region 计算点击的是哪个骨骼

            Dictionary<Bone, Tuple<JointType, JointType>> boneDic = SkeletonDictionary.GetBoneDic();
            foreach (KeyValuePair<Bone, Tuple<JointType, JointType>> pair in boneDic)
            {
                Point p1 = templateJoint2Ds.First(o => o.Joint2DType == pair.Value.Item1).Position;
                Point p2 = templateJoint2Ds.First(o => o.Joint2DType == pair.Value.Item2).Position;
                //if (JudgeChoiceBone(p1, p2, point, 5))
                if (JudgeChoiceBoneTest(p1, p2, point, 5))
                {
                    if (recordingBones.Contains(pair.Key))
                        recordingBones.Remove(pair.Key);
                    else
                        recordingBones.Add(pair.Key);
                    break;
                }
            }

            #endregion

            OpenTemplate();
        }

        /// <summary>
        /// 判断点是否在指定圆心和半径的圆内
        /// </summary>
        /// <param name="circleCenter">圆心</param>
        /// <param name="targetPoint">目标点</param>
        /// <param name="radius">半径</param>
        private bool JudgeChoiceJoint(Point circleCenter, Point targetPoint, double radius)
        {
            double temp = Math.Pow(targetPoint.X - circleCenter.X, 2) + Math.Pow(targetPoint.Y - circleCenter.Y, 2);
            temp = Math.Sqrt(temp);
            return temp < radius;
        }

        /// <summary>
        /// 根据长轴的两个顶点(p1和p2)和指定短轴的长度来判断点是否在该椭圆内
        /// 该方法要长轴平行于x
        /// </summary>
        /// <param name="p1">长轴的第一个顶点</param>
        /// <param name="p2">长轴的第二个顶点</param>
        /// <param name="targetPoint">要判断的点</param>
        /// <param name="b">短轴长</param>
        /// <returns></returns>
        private bool JudgeChoiceBone(Point p1, Point p2, Point targetPoint, double b)
        {
            //(h,k)为中心点
            double h = (p1.X + p2.X) / 2;
            double k = (p1.Y + p2.Y) / 2;
            double a = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)) / 2; //长轴长
            double temp = Math.Pow(targetPoint.X - h, 2) / (a * a) + Math.Pow(targetPoint.Y - k, 2) / (b * b);
            return temp < 1;
        }

        /// <summary>
        /// 通过椭圆的一般公式来判断
        /// </summary>
        private bool JudgeChoiceBoneTest(Point p1, Point p2, Point targetPoint, double b)
        {
            //(h,k)为中心点
            double h = (p1.X + p2.X) / 2;
            double k = (p1.Y + p2.Y) / 2;

            Point vector = new Point(p1.X - p2.X, p1.Y - p2.Y);
            double pointMultiply = vector.X * targetPoint.X + vector.Y * targetPoint.Y;
            double lengthMultiply = Math.Sqrt(vector.X * vector.X + vector.Y * vector.Y) *
                                    Math.Sqrt(targetPoint.X * targetPoint.X + targetPoint.Y * targetPoint.Y);
            double cosAngle = pointMultiply / lengthMultiply;
            double angle = Math.Acos(cosAngle); //长轴与X轴的夹角
            double a = Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2)); //长轴长

            double A = a * a * Math.Pow(Math.Sin(angle), 2) + b * b * Math.Pow(Math.Cos(angle), 2);
            double B = 2 * (a * a - b * b) * Math.Sin(angle) * Math.Cos(angle);
            double C = a * a * Math.Pow(Math.Cos(angle), 2) + b * b * Math.Pow(Math.Sin(angle), 2);
            double F = -a * a * b * b;

            double x = targetPoint.X - h;
            double y = targetPoint.Y - k;

            double temp = A * x * x + B * x * y + C * y * y + F;

            return temp < 0;
        }

        #endregion

        private void LabNewModel_OnClick(object sender, RoutedEventArgs e)
        {
            SwitchShowGrid(TemplateViewBox, ModelToolBar);
            OpenTemplate();
            SetStateText("请选择关键的关节节点和骨骼");
        }

        /// <summary> 加载模板 </summary>
        private List<Joint2D> LoadTemplateModel(string modelPath)
        {
            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                MessageBox.Show("模本文件丢失！");
                return null;
            }

            try
            {
                string xmlStr = File.ReadAllText(modelPath);
                List<Joint2D> template = XmlUtil.Deserialize<List<Joint2D>>(xmlStr);
                return template;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return null;
            }
        }

        /// <summary> 关键的关节和骨骼选择完毕 </summary>
        private void BtnOk_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SwitchShowGrid(BodyViewBox, ModelToolBar);
            KinectEventBind();
            kinectSensor.Open();
            BtnRecording.IsEnabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            timer.Enabled = false;
            try
            {
                triggerCount--;
                if (triggerCount < 0)
                {
                    isRecording = true;
                    Dispatcher.BeginInvoke(new Action(() => { BtnRecording.IsEnabled = true; }));
                    infomsg = "";
                    return;
                }

                infomsg = $"请做好动作的开始姿态，{triggerCount}秒钟够开始录制！";
            }
            catch (Exception ex)
            {
                Log.Error(
                    $"程序发生错误：{ex.Message};错误地址：{ex.StackTrace.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)[0].Trim()}");
            }

            timer.Enabled = true;
        }

        private void SetPromptInfo(DrawingContext dc, string msg)
        {
            Brush background = new SolidColorBrush(Color.FromArgb(100, 236, 238, 52));
            FormattedText formattedText = new FormattedText(
                msg,
                CultureInfo.GetCultureInfo("zh-cn"), //en-us 英文 zh-cn 中文
                FlowDirection.LeftToRight,
                new Typeface("Verdana"),
                50,
                background);
            double w = formattedText.Width;
            double h = formattedText.Height;
            Point point = new Point((displayWidth - w) / 2, (displayHeight - h) / 2);
            dc.DrawText(formattedText, point);
        }

        private void BtnCancel_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result =
                MessageBox.Show("是否确定要取消！", "取消", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                BackStartPage();
            }
        }

        /// <summary>开始或结束录制 </summary>
        private void BtnRecording_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!isRecording)
            {
                actionModel = new ActionModel();
                triggerCount = 5;
                infomsg = $"请做好动作的开始姿态，{triggerCount}秒钟够开始录制！";
                timer.Start();
                BtnRecording.IsEnabled = false;
                BtnRecording.Content = "结束录制";
            }
            else //录制结束 跳转到保存页面
            {
                isRecording = false;
                BtnRecording.Content = "开始录制";
                KinectEventCancelBind();
                kinectSensor.Close();
                SwitchShowGrid(SavePage);
            }
        }

        #endregion

        private void LabBtnOk_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                actionModel.ActionName = ActionNameTbx.Text.Trim();
                actionModel.JointAngles = recordingJointTypes;
                actionModel.KeyBones = recordingBones;
                float angularError = 0;
                if (float.TryParse(AngularErrorTbx.Text.Trim(), out angularError))
                    actionModel.AllowableAngularError = angularError;
                else
                {
                    MessageBox.Show("关键关节角度误差填写错误！，请重新填写！", "提示");
                    return;
                }

                float keyBoneError = 0;
                if (float.TryParse(KeyBoneErrorTbx.Text.Trim(), out keyBoneError))
                    actionModel.AllowableKeyBoneError = keyBoneError;
                else
                {
                    MessageBox.Show("关键骨骼角度误差！，请重新填写！", "提示");
                    return;
                }

                saveFileDialog.FileName = actionModel.ActionName;
                bool? result = saveFileDialog.ShowDialog();
                if (result != true) return;
                string filePath = saveFileDialog.FileName;
                string actionStr = XmlUtil.Serializer(actionModel);
                File.WriteAllText(filePath, actionStr);
            }
            catch (Exception ex)
            {
                Log.Error(
                    $"程序发生错误：{ex.Message};错误地址：{ex.StackTrace.Split(new[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries)[0].Trim()}");
            }

            SwitchShowGrid(SaveResultPage);
        }

        private void LabBtnCancel_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MessageBoxResult result =
                MessageBox.Show("是否确定要取消！", "取消", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                recordingJointTypes?.Clear();
                recordingBones?.Clear();
                actionModel?.Dispose();
                actionModel = null;
                BtnRecording.Content = "开始录制";
                isRecording = false;
                SwitchShowGrid(StartPageBorder);
            }
        }

        private void LabBtnBackStartPage_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            BackStartPage();
        }

        private void LabBtnExit_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
    }
}