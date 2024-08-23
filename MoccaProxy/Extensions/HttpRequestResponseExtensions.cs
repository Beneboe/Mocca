using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using MoccaProxy.Data;

namespace MoccaProxy;

internal static class HttpRequestResponseExtensions
{
    public static MoccaRequest GetMoccaRequest(this HttpRequest request)
    {
        return new MoccaRequest()
        {
            Method = request.Method,
            Path = request.Path,
            HeaderHash = Hash(request.Headers),
            ContentHash = request.ContentLength is null or 0
                ? SHA1.HashData(Array.Empty<byte>()) 
                : Hash(request.Body),
        };
    }

    public static MoccaResponse GetMoccaResponse(this HttpResponse response)
    {
        var stream = response.Body;
        if (stream is not MemoryStream memoryStream)
        {
            memoryStream = new MemoryStream();
            
            byte[] buffer = new byte[16*1024];
            int read;
            
            stream.Seek(0, SeekOrigin.Begin);
            while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                memoryStream.Write(buffer, 0, read);
            }
        }
        var contentBytes = memoryStream.ToArray();

        var headers = response.Headers.ToDictionary(
            keySelector: kvp => kvp.Key,
            elementSelector: kvp => kvp.Value.ToString());
        
        return new MoccaResponse()
        {
            StatusCode = response.StatusCode,
            Headers = headers,
            Content = contentBytes,
        };
    }

    private static byte[] Hash(IHeaderDictionary dictionary)
    {
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream, Encoding.UTF8);
        foreach (var (key, value) in dictionary)
        {
            writer.Write(key);
            writer.Write(value.ToString());
        }

        writer.Seek(0, SeekOrigin.Begin);
        
        return SHA1.HashData(stream);
        // return Convert.ToBase64String(hash);
    }

    private static byte[] Hash(Stream body)
    {
        body.Seek(0, SeekOrigin.Begin);
        return SHA1.HashData(body);
    }
}