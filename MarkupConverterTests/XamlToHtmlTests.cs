using Microsoft.VisualStudio.TestTools.UnitTesting;
using MarkupConverter;

namespace MarkupConverterTests
{
    [TestClass]
    public class XamlToHtmlTests
    {
        [TestMethod]
        public void TestParagraphRun()
        {
            var xaml = Documents.TestParagraph_Xaml;
            var html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, new HtmlFromXamlContext(new HtmlFromXamlDocumentOptions()));
            Assert.AreEqual(string.Format(Documents.FullHtml_Format, Documents.TestParagraph_Html), html);
        }

        [TestMethod]
        public void TestParagraphRunSpan()
        {
            var xaml = Documents.TestParagraph_Xaml;
            var html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, new HtmlFromXamlContext(new HtmlFromXamlDocumentOptions { OuterElement = "", InnerElement = "span" }));
            Assert.AreEqual($"<span>{Documents.TestParagraph_Html}</span>", html);
        }

        [TestMethod]
        public void TestMultipleStyles()
        {
            var xaml = Documents.MultipleFontStyles_Xaml;
            var html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, new HtmlFromXamlContext(new HtmlFromXamlDocumentOptions()));
            Assert.AreEqual(string.Format(Documents.FullHtml_Format, Documents.MultipleFontStyles_HtmlFromXaml), html);
        }
    }
}
