using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Identity.API.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [MaxLength(100)]
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        [Required]
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }

        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }
    }
}
