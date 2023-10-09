using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using redot_api.Dtos.Comment;
using redot_api.Dtos.Post;
using redot_api.Services.PostService;

namespace redot_api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;
        public PostController(IPostService postService)
        {
            _postService = postService;
        }
        [HttpGet("/post/{postId}")]
        public async Task<ActionResult<ServiceResponse<GetPostDto>>> Get(int postId){
            return Ok(await _postService.GetPost(postId));
        }
        [HttpGet("/posts")]
        public async Task<ActionResult<ServiceResponse<List<GetPostDto>>>> GetPosts(int pageNumber, int pageSize, Order order){
            return Ok(await _postService.GetPosts(pageNumber, pageSize, order));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<ServiceResponse<GetPostDto>>> AddPost(AddPostDto newPostDto){
            return Ok(await _postService.AddPost(newPostDto));
        }

        
        [HttpGet("posts/search")]
        public async Task<ActionResult<ServiceResponse<List<GetPostDto>>>> SearchPosts(string searchTerm, int pageNumber, int pageSize, Order order){
            return Ok(await _postService.SearchPosts(searchTerm, pageNumber, pageSize, order));
        }
        
        [Authorize]
        [HttpPut("/post")]
        public async Task<ActionResult<ServiceResponse<GetPostDto>>> UpdatePost(int postId, UpdatePostDto updatedPost){
            var response = await _postService.UpdatePost(postId, updatedPost);
            if(response.Data == null){
                return NotFound(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("/post/{postId}")]
        public async Task<ActionResult<ServiceResponse<List<GetPostDto>>>> DeletePost(int postId){
            var response = await _postService.DeletePost(postId);
            if(response.Data == null){
                return NotFound(response);
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPut("/post/{postId}/vote")]
        public async Task<ActionResult<ServiceResponse<GetPostDto>>> UpvotePost(int postId, bool upvote){
            var response = await _postService.RatePost(postId, upvote);
            if(response.Data == null){
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}