using System;
using System.Linq;
using System.Web.Mvc;
using VTGPost.Helper;
using VTGPost.Models;
using VTGPost.Models.BaseModel;

namespace VTGPost.Areas.ManageSite.Controllers
{
    public class CredentialsController : Controller
    {
        //
        // GET: /ManageSite/Credentials/

        public ActionResult Login()
        {
            ViewBag.Message = string.Empty;
            return View();
        }

        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Login(UserInfo credential)
        {
            using(var context = new WebsiteDBEntities())
            {
                var user = context.UserInfoes.Find(credential.UserName);
                if (user != null)
                {
                    var crypto = new Hash { Salt = user.UserCode };
                    var encyptPass = crypto.Encrypt(credential.PassCode);
                    if(encyptPass == user.PassCode && user.IsActive)
                    {
                        var userSession = new LoggedInUser
                                              {
                                                  FullName = user.FullName,
                                                  UserName = user.UserName
                                              };
                        LoadUserAccessList(context, userSession);
                        HttpContext.Session[SiteConfig.UserSession] = userSession;

                        return RedirectToAction("SiteMessage", "User", new { id = 1 });
                    }
                    ViewBag.Message = "Sai mật khẩu hoặc tài khoản đã bị khoá";
                }
                else
                {
                    ViewBag.Message = "Tên đăng nhập không đúng";
                }
                return View();
            }
        }

        private void LoadUserAccessList(WebsiteDBEntities context, LoggedInUser user)
        {
            var grantedRoles = context.UserRoles.Where(i => i.UserName == user.UserName);
            foreach(var role in grantedRoles)
            {
                var roleAccess = context.AccessControlLists.FirstOrDefault(i => i.RoleName == role.RoleName);
                if (roleAccess == null) continue;
                var parentMenu = new UserAccessList
                                     {
                                         Menu = roleAccess.AccessList.MenuDisplayText,
                                         Action = roleAccess.AccessList.RelativeLink,
                                         Children = roleAccess.AccessList
                                                              .AccessList1.Select(i => new UserAccessList
                                                                          {
                                                                              Menu = i.MenuDisplayText,
                                                                              Action = i.RelativeLink,
                                                                              IsAccessed = false,
                                                                              IsActie = i.IsActive
                                                                          }).ToList(),
                                         IsAccessed = false
                                     };

                user.AccessLists.Add(parentMenu);
            }
        }

    }
}
