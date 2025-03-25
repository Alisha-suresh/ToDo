namespace ToDo.Models
{
    public class LoginRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class RegisterRequest
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string? Username { get; set; }
        public string? RefreshToken { get; set; }
    }

    public class TokenValidationRequest
    {
        public string? Token { get; set; }
    }
}