using System.ComponentModel.DataAnnotations;

namespace TweetAPI.Domain
{
    public class Post
    {
        [Key]
        public Guid Id { get; set; }

        public string? Name { get; set; }
    }
}
