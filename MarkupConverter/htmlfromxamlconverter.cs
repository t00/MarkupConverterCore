//---------------------------------------------------------------------------
// 
// File: HtmlFromXamlConverter.cs
//
// Copyright (C) Microsoft Corporation.  All rights reserved.
//
// Description: Prototype for Xaml - Html conversion 
//
//---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

namespace MarkupConverter
{
	/// <summary>
	/// HtmlToXamlConverter is a static class that takes an HTML string
	/// and converts it into XAML
	/// </summary>
	public static class HtmlFromXamlConverter
	{
		// ---------------------------------------------------------------------
		//
		// Internal Methods
		//
		// ---------------------------------------------------------------------

		/// <summary>
		/// Main entry point for Xaml-to-Html converter.
		/// Converts a xaml string into html string.
		/// </summary>
		/// <param name="xamlString">
		/// Xaml strinng to convert.
		/// </param>
		/// <param name="options">Conversion options</param>
		/// <returns>
		/// Html string produced from a source xaml.
		/// </returns>
		public static string ConvertXamlToHtml(string xamlString, HtmlFromXamlDocumentOptions options = null)
		{
			if(options == null)
			{
				options = new HtmlFromXamlDocumentOptions();
			}
			else
			{
				options.Reset();
			}

			using(var xamlReader = XmlReader.Create(new StringReader(xamlString), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = true }))
			{
				Preprocess(xamlReader, options);
			}

			using(var xamlReader = XmlReader.Create(new StringReader(xamlString), new XmlReaderSettings { DtdProcessing = DtdProcessing.Ignore, IgnoreWhitespace = true }))
			{
				var htmlStringBuilder = new StringBuilder(256);
                using (var sw = new StringWriter(htmlStringBuilder))
                {
                    using (var htmlWriter = XmlWriter.Create(sw, new XmlWriterSettings { OmitXmlDeclaration = true }))
                    {
                        if (!WriteFlowDocument(xamlReader, htmlWriter, options))
                        {
                            return string.Empty;
                        }
                    }
                }
                return htmlStringBuilder.ToString();
            }
        }

		// ---------------------------------------------------------------------
		//
		// Private Methods
		//
		// ---------------------------------------------------------------------

		#region Private Methods

		private static void Preprocess(XmlReader xamlReader, HtmlFromXamlDocumentOptions options)
		{
			if(!xamlReader.IsEmptyElement)
			{
				while(ReadNextToken(xamlReader) && xamlReader.NodeType != XmlNodeType.EndElement)
				{
					if(xamlReader.NodeType == XmlNodeType.Element)
					{
						if(xamlReader.Name == "Table")
						{
							options.AddTable();
						}
						else if(xamlReader.Name == "TableCell")
						{
							while(xamlReader.MoveToNextAttribute())
							{
								if(xamlReader.Name == "BorderThickness")
								{
									options.CurrentTable.AddBorder(GetThickness(xamlReader.Value));
								}
							}
							xamlReader.MoveToElement();
						}

						Preprocess(xamlReader, options);
					}
				}

				//				Debug.Assert(xamlReader.NodeType == XmlNodeType.EndElement);
			}
		}

		/// <summary>
		/// Processes a root level element of XAML (normally it's FlowDocument element).
		/// </summary>
		/// <param name="xamlReader">
		/// XTextReader for a source xaml.
		/// </param>
		/// <param name="htmlWriter">
		/// TextWriter producing resulting html
		/// </param>
		/// <param name="options">Options from preprocessor</param>
		private static bool WriteFlowDocument(XmlReader xamlReader, XmlWriter htmlWriter, HtmlFromXamlDocumentOptions options)
		{
			if(!ReadNextToken(xamlReader))
			{
				// Xaml content is empty - nothing to convert
				return false;
			}

			if(xamlReader.NodeType != XmlNodeType.Element || (xamlReader.Name != "FlowDocument" && xamlReader.Name != "Section"))
			{
				// Root FlowDocument elemet is missing
				return false;
			}

			// Create a buffer StringBuilder for collecting css properties for inline STYLE attributes
			// on every element level (it will be re-initialized on every level).
			var inlineStyle = new StringBuilder();

			if(options.OuterElement != string.Empty)
			{
				htmlWriter.WriteStartElement(options.OuterElement);
			}

            WriteElementWithContent(xamlReader, htmlWriter, options.InnerElement, inlineStyle, options);

			if(options.OuterElement != string.Empty)
			{
				htmlWriter.WriteEndElement();
			}

			return true;
		}

