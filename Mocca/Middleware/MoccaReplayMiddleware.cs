using Microsoft.AspNetCore.Http;
using Mocca.Extensions;
using Mocca.Interfaces;

namespace Mocca.Middleware;

/// <summary>
/// Matches requests with recorded responses.
/// </summary>
public class MoccaReplayMiddleware
{
    public MoccaReplayMiddleware(RequestDelegate next)
    {
    }

    public async Task InvokeAsync(HttpContext context, IMoccaRepository repository)
    {
        var request = context.Request.GetMoccaRequest();
        var response = await repository.ResolveAsync(request);

        if (response is null)
        {
            await Results.NotFound().ExecuteAsync(context);
            return;
        }

        context.Response.StatusCode = response.StatusCode;
        foreach (var (key, value) in response.Headers)
        {
            context.Request.Headers.Append(key, value);
        }

        await context.Response.Body.WriteAsync(response.Content);
    }
}