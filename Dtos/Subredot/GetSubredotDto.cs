using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Post;

namespace redot_api.Dtos.Subredot
{
    public class GetSubredotDto
    {
        public int Id {get; set;}
        public string Name {get; set;} = string.Empty;
        public string Description {get; set;} = string.Empty;
        public List<GetPostDto>? Posts {get; set;}
        public List<GetUserDto>? Subscribers {get; set;}
        public List<GetUserDto>? Moderators {get; set;}
    }
}