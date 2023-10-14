using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using redot_api.Dtos.Subredot;

namespace redot_api.Services.SubredotService
{
    public class SubredotService : ISubredotService
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
         private User GetUser() => _context.Users.FirstOrDefault(u => u.Id == GetUserId())!;
        private int GetUserId() => int.Parse(_httpContextAccessor.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        public SubredotService(DataContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<ServiceResponse<GetSubredotDto>> AddSubredot(AddSubredotDto newSubredot)
        {
            ServiceResponse<GetSubredotDto> serviceResponse = new ServiceResponse<GetSubredotDto>();
            Subredot subredot = _mapper.Map<Subredot>(newSubredot);
            subredot.Moderators!.Add(GetUser());
            subredot.Subscribers!.Add(GetUser());
            _context.Subredots.Add(subredot);
            await _context.SaveChangesAsync();
            serviceResponse.Data = _mapper.Map<GetSubredotDto>(subredot);
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetSubredotDto>>> DeleteSubredot(int id)
        {
            ServiceResponse<List<GetSubredotDto>> serviceResponse = new ServiceResponse<List<GetSubredotDto>>();
            try
            {
                Subredot subredot = await _context.Subredots.FirstAsync(s => s.Id == id && s.Moderators!.Any(u => u.Id == GetUserId()));
                if (subredot != null)
                {
                    _context.Subredots.Remove(subredot);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = _context.Subredots.Select(s => _mapper.Map<GetSubredotDto>(s)).ToList();
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Subredot not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<List<GetSubredotDto>>> GetAllSubredots()
        {
            ServiceResponse<List<GetSubredotDto>> serviceResponse = new ServiceResponse<List<GetSubredotDto>>();
            List<Subredot> dbSubredots = await _context.Subredots.ToListAsync();
            serviceResponse.Data = dbSubredots.Select(s => _mapper.Map<GetSubredotDto>(s)).ToList();
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetSubredotDto>> GetSubredotById(int id)
        {
            ServiceResponse<GetSubredotDto> serviceResponse = new ServiceResponse<GetSubredotDto>();
            try
            {
                Subredot? subredot = await _context.Subredots.FirstOrDefaultAsync(s => s.Id == id);
                if (subredot != null)
                {
                    serviceResponse.Data = _mapper.Map<GetSubredotDto>(subredot);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Subredot not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetSubredotDto>> UpdateSubredot(UpdateSubredotDto updatedSubredot)
        {
            ServiceResponse<GetSubredotDto> serviceResponse = new ServiceResponse<GetSubredotDto>();
            try
            {
                Subredot? subredot = await _context.Subredots.Include(s => s.Moderators).Include(s => s.Subscribers).FirstOrDefaultAsync(s => s.Id == updatedSubredot.Id)!;
                if (subredot!.Moderators!.Any(u => u.Id == GetUserId()))
                {
                    subredot.Name = updatedSubredot.Name;
                    subredot.Description = updatedSubredot.Description;
                    _context.Subredots.Update(subredot);
                    await _context.SaveChangesAsync();
                    serviceResponse.Data = _mapper.Map<GetSubredotDto>(subredot);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Subredot not found.";
                }
            }
            catch (Exception ex)
            {
                serviceResponse.Success = false;
                serviceResponse.Message = ex.Message;
            }
            return serviceResponse;
        }

        public async Task<ServiceResponse<GetSubredotDto>> GetSubredotByName(string name)
        {
            ServiceResponse<GetSubredotDto> serviceResponse = new ServiceResponse<GetSubredotDto>();
            try
            {
                Subredot? subredot = await _context.Subredots.FirstOrDefaultAsync(s => s.Name == name);
                if (subredot != null)
                {
                    serviceResponse.Data = _mapper.Map<GetSubredotDto>(subredot);
                }
                else
                {
                    serviceResponse.Success = false;
                    serviceResponse.Message = "Subredot not found.";
                }
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