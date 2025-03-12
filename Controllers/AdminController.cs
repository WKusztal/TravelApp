using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelApp.Data;
using TravelApp.Models;
using Microsoft.EntityFrameworkCore;

namespace TravelApp.Controllers
{
    [Authorize(Roles = "Admin,Moderator")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult ManageUsers()
        {
            ViewData["ActiveNavItem"] = "ManageUsers";

            var users = _context.Users.ToList();
            return View(users);
        }

        public IActionResult EditUser(int id)
        {
            var user = _context.Users.Find(id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        public IActionResult EditUser(User updatedUser)
        {
            var user = _context.Users.Find(updatedUser.UserId);
            if (user != null)
            {
                user.Username = updatedUser.Username;
                user.Email = updatedUser.Email;
                user.Role = updatedUser.Role;
                _context.SaveChanges();
            }
            return RedirectToAction("ManageUsers");
        }

        [HttpPost]
        public IActionResult UpdateUserRole(int id, string role)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return NotFound();
            }

            user.Role = role;
            _context.SaveChanges();

            return RedirectToAction("ManageUsers");
        }

        public IActionResult DeleteUser(int id)
        {
            var user = _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Following)
                .Include(u => u.Reports)
                .FirstOrDefault(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            _context.Follows.RemoveRange(user.Followers);
            _context.Follows.RemoveRange(user.Following);
            _context.Reports.RemoveRange(user.Reports);

            _context.Users.Remove(user);
            _context.SaveChanges();
            
            return RedirectToAction("ManageUsers");
        }

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult UsersReports()
        {
            ViewData["ActiveNavItem"] = "UsersReports";

            var reports = _context.Reports.Include(r => r.User).ToList();
            return View(reports);
        }   

        [Authorize(Roles = "Admin,Moderator")]
        public IActionResult DeleteReport(int id)
        {
            var report = _context.Reports.Find(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                _context.SaveChanges();
            }
            return RedirectToAction("UsersReports");
        }
    }
}
