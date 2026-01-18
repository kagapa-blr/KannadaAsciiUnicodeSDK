using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kannada.AsciiUnicode.Converters;

/// <summary>
/// Represents a broken case with value and conditional mapping.
/// </summary>
public class BrokenCase
{
    public string Value { get; set; } = "";
    public Dictionary<string, string> CaseMapping { get; set; } = new();
}

/// <summary>
/// Converts Kannada ASCII text (Nudi/Baraha format) to Unicode Kannada.
/// Algorithm based on the reference JavaScript implementation.
/// </summary>
public class NudiBarahaConverter
{
    private readonly Dictionary<string, string> _mapping;
    private readonly Dictionary<string, BrokenCase> _brokenCases;
    private readonly HashSet<string> _dependentVowels;
    private readonly Dictionary<string, string> _vattaksharagalu;
    private readonly Dictionary<string, string> _asciiArkavattu;
    private readonly HashSet<string> _ignoreList;

    public NudiBarahaConverter(
        Dictionary<string, string> mapping,
        Dictionary<string, BrokenCase> brokenCases,
        HashSet<string> dependentVowels,
        Dictionary<string, string> vattaksharagalu,
        Dictionary<string, string> asciiArkavattu,
        HashSet<string> ignoreList)
    {
        _mapping = mapping;
        _brokenCases = brokenCases;
        _dependentVowels = dependentVowels;
        _vattaksharagalu = vattaksharagalu;
        _asciiArkavattu = asciiArkavattu;
        _ignoreList = ignoreList;
    }

    /// <summary>
    /// Convert ASCII Kannada text to Unicode.
    /// </summary>
    public string AsciiToUnicode(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Split by spaces and process each word
        var words = text.Split(' ');
        var processedWords = words.Select(ProcessWord).ToList();
        return string.Join(" ", processedWords);
    }

    private string ProcessWord(string word)
    {
        var result = new List<string>();
        int i = 0;

        while (i < word.Length)
        {
            // Check if character should be ignored
            if (_ignoreList.Contains(word[i].ToString()))
            {
                i++;
                continue;
            }

            // Check if this is a pure ASCII letter or digit (A-Z, a-z, 0-9, @, ., etc)
            // These should not be converted, pass through as-is
            char ch = word[i];
            if ((ch >= 'A' && ch <= 'Z') || (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') ||
                ch == '@' || ch == '.' || ch == '-' || ch == '_')
            {
                result.Add(ch.ToString());
                i++;
                continue;
            }

            // Try to find mapping for extended ASCII Kannada codes
            var (charsConsumed, mapped) = FindMapping(result, word, i);

            result = mapped;
            i += (1 + charsConsumed);
        }

        return string.Concat(result);
    }

    private (int, List<string>) FindMapping(List<string> current, string word, int pos)
    {
        // Maximum combination length to try is 4 characters
        // But we need to ensure we don't go beyond word length
        int maxLen = Math.Min(3, word.Length - pos - 1); // 3 means we try up to 4 chars total

        // Try progressively shorter combinations (from longest to shortest)
        for (int len = maxLen; len >= 0; len--)
        {
            if (pos + len + 1 > word.Length)
                continue; // Skip if this would go out of bounds

            string substring = word.Substring(pos, len + 1);

            if (_mapping.ContainsKey(substring))
            {
                // Found direct mapping

                // Add ZWJ if previous char ends with halant (्)
                if (current.Count > 0 && current[current.Count - 1].EndsWith('\u0CCD'.ToString()))
                {
                    current.Add("\u200D"); // ZWJ
                }

                current.Add(_mapping[substring]);
                return (len, current);
            }
        }

        // If no direct mapping found, try special cases
        string singleChar = word[pos].ToString();

        if (_asciiArkavattu.ContainsKey(singleChar))
        {
            // Process arkavattu
            current = ProcessArkavattu(current, singleChar);
        }
        else if (_vattaksharagalu.ContainsKey(singleChar))
        {
            // Process vattakshara
            current = ProcessVattakshara(current, singleChar);
        }
        else if (_brokenCases.ContainsKey(singleChar))
        {
            // Process broken cases
            current = ProcessBrokenCases(current, singleChar);
        }
        else
        {
            // No mapping found, pass through
            current.Add(singleChar);
        }

        return (0, current);
    }

    private List<string> ProcessVattakshara(List<string> current, string character)
    {
        // Get last and second-last characters
        string lastChar = current.Count > 0 ? current[current.Count - 1] : "";
        string secondLast = current.Count > 1 ? current[current.Count - 2] : "";

        string vattaksharaChar = _vattaksharagalu[character];

        if (_dependentVowels.Contains(lastChar))
        {
            // If last char is a dependent vowel, rearrange
            current[current.Count - 1] = "\u0CCD"; // Halant
            current.Add(vattaksharaChar);
            current.Add(lastChar);
        }
        else
        {
            // No dependent vowel, just append halant + consonant
            current.Add("\u0CCD");
            current.Add(vattaksharaChar);
        }

        return current;
    }

    private List<string> ProcessArkavattu(List<string> current, string character)
    {
        string lastChar = current.Count > 0 ? current[current.Count - 1] : "";
        string secondLast = current.Count > 1 ? current[current.Count - 2] : "";

        string arkavattuChar = _asciiArkavattu[character];

        if (_dependentVowels.Contains(lastChar))
        {
            current[current.Count - 2] = arkavattuChar;
            current[current.Count - 1] = "\u0CCD";
            current.Add(secondLast);
            current.Add(lastChar);
        }
        else
        {
            current[current.Count - 1] = arkavattuChar;
            current.Add("\u0CCD");
            current.Add(lastChar);
        }

        return current;
    }

    private List<string> ProcessBrokenCases(List<string> current, string character)
    {
        var brokenCase = _brokenCases[character];
        string lastChar = current.Count > 0 ? current[current.Count - 1] : "";

        // Check if we have a mapping for this combination
        if (brokenCase.CaseMapping.ContainsKey(lastChar))
        {
            current[current.Count - 1] = brokenCase.CaseMapping[lastChar];
        }
        else
        {
            // No mapping, use default value
            current.Add(brokenCase.Value);
        }

        return current;
    }
}
