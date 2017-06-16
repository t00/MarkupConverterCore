using System.Collections.Generic;

namespace MarkupConverter
{
    internal class HtmlFromXamlContext
    {
        public HtmlFromXamlDocumentOptions Options { get; }

        public HtmlFromXamlContext(HtmlFromXamlDocumentOptions options)
        {
            Options = options;
        }

        public IList<HtmlFromXamlTableInfo> Tables => tables ?? (tables = new List<HtmlFromXamlTableInfo>());

        public HtmlFromXamlTableInfo AddTable()
        {
            var to = new HtmlFromXamlTableInfo();
            Tables.Add(to);
            tableIndex = Tables.Count - 1;
            return to;
        }

        public HtmlFromXamlTableInfo CurrentTable => (tableIndex >= 0 && tableIndex < Tables.Count) ? Tables[tableIndex] : null;

        public HtmlFromXamlTableInfo TableMove()
        {
            tableIndex++;
            return CurrentTable;
        }

        private IList<HtmlFromXamlTableInfo> tables;
        private int tableIndex;

    }
}
