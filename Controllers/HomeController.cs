using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeIndexViewModel();

            try
            {
                model.CommunityCount = await _context.Communities.CountAsync();
                model.MemberCount = await _context.Members.CountAsync();
                model.UpcomingEventCount = await _context.Events
                    .CountAsync(e => e.EventDate >= DateTime.Today);
                await DatabaseRepair.EnsureAnnouncementMediaSchemaAsync(_context);
                model.Announcements = await _context.Announcements
                    .Include(a => a.Community)
                    .OrderByDescending(a => a.CreatedDate)
                    .Take(3)
                    .ToListAsync();
            }
            catch
            {
                model.Announcements = new List<Announcement>();
            }

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
