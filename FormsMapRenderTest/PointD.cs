namespace FormsMapRenderTest
{
	public struct PointD
	{
		public PointD(double x = 0, double y = 0)
		{
			X = x;
			Y = y;
		}

		public double X { get; set; }
		public double Y { get; set; }

		public static PointD operator +(PointD p1, PointD p2)
			=> new PointD(p1.X + p2.X, p1.Y + p2.Y);

		public static PointD operator -(PointD p1, PointD p2)
			=> new PointD(p1.X - p2.X, p1.Y - p2.Y);
	}
}
