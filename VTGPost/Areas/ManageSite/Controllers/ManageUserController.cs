using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VTGPost.Helper;
using VTGPost.Models;
using VTGPost.Models.BaseModel;

namespace VTGPost.Areas.ManageSite.Controllers
{
    public class ManageUserController : BaseController
    {
        //
        // GET: /ManageSite/ManageUser/

        public ViewResult Users(int page = 1)
        {
            using(var context = new WebsiteDBEntities())
            {
                if(HttpContext.Session[SiteConfig.TransferMessageSession]!=null)
                {
                    ViewBag.SaveMessage = HttpContext.Session[SiteConfig.TransferMessageSession];
                    HttpContext.Session[SiteConfig.TransferMessageSession] = null;
                }
                var users = context.UserInfoes.ToList();
                var paging = PagingHelper.Paging(users.Count, 15, page);
                ViewBag.Paging = paging;
                return View(users.GetRange(paging.StartRecord-1, paging.EndRecord + 1 - paging.StartRecord));   
            }
        }

        public ViewResult AddUser()
        {
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult AddUser(UserInfo user, string pass1, string pass2)
        {
            using (var context = new WebsiteDBEntities())
            {
                try
                {
                    if (pass1 != pass2)
                        throw new Exception("Mật khẩu và xác nhận mật khẩu không khớp");
                    if (pass1.Length < 6)
                        throw new Exception("Mật khẩu phải chứa ít nhất 6 ký tự");

                    var crypto = new Hash { Salt = Guid.NewGuid().ToString() };
                    user.UserCode = crypto.Salt;
                    user.PassCode = crypto.Encrypt(pass1);
                    context.UserInfoes.Add(user);
                    context.SaveChanges();
                    ViewBag.SaveMessage = string.Format("User {0} đã tạo thành công", user.UserName);

                    return View("EditUser", new UserInfoBase
                                                {
                                                    UserName = user.UserName,
                                                    FullName = user.FullName,
                                                    IsActive = user.IsActive,
                                                    ResetPassword = false
                                                });
                }
                catch(Exception ex)
                {
                    ViewBag.SaveMessage = string.Format("Lỗi: {0} .Không thể tạo user. Yêu cầu kiểm tra lại thông tin nhập", ex.Message);
                    return View(user);
                }
            }
        }

        public ActionResult EditUser(string userName)
        {
            using(var context = new WebsiteDBEntities())
            {
                var user = context.UserInfoes.SingleOrDefault(i => i.UserName == userName);
                if(user == null)
                {
                    HttpContext.Session[SiteConfig.TransferMessageSession] = string.Format("Không tìm thấy tài khoản tên {0}.", userName);
                    return RedirectToAction("Users");
                }
                var userinfo = new UserInfoBase
                                   {
                                       UserName = user.UserName,
                                       FullName = user.FullName,
                                       IsActive = user.IsActive,
                                       ResetPassword = false
                                   };
                return View(userinfo);
            }
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ViewResult EditUser(UserInfoBase userinfo)
        {
            using (var context = new WebsiteDBEntities())
            {
                try
                {
                    var currentUser = context.UserInfoes.SingleOrDefault(i => i.UserName == userinfo.UserName);
                    if (currentUser != null)
                    {
                        if (userinfo.ResetPassword)
                            ChangePassWord(currentUser, userinfo.NewPassword, userinfo.NewPassword2);

                        currentUser.IsActive = userinfo.IsActive;
                        currentUser.FullName = userinfo.FullName;
                        context.Entry(currentUser).State = EntityState.Modified;
                        context.SaveChanges();
                        ViewBag.SaveMessage = "Thông tin đã được lưu thành công";
                        userinfo.ResetPassword = false;
                    }
                    else
                    {
                        ViewBag.SaveMessage = string.Format("Không tìm thấy tài khoản tài khoản tên [{0}]. Yêu cầu quay lại trang danh sách và thử lại.", userinfo.UserName);
                    }
                }
                catch(Exception ex)
                {
                    ViewBag.SaveMessage = ex.Message;
                }
                return View(userinfo);
            }
        }

        public ActionResult DeleteUser(string userName)
        {
            using(var context = new WebsiteDBEntities())
            {
                var user = context.UserInfoes.SingleOrDefault(i => i.UserName == userName);
                if (user != null)
                {
                    foreach (var role in user.UserRoles)
                    {
                        context.UserRoles.Remove(role);
                    }
                    context.UserInfoes.Remove(user);
                    context.SaveChanges();
                    HttpContext.Session[SiteConfig.TransferMessageSession] = string.Format("Tài khoản tên {0} đã được xoá.", userName);
                }
                else
                {
                    HttpContext.Session[SiteConfig.TransferMessageSession] = string.Format("Không tìm thấy tài khoản {0} cần xoá.", userName);
                }
                return RedirectToAction("Users");
            }
        }

        public ViewResult Groups()
        {
            using(var context = new WebsiteDBEntities())
            {
                var groups = context.RoleInfoes.ToList();
                return View(groups);
            }
        }

        public ActionResult GrantRights(string userName)
        {
            using(var context = new WebsiteDBEntities())
            {
                if(!context.UserInfoes.Any(i => i.UserName == userName))
                {
                    Session[SiteConfig.TransferMessageSession] = "Không tìm thấy tài khoản " + userName;
                    return RedirectToAction("Users");
                }
                var addedGroup = context.UserRoles.Include("RoleInfo").Include("UserInfo").Where(i => i.UserName == userName);
                ViewBag.RoleInfoes =  (from role in context.RoleInfoes
                                      where !addedGroup.Any(i => i.RoleName == role.RoleName)
                                      select role).ToList();
                return View(addedGroup.ToList());
            }
        }

        [HttpPost]
        public void GrantRights(string groupNames, string btnAddUser, string userName)
        {
            using (var context = new WebsiteDBEntities())
            {
                if (!context.UserInfoes.Any(i => i.UserName == userName))
                {
                    Session[SiteConfig.TransferMessageSession] = "Không tìm thấy tài khoản " + userName;
                    RedirectToAction("Users");
                }

                ViewBag.SaveMessage = string.IsNullOrEmpty(btnAddUser)
                                          ? RemoveGroups(groupNames, userName, context)
                                          : AddGroups(groupNames, userName, context);

                //var addedGroup = context.UserRoles.Include("RoleInfo").Include("UserInfo").Where(i => i.UserName == userName);
                //ViewBag.RoleInfoes =  (from role in context.RoleInfoes
                //                      where !addedGroup.Any(i => i.RoleName == role.RoleName)
                //                      select role).ToList();
                //return View(addedGroup.ToList());
            }
        }

        private string AddGroups(string groupNames, string userName, WebsiteDBEntities context)
        {
            var addedIdStrs = groupNames.Split('|');
            var message = "Đã thêm tài khoản vào nhóm: ";
            foreach (var id in addedIdStrs)
            {
                var group = context.RoleInfoes.SingleOrDefault(i => i.RoleName == id);
                if (group == null || group.UserRoles.Any(i => i.RoleName == id && i.UserName == userName)) continue;
                context.UserRoles.Add(new UserRole
                {
                    RoleName = id,
                    UserName = userName
                });
                message += id + ", ";
            }
            context.SaveChanges();
            return message;
        }

        private string RemoveGroups(string groupNames, string userName, WebsiteDBEntities context)
        {
            var addedIdStrs = groupNames.Split('|');
            var message = "Đã xoá tài khoản khỏi các nhóm: ";
            foreach (var id in addedIdStrs)
            {
                if (context.UserRoles.SingleOrDefault(i => i.RoleName == id && i.UserName == userName) == null) continue;
                context.UserRoles.Remove(context.UserRoles.SingleOrDefault(i => i.UserName == userName && i.RoleName == id));
                message += id + ", ";
            }
            context.SaveChanges();
            return message;

        }

        private string AddUsersToGroup(string ids, RoleInfo group, WebsiteDBEntities context)
        {
            var addedIdStrs = ids.Split('|');
            var message = "Đã thêm những tài khoản sau vào nhóm: ";
            foreach (var id in addedIdStrs)
            {
                var user = context.UserInfoes.SingleOrDefault(i => i.UserName == id);
                if (user == null || !group.UserRoles.Any(i => i.RoleName ==@group.RoleName && i.UserName == id)) continue;
                context.UserRoles.Add(new UserRole
                                            {
                                                RoleName = group.RoleName,
                                                UserName = id
                                            });
                message += id + ", ";
            }
            context.SaveChanges();
            return message;
        }

        private string RemoveUsersFromGroup(string ids, RoleInfo role, WebsiteDBEntities context)
        {
            var addedIdStrs = ids.Split('|');
            var message = "Đã xoá những tài khoản sau khỏi nhóm: ";
            foreach (var id in addedIdStrs)
            {
                if (role.UserRoles.SingleOrDefault(i => i.UserName == id) == null) continue;
                context.UserRoles.Remove(role.UserRoles.SingleOrDefault(i => i.UserName == id));
                message += id + ", ";
            }
            context.SaveChanges();
            return message;
            
        }

        private void ChangePassWord(UserInfo user, string pass1, string pass2)
        {
            var crypto = new Hash { Salt = user.UserCode };
            
            if (pass1 != pass2)
                throw new Exception("Mật khẩu mới và xác nhận mật khẩu mới không khớp.");

            if (pass1.Length < 6)
                throw new Exception("Mật khẩu phải chứa ít nhất 6 ký tự");

            user.PassCode = crypto.Encrypt(pass1);
        }
    }
}
