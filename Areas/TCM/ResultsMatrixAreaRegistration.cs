using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.TCM
{
    public class ResultsMatrixAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "TCM";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ResultsMatrixNew_operation",
                "{operationNumber}/TCM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "ResultsMatrixOld_operation",
                "{operation}/TCM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "ResultsMatrixNew_default",
                "TCM/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}
