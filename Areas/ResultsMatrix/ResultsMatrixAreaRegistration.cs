using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.ResultsMatrix
{
    public class ResultsMatrixAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ResultsMatrix";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ResultsMatrix_operation",
                "{operation}/ResultsMatrix/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "New_ResultsMatrix_operation",
                "{operationNumber}/ResultsMatrix/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });

            context.MapRoute(
                "ResultsMatrix_default",
                "ResultsMatrix/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}