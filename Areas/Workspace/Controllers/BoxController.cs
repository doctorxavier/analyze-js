using System.Web.Mvc;

using IDB.MW.Domain.Models.Pagination;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.Global.Controllers;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.Workspace.Controllers
{
    public partial class BoxController : BaseController
    {
        private readonly IGlobalModelRepository _globalModelRepository;
        private readonly DataSourceRequest _request = new DataSourceRequest { Take = 5 };

        public BoxController(IGlobalModelRepository globalModelRepository)
        {
            _globalModelRepository = globalModelRepository;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult OperationList(string name)
        {
            ViewBag.Title = name;
            var gridData = _globalModelRepository.GetAllMyOperations(IDBContext.Current.UserName, string.Empty, _request);
            return View("OperationList", gridData);
        }

        public virtual ActionResult TaskList(string name)
        {
            ViewBag.Title = name;
            var result = _globalModelRepository.GetTaskByUser(
                IDBContext.Current.UserName,
                _request,
                GlobalCommonLogic.GetOperationDetailURL(),
                GlobalCommonLogic.GetOperationDraftDetailURL(),
                null,
                true);

            return View("TasksList", result);
        }

        public virtual ActionResult NotificationList(string name)
        {
            ViewBag.Title = name;
            var viewM = _globalModelRepository.GetNotificationsByUser(
                IDBContext.Current.UserName,
                null,
                _request,
                GlobalCommonLogic.GetOperationDetailURL(),
                GlobalCommonLogic.GetOperationDraftDetailURL(),
                true);

            var lang = IDBContext.Current.CurrentLanguage;
            foreach (var item in viewM.Data)
            {
                item.Subject = GlobalCommonLogic.BuildNotification(item.Body, "SUBJECT", lang);
                item.Body = GlobalCommonLogic.BuildNotification(item.Body, "BODY", lang);
            }

            return View("NotificationsList", viewM);
        }

        public virtual ActionResult Frame(string name, string id)
        {
            ViewBag.Title = name;
            ViewBag.Id = id;
            return View("Frame");
        }

        public virtual ActionResult VerticalGraph(string name, string id)
        {
            ViewBag.Title = name;
            ViewBag.Id = id;
            return View("VerticalGraph");
        }

        public virtual ActionResult DashGraph(string name, string id)
        {
            ViewBag.Title = name;
            ViewBag.Id = id;
            return View("DashGraph");
        }

        public virtual ActionResult StackGraph(string name, string id)
        {
            ViewBag.Title = name;
            ViewBag.Id = id;
            return View("StackGraph");
        }

        public virtual ActionResult ExpiredClauses(string name)
        {
            ViewBag.Title = name;
            var model = _globalModelRepository.GetExpiredClauses(IDBContext.Current.UserName);
            return View("ExpiredClauses", model);
        }
    }
}