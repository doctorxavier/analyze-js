using System.Web.Mvc;

using IDB.Architecture.Logging;
using IDB.MW.Application.GlobalModule.Services.OperationService;
using IDB.MW.Domain.Models.Global;
using IDB.MW.Domain.Proxies.DomainServices;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.Global.Controllers
{
    public partial class OperationHeaderController : BaseController
    {
        private readonly IOperationService _operationService;

        public OperationHeaderController(IOperationService operationService)
        {
            _operationService = operationService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public virtual JsonResult GetData(string operationNumber, string ticket)
        {
            string userName = null;
            if (ticket != null)
            {
                string[] arrTokenContent;
                string error = SecurityUtils.TryDecodeToken(
                    ticket, HttpContext.Session, out arrTokenContent);

                if (!string.IsNullOrEmpty(error))
                {
                    Logger.GetLogger().WriteMessage("OperationHeaderController", error);
                    return new JsonResult() { Data = error };
                }

                userName = SecurityUtils.GetDecodedUser(arrTokenContent);
            }

            var globalModelRepClient = new GlobalModelRepository();
            OperationHeaderInfo operationHeader;

            if (userName == null)
            {
                operationHeader = globalModelRepClient.getOperationHeaderInformation(
                    operationNumber,
                    IDBContext.Current.CurrentLanguage,
                    IDBContext.Current.UserName);
            }

            operationHeader = globalModelRepClient.getOperationHeaderInformation(
                operationNumber,
                IDBContext.Current.CurrentLanguage,
                userName);

            return Json(operationHeader, JsonRequestBehavior.AllowGet);
        }
    }
}