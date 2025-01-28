using DocumentVerificationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace DocumentVerificationSystem.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
		: base(options)
		{
		}

		public DbSet<User> Users { get; set; }
		public DbSet<DocumentCategory> DocumentCategories { get; set; }
		public DbSet<Document> Documents { get; set; }
	}
}
