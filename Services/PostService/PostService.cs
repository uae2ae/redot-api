using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public PostService(IMapper mapper, DataContext context)
        {
            _context = context;
            _mapper = mapper;
        }


        public async Task<ServiceResponse<GetPostDto>> AddPost(AddPostDto newPost)
        {
            var serviceResponse = new ServiceResponse<GetPostDto>();
            serviceResponse.Data = _mapper.Map<GetPostDto>(newPost);
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetPostDto>> GetPost(int postId)
        {
            var serviceResponse = new ServiceResponse<GetPostDto>();
            serviceResponse.Data =  _mapper.Map<GetPostDto>(posts.FirstOrDefault(p => p.Id == postId));
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetPostDto>>> GetPosts(int pageNumber, int pageSize, Order order)
        {
            List<GetPostDto> dbPosts = await _context.Posts
                .Include(p => p.Comments)
                .ThenInclude(c => c.Owner)
                .Include(p => p.Owner)
                .Select(p => _mapper.Map<GetPostDto>(p))
                .ToListAsync();
            var serviceResponse = new ServiceResponse<List<GetPostDto>>();
            serviceResponse.Data = dbPosts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
            dbPosts = order switch
            {
                Order.OldToNew => dbPosts.OrderBy(p => p.Date).ToList(),
                Order.NewToOld => dbPosts.OrderByDescending(p => p.Date).ToList(),
                Order.Hot => dbPosts.OrderByDescending(p => p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-1)).Count()).ToList(),
                Order.Top => dbPosts.OrderByDescending(p => p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count()).ToList(),
                Order.Controversial => dbPosts.OrderByDescending(p => p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() / (p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() + p.Comments.Where(c => c.Date < DateTime.Now.AddDays(-7)).Count())).ToList(),
                Order.Rizing => dbPosts.OrderByDescending(p => p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() / (p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() + p.Comments.Where(c => c.Date < DateTime.Now.AddDays(-7)).Count())).ToList(),
                _ => dbPosts.OrderByDescending(p => p.Date).ToList()
            };
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetPostDto>>> SearchPosts(string searchTerm, int pageNumber, int pageSize, Order order)
        {
            ServiceResponse<List<GetPostDto>> serviceResponse = new ServiceResponse<List<GetPostDto>>();
            try
            {
                List<Post> dbPosts = await _context.Posts
                    .Where(p => p.Title.Contains(searchTerm) || p.Content.Contains(searchTerm))
                    .ToListAsync();
                serviceResponse.Data = dbPosts.Select(p => _mapper.Map<GetPostDto>(p)).ToList();
                // order by what was requested
                dbPosts = order switch
                {
                    Order.OldToNew => dbPosts.OrderBy(p => p.Date).ToList(),
                    Order.NewToOld => dbPosts.OrderByDescending(p => p.Date).ToList(),
                    Order.Hot => dbPosts.OrderByDescending(p => p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-1)).Count()).ToList(),
                    Order.Top => dbPosts.OrderByDescending(p => p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count()).ToList(),
                    Order.Controversial => dbPosts.OrderByDescending(p => p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() / (p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() + p.Comments.Where(c => c.Date < DateTime.Now.AddDays(-7)).Count())).ToList(),
                    Order.Rizing => dbPosts.OrderByDescending(p => p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() / (p.Comments.Where(c => c.Date >= DateTime.Now.AddDays(-7)).Count() + p.Comments.Where(c => c.Date < DateTime.Now.AddDays(-7)).Count())).ToList(),
                    _ => dbPosts.OrderByDescending(p => p.Date).ToList()
                };
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }
    }
}