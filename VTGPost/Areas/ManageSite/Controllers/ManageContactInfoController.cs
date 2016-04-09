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
    public class ManageContactInfoController : BaseController
    {
        //
        // GET: /ManageSite/ManageContactInfo/

        public ViewResult Hotlines(int page = 1)
        {
            if (HttpContext.Session[SiteConfig.TransferMessageSession] != null)
            {
                ViewBag.SaveMessage = HttpContext.Session[SiteConfig.TransferMessageSession];
                HttpContext.Session[SiteConfig.TransferMessageSession] = null;
            }

            using (var context = new WebsiteDBEntities())
            {
                var lines = context.HotLines.ToList();
                var paging = PagingHelper.Paging(lines.Count, 15, page);
                ViewBag.Paging = paging;

                return View(lines.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
            }
        }

        public ViewResult AddHotline()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddHotline(HotLine line, bool type = false)
        {
            try
            {
                using (var context = new WebsiteDBEntities())
                {
                    line.LineType = type ? "Skype" : "Yahoo";
                    context.HotLines.Add(line);
                    context.SaveChanges();
                    ViewBag.SaveMessage = "Thông tin đã được lưu thành công.";
                    return View(new HotLine());
                }
            }
            catch (Exception ex)
            {
                ViewBag.SaveMessage = ex.Message;
                return View(line);
            }
        }

        public ActionResult EditHotline(int id)
        {
            using (var context = new WebsiteDBEntities())
            {
                var info = context.HotLines.SingleOrDefault(i => i.Id == id);
                if (info == null)
                {
                    Session[SiteConfig.TransferMessageSession] = "Thông tin bạn tìm không tồn tại";
                    return RedirectToAction("Hotlines", "ManageContactInfo");
                }
                return View(info);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditHotline(HotLine line, bool type = false)
        {
            try
            {
                using (var context = new WebsiteDBEntities())
                {
                    line.LineType = type ? "Skype" : "Yahoo";
                    context.HotLines.Attach(line);
                    context.Entry(line).State = EntityState.Modified;
                    context.SaveChanges();
                    ViewBag.SaveMessage = "Thông tin đã được lưu thành công.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.SaveMessage = ex.Message;
            }
            return View(line);
        }

        public ActionResult DeleteLine(int id)
        {
            using (var context = new WebsiteDBEntities())
            {
                var line = context.HotLines.SingleOrDefault(i => i.Id == id);
                if (line != null)
                {
                    context.HotLines.Remove(line);
                    context.SaveChanges();
                    HttpContext.Session[SiteConfig.TransferMessageSession] = "Dữ liệu đã xoá thành công.";
                }
                else
                {
                    HttpContext.Session[SiteConfig.TransferMessageSession] = "Dữ liệu cần xoá không tồn tại.";
                }
                return RedirectToAction("Hotlines");
            }
        }
    }
}
