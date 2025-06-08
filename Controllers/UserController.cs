using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AstrakhanExcursions.Data;

namespace AstrakhanExcursions.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult PersonalAccount(int userId)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

            if (user == null)
            {
                return NotFound();
            }

            ViewData["User"] = user;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> EditEmail(int userId, string currentPassword, string newEmail)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.PasswordHash != currentPassword)
            {
                ViewData["Message"] = "Неверный пароль";
                ViewData["OpenModal"] = "EditEmail";
                ViewData["NewEmail"] = newEmail;
                ViewData["User"] = user;
                return View("PersonalAccount");
            }

            var emailExists = await _context.Users.AnyAsync(u => u.Email == newEmail && u.UserId != userId);
            if (emailExists)
            {
                ViewData["Message"] = "Пользователь с таким email уже существует";
                ViewData["OpenModal"] = "EditEmail";
                ViewData["NewEmail"] = newEmail;
                ViewData["User"] = user;
                return View("PersonalAccount");
            }

            user.Email = newEmail;
            await _context.SaveChangesAsync();

            ViewData["Message"] = "Почта успешно изменена!";
            ViewData["User"] = user;
            return View("PersonalAccount");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int userId, string currentPassword, string newPassword)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.PasswordHash != currentPassword)
            {
                ViewData["Message"] = "Неверный текущий пароль";
                ViewData["OpenModal"] = "ChangePassword";
                ViewData["User"] = user;
                return View("PersonalAccount");
            }

            user.PasswordHash = newPassword;
            await _context.SaveChangesAsync();

            ViewData["Message"] = "Пароль успешно изменён!";
            ViewData["User"] = user;
            return View("PersonalAccount");
        }
    }
}
