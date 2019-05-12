using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using FitnessCoach.BoneNode;
using FitnessCoach.Config;
using FitnessCoach.Util;
using Microsoft.Kinect;

namespace FitnessCoach
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _isRecord;

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

        public MainWindow()
        {
            string path = Environment.CurrentDirectory;
            InitializeComponent();
            InitUi();
            InitKinect();
            this.DataContext = this;
        }

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

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.bodyFrameReader != null)
                this.bodyFrameReader.FrameArrived += BodyFrameReader_FrameArrived;

            if (this.colorFrameReader != null)
                this.colorFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
        }

        private void MainWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (this.bodyFrameReader != null)
            {
                this.bodyFrameReader.Dispose();
                this.bodyFrameReader = null;
            }

            if (this.colorFrameReader != null)
            {
                this.colorFrameReader.Dispose();
                this.colorFrameReader = null;
            }

            if (this.kinectSensor == null) return;
            this.kinectSensor.Close();
            this.kinectSensor = null;
            GC.Collect();
        }


        private void InitUi()
        {
            this.drawingGroup = new DrawingGroup();
            this._bodyBodyImageSource = new DrawingImage(drawingGroup);
        }

        private void InitKinect()
        {
            this.kinectSensor = GlobalConfig.Sensor;
            this.kinectSensor.IsAvailableChanged += KinectSensor_IsAvailableChanged;
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

            this.kinectSensor.Open();

            this.StatusText = "Kinect不可用!";
            this.DataContext = this;
        }

        private void KinectSensor_IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            this.StatusText = this.kinectSensor.IsAvailable
                ? "正在运行"
                : "Kinect不可用!";
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
                
                if (this._isRecord)
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

        private void BtnStartRecording_OnClick(object sender, RoutedEventArgs e)
        {
            _isRecord = !_isRecord;
            BtnStartRecording.Content = _isRecord ? "停止记录节点角度" : "记录节点角度";
        }

        private void RecordJointAngle(Body[] bodies)
        {
            //TODO 录制骨骼信息用于模型的识别
            Dictionary<JointType, float> jointAngleDIc = new Dictionary<JointType, float>();
            foreach (Body body in bodies)
            {
                if (body.IsTracked)
                {
                    jointAngleDIc = Skeleton.GetBodyJointAngleDic(body.Joints);
                }
            }
        }
    }
}