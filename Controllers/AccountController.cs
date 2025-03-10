using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using TravelApp.Models;
using TravelApp.Data;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Antiforgery;
using System.Text.RegularExpressions;


namespace TravelApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AccountController(ApplicationDbContext context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || 
                _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password) == PasswordVerificationResult.Failed)
            {
                ViewBag.ErrorMessage = "Nieprawidłowy e-mail lub hasło";
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, 
                new ClaimsPrincipal(claimsIdentity)).Wait();

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string username, string email, string password, string role = "User")
        {
            if (_context.Users.Any(u => u.Username == username))
            {
                ViewBag.ErrorMessage = "Nazwa użytkownika już istnieje";
                ViewData["Username"] = username;
                ViewData["Email"] = email;
                return View();
            }

            if (_context.Users.Any(u => u.Email == email))
            {
                ViewBag.ErrorMessage = "E-mail już istnieje";
                ViewData["Username"] = username;
                ViewData["Email"] = email;
                return View();
            }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ViewBag.ErrorMessage = "Nieprawidłowy format adresu e-mail.";
                ViewData["Username"] = username;
                ViewData["Email"] = email;
                return View();
            }

            if (!IsValidPassword(password))
            {
                ViewBag.ErrorMessage = "Hasło musi zawierać co najmniej 8 znaków, w tym jedną dużą literę, jedną małą literę, cyfrę i znak specjalny.";
                return View();
            }

            var user = new User
            {
                Username = username,
                Email = email,
                Role = role
            };

            user.PasswordHash = _passwordHasher.HashPassword(user, password);

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        private bool IsValidPassword(string password)
        {
            return password.Length >= 8 &&
                Regex.IsMatch(password, @"[A-Z]") && 
                Regex.IsMatch(password, @"[a-z]") && 
                Regex.IsMatch(password, @"\d") &&   
                Regex.IsMatch(password, @"[\W_]"); 
        }

        public IActionResult Logout()
        {
            HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).Wait(); // Wylogowanie użytkownika
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public IActionResult Profile()
        {
            ViewData["ActiveNavItem"] = "Profile";

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
            {
                return NotFound();
            }

            ViewData["FollowersCount"] = _context.Follows.Count(f => f.FollowingId == user.UserId);
            ViewData["FollowingCount"] = _context.Follows.Count(f => f.FollowerId == user.UserId);

            return View(user);
        }



        [HttpPost]
        [Authorize]
        public IActionResult UpdateProfile(IFormFile avatar, string username, string? bio)
        {
            var usernameLoggedIn = User.Identity?.Name;
            if (string.IsNullOrEmpty(usernameLoggedIn))
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == usernameLoggedIn);
            if (user == null) return NotFound();

            if (avatar != null)
            {
                var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatar.FileName)}";
                var filePath = Path.Combine(uploadsPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    avatar.CopyTo(stream);
                }

                user.AvatarPath = fileName;
            }

            var oldUsername = user.Username;
            user.Username = username;

            user.Bio = bio;

            _context.SaveChanges();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var newPrincipal = new ClaimsPrincipal(claimsIdentity);
            HttpContext.User = newPrincipal;
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, newPrincipal).Wait();

            return RedirectToAction("Profile", new { username = user.Username });

        }

        public IActionResult UserProfile(int userId)
        {
            var nameIdentifierClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (nameIdentifierClaim == null || string.IsNullOrEmpty(nameIdentifierClaim.Value))
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(nameIdentifierClaim.Value);

            var user = _context.Users.Find(userId);

            if (user == null)
            {
                return NotFound();
            }

            var followersCount = _context.Follows.Count(f => f.FollowingId == userId);
            var followingCount = _context.Follows.Count(f => f.FollowerId == userId);

            var isFollowing = _context.Follows.Any(f => f.FollowerId == currentUserId && f.FollowingId == userId);

            ViewData["IsFollowing"] = _context.Follows
                .Any(f => f.FollowerId == currentUserId && f.FollowingId == userId);

            ViewData["FollowersCount"] = _context.Follows.Count(f => f.FollowingId == userId);
            ViewData["FollowingCount"] = _context.Follows.Count(f => f.FollowerId == userId);
            
            ViewData["Followers"] = user.FollowersUsers;
            ViewData["Following"] = user.FollowingUsers;
            
            return View(user);
        }

        public class FollowRequest
        {
            public int UserId { get; set; }
        }

        [HttpPost]
        [Authorize]
        public IActionResult ToggleFollow([FromBody] FollowRequest request)
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var currentUserId = int.Parse(userIdClaim);
            var userId = request.UserId;

            Console.WriteLine($"ToggleFollow: currentUserId={currentUserId}, userId={userId}");

            var existingFollow = _context.Follows
                .FirstOrDefault(f => f.FollowerId == currentUserId && f.FollowingId == userId);

            if (existingFollow != null)
            {
                Console.WriteLine($"Removing follow: FollowerId={currentUserId}, FollowingId={userId}");
                _context.Follows.Remove(existingFollow);
            }
            else
            {
                Console.WriteLine($"Adding follow: FollowerId={currentUserId}, FollowingId={userId}");
                var newFollow = new Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = userId
                };

                Console.WriteLine($"Before Save: FollowerId={newFollow.FollowerId}, FollowingId={newFollow.FollowingId}");

                _context.Follows.Add(newFollow);
            }

            _context.SaveChanges();

            Console.WriteLine("Saved changes to database.");

            var followersCount = _context.Follows.Count(f => f.FollowingId == userId);
            Console.WriteLine($"Followers count after toggle: {followersCount}");
            return Json(new { followersCount });
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        [Authorize]
        public IActionResult Followers(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            var followers = _context.Follows
                .Where(f => f.FollowingId == userId)
                .Select(f => f.Follower)
                .ToList();

            ViewData["Title"] = $"Obserwujący {user.Username}";
            return View("FollowersList", followers);
        }

        [Authorize]
        public IActionResult Following(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            var following = _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.Following)
                .ToList();

            ViewData["Title"] = $"Obserwowani przez {user.Username}";
            return View("FollowersList", following);
        }

        [HttpGet]
        public IActionResult GetFollowStatus(int userId)
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Json(new { isFollowing = false });
            }

            var currentUserId = int.Parse(userIdClaim);
            bool isFollowing = _context.Follows.Any(f => f.FollowerId == currentUserId && f.FollowingId == userId);

            return Json(new { isFollowing });
        }

    }
}
