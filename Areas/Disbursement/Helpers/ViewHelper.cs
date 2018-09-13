using System.Web.Mvc;

namespace IDB.Presentation.MVC4.Areas.Disbursement.Helpers
{
    public static class ViewHelper
    {
        internal static string ViewStringify(
            string partialView,
            object model,
            ControllerBase controller)
        {
            var resultStringView = string.Empty;
            controller.ViewData.Model = model;
            using (System.IO.StringWriter writer = new System.IO.StringWriter())
            {
                ViewEngineResult resultViewEngine = ViewEngines.Engines
                    .FindPartialView(controller.ControllerContext, partialView);
                ViewContext viewContext = new ViewContext(
                    controller.ControllerContext,
                    resultViewEngine.View,
                    controller.ViewData,
                    new TempDataDictionary(),
                    writer);
                resultViewEngine.View.Render(viewContext, writer);
                resultStringView = writer.ToString();
            }

            return resultStringView;
        }
    }
}