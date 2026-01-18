using System;
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

        public static KannadaConverter Instance => _instance.Value;

        private KannadaConverter()
        {
            var (mapping, brokenCases, vattaksharagalu, asciiArkavattu, dependentVowels, ignoreList) =
                KannadaMappingLoader.LoadMappings();

            _converter = new KannadaAsciiConverter(
                mapping,
                brokenCases,
                vattaksharagalu,
                asciiArkavattu,
                dependentVowels,
                ignoreList
            );
        }

        public string ConvertAsciiToUnicode(string asciiText)
        {
            if (asciiText == null)
                throw new ArgumentNullException(nameof(asciiText));

            return string.IsNullOrEmpty(asciiText) ? string.Empty : _converter.Convert(asciiText);
        }

        public string ConvertUnicodeToAscii(string unicodeText)
        {
            if (unicodeText == null)
                throw new ArgumentNullException(nameof(unicodeText));

            throw new NotImplementedException("Unicode to ASCII not yet implemented.");
        }

        public string Convert(string text, KannadaAsciiFormat format)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));

            return format switch
            {
                KannadaAsciiFormat.Nudi => ConvertAsciiToUnicode(text),
                KannadaAsciiFormat.Baraha => ConvertAsciiToUnicode(text),
                _ => text
            };
        }
    }
}