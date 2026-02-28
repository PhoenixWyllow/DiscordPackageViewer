namespace DiscordPackageViewer.Services;

/// <summary>
/// Simple in-memory toast notification service.
/// Components subscribe to <see cref="OnToast"/> to display messages.
/// </summary>
public class ToastService
{
    public event Action<ToastMessage>? OnToast;

    public void Show(string message, ToastLevel level = ToastLevel.Info, int durationMs = 4000)
    {
        OnToast?.Invoke(new ToastMessage(message, level, durationMs));
    }
}

public enum ToastLevel { Info, Warning, Error }

public record ToastMessage(string Text, ToastLevel Level, int DurationMs);
