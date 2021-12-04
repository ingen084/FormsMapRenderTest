using FormsMapRenderTest.Projections;
using KyoshinMonitorLib;
using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace FormsMapRenderTest
{
	public class Feature
	{
		public Feature(TopologyMap map, int index)
		{
			switch (map.Arcs[index].Type)
			{
				case TopologyArcType.Coastline:
					Type = FeatureType.Coastline;
					break;
				case TopologyArcType.Admin:
					Type = FeatureType.AdminBoundary;
					break;
				case TopologyArcType.Area:
					Type = FeatureType.AreaBoundary;
					break;
				default:
					throw new NotImplementedException("未定義のTopologyArcTypeです");
			};
			Points = map.Arcs[index].Arc.ToLocations(map);
			IsClosed =
				Math.Abs(Points[0].Latitude - Points[Points.Length - 1].Latitude) < 0.001 &&
				Math.Abs(Points[0].Longitude - Points[Points.Length - 1].Longitude) < 0.001;

			// バウンドボックスを求める
			var minLoc = new Location(float.MaxValue, float.MaxValue);
			var maxLoc = new Location(float.MinValue, float.MinValue);
			foreach (var l in Points)
			{
				minLoc.Latitude = Math.Min(minLoc.Latitude, l.Latitude);
				minLoc.Longitude = Math.Min(minLoc.Longitude, l.Longitude);

				maxLoc.Latitude = Math.Max(maxLoc.Latitude, l.Latitude);
				maxLoc.Longitude = Math.Max(maxLoc.Longitude, l.Longitude);
			}
			BB = new RectD(minLoc.CastPoint(), maxLoc.CastPoint());
		}
		public Feature(TopologyMap map, Feature[] lineFeatures, TopologyPolygon topologyPolygon)
		{
			Type = FeatureType.Polygon;
			LineFeatures = lineFeatures;
			IsClosed = true;

			var polyIndexes = topologyPolygon.Arcs;

			PolyIndexes = polyIndexes;

			// バウンドボックスを求めるために地理座標の計算をしておく
			var points = new List<Location>();
			foreach (var i in PolyIndexes[0])
			{
				if (points.Count == 0)
				{
					if (i < 0)
						points.AddRange(map.Arcs[Math.Abs(i) - 1].Arc.ToLocations(map).Reverse());
					else
						points.AddRange(map.Arcs[i].Arc.ToLocations(map));
					continue;
				}

				if (i < 0)
					points.AddRange(map.Arcs[Math.Abs(i) - 1].Arc.ToLocations(map).Reverse().Skip(1));
				else
					points.AddRange(map.Arcs[i].Arc.ToLocations(map).Skip(1));
			}

			// バウンドボックスを求める
			var minLoc = new Location(float.MaxValue, float.MaxValue);
			var maxLoc = new Location(float.MinValue, float.MinValue);
			foreach (var l in points)
			{
				minLoc.Latitude = Math.Min(minLoc.Latitude, l.Latitude);
				minLoc.Longitude = Math.Min(minLoc.Longitude, l.Longitude);

				maxLoc.Latitude = Math.Max(maxLoc.Latitude, l.Latitude);
				maxLoc.Longitude = Math.Max(maxLoc.Longitude, l.Longitude);
			}
			BB = new RectD(minLoc.CastPoint(), maxLoc.CastPoint());

			Code = topologyPolygon.Code;
		}
		private Feature[] LineFeatures { get; }
		public RectD BB { get; }
		public bool IsClosed { get; }
		private Location[] Points { get; }
		private int[][] PolyIndexes { get; }
		public FeatureType Type { get; }

		public int? Code { get; }

		private ConcurrentDictionary<int, PointD[][]> ReducedPointsCache { get; set; } = new ConcurrentDictionary<int, PointD[][]>();

		public PointD[][] GetOrCreatePointsCache(MapProjection proj, int zoom)
		{
			if (ReducedPointsCache.ContainsKey(zoom))
				return ReducedPointsCache[zoom];

			if (Type != FeatureType.Polygon)
			{
				var p = Points.ToPixedAndRedction(proj, zoom, IsClosed);
				return ReducedPointsCache[zoom] = p == null ? null : new[] { p };
			}

			var pointsList = new List<List<PointD>>();

			foreach (var polyIndex in PolyIndexes)
			{
				var points = new List<PointD>();
				foreach (var i in polyIndex)
				{
					if (points.Count == 0)
					{
						if (i < 0)
						{
							var p = LineFeatures[Math.Abs(i) - 1].GetOrCreatePointsCache(proj, zoom);
							if (p != null)
								points.AddRange(p[0].Reverse());
						}
						else
						{
							var p = LineFeatures[i].GetOrCreatePointsCache(proj, zoom);
							if (p != null)
								points.AddRange(p[0]);
						}
						continue;
					}

					if (i < 0)
					{
						var p = LineFeatures[Math.Abs(i) - 1].GetOrCreatePointsCache(proj, zoom);
						if (p != null)
							points.AddRange(p[0].Reverse().Skip(1));
					}
					else
					{
						var p = LineFeatures[i].GetOrCreatePointsCache(proj, zoom);
						if (p != null)
							points.AddRange(p[0].Skip(1));
					}
				}
				if (points.Count > 0)
					pointsList.Add(points);
			}

			return ReducedPointsCache[zoom] = !pointsList.Any(p => p.Any()) ? null : pointsList.Select(p => p.ToArray()).ToArray();
		}

		public void ClearCache()
		{
			ReducedPointsCache.Clear();
		}

		public void Draw(SKCanvas canvas, MapProjection proj, int zoom, SKPaint paint, PointD leftTop, double scale)
		{
			var pointsList = GetOrCreatePointsCache(proj, zoom);
			if (pointsList == null)
				return;

			using (var path = new SKPath())
				for (var p = 0; p < pointsList.Length; p++)
				{
					var points = pointsList[p];
					var length = points.Length;
					for (var i = 0; i < length; i++)
					{
						var point = new SKPoint(
							(float)((points[i].X - leftTop.X) * scale),
							(float)((points[i].Y - leftTop.Y) * scale));
						if (i == 0)
							path.MoveTo(point);
						else
							path.LineTo(point);
					}

					if (Type == FeatureType.Polygon || IsClosed)
						path.Close();
					canvas.DrawPath(path, paint);
				}
		}
	}
	public enum FeatureType
	{
		/// <summary>
		/// 海岸線
		/// </summary>
		Coastline,
		/// <summary>
		/// 行政境界
		/// </summary>
		AdminBoundary,
		/// <summary>
		/// ポリゴン
		/// </summary>
		Polygon,
		/// <summary>
		/// サブ行政境界(市区町村)
		/// </summary>
		AreaBoundary,
	}
}
