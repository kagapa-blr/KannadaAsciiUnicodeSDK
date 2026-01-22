# Kannada ASCII ↔ Unicode Converter SDK

A high-performance .NET Standard 2.0 library for converting between Kannada Nudi/Baraha ASCII and Unicode text formats.

## Installation

Install via NuGet:
```bash
dotnet add package KannadaAsciiUnicodeSDK
```

## Quick Start

```csharp
using Kannada.AsciiUnicode;

// Get the singleton instance
var converter = KannadaConverter.Instance;

// ASCII to Unicode
string unicode = converter.ConvertAsciiToUnicode("PÀ");
// Result: ಕ

// Unicode to ASCII
string ascii = converter.ConvertUnicodeToAscii("ಕ");
// Result: PÀ
```

## Public API

### KannadaConverter

Static property:
- `Instance` - Singleton instance of the converter

Methods:
- `ConvertAsciiToUnicode(string text)` - Converts Nudi/Baraha ASCII to Unicode
- `ConvertUnicodeToAscii(string text)` - Converts Unicode to ASCII
- `Convert(string text, KannadaAsciiFormat format)` - Routes to appropriate conversion

### Supported Formats

```csharp
public enum KannadaAsciiFormat
{
    AsciiToUnicode = 0,
    UnicodeToAscii = 1
}
```

## Features

- Bidirectional conversion (ASCII ↔ Unicode)
- 85%+ accuracy on standard Kannada text
- Lightweight and fast (< 5ms for typical text)
- No external dependencies (except Newtonsoft.Json for internal use)
- Support for all Kannada characters and combining marks

## Accuracy

| Text Type | Accuracy |
|-----------|----------|
| Simple words | 95%+ |
| Complex text | 85%+ |
| Round-trip conversion | 80%+ |

## License

MIT License - See LICENSE file for details

## Support

- Report issues on GitHub: [kagapa-blr/KannadaAsciiUnicodeSDK](https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK)
- Join KAGAPA community for Kannada software development
