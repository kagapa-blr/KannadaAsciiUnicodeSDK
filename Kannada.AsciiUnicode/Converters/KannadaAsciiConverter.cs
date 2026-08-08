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
    private readonly Dictionary<string, string> _collapseDuplicateCharacters;
    private readonly Dictionary<string, string> _removeInternalSpaces;
    private readonly Dictionary<string, string> _additionalMappings;
    private readonly HashSet<string> _mappingKeyPrefixes;
    private readonly int _maxMappingKeyLength;
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
    /// <param name="collapseDuplicateCharacters">Rule-based duplicate collapse patterns</param>
    /// <param name="removeInternalSpaces">Rule-based internal space removal patterns</param>
    /// <param name="reverseMapping">Unicode to ASCII reverse mappings for bidirectional conversion</param>
    /// <param name="additionalMappings">Optional extra ASCII-to-Unicode aliases used as generic fallbacks for missing core mappings</param>
    /// <param name="maxSequenceLength">Maximum ASCII sequence length to match (default 8).
    /// Adjust based on your longest mappings. Higher values may slightly impact performance.</param>
    public KannadaAsciiConverter(
        Dictionary<string, string> mapping,
        Dictionary<string, BrokenCaseInfo> brokenCases,
        Dictionary<string, string> vattaksharagalu,
        Dictionary<string, string> asciiArkavattu,
        HashSet<string> dependentVowels,
        HashSet<string> ignoreList,
        Dictionary<string, string> collapseDuplicateCharacters,
        Dictionary<string, string> removeInternalSpaces,
        Dictionary<string, string> reverseMapping,
        Dictionary<string, string>? additionalMappings = null,
        int maxSequenceLength = 8)
    {
        _mapping = mapping;
        _brokenCases = brokenCases;
        _vattaksharagalu = vattaksharagalu;
        _asciiArkavattu = asciiArkavattu;
        _dependentVowels = dependentVowels;
        _ignoreList = ignoreList;
        _collapseDuplicateCharacters = collapseDuplicateCharacters;
        _removeInternalSpaces = removeInternalSpaces;
        _additionalMappings = additionalMappings ?? new Dictionary<string, string>();
        (_mappingKeyPrefixes, _maxMappingKeyLength) = BuildMappingKeyPrefixes(mapping);
        _reverseMapping = reverseMapping;
        _maxSequenceLength = maxSequenceLength > 0 ? maxSequenceLength : 8;  // Validate: default to 8 if invalid
    }

    private static (HashSet<string> prefixes, int maxLength) BuildMappingKeyPrefixes(
        Dictionary<string, string> mapping)
    {
        var prefixes = new HashSet<string>();
        int maxLength = 0;

        foreach (var key in mapping.Keys)
        {
            maxLength = Math.Max(maxLength, key.Length);
            for (int length = 1; length <= key.Length; length++)
            {
                prefixes.Add(key.Substring(0, length));
            }
        }

        return (prefixes, maxLength);
    }

    private bool ShouldPreserveDuplicateSequence(string word, int currentPosition)
    {
        // Preserve duplicates when the second (or later) occurrence is the start
        // of a valid mapping sequence. Look forward from the duplicate position
        // to see if any mapping prefix begins there; if so, we should keep
        // the duplicate so the mapping algorithm can match the longer sequence.
        int duplicateStart = currentPosition + 1;

        if (duplicateStart >= word.Length)
            return false;

        // Backward-looking check: if any mapping prefix ends at the duplicate
        // position (i.e., the duplicate is required to form an existing mapping
        // that spans earlier characters), preserve duplicates.
        int startWindow = Math.Max(0, currentPosition - _maxMappingKeyLength + 1);
        int duplicateEnd = duplicateStart;

        for (int start = startWindow; start <= currentPosition; start++)
        {
            int length = duplicateEnd - start + 1;
            if (length <= 0 || length > _maxMappingKeyLength)
                continue;

            string segment = word.Substring(start, length);
            if (_mappingKeyPrefixes.Contains(segment))
                return true;
        }

        // Forward vowel heuristic: only preserve when the duplicated character
        // itself is a standalone mapping whose value ends with a dependent vowel.
        string dupChar = word[currentPosition].ToString();
        if (!_mapping.TryGetValue(dupChar, out string? dupMappingValue) || string.IsNullOrEmpty(dupMappingValue))
            return false;

        bool endsWithDependentVowel = _dependentVowels.Any(v => dupMappingValue.EndsWith(v));
        if (!endsWithDependentVowel)
            return false;

        int maxLookahead = Math.Min(_maxMappingKeyLength, word.Length - duplicateStart);

        for (int len = 1; len <= maxLookahead; len++)
        {
            string segment = word.Substring(duplicateStart, len);
            if (_mappingKeyPrefixes.Contains(segment))
                return true;
        }

        return false;
    }

    public string Convert(string text, bool convertToEnglishDigit = false)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        // Process the full input as one stream so mappings that intentionally
        // include spaces (for example, "gÀæ å") can be matched correctly.
        var preprocessed = PreprocessAsciiInput(text);
        return ProcessWord(preprocessed, convertToEnglishDigit);
    }

    public string ReverseConvert(string unicodeText, bool convertToEnglishDigit = false)
    {
        if (string.IsNullOrEmpty(unicodeText))
            return string.Empty;

        return ReverseProcessWord(unicodeText, convertToEnglishDigit);
    }

    /// <summary>
    /// Preprocesses ASCII input to reduce conversion errors based on rules defined in NudiBarahaMapping.json.
    /// Applies two transformations:
    /// 1. collapseDuplicateCharacters: Removes consecutive duplicate characters (e.g., ÀÀ becomes À, ÉÉ becomes É)
    /// 2. removeInternalSpaces: Eliminates spaces within words (e.g., P À becomes PÀ, g À å becomes gÀå)
    /// These preprocessing steps help handle OCR errors and spacing artifacts.
    /// </summary>
    private string PreprocessAsciiInput(string word)
    {
        if (string.IsNullOrEmpty(word))
            return word;

        if (_removeInternalSpaces.Count > 0)
        {
            foreach (var kvp in _removeInternalSpaces)
            {
                if (word.Contains(kvp.Key))
                {
                    word = word.Replace(kvp.Key, kvp.Value);
                }
            }
        }

        if (_collapseDuplicateCharacters.Count > 0)
        {
            foreach (var kvp in _collapseDuplicateCharacters)
            {
                if (word.Contains(kvp.Key))
                {
                    word = word.Replace(kvp.Key, kvp.Value);
                }
            }
        }

        var result = new StringBuilder();
        int position = 0;

        while (position < word.Length)
        {
            char currentChar = word[position];

            if (char.IsWhiteSpace(currentChar))
            {
                result.Append(currentChar);
                position++;
                continue;
            }

            if (position + 1 < word.Length && word[position] == word[position + 1])
            {
                if (IsNumericCharacter(currentChar))
                {
                    result.Append(currentChar);
                    position++;
                }
                else if (ShouldPreserveDuplicateSequence(word, position))
                {
                    result.Append(currentChar);
                    position++;
                }
                else if (ShouldPreserveDuplicateSymbol(currentChar))
                {
                    result.Append(currentChar);
                    position++;
                }
                else
                {
                    result.Append(currentChar);
                    position++;

                    while (position < word.Length && word[position] == currentChar)
                    {
                        position++; // collapse all consecutive duplicate characters
                    }
                }
            }
            else
            {
                result.Append(currentChar);
                position++;
            }
        }

        return result.ToString();
    }

    private string ProcessWord(string word, bool convertToEnglishDigit)
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

            var (charsToSkip, result) = FindMapping(op, word, i, convertToEnglishDigit);
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


    private (int, List<string>) FindMapping(List<string> op, string txt, int currentPos, bool convertToEnglishDigit)
    {
        int maxLen = _maxSequenceLength;
        int remaining = txt.Length - currentPos;

        // Ensure we check all possible length sequences within bounds
        // The _maxSequenceLength is a default guideline, but the algorithm should still be able
        // to match longer valid sequences that exist in the mappings (for cases not anticipated by the default)
        int maxPossibleLen = remaining - 1;

        // Use maxLen as normal, but if we reach the end of a potential match with maxLen,
        // still try longer sequences if they exist
        maxLen = maxLen < maxPossibleLen ? maxLen : maxPossibleLen;

        string matchedSequence = "";
        string matchedValue = "";
        int matchedLen = -1;

        // Try from longest (full remaining) to shortest match
        // This allows capturing sequences longer than _maxSequenceLength when they're legitimately in the mapping
        for (int i = maxPossibleLen; i >= 0; i--)
        {
            int substrTill = currentPos + i + 1;

            if (substrTill > txt.Length)
                continue;

            string t = txt.Substring(currentPos, i + 1);

            if (_mapping.TryGetValue(t, out var directValue))
            {
                // BUGFIX: Check if matching this sequence would unnecessarily fragment valid longer sequences
                // Specifically: don't match sequences ending in "A" if they would prevent "AiÀ..."patterns from matching

                bool shouldSkipThisMatch = false;

                // If sequence ends with "A", check what comes next
                if (t.EndsWith("A") && currentPos + t.Length < txt.Length)
                {
                    // Get the substring starting after this match
                    string afterThisMatch = txt.Substring(currentPos + t.Length);

                    // Check if "A" + afterThisMatch forms a valid longer sequence
                    // that should have consumed the "A"
                    if (afterThisMatch.Length >= 2 && (afterThisMatch.StartsWith("iÀ") || afterThisMatch.StartsWith("iÁ")))
                    {
                        // Pattern is like "...AiÀ..." which should not be split
                        // So don't match the sequence ending in "A"
                        shouldSkipThisMatch = true;
                    }
                }

                if (shouldSkipThisMatch)
                {
                    continue; // Skip this match, try shorter ones
                }

                // This match is valid
                matchedLen = i;
                matchedSequence = t;
                matchedValue = directValue;
                break; // Found best match, exit loop
            }

            if (_additionalMappings.TryGetValue(t, out var additionalValue))
            {
                matchedLen = i;
                matchedSequence = t;
                matchedValue = additionalValue;
                break;
            }
        }

        // If we found a match, apply it
        if (matchedLen >= 0)
        {
            // Add ZWJ if previous ends with halant, but only when the matched
            // value does NOT itself start with halant (e.g. ್ರ from æ/ç).
            // Values starting with ್ are conjunct continuations and need no ZWJ.
            if (op.Count > 0 && !matchedValue.StartsWith("\u0CCD"))
            {
                string lastChar = op[op.Count - 1];
                if (lastChar.EndsWith('\u0CCD'.ToString())) // Halant
                {
                    op.Add("\u200D"); // ZWJ
                }
            }

            op.Add(matchedValue);
            return (matchedLen, op);
        }

        // No mapping found - try special cases
        var letters = op.Join("").ToList();
        string singleChar = txt[currentPos].ToString();

        if (TryMapUnicodeDigit(singleChar, convertToEnglishDigit, out var digitValue))
        {
            op.Add(digitValue);
        }
        else if (_asciiArkavattu.ContainsKey(singleChar))
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
        else if (_additionalMappings.TryGetValue(singleChar, out var additionalValue))
        {
            op.Add(additionalValue);
        }
        else if (IsStrayDiacritic(singleChar, txt, currentPos))
        {
            // skip: stray À with no valid mapping context
        }
        else
        {
            op.Add(singleChar);
        }

        return (0, op);
    }

    private static bool IsNumericCharacter(char ch)
    {
        return (ch >= '\u0CE6' && ch <= '\u0CE9') || (ch >= '0' && ch <= '9');
    }

    private static bool ShouldPreserveDuplicateSymbol(char ch)
    {
        return !char.IsLetterOrDigit(ch) && !char.IsWhiteSpace(ch);
    }

    /// <summary>
    /// Returns true when a bare 'À' (U+00C0) has no valid mapping context:
    /// neither the char alone nor 'À' + next char(s) form any mapping prefix.
    /// These are OCR/encoding artifacts that should be silently dropped.
    /// </summary>
    private bool IsStrayDiacritic(string ch, string txt, int pos)
    {
        if (ch != "À") return false;

        // If 'À' + next char starts a valid mapping prefix, it is NOT stray
        if (pos + 1 < txt.Length)
        {
            string withNext = "À" + txt[pos + 1];
            if (_mappingKeyPrefixes.Contains(withNext))
                return false;
        }

        return true;
    }

    private static bool TryMapUnicodeDigit(string singleChar, bool convertToEnglishDigit, out string mappedValue)
    {
        mappedValue = string.Empty;

        if (singleChar.Length != 1)
            return false;

        char ch = singleChar[0];

        if (ch >= '0' && ch <= '9')
        {
            mappedValue = convertToEnglishDigit
                ? singleChar
                : ((char)('೦' + (ch - '0'))).ToString();
            return true;
        }

        if (ch >= '\u0CE6' && ch <= '\u0CE9')
        {
            mappedValue = convertToEnglishDigit
                ? (ch - '\u0CE6').ToString()
                : singleChar;
            return true;
        }

        return false;
    }

    private List<string> ProcessVattakshara(List<string> letters, string t)
    {
        string lastLetter = letters.Count > 0 ? letters[letters.Count - 1] : "";
        string secondLast = letters.Count > 1 ? letters[letters.Count - 2] : "";

        // BUG FIX: Ensure we have at least 1 element before trying to replace it
        if (_dependentVowels.Contains(lastLetter) && letters.Count > 0)
        {
            // If last letter is dependent vowel, replace with ZWJ + halant
            letters[letters.Count - 1] = "\u200D\u0CCD"; // ZWJ + Halant
            letters.Add(_vattaksharagalu[t]);
            letters.Add(lastLetter);
        }
        else
        {
            // No dependent vowel, just append
            letters.Add("\u0CCD");
            letters.Add(_vattaksharagalu[t]);
        }

        return letters;
    }

    private List<string> ProcessArkavattu(List<string> letters, string t)
    {
        string lastLetter = letters.Count > 0 ? letters[letters.Count - 1] : "";
        string secondLast = letters.Count > 1 ? letters[letters.Count - 2] : "";

        // BUG FIX: Ensure we have enough elements before accessing indices
        if (_dependentVowels.Contains(lastLetter) && letters.Count >= 2)
        {
            // Safe to access letters[letters.Count - 2]
            letters[letters.Count - 2] = _asciiArkavattu[t];
            letters[letters.Count - 1] = "\u0CCD"; // Halant only (no ZWJ)
            letters.Add(secondLast);
            letters.Add(lastLetter);
        }
        else if (letters.Count > 0)
        {
            // Safe to access letters[letters.Count - 1]
            letters[letters.Count - 1] = _asciiArkavattu[t];
            letters.Add("\u0CCD"); // Halant only (no ZWJ)
            letters.Add(lastLetter);
        }
        else
        {
            // Edge case: empty letters list, just add the character as-is
            letters.Add(_asciiArkavattu[t]);
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

    private string ReverseProcessWord(string word, bool convertToEnglishDigit)
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
