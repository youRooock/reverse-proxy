using System;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace reverse_proxy
{
  public class Program
  {
    public static async Task Main(string[] args)
    {
      await CreateHostBuilder(args).Build().RunAsync();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
        .UseServiceProviderFactory(new AutofacServiceProviderFactory())
        .ConfigureWebHostDefaults(webBuilder =>
        {
          webBuilder
            .UseStartup<Startup>()
            .UseUrls("http://*:14854")
            .UseIISIntegration()
            .ConfigureLogging((cx, cb) =>
            {
              cb.AddLog4Net(new Log4NetProviderOptions("log.config", true))
                .AddFilter<Log4NetProvider>("Microsoft", LogLevel.Warning)
                .AddFilter<Log4NetProvider>("System", LogLevel.Warning);
            })
            .CaptureStartupErrors(true);
        })
        .ConfigureServices((hbc, services) =>
        {
          services.Configure<HostOptions>(option => { option.ShutdownTimeout = TimeSpan.FromSeconds(60); });
        });
  }
}