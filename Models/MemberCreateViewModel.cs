using Microsoft.AspNetCore.Mvc.Rendering;

namespace ToplulukYonetimSistemi.Models
{
    public class MemberCreateViewModel
    {
        public int Id { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Ad Soyad")]
        public string? FullName { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Öğrenci Numarası")]
        public string? StudentNumber { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "E-Posta")]
        public string? Email { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Bölüm")]
        public string? Department { get; set; }

        public string? ProfileImagePath { get; set; }

        public DateTime RegisteredDate { get; set; } = DateTime.Today;

        public List<int> SelectedCommunityIds { get; set; } = new();

        public List<SelectListItem> Communities { get; set; } = new();
    }
}
