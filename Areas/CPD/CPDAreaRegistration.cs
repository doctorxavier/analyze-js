using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.CPD
{
    public class CPDAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "CPD";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("CPD_default", "{operationNumber}/CPD/{controller}/{action}", new { action = "Index" });

            context.MapRoute("cpd_operation_routing",
                           "CPD/{controller}/{action}");
        }
    }
}