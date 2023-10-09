using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using redot_api.Dtos.Comment;
using redot_api.Services.CommentService;
using redot_api.Services.PostService;

namespace redot_api.Controllers
{
    [ApiController]
    [Route("")]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly IPostService _postService;
        public CommentController(ICommentService commentService,IPostService postService)
        {
            _commentService = commentService;
            _postService = postService;
        }
        
        [HttpGet("post/{postId}/comments")]
        public async Task<ActionResult<ServiceResponse<List<Comment>>>> GetComments(int postId, int pageNumber, int pageSize)
        {
            var post = await _postService.GetPost(postId);
            if (post.Data == null)
            {
                return NotFound();
            }
            return Ok(await _commentService.GetComments(post.Data, pageNumber, pageSize));
        }

        [Authorize]
        [HttpPost("post/{postId}/comment")]
        public async Task<ActionResult<ServiceResponse<Comment>>> AddComment(int postId, AddCommentDto newCommentDto)
        {
            var post = await _postService.GetPost(postId);
            if (post.Data == null)
            {
                return NotFound();
            }
            return Ok(await _commentService.AddComment(post.Data, newCommentDto));
        }

        [HttpGet("post/{postId}comments/{commentId}")]
        public async Task<ActionResult<ServiceResponse<Comment>>> GetComment(int commentId){
            return Ok(await _commentService.GetComment(commentId));
        }
        
        [Authorize]
        [HttpPost("post/{postId}/{commentId}/reply")]
        public async Task<ActionResult<ServiceResponse<Comment>>> AddCommentReply(int postId, int commentId, AddCommentDto newCommentDto)
        {
            var post = await _postService.GetPost(postId);
            if (post.Data == null)
            {
                return NotFound();
            }
            var comment = await _commentService.GetComment(commentId);
            if (comment.Data == null)
            {
                return NotFound();
            }
            
            return Ok(await _commentService.AddCommentReply(comment.Data, newCommentDto));
        }

        [HttpGet("post/{postId}/{commentId}/replies")]
        public async Task<ActionResult<ServiceResponse<List<Comment>>>> GetReplies(int postId, int commentId, int pageNumber, int pageSize)
        {
            var post = await _postService.GetPost(postId);
            if (post.Data == null)
            {
                return NotFound();
            }
            var comment = await _commentService.GetComment(commentId);
            if (comment.Data == null)
            {
                return NotFound();
            }
            return Ok(await _commentService.GetReplies(comment.Data, pageNumber, pageSize));
        }

        [Authorize]
        [HttpPut("post/{postId}/{commentId}")]
        public async Task<ActionResult<ServiceResponse<Comment>>> UpdateComment(int postId, int commentId, UpdateCommentDto updatedComment)
        {
            var post = await _postService.GetPost(postId);
            if (post.Data == null)
            {
                return NotFound();
            }
            var comment = await _commentService.GetComment(commentId);
            if (comment.Data == null)
            {
                return NotFound();
            }
            return Ok(await _commentService.UpdateComment(comment.Data.Id, updatedComment));
        }

        [Authorize]
        [HttpDelete("post/{postId}/{commentId}")]
        public async Task<ActionResult<ServiceResponse<List<Comment>>>> DeleteComment(int postId, int commentId)
        {
            var post = await _postService.GetPost(postId);
            if (post.Data == null)
            {
                return NotFound();
            }
            var comment = await _commentService.GetComment(commentId);
            if (comment.Data == null)
            {
                return NotFound();
            }
            return Ok(await _commentService.DeleteComment(comment.Data.Id));
        }

        [Authorize]
        [HttpPut("{postId}/{commentId}/vote")]
        public async Task<ActionResult<ServiceResponse<Comment>>> UpvoteComment(int postId, int commentId, bool upvote)
        {
            var post = await _postService.GetPost(postId);
            if (post.Data == null)
            {
                return NotFound();
            }
            var comment = await _commentService.GetComment(commentId);
            if (comment.Data == null)
            {
                return NotFound();
            }
            return Ok(await _commentService.RateComment(comment.Data.Id, upvote));
        }

    }
}