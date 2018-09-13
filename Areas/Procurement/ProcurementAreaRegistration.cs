using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.BEOProcurement
{
    public class ProcurementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Procurement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute("Procurement_default", "{operationNumber}/Procurement/{controller}/{action}");
            context.MapRoute("wo_operation_procurement", "Procurement/{controller}/{action}");
        }
    }
}