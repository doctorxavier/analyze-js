using System.Web.Mvc;

using IDB.MW.Application.DEMModule.Services;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.DEM.Controllers
{
    public partial class ImporterController : BaseController
    {
        private readonly IDEMExcelImporterService _demExcelImporterService;

        public ImporterController(IDEMExcelImporterService demExcelImporterService)
        {
            _demExcelImporterService = demExcelImporterService;
        }

        public virtual JsonResult ImportExcel()
        {
            var file = Request.Files[0];
            bool finalResult = true;
            string resultMessage = null;
            string operationNumber = IDBContext.Current.Operation;

            var isValidDemChecklistResponse = _demExcelImporterService.IsValidDemChecklist(operationNumber, file);

            if (isValidDemChecklistResponse.IsValid)
            {
                var resultImport = _demExcelImporterService.ImportFromExcel(operationNumber, file);
                if (!resultImport.IsValid)
                {
                    finalResult = false;
                    resultMessage = resultImport.ErrorMessage;
                }
            }
            else
            {
                finalResult = isValidDemChecklistResponse.IsValid;
                resultMessage = isValidDemChecklistResponse.ErrorMessage;
            }

            return Json(new { success = finalResult, message = resultMessage });
        }
    }
}