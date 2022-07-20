namespace TweetAPI.Options
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public TimeSpan TokenTimeLimit { get; set; }
    }
}
