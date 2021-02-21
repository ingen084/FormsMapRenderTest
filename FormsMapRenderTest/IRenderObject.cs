using FormsMapRenderTest.Projections;
using System.Drawing;

namespace FormsMapRenderTest
{
	public interface IRenderObject
	{
		void Render(Graphics context, RectD viewRect, double zoom, PointD leftTopPixel, bool isDarkTheme, MapProjection projection);
	}
}
