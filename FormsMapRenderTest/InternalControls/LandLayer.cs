using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FormsMapRenderTest.InternalControls
{
	internal sealed class LandLayer : MapLayerBase
	{
		public PointD LeftTopLocation { get; set; }
		public RectD ViewAreaRect { get; set; }
		public LandLayerType PrimaryRenderLayer { get; set; } = LandLayerType.PrimarySubdivisionArea;

		private IDictionary<LandLayerType, FeatureCacheController> Controllers { get; set; }
		public async Task SetupMapAsync(Dictionary<LandLayerType, TopologyMap> mapCollection, int minZoom, int maxZoom)
		{
			var controllers = new ConcurrentDictionary<LandLayerType, FeatureCacheController>();
			await Task.WhenAll(mapCollection.Select(p => Task.Run(() =>
			{
				// 市区町村のデータがでかすぎるのでいったん読み込まない
				// TODO: 制限を解除する
				if (p.Key != LandLayerType.WorldWithoutJapan &&
					p.Key != LandLayerType.NationalAndRegionForecastArea &&
					p.Key != LandLayerType.PrefectureForecastArea &&
					p.Key != LandLayerType.PrimarySubdivisionArea)
					return;
				controllers[p.Key] = new FeatureCacheController(p.Key, p.Value);
				//controllers[k].GenerateCache(Projection, minZoom, maxZoom);
			})).ToArray());
			Controllers = controllers;
		}

		#region ResourceCache
		private Pen CoastlineStroke { get; set; } = new Pen(Brushes.Blue, 1);
		private Pen PrefStroke { get; set; } = new Pen(Brushes.Red, .6f);
		private Pen AreaStroke { get; set; } = new Pen(Brushes.Green, .4f);

		private Brush LandFill { get; set; } = Brushes.Snow;
		private Brush OverSeasLandFill { get; set; } = Brushes.DimGray;

		private bool InvalidateLandStroke { get; set; }
		private bool InvalidatePrefStroke { get; set; }
		private bool InvalidateAreaStroke { get; set; }

		public void RefleshResourceCache()
		{
			//CoastlineStroke = new Pen((Brush)FindResource("LandStrokeColor"), (double)FindResource("LandStrokeThickness"));
			//CoastlineStroke.Freeze();
			//PrefStroke = new Pen((Brush)FindResource("PrefStrokeColor"), (double)FindResource("PrefStrokeThickness"));
			//PrefStroke.Freeze();
			//AreaStroke = new Pen((Brush)FindResource("AreaStrokeColor"), (double)FindResource("AreaStrokeThickness"));
			//AreaStroke.Freeze();
			//LandFill = (Brush)FindResource("LandColor");
			//LandFill.Freeze();
			//OverSeasLandFill = (Brush)FindResource("OverseasLandColor");
			//OverSeasLandFill.Freeze();
			//InvalidateLandStroke = (double)FindResource("LandStrokeThickness") <= 0;
			//InvalidatePrefStroke = (double)FindResource("PrefStrokeThickness") <= 0;
			//InvalidateAreaStroke = (double)FindResource("AreaStrokeThickness") <= 0;
		}
		#endregion

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);

			//e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
			//e.Graphics.Clear(Color.Transparent);

			// コントローラーの初期化ができていなければスキップ
			if (Controllers == null)
				return;

			// 使用するキャッシュのズーム
			var baseZoom = (int)Math.Ceiling(Zoom);
			// 実際のズームに合わせるためのスケール
			var scale = Math.Pow(2, Zoom - baseZoom);

			var leftTop = LeftTopLocation.CastLocation().ToPixel(Projection, baseZoom);

			// とりあえず海外の描画を行う
			RenderOverseas(e.Graphics, leftTop, scale, baseZoom);

			var useLayerType = PrimaryRenderLayer;
			if (baseZoom <= 5)
				useLayerType = LandLayerType.NationalAndRegionForecastArea;
			else if (baseZoom <= 7)
				useLayerType = LandLayerType.PrefectureForecastArea;
			else if (baseZoom <= 10)
				useLayerType = LandLayerType.PrimarySubdivisionArea;


			foreach (var f in Controllers[useLayerType].Find(ViewAreaRect))
			{
				switch (f.Type)
				{
					case FeatureType.Polygon:
						f.Draw(e.Graphics, Projection, baseZoom, LandFill, null, leftTop, scale);
						break;
					case FeatureType.AdminBoundary:
						if (!InvalidatePrefStroke && baseZoom > 5)
							f.Draw(e.Graphics, Projection, baseZoom, null, PrefStroke, leftTop, scale);
						break;
					case FeatureType.Coastline:
						if (!InvalidateLandStroke && baseZoom > 5)
							f.Draw(e.Graphics, Projection, baseZoom, null, CoastlineStroke, leftTop, scale);
						break;
					case FeatureType.AreaBoundary:
						if (!InvalidateAreaStroke && baseZoom > 5)
							f.Draw(e.Graphics, Projection, baseZoom, null, AreaStroke, leftTop, scale);
						break;
				}
			}
		}
		/// <summary>
		/// 海外を描画する
		/// </summary>
		private void RenderOverseas(Graphics g, PointD leftTop, double scale, int baseZoom)
		{
			if (!Controllers.ContainsKey(LandLayerType.WorldWithoutJapan))
				return;

			foreach (var f in Controllers[LandLayerType.WorldWithoutJapan].Find(ViewAreaRect))
			{
				if (f.Type != FeatureType.Polygon)
					continue;
				f.Draw(g, Projection, baseZoom, OverSeasLandFill, null, leftTop, scale);
			}
		}
	}
}
