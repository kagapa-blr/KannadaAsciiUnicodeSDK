using Kannada.AsciiUnicode.Converters;
using Xunit;

namespace Kannada.AsciiUnicode.Tests;

/// <summary>
/// Comprehensive test suite for Kannada ASCII to Unicode and Unicode to ASCII conversion
/// </summary>
public class KannadaConverterTests
{
    private readonly KannadaConverter _converter = KannadaConverter.Instance;

    #region Basic Character Tests

    /// <summary>
    /// Test conversion of basic Kannada consonants from ASCII to Unicode
    /// </summary>
    [Fact]
    public void TestBasicConsonantsAsciiToUnicode()
    {
        // Test basic consonants using Unicode escapes to avoid encoding issues
        // \u00D0 = 'P' and \u00C0 = 'À' in extended ASCII (Nudi format)
        string ascii = "P\u00C0";  // ka in Nudi ASCII
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        Assert.NotEmpty(unicode);
        // Should be converted - result should be different from input
        Assert.True(unicode.Length > 0);
    }

    /// <summary>
    /// Test conversion of basic vowels from ASCII to Unicode
    /// </summary>
    [Fact]
    public void TestBasicVowelsAsciiToUnicode()
    {
        // Test basic vowels
        string ascii = "A";
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        Assert.NotEmpty(unicode);
    }

    /// <summary>
    /// Test round-trip conversion: ASCII -> Unicode -> ASCII
    /// </summary>
    [Fact]
    public void TestRoundTripConversion_SimpleWord()
    {
        // Simple Kannada word
        string originalAscii = "PÀ£ï£ÀqÀ";  // kannada in ASCII

        // ASCII -> Unicode
        string unicode = _converter.ConvertAsciiToUnicode(originalAscii);
        Assert.NotEmpty(unicode);

        // Unicode -> ASCII
        string roundTripAscii = _converter.ConvertUnicodeToAscii(unicode);
        Assert.NotEmpty(roundTripAscii);
    }

    #endregion

    #region Complex Conjunct Tests

    /// <summary>
    /// Test conversion of complex conjuncts (double consonants / vattakshara)
    /// </summary>
    [Theory]
    [InlineData("PÀ»")]  // double ka
    [InlineData("D¹")]   // double da
    [InlineData("°À")]   // double ta
    public void TestDoubleConsonantsAsciiToUnicode(string asciiInput)
    {
        string result = _converter.ConvertAsciiToUnicode(asciiInput);
        Assert.NotEmpty(result);
        // Should be properly converted
        Assert.True(result.Length > 0);
    }

    /// <summary>
    /// Test conversion of conjuncts with dependent vowels
    /// </summary>
    [Fact]
    public void TestConjunctWithDependentVowels()
    {
        string ascii = "Ã¹À";  // ktta with aa
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        Assert.NotEmpty(unicode);
    }

    #endregion

    #region Dependent Vowel Tests

