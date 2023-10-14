using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using redot_api.Dtos.Subredot;
using redot_api.Services.SubredotService;

namespace redot_api.Controllers
{
    [Route("")]
    public class SubredotController : ControllerBase
    {
        private readonly ISubredotService _subredotService;
        public SubredotController(ISubredotService subredotService)
        {
            _subredotService = subredotService;
        }
        [HttpGet("{SubredotName?}")]
        public async Task<ActionResult<ServiceResponse<GetSubredotDto>>> Get(string SubredotName){
            return Ok(await _subredotService.GetSubredotByName(SubredotName));
        }
        [HttpGet("subredots")]
        public async Task<ActionResult<ServiceResponse<List<GetSubredotDto>>>> GetSubredots(){
            return Ok(await _subredotService.GetAllSubredots());
        }
        [HttpPost("subredot")]
        public async Task<ActionResult<ServiceResponse<GetSubredotDto>>> AddSubredot(AddSubredotDto newSubredotDto){
            return Ok(await _subredotService.AddSubredot(newSubredotDto));
        }
        [HttpPut("{SubredotName?}")]
        public async Task<ActionResult<ServiceResponse<GetSubredotDto>>> UpdateSubredot(UpdateSubredotDto updatedSubredot){
            var response = await _subredotService.UpdateSubredot(updatedSubredot);
            if(response.Data == null){
                return NotFound(response);
            }
            return Ok(response);
        }
        [HttpDelete("{SubredotName?}")]
        public async Task<ActionResult<ServiceResponse<List<GetSubredotDto>>>> DeleteSubredot(int id){
            var response = await _subredotService.DeleteSubredot(id);
            if(response.Data == null){
                return NotFound(response);
            }
            return Ok(response);
        }
    }
}