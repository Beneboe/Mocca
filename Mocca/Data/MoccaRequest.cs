using System.Text.Json.Serialization;

namespace Mocca.Data;

public sealed class MoccaRequest : IEquatable<MoccaRequest>
{
    public string Method { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public byte[] HeaderHash { get; set; } = Array.Empty<byte>();

    public byte[] ContentHash { get; set; } = Array.Empty<byte>();

    [JsonIgnore]
    public bool IsDefault
        => string.IsNullOrWhiteSpace(Path)
           || string.IsNullOrWhiteSpace(Method)
           || HeaderHash.Length == 0
           || ContentHash.Length == 0;

    public bool Equals(MoccaRequest? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Method == other.Method
               && Path == other.Path
               && HeaderHash.SequenceEqual(other.HeaderHash)
               && ContentHash.SequenceEqual(other.ContentHash);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is MoccaRequest other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Method, Path, HeaderHash, ContentHash);
    }
}