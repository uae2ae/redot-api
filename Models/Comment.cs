using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Models
{
    public class Comment
    {
        public int Id {get; set;}
        public Post? Post {get; set;}
        public int PostId {get; set;}
        public User? Commenter {get;set;}
        public int CommenterId {get;set;}
        public Type Type {get; set;} = Type.Text;
        public string Content {set; get;} = string.Empty;
        public DateTime Date {set; get;} = DateTime.Now;
        public DateTime LastEdited {set; get;} = DateTime.Now;
        public int Rating {set; get;} = 0;
        public int? ParentCommentId { set; get; }
        public List<Comment>? Replies {set; get;}
    }
}