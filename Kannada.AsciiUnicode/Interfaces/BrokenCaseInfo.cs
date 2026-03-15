namespace Kannada.AsciiUnicode.Interfaces;

/// <summary>
/// Represents mapping information for broken case (special character combinations)
/// in Kannada ASCII to Unicode conversion
/// </summary>
public class BrokenCaseInfo
{
    /// <summary>
    /// The Unicode representation of the broken case
    /// </summary>
#pragma warning disable CS8618
    public string Value { get; set; }

    /// <summary>
    /// The ASCII to Unicode mapping for this broken case
    /// </summary>
    public Dictionary<string, string> Mapping { get; set; }
#pragma warning restore CS8618
}
