using DocumentVerificationSystem.Data;
using DocumentVerificationSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace DocumentVerificationSystem.Controllers
{
  public class AccountController : Controller
  {
    private readonly ApplicationDbContext _db;

    public AccountController(ApplicationDbContext db)
    {
      _db = db;
    }

    [HttpGet]
    public IActionResult Register()
    {
      return View();
    }

    [HttpPost]
    public IActionResult Register(string email, string password, string rollNumber, DateTime? dateOfBirth)
    {
      if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
      {
        ViewBag.Error = "Email and Password are required.";
        return View();
      }

      var existingUser = _db.Users.FirstOrDefault(u => u.Email == email);
      if (existingUser != null)
      {
        ViewBag.Error = "A user with this email already exists.";
        return View();
      }

      // In production, use a real hashing approach for the password
      var user = new User
      {
        Email = email,
        PasswordHash = password,
        RollNumber = rollNumber,
        DateOfBirth = dateOfBirth,
        Role = UserRole.Candidate
      };
      _db.Users.Add(user);
      _db.SaveChanges();

      // You might implement .NET Core Identity for real login
      // For this simple example, just redirect to Login
      return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
      return View();
    }

    [HttpPost]
    public IActionResult Login(string email, string password)
    {
      var user = _db.Users
          .FirstOrDefault(u => u.Email == email && u.PasswordHash == password);

      if (user == null)
      {
        ViewBag.Error = "Invalid email or password.";
        return View();
      }

      // For simplicity, store user info in session (NOT recommended in production)
      HttpContext.Session.SetInt32("UserId", user.Id);

      if (user.Role == UserRole.Admin)
        return RedirectToAction("Index", "Admin");
      else
        return RedirectToAction("Dashboard", "Candidate");
    }

    public IActionResult Logout()
    {
      HttpContext.Session.Remove("UserId"); // or Clear entire session if you prefer
      return RedirectToAction("Login");
    }
  }
}
