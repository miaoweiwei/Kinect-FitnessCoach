# Kinetc 中有关位置点的说明
## 二维坐标
<!-- lang:C#-->
	public struct DepthSpacePoint : IEquatable<DepthSpacePoint>
	{
	    public float X;
	    public float Y;
	    public override sealed int GetHashCode();
	    [return: MarshalAs(UnmanagedType.U1)]
	    public bool Equals(DepthSpacePoint point);
	    [return: MarshalAs(UnmanagedType.U1)]
	    public override sealed bool Equals(object obj);
	    public static bool operator ==(DepthSpacePoint a, DepthSpacePoint b);
	    public static bool operator !=(DepthSpacePoint a, DepthSpacePoint b);
	}
## 三维的坐标点
<!-- lang:C#-->
	public struct CameraSpacePoint : IEquatable<CameraSpacePoint>
	{
		public float X;
	    public float Y;
	    public float Z;
	    public override sealed int GetHashCode();
	    [return: MarshalAs(UnmanagedType.U1)]
	    public bool Equals(CameraSpacePoint point);
	    [return: MarshalAs(UnmanagedType.U1)]
	    public override sealed bool Equals(object obj);
	    public static bool operator ==(CameraSpacePoint a, CameraSpacePoint b);
	    public static bool operator !=(CameraSpacePoint a, CameraSpacePoint b);
	}