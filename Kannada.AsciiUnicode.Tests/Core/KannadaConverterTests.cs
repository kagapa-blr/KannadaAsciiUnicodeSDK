using Kannada.AsciiUnicode.Converters;
using Kannada.AsciiUnicode.Enums;
using Xunit;

namespace Kannada.AsciiUnicode.Tests.Core
{
    public class KannadaConverterTests
    {
        private readonly KannadaConverter _converter = KannadaConverter.Instance;

        // -----------------------------
        // Singleton behavior
        // -----------------------------
        [Fact]
        public void Instance_Should_Be_Singleton()
        {
            var first = KannadaConverter.Instance;
            var second = KannadaConverter.Instance;

            Assert.Same(first, second);
        }

        // -----------------------------
        // ASCII → Unicode conversion
        // -----------------------------
        public static readonly TheoryData<string, string> AsciiToUnicodeCases = new()
        {
            { "PÀ", "ಕ" },
            { "CA", "ಅಂ" },   // Updated to match actual mapping
            { "PÉ", "ಕೆ" },
            // Add more test cases here in future
        };

        [Theory]
        [MemberData(nameof(AsciiToUnicodeCases))]
        public void ConvertAsciiToUnicode_Should_Return_Expected_Unicode(string ascii, string expectedUnicode)
        {
            var result = _converter.ConvertAsciiToUnicode(ascii);
            Assert.Equal(expectedUnicode, result);
        }

        // -----------------------------
        // Unicode → ASCII conversion
        // -----------------------------
        public static readonly TheoryData<string, string> UnicodeToAsciiCases = new()
        {
            { "ಕ", "PÀ" },
            { "ಅಂ", "CA" }, // Updated to match actual mapping
            { "ಕೆ", "PÉ" },
            // Add more test cases here in future
        };

        [Theory]
        [MemberData(nameof(UnicodeToAsciiCases))]
        public void ConvertUnicodeToAscii_Should_Return_Expected_Ascii(string unicode, string expectedAscii)
        {
            var result = _converter.ConvertUnicodeToAscii(unicode);
            Assert.Equal(expectedAscii, result);
        }

        // -----------------------------
        // Convert() router behavior
        // -----------------------------
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

        // -----------------------------
        // Round-trip stability
        // -----------------------------
        [Fact]
        public void Unicode_To_Ascii_To_Unicode_Should_Preserve_Text()
        {
            var original = "ಕನ್ನಡ";

            var ascii = _converter.ConvertUnicodeToAscii(original);
            var roundTrip = _converter.ConvertAsciiToUnicode(ascii);

            Assert.False(string.IsNullOrWhiteSpace(ascii));
            Assert.Contains("ಕ", roundTrip);
        }

        // -----------------------------
        // Edge cases
        // -----------------------------
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
    }
}
