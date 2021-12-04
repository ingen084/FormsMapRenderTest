using FormsMapRenderTest.Projections;
using KyoshinMonitorLib;
using SkiaSharp;

namespace FormsMapRenderTest.InternalControls
{
	internal abstract class MapLayerBase
	{
		public MapProjection Projection { get; set; }
		public double Zoom { get; set; }
		public Location CenterLocation { get; set; }


		public abstract void OnRender(SKCanvas canvas);
	}
}
