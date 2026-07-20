using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

                // Preserve paragraph properties (style, spacing, alignment)
                ParagraphProperties? preservedProperties = null;
                if (paragraph.ParagraphProperties != null)
                {
                    preservedProperties =
                        (ParagraphProperties)paragraph.ParagraphProperties.CloneNode(true);
                }

                var preservedRuns = new List<Run>();
                foreach (var run in paragraph.Elements<Run>().ToList())
                {
                    var runText = run.InnerText;
                    if (string.IsNullOrWhiteSpace(runText))
                    {
                        preservedRuns.Add((Run)run.CloneNode(true));
                        continue;
                    }

                    var convertedRunText = converter(runText);

                    var newRun = new Run();
                    if (run.RunProperties != null)
                    {
                        newRun.AppendChild((RunProperties)run.RunProperties.CloneNode(true));
                    }

                    newRun.Append(new Text(convertedRunText)
                    {
                        Space = SpaceProcessingModeValues.Preserve
                    });

                    preservedRuns.Add(newRun);
                }

                paragraph.RemoveAllChildren<Run>();
                paragraph.ParagraphProperties = preservedProperties;

                foreach (var preservedRun in preservedRuns)
                {
                    paragraph.Append(preservedRun);
                }
            }

            ApplyDefaultFontToDocument(body);

            mainPart.Document.Save();
            stopwatch.Stop();

            return stopwatch.ElapsedMilliseconds;
        }

        private static void ApplyDefaultFontToDocument(Body body)
        {
            foreach (var run in body.Descendants<Run>())
            {
                var runProperties = run.RunProperties ?? new RunProperties();
                var runFonts = runProperties.GetFirstChild<RunFonts>();

                if (runFonts == null)
                {
                    runFonts = new RunFonts
                    {
                        Ascii = "NudiParijatha",
                        Hint = FontTypeHintValues.Default
                    };
                    runProperties.AppendChild(runFonts);
                }
                else
                {
                    runFonts.Ascii = "NudiParijatha";
                    runFonts.Hint = FontTypeHintValues.Default;
                }

                if (run.RunProperties == null)
                {
                    run.AppendChild(runProperties);
                }
            }
        }
    }
}
