using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Dtos.Comment
{
    public class GetCommentDto
    {
        public int Id {get; set;}
        public int OwnerID {get;set;}
        public int PostID {get; set;}
        public int ParentID {get; set;}
        public string Type {get; set;} = string.Empty;
        public string Content {set; get;} = string.Empty;
        public DateTime Date {set; get;} = DateTime.Now;
        public int Rating {set; get;} = 0;
        public List<GetCommentDto>? Replies {set; get;}
    }
}