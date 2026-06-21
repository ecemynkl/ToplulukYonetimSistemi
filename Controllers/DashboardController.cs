using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction(nameof(Admin));
            }

            return RedirectToAction(nameof(UserPanel));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Admin()
        {
            var model = await BuildDashboardModel();
            return View(model);
        }

        [Authorize(Roles = "Kullanici")]
        public async Task<IActionResult> UserPanel()
        {
            var model = await BuildDashboardModel();
            return View(model);
        }

        private async Task<DashboardViewModel> BuildDashboardModel()
        {
            var model = new DashboardViewModel();

            try
            {
                await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
                await DatabaseRepair.EnsureContactMessageSchemaAsync(_context);
                await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

                model.CommunityCount = await _context.Communities.CountAsync();
                model.EventCount = await _context.Events.CountAsync();
                model.MemberCount = await _context.Members.CountAsync();
                model.AnnouncementCount = await _context.Announcements.CountAsync();
                model.ContactMessageCount = await _context.ContactMessages.CountAsync();
                model.UpcomingEventCount = await _context.Events.CountAsync(e => e.EventDate >= DateTime.Today);

                if (User.IsInRole("Kullanici"))
                {
                    var memberIdClaim = User.FindFirstValue("MemberId");
                    var studentNumber = User.FindFirstValue(ClaimTypes.NameIdentifier);

                    model.CurrentMember = await _context.Members
                        .Include(m => m.MemberCommunities)
                        .ThenInclude(mc => mc.Community)
                        .FirstOrDefaultAsync(m =>
                            (!string.IsNullOrWhiteSpace(memberIdClaim) && m.Id.ToString() == memberIdClaim) ||
                            (!string.IsNullOrWhiteSpace(studentNumber) && m.StudentNumber == studentNumber));

                    var userName = User.Identity?.Name ?? string.Empty;
                    model.PendingJoinRequests = await _context.JoinRequests
                        .Include(r => r.Community)
                        .Where(r => r.UserName == userName && !r.IsApproved && !r.IsRejected)
                        .OrderByDescending(r => r.RequestedDate)
                        .ToListAsync();
                }
            }
            catch
            {
                return model;
            }

            return model;
        }
    }
}
