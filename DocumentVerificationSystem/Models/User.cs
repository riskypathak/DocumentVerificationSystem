namespace DocumentVerificationSystem.Models
{
	public enum UserRole
	{
		Candidate,
		Admin
	}

	public class User
	{
		public int Id { get; set; }
		public string Email { get; set; } = default!;
		public string PasswordHash { get; set; } = default!;
		public string? RollNumber { get; set; }
		public DateTime? DateOfBirth { get; set; }
		public UserRole Role { get; set; }
	}
}
