namespace ToplulukYonetimSistemi.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CommunityId { get; set; }

        public Community? Community { get; set; }
    }
}