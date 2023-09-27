using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Comment;
using Type = redot_api.Models.Type;

namespace redot_api.Dtos.Post
{
    public class AddPostDto
    {
        public redot_api.Models.User? Owner {get;set;}
        public string Title { get; set;} = string.Empty; 
        public Type Type {get; set;} = Type.Text;
        public string Content {get; set;} = string.Empty;
        public int Rating {get; set;} = 0;
        public DateTime Date {get; set;} = DateTime.Now;
        public List<GetCommentDto>? Comments {get; set;}
    }
}