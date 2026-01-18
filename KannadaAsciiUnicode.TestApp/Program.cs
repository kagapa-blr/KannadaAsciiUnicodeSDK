using System;
using System.IO;
using System.Text;
using Kannada.AsciiUnicode.Converters;

class Program
{
    static void Main()
    {
        // Ensure console can display Kannada properly
        Console.OutputEncoding = Encoding.UTF8;

        var converter = KannadaConverter.Instance;

        // Test with simple cases first
        string[] testCases = new[] {
            "PÀ",      // Should be ಕ
            "UÀ",      // Should be ಗ
            "M",       // Should be ಒ
            "C",       // Should be ಅ
            "PÀUÀ",    // Should be ಕಗ
        };

        Console.WriteLine("=== Simple Test Cases ===\n");
        foreach (var test in testCases)
        {
            var result = converter.ConvertAsciiToUnicode(test);
            Console.WriteLine($"'{test}' → '{result}' (bytes: {string.Join(", ", System.Text.Encoding.UTF8.GetBytes(result).Select(b => b.ToString()))})");
        }

        Console.WriteLine("\n=== Original Test ===\n");

        // Original test input
        string asciiText = "MmÁÖgÉAiÀÄ zÀÈ¶Ö¬ÄAzÀ F PÉÆÃ±ÀªÀÅ UÀzÀÄV£À ¨sÁgÀvÀzÀ CzsÀåAiÀÄ£ÀPÁgÀjUÉ ¸ÀºÁAiÀÄPÀªÁzÀgÉ, CxÀªÁ PÀÄªÀiÁgÀªÁå¸À£À£ÀÄß CxÀðªÀiÁrPÉÆ¼ÀÄîªÀ°è CªÀ£À ªÀÄÆ® GzÉÝÃ±ÀzÀ ¸À«ÄÃ¥ÀPÉÌ CªÀgÀ£ÀÄß PÉÆAqÉÆAiÀÄÝgÉ £À£Àß ±ÀæªÀÄ ¸ÁxÀðPÀ. PÀÄªÀiÁgÀªÁå¸À ¨sÁgÀvÀPÉÌ ¥ÀzÀ¥ÀæAiÉÆÃUÀPÉÆÃ±ÀªÀ£ÀÄß gÀa¸ÀÄªÀ £À£Àß F ¥ÀæAiÀÄvÀß KPÀªÀåQÛAiÀÄ ¥ÀæAiÀÄvÀßªÁVzÀÄÝ, ¸ÀºÀdªÁVAiÉÄÃ £À£Àß §Ä¢ÞAiÀÄ ¹Ã«ÄvÀvÉAiÀÄ CxÀªÁ C£ÀªÀzsÁ£ÀzÀ PÀÁgÀt¢AzÀ EzÀgÀ°è PÉ®ªÀÅ PÀqÉ vÀ¥ÀÄàUÀ¼ÁVgÀ§ºÀÄzÀÄ.  F PÉÆÃ±ÀªÀ£ÀÄß §¼À¸ÀÄªÀªÀgÀÄ EzÀgÀ°è EgÀ§ºÀÄzÁzÀ vÀ¥ÀÄàUÀ¼À£ÀÄß £À£Àß UÀªÀÄ£ÀPÉÌ vÀAzÀgÉ (vkrishna1411@yahoo.co.in, mob. 90368 57528), CªÀÅUÀ¼À£ÀÄß ªÀÄÄA¢£À DªÀÈwÛAiÀÄ°è w¢ÝPÉÆ¼ÀÄîvÉÛÃ£É.  ";


        string asciiText1 ="EzÀgÀ°è ªÀÄÄSÉÆåÃ¯ÉèÃRUÀ¼À£ÀÄß CPÁgÁ¢ PÀæªÀÄzÀ°è ¤ÃrzÉÝÃ£ÉÉ, CªÀÅUÀ¼À ªÀÄÄAzÉ DAiÀiÁ ¥ÀzÀUÀ¼À CxÀð, ¥ÀæAiÉÆÃUÀ ªÀÄvÀÄÛ ¸ÀAzÀ¨sÀðªÀ£ÀÄß ¤ÃrzÉÝÃ£É.  ¸ÀAzÀ¨sÀðªÀ£ÀÄß ¸ÀÆa¸ÀÄªÀ°è, ¸ÀA§A¢ü¹zÀ ¥ÀªÀðzÀ ªÉÆzÀ®PÀëgÀªÀ£ÀÄß (GzÁºÀgÀuÉUÉ, D¢¥ÀªÀðPÉÌ ‘D’, ¸À¨sÁ¥ÀªÀðPÉÌ ‘¸À’ JA§ ¸ÀAQë¥ÀÛªÀ£ÀÄß) C£ÀAvÀgÀ ¸ÀA¢ü, ªÀÄvÀÄÛ ¥ÀzÀå¸ÀASÉåAiÀÄ£ÀÄß PÉÆnÖzÉÝÃ£É.  ªÀÄÄSÉÆåÃ¯ÉèÃRUÀ¼À£ÀÄß ¤ÃqÀÄªÁUÀ CUÀvÀå«zÉÝqÉAiÀÄ°è £ÀÄrUÀlÄÖUÀ¼À£ÀÄß ªÀÄvÀÄÛ ¸ÀªÀiÁ¸À¥ÀzÀUÀ¼À£ÀÄß MqÉAiÀÄzÉ ºÁUÉAiÉÄÃ vÉUÉzÀÄPÉÆArzÉÝÃ£É. ªÀåQÛ£ÁªÀÄUÀ¼ÀÄ, ªÀåQÛUÀ¼À ¥ÀAiÀiÁðAiÀÄ £ÁªÀÄUÀ¼ÀÄ ªÀÄvÀÄÛ ¸ÀÜ¼À£ÁªÀÄUÀ¼ÀÆ ªÀÄÄSÉÆåÃ¯ÉèÃRUÀ¼À°è ¸ÉÃjªÉ.  EzÀjAzÀ PÉÆÃ±ÀªÀ£ÀÄß §¼À¸ÀÄªÀªÀjUÉ G¥ÀAiÉÆÃUÀªÁUÀÄvÀÛzÉ JA§ £ÀA©PÉ £À£ÀßzÀÄ.  PÀÄªÀiÁgÀªÁå¸À£À£ÀÄß CzsÀåAiÀÄ£À ªÀiÁqÀÄªÀªÀjUÉ C£ÀÄPÀÆ®ªÁUÀ§ºÀÄzÉA§ D±ÀAiÀÄ¢AzÀ PÉÆÃ±ÀzÀ PÉÆ£ÉAiÀÄ°è PÉ®ªÀÅ C£ÀÄ§AzsÀUÀ¼À£ÀÄß ¤ÃrzÉÝÃ£É. ";

        // Create output folder
        Directory.CreateDirectory("output");
        string outputPath = Path.Combine("output", "results.txt");

        // Convert ASCII → Unicode
        string result1 = converter.ConvertAsciiToUnicode(asciiText);

        // Skip Unicode → ASCII for now (not yet implemented)
        // string result2 = converter.ConvertUnicodeToAscii(unicodeText);

        // Print results to console
        Console.WriteLine("=== ಕನ್ನಡ Converter Test ===\n");

        Console.WriteLine("ASCII → Unicode:");
        Console.WriteLine($"Input:  {asciiText}");
        Console.WriteLine($"Output: {result1}\n");

        // Save results to file with UTF-8 encoding
        using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
        {
            writer.WriteLine("=== ಕನ್ನಡ Converter Test ===\n");

            writer.WriteLine("ASCII → Unicode:");
            writer.WriteLine($"Input:  {asciiText}");
            writer.WriteLine($"Output: {result1}\n");
        }

        Console.WriteLine($"Saved to: {outputPath}");
    }
}

