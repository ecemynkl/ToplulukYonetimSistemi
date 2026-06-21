using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToplulukYonetimSistemi.Data;
using ToplulukYonetimSistemi.Models;

namespace ToplulukYonetimSistemi.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;

        public AccountController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var claims = new List<Claim>();
            var isAdminLogin = string.Equals(model.LoginType, "Admin", StringComparison.OrdinalIgnoreCase);

            if (isAdminLogin)
            {
                if (!string.Equals(model.UserName, "admin", StringComparison.OrdinalIgnoreCase) || model.Password != "admin123")
                {
                    ModelState.AddModelError(string.Empty, "Yönetici kullanıcı adı veya şifre hatalı.");
                    return View(model);
                }

                claims.Add(new Claim(ClaimTypes.Name, "admin"));
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            else
            {
                await DatabaseRepair.EnsureMediaColumnsAsync(_context);

                var member = await _context.Members.FirstOrDefaultAsync(m =>
                    m.StudentNumber == model.UserName &&
                    m.Phone == model.Password);

                if (member == null)
                {
                    ModelState.AddModelError(string.Empty, "Kullanıcı adı veya şifre hatalı.");
                    return View(model);
                }

                claims.Add(new Claim(ClaimTypes.Name, member.FullName ?? member.StudentNumber ?? "Öğrenci"));
                claims.Add(new Claim(ClaimTypes.NameIdentifier, member.StudentNumber ?? string.Empty));
                claims.Add(new Claim(ClaimTypes.Role, "Kullanici"));
                claims.Add(new Claim("MemberId", member.Id.ToString()));

                if (!string.IsNullOrWhiteSpace(member.Email))
                {
                    claims.Add(new Claim(ClaimTypes.Email, member.Email));
                }
            }

            var identity = new ClaimsIdentity(claims, "ToplulukCookie");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("ToplulukCookie", principal);

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("ToplulukCookie");
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
