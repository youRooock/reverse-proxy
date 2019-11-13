using System.Collections.Generic;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using reverse_proxy.Middleware;
using reverse_proxy.Model;

namespace reverse_proxy
{
  public class Startup
  {
    public Startup(IConfiguration configuration)
    {
      Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
      services.Configure<List<Proxy>>(Configuration.GetSection("Proxies"));
      services.AddHttpClient();
    }

    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime appLifetime,
      ILogger<Startup> logger)
    {
      logger.LogInformation("=== STARTING reverse_proxy ===");

      appLifetime.ApplicationStopping.Register(() => OnStopping(logger));
      appLifetime.ApplicationStopped.Register(() => OnStopped(logger));

      app.UseMiddleware<ErrorHandlingMiddleware>();
      app.UseMiddleware<ReverseProxyMiddleware>();

      logger.LogInformation("=== STARTED reverse_proxy ===");
    }

    private void OnStopping(ILogger<Startup> logger)
    {
      logger.LogInformation("=== STOPPING reverse_proxy ===");
    }

    private void OnStopped(ILogger<Startup> logger)
    {
      logger.LogInformation("=== STOPPED reverse_proxy ===");
    }
  }
}