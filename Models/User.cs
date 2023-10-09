using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Models
{
    public class User
    {
        public int Id {get; set;}
        public string Username {get; set;} = string.Empty;
        public string Email {get; set;} = string.Empty;
        public byte[] PasswordHash {get; set;} = new byte[0];
        public byte[] PasswordSalt {get; set;} = new byte[0];
        public int Karma {get; set;} = 0;
        public string Photo {get; set;} = "https://tse2.mm.bing.net/th/id/OIP.RJW_LWU3sOxea5tcSfjoBAAAAA?pid=ImgDet&rs=1";
        public List<Post>? Posts {get; set;}
        public List<Comment>? Comments {get; set;}
        public List<Vote>? Votes { get; set; }
        [Required]
        public string Role { get; set; } = "Regular";
    }
}