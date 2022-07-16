namespace TweetAPI.Extensions
{
    public static class ApiExtensions
    {
        public static string GetUserId(this HttpContext req)
        {
            if(req.User == null)
            {
                return String.Empty;
            }

            return req.User.Claims.Single(x => x.Type == "id").Value;
        }
    }
}
