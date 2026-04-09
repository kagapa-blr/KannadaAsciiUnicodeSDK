using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kannada.AsciiUnicode.Interfaces;

namespace Kannada.AsciiUnicode.Converters;

/// <summary>
/// Kannada ASCII to Unicode converter - exact port of JavaScript algorithm
/// </summary>
public class KannadaAsciiConverter
{
    private readonly Dictionary<string, string> _mapping;
    private readonly Dictionary<string, BrokenCaseInfo> _brokenCases;
    private readonly Dictionary<string, string> _vattaksharagalu;
    private readonly Dictionary<string, string> _asciiArkavattu;
    private readonly HashSet<string> _dependentVowels;
    private readonly HashSet<string> _ignoreList;
    private readonly Dictionary<string, string> _reverseMapping;
    private readonly int _maxSequenceLength;

    /// <summary>
    /// Creates a Kannada ASCII to Unicode converter.
    /// </summary>
    /// <param name="mapping">ASCII to Unicode character/sequence mappings</param>
    /// <param name="brokenCases">Special vowel transformation rules</param>
    /// <param name="vattaksharagalu">Consonant modifiers (handled with ZWJ for vowel-bearing bases)</param>
    /// <param name="asciiArkavattu">Subjoined consonants (no ZWJ required)</param>
    /// <param name="dependentVowels">Unicode characters that are vowel signs</param>
    /// <param name="ignoreList">Characters to skip during conversion</param>
    /// <param name="reverseMapping">Unicode to ASCII reverse mappings for bidirectional conversion</param>
    /// <param name="maxSequenceLength">Maximum ASCII sequence length to match (default 8).
    /// Adjust based on your longest mappings. Higher values may slightly impact performance.</param>
    public KannadaAsciiConverter(
        Dictionary<string, string> mapping,
        Dictionary<string, BrokenCaseInfo> brokenCases,
        Dictionary<string, string> vattaksharagalu,
        Dictionary<string, string> asciiArkavattu,
        HashSet<string> dependentVowels,
        HashSet<string> ignoreList,
        Dictionary<string, string> reverseMapping,
        int maxSequenceLength = 8)
    {
        _mapping = mapping;
        _brokenCases = brokenCases;
        _vattaksharagalu = vattaksharagalu;
        _asciiArkavattu = asciiArkavattu;
        _dependentVowels = dependentVowels;
        _ignoreList = ignoreList;
        _reverseMapping = reverseMapping;
        _maxSequenceLength = maxSequenceLength > 0 ? maxSequenceLength : 8;  // Validate: default to 8 if invalid
    }

    public string Convert(string text)
    {
        var words = text.Split(' ');
        var processedWords = new List<string>();

        foreach (var word in words)
        {
            // Preprocess: collapse duplicates and clean input
            var preprocessed = PreprocessAsciiInput(word);
            processedWords.Add(ProcessWord(preprocessed));
        }

        return string.Join(" ", processedWords);
    }

    public string ReverseConvert(string unicodeText)
    {
        var words = unicodeText.Split(' ');
        var processedWords = new List<string>();

        foreach (var word in words)
        {
            processedWords.Add(ReverseProcessWord(word));
        }

        return string.Join(" ", processedWords);
    }

    /// <summary>
    /// Preprocesses ASCII input to reduce conversion errors:
    /// - Collapses duplicate consecutive characters (ÀÀ → À, ÉÉ → É, etc.)
    /// - Removes internal spaces within words
    /// </summary>
    private string PreprocessAsciiInput(string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        // Remove internal spaces
        word = word.Replace(" ", "");

        // Collapse duplicate consecutive characters
        var result = new StringBuilder();
        char lastChar = '\0';

        foreach (char c in word)
        {
            // Only add if different from last character
            if (c != lastChar)
            {
                result.Append(c);
                lastChar = c;
            }
        }

        return result.ToString();
    }

    private string ProcessWord(string word)
    {
        var op = new List<string>();
        int i = 0;

        while (i < word.Length)
        {
            // Ignore certain characters
            if (_ignoreList.Contains(word[i].ToString()))
            {
                i++;
                continue;
            }

            var (charsToSkip, result) = FindMapping(op, word, i);
            op = result;

            // Lightweight fix: only check last 2 items for ordering issues
            if (op.Count >= 2)
            {
                op = QuickFixCharacterOrdering(op);
            }

            i += (1 + charsToSkip);
        }

        return string.Concat(op);
    }

    /// <summary>
    /// Quick lightweight fix: only checks the last 2 items for vowel+halant ordering.
    /// Runs on every FindMapping iteration for immediate correction without major slowdown.
    /// </summary>
    private List<string> QuickFixCharacterOrdering(List<string> op)
    {
        if (op.Count < 2)
            return op;

        int lastIdx = op.Count - 1;
        string current = op[lastIdx];
        string previous = op[lastIdx - 1];

        // Pattern 1: current IS a halant or ZWJ+halant, previous ends with vowel
        if ((current == "\u0CCD" || current == "\u200D\u0CCD"))
        {
            foreach (var vowel in _dependentVowels)
            {
                if (previous.EndsWith(vowel))
                {
                    // Found vowel before halant - reorder
                    string consonantPart = previous.Substring(0, previous.Length - vowel.Length);
                    op[lastIdx - 1] = consonantPart;
                    op[lastIdx] = current;
                    op.Add(vowel);
                    return op;
                }
            }
        }

        // Pattern 2: previous ends with vowel, current STARTS with halant
        // This handles cases like "ಪಿ" followed by "್ರ" -> need to become "ಪ್ರಿ"
        if (current.StartsWith("\u0CCD"))
        {
            foreach (var vowel in _dependentVowels)
            {
                if (previous.EndsWith(vowel))
                {
                    // Extract vowel from previous and halant from current
                    string consonantPart = previous.Substring(0, previous.Length - vowel.Length);
                    op[lastIdx - 1] = consonantPart;

                    // current starts with halant, keep it as is
                    op[lastIdx] = current;

                    // Insert vowel after current
                    op.Add(vowel);
                    return op;
                }
            }
        }

        return op;
    }


