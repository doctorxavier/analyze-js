using System.Web.Mvc;
using IDB.MW.Application.VERModule.Messages.Request;
using IDB.MW.Application.VERModule.Services.MyDocuments;
using IDB.Presentation.MVC4.Areas.VER.Models;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.VER.Controllers
{
    public partial class MyDocumentsController : BaseController
    {
        #region Fields
        private readonly MyDocumentsModelMapperHelper _myDocumentsModelMapperHelper;
        private readonly IMyDocumentsService _myDocumentsService;
        #endregion

        #region Constructors
        public MyDocumentsController(
            IMyDocumentsService myDocumentsService)
        {
            _myDocumentsService = myDocumentsService;
            _myDocumentsModelMapperHelper = new MyDocumentsModelMapperHelper(
                ViewBag, _myDocumentsService);
        }
        #endregion

        public virtual ActionResult Index(string operationNumber)
        {
            var model = _myDocumentsModelMapperHelper.GetMyDocuments(operationNumber);

            return View(model);
        }

        public virtual ActionResult VirtualEditingRoomDocumentSearch(
            string operationNumber,
            string searchText,
            int country,
            int filterType)
        {
            var isOperation = string.IsNullOrEmpty(operationNumber) == false;
            var filter = new FilterVirtualEditingRoomRequest
            {
                SearchText = isOperation ? operationNumber : searchText,
                Country = country,
                TypeInstance = filterType,
                IsOperation = isOperation
            };
            var model = _myDocumentsService.FilterInstances(filter);

            return PartialView("Partials/DataTables/VirtualEditingRoomTable", model.VerTable);
        }
    }
}