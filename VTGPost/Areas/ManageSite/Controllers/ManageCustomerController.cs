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
    public class ManageCustomerController : BaseController
    {
        //
        // GET: /ManageSite/ManageCustomer/

        public ViewResult Customers(int page = 1)
        {
            if (HttpContext.Session[SiteConfig.TransferMessageSession] != null)
            {
                ViewBag.SaveMessage = HttpContext.Session[SiteConfig.TransferMessageSession];
                HttpContext.Session[SiteConfig.TransferMessageSession] = null;
            }
            using (var context = new WebsiteDBEntities())
            {
                var customers = context.Customers.ToList();
                var paging = PagingHelper.Paging(customers.Count, 15, page);
                ViewBag.Paging = paging;
                return View(customers.GetRange(paging.StartRecord - 1, paging.EndRecord + 1 - paging.StartRecord));
            }
        }

        public ViewResult AddCustomer()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddCustomer(Customer customer, HttpPostedFileBase logoImg)
        {
            try
            {
                using (var context = new WebsiteDBEntities())
                {
                    if (logoImg != null && logoImg.ContentLength > 0)
                    {
                        var filename = string.Empty;
                        SaveBannerImage(logoImg, out filename);
                        customer.Logo = filename;
                    }

                    context.Customers.Add(customer);
                    context.SaveChanges();
                        
                    ViewBag.SaveMessage = "Thông tin lưu thành công";
                    return View("EditCustomer", customer);
                }
            }
            catch (Exception ex)
            {
                ViewBag.SaveMessage = ex.Message;
            }
            return View(customer);
        }

        public ActionResult EditCustomer(int id)
        {
            using (var context = new WebsiteDBEntities())
            {
                var customer = context.Customers.SingleOrDefault(i => i.Id == id);
                if (customer == null)
                {
                    Session[SiteConfig.TransferMessageSession] = "Thông tin bạn tìm không tồn tại";
                    return RedirectToAction("Customers", "ManageCustomer");
                }
                return View(customer);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult EditCustomer(Customer customer, HttpPostedFileBase logoImg)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (logoImg != null && logoImg.ContentLength > 0 && logoImg.FileName != customer.Logo)
                    {
                        if (!string.IsNullOrEmpty(customer.Logo) && 
                            System.IO.File.Exists(Path.Combine(Server.MapPath(SiteConfig.CustomerLogo), customer.Logo)))
                        {
                            System.IO.File.Delete(Path.Combine(Server.MapPath(SiteConfig.CustomerLogo), customer.Logo));
                        }

                        var filename = string.Empty;

                        SaveBannerImage(logoImg, out filename);
                        customer.Logo = filename;
                    }
                    using (var context = new WebsiteDBEntities())
                    {
                        context.Customers.Attach(customer);
                        context.Entry(customer).State = EntityState.Modified;
                        context.SaveChanges();
                        ViewBag.SaveMessage = "Thông tin đã lưu thành công";
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.SaveMessage = ex.Message;
                }
            }
            else
            {
                ViewBag.SaveMessage = "Không thể lưu dữ liệu. Kiểm tra lại thông tin nhập";
            }
            return View(customer);
        }

        public ActionResult DeleteCustomer(int id)
        {
            using (var context = new WebsiteDBEntities())
            {
                var customer = context.Customers.SingleOrDefault(i => i.Id == id);
                if (customer != null)
                {
                    context.Customers.Remove(customer);
                    context.SaveChanges();
                    HttpContext.Session[SiteConfig.TransferMessageSession] = "Dữ liệu đã xoá thành công.";
                }
                else
                {
                    HttpContext.Session[SiteConfig.TransferMessageSession] = "Dữ liệu yêu cầu xoá không tồn tại";
                }
                return RedirectToAction("Customers");
            }
        }

        private void SaveBannerImage(HttpPostedFileBase image, out string filename)
        {

            if (image.ContentType != "image/jpeg" && image.ContentType != "image/jpg" && image.ContentType != "image/png")
                throw new Exception("File type is not supported.");

            filename = string.Format("{0}{1}", DateTime.Now.Date.ToString("ddMMyyHHmmss"), image.FileName);
            var fullFileName = Path.Combine(Server.MapPath(SiteConfig.CustomerLogo), filename);

            image.SaveAs(fullFileName);
        }
    }
}
