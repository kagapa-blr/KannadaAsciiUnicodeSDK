# Contributing to Kannada ASCII to Unicode Converter

Thank you for your interest in contributing to this project. We welcome contributions from developers at all levels.

## Getting Started

### Prerequisites

- .NET 8.0 or later (or .NET Framework 4.7+)
- Git
- A text editor or IDE (Visual Studio, VS Code, Rider)

### Setup

1. Clone the repository:

```bash
git clone https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK.git
cd KannadaAsciiUnicode
```

1. Build the solution:

```bash
dotnet build
```

1. Run tests to verify setup:

```bash
dotnet test
```

## Areas for Contribution

### 1. Mapping Improvements

Add or refine ASCII to Unicode mappings for:

- Edge cases in existing characters
- Rare or archaic Kannada characters
- Special conjunctions and ligatures
- Domain-specific terminology

**Location:** `Kannada.AsciiUnicode/Resources/NudiBarahaMapping.json`

**How to contribute:**

1. Edit `NudiBarahaMapping.json` to add new mappings
2. Add your new mapping entries to the appropriate JSON sections
3. Add corresponding test cases in `Kannada.AsciiUnicode.Tests/Core/KannadaConverterTests.cs`
4. Test with `dotnet test`

### 2. Bug Fixes

Report and fix issues related to:

- Incorrect character conversion
- Missing Zero-Width Joiner (ZWJ) handling
- Consonant cluster rendering
- Edge cases in round-trip conversion

**Process:**

1. Create an issue describing the bug
2. Include test cases that demonstrate the issue
3. Submit a pull request with the fix
4. Ensure all tests pass

### 3. Test Coverage

Add test cases for:

- Specific character sequences
- Complex consonant clusters
- Custom mapping scenarios
- Edge cases and boundary conditions

**Location:** `Kannada.AsciiUnicode.Tests/Core/KannadaConverterTests.cs`

**Example:**

```csharp
public static readonly TheoryData<string, string> AsciiToUnicodeCases = new()
{
    { "PÀ", "ಕ" },
    { "your_ascii_sequence", "your_unicode_result" },
};
```

### 4. Documentation

Improve or expand documentation for:

- API usage examples
- Integration guides
- Performance optimization tips
- Troubleshooting common issues

**Locations:**

- `README.md` - Main overview
- `Kannada.AsciiUnicode/README.md` - SDK documentation
- Code comments and XML documentation

### 5. Performance Optimization

Identify and implement improvements for:

- Conversion speed
- Memory usage
- Algorithm efficiency
- Batch processing capabilities

**Requirements:**

- Include performance benchmarks before and after
- Ensure all tests pass
- Document the optimization approach

### 6. Examples and Samples

Create or enhance examples showing:

- Console application usage
- Batch file conversion
- Custom mapping integration
- Advanced use cases

## Development Workflow

### 1. Create a Feature Branch

```bash
git checkout -b feature/your-feature-name
```

### 2. Make Your Changes

Follow these guidelines:

- **Code Style:** Use standard C# conventions (PascalCase for public members, camelCase for private)
- **Comments:** Document complex logic clearly
- **Single Responsibility:** Keep methods focused on one task
- **Error Handling:** Use appropriate exception types
- **Backward Compatibility:** Preserve existing single-argument call patterns when adding new optional parameters, and update the documentation to reflect the supported overloads

### 3. Add or Update Tests

For each feature or fix, include tests:

```bash
dotnet test
```

Ensure all tests pass before submitting.

### 4. Commit Your Changes

Use clear, descriptive commit messages:

```bash
git commit -m "Fix: ZWJ insertion for vattakshara consonants

- Handle vowel-bearing consonants followed by consonant clusters
- Ensure proper ligature prevention across text renderers
- Add test cases for common vattakshara combinations"
```

### 5. Push and Create Pull Request

```bash
git push origin feature/your-feature-name
```

Create a pull request on GitHub with:

- Clear description of changes
- References to any related issues
- Test results showing all tests pass

## Code Standards

### Naming Conventions

- Classes: `PascalCase` (e.g., `KannadaConverter`)
- Public Methods: `PascalCase` (e.g., `ConvertAsciiToUnicode`)
- Private Members: `_camelCaseWithUnderscore` (e.g., `_mapping`)
- Constants: `UPPER_CASE` (e.g., `UTF8_ENCODING`)

### Documentation

Include XML documentation for public APIs:

```csharp
/// <summary>
/// Converts ASCII text to Kannada Unicode.
/// </summary>
/// <param name="asciiText">The ASCII text to convert</param>
/// <returns>The corresponding Unicode text</returns>
/// <exception cref="ArgumentNullException">Thrown when asciiText is null</exception>
public string ConvertAsciiToUnicode(string asciiText)
{
    // implementation
}
```

### Testing Requirements

- Minimum 80% code coverage for new features
- All tests must pass before merge
- Include edge case tests
- Test both positive and negative scenarios

## Mapping Format

### ASCII to Unicode Mapping

Structure in `NudiBarahaMapping.json`:

```json
{
  "mapping": {
    "PÀ": "ಕ",
    "gÀä": "ರಿ",
    "your_sequence": "unicode_character"
  }
}
```

### Adding Special Cases

For vattaksharagalu or complex rules:

```json
{
  "vattaksharagalu": {
    "å": "ಯ",
    "your_modifier": "unicode_base"
  }
}
```

## Testing Your Changes

### Run All Tests

```bash
dotnet test
```

### Run Specific Test

```bash
dotnet test --filter "TestMethodName"
```

### Run Test App

```bash
cd KannadaAsciiUnicode.TestApp
dotnet run
```

### Check Test Coverage

```bash
dotnet test /p:CollectCoverage=true /p:CoverageFormat=opencover
```

## Debugging Tips

### Visual Studio Code

1. Install C# extension
2. Set breakpoints in code
3. Press F5 to start debugging
4. Use Debug Console for inspection

### Visual Studio

1. Set breakpoints by clicking margin
2. Press F5 to start debugging
3. Use Watch window to inspect variables
4. Use Immediate window for evaluation

### Console Logging

```csharp
var result = converter.ConvertAsciiToUnicode("PÀ");
Console.WriteLine($"Result: {result}");
foreach (var c in result)
{
    Console.WriteLine($"  U+{(int)c:X4}: {c}");
}
```

## Pull Request Process

1. Ensure your branch is up to date with main
2. Run `dotnet test` and verify all tests pass
3. Update documentation if needed
4. Create pull request with descriptive title and description
5. Address review comments
6. Maintainers will merge when approved

## Review Criteria

Pull requests are reviewed for:

- Code quality and standards compliance
- Test coverage and passing tests
- Documentation updates
- Performance implications
- Backwards compatibility

## Questions or Issues?

- Open an issue on GitHub for bugs
- Use discussions for questions
- Check existing issues before creating new ones
- Provide detailed reproduction steps for bugs

## Code of Conduct

We are committed to providing a welcoming environment. Please:

- Be respectful to all contributors
- Provide constructive feedback
- Focus on code, not personalities
- Help newer contributors learn

## License

All contributions are made under the MIT License. By submitting a pull request, you agree that your contributions will be licensed under its terms.

---

Thank you for contributing to the Kannada ASCII to Unicode Converter project!
