using KyoshinMonitorLib;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace FormsMapRenderTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			mapControl1.MouseMove += (s, e2) =>
			{
				//if (mapControl1.IsNavigating)
				//	return;
				var curPos = e2.Location;
				if ((e2.Button & MouseButtons.Left) != 0)
				{
					var diff = new PointD(_prevPos.X - curPos.X, _prevPos.Y - curPos.Y);
					mapControl1.CenterLocation = (mapControl1.CenterLocation.ToPixel(mapControl1.Projection, mapControl1.Zoom) + diff).ToLocation(mapControl1.Projection, mapControl1.Zoom);
				}

				_prevPos = curPos;
				var rect = mapControl1.PaddedRect;

				var centerPos = mapControl1.CenterLocation.ToPixel(mapControl1.Projection, mapControl1.Zoom);
				//var mousePos = e.GetPosition(map);
				var mouseLoc = new PointD(centerPos.X + ((rect.Width / 2) - curPos.X) + rect.Left, centerPos.Y + ((rect.Height / 2) - curPos.Y) + rect.Top).ToLocation(mapControl1.Projection, mapControl1.Zoom);

				label1.Text = $"Mouse Lat: {mouseLoc.Latitude:0.000000} / Lng: {mouseLoc.Longitude:0.000000}";
			};
			mapControl1.MouseDown += (s, e2) => 
			{
				if ((e2.Button & MouseButtons.Left) != 0)
					_prevPos = e2.Location;
				if ((e2.Button & MouseButtons.Right) != 0)
					mapControl1.Navigate(new RectD(new PointD(23.996627, 123.469848), new PointD(24.662051, 124.420166)));
				if ((e2.Button & MouseButtons.Middle) != 0)
					mapControl1.Navigate(new RectD(new PointD(24.058240, 123.046875), new PointD(45.706479, 146.293945)));
			};
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			mapControl1.Map = TopologyMap.LoadCollection(@"D:\Source\Repos\KyoshinEewViewerIngen\world.mpk.lz4");
			mapControl1.Zoom = 5;
			mapControl1.CenterLocation = new Location(36.474f, 135.264f);
		}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
			base.OnMouseWheel(e);

			//if (map.IsNavigating)
			//	return;
			var paddedRect = mapControl1.PaddedRect;
			var centerPix = mapControl1.CenterLocation.ToPixel(mapControl1.Projection, mapControl1.Zoom);
			System.Diagnostics.Debug.WriteLine(e.Location);
			var mousePos = e.Location;
			var mousePix = new PointD(centerPix.X + ((paddedRect.Width / 2) - mousePos.X) + paddedRect.Left, centerPix.Y + ((paddedRect.Height / 2) - mousePos.Y) + paddedRect.Top);
			var mouseLoc = mousePix.ToLocation(mapControl1.Projection, mapControl1.Zoom);

			mapControl1.Zoom += e.Delta / 120 * 0.25;

			var newCenterPix = mapControl1.CenterLocation.ToPixel(mapControl1.Projection, mapControl1.Zoom);
			var goalMousePix = mouseLoc.ToPixel(mapControl1.Projection, mapControl1.Zoom);

			var newMousePix = new PointD(newCenterPix.X + ((paddedRect.Width / 2) - mousePos.X) + paddedRect.Left, newCenterPix.Y + ((paddedRect.Height / 2) - mousePos.Y) + paddedRect.Top);

			mapControl1.CenterLocation = (mapControl1.CenterLocation.ToPixel(mapControl1.Projection, mapControl1.Zoom) - (goalMousePix - newMousePix)).ToLocation(mapControl1.Projection, mapControl1.Zoom);
		}

		Point _prevPos;

		protected override void OnClientSizeChanged(EventArgs e)
		{
			base.OnClientSizeChanged(e);
			//map.Navigate(new Rect(new Point(24.058240, 123.046875), new Point(45.706479, 146.293945)), new Duration(TimeSpan.Zero));
		}
	}
}
