using Microsoft.VisualStudio.TestTools.UnitTesting;
using MarkupConverter;
using System;
using System.Text;
using System.Xml;
using System.Collections.Generic;

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

        [TestMethod]
        public void TestMultipleStylesToPlain()
        {
            var xaml = Documents.MultipleFontStyles_Xaml;
            var context = new HtmlFromXamlContext(new HtmlFromXamlDocumentOptions());
            texts.Clear();
            context.OnWriteText = ReadText;
            var html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, context);
            var expectedList = new List<string> { "word1 ", "word2" };
            CollectionAssert.AreEqual(expectedList, texts);
        }

        [TestMethod]
        public void TestTable()
        {
            var xaml = Documents.Table_Xaml;
            var html = HtmlFromXamlConverter.ConvertXamlToHtml(xaml, new HtmlFromXamlContext(new HtmlFromXamlDocumentOptions()));
            Assert.AreEqual(Documents.Table_HtmlFromXaml, html);
        }

        private void ReadText(XmlReader xamlReader, XmlWriter htmlWriter, StringBuilder inlineStyle, HtmlFromXamlContext context, ref string value)
        {
            texts.Add(value);
        }

        private List<string> texts = new List<string>();
    }
}
