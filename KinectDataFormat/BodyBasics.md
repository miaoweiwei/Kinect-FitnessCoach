# 骨骼框架的有关数据
## 骨骼节点
<!-- lang:C#-->
	public struct Joint : IEquatable<Joint>
	{
		public JointType JointType;
		//相对于相机的空间位置三维
		public CameraSpacePoint Position;
		//跟踪状态,这个点是推断的还是跟踪的或者识别不到的
		public TrackingState TrackingState;
		public override sealed int GetHashCode();
		[return: MarshalAs(UnmanagedType.U1)]
		public bool Equals(Joint joint);
		[return: MarshalAs(UnmanagedType.U1)]
		public override sealed bool Equals(object obj);
		public static bool operator ==(Joint a, Joint b);
		public static bool operator !=(Joint a, Joint b);
	}
## 骨骼节点数据的枚举
<!-- lang:C#-->
	public enum JointType
	{
		//尾椎
		SpineBase,
		//脊柱中间
		SpineMid,
		//颈部
		Neck,
		//头
		Head,
		//左肩膀
		ShoulderLeft,
		//左肘
		ElbowLeft,
		//左手腕
		WristLeft,
		//左手
		HandLeft,
		//右肩
		ShoulderRight,
		//右肘
		ElbowRight,
		//右手腕
		WristRight,
		//右手
		HandRight,
		//左髋关节
		HipLeft,
		//左膝盖
		KneeLeft,
		//左踝关节
		AnkleLeft,
		//左脚
		FootLeft,
		//右髋关节
		HipRight,
		//右膝盖
		KneeRight,
		//右踝关节
		AnkleRight,
		//右脚
		FootRight,
		//脊椎肩膀中间的那个点
		SpineShoulder,
		//左手掌
		HandTipLeft,
		//左拇指
		ThumbLeft,
		//右手掌
		HandTipRight,
		//右拇指
		ThumbRight,
	}
## 手掌状态的枚举
<!--lange:C#-->
	public enum HandState
	{
		//未知
		Unknown,
		//未跟踪
		NotTracked,
		//打开
		Open,
		//关闭
		Closed,
		//握拳
		Lasso,
	}

## 数据来源
<!--lange:C#-->
	this.kinectSensor = KinectSensor.GetDefault();
	// 为身体框架打开阅读器
	this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
	// 绑定接收事件
	this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
	
	// Body为一个人体骨骼的类，
	// 里面提供了所有骨骼节点的信息、手的状态、骨骼的跟踪状态等有关信息
	Body[] bodies；
	
	//接收的事假
	private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
	{
		using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
		{
			if (bodyFrame != null)
			{
			    if (this.bodies == null)
			    {
			        this.bodies = new Body[bodyFrame.BodyCount];
			    }
				// 读到的骨骼数据保存到数组里
				bodyFrame.GetAndRefreshBodyData(this.bodies);
			}
		}
	}