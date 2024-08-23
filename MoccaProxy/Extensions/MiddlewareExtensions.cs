using Microsoft.AspNetCore.Builder;
using MoccaProxy.Middleware;

namespace MoccaProxy;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseMoccaProxy(this IApplicationBuilder builder) => builder.UseMiddleware<MoccaProxyMiddleware>();

    public static IApplicationBuilder UseMoccaScribe(this IApplicationBuilder builder) =>
        builder.UseMiddleware<MoccaScribeMiddleware>();
}