using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VTGPost.Helper;
using VTGPost.Models;

namespace VTGPost.Areas.ManageSite.Controllers
{
    public class ManageBannerController : BaseController
    {
        private Dictionary<int, string> _type = new Dictionary<int, string>
                                                       {
                                                           {1, "Top"},
                                                           {2, "Other"}
                                                       };
        //
        // GET: /ManageSite/ManageBanner/

        public ActionResult Banners(int page = 1)
        {
            if(Session[SiteConfig.TransferMessageSession] != null)
            {
                ViewBag.SaveMessage = Session[SiteConfig.TransferMessageSession];
                Session[SiteConfig.TransferMessageSession] = null;
            }
            using(var context = new WebsiteDBEntities())
            {
                var banners = context.BannerImages.ToList();
                var paging = PagingHelper.Paging(banners.Count, 10, page);
                ViewBag.Paging = paging;
                return View(banners.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
            }
        }

        public ViewResult NewBanner()
        {
            ViewBag.Category = new SelectList(_type, "Key", "Value", null);
            return View();
        }

        public ActionResult EditBanner(int id)
        {
            ViewBag.Category = new SelectList(_type, "Key", "Value", null);
            using(var context = new WebsiteDBEntities())
            {
                var banner = context.BannerImages.SingleOrDefault(i => i.ImageId == id);
                if(banner == null)
                {
                    Session[SiteConfig.TransferMessageSession] = "Thông tin bạn tìm không tồn tại";
                    return RedirectToAction("Banners", "ManageBanner");
                }
                return View(banner);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult NewBanner(BannerImage banner, HttpPostedFileBase bannerImage)
        {
            if(bannerImage != null && bannerImage.ContentLength > 0)
            {
                try
                {
                    var location = banner.BannerType == 1 ? SiteConfig.BannerFolder : SiteConfig.AdsFolder;
                    var filename = string.Empty;

                    SaveBannerImage(bannerImage, location, out filename);
                    banner.ImageName = filename;

                    using (var context = new WebsiteDBEntities())
                    {
                        context.BannerImages.Add(banner);
                        context.SaveChanges();
                    }
                    ViewBag.SaveMessage = "Thông tin lưu thành công";
                    ViewBag.Category = new SelectList(_type, "Key", "Value", banner.BannerType);
                    return View("EditBanner", banner);
                }
                catch(Exception ex)
                {
                    ViewBag.SaveMessage = ex.Message;
                }
            }
            return View(banner);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditBanner(BannerImage banner, HttpPostedFileBase bannerImage)
        {
            ViewBag.Category = new SelectList(_type, "Key", "Value", null);
            try
            {
                if (bannerImage != null && banner.ImageName != bannerImage.FileName && bannerImage.ContentLength > 0)
                {
                    var location = banner.BannerType == 1 ? SiteConfig.BannerFolder : SiteConfig.AdsFolder;
                    var filename = string.Empty;

                    SaveBannerImage(bannerImage, location, out filename);

                    if(System.IO.File.Exists(Server.MapPath(location + banner.ImageName)))
                    {
                        System.IO.File.Delete(Server.MapPath(location + banner.ImageName));
                    }

                    banner.ImageName = filename;
                }

                using (var context = new WebsiteDBEntities())
                {
                    context.BannerImages.Add(banner);
                    context.Entry(banner).State = EntityState.Modified;
                    context.SaveChanges();
                }
                ViewBag.SaveMessage = "Thông tin lưu thành công.";
            }
            catch (Exception ex)
            {
                ViewBag.SaveMessage = ex.Message;
            }
           
            return View(banner);
        }

        public ActionResult DeleteBanner(int id)
        {
            using(var context = new WebsiteDBEntities())
            {
                var banner = context.BannerImages.SingleOrDefault(i => i.ImageId == id);
                if (banner != null)
                {
                    var location = banner.BannerType == 1 ? SiteConfig.BannerFolder : SiteConfig.AdsFolder;

                    if (System.IO.File.Exists(Server.MapPath(location + banner.ImageName)))
                    {
                        System.IO.File.Delete(Server.MapPath(location + banner.ImageName));
                    }

                    context.BannerImages.Remove(banner);
                    context.SaveChanges();
                }
            }

            return RedirectToAction("Banners");
        }

        private void SaveBannerImage(HttpPostedFileBase image, string location, out string filename)
        {
            if (image == null || image.ContentLength <= 0)
                throw new Exception("Yêu cầu tải hình cho banner.");

            if (image.ContentType != "image/jpeg" && image.ContentType != "image/jpg" && image.ContentType != "image/png")
                throw new Exception("Định dạng file không được hỗ trợ");

            filename = string.Format("{0}{1}", DateTime.Now.Date.ToString("ddMMyyHHmmss"), image.FileName);
            var fullFileName = Path.Combine(Server.MapPath(location), filename);

            image.SaveAs(fullFileName);
        }

    }
}
