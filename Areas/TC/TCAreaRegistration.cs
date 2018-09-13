using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.TC
{
    public class TCAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "TC";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "TC_default",
                "{operationNumber}/TC/{controller}/{action}",
                new
                {
                    action = "TCAbstract"
                });

            context.MapRoute("wo_operation_routing",
                                "TC/{controller}/{action}");
        }
    }
}
