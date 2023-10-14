using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Comment;
using redot_api.Dtos.Post;

namespace redot_api.Services.PostService
{
    public interface IPostService
    {
        Task<ServiceResponse<List<GetPostDto>>> GetPosts(int pageNumber, int pageSize, Order order);
        Task<ServiceResponse<List<GetPostDto>>> GetSubPosts(string? subredotName, int pageNumber, int pageSize, Order order);
        Task<ServiceResponse<GetPostDto>> GetPost(int postId);
        Task<ServiceResponse<GetPostDto>> AddPost(AddPostDto newPost);
        Task<ServiceResponse<List<GetPostDto>>> SearchPosts(string searchTerm, int pageNumber, int pageSize, Order order);
        Task<ServiceResponse<List<GetPostDto>>> SearchSubPosts(string? subredotName, string searchTerm, int pageNumber, int pageSize, Order order);
        Task<ServiceResponse<GetPostDto>> RatePost(int postId, bool upvote);
        Task<ServiceResponse<GetPostDto>> UpdatePost(int postId, UpdatePostDto updatedPost);
        Task<ServiceResponse<GetPostDto>> DeletePost(int postId);
    }
}