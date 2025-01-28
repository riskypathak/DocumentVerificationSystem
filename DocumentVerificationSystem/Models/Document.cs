using System.ComponentModel.DataAnnotations.Schema;

namespace DocumentVerificationSystem.Models
{
	public enum DocumentStatus
	{
		Pending,
		Verified,
		Rejected
	}

	public class Document
	{
		public int Id { get; set; }
		public string Title { get; set; } = default!;
		public string FilePath { get; set; } = default!;
		public DocumentStatus Status { get; set; }
		public string? AdminComments { get; set; }

		public int UserId { get; set; }
		[ForeignKey(nameof(UserId))]
		public virtual User? User { get; set; }

		public int CategoryId { get; set; }
		[ForeignKey(nameof(CategoryId))]
		public virtual DocumentCategory? Category { get; set; }
	}
}
