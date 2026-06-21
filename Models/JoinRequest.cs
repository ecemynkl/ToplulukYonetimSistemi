using System.ComponentModel.DataAnnotations;

namespace ToplulukYonetimSistemi.Models
{
    public class JoinRequest
    {
        public int Id { get; set; }

        public int CommunityId { get; set; }

        public Community? Community { get; set; }

        [Display(Name = "Kullanıcı")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Talep Tarihi")]
        public DateTime RequestedDate { get; set; } = DateTime.Now;

        public bool IsApproved { get; set; }

        public bool IsRejected { get; set; }
    }
}
