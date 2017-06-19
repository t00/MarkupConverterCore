using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace MarkupConverter
{
    public class HtmlToXamlContext
    {
        public HtmlToXamlDocumentOptions Options { get; }

        public Action<HtmlXamlImage, XElement, HtmlToXamlContext> OnProcessImage { get; set; }

        public HtmlToXamlContext(HtmlToXamlDocumentOptions options)
        {
            Options = options;
        }

        public CssStylesheet Stylesheet { get; internal set; }

        public IList<XElement> SourceContext { get; internal set; }

        public IList<XElement> DestinationContext { get; internal set; }
    }
}
