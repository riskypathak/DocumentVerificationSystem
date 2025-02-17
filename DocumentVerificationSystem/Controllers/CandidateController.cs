using DocumentVerificationSystem.Data;
using DocumentVerificationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using static DocumentVerificationSystem.Models.EncryptedFolderHelper;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerificationSystem.Controllers
{
    public class CandidateController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public CandidateController(ApplicationDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        // GET: /Candidate/Dashboard
        public IActionResult Dashboard()
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var docs = _db.Documents.Where(d => d.UserId == currentUser.Id).ToList();

            bool hasRejected = docs.Any(d => d.Status == DocumentStatus.Rejected);
            bool allVerified = docs.Count > 0 && docs.All(d => d.Status == DocumentStatus.Verified);
            bool allPendingOrVerified = docs.Count > 0 && docs.All(d => d.Status == DocumentStatus.Verified || d.Status == DocumentStatus.Pending);
            bool hasPending = docs.Any(d => d.Status == DocumentStatus.Pending);

            string overallStatus;
            if (allVerified)
                overallStatus = "Verified";
            else if (allPendingOrVerified)
                overallStatus = "Under Review";
            else if (hasRejected)
                overallStatus = "Action Required";
            else if (docs.Any())
                overallStatus = "Submitted";
            else
                overallStatus = "Not Uploaded";

            ViewBag.OverallStatus = overallStatus;

            var categories = _db.DocumentCategories.ToList();

            var documentsByCategory = categories.ToDictionary(
                cat => cat.Id,
                cat => docs.Where(d => d.CategoryId == cat.Id).ToList()
            );

            var adminComments = _db.Documents
                .Where(d => d.UserId == currentUser.Id && !string.IsNullOrEmpty(d.AdminComments))
                .Select(d => new { d.CategoryId, d.AdminComments })
                .ToList()
                .GroupBy(d => d.CategoryId)
                .ToDictionary(g => g.Key, g => g.Select(d => d.AdminComments).ToList());

            ViewBag.AdminComments = adminComments;
            ViewBag.DocumentsByCategory = documentsByCategory;

            return View(categories);
        }

        [HttpGet]
        public IActionResult UploadDocuments(int? categoryId)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            if (categoryId == null)
                categoryId = _db.DocumentCategories.FirstOrDefault()?.Id;

            var category = _db.DocumentCategories.Find(categoryId);
            if (category == null) return NotFound("Category not found.");

            var docs = _db.Documents
                    .Where(d => d.UserId == currentUser.Id && d.CategoryId == category.Id)
                    .ToList();

            ViewBag.Category = category;
            ViewBag.UserDocs = docs;
            return View();
        }

        [HttpPost]
        [HttpPost]
        public IActionResult UploadDocuments(int categoryId, string[] docTitles, IFormFile[] files)
        {
            var currentUser = GetCurrentUser();
            if (currentUser == null) return RedirectToAction("Login", "Account");

            var category = _db.DocumentCategories.Find(categoryId);
            if (category == null) return NotFound("Category not found.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            const int maxFileSize = 1048576; // 1 MB in bytes

            var hashedFolderName = GetHashedFolderName(currentUser.Id);
            var hashedFolderPath = Path.Combine(_env.WebRootPath, "Uploads", hashedFolderName);

            if (!Directory.Exists(hashedFolderPath))
            {
                Directory.CreateDirectory(hashedFolderPath);
            }

            var errorList = new List<string>();

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var title = docTitles[i];

                if (file != null && file.Length > 0)
                {
                    if (file.Length > maxFileSize)
                    {
                        errorList.Add($"The file '{file.FileName}' exceeds the 1 MB limit.");
                        continue;
                    }

                    var ext = Path.GetExtension(file.FileName).ToLower();
                    if (!allowedExtensions.Contains(ext))
                    {
                        errorList.Add($"The file '{file.FileName}' is not allowed (only .jpg, .jpeg, .png, .pdf).");
                        continue;
                    }

                    var uniqueFileName = Guid.NewGuid().ToString("N") + ext;
                    var filePath = Path.Combine(hashedFolderPath, uniqueFileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    file.CopyTo(stream);

                    var existingDoc = _db.Documents
                            .FirstOrDefault(d => d.UserId == currentUser.Id
                                                            && d.CategoryId == categoryId
                                                            && d.Title == title);

                    if (existingDoc == null)
                    {
                        var newDoc = new Document
                        {
                            Title = title,
                            FilePath = $"/Uploads/{hashedFolderName}/{uniqueFileName}",
                            Status = DocumentStatus.Pending,
                            UserId = currentUser.Id,
                            CategoryId = categoryId
                        };
                        _db.Documents.Add(newDoc);
                    }
                    else
                    {
                        existingDoc.FilePath = $"/Uploads/{hashedFolderName}/{uniqueFileName}";
                        existingDoc.Status = DocumentStatus.Pending;
                        existingDoc.AdminComments = null;
                    }
                }
            }

            _db.SaveChanges();

            if (errorList.Any())
            {
                TempData["Errors"] = errorList;
            }
            else
            {
                TempData["Message"] = "Documents uploaded successfully.";
            }


            TempData["LastCategoryId"] = categoryId;

            return RedirectToAction("UploadDocuments", new { categoryId });
        }


        [HttpPost]
        public IActionResult SubmitAllDocuments()
        {
            TempData["Message"] = "Your documents have been submitted for review.";
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public ActionResult FinalizeSubmission(int candidateId)
        {
            TempData["Message"] = "Submission decisions finalized. Candidate notified.";
            return RedirectToAction("Index");
        }

        private User? GetCurrentUser()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return null;
            return _db.Users.Find(userId.Value);
        }
    }
}
