using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelApp.Data;
using TravelApp.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace TravelApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly IWebHostEnvironment _hostingEnvironment;

        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<HomeController> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public IActionResult Index()
        {
            ViewData["ActiveNavItem"] = "Home";

            return View();
        }

        public IActionResult About()
        {
            ViewData["ActiveNavItem"] = "About";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["ActiveNavItem"] = "Contact";
            
            return View();
        }

        [Authorize]
        public IActionResult Stories(string? tag = null, string? filterValue = null, bool clearFilters = false, string? sortBy = null, int? userId = null)
        {
            ViewData["ActiveNavItem"] = "Stories";

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int? loggedInUserId = string.IsNullOrEmpty(currentUserId) ? (int?)null : int.Parse(currentUserId);

            ViewData["CurrentUserId"] = loggedInUserId;

            var availableTags = _context.Tags.Select(t => t.Name).ToList();
            ViewData["AvailableTags"] = availableTags;
            ViewData["SelectedTag"] = tag;

            if (clearFilters)
            {
                filterValue = null;
                tag = null;
            }

            var storiesQuery = _context.Stories
                .Include(s => s.Author) 
                .Include(s => s.Images) 
                .Include(s => s.Tags) 
                .AsQueryable();

            if (!string.IsNullOrEmpty(tag))
            {
                storiesQuery = storiesQuery.Where(s => s.Tags.Any(t => t.Name.ToLower() == tag.ToLower()));
            }
            else if (!string.IsNullOrEmpty(filterValue))
            {
                switch (filterValue.ToLower())
                {
                    case "followingstories":
                        if (loggedInUserId.HasValue)
                        {
                            var followingIds = _context.Follows
                                .Where(f => f.FollowerId == loggedInUserId.Value)
                                .Select(f => f.FollowingId)
                                .ToList();

                            storiesQuery = storiesQuery.Where(s => followingIds.Contains(s.UserId));
                        }
                        break;

                    default:
                        break;
                }
            }

            if (userId.HasValue)
            {
                storiesQuery = storiesQuery.Where(s => s.UserId == userId.Value);
            }

            switch (sortBy?.ToLower())
            {
                case "newest":
                    storiesQuery = storiesQuery.OrderByDescending(s => s.CreatedAt);
                    break;
                case "oldest":
                    storiesQuery = storiesQuery.OrderBy(s => s.CreatedAt);
                    break;
                case "best":
                    storiesQuery = storiesQuery.OrderByDescending(s => s.Likes - s.Dislikes)
                                            .ThenByDescending(s => s.CreatedAt);
                    break;
                case "worst":
                    storiesQuery = storiesQuery.OrderBy(s => s.Likes - s.Dislikes)
                                            .ThenByDescending(s => s.CreatedAt);
                    break;
                default:
                    storiesQuery = storiesQuery.OrderByDescending(s => s.CreatedAt);
                    break;
            }

            var stories = storiesQuery.ToList();
            return View(stories);
        }

        [Authorize]
        public IActionResult AddStory()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        public IActionResult AddStory(Story story, List<IFormFile> images, String Tags)
        {
            if (!ModelState.IsValid)
                return View(story);

            if (User.Identity?.Name == null)
                return Unauthorized();

            var user = _context.Users.FirstOrDefault(u => u.Username == User.Identity.Name);
            if (user == null)
                return Unauthorized();

            story.UserId = user.UserId;
            story.Author = user;

            var tagNames = System.Text.Json.JsonSerializer.Deserialize<List<string>>(Tags) ?? new List<string>();
            var tags = tagNames.Select(tagName =>
            {
                var existingTag = _context.Tags.FirstOrDefault(t => t.Name == tagName);
                if (existingTag != null) return existingTag;

                return new Tag { Name = tagName };
            }).ToList();
            story.Tags = tags;
 
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            if (images != null && images.Any())
            {
                story.Images = new List<StoryImage>();
                foreach (var image in images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    if (story.Images == null)
                    {
                        story.Images = new List<StoryImage>();
                    }

                    if (story.Images.Any(img => img.ImagePath.EndsWith(fileName)))
                    {
                        continue;
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    story.Images.Add(new StoryImage { ImagePath = $"/uploads/{fileName}" });
                }
            }

            _context.Stories.Add(story);
            _context.SaveChanges();

            return RedirectToAction("Stories");
        }

        [HttpGet]
        public IActionResult StoryDetails(int id)
        {
            ViewData["IsAdminOrModerator"] = User.IsInRole("Admin") || User.IsInRole("Moderator");

            var story = _context.Stories
                                .Include(s => s.Images) 
                                .Include(s => s.Author) 
                                .Include(s => s.Tags) 
                                .Include(s => s.Comments)
                                .ThenInclude(c => c.User)
                                .FirstOrDefault(s => s.StoryId == id);

            if (story == null)
            {
                return NotFound(); 
            }

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            ViewData["CurrentUserId"] = string.IsNullOrEmpty(currentUserId) ? (int?)null : int.Parse(currentUserId);

            return View(story);
        }


        private int GetCurrentUserId()
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(currentUserId))
            {
                Console.WriteLine("[GetCurrentUserId] User is not authenticated.");
                throw new InvalidOperationException("User is not authenticated.");
            }

            if (!int.TryParse(currentUserId, out var userId))
            {
                Console.WriteLine("[GetCurrentUserId] Failed to parse user ID.");
                throw new InvalidOperationException("Invalid user ID format.");
            }

            return userId;
        }

        [HttpGet]
        public IActionResult EditStory(int StoryId)
        {
            var story = _context.Stories
            .Include(s => s.Author)
            .Include(s => s.Images)
            .Include(s => s.Tags)
            .FirstOrDefault(s => s.StoryId == StoryId);


            if (story == null || story.Author == null || story.Author.UserId != GetCurrentUserId())
            {
                return NotFound();
            }

            return View("EditStory", story);
        }

        [HttpPost]
        public IActionResult EditStory(int storyId, Story updatedStory, List<IFormFile> images, List<string> ImagesToRemove, String Tags)
        {
            var story = _context.Stories
            .Include(s => s.Images)
            .Include(s => s.Tags)
            .FirstOrDefault(s => s.StoryId == storyId);

            if (story == null)
                return NotFound();

            story.Title = updatedStory.Title;
            story.Description = updatedStory.Description;
            story.Continent = updatedStory.Continent;

            if (ImagesToRemove != null && ImagesToRemove.Any())
            {
                foreach (var imagePath in ImagesToRemove)
                {
                    if (story.Images != null) 
                    {
                        var image = story.Images.FirstOrDefault(img => img.ImagePath == imagePath);
                        if (image != null)
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }

                            story.Images.Remove(image);
                        }
                    }
                }
            }

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            if (images != null && images.Any())
            {
                foreach (var image in images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    if (story.Images == null)
                    {
                        story.Images = new List<StoryImage>();
                    }

                    if (story.Images.Any(img => img.ImagePath.EndsWith(fileName)))
                    {
                        continue;
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    story.Images.Add(new StoryImage { ImagePath = $"/uploads/{fileName}" });
                }

            }

            if (!string.IsNullOrWhiteSpace(Tags))
            {
                var tagNames = Tags.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).Distinct().ToList();

                var existingTags = _context.Tags.Where(t => tagNames.Contains(t.Name)).ToList();

                foreach (var tagName in tagNames)
                {
                    if (!existingTags.Any(t => t.Name == tagName))
                    {
                        var newTag = new Tag { Name = tagName };
                        _context.Tags.Add(newTag);
                        existingTags.Add(newTag);
                    }
                }

                story.Tags.Clear();
                foreach (var tag in existingTags)
                {
                    if (!story.Tags.Any(st => st.Name == tag.Name))
                    {
                        story.Tags.Add(tag);
                    }
                }
            }
            else
            {
                story.Tags.Clear();
            }

            _context.SaveChanges();

            return RedirectToAction("StoryDetails", new { id = story.StoryId });
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStory(int storyId)
        {
            _logger.LogInformation($"[DeleteStory] Próba usunięcia relacji o ID: {storyId}");

            if (storyId <= 0)
            {
                _logger.LogWarning("[DeleteStory] Nieprawidłowy ID relacji.");
                return BadRequest("Nieprawidłowy ID relacji.");
            }

            var story = _context.Stories.FirstOrDefault(s => s.StoryId == storyId);

            if (story == null)
            {
                _logger.LogWarning($"[DeleteStory] Relacja o ID {storyId} nie została znaleziona.");
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(story.ImagePath))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", story.ImagePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        _logger.LogInformation($"[DeleteStory] Usuwanie pliku: {filePath}");
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Stories.Remove(story);
                _context.SaveChanges();

                _logger.LogInformation($"[DeleteStory] Relacja o ID {storyId} została pomyślnie usunięta.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DeleteStory] Błąd podczas usuwania relacji o ID {storyId}: {ex.Message}");
                return StatusCode(500, "Wystąpił błąd podczas usuwania relacji.");
            }

            return RedirectToAction("Stories");
        }

        public class DeleteImageRequest
        {
            public int ImageId { get; set; }
        }

        [HttpPost]
        public IActionResult DeleteImage([FromBody] DeleteImageRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { success = false, message = "Nieprawidłowe dane wejściowe." });
            }

            if (request == null || request.ImageId <= 0)
            {
                return BadRequest(new { success = false, message = "Nieprawidłowy imageId." });
            }

            var image = _context.StoryImages.FirstOrDefault(i => i.Id == request.ImageId);
            if (image == null)
            {
                return NotFound(new { success = false, message = "Obraz nie został znaleziony." });
            }

            _context.StoryImages.Remove(image);
            _context.SaveChanges();

            return Ok(new { success = true });
        }

        public class LikeStoryRequest
        {
            public int StoryId { get; set; }
        }

        [HttpPost]
        [Authorize]
        public IActionResult LikeStory([FromBody] LikeStoryRequest request)
        {
            if (request == null || request.StoryId <= 0)
            {
                return BadRequest(new { success = false, message = "Nieprawidłowe ID relacji." });
            }

            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized(new { success = false, message = "Użytkownik nie jest zalogowany." });
            }

            var story = _context.Stories.FirstOrDefault(s => s.StoryId == request.StoryId);
            if (story == null)
            {
                return NotFound(new { success = false, message = "Relacja nie została znaleziona." });
            }

            var userReaction = _context.Reactions.FirstOrDefault(r => r.StoryId == story.StoryId && r.UserName == currentUser);

            if (userReaction == null)
            {
                userReaction = new Reaction { StoryId = story.StoryId, UserName = currentUser, IsLike = true };
                _context.Reactions.Add(userReaction);
                story.Likes++;
            }
            else
            {
                if (userReaction.IsLike)
                {
                    _context.Reactions.Remove(userReaction);
                    story.Likes--;
                }
                else
                {
                    userReaction.IsLike = true;
                    story.Likes++;
                    story.Dislikes--;
                }
            }

            _context.SaveChanges();

            return Ok(new { success = true, likes = story.Likes, dislikes = story.Dislikes });
        }

        [HttpPost]
        [Authorize]
        public IActionResult DislikeStory([FromBody] LikeStoryRequest request)
        {
            if (request == null || request.StoryId <= 0)
            {
                return BadRequest(new { success = false, message = "Nieprawidłowe ID relacji." });
            }

            var currentUser = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUser))
            {
                return Unauthorized(new { success = false, message = "Użytkownik nie jest zalogowany." });
            }

            var story = _context.Stories.FirstOrDefault(s => s.StoryId == request.StoryId);
            if (story == null)
            {
                return NotFound(new { success = false, message = "Relacja nie została znaleziona." });
            }

            var userReaction = _context.Reactions.FirstOrDefault(r => r.StoryId == story.StoryId && r.UserName == currentUser);

            if (userReaction == null)
            {
                userReaction = new Reaction { StoryId = story.StoryId, UserName = currentUser, IsLike = false };
                _context.Reactions.Add(userReaction);
                story.Dislikes++;
            }
            else
            {
                if (!userReaction.IsLike)
                {
                    _context.Reactions.Remove(userReaction);
                    story.Dislikes--;
                }
                else
                {
                    userReaction.IsLike = false;
                    story.Likes--;
                    story.Dislikes++;
                }
            }

            _context.SaveChanges();

            return Ok(new { success = true, likes = story.Likes, dislikes = story.Dislikes });
        }

        [HttpPost]
        public IActionResult ReportStory(int StoryId, string Reason)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Json(new { success = false, message = "Użytkownik nie jest zalogowany." });
            }

            int userId = int.Parse(userIdString);

            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null)
            {
                return Json(new { success = false, message = "Nie znaleziono użytkownika." });
            }

            var story = _context.Stories.FirstOrDefault(s => s.StoryId == StoryId);
            if (story == null)
            {
                return Json(new { success = false, message = "Nie znaleziono relacji." });
            }

            try
            {
                var report = new Report
                {
                    Story = story,
                    User = user,
                    Reason = Reason,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Reports.Add(report);
                _context.SaveChanges();

                return Json(new { success = true, message = "✅ Zgłoszenie zostało wysłane!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "❌ Wystąpił błąd podczas zapisywania zgłoszenia.", error = ex.Message });
            }
        }


        [HttpPost]
        [Authorize(Roles = "Admin,Moderator")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteStoryByAdmin(int storyId)
        {
            _logger.LogInformation($"[DeleteStoryByAdmin] Próba usunięcia relacji o ID: {storyId}");

            if (storyId <= 0)
            {
                _logger.LogWarning("[DeleteStoryByAdmin] Nieprawidłowy ID relacji.");
                return BadRequest("Nieprawidłowy ID relacji.");
            }

            var story = _context.Stories
                                .Include(s => s.Images)
                                .Include(s => s.Tags)
                                .Include(s => s.Reports)
                                .FirstOrDefault(s => s.StoryId == storyId);

            if (story == null)
            {
                _logger.LogWarning($"[DeleteStoryByAdmin] Relacja o ID {storyId} nie została znaleziona.");
                return NotFound();
            }

            try
            {
                if (story.Images != null && story.Images.Any())
                {
                    _context.StoryImages.RemoveRange(story.Images);
                }

                if (story.Tags != null && story.Tags.Any())
                {
                    story.Tags.Clear();
                }

                if (story.Reports != null && story.Reports.Any())
                {
                    _context.Reports.RemoveRange(story.Reports);
                }

                if (!string.IsNullOrEmpty(story.ImagePath))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", story.ImagePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        _logger.LogInformation($"[DeleteStoryByAdmin] Usuwanie pliku: {filePath}");
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Stories.Remove(story);
                _context.SaveChanges();

                _logger.LogInformation($"[DeleteStoryByAdmin] Relacja o ID {storyId} została pomyślnie usunięta.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DeleteStoryByAdmin] Błąd podczas usuwania relacji o ID {storyId}: {ex.Message}");
                return StatusCode(500, "Wystąpił błąd podczas usuwania relacji.");
            }

            return RedirectToAction("Stories");
        }

    }
}
