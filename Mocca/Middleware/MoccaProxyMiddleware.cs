using System.Net.Http.Headers;
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
        
        if (request.ContentType is not null)
        {
            requestMessage.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);
        }
        
        foreach (var ( key, values) in request.Headers)
        {
            if (key is "Content-Length" or "Content-Type")
            {
                continue;
            }
            
            requestMessage.Headers.Add(key, (IEnumerable<string>)values);
        }

        return requestMessage;
    }

    private static async Task HandleResponse(HttpResponseMessage responseMessage, HttpResponse response)
    {
        response.StatusCode = (int)responseMessage.StatusCode;
        foreach (var (key, values) in responseMessage.Headers)
        {
            var value = values.FirstOrDefault();
            if (value is null)
            {
                continue;
            }
            
            response.Headers.Append(key, value);
        }

        if (responseMessage.Content.Headers.ContentType is not null)
        {
            response.ContentType = responseMessage.Content.Headers.ContentType.ToString();
        }

        if (responseMessage.Content.Headers.ContentLength is not null)
        {
            response.ContentLength = responseMessage.Content.Headers.ContentLength;
        }

        if ((int)responseMessage.StatusCode is not (>= 300 and < 400))
        {
            await responseMessage.Content.CopyToAsync(response.Body);
        }
    }
}