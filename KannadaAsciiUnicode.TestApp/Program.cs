using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Kannada.AsciiUnicode.Converters;
using KannadaAsciiUnicode.TestApp.Helpers;

class Program
{
    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        // -------------------------------
        // Custom mappings (optional)
        // -------------------------------
        var customAsciiToUnicode = new Dictionary<string, string>
        {
            { "wÃPÀëÚ", "ತೀಕ್ಷ್ಣ" },
            { "PÀëÚ", "ಕ್ಷ್ಣ" },
            { "UÉÀ", "ಗೆ" }
        };

        var customUnicodeToAscii = new Dictionary<string, string>
        {
            { "ತೀಕ್ಷ್ಣ", "wÃPÀëÚ" },
            { "ಕ್ಷ್ಣ", "PÀëÚ" }
        };

        // -------------------------------
        // Initialize converter
        // -------------------------------
        var converter = KannadaConverter.CreateWithCustomMapping(
            customAsciiToUnicode,
            customUnicodeToAscii
        );

        // -------------------------------
        // ASCII text conversion (string test)
        // -------------------------------
        string asciiText =
            "MmÁÖgÉAiÀÄ zÀÈ¶Ö¬ÄAzÀ F PÉÆÃ±ÀªÀÅ UÀzÀÄV£À ¨sÁgÀvÀzÀ CzsÀåAiÀÄ£ÀPÁgÀjUÉ ¸ÀºÁAiÀÄPÀªÁzÀgÉ...";

        var swA2U = Stopwatch.StartNew();
        string unicodeText = converter.ConvertAsciiToUnicode(asciiText);
        swA2U.Stop();

        var swU2A = Stopwatch.StartNew();
        string asciiRoundTrip = converter.ConvertUnicodeToAscii(unicodeText);
        swU2A.Stop();

        // -------------------------------
        // Save TXT results
        // -------------------------------
        Directory.CreateDirectory("output");
        var txtOutput = Path.Combine("output", "conversion_results.txt");

        File.WriteAllText(txtOutput, $"""
        === Kannada ASCII ↔ Unicode Conversion ===
        Time: {DateTime.Now}

        Original ASCII:
        {asciiText}

        Unicode ({FormatTime(swA2U.Elapsed)}):
        {unicodeText}

        Round-trip ASCII ({FormatTime(swU2A.Elapsed)}):
        {asciiRoundTrip}
        """, Encoding.UTF8);

        Console.WriteLine("✓ Text conversion complete");
        Console.WriteLine($"ASCII → Unicode Time: {FormatTime(swA2U.Elapsed)}");
        Console.WriteLine($"Unicode → ASCII Time: {FormatTime(swU2A.Elapsed)}");

        // ==========================================================
        // DOCX CONVERSION
        // ==========================================================
        var testDataDir = Path.Combine("TestData", "Docx");
        var outputDir = "output";

        Directory.CreateDirectory(testDataDir);
        Directory.CreateDirectory(outputDir);

        var asciiInputDocx = Path.Combine(testDataDir, "ascii_input.docx");
        var unicodeInputDocx = Path.Combine(testDataDir, "unicode_input.docx");

        // ASCII → Unicode DOCX
        var asciiToUnicodeDocx = Path.Combine(outputDir, "ascii_to_unicode.docx");
        long asciiDocxMs = DocxHelper.ConvertDocx(
            asciiInputDocx,
            asciiToUnicodeDocx,
            converter.ConvertAsciiToUnicode);

        // Unicode → ASCII DOCX
        var unicodeToAsciiDocx = Path.Combine(outputDir, "unicode_to_ascii.docx");
        long unicodeDocxMs = DocxHelper.ConvertDocx(
            unicodeInputDocx,
            unicodeToAsciiDocx,
            converter.ConvertUnicodeToAscii);

        var asciiDocxTime = TimeSpan.FromMilliseconds(asciiDocxMs);
        var unicodeDocxTime = TimeSpan.FromMilliseconds(unicodeDocxMs);

        // Append DOCX timing info
        File.AppendAllText(
            txtOutput,
            Environment.NewLine +
            "=== DOCX Conversion ===" + Environment.NewLine +
            $"ASCII → Unicode DOCX Time: {FormatTime(asciiDocxTime)}" + Environment.NewLine +
            $"Unicode → ASCII DOCX Time: {FormatTime(unicodeDocxTime)}" + Environment.NewLine +
            $"Total DOCX Conversion Time: {FormatTime(asciiDocxTime + unicodeDocxTime)}" +
            Environment.NewLine,
            Encoding.UTF8);

        Console.WriteLine("✓ DOCX conversion complete");
        Console.WriteLine($"ASCII → Unicode DOCX: {FormatTime(asciiDocxTime)}");
        Console.WriteLine($"Unicode → ASCII DOCX: {FormatTime(unicodeDocxTime)}");
    }

    // -------------------------------
    // Time formatting helper
    // -------------------------------
    private static string FormatTime(TimeSpan time) =>
        $"{time.Minutes:D2} min {time.Seconds:D2} sec {time.Milliseconds:D3} ms";
}
