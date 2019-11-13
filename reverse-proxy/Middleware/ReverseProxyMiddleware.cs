using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using reverse_proxy.Model;

namespace reverse_proxy.Middleware
{
  public class ReverseProxyMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly IHttpClientFactory _httpFactory;
    private Dictionary<string, Uri> _proxies;

    public ReverseProxyMiddleware(RequestDelegate next, IOptionsMonitor<List<Proxy>> proxyOptions,
      IHttpClientFactory httpFactory)
    {
      _next = next;
      _httpFactory = httpFactory;
      _proxies = BuildDictionaryOptions(proxyOptions.CurrentValue);
      proxyOptions.OnChange(x => _proxies = BuildDictionaryOptions(x));
    }

    public async Task Invoke(HttpContext context)
    {
      PathString remainingPath;

      context.Request.EnableBuffering();
      var matchingProxy =
        _proxies.Keys.FirstOrDefault(k => context.Request.Path.StartsWithSegments(k, out remainingPath));

      if (!string.IsNullOrEmpty(matchingProxy))
      {
        var http = _httpFactory.CreateClient();
        var baseUri = _proxies[matchingProxy];

        var builder = new HttpRequestMessageBuilder(context.Request.Method);
        builder.AddUri(baseUri + remainingPath + context.Request.QueryString);
        builder.AddContentAndHeaders(context.Request.Body, context.Request.Headers);
        builder.AddHost(baseUri.Authority);

        var request = builder.Build();

        using var response = await http.SendAsync(request);

        await ModifyResponse(context.Response, response);
        return;
      }

      await _next.Invoke(context);
    }

    private async Task ModifyResponse(HttpResponse copyTo, HttpResponseMessage copyFrom)
    {
      copyTo.StatusCode = (int) copyFrom.StatusCode;

      foreach (var header in copyFrom.Headers)
      {
        copyTo.Headers[header.Key] = header.Value.ToArray();
      }

      foreach (var header in copyFrom.Content.Headers)
      {
        copyTo.Headers[header.Key] = header.Value.ToArray();
      }

      // SendAsync removes chunking from the response. This removes the header so it doesn't expect a chunked response.
      copyTo.Headers.Remove("transfer-encoding");
      await copyFrom.Content.CopyToAsync(copyTo.Body);
    }

    private Dictionary<string, Uri> BuildDictionaryOptions(List<Proxy> options)
    {
      return options?.ToDictionary(k => k.Location, v => new Uri(v.ProxyUrl));
    }
  }
}