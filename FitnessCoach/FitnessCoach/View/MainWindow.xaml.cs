using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Speech.Synthesis;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using FitnessCoach.BoneNode;
using FitnessCoach.Config;
using FitnessCoach.Core;
using FitnessCoach.Util;
using Microsoft.Kinect;
using Microsoft.Win32;
using Timer = System.Windows.Forms.Timer;

namespace FitnessCoach.View
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region 姿态识别

        private bool _isRecognition;
        private AttitudeRecognition attitudeRecognition;

        private Timer timer;

        #endregion

        #region 有关UI的字段

        /// <summary>
        /// 用于体渲染输出的绘图组
        /// </summary>
        private DrawingGroup drawingGroup;

        /// <summary>
        /// 绘制骨骼图像
        /// </summary>
        private DrawingImage _bodyBodyImageSource;

        /// <summary>
        /// 显示彩色图像
        /// </summary>
        private WriteableBitmap _colorBitmapSource = null;

        /// <summary>
        /// 用于状态栏的显示
        /// </summary>
        private string statusText = null;

        private string recognitionResultText = null;

        /// <summary>
        /// 显示宽度（深度空间）
        /// </summary>
        private int displayWidth;

        /// <summary>
        /// 显示高度（深度空间）
        /// </summary>
        private int displayHeight;

        #endregion

        #region 有关Kinect的字段

        private KinectSensor kinectSensor;

        /// <summary>
        /// 坐标转化器，可以把三维坐标转成二维的坐标
        /// </summary>
        private CoordinateMapper coordinateMapper = null;

        /// <summary>
        /// 骨骼框架阅读器
        /// </summary>
        private BodyFrameReader bodyFrameReader = null;

        /// <summary>
        /// 彩色图像阅读器
        /// </summary>
        private ColorFrameReader colorFrameReader = null;

        /// <summary>
        /// 人体数组
        /// </summary>
        private Body[] bodies = null;

        private Skeleton skeleton;

        #endregion

        /// <summary>
        /// 属性值发生更改事件，用于通知UI更新
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 绘制骨骼图
        /// </summary>
        public DrawingImage BodyImageSource
        {
            get => _bodyBodyImageSource;
        }

        /// <summary>
        /// 显示彩色图像
        /// </summary>
        public WriteableBitmap ColorBitmapSource
        {
            get => _colorBitmapSource;
        }

        /// <summary>
        /// 用于状态栏的显示
        /// </summary>
        public string StatusText
        {
            get => statusText;
            set
            {
                if (this.statusText == value) return;
                this.statusText = value;
                //通知任何绑定元素文本已更改
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StatusText"));
            }
        }

        /// <summary>
        /// 识别结果
        /// </summary>
        public string RecognitionResultText
        {
            get => recognitionResultText;
            set
            {
                if (this.recognitionResultText == value) return;
                this.recognitionResultText = value;
                //通知任何绑定元素文本已更改
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("RecognitionResultText"));
            }
        }

        public MainWindow()
        {
            string path = Environment.CurrentDirectory;
            InitializeComponent();
            InitUi();
            InitKinect();
            this.DataContext = this;
        }

        private void InitUi()
        {
            this.drawingGroup = new DrawingGroup();
            this._bodyBodyImageSource = new DrawingImage(drawingGroup);
            SelectModelDir(GlobalConfig.ActionModelDirPath);
        }

        private void InitKinect()
        {
            this.kinectSensor = GlobalConfig.Sensor;
            //获取坐标映射器
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            FrameDescription frameDescription = this.kinectSensor.ColorFrameSource.FrameDescription; //获取彩色图的分辨率
            //得到关节空间的大小
            this.displayWidth = frameDescription.Width;
            this.displayHeight = frameDescription.Height;
            //设置骨骼空间的大小与彩色图像的一致，避免骨骼和彩色任务图像存在偏差
            this.skeleton = new Skeleton(displayWidth, displayHeight, coordinateMapper);

            //为身体骨骼打开阅读器
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            //获取彩色图像的阅读器
            this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();

            // 使用Bgra格式从颜色框架源创建颜色框架描述
            FrameDescription colorFrameDescription =
                this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
            // 创建要显示的位图
            this._colorBitmapSource = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height,
                96.0, 96.0, PixelFormats.Bgr32, null);
            this.StatusText = "Kinect不可用!";
        }

        private void KinectEventBind()
        {
            kinectSensor.IsAvailableChanged += KinectSensor_IsAvailableChanged;
            if (bodyFrameReader != null)
                bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;
            if (this.colorFrameReader != null)
                this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
        }

        private void KinectEventCancelBind()
        {
            kinectSensor.IsAvailableChanged -= KinectSensor_IsAvailableChanged;
            if (bodyFrameReader != null)
                bodyFrameReader.FrameArrived -= BodyFrameReader_FrameArrived;

            if (this.colorFrameReader != null)
                this.colorFrameReader.FrameArrived -= ColorFrameReader_FrameArrived;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            KinectEventCancelBind();
            this.bodyFrameReader?.Dispose();
            this.bodyFrameReader = null;
            this.colorFrameReader?.Dispose();
            this.colorFrameReader = null;
            this.kinectSensor?.Close();
            this.kinectSensor = null;
            GC.Collect();
        }

        private void BodyFrameReader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            bool dataReceived = false;
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null) //重新初始化身体数组
                {
                    if (this.bodies == null || this.bodies.Length < bodyFrame.BodyCount)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    //接收身体骨骼信息
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                    dataReceived = true; //数据接收成功
                }
            }

            if (!dataReceived) return;
            using (DrawingContext dc = this.drawingGroup.Open())
            {
                dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, byte.MaxValue, byte.MaxValue, byte.MaxValue)),
                    null, new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));
                this.skeleton.DrawBodyArr(this.bodies, dc);
                this.drawingGroup.ClipGeometry =
                    new RectangleGeometry(new Rect(0.0, 0.0, this.displayWidth, this.displayHeight));

                if (this._isRecognition)
                    this.RecordJointAngle(this.bodies);
            }
        }

        private void ColorFrameReader_FrameArrived(object sender, ColorFrameArrivedEventArgs e)
        {
            using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;
                    using (colorFrame.LockRawImageBuffer())
                    {
                        this.ColorBitmapSource.Lock(); //注意线程安全
                        // 验证数据并将新的彩色帧数据写入显示位图
                        if ((colorFrameDescription.Width == this.ColorBitmapSource.PixelWidth) &&
                            (colorFrameDescription.Height == this.ColorBitmapSource.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(this.ColorBitmapSource.BackBuffer,
                                (uint) (colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);
                            this.ColorBitmapSource.AddDirtyRect(new Int32Rect(0, 0, this.ColorBitmapSource.PixelWidth,
                                this.ColorBitmapSource.PixelHeight));
                        }

                        this.ColorBitmapSource.Unlock();
                    }
                }
            }
        }

        private void KinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.StatusText = this.kinectSensor.IsAvailable
                ? "正在运行"
                : "Kinect不可用!";
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
            StartPage.Visibility = Visibility.Collapsed;
            ModelPage.Visibility = Visibility.Collapsed;
            MenuBox.Visibility = Visibility.Collapsed;
            foreach (UIElement element in elements)
            {
                element.Visibility = Visibility.Visible;
            }
        }

        private void MenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            SwitchShowGrid(StartPage);
            KinectEventCancelBind();
            kinectSensor.Close();
        }

        private void LabSelectModelDir_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            using (System.Windows.Forms.FolderBrowserDialog folder = new System.Windows.Forms.FolderBrowserDialog())
            {
                folder.Description = "选择动作模型文件夹";
                folder.ShowNewFolderButton = false;
                folder.SelectedPath = AppDomain.CurrentDomain.BaseDirectory;
                if (folder.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    SelectModelDir(folder.SelectedPath);
            }
        }

        private void SelectModelDir(string modelDir)
        {
            GlobalConfig.ActionModelDirPath = modelDir;
            ModelDirPathTbx.Text = GlobalConfig.ActionModelDirPath;
            ActionRecognition action = ActionRecognition.GetActionRecognition();
            action.ModeFilePathList.Clear();
            action.LoadModelDir(GlobalConfig.ModelDirPath);
            ModelListView.Items.Clear();
            foreach (string path in action.ModeFilePathList)
            {
                string fileStr = Path.GetFileName(path);
                string file = fileStr.Substring(0, fileStr.LastIndexOf('.'));
                ListViewItem item = new ListViewItem {Content = file, Tag = path};
                ModelListView.Items.Add(item);
            }
        }

        private void ModelListBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ListViewItem item = e.Source as ListViewItem;
            ActionRecognition action = ActionRecognition.GetActionRecognition();
            action.LoadModelFromFile(item.Tag.ToString());

            KinectEventBind();
            kinectSensor.Open();
            SwitchShowGrid(ModelPage, MenuBox);
        }

        private void BtnStartRecording_OnClick(object sender, RoutedEventArgs e)
        {
            _isRecognition = !_isRecognition;
            BtnStartRecording.Content = _isRecognition ? "停止姿态识别" : "姿态识别";
            StatusText = _isRecognition ? "正在识别中..." : "未进行识别";
            if (attitudeRecognition == null)
                attitudeRecognition = AttitudeRecognition.GetAttitudeRecognition();
            if (timer == null)
            {
                timer = new Timer {Interval = 1000};
                timer.Tick += Timer_Tick;
            }

            if (_isRecognition)
                timer.Start();
            else
                timer.Stop();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            timer.Enabled = false;
            try
            {
                SpeechHelp speech = SpeechHelp.GetInstance();
                if (speech.Content == RecognitionResultText) return;
                if (speech.SpeechState != SynthesizerState.Speaking)
                    speech.Speak(RecognitionResultText);

            }
            catch (Exception ex)
            {
                LogUtil.Error(this, ex);
            }
            finally
            {
                timer.Enabled = true;
            }
        }

        private void RecordJointAngle(Body[] bodies)
        {
            //TODO 录制骨骼信息用于模型的识别
            foreach (Body body in bodies)
            {
                if (body.IsTracked)
                {
                    #region 姿态

                    //List<RecognitionResult> resList = attitudeRecognition.Identification(body.Joints);
                    //string resultStr = "";
                    //foreach (RecognitionResult result in resList)
                    //    resultStr += $"姿态：{result.AttitudeName};提示信息：{string.Join(",", result.InfoMessages)}\r";
                    //RecognitionResultText = !string.IsNullOrEmpty(resultStr)
                    //    ? resultStr
                    //    : resultStr.Remove(resultStr.Length - 1);

                    #endregion

                    #region 动作识别

                    ActionRecognition action = ActionRecognition.GetActionRecognition();

                    RecognitionResult result = action.Identification(body.Joints);

                    RecognitionResultText = string.Join(",", result.InfoMessages);

                    #endregion
                }
            }
        }
    }
}