using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    [Authorize]
    public class CommunityEventsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public CommunityEventsController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public async Task<IActionResult> Index(int? communityId, bool upcomingOnly = false)
        {
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            var eventsQuery = _context.Events
                .Include(c => c.Community)
                .AsQueryable();

            if (communityId.HasValue)
            {
                eventsQuery = eventsQuery.Where(e => e.CommunityId == communityId.Value);
                ViewBag.CommunityName = await _context.Communities
                    .Where(c => c.Id == communityId.Value)
                    .Select(c => c.Name)
                    .FirstOrDefaultAsync();
            }

            if (upcomingOnly)
            {
                eventsQuery = eventsQuery.Where(e => e.EventDate >= DateTime.Today);
            }

            ViewBag.UpcomingOnly = upcomingOnly;
            ViewBag.CommunityId = communityId;
            var communityOptions = await _context.Communities
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name ?? "Adsız Topluluk",
                    Selected = communityId.HasValue && c.Id == communityId.Value
                })
                .ToListAsync();

            communityOptions.Insert(0, new SelectListItem
            {
                Value = "",
                Text = "Tüm Topluluklar",
                Selected = !communityId.HasValue
            });

            ViewBag.CommunityOptions = communityOptions;

            return View(await eventsQuery.OrderBy(e => e.EventDate).ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            if (id == null)
            {
                return NotFound();
            }

            var communityEvent = await _context.Events
                .Include(c => c.Community)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (communityEvent == null)
            {
                return NotFound();
            }

            return View(communityEvent);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["CommunityId"] = new SelectList(_context.Communities, "Id", "Name");
            return View(new CommunityEvent { EventDate = DateTime.Today });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,EventDate,CommunityId")] CommunityEvent communityEvent, IFormFile? coverImage)
        {
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            if (ModelState.IsValid)
            {
                communityEvent.CoverImagePath = await SaveEventCoverAsync(coverImage);
                _context.Add(communityEvent);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CommunityId"] = new SelectList(_context.Communities, "Id", "Name", communityEvent.CommunityId);
            return View(communityEvent);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            if (id == null)
            {
                return NotFound();
            }

            var communityEvent = await _context.Events.FindAsync(id);
            if (communityEvent == null)
            {
                return NotFound();
            }

            ViewData["CommunityId"] = new SelectList(_context.Communities, "Id", "Name", communityEvent.CommunityId);
            return View(communityEvent);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,EventDate,CommunityId")] CommunityEvent communityEvent, IFormFile? coverImage)
        {
            await DatabaseRepair.EnsureEventMediaAndJoinRequestsSchemaAsync(_context);

            if (id != communityEvent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var existingEvent = await _context.Events.FindAsync(id);
                if (existingEvent == null)
                {
                    return NotFound();
                }

                existingEvent.Title = communityEvent.Title;
                existingEvent.Description = communityEvent.Description;
                existingEvent.EventDate = communityEvent.EventDate;
                existingEvent.CommunityId = communityEvent.CommunityId;

                var uploadedPath = await SaveEventCoverAsync(coverImage);
                if (!string.IsNullOrWhiteSpace(uploadedPath))
                {
                    existingEvent.CoverImagePath = uploadedPath;
                }

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["CommunityId"] = new SelectList(_context.Communities, "Id", "Name", communityEvent.CommunityId);
            return View(communityEvent);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var communityEvent = await _context.Events
                .Include(c => c.Community)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (communityEvent == null)
            {
                return NotFound();
            }

            return View(communityEvent);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var communityEvent = await _context.Events.FindAsync(id);
            if (communityEvent != null)
            {
                _context.Events.Remove(communityEvent);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private async Task<string?> SaveEventCoverAsync(IFormFile? coverImage)
        {
            if (coverImage == null || coverImage.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "events");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(coverImage.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await coverImage.CopyToAsync(stream);

            return $"/uploads/events/{fileName}";
        }
    }
}
