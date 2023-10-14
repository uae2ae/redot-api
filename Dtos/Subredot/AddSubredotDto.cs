using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace redot_api.Dtos.Subredot
{
    public class AddSubredotDto
    {
        public string Name {get; set;} = string.Empty;
        public string Description {get; set;} = string.Empty;
    }
}