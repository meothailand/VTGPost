using System.Web;
using System.Web.Optimization;

namespace VTGPost
{
    public class BundleConfig
    {
        // For more information on Bundling, visit http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
                        "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.unobtrusive*",
                        "~/Scripts/jquery.validate*"));

            bundles.Add(new ScriptBundle("~/rondell/js").IncludeDirectory("~/Content/frontend/plugins/rondell", "*.js"));

            bundles.Add(new ScriptBundle("~/elastislide/js").Include("~/Content/frontend/plugins/elastislide/js/modernizr.custom.17475.js"));
            bundles.Add(new ScriptBundle("~/elastislide/js1").Include("~/Content/frontend/plugins/elastislide/js/jquerypp.custom.js",
                                                                      "~/Content/frontend/plugins/elastislide/js/jquery.elastislide.js"));

            bundles.Add(new ScriptBundle("~/nivoSlider/js").Include("~/Content/frontend/plugins/nivo-slider/jquery.nivo.slider.js"));

            bundles.Add(new ScriptBundle("~/SpryAssets/js").Include("~/Content/frontend/SpryAssets/SpryMenuBar.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include("~/Content/site.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
                        "~/Content/themes/base/jquery.ui.core.css",
                        "~/Content/themes/base/jquery.ui.resizable.css",
                        "~/Content/themes/base/jquery.ui.selectable.css",
                        "~/Content/themes/base/jquery.ui.accordion.css",
                        "~/Content/themes/base/jquery.ui.autocomplete.css",
                        "~/Content/themes/base/jquery.ui.button.css",
                        "~/Content/themes/base/jquery.ui.dialog.css",
                        "~/Content/themes/base/jquery.ui.slider.css",
                        "~/Content/themes/base/jquery.ui.tabs.css",
                        "~/Content/themes/base/jquery.ui.datepicker.css",
                        "~/Content/themes/base/jquery.ui.progressbar.css",
                        "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new StyleBundle("~/frontendStyle").IncludeDirectory("~/Content/frontend/css", "*.css"));

            bundles.Add(new StyleBundle("~/nivoSlider").Include(
                        "~/Content/frontend/plugins/nivo-slider/themes/light/light.css",
                        "~/Content/frontend/plugins/nivo-slider/themes/bar/bar.css",
                        "~/Content/frontend/plugins/nivo-slider/nivo-slider.css"));

            bundles.Add(new StyleBundle("~/elastislide").Include("~/Content/frontend/plugins/elastislide/css/elastislide.css"));

            bundles.Add(new StyleBundle("~/SpryAssets").Include("~/Content/frontend/SpryAssets/SpryMenuBarHorizontal.css"));

            bundles.Add(new StyleBundle("~/rondell").Include("~/Content/frontend/plugins/rondell/jquery.rondell.css"));

            //Backend bundle
           
        }
    }
}