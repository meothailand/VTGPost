using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VTGPost.Models.BaseModel
{
    public class BillFilterCriteria
    {
        public Nullable<int> CustomerId { get; set; }

        public string CustomerName { get; set; }

        public Nullable<DateTime> SentDateFrom { get; set; }

        public Nullable<DateTime> SentDateTo { get; set; }

        public Nullable<DateTime> ReceivedDateFrom { get; set; }

        public Nullable<DateTime> ReceivedDateTo { get; set; }

        public string BillId { get; set; }
    }
}