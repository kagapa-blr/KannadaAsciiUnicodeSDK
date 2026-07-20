# Kannada ASCII to Unicode Converter

A high-performance C# library for converting between legacy Kannada ASCII encodings (Nudi, Baraha) and modern Unicode. Developed and maintained by Kannada Ganaka Parishat (KAGAPA).

## Overview

The Kannada ASCII to Unicode Converter provides robust bidirectional conversion for Kannada text. It handles complex linguistic features including consonant clusters, vowel signs, and special conjunctions with zero external dependencies.

## Key Features

- Bidirectional conversion (ASCII to Unicode and Unicode to ASCII)
- Handles Kannada consonant clusters and conjuncts correctly
- Proper placement of vowel signs and dependent forms
- Support for optional custom mappings
- Zero external dependencies
- Optimized for performance
- Optional digit handling with Kannada digits preserved by default
- 71+ unit tests with 100% pass rate

## Installation

### Via NuGet

```bash
dotnet add package KannadaAsciiUnicodeSDK
```

Visit [NuGet Package](https://www.nuget.org/packages/KannadaAsciiUnicodeSDK) for more information.

### Local Development

Clone the repository and build locally:

```bash
git clone <repository-url>
cd KannadaAsciiUnicode
dotnet build
```

## Quick Start

### Basic Usage

```csharp
using Kannada.AsciiUnicode.Converters;

// Get the default converter (singleton)
var converter = KannadaConverter.Instance;

// ASCII (Nudi/Baraha) to Unicode
string unicode = converter.ConvertAsciiToUnicode("PÀ");
// Output: "ಕ"

// Unicode to ASCII
string ascii = converter.ConvertUnicodeToAscii("ಕ");
// Output: "PÀ"

// Optional: emit English digits instead of Kannada digits
string englishDigits = converter.ConvertAsciiToUnicode("12345", convertToEnglishDigit: true);
// Output: "12345"
```

### Custom Mappings

For domain-specific or custom words, extend the default mappings:

```csharp
var customMapping = new Dictionary<string, string>
{
    { "ka", "ಕ" },
    { "kaa", "ಕಾ" },
    { "ma", "ಮ" }
};

var converter = KannadaConverter.CreateWithCustomMapping(customMapping);
string result = converter.ConvertAsciiToUnicode("ka");  // Returns: "ಕ"
```

Note: Custom ASCII to Unicode mappings are automatically reversed for Unicode to ASCII conversion.

## API Reference

### KannadaConverter Class

**Static Properties:**
- `Instance` - Singleton instance with default mappings

**Static Methods:**
- `CreateWithCustomMapping(Dictionary<string, string>?)` - Creates converter with optional custom mappings

**Instance Methods:**
- `ConvertAsciiToUnicode(string)` - Converts ASCII text to Kannada Unicode using the default behavior
- `ConvertAsciiToUnicode(string, bool)` - Converts ASCII text to Kannada Unicode and optionally emits English digits
- `ConvertUnicodeToAscii(string)` - Converts Kannada Unicode to ASCII format
- `ConvertUnicodeToAscii(string, bool)` - Converts Kannada Unicode to ASCII format and optionally uses English digits
- `Convert(string, KannadaAsciiFormat, bool = false)` - Routes conversion based on format (Nudi/Baraha)

## Supported Formats

- **Input Formats:** Kannada ASCII (Nudi, Baraha)
- **Output Format:** Kannada Unicode (U+0C80 to U+0CF2)

## Technical Details

### Consonant Clusters and Zero-Width Joiner

The converter correctly handles Kannada consonant clusters that require Zero-Width Joiner (ZWJ) characters to prevent ligature formation in text renderers. For example:

```
Input:  gÁåAPï
Output: ರ‍್ಯಾಂಕ್   (includes ZWJ after ರಾ)
```

This ensures proper rendering across different applications and platforms.

### Architecture

The library uses a longest-match-first algorithm for ASCII sequence mapping, ensuring accurate conversion of multi-character sequences. Special handling is provided for:

- Vattaksharagalu (consonant modifiers with vowel preservation)
- Arkavattu (subjoined consonants)
- Broken cases (vowel transformations)
- Dependent vowels and conjuncts

## Project Structure

```
KannadaAsciiUnicode/
├── Kannada.AsciiUnicode/           # Core SDK library
│   ├── Converters/                 # Conversion engines
│   ├── Mappings/                   # ASCII/Unicode mapping loaders
│   ├── Resources/                  # JSON mapping data
│   ├── Interfaces/                 # Public contracts
│   └── README.md                   # SDK documentation
├── Kannada.AsciiUnicode.Tests/     # Unit tests (18+ test cases)
├── KannadaAsciiUnicode.TestApp/    # Console demonstration
└── DEVELOPER_GUIDE.md              # Contribution guidelines
```

## Build and Testing

### Build

```bash
dotnet build
```

### Run Tests

```bash
dotnet test
```

Expected output:
```
Test summary: total: 71, failed: 0, succeeded: 71
```

### Run Test Application

```bash
cd KannadaAsciiUnicode.TestApp
dotnet run
```

## Examples

### Batch Processing

```csharp
var converter = KannadaConverter.Instance;
string[] inputs = { "PÀ", "gÀä", "zÀÈ¶Ö¬ÄAzÀ" };

foreach (var input in inputs)
{
    string output = converter.ConvertAsciiToUnicode(input);
    Console.WriteLine($"{input} -> {output}");
}
```

### Round-Trip Conversion

```csharp
var converter = KannadaConverter.Instance;

string original = "ಕನ್ನಡ";
string toAscii = converter.ConvertUnicodeToAscii(original);
string backToUnicode = converter.ConvertAsciiToUnicode(toAscii);

// backToUnicode == original
```

### File Content Conversion

```csharp
var converter = KannadaConverter.Instance;

string content = File.ReadAllText("input.txt", Encoding.UTF8);
string converted = converter.ConvertAsciiToUnicode(content);
File.WriteAllText("output.txt", converted, Encoding.UTF8);
```

## Supported .NET Versions

- .NET Standard 2.0+
- .NET Framework 4.7+
- .NET Core 3.1+
- .NET 5, 6, 7, 8, 9, 10+

## Contributing

Contributions are welcome. Please see [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) for:

- Code contribution guidelines
- Testing requirements
- Adding new ASCII/Unicode mappings
- Improving documentation

## Related Resources

- Reference Implementation: https://9zx.in/sanka/
- Kannada Unicode: https://en.wikipedia.org/wiki/Kannada_(Unicode_block)

## License

MIT License

Developed and maintained by Kannada Ganaka Parishat (KAGAPA)

## About KAGAPA

Kannada Ganaka Parishat (KAGAPA) is a community organization dedicated to promoting Kannada-language computing, software development, and digital tools that support the Kannada language ecosystem.
