using System;
using System.Collections.Generic;
using System.Linq;
using Kannada.AsciiUnicode.Enums;
using Kannada.AsciiUnicode.Interfaces;
using Kannada.AsciiUnicode.Mappings;

namespace Kannada.AsciiUnicode.Converters
{
    /// <summary>
    /// Simple Kannada ASCII (Nudi/Baraha) ↔ Unicode Converter
    /// </summary>
    public sealed class KannadaConverter : IAsciiUnicodeConverter
    {
        private static readonly Lazy<KannadaConverter> _instance =
            new(() => new KannadaConverter());

        private readonly KannadaAsciiConverter _converter;

        public static KannadaConverter Instance => _instance.Value;

        /// <summary>
        /// Creates a converter with optional custom ASCII→Unicode mappings.
        /// </summary>
        public static KannadaConverter CreateWithCustomMapping(Dictionary<string, string>? customMapping = null)
        {
            return new KannadaConverter(customMapping);
        }

        private KannadaConverter(Dictionary<string, string>? customMapping = null)
        {
            // Load default mappings
            var (defaultMapping, brokenCases, vattaksharagalu, asciiArkavattu,
                 dependentVowels, ignoreList, reverseMapping) = KannadaMappingLoader.LoadMappings();

            // Merge custom mappings if provided
            if (customMapping != null && customMapping.Count > 0)
            {
                foreach (var kvp in customMapping)
                    defaultMapping[kvp.Key] = kvp.Value;

                // Rebuild reverse mapping
                reverseMapping = CreateReverseMapping(defaultMapping);
            }

            // Initialize converter
            _converter = new KannadaAsciiConverter(
                defaultMapping,
                brokenCases,
                vattaksharagalu,
                asciiArkavattu,
                dependentVowels,
                ignoreList,
                reverseMapping
            );
        }

        public string ConvertAsciiToUnicode(string asciiText)
        {
            if (asciiText == null) throw new ArgumentNullException(nameof(asciiText));
            return string.IsNullOrEmpty(asciiText) ? string.Empty : _converter.Convert(asciiText);
        }

        public string ConvertUnicodeToAscii(string unicodeText)
        {
            if (unicodeText == null) throw new ArgumentNullException(nameof(unicodeText));
            return string.IsNullOrEmpty(unicodeText) ? string.Empty : _converter.ReverseConvert(unicodeText);
        }

        public string Convert(string text, KannadaAsciiFormat format)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            return format switch
            {
                KannadaAsciiFormat.Nudi => ConvertAsciiToUnicode(text),
                KannadaAsciiFormat.Baraha => ConvertAsciiToUnicode(text),
                _ => text
            };
        }

        private static Dictionary<string, string> CreateReverseMapping(Dictionary<string, string> forwardMapping)
        {
            var reverse = new Dictionary<string, string>();
            var sorted = forwardMapping.OrderByDescending(kvp => kvp.Key.Length).ToList();

            foreach (var kvp in sorted)
            {
                if (!reverse.ContainsKey(kvp.Value))
                    reverse[kvp.Value] = kvp.Key;
            }

            return reverse;
        }
    }
}
