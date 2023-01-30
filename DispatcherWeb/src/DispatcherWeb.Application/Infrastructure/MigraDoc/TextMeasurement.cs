#region MigraDoc - Creating Documents on the Fly
//
// Authors:
//   Stefan Lange
//   Klaus Potzesny
//   David Stephensen
//
// Copyright (c) 2001-2009 empira Software GmbH, Cologne (Germany)
//
// http://www.pdfsharp.com
// http://www.migradoc.com
// http://sourceforge.net/projects/pdfsharp

// Modifications by:
//   Thomas Hövel
//   Copyright (c) 2015 TH Software, Troisdorf (Germany), http://developer.th-soft.com/developer/
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included
// in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Diagnostics;
using System.ComponentModel;
using PdfSharp.Drawing;

namespace MigraDoc.DocumentObjectModel
{
	/// <summary>
	/// Provides functionality to measure the width of text during document design time.
	/// </summary>
	public sealed class TextMeasurement
	{
		/// <summary>
		/// Initializes a new instance of the TextMeasurement class with the specified font.
		/// </summary>
		public TextMeasurement(Font font)
		{
			if(font == null)
				throw new ArgumentNullException("font");

			_font = font;
		}

		/// <summary>
		/// Returns the size of the bounding box of the specified text.
		/// </summary>
		public XSize MeasureString(string text, UnitType unitType)
		{
			if(text == null)
				throw new ArgumentNullException("text");

			if(!Enum.IsDefined(typeof(UnitType), unitType))
				throw new InvalidEnumArgumentException();

			XGraphics graphics = Realize();

			XSize size = graphics.MeasureString(text, _gdiFont/*, new XPoint(0, 0), StringFormat.GenericTypographic*/);
			switch(unitType)
			{
				case UnitType.Point:
					break;

				case UnitType.Centimeter:
					size.Width = (float)(size.Width * 2.54 / 72);
					size.Height = (float)(size.Height * 2.54 / 72);
					break;

				case UnitType.Inch:
					size.Width = size.Width / 72;
					size.Height = size.Height / 72;
					break;

				case UnitType.Millimeter:
					size.Width = (float)(size.Width * 25.4 / 72);
					size.Height = (float)(size.Height * 25.4 / 72);
					break;

				case UnitType.Pica:
					size.Width = size.Width / 12;
					size.Height = size.Height / 12;
					break;

				default:
					Debug.Assert(false, "Missing unit type");
					break;
			}
			return size;
		}

		/// <summary>
		/// Returns the size of the bounding box of the specified text in point.
		/// </summary>
		public XSize MeasureString(string text)
		{
			return MeasureString(text, UnitType.Point);
		}

		/// <summary>
		/// Gets or sets the font used for measurement.
		/// </summary>
		public Font Font
		{
			get { return _font; }
			set
			{
				if(value == null)
					throw new ArgumentNullException("value");
				if(_font != value)
				{
					_font = value;
					_gdiFont = null;
				}
			}
		}
		Font _font;

		/// <summary>
		/// Initializes appropriate GDI+ objects.
		/// </summary>
		XGraphics Realize()
		{
			if(_graphics == null)
				_graphics = XGraphics.CreateMeasureContext(new XSize(2000, 2000), XGraphicsUnit.Point, XPageDirection.Downwards);

			//this.graphics.PageUnit = GraphicsUnit.Point;

			if(_gdiFont == null)
			{
				XFontStyle style = XFontStyle.Regular;
				if(_font.Bold)
					style |= XFontStyle.Bold;
				if(_font.Italic)
					style |= XFontStyle.Italic;

				_gdiFont = new XFont(_font.Name, _font.Size, style/*, System.Drawing.GraphicsUnit.Point*/);
			}
			return _graphics;
		}

		XFont _gdiFont;
		XGraphics _graphics;
	}
}