#彩色图像数据
## 彩色图像帧
注意及时释放资源以免内存泄露
<!--lange:C#-->
	public sealed class ColorFrame : IDisposable
	{
		public TimeSpan RelativeTime { get; }
		//彩色帧源
		public ColorFrameSource ColorFrameSource { get; }
		//框架描述
		public FrameDescription FrameDescription { get; }
		//彩色相机设置
		public ColorCameraSettings ColorCameraSettings { get; }
		//彩色图像格式
		public ColorImageFormat RawColorImageFormat { get; }
		//创建框架描述
		public FrameDescription CreateFrameDescription(ColorImageFormat format);
		public KinectBuffer LockRawImageBuffer();
		public void CopyRawFrameDataToArray(byte[] frameData);
		public void CopyRawFrameDataToIntPtr(IntPtr frameData, uint size);
		public void CopyConvertedFrameDataToArray(byte[] frameData, ColorImageFormat colorFormat);
		public void CopyConvertedFrameDataToIntPtr(IntPtr frameData, uint size, ColorImageFormat colorFormat);
		[HandleProcessCorruptedStateExceptions]
		protected void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0);
		public override sealed void Dispose();
		~ColorFrame();
	}

#数据获取
<!--lange:C#-->
	this.kinectSensor = KinectSensor.GetDefault();
	this.colorFrameReader = this.kinectSensor.ColorFrameSource.OpenReader();
	this.colorFrameReader.FrameArrived += this.Reader_ColorFrameArrived;

	// 使用Bgra格式从颜色框架源创建颜色框架描述
	FrameDescription colorFrameDescription = this.kinectSensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);
	
	 // 创建要显示的位图
	this.colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

	this.kinectSensor.Open();

	private void Reader_ColorFrameArrived(object sender, ColorFrameArrivedEventArgs e)
	{
		// ColorFrame is IDisposable
		using (ColorFrame colorFrame = e.FrameReference.AcquireFrame())
		{
			if (colorFrame != null)
			{
				FrameDescription colorFrameDescription = colorFrame.FrameDescription;
				
				using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
				{
					this.colorBitmap.Lock();    //注意线程安全
					
					// verify data and write the new color frame data to the display bitmap
					// 验证数据并将新的彩色帧数据写入显示位图
					if ((colorFrameDescription.Width == this.colorBitmap.PixelWidth) && (colorFrameDescription.Height == this.colorBitmap.PixelHeight))
					{
						// 把数据保存到  this.colorBitmap
						colorFrame.CopyConvertedFrameDataToIntPtr(this.colorBitmap.BackBuffer, (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4), ColorImageFormat.Bgra);
						
						this.colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this.colorBitmap.PixelWidth,  this.colorBitmap.PixelHeight));
					}
					this.colorBitmap.Unlock();
				}
			}
		}
	}