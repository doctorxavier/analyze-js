using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.SGP
{
    public class SGPAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "SGP";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("SGP_default", "{operationNumber}/SGP/{controller}/{action}");
            context.MapRoute("wo_operation_SGP", "SGP/{controller}/{action}");
        }
    }
}