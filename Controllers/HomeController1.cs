using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    [Authorize]
    public class ContactController : Controller
    {
        private readonly AppDbContext _context;

        public ContactController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var message = await BuildContactMessageAsync();
            return View(message);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ContactMessage contactMessage)
        {
            await DatabaseRepair.EnsureContactMessageSchemaAsync(_context);

            var currentMemberMessage = await BuildContactMessageAsync();
            contactMessage.FullName = currentMemberMessage.FullName;
            contactMessage.Email = currentMemberMessage.Email;

            ModelState.Remove(nameof(ContactMessage.FullName));
            ModelState.Remove(nameof(ContactMessage.Email));

            if (!ModelState.IsValid)
            {
                return View(contactMessage);
            }

            contactMessage.SentDate = DateTime.Now;
            contactMessage.IsRead = false;

            _context.ContactMessages.Add(contactMessage);
            await _context.SaveChangesAsync();

            TempData["ContactSuccess"] = "Mesajınız başarıyla gönderildi.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<ContactMessage> BuildContactMessageAsync()
        {
            var studentNumber = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var member = string.IsNullOrWhiteSpace(studentNumber)
                ? null
                : await _context.Members.FirstOrDefaultAsync(m => m.StudentNumber == studentNumber);

            return new ContactMessage
            {
                FullName = member?.FullName ?? User.Identity?.Name ?? "Kullanıcı",
                Email = member?.Email ?? $"{studentNumber ?? "ogrenci"}@ogrenci.local"
            };
        }
    }
}
