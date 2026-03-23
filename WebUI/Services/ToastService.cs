using System;
using System.Collections.Generic;
using System.Linq;

namespace WebUI.Services
{
    public class ToastService
    {
        public event Action OnChange;

        private readonly List<ToastMessage> _messages = new();

        public IReadOnlyList<ToastMessage> Messages => _messages.AsReadOnly();

        public void Notify(string message, ToastLevel level)
        {
            var toast = new ToastMessage
            {
                Id = Guid.NewGuid(),
                Text = message,
                Level = level,
                Timestamp = DateTime.Now
            };

            _messages.Add(toast);
            NotifyStateChanged();

            // 5 saniye sonra otomatik kaldır
            Task.Delay(5000).ContinueWith(_ => Remove(toast.Id));
        }

        public void Remove(Guid id)
        {
            var toast = _messages.FirstOrDefault(x => x.Id == id);
            if (toast != null)
            {
                _messages.Remove(toast);
                NotifyStateChanged();
            }
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }

    public class ToastMessage
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public ToastLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum ToastLevel
    {
        Success,
        Error,
        Info,
        Warning
    }
}
