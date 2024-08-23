using Microsoft.AspNetCore.Builder;
using MoccaProxy.Middleware;

namespace MoccaProxy;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseForwarding(this IApplicationBuilder builder) => builder.UseMiddleware<ForwardingMiddleware>();

    public static IApplicationBuilder UseScribe(this IApplicationBuilder builder) =>
        builder.UseMiddleware<ScribeMiddleware>();
}