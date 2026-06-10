using System.ComponentModel.DataAnnotations;

namespace ToplulukYonetimSistemi.Models
{
    public class CommunityEvent
    {
        public int Id { get; set; }

        [Display(Name = "Etkinlik Başlığı")]
        public string? Title { get; set; }

        [Display(Name = "Açıklama")]
        public string? Description { get; set; }

        [Display(Name = "Etkinlik Tarihi")]
        public DateTime EventDate { get; set; }

        [Display(Name = "Topluluk")]
        public int CommunityId { get; set; }

        [Display(Name = "Topluluk")]
        public Community? Community { get; set; }
    }
}