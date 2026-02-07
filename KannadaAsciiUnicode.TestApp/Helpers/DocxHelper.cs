using System;
using System.IO;
using System.Diagnostics;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace KannadaAsciiUnicode.TestApp.Helpers
{
    public static class DocxHelper
    {
        /// <summary>
        /// Converts a DOCX file by copying it first, then replacing paragraph text
        /// using the supplied converter while preserving paragraph formatting.
        /// </summary>
        /// <param name="inputPath">Source DOCX</param>
        /// <param name="outputPath">Converted DOCX</param>
        /// <param name="converter">Text converter function</param>
        /// <returns>Elapsed time in milliseconds</returns>
        public static long ConvertDocx(
            string inputPath,
            string outputPath,
            Func<string, string> converter)
        {
            if (!File.Exists(inputPath))
                throw new FileNotFoundException("Input DOCX not found", inputPath);

            // Copy original → output (keeps all styles intact)
            File.Copy(inputPath, outputPath, overwrite: true);

            var stopwatch = Stopwatch.StartNew();

            using var document = WordprocessingDocument.Open(outputPath, true);

            var mainPart = document.MainDocumentPart;
            if (mainPart?.Document?.Body == null)
                return 0;

            var body = mainPart.Document.Body;

            foreach (var paragraph in body.Elements<Paragraph>())
            {
                var originalText = paragraph.InnerText;

                if (string.IsNullOrWhiteSpace(originalText))
                    continue;

                var convertedText = converter(originalText);

                // Preserve paragraph properties (style, spacing, alignment)
                ParagraphProperties? preservedProperties = null;
                if (paragraph.ParagraphProperties != null)
                {
                    preservedProperties =
                        (ParagraphProperties)paragraph.ParagraphProperties.CloneNode(true);
                }

                // Remove old runs
                paragraph.RemoveAllChildren<Run>();

                // Restore properties
                paragraph.ParagraphProperties = preservedProperties;

                // Insert converted text
                paragraph.Append(
                    new Run(
                        new Text(convertedText)
                        {
                            Space = SpaceProcessingModeValues.Preserve
                        }));
            }

            mainPart.Document.Save();
            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }
    }
}
