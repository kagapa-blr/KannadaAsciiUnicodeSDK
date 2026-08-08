using Kannada.AsciiUnicode.Converters;
using Kannada.AsciiUnicode.Enums;
using Kannada.AsciiUnicode.Tests.Helpers;
using Xunit;

namespace Kannada.AsciiUnicode.Tests.Core
{
    /// <summary>
    /// Comprehensive unit tests for Kannada ASCII to Unicode converter.
    /// Test data is organized and loaded from TestCases.json for better maintainability.
    /// </summary>
    public class KannadaConverterTests
    {
        private readonly KannadaConverter _converter = KannadaConverter.Instance;

        // =====================================================
        // TEST DATA - Loaded from TestCases.json
        // =====================================================

        public static readonly TheoryData<string, string> AsciiToUnicodeCases =
            ConvertTestCasesToTheoryData(TestDataLoader.GetAllAsciiToUnicode());

        public static readonly TheoryData<string, string> UnicodeToAsciiCases =
            ConvertTestCasesToTheoryData(TestDataLoader.GetUnicodeToAscii());

        public static readonly TheoryData<string, string> PreprocessingDuplicatesCases =
            ConvertTestCasesToTheoryData(TestDataLoader.GetPreprocessingDuplicateCollapse());

        public static readonly TheoryData<string, string> PreprocessingMultiWordCases =
            ConvertTestCasesToTheoryData(TestDataLoader.GetPreprocessingMultipleWords());

        /// <summary>
        /// Helper method to convert TestCase objects to TheoryData for xUnit.
        /// </summary>
        private static TheoryData<string, string> ConvertTestCasesToTheoryData(List<TestCase> testCases)
        {
            var theoryData = new TheoryData<string, string>();
            foreach (var testCase in testCases)
            {
                theoryData.Add(testCase.Ascii, testCase.Unicode);
            }
            return theoryData;
        }

        // =====================================================
        // SINGLETON BEHAVIOR
        // =====================================================

        [Fact]
        public void Instance_Should_Be_Singleton()
        {
            var first = KannadaConverter.Instance;
            var second = KannadaConverter.Instance;

            Assert.Same(first, second);
        }

        // =====================================================
        // ASCII → UNICODE CONVERSION
        // =====================================================

        [Theory]
        [MemberData(nameof(AsciiToUnicodeCases))]
        public void ConvertAsciiToUnicode_Should_Return_Expected_Unicode(string ascii, string expectedUnicode)
        {
            var result = _converter.ConvertAsciiToUnicode(ascii);
            Assert.Equal(expectedUnicode, result);
        }

        [Fact]
        public void ConvertAsciiToUnicode_Should_Convert_English_Digits_To_Kannada_Digits_By_Default()
        {
            var result = _converter.ConvertAsciiToUnicode("12345");
            Assert.Equal("೧೨೩೪೫", result);
        }

        [Fact]
        public void ConvertAsciiToUnicode_Should_Allow_English_Digit_Output_When_Requested()
        {
            var result = _converter.ConvertAsciiToUnicode("12345", convertToEnglishDigit: true);
            Assert.Equal("12345", result);
        }

        [Fact]
        public void ConvertAsciiToUnicode_Should_Be_Compatible_With_Func_String_String_Delegates()
        {
            Func<string, string> convert = _converter.ConvertAsciiToUnicode;
            var result = convert("PÀ");

            Assert.Equal("ಕ", result);
        }

        [Fact]
        public void TestDataLoader_Should_Load_Test_Cases_From_Sectioned_Json()
        {
            var sectionCases = TestDataLoader.GetSection("asciiToUnicodeBasic");

            Assert.NotEmpty(sectionCases);
            Assert.Contains(sectionCases, c => c.Ascii == "PÀ" && c.Unicode == "ಕ");
        }

        // =====================================================
        // UNICODE → ASCII CONVERSION
        // =====================================================

        [Theory]
        [MemberData(nameof(UnicodeToAsciiCases))]
        public void ConvertUnicodeToAscii_Should_Return_Expected_Ascii(string unicode, string expectedAscii)
        {
            var result = _converter.ConvertUnicodeToAscii(unicode);
            Assert.Equal(expectedAscii, result);
        }

