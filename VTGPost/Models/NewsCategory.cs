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
    
    public partial class NewsCategory
    {
        public NewsCategory()
        {
            this.News = new HashSet<News>();
        }
    
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
    
        public virtual ICollection<News> News { get; set; }
    }
}
