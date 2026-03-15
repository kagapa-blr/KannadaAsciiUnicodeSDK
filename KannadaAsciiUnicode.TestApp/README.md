# TestApp - Kannada ASCII to Unicode Conversion Demonstration

Sample console application demonstrating real-world usage of the Kannada ASCII to Unicode converter.

## Overview

The TestApp is a comprehensive demonstration that shows:

- Basic ASCII to Unicode conversion
- Unicode to ASCII conversion
- Bidirectional round-trip conversion
- Performance benchmarking
- DOCX file conversion (Word document processing)
- Batch text processing
- Output file writing

## Running the Application

### Build

```bash
cd KannadaAsciiUnicode.TestApp
dotnet build
```

### Run

```bash
dotnet run
```

### Run with Arguments

```bash
dotnet run --configuration Release
```

## Output

The application generates several output files:

### Text Conversion Output

**File:** `output/conversion_results.txt`

Contains:
- Timestamp of conversion
- Original ASCII text
- Converted Unicode text
- Performance metrics (conversion time)
- Round-trip conversion results

### DOCX Conversion Output

**Files:** 
- `output/ascii_to_unicode.docx` - DOCX file converted from ASCII to Unicode
- `output/unicode_to_ascii.docx` - DOCX file converted from Unicode to ASCII

Performance metrics for each file conversion.

### Example Output

```
=== Kannada ASCII to Unicode Conversion ===
Time: 2026-03-15 14:30:00

Original ASCII:
MmÁÖgÉAiÀÄ zÀÈ¶Ö¬ÄAzÀ

Unicode (12ms):
ಮುಂಬೈ ಧರ್ಮಾಇ

Round-trip ASCII (8ms):
MmÁÖgÉAiÀÄ zÀÈ¶Ö¬ÄAzÀ
```

## Code Structure

```
KannadaAsciiUnicode.TestApp/
├── Program.cs          # Main entry point with conversion logic
├── Helpers/
│   └── DocxHelper.cs   # DOCX file processing utilities
├── input/
│   └── asciiText.txt   # Sample ASCII input
├── TestData/
│   └── Docx/           # Sample DOCX files for testing
└── output/             # Generated output files
```

## Usage Examples

### Example 1: Basic Conversion

```csharp
using Kannada.AsciiUnicode.Converters;

var converter = KannadaConverter.Instance;

// ASCII to Unicode
string unicode = converter.ConvertAsciiToUnicode("PÀ£ÀßqÀ");
Console.WriteLine($"Unicode: {unicode}");  // Output: ಕನ್ನಡ

// Unicode to ASCII
string ascii = converter.ConvertUnicodeToAscii("ಕನ್ನಡ");
Console.WriteLine($"ASCII: {ascii}");  // Output: PÀ£ï£À...
```

### Example 2: Custom Mappings

```csharp
var customMapping = new Dictionary<string, string>
{
    { "wÃPÀëÚ", "ತೀಕ್ಷ್ಣ" },
    { "PÀëÚ", "ಕ್ಷ್ಣ" }
};

var converter = KannadaConverter.CreateWithCustomMapping(customMapping);
string result = converter.ConvertAsciiToUnicode("wÃPÀëÚ");
```

### Example 3: Performance Measurement

```csharp
var converter = KannadaConverter.Instance;
var sw = Stopwatch.StartNew();

string result = converter.ConvertAsciiToUnicode("PÀ£ÀßqÀ");

sw.Stop();
Console.WriteLine($"Conversion Time: {sw.Elapsed.TotalMilliseconds}ms");
```

### Example 4: Batch Processing

```csharp
var converter = KannadaConverter.Instance;
var inputs = new[] { "PÀ", "gÀä", "zÀÈ¶" };

foreach (var input in inputs)
{
    string output = converter.ConvertAsciiToUnicode(input);
    Console.WriteLine($"{input} → {output}");
}
```

## DOCX File Processing

### DocxHelper Utilities

The `DocxHelper` class provides:

- `ConvertDocx()` - Converts text within a DOCX file
- Preserves document formatting and structure
- Measures conversion performance
- Returns conversion time in milliseconds

### Example: Converting DOCX Files

```csharp
var converter = KannadaConverter.Instance;

long elapsedMs = DocxHelper.ConvertDocx(
    "input.docx",
    "output.docx",
    converter.ConvertAsciiToUnicode
);

Console.WriteLine($"Conversion completed in {elapsedMs}ms");
```

## Performance Characteristics

### Text Conversion Speed

- Single conversion: < 1ms
- 1000 conversions: < 100ms
- Average throughput: > 10,000 conversions/second

### Sample Benchmarks

```
ASCII to Unicode (PÀ£ÀßqÀ):  0.5ms
Unicode to ASCII (ಕನ್ನಡ):      0.4ms
DOCX File (5KB):               15ms
Batch (1000 items):            85ms
```

## Customization

### Modify Test Data

Edit `Program.cs` to change:

1. **Custom mappings**: Add or remove entries in `customMapping` Dictionary
2. **Sample ASCII text**: Modify `asciiText` variable
3. **DOCX input files**: Replace files in `TestData/Docx/` folder
4. **Output directory**: Change `outputDir` path

### Add New Test Cases

```csharp
var testCases = new List<(string ascii, string unicode)>
{
    ("PÀ", "ಕ"),
    ("your_test", "ನಿಮ್ಮ_ಪರೀಕ್ಷೆ"),
};

foreach (var (ascii, expected) in testCases)
{
    var result = converter.ConvertAsciiToUnicode(ascii);
    Console.WriteLine($"{ascii} → {result}");
}
```

## Troubleshooting

### DOCX Files Not Found

Ensure input DOCX files exist in `TestData/Docx/` directory:
- `ascii_input.docx`
- `unicode_input.docx`

Create placeholder files or use your own DOCX documents.

### Output Directory Permission Denied

Ensure the `output/` directory exists and has write permissions:

```bash
mkdir output
```

### Character Encoding Issues

Ensure console supports UTF-8:

```csharp
Console.OutputEncoding = Encoding.UTF8;
```

This is already set in Program.cs.

## Integration with Your Application

### Console Application

```csharp
var converter = KannadaConverter.Instance;
string convertedText = converter.ConvertAsciiToUnicode(userInput);
Console.WriteLine(convertedText);
```

### Web Application

```csharp
[HttpPost("convert")]
public IActionResult Convert([FromBody] ConversionRequest request)
{
    var converter = KannadaConverter.Instance;
    string result = converter.ConvertAsciiToUnicode(request.AsciiText);
    return Ok(new { unicode = result });
}
```

### Desktop Application

```csharp
private readonly KannadaConverter _converter = KannadaConverter.Instance;

private void ConvertButton_Click(object sender, EventArgs e)
{
    string input = inputTextBox.Text;
    string output = _converter.ConvertAsciiToUnicode(input);
    outputTextBox.Text = output;
}
```

## Resources

- For full API reference: See [Kannada.AsciiUnicode/README.md](../Kannada.AsciiUnicode/README.md)
- For contribution guidelines: See [DEVELOPER_GUIDE.md](../DEVELOPER_GUIDE.md)
- For testing reference: See [Kannada.AsciiUnicode.Tests/README.md](../Kannada.AsciiUnicode.Tests/README.md)

## License

MIT License
