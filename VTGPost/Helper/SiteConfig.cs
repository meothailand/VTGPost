using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VTGPost.Helper
{
    public class SiteConfig
    {
        public const string UserSession = "CurrentLoginUserSession";
        public const string BillFilterCriteriaSession = "BillFilterCriteriaSession";
        public const string BillSearchCriteriaSession = "BillSearchCriteriaSession";

        public const string BannerFolder = "~/Images/banners/";
        public const string CustomerLogo = "~/Images/recent-customers/";
        public const string AdsFolder = "~/Images/ads/";
        public const string HotNewsCache = "CacheHotNews";
        public const string BannerCache = "CacheBanners";
        public const string CustomerCache = "CacheCustomers";
        public const string HomepageContentCache = "CacheHomepageContent";
        public const string ServicePagesCache = "ServicePagesCache";

        public const int HomePage = 1;
        public const int AboutPage = 2;
        public const int ServicesPage = 3;
        public const int QualityControlPage = 4;
        public const int Recruitment = 5;
        public const int HotNews = 6;

        public const string ProcessStatus = "Processing";
        public const string Retransfer1Status = "Retransfer1";
        public const string Retransfer2Status = "Retransfer2";
        public const string CompleteStatus = "Complete";
        public const string FailStatus = "Fail";

        public const string TransferMessageSession = "TransferMessageSession";

        public static Dictionary<string, string> LadingStatus = new Dictionary<string, string>
                                                                    {
                                                                        {ProcessStatus, "Đang xử lý"},
                                                                        {Retransfer1Status, "Phát lại lần 1"},
                                                                        {Retransfer2Status, "Phát lại lần 2"},
                                                                        {CompleteStatus , "Phát thành công"},
                                                                        {FailStatus, "Không chuyển được"}
                                                                    };

        

    }
}