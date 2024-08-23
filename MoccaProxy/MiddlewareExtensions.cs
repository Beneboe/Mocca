using Microsoft.AspNetCore.Builder;

namespace MoccaProxy;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseForwarding(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ForwardingMiddleware>();
    }
}