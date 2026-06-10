namespace ToplulukYonetimSistemi.Models
{
    public class MemberCommunity
    {
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public int CommunityId { get; set; }
        public Community? Community { get; set; }
    }
}