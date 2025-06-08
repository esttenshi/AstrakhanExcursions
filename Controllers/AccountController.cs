using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AstrakhanExcursions.Data;
using AstrakhanExcursions.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;

namespace AstrakhanExcursions.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == username);

            if (user == null)
            {
                ViewData["Message"] = "Пользователь с таким email не существует";
                ViewData["Email"] = username;
                return View();
            }

            if (user.PasswordHash != password) 
            {
                ViewData["Message"] = "Неверный пароль";
                ViewData["Email"] = username;
                return View();
            }

            return RedirectToAction("PersonalAccount", "User", new { userId = user.UserId });
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string email, string password, string confirmPassword)
        {

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingUser != null)
            {
                ViewData["Message"] = "Пользователь с таким email уже существует.";
                ViewData["Email"] = email;
                return View();
            }

            var user = new User { Email = email, PasswordHash = password }; 
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToAction("Login", "Account"); 
        }
    }
}
