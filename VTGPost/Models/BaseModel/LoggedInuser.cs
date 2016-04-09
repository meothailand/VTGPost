using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VTGPost.Models.BaseModel
{
    public class LoggedInUser
    {
        public string UserName { get; set; }

        public string FullName { get; set; }

        public List<UserAccessList> AccessLists { get; set; }

        public LoggedInUser()
        {
            AccessLists = new List<UserAccessList>();
        }
    }
}