using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    [Authorize]
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public AnnouncementsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            await DatabaseRepair.EnsureAnnouncementMediaSchemaAsync(_context);

            var announcements = await _context.Announcements
                .Include(a => a.Community)
                .OrderByDescending(a => a.CreatedDate)
                .ToListAsync();

            return View(announcements);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            FillCommunities();
            return View(new Announcement { CreatedDate = DateTime.Today });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Content,CreatedDate,CommunityId")] Announcement announcement, IFormFile? coverImage)
        {
            await DatabaseRepair.EnsureAnnouncementMediaSchemaAsync(_context);

            if (ModelState.IsValid)
            {
                if (announcement.CreatedDate == default)
                {
                    announcement.CreatedDate = DateTime.Today;
                }

                announcement.CoverImagePath = await SaveAnnouncementCoverAsync(coverImage);
                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            FillCommunities(announcement.CommunityId);
            return View(announcement);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            await DatabaseRepair.EnsureAnnouncementMediaSchemaAsync(_context);

            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            FillCommunities(announcement.CommunityId);
            return View(announcement);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,CreatedDate,CommunityId")] Announcement announcement, IFormFile? coverImage)
        {
            await DatabaseRepair.EnsureAnnouncementMediaSchemaAsync(_context);

            if (id != announcement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingAnnouncement = await _context.Announcements.FindAsync(id);
                if (existingAnnouncement == null)
                {
                    return NotFound();
                }

                existingAnnouncement.Title = announcement.Title;
                existingAnnouncement.Content = announcement.Content;
                existingAnnouncement.CreatedDate = announcement.CreatedDate == default
                    ? existingAnnouncement.CreatedDate
                    : announcement.CreatedDate;
                existingAnnouncement.CommunityId = announcement.CommunityId;

                var uploadedPath = await SaveAnnouncementCoverAsync(coverImage);
                if (!string.IsNullOrWhiteSpace(uploadedPath))
                {
                    existingAnnouncement.CoverImagePath = uploadedPath;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            FillCommunities(announcement.CommunityId);
            return View(announcement);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements
                .Include(a => a.Community)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement != null)
            {
                _context.Announcements.Remove(announcement);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private void FillCommunities(int? selectedCommunityId = null)
        {
            ViewData["CommunityId"] = new SelectList(
                _context.Communities.OrderBy(c => c.Name),
                "Id",
                "Name",
                selectedCommunityId);
        }

        private async Task<string?> SaveAnnouncementCoverAsync(IFormFile? coverImage)
        {
            if (coverImage == null || coverImage.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "announcements");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(coverImage.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await coverImage.CopyToAsync(stream);

            return $"/uploads/announcements/{fileName}";
        }
    }
}
