namespace ToplulukYonetimSistemi.Models
{
    public class HomeIndexViewModel
    {
        public int CommunityCount { get; set; }

        public int MemberCount { get; set; }

        public int UpcomingEventCount { get; set; }

        public List<Announcement> Announcements { get; set; } = new();
    }
}
