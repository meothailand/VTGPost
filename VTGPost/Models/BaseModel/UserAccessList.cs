using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VTGPost.Models.BaseModel
{
    public class UserAccessList
    {
        public string Menu { get; set; }

        public string Action { get; set; }

        public bool IsAccessed { get; set; }

        public bool IsActie { get; set; }

        public List<UserAccessList> Children { get; set; }

        public UserAccessList()
        {
            Children = new List<UserAccessList>();
        }
    }
}
