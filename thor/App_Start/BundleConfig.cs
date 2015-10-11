using System.Web;
using System.Web.Optimization;

namespace thor.App_Start
{
    public class BundleConfig
    {
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/signalr").Include(
                "~/Scripts/jquery.signalR-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/Scripts/angular-*"));

            bundles.Add(new ScriptBundle("~/bundles/app").IncludeDirectory(
                "~/app", "*.js", true));

            BundleTable.EnableOptimizations = false;
        }
    }
}