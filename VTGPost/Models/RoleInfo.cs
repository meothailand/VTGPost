//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VTGPost.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RoleInfo
    {
        public RoleInfo()
        {
            this.AccessControlLists = new HashSet<AccessControlList>();
            this.UserRoles = new HashSet<UserRole>();
        }
    
        public string RoleName { get; set; }
        public string RoleDescription { get; set; }
    
        public virtual ICollection<AccessControlList> AccessControlLists { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
