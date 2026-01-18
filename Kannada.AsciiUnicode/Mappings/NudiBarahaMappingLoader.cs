using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Kannada.AsciiUnicode.Converters;

namespace Kannada.AsciiUnicode.Mappings;

/// <summary>
/// Loads Nudi/Baraha mapping data from embedded JSON resources.
/// </summary>
public static class NudiBarahaMappingLoader
{
    private class MappingData
    {
        public Dictionary<string, string> Mapping { get; set; } = new();
        public Dictionary<string, BrokenCaseData> BrokenCases { get; set; } = new();
        public List<string> DependentVowels { get; set; } = new();
        public Dictionary<string, string> Vattaksharagalu { get; set; } = new();
        public Dictionary<string, string> AsciiArkavattu { get; set; } = new();
        public List<string> IgnoreList { get; set; } = new();
    }

    private class BrokenCaseData
    {
        public string Value { get; set; } = "";
        public Dictionary<string, string> Mapping { get; set; } = new();
    }

    /// <summary>
    /// Loads all mapping data from the embedded NudiBarahaMapping.json resource.
    /// </summary>
    public static (
        Dictionary<string, string> mapping,
        Dictionary<string, BrokenCase> brokenCases,
        HashSet<string> dependentVowels,
        Dictionary<string, string> vattaksharagalu,
        Dictionary<string, string> asciiArkavattu,
        HashSet<string> ignoreList) LoadMappings()
    {
        var assembly = typeof(NudiBarahaMappingLoader).Assembly;
        const string resourceName = "Kannada.AsciiUnicode.Resources.NudiBarahaMapping.json";

        using var stream = assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        string json = reader.ReadToEnd();

        var settings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore
        };
        var data = JsonConvert.DeserializeObject<MappingData>(json, settings)
            ?? throw new InvalidOperationException("Failed to deserialize mapping data.");

        // Convert BrokenCaseData to BrokenCase
        var brokenCases = data.BrokenCases.ToDictionary(
            kvp => kvp.Key,
            kvp => new BrokenCase
            {
                Value = kvp.Value.Value,
                CaseMapping = kvp.Value.Mapping
            });

        return (
            data.Mapping,
            brokenCases,
            new HashSet<string>(data.DependentVowels),
            data.Vattaksharagalu,
            data.AsciiArkavattu,
            new HashSet<string>(data.IgnoreList)
        );
    }
}
