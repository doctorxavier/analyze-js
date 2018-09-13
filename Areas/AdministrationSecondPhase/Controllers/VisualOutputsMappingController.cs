using System.Web.Mvc;
using IDB.Architecture.Language;
using IDB.MW.Application.AdministrationModule.Services.VisualOutputsMapping;
using IDB.MW.Application.AdministrationModule.ViewModels.VisualOutputsMapping;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class VisualOutputsMappingController : Controller
    {
        private readonly IVOMDownloadService _visualOutputsMappingDownloadService;
        private readonly IVODocumentService _voDocumentService;

        public VisualOutputsMappingController(
            IVOMDownloadService visualOutputsMappingDownloadService,
            IVODocumentService voDocumentService)
        {
            _visualOutputsMappingDownloadService = visualOutputsMappingDownloadService;
            _voDocumentService = voDocumentService;
        }

        public virtual ActionResult Index()
        {
            var model = new VisualOutputsMappingViewModel();

            model = LoadPermissions(model);

            return View("Index", model);
        }

        public virtual ActionResult ApplyFilterDownloadSection(FilterInformationViewModel filterInformationModel)
        {
            if (string.IsNullOrWhiteSpace(filterInformationModel.OperationName)
                && string.IsNullOrWhiteSpace(filterInformationModel.OperationNumber)
                && !filterInformationModel.MyOperations
                && string.IsNullOrWhiteSpace(filterInformationModel.CountryCode)
                && string.IsNullOrWhiteSpace(filterInformationModel.ApprovalNumber))
            {
                return new JsonResult
                {
                    Data = new
                    {
                        IsValid = false,
                        Message = string.Format("{0}: {1}",
                        Localization.GetText("COMMON.SVQ.ThisValueIsRequired"),
                        Localization.GetText("OPUS.Search.Placeholder.Country"))
                    }
                };
            }

            var response =
                _visualOutputsMappingDownloadService.GetOutputsList(filterInformationModel);

            var data = new JsonResult
            {
                Data = new
                {
                    TabView = this
                        .RenderRazorViewToString(
                            "Partials/VisualOutputsDownloadDataFilter/VisualOutputDownloadDataFilterResultsTable",
                                response.Model),
                    IsValid = response.IsValid,
                    Message = response.ErrorMessage,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                }
            };

            return data;
        }

        public virtual ActionResult VisualOutputsDetail(string operationNumber)
        {
            var response =
                _visualOutputsMappingDownloadService.GetVODetails(operationNumber);

            return View(response.Model);
        }

        public virtual ActionResult GetOperationChangesLog(string operationNumber)
        {
            var response =
                _visualOutputsMappingDownloadService
                    .GetOperationChangesLog(operationNumber);

            var data = new JsonResult
            {
                Data = new
                {
                    LogsView = this
                        .RenderRazorViewToString(
                            "Partials/VisualOutputsLoadData/VisualOutputsLoadValidationLogs",
                                response.Model),
                    IsValid = response.IsValid,
                    Message = response.ErrorMessage,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                }
            };

            return data;
        }

        public virtual ActionResult ImportExcelVOM(string operationsIds)
        {
            var file = Request.Files[0];

            var response =
                _voDocumentService.LoadVisualOuputsDataFile(file);
            var responseData = new
            {
                IsValid = response.IsValid,
                Message = response.ErrorMessage,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                model = response.DictionaryData.ContainsKey("data")
                    ? response.DictionaryData["data"] : null,
                LogsView = response.DictionaryData.ContainsKey("log")
                    ? this.RenderRazorViewToString(
                            "Partials/VisualOutputsLoadData/VisualOutputsLoadDataResultsTable",
                                response.DictionaryData["log"])
                    : null
            };

            var data = new JsonResult
            {
                Data = responseData
            };

            return data;
        }

        private VisualOutputsMappingViewModel LoadPermissions(
            VisualOutputsMappingViewModel vomViewModel)
        {
            vomViewModel.HasLoadDataPermission =
                IDBContext.Current.HasPermission(Permission.VOM_LOAD_DATA);

            vomViewModel.DownloadFilterResultsViewModel.HasDownloadDataInExcelPermission =
                IDBContext.Current.HasPermission(Permission.VOM_DOWNLOAD_DATA_IN_EXCEL);

            vomViewModel.DownloadFilterResultsViewModel.HasDownloadDataInLayersPermission =
                IDBContext.Current.HasPermission(Permission.VOM_DOWNLOAD_DATA_IN_LAYERS);

            return vomViewModel;
        }
    }
}
