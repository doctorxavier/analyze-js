using System.Collections.Generic;
using System.Web.Mvc;
using IDB.Architecture;
using IDB.MW.Domain.Models.Global;
using IDB.MW.Domain.Models.Pagination;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Application.GlobalModule.Services.OperationService;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;

namespace IDB.Presentation.MVC4.Areas.Global.Controllers
{
    public partial class OperationsController : BaseController
    {
        private readonly IOperationService _operationService;

        public OperationsController(IOperationService operationService)
        {
            _operationService = operationService;
        }

        [Authorize]
        public virtual ActionResult Index()
        {
            var authorizationManagerClient = Globals.Resolve<IAuthorizationManager>();
            var roles = authorizationManagerClient.GetRoles(IDBContext.Current.UserLoginName, string.Empty);
            var userIsGlobal = _operationService.IsRoleTypeGlobal(roles);

            var commonLogin = new GlobalCommonLogic();
            var model = new OperationsViewModel();

            ViewBag.TotalOperations = 0;
            ViewBag.OpDetailURL = Url.Action("Operation", "Mainframe"); //GlobalCommonLogic.GetOperationDetailURL();
            model.LastViewed = commonLogin.GetOperationsLastViewed();

            if (userIsGlobal)
            {
                model.IsGlobal = true;
                return View(model);
            }

            model.IsGlobal = false;
            return View(model);
        }

        public virtual ActionResult IndexDataGrid(DataSourceRequest request)
        {
            var authorizationManagerClient = Globals.Resolve<IAuthorizationManager>();
            var roles = authorizationManagerClient.GetRoles(IDBContext.Current.UserLoginName, string.Empty);
            var userIsGlobal = _operationService.IsRoleTypeGlobal(roles);

            if (userIsGlobal)
            {
                return View(new OperationViewModel());
            }

            var commonLogin = new GlobalCommonLogic();
            var viewModel = commonLogin.GetMyOperations(request);
            ViewBag.TotalOperations = 0;
            ViewBag.OpDetailURL = Url.Action("Operation", "Mainframe"); //GlobalCommonLogic.GetOperationDetailURL();
            return View(viewModel);
        }

        [HttpPost]
        public virtual JsonResult GetData(DataSourceRequest request)
        {
            var authorizationManagerClient = Globals.Resolve<IAuthorizationManager>();
            var roles = authorizationManagerClient.GetRoles(IDBContext.Current.UserLoginName, string.Empty);
            var userIsGlobal = _operationService.IsRoleTypeGlobal(roles);            

            var operationResponse = _operationService.GetMyOperations(
                new MW.Application.GlobalModule.Messages.MyOperationsRequest()
            {
                OperationNumber = string.Empty,
                UserName = IDBContext.Current.UserName,
                RequestSetting = request
            });

            if (!operationResponse.IsValid)
            {
                return Json(new OperationViewModel());
            }

            ViewBag.TotalOperations = operationResponse.OperationData.Total;
            return Json(operationResponse.OperationData);
        }
    }
}