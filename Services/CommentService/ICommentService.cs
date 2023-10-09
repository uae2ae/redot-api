using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Comment;
using redot_api.Dtos.Post;

namespace redot_api.Services.CommentService
{
    public interface ICommentService
    {
        Task<ServiceResponse<List<GetCommentDto>>> GetComments(GetPostDto post, int pageNumber, int pageSize);
        Task<ServiceResponse<List<GetCommentDto>>> GetReplies(GetCommentDto comment, int pageNumber, int pageSize);
        Task<ServiceResponse<GetCommentDto>> AddComment(GetPostDto data, AddCommentDto newComment);
        Task<ServiceResponse<GetCommentDto>> AddCommentReply(GetCommentDto comment, AddCommentDto newComment);
        Task<ServiceResponse<GetCommentDto>> GetComment(int commentId);
        Task<ServiceResponse<GetCommentDto>> RateComment(int commentId, bool upvote);
        Task<ServiceResponse<GetCommentDto>> DeleteComment(int commentId);
        Task<ServiceResponse<GetCommentDto>> UpdateComment(int commentId, UpdateCommentDto updatedComment);
    }
}