using System;
using System.Diagnostics;

namespace FormsMapRenderTest
{
	public struct RectD
	{
		public RectD(double x,
					double y,
					double width,
					double height)
		{
			if (width < 0 || height < 0)
				throw new ArgumentException();

			_x = x;
			_y = y;
			_width = width;
			_height = height;
		}
		public RectD(PointD point1,
					PointD point2)
		{
			_x = Math.Min(point1.X, point2.X);
			_y = Math.Min(point1.Y, point2.Y);

			//  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)
			_width = Math.Max(Math.Max(point1.X, point2.X) - _x, 0);
			_height = Math.Max(Math.Max(point1.Y, point2.Y) - _y, 0);
		}

		public static RectD Empty => s_empty;

		public bool IsEmpty
		{
			get
			{
				// The funny width and height tests are to handle NaNs
				Debug.Assert((!(_width < 0) && !(_height < 0)) || (this == Empty));

				return _width < 0;
			}
		}

		/// <summary>
		/// Location - The PointD representing the origin of the Rectangle
		/// </summary>
		public PointD Location
		{
			get => new PointD(_x, _y);
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException();

				_x = value.X;
				_y = value.Y;
			}
		}

		/// <summary>
		/// Size - The Size representing the area of the Rectangle
		/// </summary>
		public PointD Size
		{
			get
			{
				if (IsEmpty)
					return new PointD(0, 0);
				return new PointD(_width, _height);
			}
			set
			{
				if (value.X == 0 && value.Y == 0)
					this = s_empty;
				else
				{
					if (IsEmpty)
						throw new InvalidOperationException();

					_width = value.X;
					_height = value.Y;
				}
			}
		}

