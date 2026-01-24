# Kannada ASCII ↔ Unicode Converter SDK

High-performance **Kannada ASCII/ANSI ↔ Unicode converter** following **KAGAPA style**. Supports accurate handling of **consonant clusters, vowel signs, repha, vattaksharagalu, arkavattu**, and **custom mappings**.

---

## Quick Start

### Installation

Clone the repository or reference the **Kannada.AsciiUnicode** library in your project.

```bash
git clone https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK.git
cd KannadaAsciiUnicodeSDK
```

Add a project reference in your .NET solution:

```xml
<PackageReference Include="KannadaAsciiUnicodeSDK" Version="x.y.z" />
```

> **Note:** Version `x.y.z` will be determined automatically from [GitHub Releases](https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK/releases).

---

### Basic Usage

```csharp
using Kannada.AsciiUnicode.Converters;

var converter = KannadaConverter.Instance;

// ASCII → Unicode
string unicode = converter.ConvertAsciiToUnicode("PÀ£ÀßqÀ");
// Output: "ಕನ್ನಡ"

// Unicode → ASCII
string ascii = converter.ConvertUnicodeToAscii("ಕನ್ನಡ");
// Output: "PÀ£ï£ÀqÀ" (may vary depending on normalization)
```

---

### File Processing Example

```csharp
var converter = KannadaConverter.Instance;

string asciiContent = File.ReadAllText("input.txt", Encoding.UTF8);
string unicodeContent = converter.ConvertAsciiToUnicode(asciiContent);
File.WriteAllText("output.txt", unicodeContent, Encoding.UTF8);
```

---

## Public API

| Method                                            | Description                                                         |
| ------------------------------------------------- | ------------------------------------------------------------------- |
| `ConvertAsciiToUnicode(string asciiText)`         | Converts ASCII (Nudi/Baraha) text to Unicode.                       |
| `ConvertUnicodeToAscii(string unicodeText)`       | Converts Unicode text back to ASCII.                                |
| `Convert(string text, KannadaAsciiFormat format)` | Converts with explicit format (ASCII → Unicode or Unicode → ASCII). |
| `Instance`                                        | Access singleton instance for conversion.                           |

---

## Custom Mappings

Extend or override mappings with **custom JSON resources**:

* Location: `Kannada.AsciiUnicode/Resources/CustomMappings.json`
* Supports additional ASCII → Unicode rules.
* Loaded automatically at runtime.

```json
{
  "aa": "ಆ",
  "sh": "ಶ",
  "k~": "ಕ್ಷ"
}
```

> Custom mappings are applied after standard mappings but before post-fixups for flexibility.

---

## Features

* ✅ Bidirectional conversion (ASCII ↔ Unicode)
* ✅ Handles consonant clusters and conjuncts
* ✅ Correct vowel sign placement
* ✅ Supports KAGAPA style
* ✅ Custom mapping support
* ✅ Zero external dependencies

---

## Project Structure

```
Kannada.AsciiUnicode/
├── Converters/
│   ├── KannadaConverter.cs        ← Public API (Singleton)
│   └── KannadaAsciiConverter.cs   ← Core engine
├── Mappings/
│   ├── KannadaMappingLoader.cs    ← Loads JSON mappings
│   ├── NudiBarahaMapping.json     ← Standard ASCII→Unicode mappings
│   └── CustomMappings.json        ← Optional user mappings
├── Resources/                     ← Embedded JSON resources
└── Interfaces/
KannadaAsciiUnicode.TestApp/
└── Program.cs                      ← Example usage & tests
```

---

## Contributing

### Prerequisites

* Visual Studio 2022 / VS Code
* .NET SDK (supports .NET Standard 2.0+)

### Getting Started

```bash
git clone https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK.git
cd KannadaAsciiUnicodeSDK
```

```bash
dotnet build
```

Run the test application:

```bash
cd KannadaAsciiUnicode.TestApp
dotnet run
```

> Output results are saved to `output/results.txt`.

### How to Contribute

#### Adding New Mappings

1. Edit `Resources/NudiBarahaMapping.json` or `Resources/CustomMappings.json`
2. Add ASCII → Unicode entries
3. Run `dotnet run` in `TestApp` to verify correctness
4. Submit a Pull Request

#### Improving Conversion Logic

1. Modify `KannadaAsciiConverter.cs`
2. Add test cases in `TestApp/Program.cs`
3. Run and verify conversions
4. Document your changes

#### Reporting Issues

* Provide input text (ASCII/Unicode)
* Include expected vs actual output
* Attach test cases

---

## Features & Accuracy

| Feature               | Status | Accuracy |
| --------------------- | ------ | -------- |
| Basic ASCII→Unicode   | ✅      | 95%+     |
| Consonant Clusters    | ✅      | 85%+     |
| Vowel Signs           | ✅      | 90%+     |
| Complex/Extended Text | ✅      | 70%+     |

> Accuracy improves with **custom mappings** and KAGAPA-style post-processing.

---

## Build & Test

```bash
# Build full solution
dotnet build

# Run test application
cd KannadaAsciiUnicode.TestApp
dotnet run

# View results
cat output/results.txt
```

---

## License

[MIT License](LICENSE)

---

## Contact & Support

* GitHub Issues: [Create an Issue](https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK/issues)
* Questions & Contributions: Open Pull Requests
* Maintained by **Kannada Ganaka Parishat (KAGAPA)**

---

## Releases

All published NuGet packages and versioned releases are available at:

* [GitHub Releases](https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK/releases)
* [NuGet.org](https://www.nuget.org/packages/KannadaAsciiUnicodeSDK)

> Versioning is automatically handled via CI/CD. GitHub tags match the NuGet package version.
