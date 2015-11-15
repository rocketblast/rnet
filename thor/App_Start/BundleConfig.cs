using System.Web;
using System.Web.Optimization;

namespace thor.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
#if DEBUG
            bundles.Add(new Bundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new Bundle("~/bundles/signalr").Include(
                "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new Bundle("~/bundles/angular").Include(
                "~/Scripts/angular.js",
                //"~/Scripts/angular.min.js",
                "~/Scripts/angular-*"));

            bundles.Add(new Bundle("~/bundles/app").IncludeDirectory(
                "~/app", "*.js", true));
#else
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Scripts/angular-*"));

            bundles.Add(new ScriptBundle("~/bundles/app").IncludeDirectory(
                "~/app", "*.js", true));
#endif

            BundleTable.EnableOptimizations = false;
        }
    }
}