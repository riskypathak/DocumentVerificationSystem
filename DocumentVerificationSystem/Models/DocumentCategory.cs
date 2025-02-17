namespace DocumentVerificationSystem.Models
{
	public class DocumentCategory
	{
		public int Id { get; set; }
		public string CategoryName { get; set; } = default!;
        public virtual ICollection<Document>? Documents { get; set; }
    }
}
