using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using VTGPost.Models;

namespace VTGPost.Helper
{
    public class CacheHelper
    {
        public static void CacheBanner()
        {
          using(var context = new WebsiteDBEntities())
          {
              var banners = context.BannerImages.Where(i => i.BannerType == 1 && i.IsActive).ToList();
              HttpContext.Current.Cache.Insert(SiteConfig.BannerCache, banners, null, Cache.NoAbsoluteExpiration,
                                               new TimeSpan(0,10,0));
          }
        }

        public static void CacheHotNews()
        {
            using(var context = new WebsiteDBEntities())
            {
                var news = context.News.Where(i => i.Category == SiteConfig.HotNews).OrderByDescending(i => i.PostedDate).Take(5).ToList();
                HttpContext.Current.Cache.Insert(SiteConfig.HotNewsCache, news, null, Cache.NoAbsoluteExpiration,
                                                 new TimeSpan(0, 10, 0));
            }
        }

        public static void CacheCustomer()
        {
            using (var context = new WebsiteDBEntities())
            {
                var customers = context.Customers.ToList();
                HttpContext.Current.Cache.Insert(SiteConfig.CustomerCache, customers, null, Cache.NoAbsoluteExpiration,
                                                 new TimeSpan(0, 10, 0));
            }
        }

        public static void CacheServicesInfo()
        {
            using (var context = new WebsiteDBEntities())
            {
                var news = context.News.Where(i => i.Category == SiteConfig.ServicesPage).ToList();
                HttpContext.Current.Cache.Insert(SiteConfig.ServicePagesCache, news, null, Cache.NoAbsoluteExpiration,
                                                 new TimeSpan(1, 0, 0));
            }
        }

        public static void CacheHomepage()
        {
            using (var context = new WebsiteDBEntities())
            {
                var content = context.News.FirstOrDefault(i => i.Category == SiteConfig.HomePage);
                HttpContext.Current.Cache.Insert(SiteConfig.HomepageContentCache, content, null, Cache.NoAbsoluteExpiration,
                                                 new TimeSpan(24, 0, 0));
            }
        }
    }
}