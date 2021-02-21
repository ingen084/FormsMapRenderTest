using FormsMapRenderTest.InternalControls;
using FormsMapRenderTest.Projections;
using KyoshinMonitorLib;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsMapRenderTest
{
	public class MapControl : UserControl
	{
		private Location centerLocation;
		public Location CenterLocation
		{
			get => centerLocation;
			set
			{
				if (centerLocation == value)
					return;
				centerLocation = value;
				if (centerLocation != null)
				{
					var cl = centerLocation;
					cl.Latitude = Math.Min(Math.Max(cl.Latitude, -80), 80);
					// 1回転させる
					if (cl.Longitude < -180)
						cl.Longitude += 360;
					if (cl.Longitude > 180)
						cl.Longitude -= 360;
					CenterLocation = cl;
				}

				ApplySize();
				InvalidateChildVisual();

			}
		}

		private double zoom;
		public double Zoom
		{
			get => zoom;
			set
			{
				if (zoom == value)
					return;
				zoom = Math.Min(Math.Max(value, MinZoom), MaxZoom);

				if (LandLayer != null)
					LandLayer.Zoom = zoom;
				//if (OverlayLayer != null)
				//	OverlayLayer.Zoom = zoom;
				//if (RealtimeOverlayLayer != null)
				//	RealtimeOverlayLayer.Zoom = zoom;
				ApplySize();
				InvalidateChildVisual();
			}
		}

		private double maxZoom = 12;
		public double MaxZoom
		{
			get => maxZoom;
			set => maxZoom = value;
		}
		private double minZoom = 4;
		public double MinZoom
		{
			get => minZoom;
			set => minZoom = value;
		}

		private Dictionary<LandLayerType, TopologyMap> map;
		public Dictionary<LandLayerType, TopologyMap> Map
		{
			get => map;
			set
			{
				if (map == value)
					return;
				map = value;

				if (map != null)
					Task.Run(async () =>
					{
						await LandLayer.SetupMapAsync(map, (int)Math.Ceiling(MinZoom), (int)Math.Ceiling(MaxZoom));
						Invoke(new Action(LandLayer.Invalidate));
					});
			}
		}
		public MapProjection Projection { get; set; } = new MercatorProjection();

		public void RefleshResourceCache()
		{
			LandLayer.RefleshResourceCache();
			InvalidateChildVisual();
		}

		// 指定した範囲をすべて表示できるように調整する
		public void Navigate(RectD bound)
		{
			var boundPixel = new RectD(bound.BottomLeft.CastLocation().ToPixel(Projection, Zoom), bound.TopRight.CastLocation().ToPixel(Projection, Zoom));
			var centerPixel = CenterLocation.ToPixel(Projection, Zoom);
			var halfRect = new PointD(PaddedRect.Width / 2, PaddedRect.Height / 2);
			var leftTop = centerPixel - halfRect;
			var rightBottom = centerPixel + halfRect;
			Navigate(new NagivateAnimationParameter(
					Zoom,
					new RectD(leftTop, rightBottom),
					boundPixel));
		}
		internal void Navigate(NagivateAnimationParameter parameter)
		{
			var boundPixel = new RectD(parameter.ToRect.TopLeft, parameter.ToRect.BottomRight);
			var scale = new PointD(PaddedRect.Width / boundPixel.Width, PaddedRect.Height / boundPixel.Height);
			var relativeZoom = Math.Log(Math.Min(scale.X, scale.Y), 2);
			CenterLocation = new PointD(
				boundPixel.Left + boundPixel.Width / 2,
				boundPixel.Top + boundPixel.Height / 2).ToLocation(Projection, Zoom);
			Zoom += relativeZoom;
			return;
		}


		public RectD PaddedRect { get; private set; }

		private LandLayer LandLayer { get; set; }
		//private OverlayLayer OverlayLayer { get; set; }
		//private RealtimeOverlayLayer RealtimeOverlayLayer { get; set; }

		protected override void OnLoad(EventArgs e)
		{
			Controls.Add(LandLayer = new LandLayer
			{
				Projection = Projection,
				Zoom = Zoom,
				CenterLocation = CenterLocation,
				Dock = DockStyle.Fill,
			});
			LandLayer.RefleshResourceCache();
			// TODO: やっつけイベント
			LandLayer.MouseMove += (s, e2) => OnMouseMove(e2);
			LandLayer.MouseDown += (s, e2) => OnMouseDown(e2);

			if (Map != null)
			{
				var map = Map;
				var minZoom = MinZoom;
				var maxZoom = MaxZoom;
				Task.Run(async () =>
				{
					await LandLayer.SetupMapAsync(map, (int)Math.Ceiling(minZoom), (int)Math.Ceiling(maxZoom));
					Invoke(new Action(LandLayer.Invalidate));
				});
			}
			//Children.Add(OverlayLayer = new OverlayLayer
			//{
			//	Projection = Projection,
			//	Zoom = Zoom,
			//	CenterLocation = CenterLocation,
			//	RenderObjects = RenderObjects,
			//});
			//Children.Add(RealtimeOverlayLayer = new RealtimeOverlayLayer
			//{
			//	Projection = Projection,
			//	Zoom = Zoom,
			//	CenterLocation = CenterLocation,
			//	RealtimeRenderObjects = RealtimeRenderObjects,
			//});
			ApplySize();

			//NavigateAnimation.EasingFunction = new CircleEase { EasingMode = EasingMode.EaseOut };
			//NavigateAnimation.Completed += (s, e) =>
			//{
			//	AnimationParameter = null;
			//	NavigateAnimation.BeginTime = null;
			//};

			base.OnLoad(e);
		}
		protected override void OnSizeChanged(EventArgs e)
		{
			ApplySize();
			base.OnSizeChanged(e);
			InvalidateChildVisual();
		}
		private void ApplySize()
		{
			// DP Cache
			var renderSize = Size; //RenderSize;
			var padding = Padding;
			PaddedRect = new RectD(new PointD(padding.Left, padding.Top), new PointD(Math.Max(0, renderSize.Width - padding.Right), Math.Max(0, renderSize.Height - padding.Bottom)));
			var zoom = Zoom;
			var centerLocation = CenterLocation ?? new Location(0, 0); // null island

			var halfRenderSize = new PointD(PaddedRect.Width / 2, PaddedRect.Height / 2);
			// 左上/右下のピクセル座標
			var leftTop = centerLocation.ToPixel(Projection, zoom) - halfRenderSize - new PointD(padding.Left, padding.Top);
			var rightBottom = centerLocation.ToPixel(Projection, zoom) + halfRenderSize + new PointD(padding.Right, padding.Bottom);

			if (LandLayer != null)
			{
				LandLayer.LeftTopLocation = leftTop.ToLocation(Projection, zoom).CastPoint();
				LandLayer.ViewAreaRect = new RectD(LandLayer.LeftTopLocation, rightBottom.ToLocation(Projection, zoom).CastPoint());
			}
			//if (OverlayLayer != null)
			//{
			//	OverlayLayer.LeftTopPixel = leftTop;
			//	OverlayLayer.PixelBound = new Rect(leftTop, rightBottom);
			//}
			//if (RealtimeOverlayLayer != null)
			//{
			//	RealtimeOverlayLayer.LeftTopPixel = leftTop;
			//	RealtimeOverlayLayer.PixelBound = new Rect(leftTop, rightBottom);
			//}
		}

		public void InvalidateChildVisual()
		{
			LandLayer?.Invalidate();
			//OverlayLayer?.InvalidateVisual();
			//RealtimeOverlayLayer?.InvalidateVisual();
		}
	}
}
