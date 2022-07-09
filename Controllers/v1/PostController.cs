using Microsoft.AspNetCore.Mvc;
using TweetAPI.Contracts.v1;
using TweetAPI.Contracts.v1.Requests;
using TweetAPI.Contracts.v1.Responses;
using TweetAPI.Domain;

namespace TweetAPI.Controllers.v1
{
    public class PostController : Controller
    {
        private List<Post> _posts;

        public PostController()
        {
            _posts = new List<Post>();
            for (int i = 0; i < 5; i++)
            {
                _posts.Add(new Post { Id = Guid.NewGuid().ToString() });
            }
        }

        [HttpGet(APIRoutes.Posts.GetAll)]
        public IActionResult GetAll()
        {
            return Ok(_posts);
        }

        [HttpPost(APIRoutes.Posts.Create)]
        public IActionResult Create([FromBody] PostRequest postReq)
        {
            Post post = new Post { Id = postReq.Id };

            if (String.IsNullOrEmpty(post.Id)) 
            { 
                post.Id = Guid.NewGuid().ToString();
            }

            _posts.Add(post);

            string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            string locationUrl = baseUrl + "/" + APIRoutes.Posts.Get.Replace("{postId}", post.Id);

            var res = new PostResponse { Id = post.Id };
            return Created(locationUrl, res);
        }

    }
}
