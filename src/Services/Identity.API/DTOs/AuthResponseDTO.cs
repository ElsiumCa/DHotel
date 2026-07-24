namespace Identity.API.DTOs
{
    public class AuthResponseDTO
    {
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime? Expiration { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}