using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VTGPost.Helper;
using VTGPost.Models;
using VTGPost.Models.BaseModel;

namespace VTGPost.Areas.ManageSite.Controllers
{
    public class UserController : BaseController
    {
        //
        // GET: /ManageSite/User/

        /// <summary>
        /// Use to display message after logged in or some error message
        /// </summary>
        /// <param name="id">the message type determine the content of the page: 
        /// 1 - logged in welcome message 
        /// 2 - activation required
        /// 3 - 404 or unauthorized access message
        /// 4 - </param>
        /// <returns></returns>
        public ViewResult SiteMessage(int id = 3)
        {
            ViewBag.MessageType = id;
            if (id == 1)
            {
                if(Session[SiteConfig.TransferMessageSession] !=null)
                {
                    ViewBag.SaveMessage = Session[SiteConfig.TransferMessageSession];
                    Session[SiteConfig.TransferMessageSession] = null;
                }
                var user = (LoggedInUser) Session[SiteConfig.UserSession];
                return View(new UserInfoBase
                {
                    FullName = user.FullName,
                    UserName = user.UserName,
                    IsActive = true
                });
            }

            if (id == 3)
                ViewBag.PreviousPage = HttpContext.Request.UrlReferrer != null
                                           ? HttpContext.Request.UrlReferrer.AbsolutePath
                                           : "/ManageSite/User/SiteMessage/1";

            return View();
        }

        [HttpPost]
        public ActionResult ChangePassword(UserInfoBase user, string currentPassword)
        {
            using (var context = new WebsiteDBEntities())
            {
                try
                {
                    var currentuser = context.UserInfoes.SingleOrDefault(i => i.UserName == user.UserName && i.IsActive);
                    if (currentuser == null)
                        throw new Exception(
                            "Tài khoản của bạn đã bị khoá hoặc xoá. Liên hệ quản lý của bạn để được giúp đỡ.");

                    var crypto = new Hash {Salt = currentuser.UserCode};
                    var currentPass = crypto.Encrypt(currentPassword);

                    if (currentPass != currentuser.PassCode)
                        throw new Exception("Mật khẩu hiện tại không đúng.");

                    if (user.NewPassword != user.NewPassword2)
                        throw new Exception("Mật khẩu và xác nhận mật khẩu không khớp");
                    if (user.NewPassword.Length < 6)
                        throw new Exception("Mật khẩu phải chứa ít nhất 6 ký tự");

                    currentuser.PassCode = crypto.Encrypt(user.NewPassword);
                    context.Entry(currentuser).State = EntityState.Modified;
                    context.SaveChanges();
                    user.ResetPassword = false;
                    Session[SiteConfig.TransferMessageSession] = "Mật khẩu đã đổi thành công";
                }
                catch(Exception ex)
                {
                    Session[SiteConfig.TransferMessageSession] = string.Format("Lỗi: {0}", ex.Message);
                }
                return RedirectToAction("SiteMessage", new {id = 1});
            }
        }

    }
}
