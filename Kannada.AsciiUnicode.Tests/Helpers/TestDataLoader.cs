using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Reflection;

namespace Kannada.AsciiUnicode.Tests.Helpers
{
    /// <summary>
    /// Loads test case data from TestCases.json resource file.
    /// Provides organized access to test cases across different categories.
    /// </summary>
    public static class TestDataLoader
    {
        private const string ResourceName = "Kannada.AsciiUnicode.Tests.TestCases.json";
        private static JObject? _cachedData;

        /// <summary>
        /// Loads test data from the JSON resource file.
        /// </summary>
        private static JObject LoadJsonData()
        {
            if (_cachedData != null)
                return _cachedData;

            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(ResourceName)
                ?? throw new FileNotFoundException($"Test data file '{ResourceName}' not found.");

            using var reader = new StreamReader(stream);
            var json = reader.ReadToEnd();
            _cachedData = JObject.Parse(json);
            return _cachedData;
        }

        /// <summary>
        /// Gets a named section of test cases from the JSON file.
        /// Supports the flattened schema used by the tests.
        /// </summary>
        public static List<TestCase> GetSection(string sectionName)
        {
            var data = LoadJsonData();

            if (string.Equals(sectionName, "asciiToUnicodeBasic", StringComparison.OrdinalIgnoreCase))
            {
                return ParseTestCases(data["asciiToUnicode"]);
            }

            return ParseTestCases(data[sectionName]);
        }

        /// <summary>
        /// Gets all ASCII to Unicode test cases.
        /// </summary>
        public static List<TestCase> GetAllAsciiToUnicode()
        {
            return GetSection("asciiToUnicode");
        }

        /// <summary>
        /// Gets Unicode to ASCII test cases.
        /// </summary>
        public static List<TestCase> GetUnicodeToAscii()
        {
            return ParseUnicodeToAsciiTestCases(LoadJsonData()["unicodeToAscii"]);
        }

        /// <summary>
        /// Gets preprocessing duplicate-collapse cases.
        /// </summary>
        public static List<TestCase> GetPreprocessingDuplicateCollapse()
        {
            return new List<TestCase>
            {
                new() { Ascii = "PPÀÀ", Unicode = "ಕ" },
                new() { Ascii = "PÀÀ", Unicode = "ಕ" },
                new() { Ascii = "PÀ", Unicode = "ಕ" },
                new() { Ascii = "PÉÉ", Unicode = "ಕೆ" },
                new() { Ascii = "gÉÉå", Unicode = "ರ‍್ಯೆ" },
                new() { Ascii = "gÀåå", Unicode = "ರ‍್ಯ" },
                new() { Ascii = "gÁåå", Unicode = "ರ‍್ಯಾ" }
            };
        }

        /// <summary>
        /// Gets preprocessing multi-word spacing cases.
        /// </summary>
        public static List<TestCase> GetPreprocessingMultipleWords()
        {
            return new List<TestCase>
            {
                new() { Ascii = "PÀÀ gÀå", Unicode = "ಕ ರ‍್ಯ" },
                new() { Ascii = "PPÀÀ zzÀä", Unicode = "ಕ ದ್ಮ" }
            };
        }

        private static List<TestCase> ParseTestCases(JToken? token)
        {
            if (token == null)
                return new List<TestCase>();

            var list = new List<TestCase>();
            foreach (var item in token.Children<JObject>())
            {
                list.Add(new TestCase
                {
                    Ascii = item["ascii"]?.ToString() ?? string.Empty,
                    Unicode = item["unicode"]?.ToString() ?? string.Empty
                });
            }
            return list;
        }

        /// <summary>
        /// Parses Unicode to ASCII test cases, swapping the properties since the JSON structure
        /// has "unicode" as input and "ascii" as expected output.
        /// </summary>
        private static List<TestCase> ParseUnicodeToAsciiTestCases(JToken? token)
        {
            if (token == null)
                return new List<TestCase>();

            var list = new List<TestCase>();
            foreach (var item in token.Children<JObject>())
            {
                // Swap properties: "unicode" from JSON becomes Ascii (the input),
                // and "ascii" from JSON becomes Unicode (the expected output)
                list.Add(new TestCase
                {
                    Ascii = item["unicode"]?.ToString() ?? string.Empty,
                    Unicode = item["ascii"]?.ToString() ?? string.Empty
                });
            }
            return list;
        }
    }

    /// <summary>
    /// Represents a single test case with ASCII and Unicode values.
    /// </summary>
    public class TestCase
    {
        public string Ascii { get; set; } = string.Empty;
        public string Unicode { get; set; } = string.Empty;
        public override string ToString()
        {
            return $"{Ascii} → {Unicode}";
        }
    }
}
