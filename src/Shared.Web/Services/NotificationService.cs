using Shared.Web.Models;

namespace Shared.Web.Services;

public class NotificationService
{
    public event Action<NotificationMessage>? OnNotification;

    public void ShowSuccess(string message, string title = "Success", int autoDismissMs = 5000)
    {
        Show(message, title, AlertType.Success, autoDismissMs);
    }

    public void ShowError(string message, string title = "Error", int autoDismissMs = 5000)
    {
        Show(message, title, AlertType.Error, autoDismissMs);
    }

    public void ShowInfo(string message, string title = "Info", int autoDismissMs = 5000)
    {
        Show(message, title, AlertType.Info, autoDismissMs);
    }

    public void ShowWarning(string message, string title = "Warning", int autoDismissMs = 5000)
    {
        Show(message, title, AlertType.Warning, autoDismissMs);
    }

    private void Show(string message, string title, AlertType type, int autoDismissMs)
    {
        var notification = new NotificationMessage
        {
            Id = Guid.NewGuid(),
            Message = message,
            Title = title,
            Type = type,
            AutoDismissMs = autoDismissMs,
            Timestamp = DateTime.Now
        };

        OnNotification?.Invoke(notification);
    }
}

public class NotificationMessage
{
    public Guid Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public AlertType Type { get; set; }
    public int AutoDismissMs { get; set; }
    public DateTime Timestamp { get; set; }
}
