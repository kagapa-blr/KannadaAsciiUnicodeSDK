using System;
using System.Collections.Generic;
using Kannada.AsciiUnicode.Enums;
using Kannada.AsciiUnicode.Interfaces;
using Kannada.AsciiUnicode.Mappings;

namespace Kannada.AsciiUnicode.Converters
{
    public sealed class KannadaConverter : IAsciiUnicodeConverter
    {
        private static readonly Lazy<KannadaConverter> _instance =
            new(() => new KannadaConverter());

        private readonly KannadaAsciiConverter _converter;

        // Singleton instance
        public static KannadaConverter Instance => _instance.Value;

        /// <summary>
        /// Factory method to create a converter with optional user mappings.
        /// </summary>
        public static KannadaConverter CreateWithCustomMapping(
            Dictionary<string, string>? userAsciiToUnicodeMapping = null,
            Dictionary<string, string>? userUnicodeToAsciiMapping = null)
        {
            return new KannadaConverter(userAsciiToUnicodeMapping, userUnicodeToAsciiMapping);
        }

        // Private constructor, merges user mappings if provided
        private KannadaConverter(
            Dictionary<string, string>? userAsciiToUnicodeMapping = null,
            Dictionary<string, string>? userUnicodeToAsciiMapping = null)
        {
            // Load default mappings from JSON
            var (defaultMapping, brokenCases, vattaksharagalu, asciiArkavattu,
                 dependentVowels, ignoreList, reverseMapping) = KannadaMappingLoader.LoadMappings();

            // Merge ASCII→Unicode: user values override defaults
            if (userAsciiToUnicodeMapping != null)
            {
                foreach (var kvp in userAsciiToUnicodeMapping)
                    defaultMapping[kvp.Key] = kvp.Value;
            }

            // Merge Unicode→ASCII: user values override defaults
            if (userUnicodeToAsciiMapping != null)
            {
                foreach (var kvp in userUnicodeToAsciiMapping)
                    reverseMapping[kvp.Key] = kvp.Value;
            }

            // Initialize the internal converter
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

        // Convert ASCII text to Unicode
        public string ConvertAsciiToUnicode(string asciiText)
        {
            if (asciiText == null) throw new ArgumentNullException(nameof(asciiText));
            return string.IsNullOrEmpty(asciiText) ? string.Empty : _converter.Convert(asciiText);
        }

        // Convert Unicode text to ASCII
        public string ConvertUnicodeToAscii(string unicodeText)
        {
            if (unicodeText == null) throw new ArgumentNullException(nameof(unicodeText));
            return string.IsNullOrEmpty(unicodeText) ? string.Empty : _converter.ReverseConvert(unicodeText);
        }

        // Convert text based on specified ASCII format
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
    }
}
