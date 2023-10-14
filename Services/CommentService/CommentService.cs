using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using redot_api.Dtos.Comment;
using redot_api.Dtos.Post;

namespace redot_api.Services.CommentService
{
    public class CommentService : ICommentService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private User GetUser() => _context.Users.FirstOrDefault(u => u.Id == GetUserId())!;
        private Post GetPost() => _context.Posts.FirstOrDefault(p => p.Id == GetPostId())!;
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        private int GetPostId() => int.Parse(_httpContextAccessor.HttpContext!.Request.RouteValues.SingleOrDefault(x => x.Key == "postId").Value?.ToString()!);
        
        public CommentService(IMapper mapper, DataContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<ServiceResponse<GetCommentDto>> AddComment(GetPostDto data, AddCommentDto newComment)
        {
            Comment comment = _mapper.Map<Comment>(newComment);
            ServiceResponse<GetCommentDto> serviceResponse = new ServiceResponse<GetCommentDto>();
            comment.Post = GetPost();
            comment.Commenter = _context.Users.FirstOrDefault(u => u.Id == GetUserId());
            comment.CommenterId = comment.Commenter!.Id;
            comment.Date = DateTime.Now;
            _context.Comments.Add(comment);
            _context.SaveChanges();
            
            Post post = _context.Posts.FirstOrDefault(p => p.Id == comment.Post.Id)!;
            post.Comments!.Add(comment);
            _context.Posts.Update(post);
            serviceResponse.Data = _mapper.Map<GetCommentDto>(comment);
            return Task.FromResult(serviceResponse);
        }

        public async Task<ServiceResponse<Comment>> AddCommentReply(Comment comment, AddCommentDto newComment)
        {
            ServiceResponse<Comment> serviceResponse = new ServiceResponse<Comment>();
            try
            {
                Comment reply = _mapper.Map<Comment>(newComment);
                reply.Post = comment.Post;
                reply.Commenter = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
                reply.CommenterId = reply.Commenter!.Id;
                reply.Date = DateTime.Now;
                reply.ParentCommentId = comment.Id;
                await _context.Comments.AddAsync(reply);
                await _context.SaveChangesAsync();
                serviceResponse.Data = reply;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
        public async Task<ServiceResponse<List<Comment>>> GetReplies(Comment comment, int pageNumber, int pageSize)
        {
            ServiceResponse<List<Comment>> serviceResponse = new ServiceResponse<List<Comment>>();
            try
            {
                int skip = (pageNumber - 1) * pageSize;
                List<Comment> dbReplies = await _context.Comments
                    .Where(c => c.ParentCommentId == comment.Id)
                    .OrderByDescending(c => c.Date)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();
                serviceResponse.Data = dbReplies;
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCommentDto>>> GetComments(GetPostDto post, int pageNumber, int pageSize)
        {
            ServiceResponse<List<GetCommentDto>> serviceResponse = new ServiceResponse<List<GetCommentDto>>();
            try
            {
                int skip = (pageNumber - 1) * pageSize;
                List<Comment> dbComments = await _context.Comments
                    .Where(c => c.Post!.Id == post.Id)
                    .OrderByDescending(c => c.Date)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();
                serviceResponse.Data = dbComments.Select(c => _mapper.Map<GetCommentDto>(c)).ToList();
                dbComments = dbComments.OrderByDescending(c => c.Rating).ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetCommentDto>>> GetReplies(GetCommentDto comment, int pageNumber, int pageSize)
        {
            ServiceResponse<List<GetCommentDto>> serviceResponse = new ServiceResponse<List<GetCommentDto>>();
            try
            {
                int skip = (pageNumber - 1) * pageSize;
                List<Comment> dbReplies = await _context.Comments
                    .Where(c => c.ParentCommentId == comment.Id)
                    .OrderByDescending(c => c.Date)
                    .Skip(skip)
                    .Take(pageSize)
                    .ToListAsync();
                serviceResponse.Data = dbReplies.Select(c => _mapper.Map<GetCommentDto>(c)).ToList();
                dbReplies = dbReplies.OrderByDescending(c => c.Rating).ToList();
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCommentDto>> AddCommentReply(GetCommentDto comment, AddCommentDto newComment)
        {
            ServiceResponse<GetCommentDto> serviceResponse = new ServiceResponse<GetCommentDto>();
            try
            {
                Comment reply = _mapper.Map<Comment>(newComment);
                Comment comment1 = (await _context.Comments.FirstOrDefaultAsync(c => c.Id == comment.Id))!;
                reply.Post = comment1.Post;
                reply.Commenter = await _context.Users.FirstOrDefaultAsync(u => u.Id == GetUserId());
                reply.CommenterId = reply.Commenter!.Id;
                reply.Date = DateTime.Now;
                reply.ParentCommentId = comment.Id;
                await _context.Comments.AddAsync(reply);
                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetCommentDto>(reply);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCommentDto>> GetComment(int commentId)
        {
            ServiceResponse<GetCommentDto> serviceResponse = new ServiceResponse<GetCommentDto>();
            try
            {
                Comment dbComment = (await _context.Comments
                    .Include(c => c.Commenter)
                    .Include(c => c.Replies)!
                    .ThenInclude(c => c.CommenterId)
                    .FirstOrDefaultAsync(c => c.Id == commentId))!;
                serviceResponse.Data = _mapper.Map<GetCommentDto>(dbComment);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCommentDto>> UpdateComment(int commentId, UpdateCommentDto updatedComment)
        {
            ServiceResponse<GetCommentDto> serviceResponse = new ServiceResponse<GetCommentDto>();
            try
            {
                Comment comment = (await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId))!;
                comment!.Content = updatedComment.Content;
                comment.LastEdited = DateTime.Now;
                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetCommentDto>(comment);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetCommentDto>> RateComment(int commentId, bool upvote)
        {
            ServiceResponse<GetCommentDto> serviceResponse = new ServiceResponse<GetCommentDto>();
            try
            {
                Comment comment = (await _context.Comments.FirstOrDefaultAsync(c => c.Id == commentId))!;
                if (comment == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Comment not found.";
                    return serviceResponse;
                }
                Vote? existingVote = await _context.Votes.FirstOrDefaultAsync(v => v.UserId == GetUserId() && v.PostVote == false && v.PostId == commentId);
                if (existingVote != null)
                {
                    if (existingVote.Upvote == upvote)
                    {
                        serviceResponse.Success = false;
                        serviceResponse.Message = "You have already voted on this comment.";
                        return serviceResponse;
                    }
                    else
                    {
                        if (upvote)
                        {
                            comment.Rating++;
                        }
                        else
                        {
                            comment.Rating--;
                        }
                        _context.Comments.Update(comment);
                        _context.Votes.Remove(existingVote);
                        await _context.SaveChangesAsync();
                        serviceResponse.Data = _mapper.Map<GetCommentDto>(comment);
                        return serviceResponse;
                    }
                }
                Vote newVote = new Vote
                {
                    UserId = GetUserId(),
                    PostVote = false,
                    PostId = commentId,
                    Upvote = upvote
                };
                if (upvote)
                {
                    comment.Rating++;
                }
                else
                {
                    comment.Rating--;
                }
                _context.Comments.Update(comment);
                await _context.Votes.AddAsync(newVote);
                await _context.SaveChangesAsync();
                serviceResponse.Data = _mapper.Map<GetCommentDto>(comment);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public Task<ServiceResponse<GetCommentDto>> DeleteComment(int commentId)
        {
            ServiceResponse<GetCommentDto> serviceResponse = new ServiceResponse<GetCommentDto>();
            try
            {
                Comment comment = _context.Comments.FirstOrDefault(c => c.Id == commentId)!;
                if (comment == null)
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Comment not found.";
                    return Task.FromResult(serviceResponse);
                }
                if (comment.Commenter!.Id != GetUserId())
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "You can only delete your own comments.";
                    return Task.FromResult(serviceResponse);
                }
                _context.Comments.Remove(comment);
                _context.SaveChanges();
                serviceResponse.Data = _mapper.Map<GetCommentDto>(comment);
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return Task.FromResult(serviceResponse);
        }
    }
}