using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Comment;
using redot_api.Models;
using Type = redot_api.Models.Type;

namespace redot_api.Dtos.Post
{
    public class UpdatePostDto
    {
        public string Title { get; set;} = string.Empty; 
        public Type Type {get; set;} = Type.Text;
        public string Content {get; set;} = string.Empty;
    }
}