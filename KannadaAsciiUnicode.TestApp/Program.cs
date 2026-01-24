using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using Kannada.AsciiUnicode.Converters;

class Program
{
    static void Main()
    {
        // Ensure console can display Kannada properly
        Console.OutputEncoding = Encoding.UTF8;

        // -------------------------------
        // Step 1: Optional custom mappings
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
        // Step 2: Initialize converter
        // -------------------------------
        var converter = KannadaConverter.CreateWithCustomMapping(
            userAsciiToUnicodeMapping: customAsciiToUnicode,
            userUnicodeToAsciiMapping: customUnicodeToAscii
        );

        // -------------------------------
        // Step 3: ASCII text to convert
        // -------------------------------
        string asciiText = "MmÁÖgÉAiÀÄ zÀÈ¶Ö¬ÄAzÀ F PÉÆÃ±ÀªÀÅ UÀzÀÄV£À ¨sÁgÀvÀzÀ CzsÀåAiÀÄ£ÀPÁgÀjUÉ ¸ÀºÁAiÀÄPÀªÁzÀgÉ, CxÀªÁ PÀÄªÀiÁgÀªÁå¸À£À£ÀÄß CxÀðªÀiÁrPÉÆ¼ÀÄîªÀ°è CªÀ£À ªÀÄÆ® GzÉÝÃ±ÀzÀ ¸À«ÄÃ¥ÀPÉÌ CªÀgÀ£ÀÄß PÉÆAqÉÆAiÀÄÝgÉ £À£Àß ±ÀæªÀÄ ¸ÁxÀðPÀ. PÀÄªÀiÁgÀªÁå¸À ¨sÁgÀvÀPÉÌ ¥ÀzÀ¥ÀæAiÉÆÃUÀPÉÆÃ±ÀªÀ£ÀÄß gÀa¸ÀÄªÀ £À£Àß F ¥ÀæAiÀÄvÀß KPÀªÀåQÛAiÀÄ ¥ÀæAiÀÄvÀßªÁVzÀÄÝ, ¸ÀºÀdªÁVAiÉÄÃ £À£Àß §Ä¢ÞAiÀÄ ¹Ã«ÄvÀvÉAiÀÄ CxÀªÁ C£ÀªÀzsÁ£ÀzÀ PÀÁgÀt¢AzÀ EzÀgÀ°è PÉ®ªÀÅ PÀqÉ vÀ¥ÀÄàUÀ¼ÁVgÀ§ºÀÄzÀÄ.  F PÉÆÃ±ÀªÀ£ÀÄß §¼À¸ÀÄªÀªÀgÀÄ EzÀgÀ°è EgÀ§ºÀÄzÁzÀ vÀ¥ÀÄàUÀ¼À£ÀÄß £À£Àß UÀªÀÄ£ÀPÉÌ vÀAzÀgÉ (vkrishna1411@yahoo.co.in, mob. 90368 57528), CªÀÅUÀ¼À£ÀÄß ªÀÄÄA¢£À DªÀÈwÛAiÀÄ°è w¢ÝPÉÆ¼ÀÄîvÉÛÃ£É.  ";
        // -------------------------------
        // Step 4: Convert ASCII → Unicode with timing
        // -------------------------------
        var swA2U = Stopwatch.StartNew();
        string unicodeText = converter.ConvertAsciiToUnicode(asciiText);
        swA2U.Stop();
        long asciiToUnicodeMs = swA2U.ElapsedMilliseconds;

        // -------------------------------
        // Step 5: Convert Unicode → ASCII with timing
        // -------------------------------
        var swU2A = Stopwatch.StartNew();
        string asciiRoundTrip = converter.ConvertUnicodeToAscii(unicodeText);
        swU2A.Stop();
        long unicodeToAsciiMs = swU2A.ElapsedMilliseconds;

        // -------------------------------
        // Step 6: Save results to file
        // -------------------------------
        Directory.CreateDirectory("output");
        string outputFile = Path.Combine("output", "conversion_results.txt");

        var sb = new StringBuilder();
        sb.AppendLine("=== Kannada ASCII ↔ Unicode Conversion ===");
        sb.AppendLine($"Date/Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine("Original ASCII Text:");
        sb.AppendLine(asciiText);
        sb.AppendLine();
        sb.AppendLine($"Converted Unicode Text (Time: {asciiToUnicodeMs}ms):");
        sb.AppendLine(unicodeText);
        sb.AppendLine();
        sb.AppendLine($"Round-Trip ASCII Text (Time: {unicodeToAsciiMs}ms):");
        sb.AppendLine(asciiRoundTrip);
        sb.AppendLine();
        sb.AppendLine($"Total Conversion Time: {asciiToUnicodeMs + unicodeToAsciiMs}ms");

        File.WriteAllText(outputFile, sb.ToString(), Encoding.UTF8);

        // -------------------------------
        // Step 7: Print results in console
        // -------------------------------
        Console.WriteLine("✓ Conversion complete!");
        Console.WriteLine($"Original ASCII: {asciiText}");
        Console.WriteLine($"Converted Unicode: {unicodeText} (Time: {asciiToUnicodeMs}ms)");
        Console.WriteLine($"Round-Trip ASCII: {asciiRoundTrip} (Time: {unicodeToAsciiMs}ms)");
        Console.WriteLine($"Total Conversion Time: {asciiToUnicodeMs + unicodeToAsciiMs}ms");
        Console.WriteLine($"Results saved to: {outputFile}");
    }
}
