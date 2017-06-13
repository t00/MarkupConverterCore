using System;
using System.Collections.Generic;
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

        public virtual void Reset()
        {
            tableIndex = -1;
            tables?.Clear();
        }

        internal IList<HtmlFromXamlTableInfo> Tables => tables ?? (tables = new List<HtmlFromXamlTableInfo>());

        internal HtmlFromXamlTableInfo AddTable()
        {
            var to = new HtmlFromXamlTableInfo();
            Tables.Add(to);
            tableIndex = Tables.Count - 1;
            return to;
        }

        internal HtmlFromXamlTableInfo CurrentTable => (tableIndex >= 0 && tableIndex < Tables.Count) ? Tables[tableIndex] : null;

        internal HtmlFromXamlTableInfo TableMove()
        {
            tableIndex++;
            return CurrentTable;
        }

        private IList<HtmlFromXamlTableInfo> tables;
        private int tableIndex;
    }
}