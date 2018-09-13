using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Disbursement
{
    public class DisbursementAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "Disbursement";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "Disbursement_operation",
                "{operation}/Disbursement/{controller}/{action}/{id}",
                new
                {
                    action = "Index", id = UrlParameter.Optional,
                    controller = "DisbursementProjections"
                });
            context.MapRoute(
                "Disbursement_default",
                "Disbursement/{controller}/{action}/{id}",
                new
                {
                    action = "Index",
                    id = UrlParameter.Optional,
                    controller = "DisbursementProjections"
                });
        }
    }
}