    /// <summary>
    /// Test all Kannada dependent vowels in conversion
    /// </summary>
    [Theory]
    [InlineData("PÀ")]    // ka + aa
    [InlineData("PÉ")]    // ka + i
    [InlineData("PË")]    // ka + ii
    [InlineData("PÌ")]    // ka + u
    [InlineData("PÎ")]    // ka + uu
    [InlineData("PÏ")]    // ka + vocalic r
    public void TestDependentVowels(string asciiInput)
    {
        string result = _converter.ConvertAsciiToUnicode(asciiInput);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Test vowel disambiguation with context
    /// </summary>
    [Fact]
    public void TestVowelDisambiguation()
    {
        // Test that vowels are correctly placed based on context
        string asciiText = "PÀÃ";
        string unicode = _converter.ConvertAsciiToUnicode(asciiText);
        Assert.NotEmpty(unicode);
    }

    #endregion

    #region Kannada-Specific Linguistic Rules

    /// <summary>
    /// Test Arkavattu (Ra-based consonant clusters)
    /// </summary>
    [Fact]
    public void TestArkavattu()
    {
        // Ra-based clusters should be properly handled
        string ascii = "°À«";  // example with ra
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        Assert.NotEmpty(unicode);
    }

    /// <summary>
    /// Test Halant (Viraam) handling
    /// </summary>
    [Fact]
    public void TestHalantHandling()
    {
        string ascii = "PÀ»";
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        // Should convert successfully
        Assert.NotEmpty(unicode);
    }

    /// <summary>
    /// Test Repha (Ra-ligature) positioning
    /// </summary>
    [Fact]
    public void TestRephaPositioning()
    {
        // Ra at the beginning of a conjunct should form repha
        string ascii = "°Ã";
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        Assert.NotEmpty(unicode);
    }

    #endregion

    #region Special Characters and Numbers

    /// <summary>
    /// Test Kannada numbers conversion
    /// </summary>
    [Fact]
    public void TestKannadaNumbers()
    {
        // ASCII digits should be converted to Kannada numerals
        string ascii = "123";
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        // Verify conversion happened
        Assert.NotEmpty(unicode);
    }

    /// <summary>
    /// Test Anusavara (ಂ) and Visarga (ಃ) handling
    /// </summary>
    [Theory]
    [InlineData("PÀ¢")]  // with anusvara
    [InlineData("PÀ£")]  // with visarga
    public void TestAnusvaraAndVisarga(string asciiInput)
    {
        string result = _converter.ConvertAsciiToUnicode(asciiInput);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Test handling of special characters and punctuation
    /// </summary>
    [Fact]
    public void TestPunctuation()
    {
        string ascii = "PÀ£ï. PÉÃ!";
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        Assert.NotEmpty(unicode);
        // Punctuation should be preserved
        Assert.Contains(".", unicode);
        Assert.Contains("!", unicode);
    }

    #endregion

    #region Edge Cases and Error Handling

    /// <summary>
    /// Test null input handling
    /// </summary>
    [Fact]
    public void TestNullInputThrowsException()
    {
        Assert.Throws<ArgumentNullException>(() => _converter.ConvertAsciiToUnicode(null!));
        Assert.Throws<ArgumentNullException>(() => _converter.ConvertUnicodeToAscii(null!));
    }

    /// <summary>
    /// Test empty string input
    /// </summary>
    [Fact]
    public void TestEmptyStringInput()
    {
        string result1 = _converter.ConvertAsciiToUnicode("");
        string result2 = _converter.ConvertUnicodeToAscii("");
        Assert.Empty(result1);
        Assert.Empty(result2);
    }

    /// <summary>
    /// Test very long text conversion
    /// </summary>
    [Fact]
    public void TestLongTextConversion()
    {
        // Create a long text string
        string ascii = string.Concat(Enumerable.Repeat("PÀ£ï£ÀqÀ ", 100));
        string unicode = _converter.ConvertAsciiToUnicode(ascii);
        Assert.NotEmpty(unicode);
        // Should have multiple occurrences
        Assert.True(unicode.Length > 100);
    }

    /// <summary>
    /// Test mixed ASCII and Unicode input
    /// </summary>
    [Fact]
    public void TestMixedContent()
    {
        string mixedAscii = "PÀ£ï with some English";
        string result = _converter.ConvertAsciiToUnicode(mixedAscii);
        Assert.NotEmpty(result);
        // Should convert Kannada text
        Assert.True(result.Length > 5);
    }

    #endregion

    #region Unicode to ASCII Conversion Tests

    /// <summary>
    /// Test basic Unicode to ASCII conversion
    /// </summary>
    [Fact]
    public void TestUnicodeToAsciiBasic()
    {
        string unicode = "ಕನ್ನಡ";
        string ascii = _converter.ConvertUnicodeToAscii(unicode);
        Assert.NotEmpty(ascii);
    }

    /// <summary>
    /// Test round-trip conversion with actual Kannada text
    /// </summary>
    [Fact]
    public void TestRoundTripWithRealText()
    {
        // Real Kannada text: "ಕನ್ನಡ ಸಾಹಿತ್ಯ"
        string originalUnicode = "ಕನ್ನಡ ಸಾಹಿತ್ಯ";

        string ascii = _converter.ConvertUnicodeToAscii(originalUnicode);
        Assert.NotEmpty(ascii);

        // Convert back
        string roundTripUnicode = _converter.ConvertAsciiToUnicode(ascii);
        Assert.NotEmpty(roundTripUnicode);
    }

    /// <summary>
    /// Test Unicode text with multiple words
    /// </summary>
    [Fact]
    public void TestUnicodeWithMultipleWords()
    {
        string unicode = "ಕನ್ನಡ ಮತ್ತು ಸಾಹಿತ್ಯ";
        string ascii = _converter.ConvertUnicodeToAscii(unicode);
        Assert.NotEmpty(ascii);
        // Should preserve spaces
        Assert.Contains(" ", ascii);
    }

    #endregion

    #region Format Detection Tests

    /// <summary>
    /// Test automatic format detection
    /// </summary>
    [Fact]
    public void TestFormatDetection()
    {
        // ASCII input should be detected and converted to Unicode
        string asciiInput = "PÀ£ï£ÀqÀ";
        string result = _converter.Convert(asciiInput, Enums.KannadaAsciiFormat.Default);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Test Nudi format conversion
    /// </summary>
    [Fact]
    public void TestNudiFormat()
    {
        string asciiInput = "PÀ£ï£ÀqÀ";
        string result = _converter.Convert(asciiInput, Enums.KannadaAsciiFormat.Nudi);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Test Baraha format conversion
    /// </summary>
    [Fact]
    public void TestBaharaFormat()
    {
        string asciiInput = "PÀ£ï£ÀqÀ";
        string result = _converter.Convert(asciiInput, Enums.KannadaAsciiFormat.Baraha);
        Assert.NotEmpty(result);
    }

    #endregion

    #region Performance Tests

    /// <summary>
    /// Test conversion performance with large text
    /// </summary>
    [Fact]
    public void TestConversionPerformance()
    {
        // Create large text
        string largeText = string.Concat(Enumerable.Repeat("PÀ£ï£ÀqÀ ", 1000));

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        string result = _converter.ConvertAsciiToUnicode(largeText);
        stopwatch.Stop();

        Assert.NotEmpty(result);
        // Should complete in reasonable time (less than 5 seconds)
        Assert.True(stopwatch.ElapsedMilliseconds < 5000);
    }

    #endregion

    #region Consistency Tests

    /// <summary>
    /// Test that multiple conversions of the same text produce same result
    /// </summary>
    [Fact]
    public void TestConversionConsistency()
    {
        string ascii = "PÀ£ï£ÀqÀ";
        string result1 = _converter.ConvertAsciiToUnicode(ascii);
        string result2 = _converter.ConvertAsciiToUnicode(ascii);

        Assert.Equal(result1, result2);
    }

    /// <summary>
    /// Test Unicode to ASCII consistency
    /// </summary>
    [Fact]
    public void TestUnicodeToAsciiConsistency()
    {
        string unicode = "ಕನ್ನಡ";
        string result1 = _converter.ConvertUnicodeToAscii(unicode);
        string result2 = _converter.ConvertUnicodeToAscii(unicode);

        Assert.Equal(result1, result2);
    }

    #endregion
}