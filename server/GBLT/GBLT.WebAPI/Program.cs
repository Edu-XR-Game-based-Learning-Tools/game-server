using Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using WebAPI;

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder
                .UseKestrel(options =>
                {
                    // WORKAROUND: Accept HTTP/2 only to allow insecure HTTP/2 connections during development.
                    options.ConfigureEndpointDefaults(endpointOptions =>
                    {
                        endpointOptions.Protocols = HttpProtocols.Http2;
                    });
                })
                .UseStartup<Startup>();
        });

var builder = CreateHostBuilder(args);
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var loggerFactory = services.GetRequiredService<ILoggerFactory>();
    try
    {
        var dbContext = services.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = loggerFactory.CreateLogger<Program>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}

app.Run();
