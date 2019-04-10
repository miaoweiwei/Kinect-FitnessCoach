# 语音数据
## 音频束
<!--lange:C#-->
	public sealed class AudioBeam : INotifyPropertyChanged
	{
		//属性更改事件
		public virtual event PropertyChangedEventHandler PropertyChanged;
		//识别到的音频束的角度
		public float BeamAngle { get; set; }
		//该识别结果的置信度
		public float BeamAngleConfidence { get; }
		//音频束模式
		public AudioBeamMode AudioBeamMode { get; set; }
		public TimeSpan RelativeTime { get; }
		public AudioSource AudioSource { get; }
		//音频数据流
		public Stream OpenInputStream();
		[HandleProcessCorruptedStateExceptions]
		protected void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0);
		~AudioBeam();
	}

## 音频束子帧
<!--lange:C#-->
	public sealed class AudioBeamSubFrame : IDisposable
	{
		//帧长度的字节
		public uint FrameLengthInBytes { get; }
		//持续时间
		public TimeSpan Duration { get; }
		//识别到的音频束的角度
		public float BeamAngle { get; set; }
		//该识别结果的置信度
		public float BeamAngleConfidence { get; }
		//音频束模式
		public AudioBeamMode AudioBeamMode { get; }
		//相对时间
		public TimeSpan RelativeTime { get; }
		//音频身体相关性
		public IReadOnlyList<AudioBodyCorrelation> AudioBodyCorrelations { get; }
		public KinectBuffer LockAudioBuffer();
		public void CopyFrameDataToArray(byte[] frameData);
		public void CopyFrameDataToIntPtr(IntPtr frameData, uint size);
		[HandleProcessCorruptedStateExceptions]
		protected void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0);
		public override sealed void Dispose();
		~AudioBeamSubFrame();
	}


## 音频帧
<!--lange:C#-->
	public sealed class AudioBeamFrame : IDisposable
	{
		//相对时间开始
		public TimeSpan RelativeTimeStart { get; }
		//持续时间
		public TimeSpan Duration { get; }
		//音频源
		public AudioSource AudioSource { get; }
		//音频束
		public AudioBeam AudioBeam { get; }
		//音频束子帧列表
		public IReadOnlyList<AudioBeamSubFrame> SubFrames { get; }
		[HandleProcessCorruptedStateExceptions]
		protected void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0);
		public override sealed void Dispose();
		~AudioBeamFrame();
	}

## 音频帧列表
<!--lange:C#-->
	// 该类主要对音频帧类（AudioBeamFrame）封装成了List
	public sealed class AudioBeamFrameList : IReadOnlyList<AudioBeamFrame>, IDisposable
	{
		public virtual int Count { get; }
		public virtual AudioBeamFrame this[int index] { get; }
		public IEnumerator<AudioBeamFrame> GetEnumerator();
		IEnumerator IEnumerable.EnumerableGetEnumerator();
		[HandleProcessCorruptedStateExceptions]
		protected void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0);
		public override sealed void Dispose();
		~AudioBeamFrameList();
	}



## 数据获取
<!--lange:C#-->
	this.kinectSensor = KinectSensor.GetDefault();
	this.kinectSensor.Open();
	
	// 获取其音频源
	AudioSource audioSource = this.kinectSensor.AudioSource;

	//打开阅读器以获取音频帧
	this.reader = audioSource.OpenReader();
	
	// 订阅新的音频帧到达事件
	this.reader.FrameArrived += this.Reader_FrameArrived;


	private void Reader_FrameArrived(object sender, AudioBeamFrameArrivedEventArgs e)
	{
		//音频束帧引用
		AudioBeamFrameReference frameReference = e.FrameReference;
		//音频光束帧列表
		AudioBeamFrameList frameList = frameReference.AcquireBeamFrames();
	}