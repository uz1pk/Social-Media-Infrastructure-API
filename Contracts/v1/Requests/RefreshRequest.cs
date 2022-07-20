namespace TweetAPI.Contracts.v1.Requests
{
    public class RefreshRequest
    {
        public string Token { get; set; }

        public string RefreshToken { get; set; }
    }
}
