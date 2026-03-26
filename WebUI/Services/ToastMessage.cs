using System;

namespace WebUI.Services
{
    public class ToastMessage
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public ToastLevel Level { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
