using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Comment;
using redot_api.Dtos.Post;

namespace redot_api
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<Post, GetPostDto>();
            CreateMap<GetPostDto, Post>();
            CreateMap<UpdatePostDto, Post>();
            CreateMap<AddPostDto, Post>();
            CreateMap<Comment, GetCommentDto>();
            CreateMap<GetCommentDto, Comment>();
            CreateMap<UpdateCommentDto, Comment>();
            CreateMap<AddCommentDto, Comment>();
        }
    }
}