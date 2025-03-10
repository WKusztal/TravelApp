using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Security.Claims;
using TravelApp.Data;
using TravelApp.Models;

namespace TravelApp.Controllers
{
    [Authorize]
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public class AddCommentRequest
        {
            public int StoryId { get; set; }
            public required string CommentText { get; set; }
        }


        [HttpPost]
        [Authorize]
        public IActionResult AddComment([FromBody] AddCommentRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CommentText))
            {
                return BadRequest(new { success = false, message = "Komentarz nie może być pusty." });
            }

            var username = User.Identity?.Name;
            if (string.IsNullOrEmpty(username))
            {
                return Unauthorized(new { success = false, message = "Użytkownik nie jest zalogowany." });
            }

            var currentUser = _context.Users.FirstOrDefault(u => u.Username == username);
            if (currentUser == null)
            {
                return Unauthorized(new { success = false, message = "Nie znaleziono użytkownika w bazie." });
            }

            var story = _context.Stories.FirstOrDefault(s => s.StoryId == request.StoryId);
            if (story == null)
            {
                return NotFound(new { success = false, message = "Nie znaleziono relacji." });
            }

            var comment = new Comment
            {
                StoryId = request.StoryId,
                Content = request.CommentText,
                CreatedAt = DateTime.UtcNow.AddHours(1),
                UserId = currentUser.UserId,
                UserName = currentUser.Username ?? "Anonim"
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            return Ok(new
            {
                success = true,
                message = "Komentarz dodany!",
                commentId = comment.CommentId,
                username = currentUser.Username,
                userAvatar = currentUser.AvatarPath,
                userId = currentUser.UserId,
                createdAt = comment.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                content = comment.Content
            });
        }

        public class DeleteCommentRequest
        {
            public int CommentId { get; set; }
        }

        [HttpDelete]
        [Authorize] 
        public IActionResult DeleteComment([FromBody] DeleteCommentRequest request)
        {
            if (request == null || request.CommentId <= 0)
            {
                return BadRequest(new { success = false, message = "Nieprawidłowe ID komentarza." });
            }

            var comment = _context.Comments.FirstOrDefault(c => c.CommentId == request.CommentId);
            if (comment == null)
            {
                return NotFound(new { success = false, message = "Komentarz nie istnieje." });
            }

            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (comment.UserId != userId)
            {
                return Forbid();
            }

            _context.Comments.Remove(comment);
            _context.SaveChanges();

            return Ok(new { success = true, message = "Komentarz usunięty!" });
        }

    }
}