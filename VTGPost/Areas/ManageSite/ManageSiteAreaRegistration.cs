using System.Web.Mvc;

namespace VTGPost.Areas.ManageSite
{
    public class ManageSiteAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ManageSite";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ManageSite_default",
                "ManageSite/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
