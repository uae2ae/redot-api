using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Type = redot_api.Models.Type;

namespace redot_api.Dtos.Comment
{
    public class AddCommentDto
    {
        public int Id {get; set;}
        public int OwnerID {get;set;}
        public int PostID {get; set;}
        public int ParentID {get; set;}
        public Type Type {get; set;} = Type.Text;
        public string Content {set; get;} = string.Empty;
        public DateTime Date {set; get;} = DateTime.Now;
        public int Rating {set; get;} = 0;
        public List<GetCommentDto>? Replies {set; get;}
    }
}