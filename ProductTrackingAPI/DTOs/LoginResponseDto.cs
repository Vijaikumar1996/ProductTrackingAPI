namespace ProductTrackingAPI.DTOs
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public List<string> Roles { get; set; } = new();
    }
}
