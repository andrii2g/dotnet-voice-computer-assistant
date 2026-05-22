using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;

namespace VoiceComputerAssistant.App.DemoSite;

public sealed class DemoSiteServer : IAsyncDisposable
{
    private readonly WebApplication _application;

    private DemoSiteServer(WebApplication application, string baseUrl)
    {
        _application = application;
        BaseUrl = baseUrl;
    }

    public string BaseUrl { get; }

    public static async Task<DemoSiteServer> StartAsync(
        string siteDirectory,
        int port,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(siteDirectory))
        {
            throw new ArgumentException("Site directory is required.", nameof(siteDirectory));
        }

        if (!Directory.Exists(siteDirectory))
        {
            throw new DirectoryNotFoundException($"Demo site directory was not found: {siteDirectory}");
        }

        var builder = WebApplication.CreateBuilder();
        var baseUrl = $"http://127.0.0.1:{port}";
        builder.WebHost.UseUrls(baseUrl);

        var app = builder.Build();
        var fileProvider = new PhysicalFileProvider(siteDirectory);

        app.UseDefaultFiles(new DefaultFilesOptions
        {
            FileProvider = fileProvider
        });

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = fileProvider
        });

        app.MapGet("/health", () => "ok");

        try
        {
            await app.StartAsync(cancellationToken);
        }
        catch (IOException exception)
        {
            await app.DisposeAsync();
            throw new InvalidOperationException($"Failed to start demo site server on {baseUrl}.", exception);
        }

        return new DemoSiteServer(app, baseUrl);
    }

    public async ValueTask DisposeAsync()
    {
        await _application.StopAsync();
        await _application.DisposeAsync();
    }
}
