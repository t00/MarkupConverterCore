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
            var xaml = HtmlToXamlConverter.ConvertHtmlToXaml(html, true);
            Assert.AreEqual(Documents.TestParagraph_XamlFromHtml, xaml);
        }
    }
}
