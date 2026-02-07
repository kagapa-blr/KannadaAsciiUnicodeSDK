using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Kannada.AsciiUnicode.Converters;

internal sealed class ConversionEngine
{
    // Immutable maps (thread-safe)
    private readonly IReadOnlyDictionary<string, string> _asciiToUnicode;
    private readonly IReadOnlyDictionary<string, string> _unicodeToAscii;

    // Buckets: length → dictionary (O(1) lookup)
    private readonly IReadOnlyDictionary<int, Dictionary<string, string>> _asciiBuckets;
    private readonly IReadOnlyDictionary<int, Dictionary<string, string>> _unicodeBuckets;
    private readonly int _maxAsciiKeyLen;
    private readonly int _maxUnicodeKeyLen;

    private readonly IReadOnlyDictionary<string, string> _vattaksharagalu;
    private readonly IReadOnlyDictionary<string, string> _asciiArkavattu;
    private readonly IReadOnlyList<(string from, string to)> _postFixups;

    private const char Halant = '\u0CCD';
    private const char ZWJ = '\u200D';
    private const char ZWNJ = '\u200C';

    // =========================
    // Static compiled regex (ONE TIME)
    // =========================
    private static readonly Regex RephaFixRegex =
        new(@"([ಕ-ಹ])(ಾ|ಿ|ೀ|ು|ೂ|ೃ|ೆ|ೇ|ೈ|ೊ|ೋ|ೌ)?ರ್",
            RegexOptions.Compiled);

    private static readonly Regex DuplicateHalantRegex =
        new(@"\u0CCD{2,}", RegexOptions.Compiled);

    private static readonly Regex DuplicateVowelRegex =
        new(@"([\u0CBE-\u0CCB])\1{2,}", RegexOptions.Compiled);

    private static readonly Regex TrailingHalantRegex =
        new(@"\u0CCD([\u0CBE-\u0CCC])(?=[\s\.\,\!\?\;\:]|$)",
            RegexOptions.Compiled);

    // =========================
    // Constructor
    // =========================
    public ConversionEngine(
        Dictionary<string, string> asciiToUnicode,
        Dictionary<string, string> unicodeToAscii,
        Dictionary<string, string>? vattaksharagalu = null,
        Dictionary<string, string>? asciiArkavattu = null,
        List<(string from, string to)>? postFixups = null)
    {
        _asciiToUnicode = asciiToUnicode;
        _unicodeToAscii = unicodeToAscii;

        (_asciiBuckets, _maxAsciiKeyLen) = BuildBuckets(asciiToUnicode);
        (_unicodeBuckets, _maxUnicodeKeyLen) = BuildBuckets(unicodeToAscii);

        _vattaksharagalu = vattaksharagalu ?? new Dictionary<string, string>();
        _asciiArkavattu = asciiArkavattu ?? new Dictionary<string, string>();
        _postFixups = postFixups ?? new List<(string, string)>();
    }

    // =========================
    // ASCII → Unicode
    // =========================
    public string AsciiToUnicode(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        text = PreNormalizeAscii(text);

        var sb = new StringBuilder(text.Length * 2);
        int i = 0;

        while (i < text.Length)
        {
            bool matched = false;

            for (int len = Math.Min(_maxAsciiKeyLen, text.Length - i); len > 0; len--)
            {
                if (_asciiBuckets.TryGetValue(len, out var bucket) &&
                    bucket.TryGetValue(text.Substring(i, len), out var mapped))
                {
                    sb.Append(mapped);
                    i += len;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                string ch = text[i].ToString();

                if (_asciiArkavattu.TryGetValue(ch, out var ra))
                {
                    sb.Append(ra).Append(Halant);
                }
                else if (_vattaksharagalu.TryGetValue(ch, out var vatta))
                {
                    sb.Append(Halant).Append(vatta);
                }
                else
                {
                    sb.Append(text[i]);
                }
                i++;
            }
        }

        var output = sb.ToString();
        output = NormalizeClusters(output);
        output = NormalizeRepha(output);

        return output.Normalize(NormalizationForm.FormC);
    }

    // =========================
    // Unicode → ASCII
    // =========================
    public string UnicodeToAscii(string text)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        text = text.Normalize(NormalizationForm.FormC)
                   .Replace(ZWJ.ToString(), "")
                   .Replace(ZWNJ.ToString(), "");

        return ReplaceUsingBuckets(text, _unicodeBuckets, _maxUnicodeKeyLen);
    }

    // =========================
    // Core replacement engine (FAST)
    // =========================
    private static string ReplaceUsingBuckets(
        string input,
        IReadOnlyDictionary<int, Dictionary<string, string>> buckets,
        int maxLen)
    {
        var sb = new StringBuilder(input.Length * 2);
        int i = 0;

        while (i < input.Length)
        {
            bool matched = false;

            for (int len = Math.Min(maxLen, input.Length - i); len > 0; len--)
            {
                if (buckets.TryGetValue(len, out var bucket) &&
                    bucket.TryGetValue(input.Substring(i, len), out var mapped))
                {
                    sb.Append(mapped);
                    i += len;
                    matched = true;
                    break;
                }
            }

            if (!matched)
            {
                sb.Append(input[i++]);
            }
        }

        return sb.ToString();
    }

    // =========================
    // Bucket builder
    // =========================
    private static (IReadOnlyDictionary<int, Dictionary<string, string>>, int)
        BuildBuckets(Dictionary<string, string> map)
    {
        var result = new Dictionary<int, Dictionary<string, string>>();
        int maxLen = 0;

        foreach (var kv in map)
        {
            int len = kv.Key.Length;
            maxLen = Math.Max(maxLen, len);

            if (!result.TryGetValue(len, out var dict))
            {
                dict = new Dictionary<string, string>(StringComparer.Ordinal);
                result[len] = dict;
            }

            dict[kv.Key] = kv.Value;
        }

        return (result, maxLen);
    }

    // =========================
    // Normalization
    // =========================
    private static string NormalizeClusters(string text)
    {
        text = DuplicateHalantRegex.Replace(text, Halant.ToString());
        text = DuplicateVowelRegex.Replace(text, "$1");
        text = TrailingHalantRegex.Replace(text, "$1");
        return text;
    }

    private static string NormalizeRepha(string text)
    {
        return RephaFixRegex.Replace(text,
            m => "ರ್" + m.Groups[1].Value + m.Groups[2].Value);
    }

    private static string PreNormalizeAscii(string text)
    {
        return text.Replace("\u00EF ", "\u00EF")
                   .Replace("\u00EF\t", "\u00EF");
    }
}
