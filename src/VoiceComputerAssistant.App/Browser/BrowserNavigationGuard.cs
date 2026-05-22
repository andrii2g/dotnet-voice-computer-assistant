namespace VoiceComputerAssistant.App.Browser;

public static class BrowserNavigationGuard
{
    public static string GetAllowedOrigin(string demoUrl)
    {
        if (!Uri.TryCreate(demoUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException("Demo URL must be an absolute URL.", nameof(demoUrl));
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Demo URL must use HTTP.", nameof(demoUrl));
        }

        if (!string.Equals(uri.Host, "127.0.0.1", StringComparison.Ordinal))
        {
            throw new ArgumentException("Demo URL host must be 127.0.0.1.", nameof(demoUrl));
        }

        return $"{uri.Scheme}://{uri.Authority}";
    }

    public static bool IsAllowedRequestUrl(string url, string allowedOrigin)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        if (url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (url.StartsWith("about:", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return url.StartsWith(allowedOrigin, StringComparison.OrdinalIgnoreCase);
    }

    public static void EnsureAllowedPageUrl(string? currentUrl, string allowedOrigin)
    {
        if (string.IsNullOrWhiteSpace(currentUrl))
        {
            throw new InvalidOperationException("Browser page URL is empty.");
        }

        if (!currentUrl.StartsWith(allowedOrigin, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Browser navigated outside the allowed origin. Current URL: {currentUrl}");
        }
    }
}