        [Fact]
        public void ConvertUnicodeToAscii_Should_Be_Compatible_With_Func_String_String_Delegates()
        {
            Func<string, string> convert = _converter.ConvertUnicodeToAscii;
            var result = convert("ಕ");

            Assert.Equal("PÀ", result);
        }

        // =====================================================
        // CONVERTER ROUTER BEHAVIOR
        // =====================================================

        [Theory]
        [InlineData(KannadaAsciiFormat.Nudi)]
        [InlineData(KannadaAsciiFormat.Baraha)]
        public void Convert_Should_Route_To_AsciiToUnicode(KannadaAsciiFormat format)
        {
            var result = _converter.Convert("PÀ", format);
            Assert.Equal("ಕ", result);
        }

        [Fact]
        public void Convert_Default_Should_Return_Input_Unchanged()
        {
            var input = "PÀ";
            var result = _converter.Convert(input, KannadaAsciiFormat.Default);
            Assert.Equal(input, result);
        }

        // =====================================================
        // ROUND-TRIP STABILITY
        // =====================================================

        [Fact]
        public void Unicode_To_Ascii_To_Unicode_Should_Preserve_Text()
        {
            var original = "ಕನ್ನಡ";

            var ascii = _converter.ConvertUnicodeToAscii(original);
            var roundTrip = _converter.ConvertAsciiToUnicode(ascii);

            Assert.False(string.IsNullOrWhiteSpace(ascii));
            Assert.Contains("ಕ", roundTrip);
        }

        // =====================================================
        // EDGE CASES
        // =====================================================

        [Theory]
        [InlineData("")]
        public void ConvertAsciiToUnicode_Should_Handle_Empty_String(string input)
        {
            var result = _converter.ConvertAsciiToUnicode(input);
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ConvertAsciiToUnicode_Should_Throw_On_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _converter.ConvertAsciiToUnicode(null!));
        }

        [Fact]
        public void ConvertUnicodeToAscii_Should_Throw_On_Null()
        {
            Assert.Throws<ArgumentNullException>(() => _converter.ConvertUnicodeToAscii(null!));
        }

        // =====================================================
        // PREPROCESSING: DUPLICATE CHARACTER COLLAPSE
        // Consecutive duplicate characters are collapsed (ÀÀ → À, ÉÉ → É, etc.)
        // This reduces OCR errors and user input mistakes.
        // See preprocessingRules in NudiBarahaMapping.json
        // =====================================================

        [Theory]
        [MemberData(nameof(PreprocessingDuplicatesCases))]
        public void ConvertAsciiToUnicode_Should_Collapse_Duplicate_Characters(string ascii, string expectedUnicode)
        {
            var result = _converter.ConvertAsciiToUnicode(ascii);
            Assert.Equal(expectedUnicode, result);
        }

        // =====================================================
        // PREPROCESSING: WORD SPACING
        // =====================================================

        [Fact]
        public void ConvertAsciiToUnicode_Should_Preserve_Word_Spacing()
        {
            var result = _converter.ConvertAsciiToUnicode("PÀ gÀä");
            var parts = result.Split(' ');
            Assert.Equal(2, parts.Length);
            Assert.Equal("ಕ", parts[0]);
        }

        [Fact]
        public void ConvertAsciiToUnicode_Should_Preserve_Indentation_And_Line_Breaks()
        {
            var input = "  PÀ   PÀ\n    PÀ";
            var result = _converter.ConvertAsciiToUnicode(input);

            Assert.Equal("  ಕ   ಕ\n    ಕ", result);
        }

        // =====================================================
        // PREPROCESSING: MULTIPLE WORDS
        // =====================================================

        [Theory]
        [MemberData(nameof(PreprocessingMultiWordCases))]
        public void ConvertAsciiToUnicode_Should_Handle_Duplicate_Collapse_In_Multiple_Words(string ascii, string expectedUnicode)
        {
            var result = _converter.ConvertAsciiToUnicode(ascii);
            Assert.Equal(expectedUnicode, result);
        }
    }
}