    private (int, List<string>) FindMapping(List<string> op, string txt, int currentPos)
    {
        int maxLen = _maxSequenceLength;
        int remaining = txt.Length - currentPos;

        if (remaining < _maxSequenceLength + 1)
        {
            maxLen = remaining - 1;
        }

        int n = 0;

        // Try from longest to shortest match
        for (int i = maxLen; i >= 0; i--)
        {
            int substrTill = currentPos + i + 1;

            if (substrTill > txt.Length)
                continue;

            string t = txt.Substring(currentPos, i + 1);

            if (_mapping.ContainsKey(t))
            {
                // Direct mapping found

                // Add ZWJ if previous ends with halant
                if (op.Count > 0)
                {
                    string lastChar = op[op.Count - 1];
                    if (lastChar.EndsWith('\u0CCD'.ToString())) // Halant
                    {
                        op.Add("\u200D"); // ZWJ
                    }
                }

                op.Add(_mapping[t]);
                n = i;
                return (n, op);
            }

            // If not last iteration, continue
            if (i > 0)
                continue;

            // No mapping found - try special cases
            var letters = op.Join("").ToList();
            string singleChar = txt[currentPos].ToString();

            if (_asciiArkavattu.ContainsKey(singleChar))
            {
                op = ProcessArkavattu(letters, singleChar);
            }
            else if (_vattaksharagalu.ContainsKey(singleChar))
            {
                op = ProcessVattakshara(letters, singleChar);
            }
            else if (_brokenCases.ContainsKey(singleChar))
            {
                op = ProcessBrokenCases(letters, singleChar);
            }
            else
            {
                op.Add(singleChar);
            }
        }

        return (0, op);
    }

    private List<string> ProcessVattakshara(List<string> letters, string t)
    {
        string lastLetter = letters.Count > 0 ? letters[letters.Count - 1] : "";
        string secondLast = letters.Count > 1 ? letters[letters.Count - 2] : "";

        if (_dependentVowels.Contains(lastLetter))
        {
            // If last letter is dependent vowel, replace with ZWJ + halant
            letters[letters.Count - 1] = "\u200D\u0CCD"; // ZWJ + Halant
            letters.Add(_vattaksharagalu[t]);
            letters.Add(lastLetter);
        }
        else
        {
            // No dependent vowel
            letters.Add("\u0CCD");
            letters.Add(_vattaksharagalu[t]);
        }

        return letters;
    }

    private List<string> ProcessArkavattu(List<string> letters, string t)
    {
        string lastLetter = letters.Count > 0 ? letters[letters.Count - 1] : "";
        string secondLast = letters.Count > 1 ? letters[letters.Count - 2] : "";

        if (_dependentVowels.Contains(lastLetter))
        {
            letters[letters.Count - 2] = _asciiArkavattu[t];
            letters[letters.Count - 1] = "\u0CCD"; // Halant only (no ZWJ)
            letters.Add(secondLast);
            letters.Add(lastLetter);
        }
        else
        {
            letters[letters.Count - 1] = _asciiArkavattu[t];
            letters.Add("\u0CCD"); // Halant only (no ZWJ)
            letters.Add(lastLetter);
        }

        return letters;
    }

    private List<string> ProcessBrokenCases(List<string> letters, string t)
    {
        string lastLetter = letters.Count > 0
            ? letters[letters.Count - 1]
            : string.Empty;

        if (!_brokenCases.TryGetValue(t, out BrokenCaseInfo brokenCase))
        {
            // t is non-null here
            letters.Add(t);
            return letters;
        }

        // Safely read nullable members
        Dictionary<string, string>? mapping = brokenCase.Mapping;
        string? value = brokenCase.Value;

        if (!string.IsNullOrEmpty(lastLetter) &&
            mapping != null &&
            mapping.TryGetValue(lastLetter, out string mapped))
        {
            // mapped is guaranteed non-null
            letters[letters.Count - 1] = mapped;
        }
        else if (value != null)
        {
            // Explicit null check satisfies the compiler
            letters.Add(value);
        }

        return letters;
    }

    private string ReverseProcessWord(string word)
    {
        var result = new StringBuilder();
        int i = 0;

        while (i < word.Length)
        {
            // Try to match longest Unicode sequences first (greedy approach)
            var charsMatched = 0;
            var match = FindReverseMapping(word, i, out charsMatched);

            if (!string.IsNullOrEmpty(match))
            {
                result.Append(match);
                i += charsMatched;
            }
            else
            {
                // No mapping found, keep the character as-is
                result.Append(word[i]);
                i++;
            }
        }

        return result.ToString();
    }

    private string FindReverseMapping(string unicodeText, int startPos, out int charsMatched)
    {
        charsMatched = 0;

        // Try matching longest Unicode sequences first (using configurable max length)
        for (int len = _maxSequenceLength; len >= 1; len--)
        {
            int endPos = startPos + len;
            if (endPos > unicodeText.Length)
                continue;

            string substring = unicodeText.Substring(startPos, len);

            if (_reverseMapping.TryGetValue(substring, out string? asciiValue))
            {
                charsMatched = len;
                return asciiValue;
            }
        }

        return string.Empty;
    }

}

public static class StringExtensions
{
    public static List<string> ToList(this string str)
    {
        return str.Select(c => c.ToString()).ToList();
    }

    public static string Join(this List<string> list, string separator = "")
    {
        return string.Join(separator, list);
    }
}
