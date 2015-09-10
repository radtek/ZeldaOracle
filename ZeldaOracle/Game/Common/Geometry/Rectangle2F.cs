﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace ZeldaOracle.Common.Geometry {
/** <summary>
 * The 2D double precision rectangle with numerous operations and functions.
 * </summary> */
public struct Rectangle2F : IShape2F {

	//========== CONSTANTS ===========
	#region Constants

	/** <summary> Returns an empty rectangle. </summary> */
	public static Rectangle2F Zero {
		get { return new Rectangle2F(); }
	}

	#endregion
	//=========== MEMBERS ============
	#region Members

	/** <summary> The position of the rectangle. </summary> */
	[ContentSerializerIgnore]
	public Vector2F Point;
	/** <summary> The size of the rectangle. </summary> */
	[ContentSerializerIgnore]
	public Vector2F Size;

	#endregion
	//========= CONSTRUCTORS =========
	#region Constructors

	/** <summary> Constructs a rectangle with the specified position and size. </summary> */
	public Rectangle2F(double x, double y, double width, double height) {
		this.Point	= new Vector2F(x, y);
		this.Size	= new Vector2F(width, height);
	}
	/** <summary> Constructs a rectangle with the specified position and size. </summary> */
	public Rectangle2F(Vector2F point, double width, double height) {
		this.Point	= point;
		this.Size	= new Vector2F(width, height);
	}
	/** <summary> Constructs a rectangle with the specified position and size. </summary> */
	public Rectangle2F(double x, double y, Vector2F size) {
		this.Point	= new Vector2F(x, y);
		this.Size	= size;
	}
	/** <summary> Constructs a rectangle with the specified position and size. </summary> */
	public Rectangle2F(Vector2F point, Vector2F size) {
		this.Point	= point;
		this.Size	= size;
	}
	/** <summary> Constructs a rectangle with the specified size. </summary> */
	public Rectangle2F(double size) {
		this.Point	= Vector2F.Zero;
		this.Size	= new Vector2F(size);
	}
	/** <summary> Constructs a rectangle with the specified size. </summary> */
	public Rectangle2F(double width, double height) {
		this.Point	= Vector2F.Zero;
		this.Size	= new Vector2F(width, height);
	}
	/** <summary> Constructs a rectangle with the specified size. </summary> */
	public Rectangle2F(Vector2F size) {
		this.Point	= Vector2F.Zero;
		this.Size	= size;
	}
	/** <summary> Constructs a copy of the specified rectangle. </summary> */
	public Rectangle2F(Rectangle2F r) {
		this.Point	= r.Point;
		this.Size	= r.Size;
	}

	#endregion
	//=========== GENERAL ============
	#region General

	/** <summary> Outputs a string representing this rectangle. </summary> */
	public override string ToString() {
		return "((" + X + "," + Y + "), (" + Width + "x" + Height + "))";
	}
	/** <summary> Outputs a string representing this rectangle. </summary> */
	public string ToString(IFormatProvider provider) {
		// TODO: Write formatting for Rectangle2D.ToString(format).

		return "((" + X + "," + Y + "), (" + Width + "x" + Height + "))";
	}
	/** <summary> Outputs a string representing this rectangle. </summary> */
	public string ToString(string format) {
		return "((" + X + "," + Y + "), (" + Width + "x" + Height + "))";
	}
	/** <summary> Outputs a string representing this rectangle. </summary> */
	public string ToString(string format, IFormatProvider provider) {
		return "((" + X + "," + Y + "), (" + Width + "x" + Height + "))";
	}
	/** <summary> Returns true if the specified rectangle has the same x and y coordinates. </summary> */
	public override bool Equals(object obj) {
		if (obj is Rectangle2F)
			return (Point == ((Rectangle2F)obj).Point && Size == ((Rectangle2F)obj).Size);
		return false;
	}
	/** <summary> Returns the hash code for this rectangle. </summary> */
	public override int GetHashCode() {
		return base.GetHashCode();
	}

	#endregion
	//========== OPERATORS ===========
	#region Operators
	//--------------------------------
	#region Unary Arithmetic

	public static Rectangle2F operator +(Rectangle2F r) {
		return r;
	}
	public static Rectangle2F operator -(Rectangle2F r) {
		return new Rectangle2F(r.Point, -r.Size);
	}
	public static Rectangle2F operator ++(Rectangle2F r) {
		return new Rectangle2F(++r.Point, r.Size);
	}
	public static Rectangle2F operator --(Rectangle2F r) {
		return new Rectangle2F(--r.Point, r.Size);
	}

	#endregion
	//--------------------------------
	#region Binary Arithmetic

	public static Rectangle2F operator +(Rectangle2F r, Vector2F v) {
		return new Rectangle2F(r.Point + v, r.Size);
	}
	public static Rectangle2F operator +(Rectangle2F r, double d) {
		return new Rectangle2F(r.Point + d, r.Size);
	}
	public static Rectangle2F operator -(Rectangle2F r, Vector2F v) {
		return new Rectangle2F(r.Point - v, r.Size);
	}
	public static Rectangle2F operator -(Rectangle2F r, double d) {
		return new Rectangle2F(r.Point - d, r.Size);
	}

	/** <summary> Translates the rectangle by the specified distance. </summary> */
	public IShape2F Translate(Vector2F v) {
		return this + v;
	}
	/** <summary> Translates the rectangle by the specified distance. </summary> */
	public IShape2F Translate(double d) {
		return this + d;
	}

	public static Rectangle2F operator *(Rectangle2F r, Vector2F v) {
		return new Rectangle2F(r.Point, r.Size * v);
	}
	public static Rectangle2F operator *(Rectangle2F r, double d) {
		return new Rectangle2F(r.Point, r.Size * d);
	}
	public static Rectangle2F operator /(Rectangle2F r, Vector2F v) {
		return new Rectangle2F(r.Point, r.Size / v);
	}
	public static Rectangle2F operator /(Rectangle2F r, double d) {
		return new Rectangle2F(r.Point, r.Size / d);
	}
	public static Rectangle2F operator %(Rectangle2F r, Vector2F v) {
		return new Rectangle2F(r.Point, r.Size % v);
	}
	public static Rectangle2F operator %(Rectangle2F r, double d) {
		return new Rectangle2F(r.Point, r.Size % d);
	}

	#endregion
	//--------------------------------
	#region Binary Logic

	public static bool operator ==(Rectangle2F r1, Rectangle2F r2) {
		return (r1.Point == r2.Point && r1.Size == r2.Size);
	}
	public static bool operator !=(Rectangle2F r1, Rectangle2F r2) {
		return (r1.Point != r2.Point || r1.Size != r2.Size);
	}

	#endregion
	//--------------------------------
	#region Conversion
	
	public static implicit operator Rectangle2F(Rectangle2I r) {
		return new Rectangle2F(r.X, r.Y, r.Width, r.Height);
	}
	public static implicit operator Rectangle2F(Rectangle r) {
		return new Rectangle2F(r.X, r.Y, r.Width, r.Height);
	}

	public static explicit operator Rectangle2I(Rectangle2F r) {
		return new Rectangle2I((int)r.Point.X, (int)r.Point.Y, (int)r.Size.X, (int)r.Size.Y);
	}
	public static explicit operator Rectangle(Rectangle2F r) {
		return new Rectangle((int)r.Point.X, (int)r.Point.Y, (int)r.Size.X, (int)r.Size.Y);
	}

	#endregion
	//--------------------------------
	#endregion
	//========== PROPERTIES ==========
	#region Properties
	//--------------------------------
	#region Dimensions

	/** <summary> Gets or sets the x position of the rectangle. </summary> */
	public double X {
		get { return Point.X; }
		set { Point.X = value; }
	}
	/** <summary> Gets or sets the y position of the rectangle. </summary> */
	public double Y {
		get { return Point.Y; }
		set { Point.Y = value; }
	}
	/** <summary> Gets or sets the width of the rectangle. </summary> */
	[ContentSerializer(ElementName = "W")]
	public double Width {
		get { return Size.X; }
		set { Size.X = value; }
	}
	/** <summary> Gets or sets the height of the rectangle. </summary> */
	[ContentSerializer(ElementName = "H")]
	public double Height {
		get { return Size.Y; }
		set { Size.Y = value; }
	}

	#endregion
	//--------------------------------
	#region Shape

	/** <summary> Returns true if the rectangle has an size of zero. </summary> */
	public bool IsEmpty {
		get { return Size.IsZero; }
	}
	/** <summary> Gets the center of the rectangle. </summary> */
	public Vector2F Center {
		get { return Point + Size / 2; }
	}
	/** <summary> Gets the bounding box of the rectangle. </summary> */
	public Rectangle2F Bounds {
		get { return this; }
	}
	/** <summary> Gets the maximum point of the rectangle. </summary> */
	public Vector2F Max {
		get { return GMath.Max(Point, Point + Size); }
	}
	/** <summary> Gets the minimum point of the rectangle. </summary> */
	public Vector2F Min {
		get { return GMath.Min(Point, Point + Size); }
	}

	/** <summary> Gets the area of the rectangle. </summary> */
	public double Area {
		get { return GMath.Abs(Size.X * Size.Y); }
	}
	/** <summary> Gets the perimeter of the rectangle. </summary> */
	public double Perimeter {
		get { return GMath.Abs(Size.X * 2) + GMath.Abs(Size.Y * 2); }
	}

	/** <summary> Gets the number of points in the rectangle. </summary> */
	public int NumPoints {
		get { return 2; }
	}
	/** <summary> Gets the number of lines in the rectangle. </summary> */
	public int NumLines {
		get { return 1; }
	}
	/** <summary> Gets the list of points in the rectangle. </summary> */
	public Vector2F[] Points {
		get {
			return new Vector2F[] {
				Point,
				Point + new Vector2F(Size.X, 0),
				Point + Size,
				Point + new Vector2F(0, Size.Y)
			};
		}
	}
	/** <summary> Gets the list of lines in the rectangle. </summary> */
	public Line2F[] Lines {
		get {
			return new Line2F[] {
				new Line2F(Point, Point + new Vector2F(Size.X, 0)),
				new Line2F(Point + new Vector2F(Size.X, 0), Point + Size),
				new Line2F(Point + Size, Point + new Vector2F(0, Size.Y)),
				new Line2F(Point + new Vector2F(0, Size.Y), Point)
			};
		}
	}

	#endregion
	//--------------------------------
	#endregion
	//========= CALCULATIONS =========
	#region Calculations

	/** <summary> Returns the point on the rectangle based on the its perimeter. </summary> */
	public Vector2F PositionAt(double length, bool asRatio = false) {
		if (asRatio)
			length *= Perimeter;
		if (length < 0 || length > Perimeter)
			return Point;
		foreach (Line2F l in Lines) {
			if (length <= l.Length)
				return l.PositionAt(length);
			length -= l.Length;
		}
		return Point;
	}

	/** <summary> Returns a rectangle with the corners stretched out by the specified amount. </summary> */
	public Rectangle2F Inflated(Vector2F amount) {
		return new Rectangle2F(Point - amount, Size + amount * 2.0);
	}
	/** <summary> Returns a rectangle with the corners stretched out by the specified amount. </summary> */
	public Rectangle2F Inflated(double x, double y) {
		return new Rectangle2F(Point - new Vector2F(x, y), Size + new Vector2F(x * 2.0, y * 2.0));
	}
	/** <summary> Stretches the corners of the rectangle out by the specified amount. </summary> */
	public void Inflate(Vector2F amount) {
		Point -= amount;
		Size += amount * 2.0;
	}
	/** <summary> Stretches the corners of the rectangle out by the specified amount. </summary> */
	public void Inflate(double x, double y) {
		Point.X -= x;
		Point.Y -= y;
		Size.X += x * 2.0;
		Size.Y += y * 2.0;
	}

	#endregion
	//=========== CONTAINS ===========
	#region Contains

	/** <summary> Returns true if the specified vector is inside this rectangle. </summary> */
	public bool Contains(Vector2F point) {
		return ((point <  Max) &&
				(point >= Min));
	}
	/** <summary> Returns true if the specified shape is inside this rectangle. </summary> */
	public bool Contains(IShape2F shape) {
		if (shape is Line2F)
			return Contains((Line2F)shape);
		if (shape is Rectangle2F)
			return Contains((Rectangle2F)shape);
		if (shape is Polygon2F)
			return Contains((Polygon2F)shape);
		return false;
	}
	/** <summary> Returns true if the specified line is inside this rectangle. </summary> */
	public bool Contains(Line2F line) {
		if (IsEmpty || line.IsEmpty)
			return false;
		return ((line.Min <  Max) &&
				(line.Max >= Min));
	}
	/** <summary> Returns true if the specified rectangle is inside this rectangle. </summary> */
	public bool Contains(Rectangle2F rect) {
		if (IsEmpty || rect.IsEmpty)
			return false;
		return ((rect.Min >= Min) &&
				(rect.Max <= Max));
	}
	/** <summary> Returns true if the specified polygon is inside this rectangle. </summary> */
	public bool Contains(Polygon2F poly) {
		if (IsEmpty || poly.IsEmpty)
			return false;
		return Contains(poly.Bounds);
	}

	#endregion
	//========== COLLISION ===========
	#region Collision

	/** <summary> Returns true if the specified vector is colliding with this rectangle. </summary> */
	public bool Colliding(Vector2F point) {
		return ((point <  Max) &&
				(point >= Min));
	}
	/** <summary> Returns true if the specified shape is colliding with this rectangle. </summary> */
	public bool Colliding(IShape2F shape) {
		if (shape is Line2F)
			return Colliding((Line2F)shape);
		if (shape is Rectangle2F)
			return Colliding((Rectangle2F)shape);
		if (shape is Polygon2F)
			return Colliding((Polygon2F)shape);
		return false;
	}
	/** <summary> Returns true if the specified line is colliding with this rectangle. </summary> */
	public bool Colliding(Line2F line) {
		if (IsEmpty || line.IsEmpty)
			return false;
		if (!Colliding(line.Bounds))
			return false;
		if (Contains(line.End1))
			return true;
		foreach (Line2F l in Lines) {
			if (l.Colliding(line))
				return true;
		}
		return false;
	}
	/** <summary> Returns true if the specified rectangle is colliding with this rectangle. </summary> */
	public bool Colliding(Rectangle2F rect) {
		if (IsEmpty || rect.IsEmpty)
			return false;
		return ((rect.Min < Max) &&
				(rect.Max > Min));
	}
	/** <summary> Returns true if the specified polygon is colliding with this rectangle. </summary> */
	public bool Colliding(Polygon2F poly) {
		if (IsEmpty || poly.IsEmpty)
			return false;
		if (!Colliding(poly.Bounds))
			return false;
		if (Contains(poly.PointList[0]))
			return true;
		if (poly.Contains(Point))
			return true;
		foreach (Line2F l1 in Lines) {
			foreach (Line2F l2 in poly.Lines) {
				if (l1.Colliding(l2))
					return true;
			}
		}
		return false;
	}

	#endregion
}
} // End namespace