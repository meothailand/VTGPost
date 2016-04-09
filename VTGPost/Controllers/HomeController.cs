using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VTGPost.Helper;
using VTGPost.Models;

namespace VTGPost.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Home = "active";
            LoadCache();
            using (var context = new WebsiteDBEntities())
            {
                ViewBag.Ads = context.BannerImages.Where(i => i.BannerType == 2 && i.IsActive).ToList();
                if (HttpContext.Cache[SiteConfig.HomepageContentCache] == null) CacheHelper.CacheHomepage();
                return View(HttpContext.Cache[SiteConfig.HomepageContentCache]);
            }
        }

        public ActionResult About()
        {
            ViewBag.About = "active";
            LoadCache();
            using(var context = new WebsiteDBEntities())
            {
                var intro = context.News.FirstOrDefault(i => i.Category == SiteConfig.AboutPage);
                intro.Content = HttpUtility.HtmlDecode(intro.Content);
                return View(intro);
            }
        }

        public ActionResult Services(int id)
        {
            ViewBag.Services = "MenuBarItemSubmenu active";
            LoadCache();
            return View(LoadNews(id, SiteConfig.ServicesPage));
        }

        public ActionResult News(int? id, int page = 1)
        {
            ViewBag.Info = "active";
            LoadCache();
            using (var context = new WebsiteDBEntities())
            {
                var newsList = context.News.Where(i => i.Category == SiteConfig.HotNews).ToList();
                var paging = PagingHelper.Paging(newsList.Count, 5, page);
                ViewBag.Paging = paging;
                ViewBag.News = id.HasValue ? LoadNews(id.Value, SiteConfig.HotNews) : null;
                return View(newsList.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
            }
        }

        public ActionResult Control(int? id, int page = 1)
        {
            ViewBag.Control = "active";
            LoadCache();
            using (var context = new WebsiteDBEntities())
            {
                var newsList = context.News.Where(i => i.Category == SiteConfig.QualityControlPage).ToList();
                var paging = PagingHelper.Paging(newsList.Count, 5, page);
                ViewBag.Paging = paging;
                ViewBag.News = id.HasValue ? LoadNews(id.Value, SiteConfig.QualityControlPage) : null;
                return View(newsList.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
            }
        }

        public ActionResult Contact()
        {
            ViewBag.Contact = "active";
            LoadCache();
            return View();
        }

        public ActionResult Recruitment(int? id, int page = 1)
        {
            ViewBag.Recruitment = "active";
            LoadCache();
            using (var context = new WebsiteDBEntities())
            {
                var newsList = context.News.Where(i => i.Category == SiteConfig.Recruitment).ToList();
                var paging = PagingHelper.Paging(newsList.Count, 10, page);
                ViewBag.Paging = paging;
                ViewBag.News = id.HasValue ? LoadNews(id.Value, SiteConfig.Recruitment) : null;
                return View(newsList.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
            }
        }

        public ActionResult TrackAndTrace(string tracking_id)
        {
            LoadCache();
            using(var context = new WebsiteDBEntities())
            {
                var tracking = context.BillOfLadings.SingleOrDefault(i => i.BillCode == tracking_id);
                if (tracking == null) ViewBag.TrackingID = tracking_id;
                return View(tracking);
            }
        }

        private void LoadCache()
        {
            if (HttpContext.Cache[SiteConfig.BannerCache] == null) CacheHelper.CacheBanner();
            if (HttpContext.Cache[SiteConfig.HotNewsCache] == null) CacheHelper.CacheHotNews();
            if (HttpContext.Cache[SiteConfig.CustomerCache] == null) CacheHelper.CacheCustomer();
            if (HttpContext.Cache[SiteConfig.ServicePagesCache] == null) CacheHelper.CacheServicesInfo();

            ViewBag.Banners = HttpContext.Cache.Get(SiteConfig.BannerCache);
            ViewBag.Customers = HttpContext.Cache.Get(SiteConfig.CustomerCache);
            ViewBag.News = HttpContext.Cache.Get(SiteConfig.HotNewsCache);
            ViewBag.ServicePages = HttpContext.Cache.Get(SiteConfig.ServicePagesCache);
        }

        private News LoadNews(int id, int category)
        {
            using (var context = new WebsiteDBEntities())
            {
                var news = context.News.SingleOrDefault(i => i.NewsId == id && i.Category == category);
                if(news != null) news.Content = HttpUtility.HtmlDecode(news.Content);
                return news;
            }
        }
    }
}
