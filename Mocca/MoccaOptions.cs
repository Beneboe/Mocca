namespace Mocca;

public sealed class MoccaOptions
{
    public string ForwardTo { get; set; } = string.Empty;

    public string[] AllowedMethods { get; set; } = [ "GET", "POST" ];

    public string[] IgnoredPaths { get; set; } = Array.Empty<string>();

    public bool IgnoreRequestHeadersEquality { get; set; }

    public string ResponseFile { get; set; } = string.Empty;

    public JsonPropertyValueReplacement[] Overwrite { get; set; } =
        Array.Empty<JsonPropertyValueReplacement>();
}

public sealed class JsonPropertyValueReplacement
{
    public string RequestMethod { get; set; } = string.Empty;

    public string RequestPathPattern { get; set; } = string.Empty;

    public string PropertyPath { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}