using System.Web.Mvc;
using IDB.Architecture.Language;
using IDB.MW.Application.AdministrationModule.Services.VisualOutputsMapping;
using IDB.MW.Application.AdministrationModule.ViewModels.VisualOutputsMapping;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Helpers;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class VisualOutputsMappingDocumentController : BaseController
    {
        private readonly IVODocumentService _voDocumentService;

        public VisualOutputsMappingDocumentController(
            IVODocumentService voDocumentService)
        {
            _voDocumentService = voDocumentService;
        }

        public virtual ActionResult ExportExcelVOM(string operationsIds)
        {
            var response =
                _voDocumentService.GetVOMDocument(operationsIds);

            return base.File(response.Document,
                "application/vnd.ms-excel.sheet.macroEnabled.12",
                VisualOutputsMappingHelper.GetDocName());
        }

        public virtual ActionResult ExportLogOfChanges(
            string operationNumbers,
            string fromDownloadDate,
            string toDownloadDate)
        {
            var logDownloadDataViewModel = new LogDownloadDataViewModel()
            {
                OperationNumbers = operationNumbers,
                FromDate = fromDownloadDate,
                ToDate = toDownloadDate
            };

            var response =
                _voDocumentService.GetVOMLogOfChanges(logDownloadDataViewModel);

            return base.File(response.Document,
                "application/vnd.ms-excel.sheet.macroEnabled.12",
                VisualOutputsMappingHelper.GetLogOfChangesFileName());
        }

        public virtual ActionResult SaveExcelVOM(VOExcelViewModel vOExcelViewModel, string operationsIds)
        {
            var response =
                _voDocumentService.SaveVisualOuputsDataFile(vOExcelViewModel, operationsIds);
            var data = new JsonResult
            {
                Data = new
                {
                    IsValid = response.IsValid,
                    Message = response.IsValid ? Localization.GetText("VOM.SaveChanges") : "Error: " + response.ErrorMessage,
                }
            };

            return data;
        }

        public virtual ActionResult ExportLogOfChangesPreSaved(
            string vOExcelViewModel, string operationsIds)
        {
            var vm = Newtonsoft.Json.JsonConvert.DeserializeObject<VOExcelViewModel>(vOExcelViewModel);
            var response =
                _voDocumentService.GetVOMLogOfChangesPreSaved(vm, operationsIds);

            return base.File(response.Document,
                "application/vnd.ms-excel.sheet.macroEnabled.12",
                VisualOutputsMappingHelper.GetLogOfChangesFileName());
        }

        public virtual ActionResult ExportMapLayers(string operationsIds, string layer, string filterVO)
        {
            var response =
                _voDocumentService.GetVOMapLayers(operationsIds, layer, filterVO);

            return base.File(response.Document,
                "text/plain",
                VisualOutputsMappingHelper.GetMapLayersFileName(layer));
        }
    }
}