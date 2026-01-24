using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Kannada.AsciiUnicode.Converters;

namespace Kannada.AsciiUnicode.Mappings
{
    public static class KannadaMappingLoader
    {
        private const string ResourceName =
            "Kannada.AsciiUnicode.Resources.NudiBarahaMapping.json";

        public static (
            Dictionary<string, string> mapping,
            Dictionary<string, KannadaAsciiConverter.BrokenCaseInfo> brokenCases,
            Dictionary<string, string> vattaksharagalu,
            Dictionary<string, string> asciiArkavattu,
            HashSet<string> dependentVowels,
            HashSet<string> ignoreList,
            Dictionary<string, string> reverseMapping)
        LoadMappings()
        {
            var json = ReadEmbeddedResource(ResourceName);
            var root = ParseJson(json);

            var mapping = LoadDictionary(root, "mapping");
            var reverseMapping = CreateReverseMapping(mapping);

            return (
                mapping,
                LoadBrokenCases(root),
                LoadDictionary(root, "vattaksharagalu"),
                LoadDictionary(root, "asciiArkavattu"),
                LoadHashSet(root, "dependentVowels"),
                LoadHashSet(root, "ignoreList"),
                reverseMapping
            );
        }

        // -------------------- Helpers --------------------

        private static string ReadEmbeddedResource(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                var available = string.Join(", ", assembly.GetManifestResourceNames());
                throw new FileNotFoundException(
                    $"Embedded resource '{resourceName}' not found.\nAvailable resources:\n{available}"
                );
            }

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static JObject ParseJson(string json)
        {
            try
            {
                return JObject.Parse(json);
            }
            catch (JsonException ex)
            {
                throw new InvalidDataException(
                    "Failed to parse NudiBarahaMapping.json. Invalid JSON format.",
                    ex
                );
            }
        }

        private static Dictionary<string, string> LoadDictionary(
            JObject root,
            string propertyName)
        {
            var token = root[propertyName];
            if (token == null)
                throw new InvalidDataException($"Missing required JSON section '{propertyName}'.");

            return token.ToObject<Dictionary<string, string>>()
                   ?? throw new InvalidDataException(
                       $"Section '{propertyName}' is not a valid string dictionary."
                   );
        }

        private static HashSet<string> LoadHashSet(
            JObject root,
            string propertyName)
        {
            var token = root[propertyName];
            if (token == null)
                throw new InvalidDataException($"Missing required JSON section '{propertyName}'.");

            var list = token.ToObject<List<string>>();
            if (list == null)
                throw new InvalidDataException(
                    $"Section '{propertyName}' is not a valid string array."
                );

            return new HashSet<string>(list);
        }

        private static Dictionary<string, KannadaAsciiConverter.BrokenCaseInfo>
            LoadBrokenCases(JObject root)
        {
            var token = root["brokenCases"];
            if (token == null)
                throw new InvalidDataException("Missing required JSON section 'brokenCases'.");

            var raw = token.ToObject<Dictionary<string, JObject>>();
            if (raw == null)
                throw new InvalidDataException("'brokenCases' is not a valid object.");

            var result = new Dictionary<string, KannadaAsciiConverter.BrokenCaseInfo>();

            foreach (var kvp in raw)
            {
                var key = kvp.Key;
                var obj = kvp.Value;

                if (obj == null)
                    throw new InvalidDataException($"Broken case '{key}' is null.");

                var value = obj["value"]?.ToString();
                if (string.IsNullOrEmpty(value))
                    throw new InvalidDataException(
                        $"Broken case '{key}' is missing required 'value'."
                    );

                var mappingToken = obj["mapping"];
                if (mappingToken == null)
                    throw new InvalidDataException(
                        $"Broken case '{key}' is missing required 'mapping'."
                    );

                var mapping = mappingToken.ToObject<Dictionary<string, string>>();
                if (mapping == null)
                    throw new InvalidDataException(
                        $"Broken case '{key}.mapping' is invalid."
                    );

                result[key] = new KannadaAsciiConverter.BrokenCaseInfo
                {
                    Value = value,
                    Mapping = mapping
                };
            }

            return result;
        }

        internal static Dictionary<string, string> CreateReverseMapping(
            Dictionary<string, string> forwardMapping)
        {
            var reverse = new Dictionary<string, string>();

            // Sort by key length descending to prioritize longer ASCII sequences
            var sortedMappings = forwardMapping
                .OrderByDescending(kvp => kvp.Key.Length)
                .ToList();

            foreach (var kvp in sortedMappings)
            {
                var asciiKey = kvp.Key;
                var unicodeValue = kvp.Value;

                // For ambiguous cases (multiple ASCII → same Unicode),
                // keep the longest/first ASCII sequence
                if (!reverse.ContainsKey(unicodeValue))
                {
                    reverse[unicodeValue] = asciiKey;
                }
            }

            return reverse;
        }




    }
}
