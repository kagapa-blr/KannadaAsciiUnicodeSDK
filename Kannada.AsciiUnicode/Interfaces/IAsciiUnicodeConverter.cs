using Kannada.AsciiUnicode.Enums;

namespace Kannada.AsciiUnicode.Interfaces;

/// <summary>
/// Main interface for Kannada ASCII ↔ Unicode conversion
/// </summary>
public interface IAsciiUnicodeConverter
{
    /// <summary>
    /// Converts ASCII Kannada text (Nudi/Baraha) to Unicode
    /// </summary>
    /// <param name="asciiText">Input ASCII encoded Kannada text</param>
    /// <param name="convertToEnglishDigit">When true, converts digits to English numerals; otherwise preserves Kannada digits by default.</param>
    /// <returns>Unicode Kannada text</returns>
    string ConvertAsciiToUnicode(string asciiText, bool convertToEnglishDigit = false);

    /// <summary>
    /// Converts Unicode Kannada text to ASCII (Nudi/Baraha)
    /// </summary>
    /// <param name="unicodeText">Input Unicode Kannada text</param>
    /// <param name="convertToEnglishDigit">When true, converts Kannada numerals to English digits.</param>
    /// <returns>ASCII encoded Kannada text</returns>
    string ConvertUnicodeToAscii(string unicodeText, bool convertToEnglishDigit = false);

    /// <summary>
    /// Converts text based on specified format
    /// </summary>
    /// <param name="text">Input text</param>
    /// <param name="format">Conversion direction/format</param>
    /// <param name="convertToEnglishDigit">When true, converts digits to English numerals; otherwise preserves Kannada digits by default.</param>
    /// <returns>Converted text</returns>
    string Convert(string text, KannadaAsciiFormat format, bool convertToEnglishDigit = false);
}
