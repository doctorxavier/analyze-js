using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

using IDB.MW.Application.TCM.Services.ValidationProcessService;
using IDB.MW.Domain.Models.Architecture.TCM.TCMValidationWorkFlow;
using IDB.MW.Domain.Session;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Business.TCAbstractModule.Contracts;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class ValidationProcessController : MVC4.Controllers.ConfluenceController
    {
        #region Constants
        public static string PAGE_CHART = "UI-VP-001-ValidationProcess,UI-COM-004Partial-TabMappingProgress";
        #endregion
        private readonly IValidationService _validationService;
        private readonly ITcmUniverseService _tcmService;
        private readonly IComponentCNDService _componentCNDService;

        #region Constructors
        public ValidationProcessController(IValidationService validationService,
            ITcmUniverseService tcmService,
            IComponentCNDService componentCNDService)      
        {
            _validationService = validationService;
            _tcmService = tcmService;
            _componentCNDService = componentCNDService;
        }
        #endregion

        public virtual ActionResult Index(string operationNumber)
        {
            Logger.GetLogger().WriteDebug("ValidationProcessController", "Method: Index - Parameters( operationNumber: " + operationNumber + ")");
            var model = new ValidationProcessModel()
            {
                ValidationProcessStatus = new List<RowValidationProcessModel>()
            };

            var cycle = _validationService.GetTCMCycleActive(operationNumber);
            model.CodeCycle = cycle.CodeCycle;
            model.ResultsMatrixId = cycle.ResultsMatrixId;
            model.IsFinalReport = cycle.IsFinalReport;

            if (cycle.ResultsMatrixId != 0)
            {
                var listProcessResponse = _validationService.GetProcessStatusByResultsMatrix(cycle.ResultsMatrixId);
                model.ValidationProcessStatus = listProcessResponse.ListProcessStatus;

                var validation = _validationService.GetVisibilitySubmit(listProcessResponse.ListProcessStatus);
                model.IsSubmit = validation.IsSubmit;
                model.IsPmrRequired = validation.IsPmrRequired;
                model.IsOperationDaughter = validation.IsOperationDaughter;
            }

            model.IsEditable = IsEditableValidationProcess(model);
            Logger.GetLogger().WriteDebug("ValidationProcessController", "Method: Index - IsEditableValidationProcess: " + model.IsEditable);

            Logger.GetLogger().WriteDebug("ValidationProcessController", "Method: Index - IsEditableValidationProcess: " + model.IsEditable);

            model.TextCodeCycle = Localization.GetText("TCM.VP.ValidationProcess.WarningMessageTop");
            Logger.GetLogger().WriteDebug("ValidationProcessController", "Method: Index - Final");

            var isCndOperation = _componentCNDService.IsCNDOperation(operationNumber);

            if (!isCndOperation.IsValid)
                Logger.GetLogger().WriteDebug(
                    "ComponentCNDService",
                    "Method: IsCNDOperation - Error when checking CND operation logic; assuming false");

            model.IsCndOperation = 
                isCndOperation.IsValid ? isCndOperation.HasCondition : false;

            return View(model);
        }

        public virtual ActionResult RefreshTableProcess(string operationNumber)
        {
            var model = new ValidationProcessModel()
            {
                ValidationProcessStatus = new List<RowValidationProcessModel>()
            };

            var cycle = _validationService.GetTCMCycleActive(operationNumber);
            if (cycle.ResultsMatrixId != 0)
            {
                var listProcessResponse = _validationService.GetProcessStatusByResultsMatrix(cycle.ResultsMatrixId);
                model.ValidationProcessStatus = listProcessResponse.ListProcessStatus;
            }

            return PartialView("Partials/IndexDataTableContent", model.ValidationProcessStatus);
        }

        public virtual FileResult DownloadExportsValidation(string operationNumber, string formatType)
        {
            var response = _validationService.DownloadExportProcess(operationNumber, formatType);
            if (!response.IsValid)
            {
                return null;
            }

            var application = "application/";
            application = (formatType == "pdf") ? (application + formatType) : (application + "vnd.ms-excel");

            return File(response.File, application);
        }

        internal virtual bool IsEditableValidationProcess(ValidationProcessModel model)
        {
            bool tlSubmit = IDBContext.Current.HasPermission(Permission.TL_SUBMIT);

            bool tcResultMatrixAdmin = IDBContext.Current.HasPermission(
                Permission.TC_RESULTS_MATRIX_ADMIN_PERMISSION);

            bool fcCoordinator = false;

            var roles = model.ValidationProcessStatus
                .Select(vps => vps.ValidationRole);

            foreach (var role in roles)
            {
                if (IDBContext.Current
                    .HasFundPermission(role, Permission.FC_EDIT_VALIDATION_PROCESS))
                {
                    fcCoordinator = true;
                    break;
                }
            }

            bool tcmAdminEdit = IDBContext.Current.HasPermission(
                Permission.TCM_ADMIN_EDIT_VALIDATION_PROCESS);

            bool notIsFinalReport = model.IsFinalReport.HasValue ?
                !model.IsFinalReport.Value : true;

            if (model.IsSubmit && (tlSubmit || tcResultMatrixAdmin))
            {
                return !model.IsPmrRequired && !model.IsOperationDaughter;
            }

            if (!model.IsSubmit && tcmAdminEdit)
            {
                return true;
            }

            if (!model.IsSubmit && fcCoordinator)
            {
                return notIsFinalReport;
            }

            return false;
        }
    }
}