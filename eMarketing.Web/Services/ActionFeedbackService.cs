namespace eMarketing.Web.Services;

public sealed class ActionFeedbackService
{
    public event Action<ActionToast>? ToastRequested;

    public void Success(string title, string message = "") => Show("success", title, message);

    public void Error(string title, string message = "") => Show("error", title, message);

    public void Warning(string title, string message = "") => Show("warning", title, message);

    public void Info(string title, string message = "") => Show("info", title, message);

    private void Show(string tone, string title, string message)
    {
        ToastRequested?.Invoke(new ActionToast(Guid.NewGuid(), tone, title, message, DateTimeOffset.UtcNow));
    }
}

public sealed record ActionToast(Guid Id, string Tone, string Title, string Message, DateTimeOffset CreatedAt);
