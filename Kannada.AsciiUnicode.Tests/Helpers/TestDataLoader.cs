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
        /// Gets basic ASCII to Unicode test cases.
        /// </summary>
        public static List<TestCase> GetAsciiToUnicodeBasic()
        {
            var data = LoadJsonData();
            return ParseTestCases(data["asciiToUnicodeBasic"]);
        }

        /// <summary>
        /// Gets advanced ASCII to Unicode test cases (vattakshara, conjuncts, etc.).
        /// </summary>
        public static List<TestCase> GetAsciiToUnicodeAdvanced()
        {
            var data = LoadJsonData();
            return ParseTestCases(data["asciiToUnicodeAdvanced"]);
        }

        /// <summary>
        /// Gets real-world word conversion test cases.
        /// </summary>
        public static List<TestCase> GetAsciiToUnicodeRealWords()
        {
            var data = LoadJsonData();
            return ParseTestCases(data["asciiToUnicodeRealWords"]);
        }

        /// <summary>
        /// Gets all ASCII to Unicode test cases combined.
        /// </summary>
        public static List<TestCase> GetAllAsciiToUnicode()
        {
            var all = new List<TestCase>();
            all.AddRange(GetAsciiToUnicodeBasic());
            all.AddRange(GetAsciiToUnicodeAdvanced());
            all.AddRange(GetAsciiToUnicodeRealWords());
            return all;
        }

        /// <summary>
        /// Gets Unicode to ASCII test cases.
        /// </summary>
        public static List<TestCase> GetUnicodeToAscii()
        {
            var data = LoadJsonData();
            return ParseUnicodeToAsciiTestCases(data["unicodeToAscii"]);
        }

        /// <summary>
        /// Gets preprocessing duplicate collapse test cases.
        /// </summary>
        public static List<TestCase> GetPreprocessingDuplicateCollapse()
        {
            var data = LoadJsonData();
            return ParseTestCases(data["preprocessingDuplicateCollapse"]);
        }

        /// <summary>
        /// Gets preprocessing multi-word test cases.
        /// </summary>
        public static List<TestCase> GetPreprocessingMultipleWords()
        {
            var data = LoadJsonData();
            return ParseTestCases(data["preprocessingMultipleWords"]);
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
                    Unicode = item["unicode"]?.ToString() ?? string.Empty,
                    Description = item["description"]?.ToString() ?? string.Empty
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
                    Unicode = item["ascii"]?.ToString() ?? string.Empty,
                    Description = item["description"]?.ToString() ?? string.Empty
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
        public string Description { get; set; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrEmpty(Description)
                ? $"{Ascii} → {Unicode}"
                : $"{Ascii} → {Unicode} ({Description})";
        }
    }
}
