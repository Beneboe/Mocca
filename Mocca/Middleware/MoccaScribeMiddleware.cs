using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Mocca.Extensions;
using Mocca.Interfaces;

namespace Mocca.Middleware;

/// <summary>
/// Records request and responses.
/// </summary>
public sealed class MoccaScribeMiddleware
{
    private readonly RequestDelegate _next;

    public MoccaScribeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IOptions<MoccaOptions> options, IMoccaRepository repository)
    {
        var moccaOptions = options.Value;
        
        // Redirects require the request stream needs to be resettable.
        var requestStreamBuffer = new MemoryStream();
        await httpContext.Request.Body.CopyToAsync(requestStreamBuffer);
        httpContext.Request.Body = requestStreamBuffer;

        // Skip this middleware if the method is not supported such as DELETE.
        if (!moccaOptions.AllowedMethods.Contains(httpContext.Request.Method))
        {
            await _next(httpContext);
            return;
        }

        // Skip this middleware if the path is ignored.
        if (moccaOptions.IgnoredPaths.Any(pattern => Matches(httpContext.Request.Path.ToString(), pattern)))
        {
            await _next(httpContext);
            return;
        }

        var request = httpContext.Request.GetMoccaRequest();
        
        // Later, GetMoccaResponse() requires a readable stream.
        var responseStream = httpContext.Response.Body;
        var responseStreamBuffer = new MemoryStream();
        httpContext.Response.Body = responseStreamBuffer;

        // Reset the request stream buffer.
        requestStreamBuffer.Seek(0, SeekOrigin.Begin);
        await _next(httpContext);
        
        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            return;
        }
        
        responseStreamBuffer.Seek(0, SeekOrigin.Begin);
        await responseStreamBuffer.CopyToAsync(responseStream);
        await responseStream.FlushAsync();
        
        if (IgnoredStatusCode(httpContext.Response.StatusCode))
        {
            return;
        }

        // Requires a readable stream.
        var response = httpContext.Response.GetMoccaResponse();
        await repository.AddAsync(request, response);
    }

    private static bool Matches(ReadOnlySpan<char> path, ReadOnlySpan<char> pattern)
    {
        if (path.Length == 0 && pattern.Length == 0)
        {
            return true;
        }
        else if (path.Length == 0 && pattern.Length > 0)
        {
            return pattern[0] == '*' 
                   && (pattern.Length == 1  || (pattern.Length == 2  && pattern[1] == '*'));
        }
        else if (pattern.Length > 0 && pattern[0] == '*')
        {
            var pathTail = path.Slice(1);
            var patternTail = pattern.Slice(1);
            
            return Matches(path, patternTail) || Matches(pathTail, pattern);
        }
        else if (pattern.Length > 0)
        {
            var pathTail = path.Slice(1);
            var patternTail = pattern.Slice(1);

            return path[0] == pattern[0] && Matches(pathTail, patternTail);
        }
        else
        {
            return false;
        }
    }

    private static bool IgnoredStatusCode(int statusCode) => statusCode is not (>= 200 and < 300);
}