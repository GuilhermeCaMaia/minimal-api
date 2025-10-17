using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using MinimalApi;

IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
}

CreateHostBuilder(args).Build().Run();