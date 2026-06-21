using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    [Authorize]
    public class CommunitiesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CommunitiesController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Communities
        public async Task<IActionResult> Index()
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            return View(await _context.Communities
                .Include(c => c.MemberCommunities)
                .ThenInclude(mc => mc.Member)
                .OrderBy(c => c.Name)
                .ToListAsync());
        }

        // GET: Communities/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (id == null)
            {
                return NotFound();
            }

            var community = await _context.Communities
                .Include(c => c.MemberCommunities)
                .ThenInclude(mc => mc.Member)
                .Include(c => c.Events)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (community == null)
            {
                return NotFound();
            }

            return View(community);
        }

        // GET: Communities/Create
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create()
        {
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);
            return View();
        }

        // POST: Communities/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CreatedDate")] Community community, IFormFile? coverImage)
        {
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (ModelState.IsValid)
            {
                if (community.CreatedDate == default)
                {
                    community.CreatedDate = DateTime.Today;
                }

                community.CoverImagePath = await SaveCommunityCoverAsync(coverImage);
                _context.Add(community);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(community);
        }

        // GET: Communities/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            await DatabaseRepair.EnsureMediaColumnsAsync(_context);
            var community = await _context.Communities.FindAsync(id);
            if (community == null)
            {
                return NotFound();
            }
            return View(community);
        }

        // POST: Communities/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CreatedDate")] Community community, IFormFile? coverImage)
        {
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (id != community.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCommunity = await _context.Communities.FindAsync(id);
                    if (existingCommunity == null)
                    {
                        return NotFound();
                    }

                    existingCommunity.Name = community.Name;
                    existingCommunity.Description = community.Description;
                    existingCommunity.CreatedDate = community.CreatedDate == default
                        ? existingCommunity.CreatedDate
                        : community.CreatedDate;

                    var uploadedPath = await SaveCommunityCoverAsync(coverImage);
                    if (!string.IsNullOrWhiteSpace(uploadedPath))
                    {
                        existingCommunity.CoverImagePath = uploadedPath;
                    }

                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CommunityExists(community.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(community);
        }

        // GET: Communities/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (id == null)
            {
                return NotFound();
            }

            var community = await _context.Communities
                .Include(c => c.MemberCommunities)
                .ThenInclude(mc => mc.Member)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (community == null)
            {
                return NotFound();
            }

            return View(community);
        }

        // POST: Communities/Delete/5
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            var community = await _context.Communities
                .Include(c => c.MemberCommunities)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (community != null)
            {
                _context.MemberCommunities.RemoveRange(community.MemberCommunities);
                _context.Communities.Remove(community);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestJoin(int id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            var communityExists = await _context.Communities.AnyAsync(c => c.Id == id);
            if (!communityExists)
            {
                return NotFound();
            }

            var memberIdClaim = User.FindFirstValue("MemberId");
            var studentNumber = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = await _context.Members
                .Include(m => m.MemberCommunities)
                .FirstOrDefaultAsync(m =>
                    (!string.IsNullOrWhiteSpace(memberIdClaim) && m.Id.ToString() == memberIdClaim) ||
                    (!string.IsNullOrWhiteSpace(studentNumber) && m.StudentNumber == studentNumber));

            if (member == null)
            {
                TempData["CommunityWarning"] = "Katılma isteği göndermek için öğrenci kaydınız bulunmalıdır.";
                return RedirectToAction(nameof(Index));
            }

            if (member.MemberCommunities.Any(mc => mc.CommunityId == id))
            {
                TempData["CommunityWarning"] = "Bu topluluğa zaten üyesiniz.";
                return RedirectToAction(nameof(Index));
            }

            if (member.MemberCommunities.Count >= 5)
            {
                TempData["CommunityWarning"] = "En fazla 5 topluluğa üye olabilirsiniz.";
                return RedirectToAction(nameof(Index));
            }

            var userName = User.Identity?.Name ?? "Kullanıcı";
            var hasOpenRequest = await _context.JoinRequests.AnyAsync(r =>
                r.CommunityId == id &&
                r.UserName == userName &&
                !r.IsApproved &&
                !r.IsRejected);

            if (!hasOpenRequest)
            {
                _context.JoinRequests.Add(new JoinRequest
                {
                    CommunityId = id,
                    UserName = userName,
                    RequestedDate = DateTime.Now
                });
                await _context.SaveChangesAsync();
            }
            else
            {
                TempData["CommunityWarning"] = "Bu topluluk için bekleyen bir katılma isteğiniz var.";
                return RedirectToAction(nameof(Index));
            }

            TempData["CommunitySuccess"] = "Katılma isteğiniz yönetime gönderildi.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelJoinRequest(int id)
        {
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            var userName = User.Identity?.Name ?? string.Empty;
            var request = await _context.JoinRequests.FirstOrDefaultAsync(r =>
                r.Id == id &&
                r.UserName == userName &&
                !r.IsApproved &&
                !r.IsRejected);

            if (request == null)
            {
                TempData["CommunityWarning"] = "Silinebilecek bekleyen bir katılma isteği bulunamadı.";
                return RedirectToAction("UserPanel", "Dashboard");
            }

            _context.JoinRequests.Remove(request);
            await _context.SaveChangesAsync();

            TempData["CommunitySuccess"] = "Katılma isteğiniz silindi.";
            return RedirectToAction("UserPanel", "Dashboard");
        }

        private bool CommunityExists(int id)
        {
            return _context.Communities.Any(e => e.Id == id);
        }

        private async Task<string?> SaveCommunityCoverAsync(IFormFile? coverImage)
        {
            if (coverImage == null || coverImage.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "communities");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(coverImage.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await coverImage.CopyToAsync(stream);

            return $"/uploads/communities/{fileName}";
        }
    }
}
