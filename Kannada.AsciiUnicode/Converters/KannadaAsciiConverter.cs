using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

    public class BrokenCaseInfo
    {
        public string? Value { get; set; }
        public Dictionary<string, string>? Mapping { get; set; }
    }

    public KannadaAsciiConverter(
        Dictionary<string, string> mapping,
        Dictionary<string, BrokenCaseInfo> brokenCases,
        Dictionary<string, string> vattaksharagalu,
        Dictionary<string, string> asciiArkavattu,
        HashSet<string> dependentVowels,
        HashSet<string> ignoreList,
        Dictionary<string, string> reverseMapping)
    {
        _mapping = mapping;
        _brokenCases = brokenCases;
        _vattaksharagalu = vattaksharagalu;
        _asciiArkavattu = asciiArkavattu;
        _dependentVowels = dependentVowels;
        _ignoreList = ignoreList;
        _reverseMapping = reverseMapping;
    }

    public string Convert(string text)
    {
        var words = text.Split(' ');
        var processedWords = new List<string>();

        foreach (var word in words)
        {
            processedWords.Add(ProcessWord(word));
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
            i += (1 + charsToSkip);
        }

        return string.Concat(op);
    }

    private (int, List<string>) FindMapping(List<string> op, string txt, int currentPos)
    {
        int maxLen = 4;
        int remaining = txt.Length - currentPos;

        if (remaining < 5)
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
            // If last letter is dependent vowel
            letters[letters.Count - 1] = "\u0CCD"; // Halant
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
            letters[letters.Count - 1] = "\u0CCD";
            letters.Add(secondLast);
            letters.Add(lastLetter);
        }
        else
        {
            letters[letters.Count - 1] = _asciiArkavattu[t];
            letters.Add("\u0CCD");
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

        // Try matching longest sequences first (up to 4 Unicode characters)
        for (int len = 4; len >= 1; len--)
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
