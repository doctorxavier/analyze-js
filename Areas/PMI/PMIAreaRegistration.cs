using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.PMI
{
    public class PMIAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "PMI";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "PMI_operation",
                "{operation}/PMI/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "PMI_default",
                "PMI/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}
