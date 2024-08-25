namespace Mocca;

public class MoccaOptions
{
    public string ForwardTo { get; set; } = string.Empty;

    public string[] AllowedMethods { get; set; } = [ "GET", "POST" ];

    public string[] IgnoredPaths { get; set; } = Array.Empty<string>();

    public string ResponseFile { get; set; } = string.Empty;
}