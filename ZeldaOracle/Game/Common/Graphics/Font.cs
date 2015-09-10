﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XnaColor		= Microsoft.Xna.Framework.Color;

using ZeldaOracle.Common.Geometry;

namespace ZeldaOracle.Common.Graphics {
/** <summary>
 * An font containing a sprite font.
 * </summary> */
public class Font {

	//========== CONSTANTS ===========
	#region Constants

	#endregion
	//=========== MEMBERS ============
	#region Members
	
	/** <summary> The sprite font of the font. </summary> */
	private SpriteFont spriteFont;
	/** <summary> The file path of the font. </summary> */
	private string filePath;

	#endregion
	//========= CONSTRUCTORS =========
	#region Constructors

	/** <summary> Constructs a font with the specified sprite font. </summary> */
	public Font(SpriteFont spriteFont, string filePath = "") {
		this.spriteFont		= spriteFont;
		this.filePath		= filePath;
	}
	/** <summary> Load a font from the specified file path. </summary> */
	public Font(ContentManager content, string filePath) {
		if (filePath.Length != 0)
			this.spriteFont	= content.Load<SpriteFont>(filePath);
		else
			this.spriteFont	= null;
		this.filePath		= filePath;
	}

	#endregion
	//========== OPERATORS ===========
	#region Operators

	public static implicit operator SpriteFont(Font font) {
		return font.spriteFont;
	}

	#endregion
	//========== PROPERTIES ==========
	#region Properties
	//--------------------------------
	#region Dimensions

	/** <summary> Gets or sets the spacing of the font characters. </summary> */
	public double CharacterSpacing {
		get { return (double)spriteFont.Spacing; }
		set { spriteFont.Spacing = (float)value; }
	}
	/** <summary> Gets or sets the vertical distance (in pixels) between the base lines of two consecutive lines of text. </summary> */
	public int LineSpacing {
		get { return spriteFont.LineSpacing; }
		set { spriteFont.LineSpacing = (int)value; }
	}

	#endregion
	//--------------------------------
	#region Information

	/** <summary> Gets the sprite font of the font. </summary> */
	public SpriteFont SpriteFont {
		get { return spriteFont; }
	}
	/** <summary> Gets the file path of the font. </summary> */
	public string FilePath {
		get { return filePath; }
	}

	#endregion
	//--------------------------------
	#endregion
	//========== MANAGEMENT ==========
	#region Management
	
	/** <summary> Returns the width and height of a string. </summary> */
	public Vector2F MeasureString(string text) {
		return (Vector2F)spriteFont.MeasureString(text);
	}
	/** <summary> Returns the width and height of a string. </summary> */
	public Rectangle2F MeasureStringBounds(string text, Align alignment) {
		Rectangle2F stringBounds = new Rectangle2F(Vector2F.Zero, spriteFont.MeasureString(text));
		bool intAlign = (alignment & Align.Int) != 0;
		if (((alignment & Align.Left) != 0) == ((alignment & Align.Right) != 0))
			stringBounds.X -= (intAlign ? (int)(stringBounds.Width / 2.0) : (stringBounds.Width / 2.0));
		else if ((alignment & Align.Right) != 0)
			stringBounds.X -= stringBounds.Width;
		if (((alignment & Align.Top) != 0) == ((alignment & Align.Bottom) != 0))
			stringBounds.Y -= (intAlign ? (int)(stringBounds.Height / 2.0) : (stringBounds.Height / 2.0));
		else if ((alignment & Align.Bottom) != 0)
			stringBounds.Y -= stringBounds.Height;
		return stringBounds;
	}
	/** <summary> Loads the image from the file path. </summary> */
	public void Load(ContentManager content) {
		if (spriteFont == null && filePath.Length != 0)
			spriteFont = content.Load<SpriteFont>(filePath);
	}

	#endregion
}
} // End namespace