using System.ComponentModel.DataAnnotations;

namespace ToplulukYonetimSistemi.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Display(Name = "Başlık")]
        [Required(ErrorMessage = "Başlık zorunludur.")]
        public string? Title { get; set; }

        [Display(Name = "İçerik")]
        [Required(ErrorMessage = "İçerik zorunludur.")]
        public string? Content { get; set; }

        [Display(Name = "Duyuru Tarihi")]
        public DateTime CreatedDate { get; set; }

        [Display(Name = "Kapak Fotoğrafı")]
        public string? CoverImagePath { get; set; }

        [Display(Name = "Topluluk")]
        public int CommunityId { get; set; }

        public Community? Community { get; set; }
    }
}
