namespace Mocca.Data;

public sealed class MoccaResponse
{
    public int StatusCode { get; set; }

    public Dictionary<string, string?> Headers { get; set; } = new Dictionary<string, string?>();

    public byte[] Content { get; set; } = Array.Empty<byte>();
}