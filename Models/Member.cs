using System.ComponentModel.DataAnnotations;

namespace ToplulukYonetimSistemi.Models
{
    public class Member
    {
        public int Id { get; set; }

        [Display(Name = "Ad Soyad")]
        public string? FullName { get; set; }

        [Display(Name = "Öğrenci Numarası")]
        public string? StudentNumber { get; set; }

        [Display(Name = "E-Posta")]
        public string? Email { get; set; }

        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [Display(Name = "Bölüm")]
        public string? Department { get; set; }

        [Display(Name = "Profil Resmi")]
        public string? ProfileImagePath { get; set; }

        [Display(Name = "Kayıt Tarihi")]
        public DateTime RegisteredDate { get; set; } = DateTime.Today;

        public ICollection<MemberCommunity> MemberCommunities { get; set; } = new List<MemberCommunity>();

    }
}
