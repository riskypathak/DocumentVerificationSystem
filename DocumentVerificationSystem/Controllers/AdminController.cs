using DocumentVerificationSystem.Data;
using DocumentVerificationSystem.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerificationSystem.Controllers
{
	public class AdminController : Controller
	{
		private readonly ApplicationDbContext _db;

		public AdminController(ApplicationDbContext db)
		{
			_db = db;
		}

		public IActionResult Index()
		{
			var currentUser = GetCurrentUser();
			if (currentUser == null || currentUser.Role != UserRole.Admin)
			{
				return RedirectToAction("Login", "Account");
			}

            var candidates = _db.Users
        .Where(u => u.Role == UserRole.Candidate)
        .Select(u => new User
        {
            Id = u.Id,
            Email = u.Email,
            RollNumber = u.RollNumber,
            IsFinalized = u.IsFinalized,
            HasUploadedDocuments = _db.Documents.Any(d => d.UserId == u.Id)
        })
        .ToList();

            return View(candidates);
        }

		public IActionResult Review(int candidateId)
		{
			var candidate = _db.Users.Find(candidateId);
			if (candidate == null) return NotFound();

			var documents = _db.Documents.Where(d => d.UserId == candidate.Id).ToList();
			var categories = _db.DocumentCategories.ToList();

			ViewBag.Candidate = candidate;
			ViewBag.Documents = documents;
			ViewBag.Categories = categories;
			return View();
		}

		[HttpPost]
		public IActionResult Review(int candidateId, int[] docIds, string[] statuses, string[] comments)
		{
			var candidate = _db.Users.Find(candidateId);
			if (candidate == null) return NotFound();

			for (int i = 0; i < docIds.Length; i++)
			{
				var doc = _db.Documents.Find(docIds[i]);
				if (doc != null && doc.UserId == candidateId)
				{
					if (Enum.TryParse(statuses[i], out DocumentStatus newStatus))
					{
						doc.Status = newStatus;
					}
					else
					{
						doc.Status = DocumentStatus.Pending;
					}
					doc.AdminComments = comments[i];
				}
			}

			_db.SaveChanges();
			TempData["Message"] = "Review saved successfully.";
            
            var documents = _db.Documents.Where(d => d.UserId == candidateId).ToList();
            var categories = _db.DocumentCategories.ToList();

            ViewBag.Candidate = candidate;
            ViewBag.Documents = documents;
            ViewBag.Categories = categories;

            return RedirectToAction("Review", new { candidateId });
        }

		[HttpPost]
		public IActionResult FinalizeSubmission(int candidateId)
		{
            var candidate = _db.Users.Find(candidateId);
            if (candidate == null) return NotFound();

            candidate.IsFinalized = true;
            _db.SaveChanges();

            TempData["Message"] = "Submission decisions finalized. Candidate notified.";
			return RedirectToAction("Index");
		}

        public IActionResult ViewDoc(int candidateId)
        {
			var candidate = _db.Users.Find(candidateId);
			if (candidate == null) return NotFound();

			var documents = _db.Documents.Where(d => d.UserId == candidate.Id).ToList();
			var categories = _db.DocumentCategories.ToList();

			ViewBag.Candidate = candidate;
			ViewBag.Documents = documents;
			ViewBag.Categories = categories;

            return View();
        }

        private User? GetCurrentUser()
		{
			var userId = HttpContext.Session.GetInt32("UserId");
			if (userId == null) return null;
			return _db.Users.Find(userId.Value);
		}
	}
}
