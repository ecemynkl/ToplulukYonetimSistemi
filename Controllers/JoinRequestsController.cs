using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class JoinRequestsController : Controller
    {
        private readonly AppDbContext _context;

        public JoinRequestsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            var requests = await _context.JoinRequests
                .Include(r => r.Community)
                .OrderByDescending(r => r.RequestedDate)
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            var request = await _context.JoinRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.MemberCommunities)
                .FirstOrDefaultAsync(m => m.FullName == request.UserName);

            if (member == null)
            {
                member = new Member
                {
                    FullName = request.UserName,
                    RegisteredDate = DateTime.Today
                };
                _context.Members.Add(member);
                await _context.SaveChangesAsync();
            }

            var alreadyMember = await _context.MemberCommunities
                .AnyAsync(mc => mc.MemberId == member.Id && mc.CommunityId == request.CommunityId);

            if (!alreadyMember && member.MemberCommunities.Count >= 5)
            {
                TempData["JoinRequestWarning"] = $"{member.FullName} en fazla 5 topluluğa üye olabilir.";
                return RedirectToAction(nameof(Index));
            }

            if (!alreadyMember)
            {
                _context.MemberCommunities.Add(new MemberCommunity
                {
                    MemberId = member.Id,
                    CommunityId = request.CommunityId
                });
            }

            request.IsApproved = true;
            request.IsRejected = false;
            await _context.SaveChangesAsync();

            TempData["JoinRequestSuccess"] = "Katılma isteği onaylandı.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var request = await _context.JoinRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null)
            {
                return NotFound();
            }

            request.IsRejected = true;
            request.IsApproved = false;
            await _context.SaveChangesAsync();

            TempData["JoinRequestWarning"] = "Katılma isteği reddedildi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            var request = await _context.JoinRequests.FirstOrDefaultAsync(r => r.Id == id);
            if (request == null)
            {
                return NotFound();
            }

            _context.JoinRequests.Remove(request);
            await _context.SaveChangesAsync();

            TempData["JoinRequestWarning"] = "Katılma isteği silindi.";
            return RedirectToAction(nameof(Index));
        }
    }
}
