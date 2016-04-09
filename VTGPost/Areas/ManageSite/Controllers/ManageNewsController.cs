using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VTGPost.Helper;
using VTGPost.Models;

namespace VTGPost.Areas.ManageSite.Controllers
{
    public class ManageNewsController : BaseController
    {
        //
        // GET: /ManageSite/ManageNews/

        private Dictionary<int, string> _category = new Dictionary<int, string>
                                                        {
                                                            {SiteConfig.ServicesPage, "Services - Dịch vụ"},
                                                            {
                                                                SiteConfig.QualityControlPage,
                                                                "Quality Control - Quản lý chất lượng"
                                                            },
                                                            {SiteConfig.Recruitment, "Recruitment - Tuyển dụng"},
                                                            {SiteConfig.HotNews, "News - Tin tức"}
                                                        };

        public ActionResult News(int page = 1)
        {
            using(var context = new WebsiteDBEntities())
            {
                if (HttpContext.Session[SiteConfig.TransferMessageSession] != null)
                {
                    ViewBag.SaveMessage = HttpContext.Session[SiteConfig.TransferMessageSession];
                    HttpContext.Session[SiteConfig.TransferMessageSession] = null;
                }

                var newsList = context.News.Include("NewsCategory").Where(i => i.Category != SiteConfig.HomePage && i.Category != SiteConfig.AboutPage).ToList();
                var paging = PagingHelper.Paging(newsList.Count, 15, page);
                ViewBag.Paging = paging;
                return View(newsList.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
            }
        }

        public ActionResult DeleteNews(int id)
        {
            using (var context = new WebsiteDBEntities())
            {
                var news = context.News.SingleOrDefault(i => i.NewsId == id && 
                                                             i.Category != SiteConfig.HomePage && 
                                                             i.Category != SiteConfig.AboutPage);
                if (news != null)
                {
                    context.News.Remove(news);
                    context.SaveChanges();
                    HttpContext.Session[SiteConfig.TransferMessageSession] = "Tin tức đã được xoá";
                }
                else
                {
                    HttpContext.Session[SiteConfig.TransferMessageSession] = "Thông tin yêu cầu xoá không tồn tại";
                }
                return RedirectToAction("News");
            }
        }

        public ViewResult PostNews(int? id)
        {
            using(var context = new WebsiteDBEntities())
            {
                var news = context.News.Include("NewsCategory")
                                       .SingleOrDefault(
                                                        i => i.NewsId == id.Value && 
                                                        (i.Category >= SiteConfig.ServicesPage && 
                                                         i.Category <= SiteConfig.Recruitment));
                if(news != null) news.Content = HttpUtility.HtmlDecode(news.Content);
                ViewBag.Category = new SelectList(_category, "Key", "Value", null);
                return View(news);
            }
        }
        
        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult PostNews(News postedNews)
        {
            postedNews.Title = string.IsNullOrEmpty(postedNews.Title) ? "Tin tức" : postedNews.Title;
            postedNews.Content = HttpUtility.HtmlEncode(postedNews.Content);
            ViewBag.Category = new SelectList(_category, "Key", "Value", null);
            if (postedNews.Category > 2)
            {
               try
               {
                   SaveNews(postedNews, postedNews.NewsId > 0 ? EntityState.Modified : EntityState.Added);
                   ViewBag.SaveMessage = "Entry is successfully saved.";
               }
               catch(Exception ex)
               {
                   ViewBag.SaveMessage = ex.Message;
               }
            }
            return View(postedNews);
        }

        public ViewResult About()
        {
            using(var context = new WebsiteDBEntities())
            {
                var intro = context.News.Include("NewsCategory").FirstOrDefault(i => i.Category == SiteConfig.AboutPage);
                if (intro != null) intro.Content = HttpUtility.HtmlDecode(intro.Content);
                ViewBag.PostedTo = "About";
                return View("InfoPage", intro);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult About(News postedNews)
        {
            postedNews.Content = HttpUtility.HtmlEncode(postedNews.Content);
            ViewBag.PostedTo = "About";
            if (postedNews.Category == SiteConfig.AboutPage)
            {
                try
                {
                    SaveNews(postedNews, EntityState.Modified);
                    ViewBag.SaveMessage = "Entry is successfully saved.";
                }
                catch (Exception ex)
                {
                    ViewBag.SaveMessage = ex.Message;
                }
            }
            return View("InfoPage", postedNews);
        }

        public ViewResult Homepage()
        {
            using (var context = new WebsiteDBEntities())
            {
                var page = context.News.Include("NewsCategory").FirstOrDefault(i => i.Category == SiteConfig.HomePage);
                if (page != null) page.Content = HttpUtility.HtmlDecode(page.Content);
                ViewBag.PostedTo = "Homepage";
                return View("InfoPage", page);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [ValidateInput(false)]
        public ActionResult HomePage(News postedNews)
        {
            postedNews.Content = HttpUtility.HtmlEncode(postedNews.Content);
            ViewBag.PostedTo = "Homepage";
            if ( postedNews.Category == SiteConfig.HomePage)
            {
                try
                {
                    SaveNews(postedNews, EntityState.Modified);
                    ViewBag.SaveMessage = "Entry is successfully saved.";
                }
                catch (Exception ex)
                {
                    ViewBag.SaveMessage = ex.Message;
                }
            }
            return View("InfoPage", postedNews);
        }

        private void SaveNews(News news, EntityState state)
        {
            using (var context = new WebsiteDBEntities())
            {
                news.PostedDate = state == EntityState.Added ? DateTime.Now : news.PostedDate;
                context.News.Attach(news);
                context.Entry(news).State = state;
                context.SaveChanges();
                news.NewsCategory = context.NewsCategories.SingleOrDefault(i => i.CategoryId == news.Category);
            }
        }
    }
}
