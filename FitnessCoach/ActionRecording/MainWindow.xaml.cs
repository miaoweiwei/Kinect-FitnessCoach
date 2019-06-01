using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using FitnessCoach.BoneNode;
using FitnessCoach.Config;
using FitnessCoach.Util;
using Microsoft.Kinect;
using Microsoft.Win32;

namespace ActionRecording
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private OpenFileDialog openFileDialog;
        private SaveFileDialog saveFileDialog;
        private List<Joint2D> templateJoint2Ds;

        private List<JointType> recordingJointTypeList; //记录用于对比的关节

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
        private Pen _drawBonePen = new Pen(Brushes.Red, 4);
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
            drawingGroup = new DrawingGroup();
            _bodyImageSource = new DrawingImage(drawingGroup);
            templateDrawingGroup = new DrawingGroup();
            _templateImageSource = new DrawingImage(templateDrawingGroup);
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
        }

        private void InitKinect()
        {
            kinectSensor = GlobalConfig.Sensor;
            kinectSensor.IsAvailableChanged += KinectSensor_IsAvailableChanged;
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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (bodyFrameReader != null)
                bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (bodyFrameReader != null)
            {
                bodyFrameReader.Dispose();
                bodyFrameReader = null;
            }

            if (kinectSensor == null) return;
            kinectSensor.Close();
            kinectSensor = null;
            GC.Collect();
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
        /// <param name="ctx">要显示的图层的名字</param>
        private void SwitchShowGrid(string ctx)
        {
            if (StartPageBorder.Name == ctx)
            {
                StartPageBorder.Visibility = Visibility.Visible;
                TemplateViewBox.Visibility = Visibility.Collapsed;
                BodyViewBox.Visibility = Visibility.Collapsed;
            }
            else if (TemplateViewBox.Name == ctx)
            {
                TemplateViewBox.Visibility = Visibility.Visible;
                StartPageBorder.Visibility = Visibility.Collapsed;
                BodyViewBox.Visibility = Visibility.Collapsed;
            }
            else if (BodyViewBox.Name == ctx)
            {
                BodyViewBox.Visibility = Visibility.Visible;
                StartPageBorder.Visibility = Visibility.Collapsed;
                TemplateViewBox.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null) //重新初始化身体数组
                {
                    if (bodies == null || bodies.Length < bodyFrame.BodyCount)
                        bodies = new Body[bodyFrame.BodyCount];
                    //接收身体骨骼信息
                    bodyFrame.GetAndRefreshBodyData(bodies);
                    dataReceived = true; //数据接收成功
                }
            }

            if (!dataReceived) return;
            using (DrawingContext dc = drawingGroup.Open())
            {
                Rect r = new Rect(0.0, 0.0, displayWidth, displayHeight);
                dc.DrawRectangle(_bodyBackGroundColor, null, r);

                //skeleton.DrawBodyArr(bodies, dc);
                for (int i = 0; i < bodies.Length; i++)
                {
                    Body body = bodies[i];
                    skeleton.DrawBody(body, i, dc);


                }

                //画关节点的圆
                foreach (Joint2D joint2D in templateJoint2Ds)
                {
                    if (recordingJointTypeList != null && recordingJointTypeList.Contains(joint2D.Joint2DType)) //选中的关节
                        dc.DrawEllipse(_joinSelectPen.Brush, _joinSelectPen, joint2D.Position, 5, 5);
                    else //没有选中的关节
                        dc.DrawEllipse(_joinDefaultPen.Brush, _joinDefaultPen, joint2D.Position, 4, 4);
                }

                drawingGroup.ClipGeometry = new RectangleGeometry(r);
            }
        }

        private void LabBtnOpenModel_OnClick(object sender, RoutedEventArgs e)
        {
            var result = openFileDialog.ShowDialog();
            if (result != true) return;
            string filePath = openFileDialog.FileName;
            //TODO 打开模型
            ModelToolBar.Visibility = Visibility.Visible;
        }

        #region 新建模型

        private void LabNewModel_OnClick(object sender, RoutedEventArgs e)
        {
            //kinectSensor.Open();
            //SwitchShowGrid(BodyViewBox.Name);
            ModelToolBar.Visibility = Visibility.Visible;
            SwitchShowGrid(TemplateViewBox.Name);
            OpenTemplate();
        }

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
                    dc.DrawLine(_drawBonePen, joint0.Position, joint1.Position);
                }

                //画关节点的圆
                foreach (Joint2D joint2D in templateJoint2Ds)
                {
                    if (recordingJointTypeList != null && recordingJointTypeList.Contains(joint2D.Joint2DType)) //选中的关节
                        dc.DrawEllipse(_joinSelectPen.Brush, _joinSelectPen, joint2D.Position, 5, 5);
                    else //没有选中的关节
                        dc.DrawEllipse(_joinDefaultPen.Brush, _joinDefaultPen, joint2D.Position, 4, 4);
                }

                drawingGroup.ClipGeometry = new RectangleGeometry(r);
            }
        }

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

        private void BtnSave_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void BtnSelectJoint_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void BtnSelectKeyBone_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void BtnStartRecording_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        #endregion

        private void BoneModelImage_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image control = sender as Image;
            Point point = e.GetPosition(control); //获取鼠标点击的位置
            Debug.WriteLine($"鼠标右键状态：{e.ButtonState}，鼠标点击位置：{point}");
            if (recordingJointTypeList == null)
                recordingJointTypeList = new List<JointType>();

            Dictionary<JointType, double> tempDic = new Dictionary<JointType, double>();

            foreach (Joint2D joint2D in templateJoint2Ds)
            {
                double datX = Math.Pow(point.X - joint2D.Position.X, 2);
                double datY = Math.Pow(point.Y - joint2D.Position.Y, 2);
                double distance = Math.Sqrt(datX + datY);
                tempDic.Add(joint2D.Joint2DType, distance);
            }

            var jointPair = tempDic.OrderBy(o => o.Value).First();

            Debug.WriteLine(tempDic.OrderBy(o => o.Value).First());
            Debug.WriteLine(tempDic.OrderBy(o => o.Value).ElementAt(1));

            if (jointPair.Value > 5) return;
            if (recordingJointTypeList.Contains(jointPair.Key))
                recordingJointTypeList.Remove(jointPair.Key);
            else
                recordingJointTypeList.Add(jointPair.Key);
            OpenTemplate();
        }
    }
}