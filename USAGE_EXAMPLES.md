# Kannada ASCII to Unicode Conversion - Usage Examples

This document provides practical examples of using the Kannada.AsciiUnicode SDK.

## Table of Contents
1. [Basic Conversions](#basic-conversions)
2. [File Processing](#file-processing)
3. [Advanced Scenarios](#advanced-scenarios)
4. [Error Handling](#error-handling)
5. [Performance Optimization](#performance-optimization)

## Basic Conversions

### Example 1: Simple ASCII to Unicode

```csharp
using Kannada.AsciiUnicode.Converters;

// Get singleton instance
var converter = KannadaConverter.Instance;

// Convert a single word
string ascii = "PÀ£ï";  // Kannada word in ASCII (Nudi format)
string unicode = converter.ConvertAsciiToUnicode(ascii);
Console.WriteLine(unicode);  // Output: ಕನ್ನ
```

### Example 2: Simple Unicode to ASCII

```csharp
var converter = KannadaConverter.Instance;

// Convert Kannada Unicode to ASCII
string unicode = "ಕನ್ನಡ";
string ascii = converter.ConvertUnicodeToAscii(unicode);
Console.WriteLine(ascii);  // Output: PÀ£ï£ÀqÀ
```

### Example 3: Converting Multiple Sentences

```csharp
var converter = KannadaConverter.Instance;

// Full paragraph conversion
string paragraph = "PÀ£ï£ÀqÀ ªÀiÁrPÉÆnÖgÀÄªÀ ¸ÀzÀ©ü£ÀªÀÅ. ¸ÀzÀ¸ÀåjUÀÆ ±ÀæªÀÄªÀ£ÀÄß ¸Á¬ÄgÀzÉ.";

string result = converter.ConvertAsciiToUnicode(paragraph);
Console.WriteLine(result);
// Outputs properly converted Kannada text
```

## File Processing

### Example 4: Convert ASCII File to Unicode

```csharp
using System;
using System.IO;
using System.Text;
using Kannada.AsciiUnicode.Converters;

var converter = KannadaConverter.Instance;

// Read ASCII-encoded Kannada file
string inputPath = "kannada_ascii.txt";
string outputPath = "kannada_unicode.txt";

try
{
    // Read with appropriate encoding (Windows-1252 is common for Nudi)
    string[] lines = File.ReadAllLines(inputPath, Encoding.GetEncoding(1252));
    
    // Convert each line
    var convertedLines = new List<string>();
    foreach (string line in lines)
    {
        if (!string.IsNullOrWhiteSpace(line))
        {
            string converted = converter.ConvertAsciiToUnicode(line);
            convertedLines.Add(converted);
        }
        else
        {
            convertedLines.Add(line);  // Preserve empty lines
        }
    }
    
    // Write Unicode Kannada to file (UTF-8)
    File.WriteAllLines(outputPath, convertedLines, Encoding.UTF8);
    
    Console.WriteLine($"Converted {lines.Length} lines");
    Console.WriteLine($"Output saved to: {outputPath}");
}
catch (FileNotFoundException)
{
    Console.WriteLine($"File not found: {inputPath}");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

### Example 5: Convert Unicode File to ASCII

```csharp
var converter = KannadaConverter.Instance;

string inputPath = "kannada_unicode.txt";
string outputPath = "kannada_ascii.txt";

// Read Unicode file
string[] lines = File.ReadAllLines(inputPath, Encoding.UTF8);

// Convert each line
var convertedLines = new List<string>();
foreach (string line in lines)
{
    if (!string.IsNullOrWhiteSpace(line))
    {
        convertedLines.Add(converter.ConvertUnicodeToAscii(line));
    }
    else
    {
        convertedLines.Add(line);
    }
}

// Write with ASCII-compatible encoding
File.WriteAllLines(outputPath, convertedLines, Encoding.GetEncoding(1252));
```

### Example 6: Batch Convert Files in Directory

```csharp
var converter = KannadaConverter.Instance;

string sourceDir = "C:\\Kannada\\ASCII";
string targetDir = "C:\\Kannada\\Unicode";

// Create target directory if it doesn't exist
Directory.CreateDirectory(targetDir);

// Process all .txt files
foreach (string filePath in Directory.GetFiles(sourceDir, "*.txt"))
{
    try
    {
        string fileName = Path.GetFileName(filePath);
        string outputPath = Path.Combine(targetDir, fileName);
        
        // Read and convert
        string content = File.ReadAllText(filePath, Encoding.GetEncoding(1252));
        string converted = converter.ConvertAsciiToUnicode(content);
        
        // Write output
        File.WriteAllText(outputPath, converted, Encoding.UTF8);
        
        Console.WriteLine($"✓ Converted: {fileName}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Error processing {filePath}: {ex.Message}");
    }
}

Console.WriteLine("Batch conversion complete");
```

## Advanced Scenarios

### Example 7: Format Detection and Conversion

```csharp
using Kannada.AsciiUnicode.Enums;

var converter = KannadaConverter.Instance;

// Automatic format detection
string input = "PÀ£ï";
string result = converter.Convert(input, KannadaAsciiFormat.Default);

// Explicit Nudi format
result = converter.Convert(input, KannadaAsciiFormat.Nudi);

// Explicit Baraha format  
result = converter.Convert(input, KannadaAsciiFormat.Baraha);
```

### Example 8: Round-Trip Conversion Validation

```csharp
var converter = KannadaConverter.Instance;

string[] testWords = new[]
{
    "ಕನ್ನಡ",
    "ಭಾರತ",
    "ಸಾಹಿತ್ಯ",
    "ವಿಜ್ಞಾನ"
};

bool allValid = true;

foreach (string word in testWords)
{
    // Convert Unicode -> ASCII -> Unicode
    string ascii = converter.ConvertUnicodeToAscii(word);
    string backToUnicode = converter.ConvertAsciiToUnicode(ascii);
    
    bool isValid = word == backToUnicode;
    
    Console.WriteLine($"{word} -> {ascii} -> {backToUnicode} : {(isValid ? "✓" : "✗")}");
    
    if (!isValid)
        allValid = false;
}

Console.WriteLine($"\nAll conversions valid: {allValid}");
```

### Example 9: Performance Benchmarking

```csharp
using System.Diagnostics;

var converter = KannadaConverter.Instance;

// Create test data
var testData = Enumerable.Range(0, 10000)
    .Select(i => "PÀ£ï£ÀqÀ ªÀiÁrPÉÆnÖgÀÄªÀ ¸ÀzÀ©ü£ÀªÀÅ")
    .ToList();

// Benchmark ASCII to Unicode
var watch = Stopwatch.StartNew();
var results = testData.Select(t => converter.ConvertAsciiToUnicode(t)).ToList();
watch.Stop();

double itemsPerSecond = (testData.Count / (double)watch.ElapsedMilliseconds) * 1000;
Console.WriteLine($"Processed {testData.Count} items in {watch.ElapsedMilliseconds}ms");
Console.WriteLine($"Performance: {itemsPerSecond:F0} items/second");
Console.WriteLine($"Per-item: {watch.ElapsedMilliseconds / (double)testData.Count:F4}ms");
```

### Example 10: Stream Processing

```csharp
using System.IO;

var converter = KannadaConverter.Instance;

// Process large file line by line
string inputPath = "large_kannada_file.txt";
string outputPath = "large_kannada_converted.txt";

using (var reader = new StreamReader(inputPath, Encoding.GetEncoding(1252)))
using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
{
    string line;
    int lineCount = 0;
    
    while ((line = reader.ReadLine()) != null)
    {
        string converted = converter.ConvertAsciiToUnicode(line);
        writer.WriteLine(converted);
        lineCount++;
        
        if (lineCount % 1000 == 0)
            Console.WriteLine($"Processed {lineCount} lines...");
    }
    
    Console.WriteLine($"Completed: {lineCount} lines processed");
}
```

## Error Handling

### Example 11: Handling Null and Empty Input

```csharp
var converter = KannadaConverter.Instance;

// Safe conversion with validation
string ConvertSafely(string input, bool toUnicode = true)
{
    // Check for null
    if (input == null)
        return null;  // Or throw custom exception
    
    // Check for empty
    if (string.IsNullOrWhiteSpace(input))
        return string.Empty;
    
    try
    {
        return toUnicode 
            ? converter.ConvertAsciiToUnicode(input)
            : converter.ConvertUnicodeToAscii(input);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Conversion error: {ex.Message}");
        return input;  // Return original on error
    }
}

// Usage
string result = ConvertSafely("PÀ£ï");
string nullResult = ConvertSafely(null);
string emptyResult = ConvertSafely("");
```

### Example 12: Error Handling with Logging

```csharp
using System.Collections.Generic;

var converter = KannadaConverter.Instance;

class ConversionResult
{
    public string Original { get; set; }
    public string Converted { get; set; }
    public bool Success { get; set; }
    public string Error { get; set; }
}

List<ConversionResult> ConvertWithLogging(string[] inputs)
{
    var results = new List<ConversionResult>();
    
    foreach (string input in inputs)
    {
        try
        {
            string converted = converter.ConvertAsciiToUnicode(input);
            
            results.Add(new ConversionResult
            {
                Original = input,
                Converted = converted,
                Success = true
            });
        }
        catch (ArgumentNullException)
        {
            results.Add(new ConversionResult
            {
                Original = input,
                Success = false,
                Error = "Input was null"
            });
        }
        catch (Exception ex)
        {
            results.Add(new ConversionResult
            {
                Original = input,
                Success = false,
                Error = ex.Message
            });
        }
    }
    
    return results;
}
```

## Performance Optimization

### Example 13: Optimized Batch Processing

```csharp
var converter = KannadaConverter.Instance;

// Use parallel processing for large datasets
var items = GetLargeDataset();  // 100,000+ items

var results = items
    .AsParallel()
    .WithDegreeOfParallelism(Environment.ProcessorCount)
    .Select(item => new
    {
        Original = item,
        Converted = converter.ConvertAsciiToUnicode(item)
    })
    .ToList();

Console.WriteLine($"Processed {results.Count} items in parallel");
```

### Example 14: Caching for Repeated Conversions

```csharp
using System.Collections.Generic;

var converter = KannadaConverter.Instance;

class CachingConverter
{
    private Dictionary<string, string> _cache = new();
    
    public string ConvertWithCache(string input)
    {
        if (_cache.TryGetValue(input, out var cached))
            return cached;
        
        string result = converter.ConvertAsciiToUnicode(input);
        _cache[input] = result;
        return result;
    }
    
    public void ClearCache() => _cache.Clear();
}

// Usage
var cachingConverter = new CachingConverter();
string result1 = cachingConverter.ConvertWithCache("PÀ£ï");  // Computed
string result2 = cachingConverter.ConvertWithCache("PÀ£ï");  // From cache
```

### Example 15: Memory-Efficient Large File Processing

```csharp
// Don't load entire file into memory
string inputPath = "huge_kannada_file.txt";
string outputPath = "huge_kannada_converted.txt";

const int bufferSize = 1024 * 1024;  // 1 MB buffer

using (var reader = new StreamReader(inputPath, Encoding.GetEncoding(1252), false, bufferSize))
using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8, bufferSize))
{
    var sb = new StringBuilder();
    string line;
    int lineCount = 0;
    
    while ((line = reader.ReadLine()) != null)
    {
        string converted = converter.ConvertAsciiToUnicode(line);
        writer.WriteLine(converted);
        lineCount++;
        
        // Periodic garbage collection for very large files
        if (lineCount % 10000 == 0)
            GC.Collect(0);  // Collect generation 0
    }
}
```

---

## Running the Examples

To run these examples in a C# project:

1. Add NuGet reference:
   ```bash
   dotnet add package Kannada.AsciiUnicode
   ```

2. Add using statements:
   ```csharp
   using System;
   using System.IO;
   using System.Text;
   using System.Collections.Generic;
   using System.Linq;
   using Kannada.AsciiUnicode.Converters;
   using Kannada.AsciiUnicode.Enums;
   ```

3. Copy example code and run!

## Tips & Best Practices

1. **Always use singleton instance**: `KannadaConverter.Instance`
2. **Set console encoding for Unicode output**: `Console.OutputEncoding = Encoding.UTF8;`
3. **Use UTF-8 for file I/O**: `new StreamWriter(path, false, Encoding.UTF8)`
4. **Validate input before conversion**: Check for null/empty
5. **Use try-catch for robust applications**: Handle potential exceptions
6. **Cache repeated conversions**: For frequently converted strings
7. **Use streaming for large files**: Don't load entire files into memory
8. **Test round-trip conversions**: Verify data integrity

## Support

For more examples and detailed API documentation, see `SDK_DOCUMENTATION.md`
