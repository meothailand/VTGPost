using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VTGPost.Models.BaseModel
{
    public class UserInfoBase
    {
        public string UserName { get; set; }

        public string FullName { get; set; }

        public string NewPassword { get; set; }

        public string NewPassword2 { get; set; }

        public bool ResetPassword { get; set; }

        public bool IsActive { get; set; }
    }
}