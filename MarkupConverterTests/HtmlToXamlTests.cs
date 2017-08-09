using MarkupConverter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MarkupConverterTests
{
    [TestClass]
    public class HtmlToXamlTests
    {
        [TestMethod]
        public void TestParagraphRun()
        {
            var html = Documents.TestParagraph_Html;
            var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, new HtmlToXamlContext(new HtmlToXamlDocumentOptions()));
            Assert.AreEqual(Documents.TestParagraph_XamlFromHtml, xaml);
        }

        [TestMethod]
        public void TestTable()
        {
            var html = Documents.Table_Html;
            var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, new HtmlToXamlContext(new HtmlToXamlDocumentOptions()));
            Assert.AreEqual(Documents.Table_XamlFromHtml, xaml);
        }
    }
}
