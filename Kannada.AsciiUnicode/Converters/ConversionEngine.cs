using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Kannada.AsciiUnicode.Converters;

internal sealed class ConversionEngine
{
    private readonly Dictionary<string, string> _asciiToUnicode;
    private readonly Dictionary<string, string> _unicodeToAscii;

    private readonly IReadOnlyList<string>[] _asciiKeysByLen;
    private readonly IReadOnlyList<string>[] _unicodeKeysByLen;
    private readonly int _maxAsciiKeyLen;
    private readonly int _maxUnicodeKeyLen;

    // Additional JSON-based maps
    private readonly Dictionary<string, string> _vattaksharagalu;
    private readonly Dictionary<string, string> _asciiArkavattu;
    private readonly List<(string from, string to)> _postFixups;

    private readonly char _halant = '\u0CCD';
    private readonly char _zwj = '\u200D';
    private readonly char _zwnj = '\u200C';

    private readonly HashSet<char> _dependentVowels = new()
    {
        '\u0CBE','\u0CBF','\u0CC0','\u0CC1','\u0CC2','\u0CC3',
        '\u0CC6','\u0CC7','\u0CC8','\u0CCA','\u0CCB','\u0CCC',
        '\u0CCD','\u0C82','\u0C83'
    };

    public ConversionEngine(
        Dictionary<string, string> asciiToUnicode,
        Dictionary<string, string> unicodeToAscii,
        Dictionary<string, string>? vattaksharagalu = null,
        Dictionary<string, string>? asciiArkavattu = null,
        List<(string from, string to)>? postFixups = null)
    {
        _asciiToUnicode = asciiToUnicode;
        _unicodeToAscii = unicodeToAscii;

        (_asciiKeysByLen, _maxAsciiKeyLen) = BuildBuckets(asciiToUnicode);
        (_unicodeKeysByLen, _maxUnicodeKeyLen) = BuildBuckets(unicodeToAscii);

        _vattaksharagalu = vattaksharagalu ?? new();
        _asciiArkavattu = asciiArkavattu ?? new();
        _postFixups = postFixups ?? new();
    }

    // =========================
    // ASCII → UNICODE
    // =========================
    public string AsciiToUnicode(string asciiText)
    {
        if (string.IsNullOrEmpty(asciiText))
            return string.Empty;

        // 1️⃣ Pre-normalization: remove whitespace after halant
        asciiText = PreNormalizeAscii(asciiText);

        // 2️⃣ Longest token streaming mapping
        var mapped = ProcessStreamLongestToken(asciiText);

        // 3️⃣ Cluster normalization + post-fixups
        mapped = NormalizeKannadaClusters(mapped);

        // 3b️⃣ Intelligent vowel placement fixing
        mapped = FixVowelPlacementLogically(mapped);

        // 4️⃣ Repha normalization
        mapped = NormalizeKannadaRepha(mapped);

        return mapped.Normalize(NormalizationForm.FormC);
    }

    // =========================
    // UNICODE → ASCII
    // =========================
    public string UnicodeToAscii(string unicodeText)
    {
        if (string.IsNullOrEmpty(unicodeText))
            return string.Empty;

        unicodeText = unicodeText
            .Normalize(NormalizationForm.FormC)
            .Replace(_zwj.ToString(), "")
            .Replace(_zwnj.ToString(), "");

        unicodeText = Regex.Replace(
            unicodeText,
            @"\bರ\u0CCD([\u0C95-\u0CB9\u0CDE])",
            "$1\u0CCDರ",
            RegexOptions.Compiled);

        return ReplaceUsingBuckets(unicodeText, _unicodeToAscii, _unicodeKeysByLen, _maxUnicodeKeyLen);
    }

