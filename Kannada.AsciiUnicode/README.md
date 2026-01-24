# KannadaAsciiUnicodeSDK

High-performance Kannada ASCII/ANSI ↔ Unicode converter, developed and maintained by **KAGAPA**.

This library allows bidirectional conversion between legacy Kannada ASCII encodings (like Nudi/Baraha) and Unicode.

---

## Quick Start

### Installation

Clone or reference the `Kannada.AsciiUnicode` library in your project.

### Basic Usage

```csharp
using Kannada.AsciiUnicode.Converters;

var converter = KannadaConverter.Instance;

// ASCII → Unicode
string unicode = converter.ConvertAsciiToUnicode("PÀ£ÀßqÀ");
// Output: "ಕನ್ನಡ"

// Unicode → ASCII
string ascii = converter.ConvertUnicodeToAscii("ಕನ್ನಡ");
// Output: "PÀ£ï£ÀqÀ"
```

### Using Custom User Mappings

Developers can provide **custom ASCII → Unicode or Unicode → ASCII mappings**:

```csharp
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

var converter = KannadaConverter.CreateWithCustomMapping(
    userAsciiToUnicodeMapping: customAsciiToUnicode,
    userUnicodeToAsciiMapping: customUnicodeToAscii
);

string unicodeText = converter.ConvertAsciiToUnicode("wÃPÀëÚ PÀëÚ");
string asciiText = converter.ConvertUnicodeToAscii("ತೀಕ್ಷ್ಣ ಕ್ಷ್ಣ");
```

This enables developers to **extend the default mapping** for rare or custom words.

---

## Features

* ✅ Bidirectional conversion (ASCII ↔ Unicode)
* ✅ Handles consonant clusters and conjuncts
* ✅ Correct placement of vowel signs
* ✅ Supports custom user mappings
* ✅ Optimized for performance, zero external dependencies
* ✅ Robust handling of common conversion errors

---

## Public API

### KannadaConverter (Singleton)

```csharp
public class KannadaConverter : IAsciiUnicodeConverter
{
    public static KannadaConverter Instance { get; }

    public string ConvertAsciiToUnicode(string asciiText);

    public string ConvertUnicodeToAscii(string unicodeText);

    public string Convert(string text, KannadaAsciiFormat format);

    public static KannadaConverter CreateWithCustomMapping(
        Dictionary<string, string>? userAsciiToUnicodeMapping = null,
        Dictionary<string, string>? userUnicodeToAsciiMapping = null
    );
}
```

---

## Developer Contribution

KAGAPA encourages developers to contribute:

### Areas to Contribute

* 📝 Add more ASCII → Unicode mappings for rare characters
* ⚡ Optimize conversion performance
* 🧪 Add test cases for edge scenarios
* 📖 Improve documentation
* 🐛 Report bugs and suggest fixes

### How to Contribute

1. Fork the repository
2. Add/update mappings or modify conversion logic
3. Test thoroughly using `KannadaAsciiUnicode.TestApp`
4. Submit a pull request

---

## Testing & Example Usage

```bash
# Build the solution
dotnet build

# Run the test app
cd KannadaAsciiUnicode.TestApp
dotnet run

# Check conversion results in output/conversion_results.txt
```

---

## License

MIT License
Developed and maintained by **KAGAPA**

---

## Releases

Latest releases and NuGet packages are available at:
[https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK/releases](https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK/releases)
