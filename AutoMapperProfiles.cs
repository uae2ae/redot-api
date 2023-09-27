using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Post;

namespace redot_api
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Post, GetPostDto>();
            CreateMap<AddPostDto, Post>();
        }
    }
}