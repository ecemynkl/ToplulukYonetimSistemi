namespace ToplulukYonetimSistemi.Models
{
    public class DashboardViewModel
    {
        public int CommunityCount { get; set; }

        public int EventCount { get; set; }

        public int MemberCount { get; set; }

        public int AnnouncementCount { get; set; }

        public int ContactMessageCount { get; set; }

        public int UpcomingEventCount { get; set; }

        public Member? CurrentMember { get; set; }

        public List<JoinRequest> PendingJoinRequests { get; set; } = new();
    }
}
