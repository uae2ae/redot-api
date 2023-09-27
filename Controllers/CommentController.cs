using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using redot_api.Dtos.Comment;
using redot_api.Services.CommentService;
using redot_api.Services.PostService;

namespace redot_api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;
        public CommentController(ICommentService commentService,IPostService postService)
        {
            _commentService = commentService;
            _postService = postService;
        }
        
        [HttpGet("{postId}/comments")]
        public async Task<ActionResult<ServiceResponse<List<Comment>>>> GetComments(int postId, int pageNumber, int pageSize)
        {
            var post = await _postService.GetPost(postId);
            if (post.Data == null)
            {
                return NotFound();
            }
            return Ok(await _commentService.GetComments(post.Data, pageNumber, pageSize));
        }

        [HttpGet("{postId}/{commentId}")]
        public async Task<ActionResult<ServiceResponse<Comment>>> GetComment(int commentId){
            return Ok(await _commentService.GetComment(commentId));
        }
    }
}