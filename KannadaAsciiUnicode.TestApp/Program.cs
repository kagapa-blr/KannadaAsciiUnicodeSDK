using System;
using System.IO;
using System.Text;
using System.Diagnostics;
using Kannada.AsciiUnicode.Converters;

class Program
{
    static void Main()
    {
        // Ensure console can display Kannada properly
        Console.OutputEncoding = Encoding.UTF8;

        var converter = KannadaConverter.Instance;

        // Create output folder
        Directory.CreateDirectory("output");
        string outputPath = Path.Combine("output", "results.txt");

        // Collect all test results
        var testResults = new StringBuilder();
        testResults.AppendLine("=== ಕನ್ನಡ ASCII ↔ Unicode Converter - Comprehensive Test Results ===\n");
        testResults.AppendLine($"Test Date/Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
        testResults.AppendLine($"Converter Type: Nudi/Baraha Format\n");
        testResults.AppendLine("═══════════════════════════════════════════════════════════════════\n");

        // Run tests and collect results
        RunSimpleBidirectionalTests(converter, testResults);
        testResults.AppendLine();
        RunComplexRoundTripTests(converter, testResults);

        // Save all results to file
        File.WriteAllText(outputPath, testResults.ToString(), Encoding.UTF8);
        Console.WriteLine($"\n✓ Results saved to: {outputPath}");
    }

    static void RunSimpleBidirectionalTests(KannadaConverter converter, StringBuilder results)
    {
        string[] testCases = new[] {
            "PÀ",           // Should be ಕ
            "UÀ",           // Should be ಗ
            "M",            // Should be ಒ
            "C",            // Should be ಅ
            "PÀUÀ",         // Should be ಕಗ
            "PÉ",           // Should be ಕೆ
            "PÈ",           // Should be ಕೇ
        };

        results.AppendLine("TEST 1: Simple Bidirectional Conversion (ASCII → Unicode → ASCII)");
        results.AppendLine("════════════════════════════════════════════════════════════════════\n");

        Console.WriteLine("=== Simple Bidirectional Test Cases (ASCII → Unicode → ASCII) ===\n");
        int passCount = 0;
        long totalTimeMs = 0;

        foreach (var test in testCases)
        {
            var sw = Stopwatch.StartNew();

            var unicode = converter.ConvertAsciiToUnicode(test);
            var asciiBack = converter.ConvertUnicodeToAscii(unicode);

            sw.Stop();
            totalTimeMs += sw.ElapsedMilliseconds;

            bool match = test == asciiBack;
            string status = match ? "✓ PASS" : "✗ FAIL";

            if (match) passCount++;

            string consoleLine = $"{status} | ASCII: '{test}' → Unicode: '{unicode}' → ASCII: '{asciiBack}' ({sw.ElapsedMilliseconds}ms)";
            string fileLine = $"{status} | ASCII: '{test}' → Unicode: '{unicode}' → ASCII: '{asciiBack}' | Time: {sw.ElapsedMilliseconds}ms";

            Console.WriteLine(consoleLine);
            results.AppendLine(fileLine);

            if (!match)
            {
                Console.WriteLine($"     Expected: '{test}', Got: '{asciiBack}'");
                results.AppendLine($"     Expected: '{test}', Got: '{asciiBack}'");
            }
        }

        Console.WriteLine($"\nPassed: {passCount}/{testCases.Length}\n");

        results.AppendLine();
        results.AppendLine($"Summary:");
        results.AppendLine($"  Total Tests: {testCases.Length}");
        results.AppendLine($"  Passed: {passCount}");
        results.AppendLine($"  Failed: {testCases.Length - passCount}");
        results.AppendLine($"  Success Rate: {Math.Round((passCount * 100.0) / testCases.Length, 2)}%");
        results.AppendLine($"  Total Time: {totalTimeMs}ms");
        if (testCases.Length > 0)
        {
            results.AppendLine($"  Average Time Per Test: {Math.Round((double)totalTimeMs / testCases.Length, 3)}ms");
        }
        results.AppendLine();
    }

    static void RunComplexRoundTripTests(KannadaConverter converter, StringBuilder results)
    {
        string asciiText = "MmÁÖgÉAiÀÄ zÀÈ¶Ö¬ÄAzÀ F PÉÆÃ±ÀªÀÅ UÀzÀÄV£À ¨sÁgÀvÀzÀ CzsÀåAiÀÄ£ÀPÁgÀjUÉ ¸ÀºÁAiÀÄPÀªÁzÀgÉ, CxÀªÁ PÀÄªÀiÁgÀªÁå¸À£À£ÀÄß CxÀðªÀiÁrPÉÆ¼ÀÄîªÀ°è CªÀ£À ªÀÄÆ® GzÉÝÃ±ÀzÀ ¸À«ÄÃ¥ÀPÉÌ CªÀgÀ£ÀÄß PÉÆAqÉÆAiÀÄÝgÉ £À£Àß ±ÀæªÀÄ ¸ÁxÀðPÀ. PÀÄªÀiÁgÀªÁå¸À ¨sÁgÀvÀPÉÌ ¥ÀzÀ¥ÀæAiÉÆÃUÀPÉÆÃ±ÀªÀ£ÀÄß gÀa¸ÀÄªÀ £À£Àß F ¥ÀæAiÀÄvÀß KPÀªÀåQÛAiÀÄ ¥ÀæAiÀÄvÀßªÁVzÀÄÝ, ¸ÀºÀdªÁVAiÉÄÃ £À£Àß §Ä¢ÞAiÀÄ ¹Ã«ÄvÀvÉAiÀÄ CxÀªÁ C£ÀªÀzsÁ£ÀzÀ PÀÁgÀt¢AzÀ EzÀgÀ°è PÉ®ªÀÅ PÀqÉ vÀ¥ÀÄàUÀ¼ÁVgÀ§ºÀÄzÀÄ.  F PÉÆÃ±ÀªÀ£ÀÄß §¼À¸ÀÄªÀªÀgÀÄ EzÀgÀ°è EgÀ§ºÀÄzÁzÀ vÀ¥ÀÄàUÀ¼À£ÀÄß £À£Àß UÀªÀÄ£ÀPÉÌ vÀAzÀgÉ (vkrishna1411@yahoo.co.in, mob. 90368 57528), CªÀÅUÀ¼À£ÀÄß ªÀÄÄA¢£À DªÀÈwÛAiÀÄ°è w¢ÝPÉÆ¼ÀÄîvÉÛÃ£É.  ";

        results.AppendLine("TEST 2: Complex Text Round-Trip Conversion");
        results.AppendLine("════════════════════════════════════════════════════════════════════\n");

        Console.WriteLine("=== Complex Text Converter Test (Long Kannada Text) ===\n");

        // ASCII to Unicode
        var swA2U = Stopwatch.StartNew();
        string unicodeResult = converter.ConvertAsciiToUnicode(asciiText);
        swA2U.Stop();

        // Unicode to ASCII
        var swU2A = Stopwatch.StartNew();
        string asciiRoundTrip = converter.ConvertUnicodeToAscii(unicodeResult);
        swU2A.Stop();

        long totalTime = swA2U.ElapsedMilliseconds + swU2A.ElapsedMilliseconds;

        results.AppendLine($"Input Text Details:");
        results.AppendLine($"  ASCII Input Length: {asciiText.Length} characters");
        results.AppendLine($"  ASCII Input Bytes: {Encoding.UTF8.GetByteCount(asciiText)} bytes");
        results.AppendLine();

        results.AppendLine($"Conversion 1: ASCII → Unicode");
        results.AppendLine($"  Time Taken: {swA2U.ElapsedMilliseconds}ms");
        results.AppendLine($"  Output Length: {unicodeResult.Length} characters");
        results.AppendLine($"  Output Bytes: {Encoding.UTF8.GetByteCount(unicodeResult)} bytes");
        results.AppendLine($"  Full Output:");
        results.AppendLine($"{unicodeResult}");
        results.AppendLine();

        results.AppendLine($"Conversion 2: Unicode → ASCII (Round-trip)");
        results.AppendLine($"  Time Taken: {swU2A.ElapsedMilliseconds}ms");
        results.AppendLine($"  Output Length: {asciiRoundTrip.Length} characters");
        results.AppendLine($"  Output Bytes: {Encoding.UTF8.GetByteCount(asciiRoundTrip)} bytes");
        results.AppendLine($"  Full Output:");
        results.AppendLine($"{asciiRoundTrip}");
        results.AppendLine();

        bool roundTripMatch = asciiText == asciiRoundTrip;
        results.AppendLine($"Round-trip Analysis:");
        results.AppendLine($"  Perfect Match: {(roundTripMatch ? "YES ✓" : "NO ✗")}");

        if (!roundTripMatch)
        {
            int differences = 0;
            int minLength = Math.Min(asciiText.Length, asciiRoundTrip.Length);
            for (int i = 0; i < minLength; i++)
            {
                if (asciiText[i] != asciiRoundTrip[i])
                    differences++;
            }
            differences += Math.Abs(asciiText.Length - asciiRoundTrip.Length);

            double similarity = Math.Round((1.0 - (double)differences / asciiText.Length) * 100, 2);
            results.AppendLine($"  Differences: {differences} characters out of {asciiText.Length}");
            results.AppendLine($"  Similarity: {similarity}%");
        }

        Console.WriteLine("ASCII → Unicode:");
        Console.WriteLine($"Time: {swA2U.ElapsedMilliseconds}ms");
        Console.WriteLine($"Output Length: {unicodeResult.Length} chars\n");

        Console.WriteLine("Unicode → ASCII (Round-trip):");
        Console.WriteLine($"Time: {swU2A.ElapsedMilliseconds}ms");
        Console.WriteLine($"Round-trip Match: {(roundTripMatch ? "✓ YES" : "✗ NO")}\n");

        results.AppendLine();
        results.AppendLine($"Performance Metrics:");
        results.AppendLine($"  ASCII → Unicode Time: {swA2U.ElapsedMilliseconds}ms");
        results.AppendLine($"  Unicode → ASCII Time: {swU2A.ElapsedMilliseconds}ms");
        results.AppendLine($"  Total Round-trip Time: {totalTime}ms");
        results.AppendLine($"  Average Time Per Conversion: {Math.Round((double)totalTime / 2, 3)}ms");
        results.AppendLine();

        results.AppendLine("═══════════════════════════════════════════════════════════════════");
        results.AppendLine("Test Execution Complete");
        results.AppendLine($"Report Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
    }
}

