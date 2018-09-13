using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Search
{
    public class SearchAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Search";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                name: "Search_operation",
                url: "{operation}/Search/{controller}/{action}/{id}",
                defaults: new { action = "Index", controller = "Search" },
                constraints: new { operation = "(?!Search).*" });

            context.MapRoute(
                "Search_default",
                "Search/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}
