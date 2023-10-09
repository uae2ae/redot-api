using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Type = redot_api.Models.Type;

namespace redot_api.Dtos.Comment
{
    public class AddCommentDto
    {
        public Type Type {get; set;} = Type.Text;
        public string Content {set; get;} = string.Empty;
    }
}