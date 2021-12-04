using SkiaSharp;
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
		private SKPaint CoastlineStroke { get; set; } = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Green,
			StrokeWidth = 1,
			IsAntialias = true,
		};
		private SKPaint PrefStroke { get; set; } = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Green,
			StrokeWidth = .8f,
			IsAntialias = true,
		};
		private SKPaint AreaStroke { get; set; } = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			Color = SKColors.Green,
			StrokeWidth = .4f,
			IsAntialias = true,
		};

		private SKPaint LandFill { get; set; } = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = new SKColor(242, 239, 233),
		};
		private SKPaint OverSeasLandFill { get; set; } = new SKPaint
		{
			Style = SKPaintStyle.Fill,
			Color = new SKColor(169, 169, 169),
		};

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

		public override void OnRender(SKCanvas canvas)
		{
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
			RenderOverseas(canvas, leftTop, scale, baseZoom);

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
						f.Draw(canvas, Projection, baseZoom, LandFill, leftTop, scale);
						break;
					case FeatureType.AdminBoundary:
						if (!InvalidatePrefStroke && baseZoom > 5)
							f.Draw(canvas, Projection, baseZoom, PrefStroke, leftTop, scale);
						break;
					case FeatureType.Coastline:
						if (!InvalidateLandStroke && baseZoom > 5)
							f.Draw(canvas, Projection, baseZoom, CoastlineStroke, leftTop, scale);
						break;
					case FeatureType.AreaBoundary:
						if (!InvalidateAreaStroke && baseZoom > 5)
							f.Draw(canvas, Projection, baseZoom, AreaStroke, leftTop, scale);
						break;
				}
			}
		}
		/// <summary>
		/// 海外を描画する
		/// </summary>
		private void RenderOverseas(SKCanvas canvas, PointD leftTop, double scale, int baseZoom)
		{
			if (!Controllers.ContainsKey(LandLayerType.WorldWithoutJapan))
				return;

			foreach (var f in Controllers[LandLayerType.WorldWithoutJapan].Find(ViewAreaRect))
			{
				if (f.Type != FeatureType.Polygon)
					continue;
				f.Draw(canvas, Projection, baseZoom, OverSeasLandFill, leftTop, scale);
			}
		}
	}
}
