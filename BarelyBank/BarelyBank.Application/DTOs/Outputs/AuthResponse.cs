namespace BarelyBank.Application.DTOs.Outputs
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public ClientViewModel Client { get; set; } = null!;
    }
}