		/// <summary>
		/// Reads attributes of the current xaml element and converts
		/// them into appropriate html attributes or css styles.
		/// </summary>
		/// <param name="xamlReader">
		/// XTextReader which is expected to be at XmlNodeType.Element
		/// (opening element tag) position.
		/// The reader will remain at the same level after function complete.
		/// </param>
		/// <param name="htmlWriter">
		/// TextWriter for output html, which is expected to be in
		/// after WriteStartElement state.
		/// </param>
		/// <param name="inlineStyle">
		/// String builder for collecting css properties for inline STYLE attribute.
		/// </param>
		/// <param name="options">Element level options</param>
		private static void WriteFormattingProperties(XmlReader xamlReader, XmlWriter htmlWriter, StringBuilder inlineStyle, IList<string> subElements, HtmlFromXamlDocumentOptions options)
		{
			Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

			var elementName = xamlReader.LocalName;

			// Clear string builder for the inline style
			inlineStyle.Remove(0, inlineStyle.Length);

			bool borderSet = false;
			bool borderColorSet = false;

            string fontSizeStyle = null;
            var fontSizeIgnore = false;
			if(xamlReader.HasAttributes)
			{
				while(xamlReader.MoveToNextAttribute())
				{
					string css = null;

					switch(xamlReader.Name)
					{
						// Character fomatting properties
						// ------------------------------
						case "Background":
							css = "background-color:" + ParseXamlColor(xamlReader.Value) + ";";
							break;
						case "FontFamily":
							css = "font-family:" + xamlReader.Value + ";";
							break;
						case "FontStyle":
							css = "font-style:" + xamlReader.Value.ToLower() + ";";
							break;
						case "FontWeight":
							css = "font-weight:" + xamlReader.Value.ToLower() + ";";
							break;
						case "FontStretch":
							break;
						case "FontSize":
							double size;
							if(double.TryParse(xamlReader.Value, out size))
							{
                                fontSizeStyle = "font-size:" + Math.Round(size, 1, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture) + "px;";
							}
							else
							{
                                fontSizeStyle = "font-size:" + xamlReader.Value + "px;";
							}
							break;
						case "Foreground":
							css = "color:" + ParseXamlColor(xamlReader.Value) + ";";
							break;
						case "TextDecorations":
							css = "text-decoration:underline;";
							break;
						case "TextEffects":
							break;
						case "Emphasis":
							break;
						case "StandardLigatures":
							break;
						case "Variants":
							break;
						case "Capitals":
							break;
						case "Fraction":
							break;
						case "BaselineAlignment":
							if(xamlReader.Value == "Subscript")
							{
                                subElements.Add("sub");
                                fontSizeIgnore = true;
                            }
							else if(xamlReader.Value == "Superscript")
							{
                                subElements.Add("sup");
                                fontSizeIgnore = true;
                            }
                            break;

						// Paragraph formatting properties
						// -------------------------------
						case "Padding":
							css = "padding:" + ParseXamlThickness(xamlReader.Value) + ";";
							break;
						case "Margin":
							css = "margin:" + ParseXamlThickness(xamlReader.Value) + ";";
							break;
						case "BorderThickness":
							var t = GetThickness(xamlReader.Value);
							var bw = PrintThickness(t);
							css = $"border-width:{bw};";
							borderSet = true;
							break;
						case "BorderBrush":
							css = "border-color:" + ParseXamlColor(xamlReader.Value) + ";";
							borderColorSet = true;
							break;
						case "LineHeight":
							break;
						case "TextIndent":
							css = "text-indent:" + xamlReader.Value + ";";
							break;
						case "TextAlignment":
							css = "text-align:" + xamlReader.Value + ";";
							break;
						case "IsKeptTogether":
							break;
						case "IsKeptWithNext":
							break;
						case "ColumnBreakBefore":
							break;
						case "PageBreakBefore":
							break;
						case "FlowDirection":
							break;

						// Table attributes
						// ----------------
						case "Width":
							css = "width:" + ParseXamlSize(xamlReader.Value) + ";";
							break;
						case "ColumnSpan":
							htmlWriter.WriteAttributeString("COLSPAN", xamlReader.Value);
							break;
						case "RowSpan":
							htmlWriter.WriteAttributeString("ROWSPAN", xamlReader.Value);
							break;
                        default:
                            options.OnWriteCustomProperty?.Invoke(xamlReader, htmlWriter, inlineStyle, options);
                            break;
					}

					if(css != null)
					{
						inlineStyle.Append(css);
					}
				}
                if(!fontSizeIgnore && fontSizeStyle != null)
                {
                    inlineStyle.Append(fontSizeStyle);
                }
			}

			if(elementName == "Table")
			{
				inlineStyle.Append("border-collapse:collapse;");
				if(!borderSet && options.CurrentTable != null)
				{
					var t = options.CurrentTable.CommonBorder;
					var thickness = PrintThickness(new HtmlThickness(0, t.Top, t.Right, 0));
					inlineStyle.Append($"border-width:{thickness};");
					borderSet = true;
				}
			}
			if(borderSet || borderColorSet)
			{
				inlineStyle.Append("border-style:solid;");
			}

			// Return the xamlReader back to element level
			xamlReader.MoveToElement();
			Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);
		}

		private static string ParseXamlSize(string xamlSize)
		{
			double size;
			if(double.TryParse(xamlSize, out size))
			{
				return xamlSize + "px";
			}
			else
			{
				return xamlSize;
			}
		}

		private static string ParseXamlColor(string color)
		{
			if(color.StartsWith("#"))
			{
				// Remove transparancy value
				color = "#" + color.Substring(3);
			}
			return color;
		}

		private static HtmlThickness GetThickness(string thickness)
		{
			var textValues = thickness.Split(',');
			var values = new double[textValues.Length];

			for(var i = 0; i < textValues.Length; i++)
			{
				double value;
				if(double.TryParse(textValues[i], out value))
				{
					values[i] = Math.Ceiling(value);
				}
				else
				{
					values[i] = 1;
				}
			}

			switch(values.Length)
			{
				case 1:
					return new HtmlThickness(values[0]);
				case 2:
					return new HtmlThickness(values[0], values[1], values[0], values[1]);
				case 4:
					return new HtmlThickness(values[0], values[1], values[2], values[3]);
				default:
					return new HtmlThickness(1);
			}
		}

		private static string ParseXamlThickness(string thickness)
		{
			return PrintThickness(GetThickness(thickness));
		}

		private static string PrintThickness(HtmlThickness t)
		{   
			return string.Format("{0:0.#}px {1:0.#}px {2:0.#}px {3:0.#}px", t.Top, t.Right, t.Bottom, t.Left);
		}

		/// <summary>
		/// Reads a content of current xaml element, converts it
		/// </summary>
		/// <param name="xamlReader">
		/// XTextReader which is expected to be at XmlNodeType.Element
		/// (opening element tag) position.
		/// </param>
		/// <param name="htmlWriter">
		/// May be null, in which case we are skipping the xaml element;
		/// witout producing any output to html.
		/// </param>
		/// <param name="inlineStyle">
		/// StringBuilder used for collecting css properties for inline STYLE attribute.
		/// </param>
		/// <param name="options">Element level option</param>
		private static void WriteElementContent(XmlReader xamlReader, XmlWriter htmlWriter, StringBuilder inlineStyle, HtmlFromXamlDocumentOptions options)
		{
			Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

			bool elementContentStarted = false;

			if(xamlReader.IsEmptyElement)
			{
				if(htmlWriter != null && !elementContentStarted && inlineStyle.Length > 0)
				{
					// Output STYLE attribute and clear inlineStyle buffer.
						htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
					inlineStyle.Remove(0, inlineStyle.Length);
				}
				elementContentStarted = true;
			}
			else
			{
				while(ReadNextToken(xamlReader) && xamlReader.NodeType != XmlNodeType.EndElement)
				{
					switch(xamlReader.NodeType)
					{
						case XmlNodeType.Element:
							if(!HandleComplexProperty(xamlReader, htmlWriter, inlineStyle, options))
							{
								if(htmlWriter != null && !elementContentStarted && inlineStyle.Length > 0)
								{
									// Output STYLE attribute and clear inlineStyle buffer.
									htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
									inlineStyle.Remove(0, inlineStyle.Length);
								}
								elementContentStarted = true;
								WriteElement(xamlReader, htmlWriter, inlineStyle, options);
							}
							Debug.Assert(xamlReader.NodeType == XmlNodeType.EndElement || xamlReader.NodeType == XmlNodeType.Element && xamlReader.IsEmptyElement);
							break;
						case XmlNodeType.Comment:
							if(htmlWriter != null)
							{
								if(!elementContentStarted && inlineStyle.Length > 0)
								{
									htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
								}
								htmlWriter.WriteComment(xamlReader.Value);
							}
							elementContentStarted = true;
							break;
						case XmlNodeType.CDATA:
						case XmlNodeType.Text:
						case XmlNodeType.SignificantWhitespace:
							if(htmlWriter != null)
							{
								if(!elementContentStarted && inlineStyle.Length > 0)
								{
									htmlWriter.WriteAttributeString("style", inlineStyle.ToString());
								}
								htmlWriter.WriteString(xamlReader.Value);
							}
							elementContentStarted = true;
							break;
					}
				}

				Debug.Assert(xamlReader.NodeType == XmlNodeType.EndElement);
			}
		}

		/// <summary>
		/// Conberts an element notation of complex property into
		/// </summary>
		/// <param name="xamlReader">
		/// On entry this XTextReader must be on Element start tag;
		/// on exit - on EndElement tag.
		/// </param>
		/// <param name="htmlWriter">
		/// May be null, in which case we are skipping xaml content
		/// without producing any html output
		/// </param>
		/// <param name="inlineStyle">
		/// StringBuilder containing a value for STYLE attribute.
		/// </param>
		/// <param name="options">Element level options</param>
		private static bool HandleComplexProperty(XmlReader xamlReader, XmlWriter htmlWriter, StringBuilder inlineStyle, HtmlFromXamlDocumentOptions options)
		{
			Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

			if(!xamlReader.Name.Contains(".") || xamlReader.Name == "Table.Columns")
			{
				return false;
			}

			if(xamlReader.Name.EndsWith(".TextDecorations"))
			{
				var l = 0;
				while(ReadNextToken(xamlReader))
				{
					if(xamlReader.NodeType == XmlNodeType.Element)
					{
						l++;
						if(xamlReader.Name == "TextDecoration")
						{
							if(xamlReader.HasAttributes && xamlReader.MoveToAttribute("Location"))
							{
								if(xamlReader.Value == "Strikethrough")
								{
									(inlineStyle ?? (inlineStyle = new StringBuilder())).Append("text-decoration:line-through;");
								}
								else if(xamlReader.Value == "Underline")
								{
									(inlineStyle ?? (inlineStyle = new StringBuilder())).Append("text-decoration:underline;");
								}
							}
						}
					}
					else if(xamlReader.NodeType == XmlNodeType.EndElement)
					{
						l--;
					}
					if(l <= 0)
					{
						break;
					}
				}
			}
			else
			{
				// Skip the element representing the complex property
				WriteElementContent(xamlReader, /*htmlWriter:*/null, /*inlineStyle:*/null, options);
			}
			return true;
		}

		/// <summary>
		/// Converts a xaml element into an appropriate html element.
		/// </summary>
		/// <param name="xamlReader">
		/// On entry this XTextReader must be on Element start tag;
		/// on exit - on EndElement tag.
		/// </param>
		/// <param name="htmlWriter">
		/// May be null, in which case we are skipping xaml content
		/// without producing any html output
		/// </param>
		/// <param name="inlineStyle">
		/// StringBuilder used for collecting css properties for inline STYLE attributes on every level.
		/// </param>
		/// <param name="options">Options from preprocessor</param>
		private static void WriteElement(XmlReader xamlReader, XmlWriter htmlWriter, StringBuilder inlineStyle, HtmlFromXamlDocumentOptions options)
		{
			Debug.Assert(xamlReader.NodeType == XmlNodeType.Element);

			if(htmlWriter == null)
			{
				// Skipping mode; recurse into the xaml element without any output
				WriteElementContent(xamlReader, /*htmlWriter:*/null, null, options);
			}
			else
			{
				string htmlElementName = null;

				switch(xamlReader.LocalName)
				{
					case "Run":
					case "Span":
						htmlElementName = "span";
						break;
					case "InlineUIContainer":
						htmlElementName = "span";
						break;
					case "Bold":
						htmlElementName = "b";
						break;
					case "Italic":
						htmlElementName = "i";
						break;
					case "Paragraph":
						htmlElementName = "p";
						break;
					case "BlockUIContainer":
						htmlElementName = "div";
						break;
					case "Section":
						htmlElementName = "div";
						break;
					case "Table":
						htmlElementName = "table";
						options.TableMove();
						break;
					case "Table.Columns":
						htmlElementName = "colgroup";
						break;
					case "TableColumn":
						htmlElementName = "col";
						break;
					case "TableRowGroup":
						htmlElementName = "tbody";
						break;
					case "TableRow":
						htmlElementName = "tr";
						break;
					case "TableCell":
						htmlElementName = "td";
						break;
					case "List":
						string marker = xamlReader.GetAttribute("MarkerStyle");
						if(marker == null || marker == "None" || marker == "Disc" || marker == "Circle" || marker == "Square" || marker == "Box")
						{
							htmlElementName = "ul";
						}
						else
						{
							htmlElementName = "ol";
						}
						break;
					case "ListItem":
						htmlElementName = "li";
						break;
					default:
                        if (options.OnGetHtmlElementName != null)
                        {
                            // Custom handling of the element
                            htmlElementName = options.OnGetHtmlElementName(xamlReader.LocalName);
                        }
                        else
                        {
                            htmlElementName = null;
                        }
						break;
				}

				if(htmlWriter != null && htmlElementName != null)
                {
                    WriteElementWithContent(xamlReader, htmlWriter, htmlElementName, inlineStyle, options);
                }
                else
				{
					// Skip this unrecognized xaml element
					WriteElementContent(xamlReader, /*htmlWriter:*/null, null, options);
				}
			}
		}

        private static void WriteElementWithContent(XmlReader xamlReader, XmlWriter htmlWriter, string htmlElementName, StringBuilder inlineStyle, HtmlFromXamlDocumentOptions options)
        {
            htmlWriter.WriteStartElement(htmlElementName);

            var subElements = new List<string>();

            WriteFormattingProperties(xamlReader, htmlWriter, inlineStyle, subElements, options);

            foreach (var element in subElements)
            {
                htmlWriter.WriteStartElement(element);
            }

            WriteElementContent(xamlReader, htmlWriter, inlineStyle, options);

            foreach (var element in subElements)
            {
                htmlWriter.WriteEndElement();
            }

            htmlWriter.WriteEndElement();
        }

        // Reader advance helpers
        // ----------------------

        /// <summary>
        /// Reads several items from xamlReader skipping all non-significant stuff.
        /// </summary>
        /// <param name="xamlReader">
        /// XTextReader from tokens are being read.
        /// </param>
        /// <returns>
        /// True if new token is available; false if end of stream reached.
        /// </returns>
        public static bool ReadNextToken(XmlReader xamlReader)
		{
			while(xamlReader.Read())
			{
				Debug.Assert(xamlReader.ReadState == ReadState.Interactive, "Reader is expected to be in Interactive state (" + xamlReader.ReadState + ")");
				switch(xamlReader.NodeType)
				{
					case XmlNodeType.Element:
					case XmlNodeType.EndElement:
					case XmlNodeType.None:
					case XmlNodeType.CDATA:
					case XmlNodeType.Text:
					case XmlNodeType.SignificantWhitespace:
						return true;

					case XmlNodeType.Whitespace:
						if(xamlReader.XmlSpace == XmlSpace.Preserve)
						{
							return true;
						}
						// ignore insignificant whitespace
						break;

					case XmlNodeType.EndEntity:
					case XmlNodeType.EntityReference:
						//  Implement entity reading
						//xamlReader.ResolveEntity();
						//xamlReader.Read();
						//ReadChildNodes( parent, parentBaseUri, xamlReader, positionInfo);
						break; // for now we ignore entities as insignificant stuff

					case XmlNodeType.Comment:
						return true;
					case XmlNodeType.ProcessingInstruction:
					case XmlNodeType.DocumentType:
					case XmlNodeType.XmlDeclaration:
					default:
						// Ignorable stuff
						break;
				}
			}
			return false;
		}

		#endregion Private Methods

		// ---------------------------------------------------------------------
		//
		// Private Fields
		//
		// ---------------------------------------------------------------------

		#region Private Fields

		#endregion Private Fields
	}
}
