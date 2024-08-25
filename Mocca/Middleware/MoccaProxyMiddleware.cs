using Microsoft.AspNetCore.Http;

namespace Mocca.Middleware;

/// <summary>
/// Forwards request to another web server.
/// </summary>
public sealed class MoccaProxyMiddleware
{
    public MoccaProxyMiddleware(RequestDelegate next)
    {
    }

    public async Task InvokeAsync(HttpContext context, IHttpClientFactory httpClientFactory)
    {
        var client = httpClientFactory.CreateClient("ProxyClient");
        var requestMessage = CreateRequest(context.Request);
        var responseMessage = await client.SendAsync(requestMessage, context.RequestAborted);

        await HandleResponse(responseMessage, context.Response);
    }

    private static HttpRequestMessage CreateRequest(HttpRequest request)
    {
        var requestMessage = new HttpRequestMessage(HttpMethod.Parse(request.Method), request.Path)
        {
            Content = new StreamContent(request.Body),
        };
        
        foreach (var ( key, values) in request.Headers)
        {
            request.Headers.Append(key, values);
        }

        return requestMessage;
    }

    private static async Task HandleResponse(HttpResponseMessage responseMessage, HttpResponse response)
    {
        response.StatusCode = (int)responseMessage.StatusCode;
        foreach (var (key, values) in response.Headers)
        {
            response.Headers.Append(key, values);
        }
        
        await responseMessage.Content.CopyToAsync(response.Body);
    }
}