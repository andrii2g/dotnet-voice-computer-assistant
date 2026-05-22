using System.Text.Json;
using Microsoft.Playwright;
using VoiceComputerAssistant.App.OpenAI;

namespace VoiceComputerAssistant.App.Browser;

public sealed class ComputerActionExecutor
{
    private readonly int _viewportWidth;
    private readonly int _viewportHeight;

    public ComputerActionExecutor(int viewportWidth, int viewportHeight)
    {
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
    }

    public async Task ExecuteAsync(
        IPage page,
        IReadOnlyList<ComputerAction> actions,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(actions);

        foreach (var action in actions)
        {
            cancellationToken.ThrowIfCancellationRequested();
            await ExecuteSingleAsync(page, action);
        }
    }

    private async Task ExecuteSingleAsync(IPage page, ComputerAction action)
    {
        switch (action.Type)
        {
            case "screenshot":
                return;
            case "click":
                await ExecuteClickAsync(page, action);
                return;
            case "double_click":
                await ExecuteDoubleClickAsync(page, action);
                return;
            case "move":
                await ExecuteMoveAsync(page, action);
                return;
            case "scroll":
                await ExecuteScrollAsync(page, action);
                return;
            case "type":
                await page.Keyboard.TypeAsync(action.Text ?? string.Empty);
                return;
            case "keypress":
                await ExecuteKeyPressAsync(page, action);
                return;
            case "drag":
                await ExecuteDragAsync(page, action);
                return;
            case "wait":
                await page.WaitForTimeoutAsync(1000);
                return;
            default:
                throw new NotSupportedException(
                    $"Unsupported action: {action.Type}. Raw action: {FormatRawAction(action.Raw)}");
        }
    }

    private Task ExecuteClickAsync(IPage page, ComputerAction action)
    {
        var x = RequireCoordinate(action.X, nameof(action.X));
        var y = RequireCoordinate(action.Y, nameof(action.Y));

        return WithModifiersAsync(page, action.Keys, () =>
            page.Mouse.ClickAsync(x, y, new MouseClickOptions
            {
                Button = NormalizeMouseButton(action.Button)
            }));
    }

    private Task ExecuteDoubleClickAsync(IPage page, ComputerAction action)
    {
        var x = RequireCoordinate(action.X, nameof(action.X));
        var y = RequireCoordinate(action.Y, nameof(action.Y));

        return WithModifiersAsync(page, action.Keys, () =>
            page.Mouse.DblClickAsync(x, y, new MouseDblClickOptions
            {
                Button = NormalizeMouseButton(action.Button)
            }));
    }

    private Task ExecuteMoveAsync(IPage page, ComputerAction action)
    {
        var x = RequireCoordinate(action.X, nameof(action.X));
        var y = RequireCoordinate(action.Y, nameof(action.Y));

        return WithModifiersAsync(page, action.Keys, () => page.Mouse.MoveAsync(x, y));
    }

    private Task ExecuteScrollAsync(IPage page, ComputerAction action)
    {
        var x = RequireCoordinate(action.X, nameof(action.X));
        var y = RequireCoordinate(action.Y, nameof(action.Y));
        var scrollX = action.ScrollX ?? 0;
        var scrollY = action.ScrollY ?? 0;

        return WithModifiersAsync(page, action.Keys, async () =>
        {
            await page.Mouse.MoveAsync(x, y);
            await page.Mouse.WheelAsync(scrollX, scrollY);
        });
    }

    private async Task ExecuteKeyPressAsync(IPage page, ComputerAction action)
    {
        foreach (var key in action.Keys)
        {
            await page.Keyboard.PressAsync(KeyNormalizer.Normalize(key));
        }
    }

    private Task ExecuteDragAsync(IPage page, ComputerAction action)
    {
        if (action.Path.Count < 2)
        {
            throw new InvalidOperationException("Drag action requires at least two points.");
        }

        foreach (var point in action.Path)
        {
            ValidateCoordinate(point.X, "drag.x");
            ValidateCoordinate(point.Y, "drag.y");
        }

        return WithModifiersAsync(page, action.Keys, async () =>
        {
            var start = action.Path[0];
            await page.Mouse.MoveAsync(start.X, start.Y);
            await page.Mouse.DownAsync();

            foreach (var point in action.Path.Skip(1))
            {
                await page.Mouse.MoveAsync(point.X, point.Y);
            }

            await page.Mouse.UpAsync();
        });
    }

    private async Task WithModifiersAsync(
        IPage page,
        IReadOnlyList<string> keys,
        Func<Task> action)
    {
        var modifierKeys = keys
            .Select(KeyNormalizer.Normalize)
            .Where(IsModifierKey)
            .ToArray();

        try
        {
            foreach (var key in modifierKeys)
            {
                await page.Keyboard.DownAsync(key);
            }

            await action();
        }
        finally
        {
            for (var index = modifierKeys.Length - 1; index >= 0; index--)
            {
                await page.Keyboard.UpAsync(modifierKeys[index]);
            }
        }
    }

    private int RequireCoordinate(int? value, string name)
    {
        if (!value.HasValue)
        {
            throw new InvalidOperationException($"Missing coordinate value: {name}.");
        }

        ValidateCoordinate(value.Value, name);
        return value.Value;
    }

    private void ValidateCoordinate(int value, string name)
    {
        if (name.EndsWith(".X", StringComparison.OrdinalIgnoreCase) ||
            name.EndsWith("x", StringComparison.OrdinalIgnoreCase))
        {
            if (value < 0 || value > _viewportWidth)
            {
                throw new InvalidOperationException(
                    $"Coordinate {name} is outside the viewport: {value}.");
            }

            return;
        }

        if (value < 0 || value > _viewportHeight)
        {
            throw new InvalidOperationException(
                $"Coordinate {name} is outside the viewport: {value}.");
        }
    }

    private static MouseButton NormalizeMouseButton(string? button) =>
        button?.ToLowerInvariant() switch
        {
            null or "" or "left" => MouseButton.Left,
            "right" => MouseButton.Right,
            "middle" => MouseButton.Middle,
            _ => throw new InvalidOperationException($"Unsupported mouse button: {button}.")
        };

    private static bool IsModifierKey(string key) =>
        key is "Control" or "Meta" or "Alt" or "Shift";

    private static string FormatRawAction(JsonElement raw) =>
        raw.ValueKind == JsonValueKind.Undefined ? "<undefined>" : raw.GetRawText();
}
