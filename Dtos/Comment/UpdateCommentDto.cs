using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Dtos.Comment
{
    public class UpdateCommentDto
    {
        public string Type {get; set;} = string.Empty;
        public string Content {set; get;} = string.Empty;
        public DateTime LastEdited {set; get;} = DateTime.Now;
    }
}