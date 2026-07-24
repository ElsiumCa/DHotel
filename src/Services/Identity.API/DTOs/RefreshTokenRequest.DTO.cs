using System.ComponentModel.DataAnnotations;
namespace Identity.API.DTOs
{
    public class RefreshTokenRequestDTO
    {
        [Required(ErrorMessage = "Refresh token is required")]
        [MaxLength(256)]
        public string RefreshToken { get; set; } = string.Empty;

        [Required(ErrorMessage = "Access token is required")]
        [MaxLength(256)]
        public string AccessToken { get; set; } = string.Empty;
    }
}