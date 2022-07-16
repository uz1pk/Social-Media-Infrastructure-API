using TweetAPI.Domain;

namespace TweetAPI.Services
{
    public interface IPostService
    {
        Task<List<Post>> GetPostsAsync();

        Task<Post> GetPostByIdAsync(Guid postId);

        Task<bool> UpdatePostAsync(Post postToUpdate);

        Task<bool> CreatePostAsync(Post post);

        Task<bool> DeletePostAsync(Guid postId);

        Task<bool> CorrectUserAsync(Guid postId, string userId);
    }
}
