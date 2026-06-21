using System.ComponentModel.DataAnnotations;

namespace ToplulukYonetimSistemi.Models
{
    public class Community
    {
        public int Id { get; set; }

        [Display(Name = "Topluluk Adı")]
        public string? Name { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Oluşturulma Tarihi")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Kapak Görseli")]
        public string? CoverImagePath { get; set; }

        public ICollection<MemberCommunity> MemberCommunities { get; set; } = new List<MemberCommunity>();

        public ICollection<CommunityEvent> Events { get; set; } = new List<CommunityEvent>();

    }
}
