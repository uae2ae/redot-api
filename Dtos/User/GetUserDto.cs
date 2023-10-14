using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using redot_api.Dtos.Comment;
using redot_api.Dtos.Post;
using redot_api.Dtos.Subredot;

namespace redot_api.Dtos.User
{
    public class GetUserDto
    {
        public int Id {get; set;}
        public string Username {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        public byte[] PasswordHash {get; set;} = new byte[0];
        public byte[] PasswordSalt {get; set;} = new byte[0];
        public int Karma {get; set;} = 0;
        public string Photo {get; set;} = "https://tse2.mm.bing.net/th/id/OIP.RJW_LWU3sOxea5tcSfjoBAAAAA?pid=ImgDet&rs=1";
        public List<GetPostDto>? Posts {get; set;}
        public List<GetCommentDto>? Comments {get; set;}
        public List<Vote>? Votes { get; set; }
        public List<GetSubredotDto>? SubredotsSubscription { get; set; }
        public List<GetSubredotDto>? SubredotsModerators { get; set; }
        public string Role { get; set; } = "Regular";
    }
}