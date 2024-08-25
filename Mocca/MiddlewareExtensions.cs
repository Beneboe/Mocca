using Microsoft.AspNetCore.Builder;
using Mocca.Middleware;

namespace Mocca;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseMoccaProxy(this IApplicationBuilder builder) =>
        builder.UseMiddleware<MoccaProxyMiddleware>();

    public static IApplicationBuilder UseMoccaScribe(this IApplicationBuilder builder) =>
        builder.UseMiddleware<MoccaScribeMiddleware>();

    public static IApplicationBuilder UseMoccaReplay(this IApplicationBuilder builder) =>
        builder.UseMiddleware<MoccaReplayMiddleware>();
}