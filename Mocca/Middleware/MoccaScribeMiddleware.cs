using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Mocca.Extensions;
using Mocca.Interfaces;

namespace Mocca.Middleware;

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
        
        // Skip this middleware if the method is not supported such as DELETE.
        if (!moccaOptions.AllowedMethods.Contains(httpContext.Request.Method))
        {
            await _next(httpContext);
            return;
        }

        if (moccaOptions.IgnoredPaths.Any(pattern => Matches(httpContext.Request.Path.ToString(), pattern)))
        {
            await _next(httpContext);
            return;
        }
        
        var request = httpContext.Request.GetMoccaRequest();
        
        var originalStream = httpContext.Response.Body;
        var memoryStream = new MemoryStream();
        
        // Capture body in the memory stream.
        httpContext.Response.Body = memoryStream;
        await _next(httpContext);

        if (httpContext.RequestAborted.IsCancellationRequested)
        {
            return;
        }

        // Requires a readable stream.
        var response = httpContext.Response.GetMoccaResponse();

        await repository.AddAsync(request, response);

        memoryStream.Seek(0, SeekOrigin.Begin);
        await memoryStream.CopyToAsync(originalStream);
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
}