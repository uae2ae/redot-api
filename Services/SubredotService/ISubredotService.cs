using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Subredot;

namespace redot_api.Services.SubredotService
{
    public interface ISubredotService
    {
        Task<ServiceResponse<GetSubredotDto>> AddSubredot(AddSubredotDto newSubredot);
        Task<ServiceResponse<List<GetSubredotDto>>> GetAllSubredots();
        Task<ServiceResponse<GetSubredotDto>> GetSubredotById(int id);
        Task<ServiceResponse<GetSubredotDto>> GetSubredotByName(string name);
        Task<ServiceResponse<GetSubredotDto>> UpdateSubredot(UpdateSubredotDto updatedSubredot);
        Task<ServiceResponse<List<GetSubredotDto>>> DeleteSubredot(int id);
    }
}