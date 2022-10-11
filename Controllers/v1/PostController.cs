using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using TweetAPI.Contracts.v1;
using TweetAPI.Contracts.v1.Requests;
using TweetAPI.Contracts.v1.Responses;
using TweetAPI.Domain;
using TweetAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using TweetAPI.Extensions;

namespace TweetAPI.Controllers.v1
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostController : Controller
    {
        private readonly IPostService _postService;

        public PostController(IPostService service)
        {
            _postService = service;
        }

        [HttpGet(APIRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _postService.GetPostsAsync());
        }

        [HttpPut(APIRoutes.Posts.Update)]
        public async Task<IActionResult> Update([FromRoute]Guid postId, [FromBody] UpdatePostRequest request)
        {
            if (String.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { error = "Post name cannot be empty" });
            }

            var isCorrectUser = await _postService.CorrectUserAsync(postId, HttpContext.GetUserId());

            if (!isCorrectUser)
            {
                return BadRequest(new { error = "Post not associated with this ID" });
            }

            var post = await _postService.GetPostByIdAsync(postId);
            post.Name = request.Name;

            var updated = await _postService.UpdatePostAsync(post);

            if (updated)
            {
                return Ok(post);
            }

            return NotFound();
        }

        [HttpDelete(APIRoutes.Posts.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid postId)
        {
            var isCorrectUser = await _postService.CorrectUserAsync(postId, HttpContext.GetUserId());

            if (!isCorrectUser)
            {
                return BadRequest(new { error = "Post not associated with this ID" });
            }

            var deleted = await _postService.DeletePostAsync(postId);

            if (deleted)
            {
                return Ok(postId);
            }

            return NotFound();
        }

        [HttpGet(APIRoutes.Posts.Get)]
        public async Task<IActionResult> Get([FromRoute]Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);

            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

        [HttpPost(APIRoutes.Posts.Create)]
        public async Task<IActionResult> Create([FromBody]CreatePostRequest postReq)
        {
            if (String.IsNullOrEmpty(postReq.Name))
            {
                return BadRequest(new { error = "Post name cannot be empty" });
            }

            //User.FindFirst(ClaimTypes.NameIdentifier)
            var post = new Post { 
                Name = postReq.Name,
                UserId = HttpContext.GetUserId()
            };

            //CHANGE LATER
            await _postService.CreatePostAsync(post);

            string baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            string locationUrl = baseUrl + "/" + APIRoutes.Posts.Get.Replace("{postId}", post.Id.ToString());

            var res = new PostResponse { Id = post.Id };

            return Created(locationUrl, res);
        }

    }
}
