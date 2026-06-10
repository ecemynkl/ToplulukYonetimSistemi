using Microsoft.AspNetCore.Mvc.Rendering;

namespace ToplulukYonetimSistemi.Models
{
    public class MemberCreateViewModel
    {
        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? Phone { get; set; }

        public string? Department { get; set; }

        public List<int> SelectedCommunityIds { get; set; } = new();

        public List<SelectListItem> Communities { get; set; } = new();
    }
}