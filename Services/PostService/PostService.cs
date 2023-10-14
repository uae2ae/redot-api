using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using redot_api.Dtos.Comment;
using redot_api.Dtos.Post;

namespace redot_api.Services.PostService
{
    public class PostService : IPostService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private User GetUser() => _context.Users.FirstOrDefault(u => u.Id == GetUserId())!;
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private int GetPostId() => int.Parse(_httpContextAccessor.HttpContext!.Request.RouteValues.SingleOrDefault(x => x.Key == "postId").Value?.ToString()!);
        private Subredot? GetSubredotFromName() => _context.Subredots.FirstOrDefault(s => s.Name == GetSubredotName());
        private string GetSubredotName() => _httpContextAccessor.HttpContext!.Request.RouteValues.SingleOrDefault(x => x.Key == "SubredotName").Value?.ToString()!;
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
            Subredot? subredot = GetSubredotFromName();
            post.SubredotId = subredot?.Id ?? 0;
            post.Poster = GetUser();
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
            List<GetPostDto> dbPosts = new List<GetPostDto>();
            var serviceResponse = new ServiceResponse<List<GetPostDto>>();
            
            dbPosts = await _context.Posts.Select(p => _mapper.Map<GetPostDto>(p)).ToListAsync();
            
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

            pageSize = pageSize > 0 ? pageSize : 10;
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            dbPosts = dbPosts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            serviceResponse.Data = dbPosts;
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetPostDto>> RatePost(int postId, bool upvote)
        {
            var serviceResponse = new ServiceResponse<GetPostDto>();
            var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
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
                    post.Rating += upvote ? 1 : -1;
                    vote.Upvote = upvote;
                    _context.Votes.Update(vote);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "You have already voted this post.";
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

        public async Task<ServiceResponse<List<GetPostDto>>> SearchPosts(string searchTerm, int pageNumber, int pageSize, Order order)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            var searchWords = searchTerm.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var resultPosts = await _context.Posts
                .Include(p => p.Comments)!
                .ThenInclude(c => c.Commenter)
                .Include(p => p.Poster)
                .Select(p => new
                {
                    Post = p,
                    MatchPercentage = CalculateMatchPercentage(p, searchWords)
                })
                .OrderByDescending(p => p.MatchPercentage)
                .Select(p => p.Post)
                .ToListAsync();
            

            pageSize = pageSize > 0 ? pageSize : 10;
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            resultPosts = resultPosts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            serviceResponse.Data = resultPosts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            return serviceResponse;
        }

        private double CalculateMatchPercentage(Post post, string[] searchWords)
        {
            int matchCount = 0;
            foreach (string searchWord in searchWords)
            {
                int[] prefixTable = new int[searchWord.Length];
                int j = 0;
                for (int i = 1; i < searchWord.Length; i++)
                {
                    while (j > 0 && searchWord[j] != searchWord[i])
                    {
                        j = prefixTable[j - 1];
                    }
                    if (searchWord[j] == searchWord[i])
                    {
                        j++;
                    }
                    prefixTable[i] = j;
                }

                int textIndex = 0;
                int patternIndex = 0;
                while (textIndex < post.Content.Length)
                {
                    if (post.Content[textIndex] == searchWord[patternIndex])
                    {
                        textIndex++;
                        patternIndex++;
                        if (patternIndex == searchWord.Length)
                        {
                            matchCount++;
                            patternIndex = prefixTable[patternIndex - 1];
                        }
                    }
                    else if (patternIndex == 0)
                    {
                        textIndex++;
                    }
                    else
                    {
                        patternIndex = prefixTable[patternIndex - 1];
                    }
                }
            }

            double matchPercentage = (double)matchCount / (double)post.Content.Length;
            return matchPercentage;
        }

        public async Task<ServiceResponse<GetPostDto>> DeletePost(int postId)
        {
            ServiceResponse<GetPostDto> serviceResponse = new ServiceResponse<GetPostDto>();
            try
            {
                var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == postId);
                if (post!.Poster!.Id != GetUserId())
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "You are not the owner of this post.";
                    return serviceResponse;
                }
                _context.Posts.Remove(post);
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

        public async Task<ServiceResponse<List<GetPostDto>>> GetPostsByUser(int userId)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            serviceResponse.Data = await _context.Posts.Where(p => p.Poster!.Id == userId).Select(p => _mapper.Map<GetPostDto>(p)).ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetPostDto>>> GetPostsBySubreddit(int subredotId)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            serviceResponse.Data = await _context.Posts.Where(p => p.SubredotId == subredotId).Select(p => _mapper.Map<GetPostDto>(p)).ToListAsync();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetPostDto>>> GetSubPosts(string? subredotName, int pageNumber, int pageSize, Order order)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            var subredot = await _context.Subredots.FirstOrDefaultAsync(s => s.Name == subredotName);
            if (subredot == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Subredot not found.";
                return serviceResponse;
            }
            var posts = await _context.Posts.Where(p => p.SubredotId == subredot.Id).ToListAsync();
            posts = order switch
            {
                Order.OldToNew => posts.OrderBy(p => p.Date).ToList(),
                Order.NewToOld => posts.OrderByDescending(p => p.Date).ToList(),
                Order.Hot => posts.OrderByDescending(p => p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-1)).Count()).ToList(),
                Order.Top => posts.OrderByDescending(p => p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count()).ToList(),
                Order.Controversial => posts.OrderByDescending(p => p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() / (p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() + p.Comments!.Where(c => c.Date < DateTime.Now.AddDays(-7)).Count())).ToList(),
                Order.Rizing => posts.OrderByDescending(p => p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() / (p.Comments!.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() + p.Comments!.Where(c => c.Date < DateTime.Now.AddDays(-7)).Count())).ToList(),
                _ => posts.OrderByDescending(p => p.Date).ToList()
            };
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            pageSize = pageSize > 0 ? pageSize : 10;
            posts = posts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            serviceResponse.Data = posts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetPostDto>>> SearchSubPosts(string? subredotName, string searchTerm, int pageNumber, int pageSize, Order order)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            var subredot = await _context.Subredots.FirstOrDefaultAsync(s => s.Name == subredotName);
            if (subredot == null)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = "Subredot not found.";
                return serviceResponse;
            }
            var searchWords = searchTerm.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var resultPosts = await _context.Posts
                .Include(p => p.Comments)!
                .ThenInclude(c => c.Commenter)
                .Include(p => p.Poster)
                .Where(p => p.SubredotId == subredot.Id)
                .Select(p => new
                {
                    Post = p,
                    MatchPercentage = CalculateMatchPercentage(p, searchWords)
                })
                .OrderByDescending(p => p.MatchPercentage)
                .Select(p => p.Post)
                .ToListAsync();
            
            pageSize = pageSize > 0 ? pageSize : 10;
            pageNumber = pageNumber > 0 ? pageNumber : 1;
            resultPosts = resultPosts.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            serviceResponse.Data = resultPosts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            return serviceResponse;
        }
    }
}