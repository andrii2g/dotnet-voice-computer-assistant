using Microsoft.Playwright;

namespace VoiceComputerAssistant.App.Browser;

public sealed class PlaywrightBrowserSession : IAsyncDisposable
{
    private readonly IPlaywright _playwright;
    private readonly IBrowser _browser;
    private readonly IBrowserContext _context;

    private PlaywrightBrowserSession(
        IPlaywright playwright,
        IBrowser browser,
        IBrowserContext context,
        IPage page,
        string allowedOrigin,
        int viewportWidth,
        int viewportHeight)
    {
        _playwright = playwright;
        _browser = browser;
        _context = context;
        Page = page;
        AllowedOrigin = allowedOrigin;
        ViewportWidth = viewportWidth;
        ViewportHeight = viewportHeight;
    }

    public IPage Page { get; }

    public string AllowedOrigin { get; }

    public int ViewportWidth { get; }

    public int ViewportHeight { get; }

    public static async Task<PlaywrightBrowserSession> StartAsync(
        string demoUrl,
        bool headless,
        CancellationToken cancellationToken)
    {
        const int viewportWidth = 1280;
        const int viewportHeight = 720;

        cancellationToken.ThrowIfCancellationRequested();

        var allowedOrigin = BrowserNavigationGuard.GetAllowedOrigin(demoUrl);
        var playwright = await Playwright.CreateAsync().WaitAsync(cancellationToken);

        try
        {
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = headless,
                Args =
                [
                    "--disable-extensions",
                    "--disable-file-system"
                ],
                Env = new Dictionary<string, string>()
            }).WaitAsync(cancellationToken);

            try
            {
                var context = await browser.NewContextAsync(new BrowserNewContextOptions
                {
                    ViewportSize = new ViewportSize
                    {
                        Width = viewportWidth,
                        Height = viewportHeight
                    }
                }).WaitAsync(cancellationToken);

                try
                {
                    await context.RouteAsync("**/*", route =>
                    {
                        if (BrowserNavigationGuard.IsAllowedRequestUrl(route.Request.Url, allowedOrigin))
                        {
                            return route.ContinueAsync();
                        }

                        return route.AbortAsync();
                    }).WaitAsync(cancellationToken);

                    var page = await context.NewPageAsync().WaitAsync(cancellationToken);
                    await page.GotoAsync(demoUrl, new PageGotoOptions
                    {
                        WaitUntil = WaitUntilState.NetworkIdle
                    }).WaitAsync(cancellationToken);

                    await page.Locator("h1").WaitForAsync().WaitAsync(cancellationToken);
                    BrowserNavigationGuard.EnsureAllowedPageUrl(page.Url, allowedOrigin);

                    return new PlaywrightBrowserSession(
                        playwright,
                        browser,
                        context,
                        page,
                        allowedOrigin,
                        viewportWidth,
                        viewportHeight);
                }
                catch
                {
                    await context.DisposeAsync();
                    throw;
                }
            }
            catch
            {
                await browser.DisposeAsync();
                throw;
            }
        }
        catch
        {
            playwright.Dispose();
            throw;
        }
    }

    public Task<byte[]> CaptureScreenshotAsync(CancellationToken cancellationToken) =>
        Page.ScreenshotAsync(new PageScreenshotOptions
        {
            FullPage = false,
            Type = ScreenshotType.Png
        }).WaitAsync(cancellationToken);

    public Task EnsureStillOnAllowedOriginAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        BrowserNavigationGuard.EnsureAllowedPageUrl(Page.Url, AllowedOrigin);
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync()
    {
        await _context.DisposeAsync();
        await _browser.DisposeAsync();
        _playwright.Dispose();
    }
}
