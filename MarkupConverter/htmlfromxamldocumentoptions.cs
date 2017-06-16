using System;
using System.Text;
using System.Xml;

namespace MarkupConverter
{
    public class HtmlFromXamlDocumentOptions
    {
        public string InnerElement { get; set; }

        public string OuterElement { get; set; }

        public Func<string, string> OnGetHtmlElementName { get; set; }

        public Action<XmlReader, XmlWriter, StringBuilder, HtmlFromXamlDocumentOptions> OnWriteCustomProperty { get; set; }

        public HtmlFromXamlDocumentOptions()
        {
            OuterElement = "html";
            InnerElement = "body";
        }
    }
}