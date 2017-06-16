using System.Collections.Generic;
using System.Xml.Linq;

namespace MarkupConverter
{
    internal class HtmlToXamlContext
    {
        public HtmlToXamlDocumentOptions Options { get; }

        public HtmlToXamlContext(HtmlToXamlDocumentOptions options)
        {
            Options = options;
        }

        public CssStylesheet Stylesheet { get; set; }

        public IList<XElement> SourceContext { get; set; }

        public IList<XElement> DestinationContext { get; set; }
    }
}
