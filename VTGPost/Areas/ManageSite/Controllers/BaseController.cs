using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VTGPost.Helper;
using VTGPost.Models.BaseModel;

namespace VTGPost.Areas.ManageSite.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (Session[SiteConfig.UserSession] == null)
            {
                filterContext.Result = RedirectToAction("Login", "Credentials");
                return;
            }

            var controller = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var menus = ((LoggedInUser)Session[SiteConfig.UserSession]).AccessLists;
            var currentMenu = menus.SingleOrDefault(i => i.Action == controller);
            if (currentMenu == null)
            {
                if (controller != "User")
                        filterContext.Result = RedirectToAction("SiteMessage", "User", new { id = 3 });
            }
            else
            {
                foreach (var menu in menus.Where(i => i.IsAccessed == true))
                {
                    menu.IsAccessed = false;
                }
                currentMenu.IsAccessed = true;
            }
        }

    }
}
