using System.IO.Pipelines;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using Mocca.Extensions;
using Mocca.Helpers;
using Mocca.Interfaces;

namespace Mocca.Middleware;

/// <summary>
/// Records request and responses.
/// </summary>
public sealed class MoccaScribeMiddleware(RequestDelegate next)
{
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
            await next(httpContext);
            return;
        }

        // Skip this middleware if the path is ignored.
        if (moccaOptions.IgnoredPaths.Any(pattern => UrlPathHelper.Matches(httpContext.Request.Path.ToString(), pattern)))
        {
            await next(httpContext);
            return;
        }

        var request = httpContext.Request.GetMoccaRequest();
        
        // Later, GetMoccaResponse() requires a readable stream.
        var responseStream = httpContext.Response.Body;
        var responseStreamBuffer = new MemoryStream();
        httpContext.Response.Body = responseStreamBuffer;

        // Reset the request stream buffer.
        requestStreamBuffer.Seek(0, SeekOrigin.Begin);
        await next(httpContext);
        
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

        if (IgnoredContentType(httpContext.Response.ContentType))
        {
            return;
        }

        // Requires a readable stream.
        var response = httpContext.Response.GetMoccaResponse();
        await repository.AddAsync(request, response);
    }

    private static bool IgnoredStatusCode(int statusCode) => statusCode is not (>= 200 and < 300);

    private static bool IgnoredContentType(string? contentType) =>
        contentType is null || !MediaTypeHeaderValue.Parse(contentType).MatchesMediaType("application/json");
}