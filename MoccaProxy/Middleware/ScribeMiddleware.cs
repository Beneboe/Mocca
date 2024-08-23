using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using MoccaProxy.Interfaces;

namespace MoccaProxy.Middleware;

public sealed class ScribeMiddleware
{
    private readonly RequestDelegate _next;

    public ScribeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, IMoccaRepository repository)
    {
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
}