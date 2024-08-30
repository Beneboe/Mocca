using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Mocca.Helpers;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Mocca.Middleware;

/// <summary>
/// Overwrites json properties in the response.
/// </summary>
public sealed class MoccaOverwriteMiddleware(RequestDelegate next)
{
    private readonly JsonSerializerOptions options = new JsonSerializerOptions
    {
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
    };

    public async Task InvokeAsync(HttpContext context, IOptions<MoccaOptions> moccaOptions)
    {
        var options = moccaOptions.Value;

        var replacements = options.Overwrite
            .Where(replacement => Matches(replacement, context.Request))
            .ToArray();

        if (replacements.Length == 0)
        {
            await next(context);
            return;
        }
        
        var responseStream = context.Response.Body;
        var responseStreamBuffer = new MemoryStream();
        context.Response.Body = responseStreamBuffer;
        
        await next(context);
        
        if (IgnoredStatusCode(context.Response.StatusCode))
        {
            responseStreamBuffer.Seek(0, SeekOrigin.Begin);
            await responseStreamBuffer.CopyToAsync(responseStream);
            await responseStream.FlushAsync();
            return;
        }

        if (IgnoredContentType(context.Response.ContentType))
        {
            responseStreamBuffer.Seek(0, SeekOrigin.Begin);
            await responseStreamBuffer.CopyToAsync(responseStream);
            await responseStream.FlushAsync();
            return;
        }

        responseStreamBuffer.Seek(0, SeekOrigin.Begin);
        var documentNode = JsonSerializer.Deserialize<JsonNode>(responseStreamBuffer, this.options);

        if (documentNode is null)
        {
            responseStreamBuffer.Seek(0, SeekOrigin.Begin);
            await responseStreamBuffer.CopyToAsync(responseStream);
            await responseStream.FlushAsync();
            return;
        }
            
        foreach (var replacement in replacements)
        {
            ReplaceJson(documentNode, replacement);
        }
        
        context.Response.ContentLength = null;

        await JsonSerializer.SerializeAsync(responseStream, documentNode);
        await responseStream.FlushAsync();

    }

    private bool Matches(JsonPropertyValueReplacement replacement, HttpRequest request)
        => replacement.RequestMethod == request.Method
           && UrlPathHelper.Matches(request.Path.ToString(), replacement.RequestPathPattern);

    private static bool IgnoredStatusCode(int statusCode) => statusCode is not 200;

    private static bool IgnoredContentType(string? contentType) =>
        contentType is null || !MediaTypeHeaderValue.Parse(contentType).MatchesMediaType("application/json");

    private void ReplaceJson(JsonNode documentNode, JsonPropertyValueReplacement replacement)
    {
        var replacementNode = JsonSerializer.Deserialize<JsonNode>(replacement.Value);
        var leafNode = GetNode(documentNode, replacement.PropertyPath);

        if (leafNode is null)
        {
            return;
        }
        
        leafNode.ReplaceWith(replacementNode);
    }

    private JsonNode? GetNode(JsonNode? node, string propertyPath)
    {
        // <propertyPath> ::= <propertyName> | ""
        // <propertyAccess> ::= <propertyName> | <propertyName> <arrayAccess> | <propertyName> "." <propertyName>
        // <arrayAccess> :: "[" <arrayIndex> "]

        if (string.IsNullOrEmpty(propertyPath))
        {
            return node;
        }

        if (node is null)
        {
            return null;
        }
        
        var memberAccessIdx = propertyPath.IndexOfAny(['.', '[']);
        if (memberAccessIdx < 0)
        {
            return node[propertyPath];
        }
        else if (memberAccessIdx > 0)
        {
            var propertyName = propertyPath.Substring(0, memberAccessIdx);
            return node[propertyName];
        }

        if (propertyPath[0] == '.')
        {
            if (propertyPath.Length < 2)
            {
                throw new InvalidDataException("Input is too short. Expected another member access.");
            }
            
            var rest = propertyPath.Substring(1);
            return GetNode(node, rest);
        }
        else if (propertyPath[0] == '[')
        {
            if (propertyPath.Length < 3)
            {
                throw new InvalidDataException("Input is too short.");
            }
            
            var closeBracketIdx = propertyPath.IndexOf(']');
            if (closeBracketIdx <= 1)
            {
                throw new InvalidDataException("Input has a '[' but is missing a closing ']'.");
            }
            
            var indexTxt = propertyPath.Substring(1, closeBracketIdx - 1);
            var index = int.Parse(indexTxt, NumberStyles.None);
            var rest = propertyPath.Substring(closeBracketIdx + 1);

            return GetNode(node[index], rest);
        }

        return null;
    }
}