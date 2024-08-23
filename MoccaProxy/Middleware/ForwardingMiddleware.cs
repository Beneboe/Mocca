using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MoccaProxy;

public class ForwardingMiddleware
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ForwardingMiddleware(RequestDelegate next, IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var client = _httpClientFactory.CreateClient("ProxyClient");
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