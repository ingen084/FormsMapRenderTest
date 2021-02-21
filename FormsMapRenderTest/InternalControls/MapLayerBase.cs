using FormsMapRenderTest.Projections;
using KyoshinMonitorLib;
using System.Windows.Forms;

namespace FormsMapRenderTest.InternalControls
{
	internal abstract class MapLayerBase : Control
	{
		public MapProjection Projection { get; set; }
		public double Zoom { get; set; }
		public Location CenterLocation { get; set; }

		public MapLayerBase()
		{
			DoubleBuffered = true;
		}
	}
}
