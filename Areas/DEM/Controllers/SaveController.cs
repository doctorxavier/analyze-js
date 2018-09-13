using System;
using System.Web.Mvc;

using IDB.Architecture.Cache;
using IDB.MW.Application.DEMModule.Services;
using IDB.MW.Application.DEMModule.ViewModels;
using IDB.MW.Business.DEMModule.Messages;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.Presentation.MVC4.Areas.DEM.Models;
using IDB.Presentation.MVC4.Helpers;
using Business = IDB.MW.Business.DEMModule.Contracts;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Values.Dem;

namespace IDB.Presentation.MVC4.Areas.DEM.Controllers
{
    public partial class SaveController : ConfluenceController
    {
        private readonly IDEMService _demService;
        private readonly Business.IDEMService _demBusinessService;
        private readonly IOperationRepository _operationRepository;
        private ICacheManagement _cacheData = null;
        private string _impactsIndicatorCacheName = string.Empty;
        private string _outputsPhysicalCacheName = string.Empty;
        private string _outputsFinancialCacheName = string.Empty;
        private string _outcomesCacheName = string.Empty;

        public SaveController(
            IDEMService demService, 
            Business.IDEMService demBusinessService, 
            IOperationRepository operationRepository,
            ICacheManagement cacheData)
        {
            _demService = demService;
            _demBusinessService = demBusinessService;
            _operationRepository = operationRepository;
            _cacheData = cacheData;
            _impactsIndicatorCacheName = string.Format(CacheNames.IMPACTS, 
                IDBContext.Current.Operation);
            _outputsPhysicalCacheName = string.Format(CacheNames.OUTPUTS_PHYSICAL, 
                IDBContext.Current.Operation);
            _outputsFinancialCacheName = string.Format(CacheNames.OUTPUTS_FINANCIAL, 
                IDBContext.Current.Operation);
            _outcomesCacheName = string.Format(CacheNames.OUTCOMES, 
                IDBContext.Current.Operation);
        }

