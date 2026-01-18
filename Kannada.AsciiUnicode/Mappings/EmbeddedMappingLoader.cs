using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kannada.AsciiUnicode.Mappings
{
    internal static class EmbeddedMappingLoader
    {
        private const string AsciiToUnicodeResource = "Kannada.AsciiUnicode.Resources.AsciiToUnicodeMapping.json";
        private const string UnicodeToAsciiResource = "Kannada.AsciiUnicode.Resources.UnicodeToAsciiMapping.json";

        public static Dictionary<string, string> LoadAsciiToUnicode() =>
            LoadMapping(AsciiToUnicodeResource);

        public static Dictionary<string, string> LoadUnicodeToAscii() =>
            LoadMapping(UnicodeToAsciiResource);

        public static Dictionary<string, string> LoadVattaksharagalu() =>
            LoadSection(AsciiToUnicodeResource, "vattaksharagalu");

        public static Dictionary<string, string> LoadAsciiArkavattu() =>
            LoadSection(AsciiToUnicodeResource, "asciiArkavattu");

        public static List<(string from, string to)> LoadPostFixups()
        {
            var list = new List<(string from, string to)>();
            var jObject = LoadJsonObject(AsciiToUnicodeResource);

            if (jObject["postFixups"] is JArray postFixups)
            {
                foreach (JObject fixup in postFixups)
                {
                    string from = JsonConvert.DeserializeObject<string>($"\"{fixup["from"]}\"") ?? string.Empty;
                    string to = JsonConvert.DeserializeObject<string>($"\"{fixup["to"]}\"") ?? string.Empty;

                    if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
                        list.Add((from, to));
                }
            }

            return list;
        }

        private static Dictionary<string, string> LoadMapping(string resourceName)
        {
            var mapping = new Dictionary<string, string>(StringComparer.Ordinal);
            var jObject = LoadJsonObject(resourceName);

            foreach (var section in new[] { "mapping", "numbersMapping" })
            {
                if (jObject[section] is JObject secObj)
                {
                    foreach (var prop in secObj.Properties())
                    {
                        string key = JsonConvert.DeserializeObject<string>($"\"{prop.Name}\"") ?? string.Empty;
                        string value = JsonConvert.DeserializeObject<string>($"\"{prop.Value}\"") ?? string.Empty;

                        if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                            mapping[key] = value;
                    }
                }
            }

            return mapping;
        }

        private static Dictionary<string, string> LoadSection(string resourceName, string sectionName)
        {
            var mapping = new Dictionary<string, string>(StringComparer.Ordinal);
            var jObject = LoadJsonObject(resourceName);

            if (jObject[sectionName] is JObject secObj)
            {
                foreach (var prop in secObj.Properties())
                {
                    string key = JsonConvert.DeserializeObject<string>($"\"{prop.Name}\"") ?? string.Empty;
                    string value = JsonConvert.DeserializeObject<string>($"\"{prop.Value}\"") ?? string.Empty;

                    if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
                        mapping[key] = value;
                }
            }

            return mapping;
        }

        private static JObject LoadJsonObject(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new FileNotFoundException($"Embedded resource not found: {resourceName}");

            using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            string json = reader.ReadToEnd();

            return JObject.Parse(json);
        }
    }
}
