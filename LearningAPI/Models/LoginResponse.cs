namespace LearningAPI.Models
{
    public class LoginResponse
    {
        public User User { get; set; } = new User();

        public string AccessToken { get; set; } = string.Empty;
    }
}
