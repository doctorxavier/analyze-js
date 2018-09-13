using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.RiskMatrix
{
    public class RiskMatrixAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "RiskMatrix";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "RiskMatrix_default",
                "RiskMatrix/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional });
        }
    }
}