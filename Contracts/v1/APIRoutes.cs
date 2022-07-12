namespace TweetAPI.Contracts.v1
{
    public static class APIRoutes
    {
        public const string RootApi = "api";
        public const string Version = "v1";
        public const string BaseRoute = $"{RootApi}/{Version}";

        public static class Posts
        {
            public const string GetAll = BaseRoute + "/posts";
            public const string Create = BaseRoute + "/posts";

            public const string Get = BaseRoute + "/posts/{postId}";
            public const string Update = BaseRoute + "/posts{postId}";
            public const string Delete = BaseRoute + "/posts{postId}";
        }

        public static class AuthRoutes
        {
            public const string Login = BaseRoute + "/auth/login";

            public const string Register = BaseRoute + "/auth/register";
        }   

    }
}