        public virtual JsonResult Summary(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModelOld = 
                PageSerializationHelper.DeserializeObject<DemViewModel>(jsonDataRequest.SerializedData);

            var viewModel = new DemViewModel();

            if (viewModelOld != null)
            {
                if (viewModelOld.Required)
                {
                    viewModel.Resumen.EvaluabilityAssessmentNote = viewModelOld.Resumen.EvaluabilityAssessmentNote;
                    viewModel.Summary.EvaluabilityAssessmentNote = viewModelOld.Summary.EvaluabilityAssessmentNote;
                    viewModel.Required = viewModelOld.Required;
                    viewModel.Justification = viewModelOld.Justification;
                    viewModel.Summary.ShowDEMRequired = viewModelOld.Summary.ShowDEMRequired;
                    viewModel.DemOperationId = viewModelOld.DemOperationId;
                }
                else
                {
                    viewModel.Summary.ShowDEMRequired = viewModelOld.Summary.ShowDEMRequired;
                    viewModel.Justification = viewModelOld.Justification;
                    viewModel.Resumen.EvaluabilityAssessmentNote = viewModelOld.Resumen.EvaluabilityAssessmentNote;
                    viewModel.Summary.EvaluabilityAssessmentNote = viewModelOld.Summary.EvaluabilityAssessmentNote;
                }
            }

            viewModel.OperationNumber = operationNumber;
            viewModel.UpdateDemViewModel(jsonDataRequest.ClientFieldData);

            var response = _demService.SaveDem(viewModel.DemOperationId, viewModel);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            return Json(new JsonResult
            {
                Data = new
                {
                    partialSummary = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", 
                        response.SummaryModel),
                    partialResumen = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", 
                        response.ResumenModel),
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage
                }
            });
        }

        public virtual JsonResult RiskSave(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModelOld = 
                PageSerializationHelper.DeserializeObject<DemViewModel>(jsonDataRequest.SerializedData);

            var viewModel = new DemViewModel();

            if (viewModelOld != null)
            {
                viewModel.Risk.CurrentStage = viewModelOld.Stage;
                viewModel.Risk.CurrentCheckListVersion = viewModelOld.DemChecklistStatus;
                viewModel.Risk.DemOperationId = viewModelOld.DemOperationId;
                viewModel.Resumen.EvaluabilityAssessmentNote = 
                    viewModelOld.Resumen.EvaluabilityAssessmentNote;
                viewModel.Summary.EvaluabilityAssessmentNote = 
                    viewModelOld.Summary.EvaluabilityAssessmentNote;
                viewModel.Required = viewModelOld.Required;
                viewModel.Justification = viewModelOld.Justification;
                viewModel.Summary.ShowDEMRequired = viewModelOld.Summary.ShowDEMRequired;
                viewModel.DemOperationId = viewModelOld.DemOperationId;
            }

            viewModel.Risk.UpdateRiskViewModel(jsonDataRequest.ClientFieldData);            
            var response = _demService.SaveRisk(viewModel.Risk.DemOperationId, viewModel.Risk);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            if (response.IsValid && viewModelOld != null)
            {
                response.DemViewModel.CanDownloadSpecialist = viewModelOld.CanDownloadSpecialist;
                response.DemViewModel.CanDownloadCoordinator = viewModelOld.CanDownloadCoordinator;
                response.DemViewModel.CanDownloadChecklist = viewModelOld.CanDownloadChecklist;
                response.DemViewModel.OfflineIsEnabled = viewModelOld.OfflineIsEnabled;
                response.DemViewModel.CurrentRole = viewModelOld.CurrentRole;

                if (viewModelOld.IsWriteDemAferFinalVersion)
                {
                    var operationId = _demBusinessService.GetOperationId(operationNumber);
                    var demChecklistStatusCodeInVps = _demService.GetPreviousDemChecklistStatusCode(operationNumber);

                    var rspnseCompleted = _demBusinessService.DraftVersionToCompletedVersion(
                        operationNumber,
                        operationId,
                        demChecklistStatusCodeInVps);

                    var responseSetCompleted = _demBusinessService.SetCompletedVersionDem(
                        operationNumber,
                        DemGlobalValues.LAST_VERSION_COMPLETED);

                    if (rspnseCompleted.IsValid)
                    {
                        response.DemViewModel.DemChecklistStatus =
                            _demService.GetPreviousDemChecklistStatus(_demService.GetDemOpereationIdCompleted(operationNumber));

                        var validationProcessStatus = _demService.GetValidationProcessStatus(
                            operationNumber, IDBContext.Current.CurrentLanguage);

                        if (validationProcessStatus.DEMValidationProcessStatusList != null)
                        {
                            response.DemViewModel.ValidationProcessStatus = validationProcessStatus.DEMValidationProcessStatusList;
                        }
                    }

                    response.IsValid = rspnseCompleted.IsValid;
                    response.ErrorMessage = rspnseCompleted.ErrorMessage;
                }
                else
                {
                    response.DemViewModel.DemChecklistStatus = viewModelOld.DemChecklistStatus;
                    var validationProcessStatus = _demService.GetValidationProcessStatus(
                            operationNumber, IDBContext.Current.CurrentLanguage);

                    if (validationProcessStatus.DEMValidationProcessStatusList != null)
                    {
                        response.DemViewModel.ValidationProcessStatus = validationProcessStatus.DEMValidationProcessStatusList;
                    }
                }
            }

            return Json(new JsonResult
            {
                Data = new
                {
                    partial = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PRisk.cshtml", response.RiskModel),
                    partialSummary = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", 
                        response.SummaryModel),
                    partialResumen = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", 
                        response.ResumenModel),
                    partialHeader = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/PHeaderInfo.cshtml",
                        response.DemViewModel),
                    partialValidation = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PValidationProcessStatus.cshtml",
                        response.DemViewModel),
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage
                }
            });
        }

        public virtual JsonResult AdditionalitySave(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModel = new DemViewModel();

            var viewModelOld = 
                PageSerializationHelper.DeserializeObject<DemViewModel>(jsonDataRequest.SerializedData);

            if (viewModelOld != null)
            {
                viewModel.Additionality.CurrentStage = viewModelOld.Stage;
                viewModel.Additionality.CurrentCheckListVersion = viewModelOld.DemChecklistStatus;
                viewModel.Additionality.DemOperationId = viewModelOld.DemOperationId;

                viewModel.Resumen.EvaluabilityAssessmentNote = 
                    viewModelOld.Resumen.EvaluabilityAssessmentNote;
                viewModel.Summary.EvaluabilityAssessmentNote = 
                    viewModelOld.Summary.EvaluabilityAssessmentNote;
                viewModel.Required = viewModelOld.Required;
                viewModel.Justification = viewModelOld.Justification;
                viewModel.Summary.ShowDEMRequired = viewModelOld.Summary.ShowDEMRequired;
                viewModel.DemOperationId = viewModelOld.DemOperationId;
            }

            viewModel.Additionality.UpdateAdditionalityViewModel(jsonDataRequest.ClientFieldData);

            var response = _demService.SaveAdditionality(
                viewModel.Additionality.DemOperationId, viewModel.Additionality);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            if (response.IsValid && viewModelOld != null)
            {
                response.DemViewModel.CanDownloadSpecialist = viewModelOld.CanDownloadSpecialist;
                response.DemViewModel.CanDownloadCoordinator = viewModelOld.CanDownloadCoordinator;
                response.DemViewModel.CanDownloadChecklist = viewModelOld.CanDownloadChecklist;
                response.DemViewModel.OfflineIsEnabled = viewModelOld.OfflineIsEnabled;
                response.DemViewModel.CurrentRole = viewModelOld.CurrentRole;

                if (viewModelOld.IsWriteDemAferFinalVersion)
                {
                    var operationId = _demBusinessService.GetOperationId(operationNumber);
                    var demChecklistStatusCodeInVps = _demService.GetPreviousDemChecklistStatusCode(operationNumber);

                    var rspnseCompleted = _demBusinessService.DraftVersionToCompletedVersion(
                        operationNumber,
                        operationId,
                        demChecklistStatusCodeInVps);

                    var responseSetCompleted = _demBusinessService.SetCompletedVersionDem(
                        operationNumber,
                        DemGlobalValues.LAST_VERSION_COMPLETED);

                    if (rspnseCompleted.IsValid)
                    {
                        response.DemViewModel.DemChecklistStatus =
                            _demService.GetPreviousDemChecklistStatus(_demService.GetDemOpereationIdCompleted(operationNumber));

                        var validationProcessStatus = _demService.GetValidationProcessStatus(
                            operationNumber, IDBContext.Current.CurrentLanguage);

                        if (validationProcessStatus.DEMValidationProcessStatusList != null)
                        {
                            response.DemViewModel.ValidationProcessStatus = validationProcessStatus.DEMValidationProcessStatusList;
                        }
                    }

                    response.IsValid = rspnseCompleted.IsValid;
                    response.ErrorMessage = rspnseCompleted.ErrorMessage;
                }
                else
                {
                    response.DemViewModel.DemChecklistStatus = viewModelOld.DemChecklistStatus;
                    var validationProcessStatus = _demService.GetValidationProcessStatus(
                            operationNumber, IDBContext.Current.CurrentLanguage);

                    if (validationProcessStatus.DEMValidationProcessStatusList != null)
                    {
                        response.DemViewModel.ValidationProcessStatus = validationProcessStatus.DEMValidationProcessStatusList;
                    }
                }
            }

            return Json(new JsonResult
            {
                Data = new
                {
                    partial = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PAdditionality.cshtml", 
                        response.AdditionalityModel),
                    partialSummary = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", 
                        response.SummaryModel),
                    partialResumen = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", 
                        response.ResumenModel),
                    partialHeader = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/PHeaderInfo.cshtml",
                        response.DemViewModel),
                    partialValidation = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PValidationProcessStatus.cshtml",
                        response.DemViewModel),
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage
                }
            });
        }

        public virtual JsonResult EvaluabilitySave(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModel = new DemViewModel();

            var viewModelOld = 
                PageSerializationHelper.DeserializeObject<DemViewModel>(jsonDataRequest.SerializedData);

            if (viewModelOld != null)
            {
                viewModel.Evaluability.CurrentStage = viewModelOld.Stage;
                viewModel.Evaluability.CurrentCheckListVersion = viewModelOld.DemChecklistStatus;
                viewModel.Evaluability.DemOperationId = viewModelOld.DemOperationId;

                viewModel.Resumen.EvaluabilityAssessmentNote = 
                    viewModelOld.Resumen.EvaluabilityAssessmentNote;
                viewModel.Summary.EvaluabilityAssessmentNote = 
                    viewModelOld.Summary.EvaluabilityAssessmentNote;
                viewModel.Required = viewModelOld.Required;
                viewModel.Justification = viewModelOld.Justification;
                viewModel.Summary.ShowDEMRequired = viewModelOld.Summary.ShowDEMRequired;
                viewModel.DemOperationId = viewModelOld.DemOperationId;
            }

            viewModel.Evaluability.UpdateEvaluabilityViewModel(jsonDataRequest.ClientFieldData);

            var response = _demService.SaveEvaluability(
                viewModel.Evaluability.DemOperationId, viewModel.Evaluability);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel);

            if (response.IsValid && viewModelOld != null)
            {
                response.DemViewModel.CanDownloadSpecialist = viewModelOld.CanDownloadSpecialist;
                response.DemViewModel.CanDownloadCoordinator = viewModelOld.CanDownloadCoordinator;
                response.DemViewModel.CanDownloadChecklist = viewModelOld.CanDownloadChecklist;
                response.DemViewModel.OfflineIsEnabled = viewModelOld.OfflineIsEnabled;
                response.DemViewModel.CurrentRole = viewModelOld.CurrentRole;

                if (viewModelOld.IsWriteDemAferFinalVersion)
                {
                    var operationId = _demBusinessService.GetOperationId(operationNumber);
                    var demChecklistStatusCodeInVps = _demService.GetPreviousDemChecklistStatusCode(operationNumber);

                    var rspnseCompleted = _demBusinessService.DraftVersionToCompletedVersion(
                        operationNumber,
                        operationId,
                        demChecklistStatusCodeInVps);

                    var responseSetCompleted = _demBusinessService.SetCompletedVersionDem(
                        operationNumber,
                        DemGlobalValues.LAST_VERSION_COMPLETED);

                    if (rspnseCompleted.IsValid)
                    {
                        response.DemViewModel.DemChecklistStatus =
                            _demService.GetPreviousDemChecklistStatus(_demService.GetDemOpereationIdCompleted(operationNumber));

                        var validationProcessStatus = _demService.GetValidationProcessStatus(
                            operationNumber, IDBContext.Current.CurrentLanguage);

                        if (validationProcessStatus.DEMValidationProcessStatusList != null)
                        {
                            response.DemViewModel.ValidationProcessStatus = validationProcessStatus.DEMValidationProcessStatusList;
                        }
                    }

                    response.IsValid = rspnseCompleted.IsValid;
                    response.ErrorMessage = rspnseCompleted.ErrorMessage;
                }
                else
                {
                    response.DemViewModel.DemChecklistStatus = viewModelOld.DemChecklistStatus;
                    var validationProcessStatus = _demService.GetValidationProcessStatus(
                            operationNumber, IDBContext.Current.CurrentLanguage);

                    if (validationProcessStatus.DEMValidationProcessStatusList != null)
                    {
                        response.DemViewModel.ValidationProcessStatus = validationProcessStatus.DEMValidationProcessStatusList;
                    }
                }
            }

            return Json(new JsonResult
            {
                Data = new
                {
                    partial = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PEvaluability.cshtml", 
                        response.EvaluavilityModel),
                    partialSummary = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", 
                        response.SummaryModel),
                    partialResumen = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PSummaryDualVersion.cshtml", 
                        response.ResumenModel),
                    partialHeader = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/PHeaderInfo.cshtml",
                        response.DemViewModel),
                    partialValidation = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PValidationProcessStatus.cshtml",
                        response.DemViewModel),
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage
                }
            });
        }

        public virtual ActionResult CountryDevelopmentResults(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);

            var viewModel = PageSerializationHelper
                .DeserializeObject<AlignmentContributionDemViewModel>(jsonDataRequest.SerializedData);

            viewModel = viewModel.UpdateCountryViewModel(jsonDataRequest.ClientFieldData);

            var response = _demService.SaveCountryDevelopmentResults(operationNumber, viewModel);

            if (response.IsValid)
            {
                _cacheData.Remove(_impactsIndicatorCacheName, System.Web.Caching.CacheItemRemovedReason.DependencyChanged);
                _cacheData.Remove(_outputsPhysicalCacheName, System.Web.Caching.CacheItemRemovedReason.DependencyChanged);
                _cacheData.Remove(_outputsFinancialCacheName, System.Web.Caching.CacheItemRemovedReason.DependencyChanged);
                _cacheData.Remove(_outcomesCacheName, System.Web.Caching.CacheItemRemovedReason.DependencyChanged);
            }

            var viewModelDem = new DemViewModel()
            {
                Stage = viewModel.InformationDem.Stage != null ?
                    viewModel.InformationDem.Stage.ToUpper() :
                    string.Empty,
                CurrentUser = viewModel.InformationDem.UserName,
                LastUpdate = Convert.ToDateTime(viewModel.InformationDem.Date),
                DemChecklistStatus = viewModel.InformationDem.CheckListVersion
            };

            if (response.IsValid)
            {
                var isWriteDemAfterFinalVersion = _demService.IsFinalVersionCompletedDem(operationNumber) &&
                    !_demService.IsCompletedAppr(operationNumber);

                var demChecklistStatusCodeInVps = _demService.GetPreviousDemChecklistStatusCode(operationNumber);

                if (isWriteDemAfterFinalVersion)
                {
                    var operationId = _demBusinessService.GetOperationId(operationNumber);

                    var rspnseCompleted = _demBusinessService.DraftVersionToCompletedVersion(
                        operationNumber,
                        operationId,
                        demChecklistStatusCodeInVps);

                    var responseSetCompleted = _demBusinessService.SetCompletedVersionDem(
                        operationNumber,
                        DemGlobalValues.LAST_VERSION_COMPLETED);

                    if (rspnseCompleted.IsValid)
                    {
                        viewModelDem.DemChecklistStatus =
                            _demService.GetPreviousDemChecklistStatus(_demService.GetDemOpereationIdCompleted(operationNumber));
                    }

                    response.IsValid = rspnseCompleted.IsValid;
                    response.ErrorMessage = rspnseCompleted.ErrorMessage;
                }

                if (response.IsValid)
                {
                    var validationProcessStatus = _demService.GetValidationProcessStatus(
                    operationNumber, IDBContext.Current.CurrentLanguage);

                    if (validationProcessStatus.DEMValidationProcessStatusList != null)
                    {
                        viewModelDem.ValidationProcessStatus = validationProcessStatus.DEMValidationProcessStatusList;
                    }

                    viewModelDem.CanDownloadSpecialist = _demService.DownloadSPDQRRSpecialist(operationNumber);
                    viewModelDem.CanDownloadCoordinator = _demService.DownloadSPDQRRCoordinator(operationNumber);
                    viewModelDem.CanDownloadChecklist = _demService.CanDownloadChecklist(
                                operationNumber,
                                demChecklistStatusCodeInVps,
                                viewModel.InformationDem.Role);
                    viewModelDem.OfflineIsEnabled = _demService.GetOfflineFunctionalityStatus(
                                operationNumber).OfflineFunctionalityIsEnabled;
                    viewModelDem.CurrentRole = viewModel.InformationDem.Role;
                }
            }

            return Json(new JsonResult
            {
                Data = new
                {
                    partialHeader = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/PHeaderInfo.cshtml", 
                        viewModelDem),
                    partialValidation = this.RenderRazorViewToString(
                        "~/Areas/DEM/Views/View/Partials/Tabs/PValidationProcessStatus.cshtml",
                        viewModelDem),
                    IsValid = response.IsValid,
                    ErrorMessage = response.ErrorMessage
                }
            });
        }

        public virtual JsonResult DraftVersionToCompletedVersion(
            string operationNumber, string nextChecklistStatus = null)
        {
            var operation = _operationRepository.GetOne(x => x.OperationNumber == operationNumber);

            var response = _demBusinessService.DraftVersionToCompletedVersion(
                operationNumber, operation.OperationId, nextChecklistStatus);

            return Json(response);
        }

        public virtual JsonResult UpdateDEMStageDraftVersion(
            string operationNumber, string newStageCode)
        {
            var response = 
                _demBusinessService.UpdateDEMStageDraftVersion(operationNumber, newStageCode);

            return Json(response);
        }

        public virtual JsonResult SetDraftChecklistStatus(string operationNumber, string isBlocked)
        {
            var response = _demBusinessService.SetDraftChecklistStatus(
                operationNumber, Convert.ToBoolean(Convert.ToInt16(isBlocked)));

            return Json(response);
        }

        public virtual JsonResult SetCompletedVersionDem(
            string operationNumber, string newCode = null)
        {
            var response = new UpdateDEMStageResponse();

            if (newCode != null)
            {
                response = _demBusinessService.SetCompletedVersionDem(operationNumber, newCode);
            }

            return Json(response);
        }
    }
}