    // =========================
    // Longest-token streaming with enhanced vowel handling
    // =========================
    private string ProcessStreamLongestToken(string text)
    {
        var sb = new StringBuilder(text.Length * 2);
        int i = 0;
        int n = text.Length;

        while (i < n)
        {
            char ch = text[i];

            // Skip pure ASCII English characters (0-127) - don't convert them
            // When we encounter an ASCII letter/digit, consume the entire word
            if (ch < 128 && ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9')))
            {
                // Consume entire ASCII word/number
                while (i < n)
                {
                    char c = text[i];
                    if ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                    {
                        sb.Append(c);
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                continue;
            }

            // Handle ASCII punctuation and whitespace
            if (ch < 128 && (ch == ' ' || ch == '\t' || ch == '\n' || ch == '\r' ||
                ch == '.' || ch == ',' || ch == '!' || ch == '?' ||
                ch == ':' || ch == ';' || ch == '-' || ch == '/' ||
                ch == '@' || ch == '(' || ch == ')' || ch == '[' || ch == ']' ||
                ch == '{' || ch == '}' || ch == '"' || ch == '\'' || ch == '_' ||
                ch == '+' || ch == '=' || ch == '\\' || ch == '|' || ch == '<' || ch == '>' ||
                ch == '#' || ch == '%' || ch == '&' || ch == '*' || ch == '~' || ch == '`'))
            {
                sb.Append(ch);
                i++;
                continue;
            }

            // Check if next character is ASCII letter - if so, skip extended ASCII chars (corrupted data)
            if (ch >= 128 && i + 1 < n)
            {
                char nextChar = text[i + 1];
                if ((nextChar >= 'A' && nextChar <= 'Z') || (nextChar >= 'a' && nextChar <= 'z') ||
                    (nextChar >= '0' && nextChar <= '9'))
                {
                    // Extended ASCII followed by ASCII letter - likely corrupted data, pass through
                    sb.Append(ch);
                    i++;
                    continue;
                }
            }

            // Check if previous character was ASCII letter - if so, skip extended ASCII
            if (ch >= 128 && sb.Length > 0)
            {
                char lastChar = sb.ToString()[sb.Length - 1];
                if ((lastChar >= 'A' && lastChar <= 'Z') || (lastChar >= 'a' && lastChar <= 'z') ||
                    (lastChar >= '0' && lastChar <= '9'))
                {
                    // Extended ASCII preceded by ASCII letter - likely corrupted, pass through
                    sb.Append(ch);
                    i++;
                    continue;
                }
            }

            bool matched = false;

            // Try longest matches first (greedy algorithm) - for extended ASCII Kannada characters
            for (int len = Math.Min(_maxAsciiKeyLen, n - i); len > 0; len--)
            {
                foreach (var key in _asciiKeysByLen[len])
                {
                    if (string.CompareOrdinal(text, i, key, 0, len) == 0)
                    {
                        string mapped = _asciiToUnicode[key];

                        // Apply context-aware vowel processing if needed
                        if (len == 1 && (key[0] == 'Ã' || key[0] == 'Æ' || key[0] == 'Ê'))
                        {
                            // Single character that's a vowel modifier
                            mapped = GetContextAwareVowel(key[0], sb.ToString());
                        }

                        sb.Append(mapped);
                        i += len;
                        matched = true;
                        break;
                    }
                }
                if (matched) break;
            }

            if (!matched)
            {
                // Handle special ASCII characters with intelligent fallback
                if (_asciiArkavattu.TryGetValue(ch.ToString(), out var ra))
                {
                    // Arkavattu: ra-vowel variant
                    sb.Append(ra);
                    sb.Append(_halant);
                }
                else if (_vattaksharagalu.TryGetValue(ch.ToString(), out var vatta))
                {
                    // Vattaksharagalu: double consonant
                    sb.Append(_halant);
                    sb.Append(vatta);
                }
                else
                {
                    // Unmapped character - pass through
                    sb.Append(ch);
                }
                i++;
            }
        }

        return sb.ToString();
    }

    // =========================
    // Bucket engine (optimized)
    // =========================
    private static string ReplaceUsingBuckets(string input, Dictionary<string, string> mapping, IReadOnlyList<string>[] buckets, int maxKeyLen)
    {
        var sb = new StringBuilder(input.Length * 2);
        int i = 0;

        while (i < input.Length)
        {
            bool matched = false;
            for (int len = Math.Min(maxKeyLen, input.Length - i); len > 0; len--)
            {
                foreach (var key in buckets[len])
                {
                    if (string.CompareOrdinal(input, i, key, 0, len) == 0)
                    {
                        sb.Append(mapping[key]);
                        i += len;
                        matched = true;
                        break;
                    }
                }
                if (matched) break;
            }

            if (!matched)
            {
                sb.Append(input[i]);
                i++;
            }
        }

        return sb.ToString();
    }

    private static (IReadOnlyList<string>[], int) BuildBuckets(Dictionary<string, string> mapping)
    {
        int maxLen = mapping.Keys.Max(k => k.Length);
        var buckets = new List<string>[maxLen + 1];

        foreach (var key in mapping.Keys.OrderByDescending(k => k.Length).ThenBy(k => k, StringComparer.Ordinal))
        {
            int len = key.Length;
            buckets[len] ??= new List<string>();
            buckets[len].Add(key);
        }

        for (int i = 0; i < buckets.Length; i++)
            buckets[i] ??= new List<string>();

        return (buckets, maxLen);
    }

    // =========================
    // ASCII pre-normalization
    // =========================
    private string PreNormalizeAscii(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;
        return text.Replace($"{(char)0x00EF} ", "\u00EF").Replace($"{(char)0x00EF}\t", "\u00EF");
    }

    // =========================
    // Kannada cluster normalization
    // =========================
    private string NormalizeKannadaClusters(string text)
    {
        // Replace arkavattu
        foreach (var kv in _asciiArkavattu)
            text = text.Replace(kv.Key, kv.Value);

        // Vattaksharagalu
        foreach (var kv in _vattaksharagalu.OrderByDescending(k => k.Key.Length))
            text = text.Replace(kv.Key, kv.Value);

        // Post-fixups
        foreach (var fix in _postFixups)
            text = text.Replace(fix.from, fix.to);

        // Double consonants: ನ + ್ + ನ → ನ್ನ
        text = Regex.Replace(text, @"([\u0C95-\u0CB9\u0CDE])\u0CCD\1", "$1\u0CCD$1", RegexOptions.Compiled);

        // Apply Sanka-inspired regex fixes for common conversion issues
        text = ApplySankaRegexFixes(text);

        return text;
    }

    // =========================
    // Sanka-inspired regex fixes (9zx-sanka algorithm) - Safe subset
    // =========================
    private string ApplySankaRegexFixes(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Apply only the SAFEST regex fixes that don't cause over-transformations:

        // 1. Fix: Remove duplicate halants (always safe)
        //    Pattern: ್್ → ್
        text = Regex.Replace(
            text,
            @"\u0CCD{2,}",
            "\u0CCD",
            RegexOptions.Compiled
        );

        // 2. Fix: Consolidate triple+ duplicate vowels (safe - removes clear duplication errors)
        //    Pattern: ೆೆೆ → ೆ (only 3+ repetitions to be safe)
        text = Regex.Replace(
            text,
            @"([\u0CBE-\u0CCB])\1{2,}",
            "$1",
            RegexOptions.Compiled
        );

        // 3. Fix: Remove trailing halant with vowel at word end (safe - incomplete structure)
        //    Pattern: ್ೆ$ → ೆ (halant-vowel at end of word becomes just vowel)
        text = Regex.Replace(
            text,
            @"\u0CCD([\u0CBE-\u0CCC])(?=[\s\.\,\!\?\;\:]|$)",
            "$1",
            RegexOptions.Compiled
        );

        return text;
    }

    // =========================
    // Repha normalization
    // =========================
    private static string NormalizeKannadaRepha(string text)
    {
        return Regex.Replace(
            text,
            @"([ಕ-ಹ])(ಾ|ಿ|ೀ|ು|ೂ|ೃ|ೆ|ೇ|ೈ|ೊ|ೋ|ೌ)?ರ್",
            m => "ರ್" + m.Groups[1].Value + m.Groups[2].Value,
            RegexOptions.Compiled
        );
    }

    // =========================
    // Intelligent vowel placement fixing
    // =========================
    private string FixVowelPlacementLogically(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        // Enhanced context-aware vowel processing
        text = ProcessContextAwareBrokenCases(text);

        // Remove stray unmapped characters
        text = text.Replace("Ã", "");

        return text;
    }

    // =========================
    // Context-aware broken cases processing (9zx-sanka inspired)
    // =========================
    private string ProcessContextAwareBrokenCases(string text)
    {
        if (string.IsNullOrEmpty(text)) return text;

        var sb = new StringBuilder(text.Length);

        for (int i = 0; i < text.Length; i++)
        {
            char current = text[i];
            char prev = i > 0 ? text[i - 1] : '\0';
            char next = i < text.Length - 1 ? text[i + 1] : '\0';

            // Process broken cases based on previous character context
            if (current == 'ೀ' && i > 0)
            {
                // If previous is ಿ (i-vowel), keep ೀ (ii-vowel) - they form a unit
                // If previous is ೆ (e-vowel), convert to ೇ (ee-vowel)
                if (prev == '\u0CBF')  // ಿ (i-vowel)
                {
                    sb.Append(current);
                }
                else if (prev == '\u0CC6')  // ೆ (e-vowel)
                {
                    sb.Append('\u0CC7');  // Replace with ೇ (ee-vowel)
                }
                else if (prev == '\u0CCA')  // ೊ (o-vowel)
                {
                    sb.Append('\u0CCB');  // Replace with ೋ (oo-vowel)
                }
                else
                {
                    sb.Append(current);
                }
            }
            else if (current == 'ೂ' && i > 0)
            {
                // If previous is ೆ, could be ೊ instead
                if (prev == '\u0CC6')  // ೆ (e-vowel)
                {
                    sb.Append('\u0CCA');  // Replace with ೊ (o-vowel)
                }
                else
                {
                    sb.Append(current);  // Keep as ೂ (uu-vowel)
                }
            }
            else if (current == 'ೃ' && next == 'ೃ')
            {
                // Avoid duplicate ri vowels
                sb.Append(current);
                i++;  // Skip the next one
            }
            else
            {
                sb.Append(current);
            }
        }

        return sb.ToString();
    }

    // =========================
    // Enhanced vowel disambiguation with consonant context
    // =========================
    private string GetContextAwareVowel(char ambiguousChar, string precedingText)
    {
        if (precedingText.Length == 0) return ambiguousChar.ToString();

        char lastConsonant = '\0';

        // Find the last consonant in the preceding text
        for (int i = precedingText.Length - 1; i >= 0; i--)
        {
            char ch = precedingText[i];
            // Check if it's a Kannada consonant
            if ((ch >= '\u0C95' && ch <= '\u0CB9') || ch == '\u0CDE')
            {
                lastConsonant = ch;
                break;
            }
        }

        // Apply rules based on the last consonant
        if (ambiguousChar == 'Ã' && lastConsonant != '\0')
        {
            // Special handling for specific consonants
            switch (lastConsonant)
            {
                case 'ಕ':
                case 'ಖ':
                case 'ಗ':
                case 'ಘ':
                    // These consonants more often use ೀ (ii) vowel
                    return "ೀ";
                case 'ಸ':
                case 'ಶ':
                case 'ಷ':
                    // These also use ೀ more frequently
                    return "ೀ";
                default:
                    return "ೆ";  // Default to e-vowel for ambiguity
            }
        }

        return ambiguousChar.ToString();
    }
}
