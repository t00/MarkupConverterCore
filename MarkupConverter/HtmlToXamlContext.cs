using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MarkupConverter
{
    public class HtmlToXamlContext
    {
        public HtmlToXamlDocumentOptions Options { get; }

        public Func<HtmlXamlImage, XElement, HtmlToXamlContext, bool> OnProcessImage { get; set; }

        public Action<XElement, HtmlToXamlContext> OnElementAdded { get; set; }

        public Action<XElement, string, HtmlToXamlContext> OnWriteText { get; set; }

        public HtmlToXamlContext(HtmlToXamlDocumentOptions options)
        {
            Options = options;
        }

        public CssStylesheet Stylesheet { get; internal set; }

        public IList<XElement> SourceContext { get; internal set; }

        public IList<XElement> DestinationContext { get; internal set; }
    }
}
