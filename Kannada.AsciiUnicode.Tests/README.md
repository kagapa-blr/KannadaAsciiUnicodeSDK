# Unit Tests

Comprehensive test suite for the Kannada ASCII to Unicode converter, ensuring conversion accuracy and reliability.

## Overview

The test project contains 18+ test cases covering:

- ASCII to Unicode conversion accuracy
- Unicode to ASCII conversion accuracy
- Bidirectional round-trip stability
- Singleton pattern behavior
- Custom mapping functionality
- Edge cases and special characters
- Consonant cluster handling with vattaksharagalu
- Consonant cluster handling with arkavattu

## Running Tests

### Run All Tests

```bash
dotnet test
```

Expected output:
```
Test summary: total: 18, failed: 0, succeeded: 18, skipped: 0
```

### Run Specific Test

```bash
dotnet test --filter "ConvertAsciiToUnicode"
```

### Run with Verbose Output

```bash
dotnet test --verbosity detailed
```

## Test Cases

### ASCII to Unicode Conversion

Tests basic and complex ASCII sequences:

- Simple consonants: `PÀ` → `ಕ`
- Consonant clusters: `gÁåAPï` → `ರ‍್ಯಾಂಕ್`
- Vowel combinations: `PÉ` → `ಕೆ`
- Complex conjuncts with proper ZWJ handling

### Unicode to ASCII Conversion

Tests reverse conversion:

- Simple consonants: `ಕ` → `PÀ`
- Complex sequences with proper reconstruction

### Round-Trip Conversion

Ensures bidirectional stability:

```
Original Unicode → ASCII → Unicode = Original
ಕನ್ನಡ → (ASCII) → ಕನ್ನಡ
```

### Zero-Width Joiner (ZWJ) Insertion

Tests correct ZWJ insertion for consonant clusters:

- Vattaksharagalu with vowels: Includes ZWJ for proper rendering
- Arkavattu without vowels: No ZWJ required
- Proper ligature prevention across text renderers

## Adding New Tests

### Add ASCII to Unicode Test Case

Edit `Core/KannadaConverterTests.cs`:

```csharp
public static readonly TheoryData<string, string> AsciiToUnicodeCases = new()
{
    { "PÀ", "ಕ" },
    { "your_ascii_sequence", "expected_unicode_result" },
};
```

### Example: Testing Consonant Clusters

```csharp
[Theory]
[InlineData("gÁåAPï", "ರ‍್ಯಾಂಕ್")]  // With ZWJ
public void ConvertAsciiToUnicode_Consonant_Cluster_With_Vattakshara(
    string ascii, 
    string expected)
{
    var result = _converter.ConvertAsciiToUnicode(ascii);
    Assert.Equal(expected, result);
}
```

## Debugging Failed Tests

### Inspect Unicode Codepoints

```csharp
var result = converter.ConvertAsciiToUnicode("gÁåAPï");
foreach (var c in result)
{
    Console.WriteLine($"U+{(int)c:X4}: {c}");
}
```

### Common Issues

**Missing ZWJ (U+200D):**
```
Expected: "ರ‍್ಯಾಂಕ್"  (with ZWJ)
Actual:   "ರ್ಯಾಂಕ್"   (without ZWJ)
```

Issue is typically with vattaksharagalu consonant handling.

**Incorrect Character:**
```
Expected: "ಕ"
Actual:   "ಸ"
```

Check mapping in `NudiBarahaMapping.json` under the `mapping` section.

## Test Structure

```
Kannada.AsciiUnicode.Tests/
├── Core/
│   └── KannadaConverterTests.cs    # Main test class with 18+ test cases
├── Usings.cs
└── Kannada.AsciiUnicode.Tests.csproj
```

## Contributing Tests

When adding new features or fixing bugs:

1. Add corresponding test cases to validate the change
2. Ensure all existing tests still pass
3. Run `dotnet test` before committing
4. Aim for high code coverage (90%+)

See [DEVELOPER_GUIDE.md](../DEVELOPER_GUIDE.md) for more information on contributing.

## Resources

- XUnit documentation: https://xunit.net/
- For contribution guidelines: See [DEVELOPER_GUIDE.md](../DEVELOPER_GUIDE.md)
- For SDK details: See [Kannada.AsciiUnicode/README.md](../Kannada.AsciiUnicode/README.md)
