namespace ToplulukYonetimSistemi.Models
{
    public class NotificationViewItem
    {
        public string Icon { get; set; } = "bi-bell";

        public string Title { get; set; } = string.Empty;

        public string Text { get; set; } = string.Empty;

        public string Url { get; set; } = "#";

        public DateTime CreatedDate { get; set; }
    }
}
