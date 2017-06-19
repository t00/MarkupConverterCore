using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace MarkupConverter
{
    public class HtmlFromXamlContext
    {
        public delegate void HtmlWriteDelegate(XmlReader xamlReader, XmlWriter htmlWriter, StringBuilder inlineStyle, HtmlFromXamlContext context, string value);

        public HtmlFromXamlDocumentOptions Options { get; }

        public HtmlWriteDelegate OnWriteCustomProperty;

        public HtmlWriteDelegate OnWriteText { get; set; }

        public Func<string, string> OnGetHtmlElementName { get; set; }

        public HtmlFromXamlContext(HtmlFromXamlDocumentOptions options)
        {
            Options = options;
        }

        public IReadOnlyList<HtmlFromXamlTableInfo> Tables => (tables ?? (tables = new List<HtmlFromXamlTableInfo>())).AsReadOnly();

        public HtmlFromXamlTableInfo CurrentTable => (tableIndex >= 0 && tableIndex < Tables.Count) ? Tables[tableIndex] : null;

        internal HtmlFromXamlTableInfo AddTable()
        {
            var to = new HtmlFromXamlTableInfo();
            tables.Add(to);
            tableIndex = Tables.Count - 1;
            return to;
        }

        internal HtmlFromXamlTableInfo TableMove()
        {
            tableIndex++;
            return CurrentTable;
        }

        private List<HtmlFromXamlTableInfo> tables;
        private int tableIndex;
    }
}
