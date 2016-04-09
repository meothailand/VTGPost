using System;
using System.Collections.Generic;

namespace VTGPost.Helper
{
    public class PagingHelper
    {
        public int TotalPage { get; set; }

        public int CurrentPage { get; set; }

        public int StartRecord { get; set; }

        public int EndRecord { get; set; }

        public int TotalRecord { get; set; }

        public static PagingHelper Paging(int totalRecord, int recordsPerPage = 10, int pageNo = 1)
        {
            if (totalRecord == 0)
            {
                return new PagingHelper
                           {
                               TotalPage = 0,
                               TotalRecord = 0,
                               StartRecord = 1,
                               EndRecord = 0,
                               CurrentPage = 0
                           };
            }

            var pageInfo = new PagingHelper {TotalRecord = totalRecord};
            pageInfo.TotalPage = pageInfo.TotalRecord%recordsPerPage > 0
                                     ? pageInfo.TotalRecord/recordsPerPage + 1
                                     : pageInfo.TotalRecord/recordsPerPage;
            if (pageInfo.TotalPage >= pageNo)
            {
                pageInfo.CurrentPage = pageNo;
                pageInfo.StartRecord = (pageInfo.CurrentPage - 1)*recordsPerPage + 1;
                var end = pageInfo.StartRecord + recordsPerPage - 1;
                pageInfo.EndRecord = pageInfo.TotalRecord >= end ? end : pageInfo.TotalRecord;

                return pageInfo;
            }
            throw new Exception("Page not found");
        } 

    }
}