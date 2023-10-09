using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using redot_api.Dtos.Post;

namespace redot_api.Services.PostService
{
    public class PostService : IPostService
    {
        private static List<Post> posts = new List<Post> {
            new Post(),
            new Post{ Id = 1 ,Content = "This is a test" }
        };
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private int GetPostId() => int.Parse(_httpContextAccessor.HttpContext!.Request.RouteValues.SingleOrDefault(x => x.Key == "postId").Value?.ToString()!);
        public PostService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize]
        public async Task<ServiceResponse<GetPostDto>> AddPost(AddPostDto newPost)
        {
            var serviceResponse = new ServiceResponse<GetPostDto>();
            var post = _mapper.Map<Post>(newPost);
            post.PosterID = GetUserId();
            post.Date = DateTime.Now;
            post.Comments = new List<Comment>();
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<GetPostDto>(post);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetPostDto>> GetPost(int postId)
        {
            var serviceResponse = new ServiceResponse<GetPostDto>();
            serviceResponse.Data =  _mapper.Map<GetPostDto>(await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId));
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetPostDto>>> GetPosts(int pageNumber, int pageSize, Order order)
        {
            List<GetPostDto> dbPosts = await _context.Posts
                .Include(p => p.Comments)!
                .ThenInclude(c => c.Owner)
                .Include(p => p.PosterID)
                .Select(p => _mapper.Map<GetPostDto>(p))
                .ToListAsync();
            var serviceResponse = new ServiceResponse<List<GetPostDto>>();
            serviceResponse.Data = dbPosts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            dbPosts = order switch
            {
                Order.OldToNew => dbPosts.OrderBy(p => p.Date).ToList(),
                Order.NewToOld => dbPosts.OrderByDescending(p => p.Date).ToList(),
                Order.Hot => dbPosts.OrderByDescending(p => p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-1)).Count()).ToList(),
                Order.Top => dbPosts.OrderByDescending(p => p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count()).ToList(),
                Order.Controversial => dbPosts.OrderByDescending(p => p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() / (p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() + p.Comments!.Where(c => c.Date < DateTime.Now.AddDays(-7)).Count())).ToList(),
                Order.Rizing => dbPosts.OrderByDescending(p => p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() / (p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() + p.Comments!.Where(c => c.Date < DateTime.Now.AddDays(-7)).Count())).ToList(),
                _ => dbPosts.OrderByDescending(p => p.Date).ToList()
            };
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetPostDto>> RatePost(int postId, bool upvote)
        {
            var serviceResponse = new ServiceResponse<GetPostDto>();
            var post = posts.FirstOrDefault(p => p.Id == postId);
            if (post == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Post not found.";
                return serviceResponse;
            }
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == GetUserId());
                if (user == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "User not found.";
                    return serviceResponse;
                }
                var vote = _context.Votes.FirstOrDefault(v => v.PostId == postId && v.UserId == user.Id);
                if (vote == null)
                {
                    vote = new Vote
                    {
                        PostId = postId,
                        UserId = user?.Id ?? 0,
                        Upvote = upvote
                    };
                    _context.Votes.Add(vote);
                    post.Rating += upvote ? 1 : -1;
                }
                else if (vote.Upvote != upvote)
                {
                    post.Rating += upvote ? 2 : -2;
                    vote.Upvote = upvote;
                }
                else
                {
                    post.Rating += upvote ? -1 : 1;
                    _context.Votes.Remove(vote);
                }
                _context.Posts.Update(post);
                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetPostDto>(post);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetPostDto>> UpdatePost(int postId, UpdatePostDto updatedPost)
        {
            var serviceResponse = new ServiceResponse<GetPostDto>();
            try
            {
                //fetch posts with _context
                var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
                post!.Content = updatedPost.Content;
                post.Title = updatedPost.Title;
                post.LastEdited = DateTime.Now;
                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetPostDto>(post);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public Task<ServiceResponse<List<GetPostDto>>> SearchPosts(string searchTerm, int pageNumber, int pageSize, Order order)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            serviceResponse.Data = posts.Where(p => p.Content.Contains(searchTerm) || p.Title.Contains(searchTerm)).Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            return Task.FromResult(serviceResponse);
        }

        public Task<ServiceResponse<GetPostDto>> DeletePost(int postId)
        {
            ServiceResponse<GetPostDto> serviceResponse = new ServiceResponse<GetPostDto>();
            try
            {
                var post = posts.First(p => p.Id == postId);
                if (post.PosterID != GetUserId())
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "You are not the owner of this post.";
                    return Task.FromResult(serviceResponse);
                }
                posts.Remove(post);
                serviceResponse.Data = _mapper.Map<GetPostDto>(post);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return Task.FromResult(serviceResponse);
        }

        public Task<ServiceResponse<List<GetPostDto>>> GetPostsByUser(int userId)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            serviceResponse.Data = posts.Where(p => p.PosterID == userId).Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            return Task.FromResult(serviceResponse);
        }

        public Task<ServiceResponse<List<GetPostDto>>> GetPostsBySubreddit(int subredditId)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            serviceResponse.Data = posts.Where(p => p.SubredditId == subredditId).Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            return Task.FromResult(serviceResponse);
        }
    }
}