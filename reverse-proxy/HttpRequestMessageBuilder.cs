using System;
using System.IO;
using System.Net.Http;
using Microsoft.AspNetCore.Http;

namespace reverse_proxy
{
  public class HttpRequestMessageBuilder
  {
    private readonly HttpRequestMessage _request;

    public HttpRequestMessageBuilder(string method)
    {
      _request = new HttpRequestMessage {Method = new HttpMethod(method)};
    }

    public void AddContentAndHeaders(Stream bodyStream, IHeaderDictionary headers)
    {
      var requestMethod = _request.Method.Method;
      if (!HttpMethods.IsGet(requestMethod) &&
          !HttpMethods.IsHead(requestMethod) &&
          !HttpMethods.IsDelete(requestMethod) &&
          !HttpMethods.IsTrace(requestMethod))
      {
        _request.Content = new StreamContent(bodyStream);
      }

      foreach (var header in headers)
      {
        if (!_request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()))
        {
          _request.Content?.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
        }
      }
    }

    public void AddUri(string uri)
    {
      _request.RequestUri = new Uri(uri);
    }

    public void AddHost(string host)
    {
      _request.Headers.Host = host;
    }

    public HttpRequestMessage Build()
    {
      return _request;
    }
  }
}