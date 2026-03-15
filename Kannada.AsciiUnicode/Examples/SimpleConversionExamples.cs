using System;
using System.Collections.Generic;
using Kannada.AsciiUnicode.Converters;

namespace Kannada.AsciiUnicode.Examples
{
    /// <summary>
    /// Simple examples for Kannada ASCII ↔ Unicode conversion
    /// </summary>
    public static class SimpleConversionExamples
    {
        /// <summary>
        /// Example 1: Using default converter (no setup needed)
        /// </summary>
        public static void Example1_DefaultConverter()
        {
            Console.WriteLine("=== Example 1: Default Converter ===\n");

            var converter = KannadaConverter.Instance;

            // ASCII to Unicode
            string asciiInput = "PÀ";
            string unicode = converter.ConvertAsciiToUnicode(asciiInput);
            Console.WriteLine($"ASCII: {asciiInput} → Unicode: {unicode}");

            // Unicode to ASCII
            string unicodeInput = "ಕ";
            string ascii = converter.ConvertUnicodeToAscii(unicodeInput);
            Console.WriteLine($"Unicode: {unicodeInput} → ASCII: {ascii}");

            Console.WriteLine();
        }

        /// <summary>
        /// Example 2: Using custom mappings
        /// </summary>
        public static void Example2_CustomMappings()
        {
            Console.WriteLine("=== Example 2: Custom Mappings ===\n");

            // Define custom ASCII→Unicode mappings
            var customMapping = new Dictionary<string, string>
            {
                { "ka", "ಕ" },
                { "kaa", "ಕಾ" },
                { "ma", "ಮ" },
                { "maa", "ಮಾ" },
                { "ra", "ರ" },
                { "raa", "ರಾ" }
            };

            // Create converter with custom mappings
            var converter = KannadaConverter.CreateWithCustomMapping(customMapping);

            // Use it
            string[] testCases = { "ka", "kaa", "ma", "ra" };
            foreach (var test in testCases)
            {
                string result = converter.ConvertAsciiToUnicode(test);
                Console.WriteLine($"{test} → {result}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Example 3: Bidirectional conversion
        /// </summary>
        public static void Example3_BidirectionalConversion()
        {
            Console.WriteLine("=== Example 3: Bidirectional Conversion ===\n");

            var converter = KannadaConverter.Instance;

            string[] unicodeTexts = { "ಕ", "ಮ", "ರ", "ನ" };

            foreach (var unicode in unicodeTexts)
            {
                string ascii = converter.ConvertUnicodeToAscii(unicode);
                string backToUnicode = converter.ConvertAsciiToUnicode(ascii);

                Console.WriteLine($"Unicode: {unicode}");
                Console.WriteLine($"  → ASCII: {ascii}");
                Console.WriteLine($"  → Back to Unicode: {backToUnicode}");
                Console.WriteLine();
            }
        }

        /// <summary>
        /// Example 4: Batch processing
        /// </summary>
        public static void Example4_BatchProcessing()
        {
            Console.WriteLine("=== Example 4: Batch Processing ===\n");

            var converter = KannadaConverter.Instance;

            string[] inputs = { "PÀ", "gÀä", "zÀÄgÀä", "PÀgÀä" };

            Console.WriteLine("Converting multiple ASCII sequences:\n");
            foreach (var input in inputs)
            {
                string output = converter.ConvertAsciiToUnicode(input);
                Console.WriteLine($"  {input,-12} → {output}");
            }

            Console.WriteLine();
        }

        /// <summary>
        /// Run all examples
        /// </summary>
        public static void RunAllExamples()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════╗");
            Console.WriteLine("║  Kannada ASCII ↔ Unicode Converter - Simple API  ║");
            Console.WriteLine("╚════════════════════════════════════════════════════╝\n");

            try
            {
                Example1_DefaultConverter();
                Example2_CustomMappings();
                Example3_BidirectionalConversion();
                Example4_BatchProcessing();

                Console.WriteLine("╔════════════════════════════════════════════════════╗");
                Console.WriteLine("║  All examples completed successfully! ✓            ║");
                Console.WriteLine("╚════════════════════════════════════════════════════╝");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n✗ Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
