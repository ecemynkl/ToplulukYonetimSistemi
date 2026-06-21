using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MembersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public MembersController(AppDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            return View(await _context.Members
                .Include(m => m.MemberCommunities)
                .ThenInclude(mc => mc.Community)
                .ToListAsync());
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.MemberCommunities)
                .ThenInclude(mc => mc.Community)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            var communityIds = member.MemberCommunities.Select(mc => mc.CommunityId).ToList();
            ViewBag.MemberEvents = await _context.Events
                .Include(e => e.Community)
                .Where(e => communityIds.Contains(e.CommunityId))
                .OrderByDescending(e => e.EventDate)
                .Take(6)
                .ToListAsync();

            return View(member);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context).GetAwaiter().GetResult();
            DatabaseRepair.EnsureMediaColumnsAsync(_context).GetAwaiter().GetResult();

            var model = new MemberCreateViewModel
            {
                Communities = GetCommunitySelectList()
            };

            return View(model);
        }

        // POST: Members/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberCreateViewModel model, IFormFile? profileImage)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (ModelState.IsValid)
            {
                var member = new Member
                {
                    FullName = model.FullName,
                    StudentNumber = model.StudentNumber,
                    Email = model.Email,
                    Phone = model.Phone,
                    Department = model.Department,
                    ProfileImagePath = await SaveMemberProfileImageAsync(profileImage),
                    RegisteredDate = DateTime.Today
                };

                _context.Members.Add(member);
                await _context.SaveChangesAsync();

                foreach (var communityId in model.SelectedCommunityIds.Distinct())
                {
                    _context.MemberCommunities.Add(new MemberCommunity
                    {
                        MemberId = member.Id,
                        CommunityId = communityId
                    });
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            model.Communities = GetCommunitySelectList(model.SelectedCommunityIds);

            return View(model);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.MemberCommunities)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            var model = new MemberCreateViewModel
            {
                Id = member.Id,
                FullName = member.FullName,
                StudentNumber = member.StudentNumber,
                Email = member.Email,
                Phone = member.Phone,
                Department = member.Department,
                ProfileImagePath = member.ProfileImagePath,
                RegisteredDate = member.RegisteredDate,
                SelectedCommunityIds = member.MemberCommunities.Select(mc => mc.CommunityId).ToList()
            };
            model.Communities = GetCommunitySelectList(model.SelectedCommunityIds);

            return View(model);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, MemberCreateViewModel model, IFormFile? profileImage)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (id != model.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var member = await _context.Members
                    .Include(m => m.MemberCommunities)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (member == null)
                {
                    return NotFound();
                }

                member.FullName = model.FullName;
                member.StudentNumber = model.StudentNumber;
                member.Email = model.Email;
                member.Phone = model.Phone;
                member.Department = model.Department;
                var uploadedImagePath = await SaveMemberProfileImageAsync(profileImage);
                if (!string.IsNullOrWhiteSpace(uploadedImagePath))
                {
                    member.ProfileImagePath = uploadedImagePath;
                }

                if (member.RegisteredDate == default)
                {
                    member.RegisteredDate = DateTime.Today;
                }

                var selectedIds = model.SelectedCommunityIds.Distinct().ToHashSet();
                var existingIds = member.MemberCommunities.Select(mc => mc.CommunityId).ToHashSet();

                var removedCommunities = member.MemberCommunities
                    .Where(mc => !selectedIds.Contains(mc.CommunityId))
                    .ToList();
                _context.MemberCommunities.RemoveRange(removedCommunities);

                foreach (var communityId in selectedIds.Except(existingIds))
                {
                    _context.MemberCommunities.Add(new MemberCommunity
                    {
                        MemberId = member.Id,
                        CommunityId = communityId
                    });
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
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

            model.Communities = GetCommunitySelectList(model.SelectedCommunityIds);
            return View(model);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .Include(m => m.MemberCommunities)
                .ThenInclude(mc => mc.Community)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await DatabaseRepair.EnsureMemberCommunitySchemaAsync(_context);
            await DatabaseRepair.EnsureMediaColumnsAsync(_context);

            var member = await _context.Members
                .Include(m => m.MemberCommunities)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (member != null)
            {
                _context.MemberCommunities.RemoveRange(member.MemberCommunities);
                _context.Members.Remove(member);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }

        private List<SelectListItem> GetCommunitySelectList(IEnumerable<int>? selectedIds = null)
        {
            var selectedIdSet = selectedIds?.ToHashSet() ?? new HashSet<int>();

            return _context.Communities
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name ?? "Adsız Topluluk",
                    Selected = selectedIdSet.Contains(c.Id)
                })
                .ToList();
        }

        private async Task<string?> SaveMemberProfileImageAsync(IFormFile? profileImage)
        {
            if (profileImage == null || profileImage.Length == 0)
            {
                return null;
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "members");
            Directory.CreateDirectory(uploadsFolder);

            var extension = Path.GetExtension(profileImage.FileName);
            var fileName = $"{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using var stream = System.IO.File.Create(filePath);
            await profileImage.CopyToAsync(stream);

            return $"/uploads/members/{fileName}";
        }
    }
}
