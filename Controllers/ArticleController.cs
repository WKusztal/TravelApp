using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using TravelApp.Models;
using TravelApp.Data;
using Microsoft.Extensions.Logging;

namespace TravelApp.Controllers
{
    public class ArticleController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly ILogger<ArticleController> _logger;

        public ArticleController(ApplicationDbContext context, IWebHostEnvironment hostingEnvironment, ILogger<ArticleController> logger)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _logger = logger;
        }

        public IActionResult Articles()
        {
            var articles = _context.Articles
            .Include(a => a.Images)
            .OrderByDescending(a => a.CreatedAt)
            .ToList();

            ViewData["ActiveNavItem"] = "Articles";
            
            return View(articles);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult AddArticle()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddArticle(Article article, List<IFormFile> images)
        {
            if (!ModelState.IsValid)
            {
                return View(article);
            }

            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            if (images != null && images.Any())
            {
                article.Images = new List<ArticleImage>();
                foreach (var image in images)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
                    var filePath = Path.Combine(uploadsPath, fileName);

                    if (article.Images == null)
                    {
                        article.Images = new List<ArticleImage>();
                    }

                    if (article.Images.Any(img => img.ImagePath.EndsWith(fileName)))
                    {
                        continue;
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    article.Images.Add(new ArticleImage { ImagePath = $"/uploads/{fileName}" });
                }
            }

            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            return RedirectToAction("Articles");
        }



        [Authorize(Roles = "Admin")]
        public IActionResult EditArticle(int id)
        {
            var article = _context.Articles
            .Include(a => a.Images)
            .FirstOrDefault(a => a.ArticleId == id);

            if (article == null)
                return NotFound();

            return View(article);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult EditArticle(int id, Article updatedArticle, List<IFormFile> images, List<string> imagesToRemove)
        {
            var article = _context.Articles
                .Include(a => a.Images)
                .FirstOrDefault(a => a.ArticleId == id);

            if (article == null)
                return NotFound();

            article.Title = updatedArticle.Title;
            article.Description = updatedArticle.Description;

            if (imagesToRemove != null && imagesToRemove.Any())
            {
                foreach (var imagePath in imagesToRemove)
                {
                    if (article.Images != null)
                    {
                        var image = article.Images.FirstOrDefault(img => img.ImagePath == imagePath);
                        if (image != null)
                        {
                            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", image.ImagePath.TrimStart('/'));
                            if (System.IO.File.Exists(filePath))
                            {
                                System.IO.File.Delete(filePath);
                            }

                            article.Images.Remove(image);
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

                    if (article.Images == null)
                    {
                        article.Images = new List<ArticleImage>();
                    }

                    if (article.Images.Any(img => img.ImagePath.EndsWith(fileName)))
                    {
                        continue;
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }

                    article.Images.Add(new ArticleImage { ImagePath = $"/uploads/{fileName}" });
                }
            }

            _context.SaveChanges();

            return RedirectToAction("ArticleDetails", new { id = article.ArticleId });
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

            var image = _context.ArticleImages.FirstOrDefault(i => i.Id == request.ImageId);
            if (image == null)
            {
                return NotFound(new { success = false, message = "Obraz nie został znaleziony." });
            }

            _context.ArticleImages.Remove(image);
            _context.SaveChanges();

            return Ok(new { success = true });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteArticle(int articleId)
        {
            _logger.LogInformation($"[DeleteArticle] Próba usunięcia artykułu o ID: {articleId}");

            if (articleId <= 0)
            {
                _logger.LogWarning("[DeleteArticle] Nieprawidłowy ID artykułu.");
                return BadRequest("Nieprawidłowy ID artykułu.");
            }

            var article = _context.Articles.FirstOrDefault(a => a.ArticleId == articleId);

            if (article == null)
            {
                _logger.LogWarning($"[DeleteArticle] Artykuł o ID {articleId} nie został znaleziony.");
                return NotFound();
            }

            try
            {
                if (!string.IsNullOrEmpty(article.ImagePath))
                {
                    var filePath = Path.Combine(_hostingEnvironment.WebRootPath, "uploads", article.ImagePath);
                    if (System.IO.File.Exists(filePath))
                    {
                        _logger.LogInformation($"[DeleteArticle] Usuwanie pliku: {filePath}");
                        System.IO.File.Delete(filePath);
                    }
                }

                _context.Articles.Remove(article);
                _context.SaveChanges();

                _logger.LogInformation($"[DeleteArticle] Artykuł o ID {articleId} został pomyślnie usunięty.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"[DeleteArticle] Błąd podczas usuwania artykułu o ID {articleId}: {ex.Message}");
                return StatusCode(500, "Wystąpił błąd podczas usuwania artykułu.");
            }

            return RedirectToAction("Articles");
        }

        [HttpGet]
        public IActionResult ArticleDetails(int id)
        {
            var article = _context.Articles
                                .Include(a => a.Images)
                                .FirstOrDefault(a => a.ArticleId == id);

            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }

    }
}
