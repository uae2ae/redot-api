using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Models
{
    public class Subredot
    {
        public int Id {get; set;}
        public string Name {get; set;} = string.Empty;
        public string Description {get; set;} = string.Empty;
        public List<Post>? Posts {get; set;} = new List<Post>();
        public List<User>? Subscribers {get; set;} = new List<User>();
        public List<User>? Moderators {get; set;} = new List<User>();
    }
}