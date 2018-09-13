using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.EvaluationTracking
{
    public class EvaluationTrackingAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "EvaluationTracking";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "EvaluationTracking_operation",
                "{operation}/EvaluationTracking/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
            context.MapRoute(
                "EvaluationTracking_operationNumber",
                "{operationNumber}/EvaluationTracking/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
            context.MapRoute(
               "EvaluationTracking_operationNumber_route",
               "{mainOperationNumber}/EvaluationTracking/{controller}/{action}/{id}",
               new { action = "Index", id = UrlParameter.Optional });
            context.MapRoute(
                "EvaluationTracking_default",
                "EvaluationTracking/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}