		public double X
		{
			get => _x;
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException();

				_x = value;
			}
		}

		public double Y
		{
			get => _y;
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException();

				_y = value;
			}
		}

		/// <summary>
		/// Width - The Width component of the Size.  This cannot be set to negative, and will only
		/// be negative if this is the empty rectangle, in which case it will be negative infinity.
		/// If this RectD is Empty, setting this property is illegal.
		/// </summary>
		public double Width
		{
			get => _width;
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException();

				if (value < 0)
					throw new ArgumentException();

				_width = value;
			}
		}

		/// <summary>
		/// Height - The Height component of the Size.  This cannot be set to negative, and will only
		/// be negative if this is the empty rectangle, in which case it will be negative infinity.
		/// If this RectD is Empty, setting this property is illegal.
		/// </summary>
		public double Height
		{
			get => _height;
			set
			{
				if (IsEmpty)
					throw new InvalidOperationException();

				if (value < 0)
					throw new ArgumentException();

				_height = value;
			}
		}

		/// <summary>
		/// Left Property - This is a read-only alias for X
		/// If this is the empty rectangle, the value will be positive infinity.
		/// </summary>
		public double Left => _x;

		/// <summary>
		/// Top Property - This is a read-only alias for Y
		/// If this is the empty rectangle, the value will be positive infinity.
		/// </summary>
		public double Top => _y;

		/// <summary>
		/// Right Property - This is a read-only alias for X + Width
		/// If this is the empty rectangle, the value will be negative infinity.
		/// </summary>
		public double Right
		{
			get
			{
				if (IsEmpty)
					return double.NegativeInfinity;

				return _x + _width;
			}
		}

		/// <summary>
		/// Bottom Property - This is a read-only alias for Y + Height
		/// If this is the empty rectangle, the value will be negative infinity.
		/// </summary>
		public double Bottom
		{
			get
			{
				if (IsEmpty)
					return double.NegativeInfinity;

				return _y + _height;
			}
		}

		/// <summary>
		/// TopLeft Property - This is a read-only alias for the PointD which is at X, Y
		/// If this is the empty rectangle, the value will be positive infinity, positive infinity.
		/// </summary>
		public PointD TopLeft => new PointD(Left, Top);

		/// <summary>
		/// TopRight Property - This is a read-only alias for the PointD which is at X + Width, Y
		/// If this is the empty rectangle, the value will be negative infinity, positive infinity.
		/// </summary>
		public PointD TopRight => new PointD(Right, Top);

		/// <summary>
		/// BottomLeft Property - This is a read-only alias for the PointD which is at X, Y + Height
		/// If this is the empty rectangle, the value will be positive infinity, negative infinity.
		/// </summary>
		public PointD BottomLeft => new PointD(Left, Bottom);

		/// <summary>
		/// BottomRight Property - This is a read-only alias for the PointD which is at X + Width, Y + Height
		/// If this is the empty rectangle, the value will be negative infinity, negative infinity.
		/// </summary>
		public PointD BottomRight => new PointD(Right, Bottom);

		/// <summary>
		/// Contains - Returns true if the PointD is within the rectangle, inclusive of the edges.
		/// Returns false otherwise.
		/// </summary>
		/// <param name="point"> The PointD which is being tested </param>
		/// <returns>
		/// Returns true if the PointD is within the rectangle.
		/// Returns false otherwise
		/// </returns>
		public bool Contains(PointD point) => Contains(point.X, point.Y);

		/// <summary>
		/// Contains - Returns true if the PointD represented by x,y is within the rectangle inclusive of the edges.
		/// Returns false otherwise.
		/// </summary>
		/// <param name="x"> X coordinate of the PointD which is being tested </param>
		/// <param name="y"> Y coordinate of the PointD which is being tested </param>
		/// <returns>
		/// Returns true if the PointD represented by x,y is within the rectangle.
		/// Returns false otherwise.
		/// </returns>
		public bool Contains(double x, double y)
		{
			if (IsEmpty)
				return false;

			return ContainsInternal(x, y);
		}

		/// <summary>
		/// Contains - Returns true if the RectD non-Empty and is entirely contained within the
		/// rectangle, inclusive of the edges.
		/// Returns false otherwise
		/// </summary>
		public bool Contains(RectD rect)
		{
			if (IsEmpty || rect.IsEmpty)
				return false;

			return (_x <= rect._x &&
					_y <= rect._y &&
					_x + _width >= rect._x + rect._width &&
					_y + _height >= rect._y + rect._height);
		}

		/// <summary>
		/// IntersectsWith - Returns true if the RectD intersects with this rectangle
		/// Returns false otherwise.
		/// Note that if one edge is coincident, this is considered an intersection.
		/// </summary>
		/// <returns>
		/// Returns true if the RectD intersects with this rectangle
		/// Returns false otherwise.
		/// or Height
		/// </returns>
		/// <param name="rect"> RectD </param>
		public bool IntersectsWith(RectD rect)
		{
			if (IsEmpty || rect.IsEmpty)
			{
				return false;
			}

			return (rect.Left <= Right) &&
				   (rect.Right >= Left) &&
				   (rect.Top <= Bottom) &&
				   (rect.Bottom >= Top);
		}

		/// <summary>
		/// Intersect - Update this rectangle to be the intersection of this and rect
		/// If either this or RectD are Empty, the result is Empty as well.
		/// </summary>
		/// <param name="rect"> The RectD to intersect with this </param>
		public void Intersect(RectD rect)
		{
			if (!IntersectsWith(rect))
				this = Empty;
			else
			{
				double left = Math.Max(Left, rect.Left);
				double top = Math.Max(Top, rect.Top);

				//  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)
				_width = Math.Max(Math.Min(Right, rect.Right) - left, 0);
				_height = Math.Max(Math.Min(Bottom, rect.Bottom) - top, 0);

				_x = left;
				_y = top;
			}
		}

		/// <summary>
		/// Intersect - Return the result of the intersection of rect1 and rect2.
		/// If either this or RectD are Empty, the result is Empty as well.
		/// </summary>
		public static RectD Intersect(RectD rect1, RectD rect2)
		{
			rect1.Intersect(rect2);
			return rect1;
		}

		/// <summary>
		/// Union - Update this rectangle to be the union of this and rect.
		/// </summary>
		public void Union(RectD rect)
		{
			if (IsEmpty)
				this = rect;
			else if (!rect.IsEmpty)
			{
				double left = Math.Min(Left, rect.Left);
				double top = Math.Min(Top, rect.Top);


				// We need this check so that the math does not result in NaN
				if ((rect.Width == double.PositiveInfinity) || (Width == double.PositiveInfinity))
				{
					_width = double.PositiveInfinity;
				}
				else
				{
					//  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)                    
					double maxRight = Math.Max(Right, rect.Right);
					_width = Math.Max(maxRight - left, 0);
				}

				// We need this check so that the math does not result in NaN
				if ((rect.Height == double.PositiveInfinity) || (Height == double.PositiveInfinity))
				{
					_height = double.PositiveInfinity;
				}
				else
				{
					//  Max with 0 to prevent double weirdness from causing us to be (-epsilon..0)
					double maxBottom = Math.Max(Bottom, rect.Bottom);
					_height = Math.Max(maxBottom - top, 0);
				}

				_x = left;
				_y = top;
			}
		}

		/// <summary>
		/// Union - Return the result of the union of rect1 and rect2.
		/// </summary>
		public static RectD Union(RectD rect1, RectD rect2)
		{
			rect1.Union(rect2);
			return rect1;
		}

		/// <summary>
		/// Union - Update this rectangle to be the union of this and point.
		/// </summary>
		public void Union(PointD point)
		{
			Union(new RectD(point, point));
		}

		/// <summary>
		/// Union - Return the result of the union of RectD and point.
		/// </summary>
		public static RectD Union(RectD rect, PointD point)
		{
			rect.Union(new RectD(point, point));
			return rect;
		}

		public void Offset(PointD offsetVector)
		{
			if (IsEmpty)
				throw new InvalidOperationException();

			_x += offsetVector.X;
			_y += offsetVector.Y;
		}
		public void Offset(double offsetX, double offsetY)
		{
			if (IsEmpty)
				throw new InvalidOperationException();

			_x += offsetX;
			_y += offsetY;
		}
		public static RectD Offset(RectD rect, PointD offsetVector)
		{
			rect.Offset(offsetVector.X, offsetVector.Y);
			return rect;
		}
		public static RectD Offset(RectD rect, double offsetX, double offsetY)
		{
			rect.Offset(offsetX, offsetY);
			return rect;
		}

		/// <summary>
		/// Scale the rectangle in the X and Y directions
		/// </summary>
		/// <param name="scaleX"> The scale in X </param>
		/// <param name="scaleY"> The scale in Y </param>
		public void Scale(double scaleX, double scaleY)
		{
			if (IsEmpty)
				return;

			_x *= scaleX;
			_y *= scaleY;
			_width *= scaleX;
			_height *= scaleY;

			// If the scale in the X dimension is negative, we need to normalize X and Width
			if (scaleX < 0)
			{
				// Make X the left-most edge again
				_x += _width;

				// and make Width positive
				_width *= -1;
			}

			// Do the same for the Y dimension
			if (scaleY < 0)
			{
				// Make Y the top-most edge again
				_y += _height;

				// and make Height positive
				_height *= -1;
			}
		}
		private bool ContainsInternal(double x, double y)
		{
			// We include points on the edge as "contained".
			// We do "x - _width <= _x" instead of "x <= _x + _width"
			// so that this check works when _width is PositiveInfinity
			// and _x is NegativeInfinity.
			return ((x >= _x) && (x - _width <= _x) &&
					(y >= _y) && (y - _height <= _y));
		}

		static private RectD CreateEmptyRectD()
		{
			RectD rect = new RectD
			{
				_x = double.PositiveInfinity,
				_y = double.PositiveInfinity,
				_width = double.NegativeInfinity,
				_height = double.NegativeInfinity
			};
			return rect;
		}

		private readonly static RectD s_empty = CreateEmptyRectD();


		public static bool operator ==(RectD rect1, RectD rect2)
		{
			return rect1.X == rect2.X &&
				   rect1.Y == rect2.Y &&
				   rect1.Width == rect2.Width &&
				   rect1.Height == rect2.Height;
		}

		public static bool operator !=(RectD rect1, RectD rect2)
		{
			return !(rect1 == rect2);
		}
		public static bool Equals(RectD rect1, RectD rect2)
		{
			if (rect1.IsEmpty)
				return rect2.IsEmpty;
			return rect1.X.Equals(rect2.X) &&
				   rect1.Y.Equals(rect2.Y) &&
				   rect1.Width.Equals(rect2.Width) &&
				   rect1.Height.Equals(rect2.Height);
		}

		public override bool Equals(object o)
		{
			if ((null == o) || !(o is RectD))
			{
				return false;
			}

			RectD value = (RectD)o;
			return Equals(this, value);
		}

		public bool Equals(RectD value) => Equals(this, value);
		public override int GetHashCode()
		{
			if (IsEmpty)
				return 0;
			// Perform field-by-field XOR of HashCodes
			return X.GetHashCode() ^
				   Y.GetHashCode() ^
				   Width.GetHashCode() ^
				   Height.GetHashCode();
		}

		/// <summary>
		/// Creates a string representation of this object based on the current culture.
		/// </summary>
		/// <returns>
		/// A string representation of this object.
		/// </returns>
		public override string ToString()
		{
			// Delegate to the internal method which implements all ToString calls.
			return ConvertToString(null /* format string */, null /* format provider */);
		}

		/// <summary>
		/// Creates a string representation of this object based on the format string
		/// and IFormatProvider passed in.
		/// If the provider is null, the CurrentCulture is used.
		/// See the documentation for IFormattable for more information.
		/// </summary>
		/// <returns>
		/// A string representation of this object.
		/// </returns>
		internal string ConvertToString(string format, IFormatProvider provider)
		{
			if (IsEmpty)
				return "Empty";

			return string.Format(provider,
								 "{1:" + format + "}{0}{2:" + format + "}{0}{3:" + format + "}{0}{4:" + format + "}",
								 ',',
								 _x,
								 _y,
								 _width,
								 _height);
		}

		internal double _x;
		internal double _y;
		internal double _width;
		internal double _height;
	}
}
