using System.Security.Cryptography;
using System.Text;

namespace DocumentVerificationSystem.Models
{
	public static class EncryptedFolderHelper
	{
		private const string DefaultSalt = "YourSecretSaltHere";

		public static string GetHashedFolderName(int userId, string salt = DefaultSalt)
		{
			using var sha256 = SHA256.Create();
			var rawBytes = Encoding.UTF8.GetBytes(userId + salt);
			var hashBytes = sha256.ComputeHash(rawBytes);

			var sb = new StringBuilder();
			foreach (var b in hashBytes)
				sb.Append(b.ToString("x2"));

			return sb.ToString();
		}
	}
}
