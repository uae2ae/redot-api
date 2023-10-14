using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Models
{
    public class Post
    {
        public int Id {get; set;}
        public User? Poster {get;set;}
        public int PosterId {get;set;}
        public int SubredotId { get; internal set; }
        public string Title { get; set;} = string.Empty; 
        public Type Type {get; set;} = Type.Text;
        public string Content {get; set;} = string.Empty;
        public int Rating {get; set;} = 0;
        public DateTime Date {get; set;}
        public DateTime LastEdited {set; get;}
        public List<Comment>? Comments {get; set;}
    }
}