using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace reverse_proxy.Middleware
{
  public class ErrorHandlingMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
      _next = next;
      _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
      try
      {
        await _next.Invoke(context);
      }
      catch (Exception ex)
      {
        await HandleExceptionAsync(context, ex);
      }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
      var statusCode = HttpStatusCode.InternalServerError;

      _logger.LogError(exception, "Unknown exception");

      var result = JsonConvert.SerializeObject(new {message = exception.GetBaseException().Message});
      context.Response.ContentType = "application/json";
      context.Response.StatusCode = (int) statusCode;
      return context.Response.WriteAsync(result);
    }
  }
}