using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VTGPost.Helper;

namespace VTGPost.Areas.ManageSite.Controllers
{
    public class ActivationController : Controller
    {
        //
        // GET: /ManageSite/Activation/

        public ActionResult Index()
        {
            if (Session[SiteConfig.UserSession] == null)
            {
                return RedirectToAction("Login", "Credentials");
            }
            return View();
        }

    }
}
