using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace redot_api.Models
{
    public enum Order
    {
        NewToOld = 1,
        OldToNew = 2,
        Hot = 3,
        Top = 4,
        Controversial = 5,
        Rizing = 6

    }
}