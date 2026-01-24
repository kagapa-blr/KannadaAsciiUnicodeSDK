# KannadaAsciiUnicodeSDK

High-performance Kannada ASCII/ANSI ↔ Unicode conversion toolkit, developed and maintained by **Kannada Ganaka Parishat (KAGAPA)**.

This repository hosts the **KannadaAsciiUnicode SDK**, a robust and optimized solution for converting legacy Kannada ASCII encodings (such as **Nudi** and **Baraha**) to modern **Unicode**, and vice versa.

---

## 📦 Repository Structure

This solution contains the following projects:

- **Kannada.AsciiUnicode**  
  Core SDK library published as a NuGet package.

- **Kannada.AsciiUnicode.Tests**  
  Unit tests validating conversion accuracy and edge cases.

- **KannadaAsciiUnicode.TestApp**  
  Console application demonstrating real-world usage.

---

## 🚀 Kannada.AsciiUnicode SDK

The **Kannada.AsciiUnicode** project is the primary SDK and includes:

- Bidirectional conversion (ASCII ↔ Unicode)
- Correct handling of Kannada conjuncts and vowel signs
- Optimized conversion logic
- Support for user-defined custom mappings

### 📘 SDK Documentation

All SDK-specific documentation, including installation, usage examples, public API, and custom mapping support, is maintained here:

Kannada.AsciiUnicode/README.md

Please refer to that document for detailed developer guidance.

---

## 📦 NuGet Package

The SDK is available on NuGet as:

KannadaAsciiUnicodeSDK

Install via the .NET CLI:

dotnet add package KannadaAsciiUnicodeSDK

Package links:

- NuGet: https://www.nuget.org/packages/KannadaAsciiUnicodeSDK
- Releases: https://github.com/kagapa-blr/KannadaAsciiUnicodeSDK/releases

---

## 🧪 Build and Test

To build the solution:

dotnet build

To run the sample test application:

cd KannadaAsciiUnicode.TestApp  
dotnet run

---

## 🤝 Contributions

Contributions are welcome and encouraged by KAGAPA.

You can contribute by:

- Adding new ASCII ↔ Unicode mappings
- Improving performance
- Adding test coverage
- Enhancing documentation

Please follow the contribution guidelines described in:

Kannada.AsciiUnicode/README.md

---

## 📄 License

MIT License

Developed and maintained by **Kannada Ganaka Parishat (KAGAPA)**

---

## 🌐 About KAGAPA

Kannada Ganaka Parishat (KAGAPA) is dedicated to promoting Kannada computing, language tools, and open-source software that support the Kannada language ecosystem.
