using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool PostVote { get; set; } // if false, comment vote
        public int PostId { get; set; } // if post vote is false, this is a comment vote
        public bool Upvote { get; set; }
    }
}