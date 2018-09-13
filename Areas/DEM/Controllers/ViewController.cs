using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.CaseManager.Business;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.DEMModule.Enums;
using IDB.MW.Application.DEMModule.Services;
using IDB.MW.Application.DEMModule.ViewModels;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Application.OPUSModule.ViewModels.OperationDataService;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Values.Dem;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.OPUS.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.Architecture.Logging;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.DEM.Controllers
{
    public partial class ViewController : ConfluenceController
    {
        private readonly IDEMService _demService;
        private readonly IOperationDataService _operationDataService;
        private readonly IDEMExcelExportService _demExcelExportService;
        private readonly ICatalogService _catalogService;

        public ViewController(
            IDEMService demService, 
            IOperationDataService operationDataService, 
            IDEMExcelExportService demExcelExportService, 
            ICatalogService catalogService)
        {
            _demService = demService;
            _operationDataService = operationDataService;
            _demExcelExportService = demExcelExportService;
            _catalogService = catalogService;
        }

        public virtual ActionResult Dem(string operationNumber)
        {
            var haveAssociatedCaseManagerTemplate =
                _demService.GetActivityPlanTemplate(operationNumber);

            var response = new DemViewModel();
            var status = new DEMStatusViewModel();
            if (haveAssociatedCaseManagerTemplate)
            {
                var newResponse = _demService.GetDemForOperation(operationNumber);
                if (newResponse.IsValid)
                {
                    bool isFinalVersionCompletedDem = newResponse.DemViewModel.IsWriteDemAferFinalVersion;

                    response.Justification = newResponse.DemViewModel.Justification;
                    response.Required = newResponse.DemViewModel.Required;
                    response.LastUpdate = newResponse.DemViewModel.LastUpdate;
                    response.DemChecklistStatus = "-";
                    response.Stage = newResponse.DemViewModel.Stage;
                    response.CurrentUser = newResponse.DemViewModel.CurrentUser;
                    response.FullName = _demService.GetFullName(IDBContext.Current.UserName);
                    response.Summary.Language = Language.EN;
                    response.Resumen.Language = Language.ES;
                    response.Summary.OperationNumber = operationNumber;
                    response.Resumen.OperationNumber = operationNumber;
                    response.Summary.Role = newResponse.DemViewModel.CurrentRole;
                    response.Resumen.Role = newResponse.DemViewModel.CurrentRole;
                    response.CurrentRole = newResponse.DemViewModel.CurrentRole;
                    response.Summary.ShowDEMRequired = false;
                    response.Summary.EvaluabilityAssessmentNote =
                        newResponse.DemViewModel.Summary.EvaluabilityAssessmentNote;
                    response.Resumen.EvaluabilityAssessmentNote =
                        newResponse.DemViewModel.Resumen.EvaluabilityAssessmentNote;
                    response.IsEditSPDCompletedTL = newResponse.DemViewModel.IsEditSPDCompletedTL;

                    if (newResponse.DemViewModel.Required)
                    {
                        bool isDraft = newResponse.DemViewModel.DemChecklistStatus.ToUpper() ==
                            DemGlobalValues.DRAFT || newResponse.DemViewModel.IsFakeCompleted;

                        response = PopulateDemViewModel(newResponse.DemViewModel.DemOperationId,
                            newResponse.DemViewModel.Stage,
                            newResponse.DemViewModel.CurrentRole,
                            newResponse.DemViewModel.DemChecklistStatus,
                            operationNumber,
                            isDraft);

                        response.DemChecklistStatus = newResponse.DemViewModel.DemChecklistStatus;
                        response.OfflineIsEnabled = newResponse.DemViewModel.OfflineIsEnabled;
                        response.CurrentUser = newResponse.DemViewModel.CurrentUser;
                        response.FullName = _demService.GetFullName(IDBContext.Current.UserName);
                        response.DemOperationId = newResponse.DemViewModel.DemOperationId;
                        response.IsBlocked = newResponse.DemViewModel.IsBlocked;
                        newResponse.DemViewModel.Summary.ShowDEMRequired =
                            response.Summary.ShowDEMRequired;
                        response.Summary.EvaluabilityAssessmentNote =
                            newResponse.DemViewModel.Summary.EvaluabilityAssessmentNote;
                        response.Resumen.EvaluabilityAssessmentNote =
                            newResponse.DemViewModel.Resumen.EvaluabilityAssessmentNote;
                        response.Summary.Role = newResponse.DemViewModel.CurrentRole;
                        response.Resumen.Role = newResponse.DemViewModel.CurrentRole;
                        response.CurrentRole = newResponse.DemViewModel.CurrentRole;
                        response.Summary.Language = Language.EN;
                        response.Resumen.Language = Language.ES;
                        response.Summary.OperationNumber = operationNumber;
                        response.Resumen.OperationNumber = operationNumber;
                        response.Justification = newResponse.DemViewModel.Justification;
                        response.Required = newResponse.DemViewModel.Required;
                        response.LastUpdate = newResponse.DemViewModel.LastUpdate;
                        response.Stage = newResponse.DemViewModel.Stage;
                        response.IsEditableInWF = newResponse.DemViewModel.IsEditableInWF;
                        var dataDemOperation = _demService.GetDataDemOperation(response.DemOperationId);

                        var demChecklistStatus = newResponse.DemViewModel.IsFakeCompleted
                            ? DemGlobalValues.DRAFT
                            : newResponse.DemViewModel.DemChecklistStatus.ToUpper();

                        var isCompletedAppr = _demService.IsCompletedAppr(operationNumber);
                        response.IsDemAfterApproved = isCompletedAppr;

                        response.StrategicAlignment = StrategicAlignmentDataContent(
                            response.DemOperationId,
                            demChecklistStatus,
                            operationNumber);
                        response.StrategicAlignment.IsFinalVersionCompletedDem = isFinalVersionCompletedDem;
                        response.StrategicAlignment.InformationDem.DemOperationId = response.DemOperationId;
                        response.StrategicAlignment.InformationDem.UserName = IDBContext.Current.UserName;
                        response.StrategicAlignment.InformationDem.FullName =
                            _demService.GetFullName(IDBContext.Current.UserName);
                        response.StrategicAlignment.InformationDem.Role =
                            newResponse.DemViewModel.CurrentRole;
                        response.StrategicAlignment.InformationDem.Date =
                            string.Format("{0:dd MMM yyyy}", DateTime.Now);
                        response.StrategicAlignment.InformationDem.Stage = newResponse.DemViewModel.Stage;
                        response.StrategicAlignment.InformationDem.StageId =
                            dataDemOperation.DataDemOperation.DemStageId;
                        response.StrategicAlignment.InformationDem.CheckListVersion =
                            newResponse.DemViewModel.DemChecklistStatus;
                        response.StrategicAlignment.InformationDem.CheckListVersionId =
                            dataDemOperation.DataDemOperation.DemChecklistStatusId;
                        response.StrategicAlignment.IsCompletedVersion =
                            newResponse.DemViewModel.DemChecklistStatus.ToUpper() !=
                            DemGlobalValues.DRAFT && newResponse.DemViewModel.IsBlocked;
                        response.IsEditSPDCompletedTL = newResponse.DemViewModel.IsEditSPDCompletedTL;

                        response.IsWriteDemAferFinalVersion = isFinalVersionCompletedDem;

                        response.StrategicAlignment.CountryStrategyData.IsFinalVersionCompletedDem = 
                            isFinalVersionCompletedDem;
                        response.StrategicAlignment.CountryStrategyData.InformationDem.DemOperationId =
                            response.DemOperationId;
                        response.StrategicAlignment.CountryStrategyData.InformationDem.UserName =
                            IDBContext.Current.UserName;
                        response.StrategicAlignment.CountryStrategyData.InformationDem.FullName =
                            _demService.GetFullName(IDBContext.Current.UserName);
                        response.StrategicAlignment.CountryStrategyData.InformationDem.Role =
                            newResponse.DemViewModel.CurrentRole;
                        response.StrategicAlignment.CountryStrategyData.InformationDem.Date =
                            string.Format("{0:dd MMM yyyy}", DateTime.Now);
                        response.StrategicAlignment.CountryStrategyData.InformationDem.Stage =
                            newResponse.DemViewModel.Stage;
                        response.StrategicAlignment.CountryStrategyData.InformationDem.StageId =
                            dataDemOperation.DataDemOperation.DemStageId;
                        response.StrategicAlignment.CountryStrategyData.InformationDem.CheckListVersion =
                            newResponse.DemViewModel.DemChecklistStatus;
                        response.StrategicAlignment.CountryStrategyData.InformationDem.CheckListVersionId =
                            dataDemOperation.DataDemOperation.DemChecklistStatusId;
                        response.StrategicAlignment.SerializedViewModelSA =
                            PageSerializationHelper.SerializeObject(response.StrategicAlignment);
                        var cmbBusiness = CMBusiness.Get(operationNumber);
                        var activityItemAppr = cmbBusiness.GetActivityItem(CMConstants.DefaultActivityItems.APPR);
                        response.StrategicAlignment.StrategicAlignmentWrite =
                            IDBContext.Current.HasPermission(Permission.STRATEGIC_ALIGNMENT_WRITE) && !activityItemAppr.IsCompleted();

                        response.StrategicAlignment.CountryProgramData.IsFinalVersionCompletedDem = 
                            isFinalVersionCompletedDem;
                        response.StrategicAlignment.CountryProgramData.InformationDem.DemOperationId =
                            response.DemOperationId;
                        response.StrategicAlignment.CountryProgramData.InformationDem.UserName =
                            IDBContext.Current.UserName;
                        response.StrategicAlignment.CountryProgramData.InformationDem.FullName =
                            _demService.GetFullName(IDBContext.Current.UserName);
                        response.StrategicAlignment.CountryProgramData.InformationDem.Role =
                            newResponse.DemViewModel.CurrentRole;
                        response.StrategicAlignment.CountryProgramData.InformationDem.Date =
                            string.Format("{0:dd MMM yyyy}", DateTime.Now);
                        response.StrategicAlignment.CountryProgramData.InformationDem.Stage =
                            newResponse.DemViewModel.Stage;
                        response.StrategicAlignment.CountryProgramData.InformationDem.StageId =
                            dataDemOperation.DataDemOperation.DemStageId;
                        response.StrategicAlignment.CountryProgramData.InformationDem.CheckListVersion =
                            newResponse.DemViewModel.DemChecklistStatus;
                        response.StrategicAlignment.CountryProgramData.InformationDem.CheckListVersionId =
                            dataDemOperation.DataDemOperation.DemChecklistStatusId;

                        response.AlignmentContribution = CountryDevelopmentResultsDataContent(
                            response.DemOperationId,
                            newResponse.DemViewModel.DemChecklistStatus,
                            operationNumber);
                        response.AlignmentContribution.IsFinalVersionCompletedDem = isFinalVersionCompletedDem;
                        response.AlignmentContribution.InformationDem.DemOperationId =
                            response.DemOperationId;
                        response.AlignmentContribution.InformationDem.UserName =
                            IDBContext.Current.UserName;
                        response.AlignmentContribution.InformationDem.FullName =
                            _demService.GetFullName(IDBContext.Current.UserName);
                        response.AlignmentContribution.InformationDem.Role =
                            newResponse.DemViewModel.CurrentRole;
                        response.AlignmentContribution.InformationDem.Date =
                            string.Format("{0:dd MMM yyyy}", DateTime.Now);
                        response.AlignmentContribution.InformationDem.Stage =
                            newResponse.DemViewModel.Stage;
                        response.AlignmentContribution.InformationDem.StageId =
                            dataDemOperation.DataDemOperation.DemStageId;
                        response.AlignmentContribution.InformationDem.CheckListVersion =
                           newResponse.DemViewModel.DemChecklistStatus;
                        response.AlignmentContribution.InformationDem.CheckListVersionId =
                            dataDemOperation.DataDemOperation.DemChecklistStatusId;

                        response.AlignmentContribution.SerializedViewModelCountry =
                            PageSerializationHelper.SerializeObject(response.AlignmentContribution);
                        response.Risk.DemOperationId = newResponse.DemViewModel.DemOperationId;
                        response.Additionality.DemOperationId = newResponse.DemViewModel.DemOperationId;

                        var riskRateList = _demService.GetRiskRateList().RiskRateList;

                        if (!string.IsNullOrEmpty(response.Risk.RiskRateValue))
                        {
                            foreach (var item in riskRateList)
                            {
                                if (item.Value == response.Risk.RiskRateValue)
                                {
                                    item.Selected = true;
                                    response.Risk.RiskRateDescription = item.Text;
                                }
                            }
                        }

                        response.Risk.RiskRateList = riskRateList;
                        response.OperationNumber = operationNumber;
                        var resultsMatrix = _operationDataService.GetResultMatrixByOperation(operationNumber);
                        response.StrategicAlignment.ResultMatrixId = resultsMatrix.ResultsMatrixId;

                        response.IsEditable = isFinalVersionCompletedDem ?
                            isFinalVersionCompletedDem : !newResponse.DemViewModel.IsBlocked;

                        var cmb = CMBusiness.Get(operationNumber);
                        var activityItemErm = cmb.GetActivityItem(CMConstants.DefaultActivityItems.ERM);

                        bool isMilestoneErmInProgress = false;
                        
                        if (activityItemErm != null)
                        {
                            isMilestoneErmInProgress = activityItemErm.StatusId == CMConstants.ActivityItemStatus.InProgress.MasterId;
                        }

                        var activityItemQrr = cmb.GetActivityItem(CMConstants.DefaultActivityItems.QRR);

                        bool isMilestoneQrrInProgress = false;
                            
                        if (activityItemQrr != null)
                        {
                            isMilestoneQrrInProgress = activityItemQrr.StatusId == CMConstants.ActivityItemStatus.InProgress.MasterId;
                        }

                        response.SubmitEdit =
                            ((newResponse.DemViewModel.Stage == DemGlobalValues.STAGE_ERM && isMilestoneErmInProgress) ||
                            (newResponse.DemViewModel.Stage == DemGlobalValues.STAGE_QRR && isMilestoneQrrInProgress)) &&
                            IDBContext.Current.HasPermission(Permission.DEM_REVIEWER_WRITE) && isDraft && response.IsEditable;
                        ViewBag.SerializedViewModel =
                            PageSerializationHelper.SerializeObject(newResponse.DemViewModel);

                        response.SubmitForDemRevalidation = _demService.IsSubmitForDemRevalidation(operationNumber);

                        ViewBag.CurrentStage = newResponse.DemViewModel.Stage;

                        status.CurrentVersion = newResponse
                            .DemViewModel.DemChecklistStatus.ToUpper();

                        response.Additionality.CurrentVersion = status.CurrentVersion;

                        response.Evaluability.CurrentVersion = status.CurrentVersion;

                        response.Risk.CurrentVersion = status.CurrentVersion;

                        if (isCompletedAppr && !(IDBContext.Current.HasPermission(Permission.DEM_READ) &&
                                IDBContext.Current.HasPermission(Permission.DEM_COMMENTS_READ)))
                        {
                            response.Additionality.IsUserConvergenceReadAfterApproved = true;
                            response.Evaluability.IsUserConvergenceReadAfterApproved = true;
                            response.Risk.IsUserConvergenceReadAfterApproved = true;
                        }

                        response.Additionality.IsFinalVersionCompletedDem = isFinalVersionCompletedDem;
                        response.Evaluability.IsFinalVersionCompletedDem = isFinalVersionCompletedDem;
                        response.Risk.IsFinalVersionCompletedDem = isFinalVersionCompletedDem;

                        response.StrategicAlignment.CountryProgramData.CurrentVersion = status.CurrentVersion;

                        response.StrategicAlignment.CountryStrategyData.CurrentVersion = status.CurrentVersion;

                        response.StrategicAlignment.CurrentVersion = status.CurrentVersion;

                        response.AlignmentContribution.CurrentVersion = status.CurrentVersion;

                        bool hasAccessPermissions = false;

                        var overallStage = _demService.GetOperationStageCode(operationNumber);

                        try
                        {
                            if (overallStage != DemGlobalValues.APPR)
                            {
                                hasAccessPermissions =
                                    IDBContext.Current.HasPermission(Permission.DEM_READ) &&
                                    IDBContext.Current.HasPermission(Permission.DEM_COMMENTS_READ);
                            }
                            else
                            {
                                hasAccessPermissions =
                                    IDBContext.Current.HasPermission(Permission.CONVERGENCE_READ_PERMISSION);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.GetLogger().WriteError(
                               "DEM ViewController",
                               string.Format(
                                   "DEM: Operation: {0}. User: {1}. Has Permission: {2}. {3}",
                                   operationNumber,
                                   IDBContext.Current.UserName,
                                   hasAccessPermissions.ToString(),
                                   e.Message),
                                   e);
                        }

                        response.EnabledForThisUser = hasAccessPermissions;

                        if (hasAccessPermissions || (!hasAccessPermissions && response.IsDemAfterApproved))
                        {
                            var checklistStatus = newResponse.DemViewModel.DemChecklistStatusCode;

                            if (checklistStatus == DemGlobalValues.UNDEFINED)
                            {
                                response.ErrorMessageDEM =
                                    Localization.GetText("R6.DEM.Main.MessageVersionUndefined");
                                response.EnabledForThisUser = false;
                            }

                            if (checklistStatus == DemGlobalValues.LOOP)
                            {
                                response.DemChecklistStatus = _demService.GetDemChecklistStatusDescription(
                                    DemGlobalValues.TL_SELF_ASSESSMENT);
                            }

                            if (checklistStatus == DemGlobalValues.RETURN ||
                                checklistStatus == DemGlobalValues.SUBMIT ||
                                checklistStatus == DemGlobalValues.SPD_PRE_VALIDATED ||
                                checklistStatus == DemGlobalValues.RETURNED_SPD_REVIEW ||
                                checklistStatus == DemGlobalValues.SPD_REJECTED ||
                                checklistStatus == DemGlobalValues.LAST_VERSION)
                            {
                                response.DemChecklistStatus =
                                    _demService.GetPreviousDemChecklistStatus(newResponse.DemViewModel.DemOperationId);
                            }

                            if (response.IsWriteDemAferFinalVersion ||
                                checklistStatus == DemGlobalValues.LAST_VERSION_COMPLETED)
                            {
                                response.DemChecklistStatus =
                                    _demService.GetPreviousDemChecklistStatus(newResponse.DemViewModel.DemOperationIdLastversion);
                            }
                        }
                        else if (!hasAccessPermissions)
                        {
                            response.ErrorMessageDEM =
                                Localization.GetText("R6.DEM.Main.MessageVersionUndefined");
                        }
                    }
                    else
                    {
                        bool hasAccessPermissions = false;

                        var overallStage = _demService.GetOperationStageCode(operationNumber);

                        try
                        {
                            if (overallStage != DemGlobalValues.APPR)
                            {
                                hasAccessPermissions =
                                    IDBContext.Current.HasPermission(Permission.DEM_READ) &&
                                    IDBContext.Current.HasPermission(Permission.DEM_COMMENTS_READ);
                            }
                            else
                            {
                                hasAccessPermissions =
                                    IDBContext.Current.HasPermission(Permission.CONVERGENCE_READ_PERMISSION);
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.GetLogger().WriteError(
                               "DEM ViewController",
                               string.Format(
                                   "DEM: Operation: {0}. User: {1}. Has Permission: {2}. {3}",
                                   operationNumber,
                                   IDBContext.Current.UserName,
                                   hasAccessPermissions.ToString(),
                                   e.Message),
                                   e);
                        }

                        response.EnabledForThisUser = hasAccessPermissions;

                        if (!hasAccessPermissions)
                        {
                            response.ErrorMessageDEM =
                                Localization.GetText("R6.DEM.Summary.MessageDemRequired");
                        }

                        response.Summary.ShowDEMRequired = _demService.IsDemRequiredEdit(
                            operationNumber, response.Summary.Role);

                        response.IsEditable =
                            IDBContext.Current.HasPermission(Permission.DEM_COORDINATOR_WRITE);

                        response.DemChecklistStatus =
                            Localization.GetText("R6.DEM.DEMRequired.ChecklistStatus");
                        response.SubmitEdit = false;
                        ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(response);
                        response.SubmitForDemRevalidation = false;
                    }

                    response.IsEditButtonHide =
                        !IDBContext.Current.HasPermission(Permission.DEM_REVIEWER_WRITE) &&
                        IDBContext.Current.HasPermission(Permission.DEM_TEAM_LEADER_WRITE);

                    response.IsValidationProcessStatusVisible =
                        newResponse.DemViewModel.IsValidationProcessStatusVisible;

                    response.IsCommentVisible = newResponse.DemViewModel.IsCommentVisible;

                    response.CanDownloadChecklist = newResponse.DemViewModel.CanDownloadChecklist;

                    response.CanDownloadSpecialist = _demService.DownloadSPDQRRSpecialist(operationNumber);

                    response.CanDownloadCoordinator = _demService.DownloadSPDQRRCoordinator(operationNumber);

                    response.Summary.IsEvaluabilityAssessmentNoteRequired =
                        newResponse.DemViewModel.Summary.IsEvaluabilityAssessmentNoteRequired;

                    if (response.IsEditSPDCompletedTL)
                    {
                        response.DemChecklistStatus = newResponse.DemViewModel.ChecklistStatusEditSPDReview;
                    }
                }
                else
                {
                    response.EnabledForThisUser = false;
                    response.ErrorMessageDEM = Localization.GetText("GLOBAL.SERVICE.GeneralError");
                }
            }
            else
            {
                response.EnabledForThisUser = false;
                response.ErrorMessageDEM =
                    Localization.GetText("R6.DEM.Main.MessageNotExistTemplateLifeCycle");
            }

            return View(response);
        }

        public virtual FileResult DownloadFile(string operationNumber, string version)
        {
            var file = _demExcelExportService.ExportExcel(operationNumber, version);
            string saveAsFileName = 
                string.Format(DemGlobalValues.NAME_DOWNLOAD_EXCEL, operationNumber);
            return File(file.FileExport, "application/vnd.ms-excel", saveAsFileName);
        }

        public virtual FileResult DownloadAssessmentNote(
            string operationNumber, string formatType, string language, string demOperation)
        {
            int demOperationNum = 0;
            bool result = int.TryParse(demOperation, out demOperationNum);

            var responseDownload = _demService.DownloadAssessmentNote(
                operationNumber, formatType, language, demOperationNum);

            var languageFileName = DemGlobalValues.FILE_NAME_NOTE_EN;
            if (language == Language.ES)
            {
                languageFileName = DemGlobalValues.FILE_NAME_NOTE_ES;
            }

            var reportName = languageFileName + operationNumber + "." + formatType;

            return !responseDownload.IsValid ? null :
                File(responseDownload.File, "application/" + formatType, reportName);
        }

        public virtual FileResult DownloadSummaryReport(
            string operationNumber, string formatType, string language, string demOperation)
        {
            int demOperationNum = 0;
            bool result = int.TryParse(demOperation, out demOperationNum); 

            var responseDownload = _demService.DownloadSummaryReport(
                operationNumber, formatType, language, demOperationNum);

            string summaryName = DemGlobalValues.SUMMARY_PDF_REPORT_NAME;

            if (language.ToUpper() == DemGlobalValues.TAB_RESUMEN.ToUpper())
            {
                summaryName = DemGlobalValues.RESUMEN_PDF_REPORT_NAME;
            }

            var reportName = summaryName + "-" + operationNumber + "." + formatType;

            return !responseDownload.IsValid ? null : 
                File(responseDownload.File, "application/" + formatType, reportName);
        }

        public virtual ActionResult StrategicAlignmentDataContentDEM(string operationNumber)
        {
            var newResponse = _demService.GetDemForOperation(operationNumber);
            var isDraft = newResponse.DemViewModel.DemChecklistStatus.ToUpper() == 
                DemGlobalValues.DRAFT;
            var response = PopulateDemViewModel(newResponse.DemViewModel.DemOperationId, 
                newResponse.DemViewModel.Stage, 
                newResponse.DemViewModel.CurrentRole,
                newResponse.DemViewModel.DemChecklistStatus, 
                operationNumber,
                isDraft);

            response.DemOperationId = newResponse.DemViewModel.DemOperationId;

            var dataDemOperation = _demService.GetDataDemOperation(response.DemOperationId);
            var model = StrategicAlignmentDataContent(
                response.DemOperationId, newResponse.DemViewModel.DemChecklistStatus, operationNumber);

            var resultsMatrix = _operationDataService.GetResultMatrixByOperation(operationNumber);
            model.ResultMatrixId = resultsMatrix.ResultsMatrixId;

            var isFinalVersionCompletedDem = _demService.IsFinalVersionCompletedDem(operationNumber) &&
                        !_demService.IsCompletedAppr(operationNumber);

            model.IsFinalVersionCompletedDem = isFinalVersionCompletedDem;
            model.InformationDem.DemOperationId = response.DemOperationId;
            model.InformationDem.UserName = IDBContext.Current.UserName;
            model.InformationDem.FullName = _demService.GetFullName(IDBContext.Current.UserName);
            model.InformationDem.Role = newResponse.DemViewModel.CurrentRole;
            model.InformationDem.Date = string.Format("{0:dd MMM yyyy}", DateTime.Now);
            model.InformationDem.Stage = newResponse.DemViewModel.Stage;
            model.InformationDem.StageId = dataDemOperation.DataDemOperation.DemStageId;
            model.InformationDem.CheckListVersion = newResponse.DemViewModel.DemChecklistStatus;
            model.InformationDem.CheckListVersionId = dataDemOperation.DataDemOperation.DemChecklistStatusId;

            model.CountryStrategyData.IsFinalVersionCompletedDem = isFinalVersionCompletedDem;
            model.CountryStrategyData.InformationDem.DemOperationId = response.DemOperationId;
            model.CountryStrategyData.InformationDem.UserName = IDBContext.Current.UserName;
            model.CountryStrategyData.InformationDem.FullName = _demService.GetFullName(IDBContext.Current.UserName);
            model.CountryStrategyData.InformationDem.Role = newResponse.DemViewModel.CurrentRole;
            model.CountryStrategyData.InformationDem.Date = string.Format("{0:dd MMM yyyy}", DateTime.Now);
            model.CountryStrategyData.InformationDem.Stage = newResponse.DemViewModel.Stage;
            model.CountryStrategyData.InformationDem.StageId = dataDemOperation.DataDemOperation.DemStageId;
            model.CountryStrategyData.InformationDem.CheckListVersion = newResponse.DemViewModel.DemChecklistStatus;
            model.CountryStrategyData.InformationDem.CheckListVersionId = dataDemOperation.DataDemOperation.DemChecklistStatusId;

            model.CountryProgramData.IsFinalVersionCompletedDem = isFinalVersionCompletedDem;
            model.CountryProgramData.InformationDem.DemOperationId = response.DemOperationId;
            model.CountryProgramData.InformationDem.UserName = IDBContext.Current.UserName;
            model.CountryProgramData.InformationDem.FullName = _demService.GetFullName(IDBContext.Current.UserName);
            model.CountryProgramData.InformationDem.Role = newResponse.DemViewModel.CurrentRole;
            model.CountryProgramData.InformationDem.Date = string.Format("{0:dd MMM yyyy}", DateTime.Now);
            model.CountryProgramData.InformationDem.Stage = newResponse.DemViewModel.Stage;
            model.CountryProgramData.InformationDem.StageId = dataDemOperation.DataDemOperation.DemStageId;
            model.CountryProgramData.InformationDem.CheckListVersion = newResponse.DemViewModel.DemChecklistStatus;
            model.CountryProgramData.InformationDem.CheckListVersionId = dataDemOperation.DataDemOperation.DemChecklistStatusId;

            model.SerializedViewModelSA = PageSerializationHelper.SerializeObject(model);
            var cmb = CMBusiness.Get(operationNumber);
            var activityItemAppr = cmb.GetActivityItem(CMConstants.DefaultActivityItems.APPR);
            model.StrategicAlignmentWrite =
                IDBContext.Current.HasPermission(Permission.STRATEGIC_ALIGNMENT_WRITE) && !activityItemAppr.IsCompleted();

            return PartialView(
                "~/Areas/DEM/Views/View/Partials/Tabs/StrategicAlignmentPartialDEM.cshtml", model);
        }

        public virtual ActionResult GetPartialView(
            string operationNumber, 
            string partial, 
            string inputIdentifier, 
            string addedOperationNumbers)
        {
            if (inputIdentifier != string.Empty && inputIdentifier != null)
            {
                ViewBag.inputID = inputIdentifier;

                return PartialView(partial);
            }

            return PartialView(partial);
        }

        public virtual ActionResult SummaryPartialView(
            int demOperationId, string currentRole, string operationNumber)
        {
            var summaryData = GetSummaryData(demOperationId, currentRole, operationNumber, true);
            return PartialView(
                "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", summaryData);
        }

        public virtual ActionResult ResumenPartialView(int demOperationId, string operationNumber)
        {
            var resumenData = GetResumenData(demOperationId, operationNumber, true);
            return PartialView(
                "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", resumenData);
        }

        public virtual ActionResult RiskPartalView(int demOperationId, string stage, string checkListVersion)
        {
            var riskData = GetRiskData(demOperationId, stage, checkListVersion);
            return PartialView("~/Areas/DEM/Views/View/Partials/Tabs/PRisk.cshtml", riskData);
        }

        public virtual ActionResult AdditionalityPartalView(int demOperationId, string stage, string checkListVersion)
        {
            var additionaltyData = GetAdditionalityData(demOperationId, stage, checkListVersion);
            return PartialView(
                "~/Areas/DEM/Views/View/Partials/Tabs/PAdditionality.cshtml", additionaltyData);
        }

        public virtual ActionResult EvaluabilityPartialView(int demOperationId, string stage, string checkListVersion)
        {
            var evaluabilityData = GetEvaluabilityView(demOperationId, stage, checkListVersion);
            return PartialView(
                "~/Areas/DEM/Views/View/Partials/Tabs/PEvaluability.cshtml", evaluabilityData);
        }

        public virtual ActionResult ExecuteDemRevalidation(string operationNumber)
        {
            var response = _demService.ExecuteDemRevalidation(operationNumber);

            return Json(response);
        }

        #region Public Country Development Results

        public virtual ActionResult CountryDevelopmentResultsDataContentLoad(string operationNumber)
        {
            var newResponse = _demService.GetDemForOperation(operationNumber);
            var isDraft = newResponse.DemViewModel.DemChecklistStatus.ToUpper() == 
                DemGlobalValues.DRAFT;
            var response = PopulateDemViewModel(
                newResponse.DemViewModel.DemOperationId, 
                newResponse.DemViewModel.Stage, 
                newResponse.DemViewModel.CurrentRole,
                newResponse.DemViewModel.DemChecklistStatus, 
                operationNumber, 
                isDraft);

            response.DemOperationId = newResponse.DemViewModel.DemOperationId;

            var dataDemOperation = _demService.GetDataDemOperation(response.DemOperationId);
            var model = CountryDevelopmentResultsDataContent(
                response.DemOperationId, newResponse.DemViewModel.DemChecklistStatus, operationNumber);

            model.IsFinalVersionCompletedDem = _demService.IsFinalVersionCompletedDem(operationNumber) &&
                        !_demService.IsCompletedAppr(operationNumber);
            model.InformationDem.DemOperationId = response.DemOperationId;
            model.InformationDem.UserName = IDBContext.Current.UserName;
            model.InformationDem.FullName = _demService.GetFullName(IDBContext.Current.UserName);
            model.InformationDem.Role = newResponse.DemViewModel.CurrentRole;
            model.InformationDem.Date = string.Format("{0:dd MMM yyyy}", DateTime.Now);
            model.InformationDem.Stage = newResponse.DemViewModel.Stage;
            model.InformationDem.StageId = dataDemOperation.DataDemOperation.DemStageId;
            model.InformationDem.CheckListVersion = newResponse.DemViewModel.DemChecklistStatus;
            model.InformationDem.CheckListVersionId = dataDemOperation.DataDemOperation.DemChecklistStatusId;

            response.AlignmentContribution.SerializedViewModelCountry = PageSerializationHelper.SerializeObject(model);

            return PartialView("~/Areas/DEM/Views/View/Partials/Tabs/PCountryPartial.cshtml", model);
        }

        public virtual JsonResult AccessToDemResource(string tabActive)
        {
            var response = new ResponseBase();

            var userName = IDBContext.Current.UserLoginName;
            var urlTabActive = DemGlobalValues.URL_DEM_EDIT + tabActive;

            var syncErrorMessage = SynchronizationHelper.AccessToResources(
                SynchronizationHelper.EDIT_MODE,
                urlTabActive, 
                IDBContext.Current.Operation,
                userName);

            if (!string.IsNullOrWhiteSpace(syncErrorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = syncErrorMessage;
            }
            else
            {
                response.IsValid = true;
            }

            return Json(response);
        }

        public virtual JsonResult FreeDemResource(string tabActive)
        {
            var response = new ResponseBase
            {
                IsValid = true
            };

            try
            {
                var userName = IDBContext.Current.UserLoginName;
                var urlTabActive = DemGlobalValues.URL_DEM_EDIT + tabActive;

                SynchronizationHelper.TryReleaseLock(
                    urlTabActive, 
                    IDBContext.Current.Operation,
                    userName);
            }
            catch (Exception ex)
            {
                response.IsValid = false;
                response.ErrorMessage = ex.Message;
            }

            return Json(response);
        }
        #endregion

        #region Private Methods
        private DemViewModel PopulateDemViewModel(
            int demOperationId, 
            string currentStage, 
            string currentRole,
            string currentCheckListVersion, 
            string operationNumber, 
            bool isDraft)
        {
            var response = new DemViewModel(); 
            var summary = GetSummaryData(demOperationId, currentRole, operationNumber, isDraft);
            var resumen = GetResumenData(demOperationId, operationNumber, isDraft);
            var risk = GetRiskData(demOperationId, currentStage, currentCheckListVersion);
            var validation = GetValidationData(operationNumber);
            var verify = GetVerifyStatusData();
            var additionality = GetAdditionalityData(demOperationId, currentStage, currentCheckListVersion);
            var evaluability = GetEvaluabilityView(demOperationId, currentStage, currentCheckListVersion);

            response.Summary = summary;
            response.Resumen = resumen;
            response.Risk = risk;
            response.ValidationProcessStatus = validation;
            response.VerifyStatus = verify;
            response.Additionality = additionality;
            response.Evaluability = evaluability;

            return response;
        }

        private SummaryTabViewModel GetSummaryData(
            int demOperationId, 
            string currentRole, 
            string operationNumber, 
            bool isDraft)
        {
            var response = new SummaryTabViewModel();
            var summaryRisk = _demService.GetSummaryRiskView(demOperationId, Language.EN);
            var summaryAdditionality = 
                _demService.GetSummaryAddicionalitView(demOperationId, Language.EN);
            var summaryEvaluability = 
                _demService.GetSummaryEvaluabilityView(demOperationId, Language.EN);
            var strategicAlignment = 
                _demService.GetSummarCorporateView(operationNumber, Language.EN, isDraft);
            response.EvaluabilityGrade = 
                _demService.GetEvaluabilityStatus(demOperationId, Language.EN).EvaluabilityStatus;
            response.Role = _demService.GetDEMUserRole().DEMUserRole;

            response.Risk = summaryRisk.DEMSummaryRiskViewList;
            response.Additionality = summaryAdditionality.DEMSummaryAddicionalityViewList;
            response.Evaluability = summaryEvaluability.DEMSummaryEvaluabilityViewList;
            response.StrategicAlignment = strategicAlignment.DEMSummaryCorporateView;
            response.ShowDEMRequired = _demService.IsDemRequiredEdit(operationNumber, currentRole);
            
            return response;
        }

        private SummaryTabViewModel GetResumenData(
            int demOperationId, string operationNumber, bool isDraft)
        {
            var response = new SummaryTabViewModel();
            var summaryRisk = _demService.GetSummaryRiskView(demOperationId, Language.ES);
            var summaryAdditionality = 
                _demService.GetSummaryAddicionalitView(demOperationId, Language.ES);
            var summaryEvaluability = 
                _demService.GetSummaryEvaluabilityView(demOperationId, Language.ES);
            var strategicAlignment = 
                _demService.GetSummarCorporateView(operationNumber, Language.ES, isDraft);
            response.EvaluabilityGrade = 
                _demService.GetEvaluabilityStatus(demOperationId, Language.ES).EvaluabilityStatus;
            response.Role = _demService.GetDEMUserRole().DEMUserRole;

            response.Risk = summaryRisk.DEMSummaryRiskViewList;
            response.Additionality = summaryAdditionality.DEMSummaryAddicionalityViewList;
            response.Evaluability = summaryEvaluability.DEMSummaryEvaluabilityViewList;
            response.StrategicAlignment = strategicAlignment.DEMSummaryCorporateView;

            return response;
        }

        private RiskViewModel GetRiskData(int demOperationId, string currentStage, string checkListVersion)
        {
            var riskView = _demService.GetRiskView(demOperationId, currentStage, checkListVersion);

            return riskView.DEMRiskViewList;
        }

        private List<VerifyStatusViewModel> GetVerifyStatusData()
        {
            var response = new List<VerifyStatusViewModel>
            { 
                new VerifyStatusViewModel
                {
                    Verification = 
                    "The Operations alignment to UIS Challenges and Cross-Cutting Themes should be entered and justified.",
                    Compliance = VerifyComplianceStatusEnum.CorrectAndComplete
                },
                new VerifyStatusViewModel
                {
                    Verification = 
                    "Country Strategy/ Country Program Alignment in Strategic Alignment section must be entered. ",
                    Compliance = VerifyComplianceStatusEnum.ReviewWarning
                },
                new VerifyStatusViewModel
                {
                    Verification = "Information in Summary (English version) has been entered.",
                    Compliance = VerifyComplianceStatusEnum.ReviewWarning
                },
                new VerifyStatusViewModel
                {
                    Verification = 
                    "Evaluability Development Note must be entered in Summary (English version).",
                    Compliance = VerifyComplianceStatusEnum.CorrectAndComplete
                },
                new VerifyStatusViewModel
                {
                    Verification = "Information in Resumen (Spanish version) has been entered.",
                    Compliance = VerifyComplianceStatusEnum.CorrectAndComplete
                },
                new VerifyStatusViewModel
                {
                    Verification = 
                    "Evaluability Development Note must be entered in Resumen (Spanish version).",
                    Compliance = VerifyComplianceStatusEnum.ReviewCritical
                },
                new VerifyStatusViewModel
                {
                    Verification = "Fill out Evaluability section.",
                    Compliance = VerifyComplianceStatusEnum.ReviewWarning
                },
                new VerifyStatusViewModel
                {
                    Verification = "Fill out Risk Management section.",
                    Compliance = VerifyComplianceStatusEnum.CorrectAndComplete
                },
                new VerifyStatusViewModel
                {
                    Verification = "Fill out Additionality section.",
                    Compliance = VerifyComplianceStatusEnum.ReviewWarning
                },
            };

            return response;
        }

        private AdditionalityTabViewModel GetAdditionalityData(
            int demOperationId, string currentStage, string checkListVersion)
        {
            var serviceResponse = _demService.GetAdditionalityView(demOperationId, currentStage, checkListVersion);         

            return serviceResponse.Additionality;           
        }

        private EvaluabilityTabViewModel GetEvaluabilityView(int demOperationId, string currentStage, string checkListVersion)
        {
            var serviceResponse = _demService.GetEvaluabilityView(demOperationId, currentStage, checkListVersion);

            return serviceResponse.Evaluability;
        }

        private StrategicAlignmentViewModel StrategicAlignmentDataContent(
            int demOperationId, string statusDem, string operationNumber)
        {
            var model = _operationDataService.GetStrategicAlignment(
                demOperationId, statusDem, operationNumber);

            SetViewbagStrategicAlignment(operationNumber);

            return model.StrategicAlignment;
        }

        private void SetViewbagStrategicAlignment(string operationNumber)
        {
            var resultsMatrix = _operationDataService.GetResultMatrixByOperation(operationNumber);
            var impactIndicators = new List<MultiDropDownItem>();
            var impactsResponse = _operationDataService.GetResultMatrixImpacts(operationNumber, resultsMatrix);

            if (impactsResponse.IsValid)
            {
                impactsResponse.Impacts.ForEach(x => impactIndicators.Add(new MultiDropDownItem
                {
                    Text = x.Text,
                    Value = x.Value
                }));

                ViewBag.ImpactIndicators = impactIndicators;
            }

            var outcomeIndicators = new List<MultiDropDownItem>();
            var outcomesResponse = _operationDataService.GetResultMatrixOutcomes(operationNumber, resultsMatrix);

            if (outcomesResponse.IsValid)
            {
                outcomesResponse.Outcomes.ForEach(x => outcomeIndicators.Add(new MultiDropDownItem
                {
                    Text = x.Text,
                    Value = x.Value
                }));

                ViewBag.OutcomeIndicators = outcomeIndicators;
            }

            var outputs = new List<MultiDropDownItem>();
            var outputsResponse = _operationDataService.GetResultMatrixOutputs(operationNumber, resultsMatrix);

            if (outputsResponse.IsValid)
            {
                outputsResponse.Outputs.ForEach(x => outputs.Add(new MultiDropDownItem
                {
                    Text = x.Text,
                    Value = x.Value
                }));

                ViewBag.Outputs = outputs;
            }

            ViewBag.StrategicAlignmentPermission = true;

            var objectivesList = new List<MultiDropDownItem>();
            var objectivesResponse = 
                _operationDataService.GetCountryStrategyObjectivesList(operationNumber);

            if (objectivesResponse.IsValid)
            {
                objectivesList = objectivesResponse.Objectives.Select(x => new MultiDropDownItem
                {
                    Value = x.Value,
                    Text = x.Text
                }).ToList();
            }

            ViewBag.CountryStrategyObjectivesList = objectivesList;

            var notAlignedList = new List<MultiDropDownItem>();

            if (objectivesResponse.HaveResultMatrix)
            {
                var strategyObjectivescurrentApprovedType = _catalogService.GetListMasterData(
                    MasterType.STRATEGY_OBJECTIVES_CURRENT_APPROVED);

                foreach (var data in strategyObjectivescurrentApprovedType)
                {
                    var aux = new MultiDropDownItem
                    {
                        Value = string.Format("SOCA-{0}", data.Value),
                        Text = data.Text
                    };
                    notAlignedList.Add(aux);
                }
            }

            ViewBag.CountryStrategyNotAligned = notAlignedList;
        }

        private List<ValidationProcessStatusViewModel> GetValidationData(string operationNumber)
        {
            var validationProcessStatus = _demService.GetValidationProcessStatus(
                operationNumber, IDBContext.Current.CurrentLanguage);

            return validationProcessStatus.DEMValidationProcessStatusList;
        }
        #endregion

        #region Private Country Development Results

        private AlignmentContributionDemViewModel CountryDevelopmentResultsDataContent(
            int demOperationId,
            string statusDem,
            string operationNumber)
        {
            var model = _demService.GetCountryDevelopmentResults(
                demOperationId, statusDem, operationNumber);

            return model.Country;
        }

        #endregion
    }
}
