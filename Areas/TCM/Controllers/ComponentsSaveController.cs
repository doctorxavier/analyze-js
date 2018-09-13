using System;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.TCM.Messages.ResultsMatrix;
using IDB.MW.Application.TCM.Services.ResultsMatrixService;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Components;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Components.StandardOutputs;
using IDB.MW.Business.TCM.Contracts;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Areas.TCM.Models;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class ComponentsSaveController : MVC4.Controllers.ConfluenceController
    {
        private readonly IComponentService _componentService;
        private readonly ICommonTCMService _commonTCMService;

        public ComponentsSaveController(
            IComponentService componentService, ICommonTCMService commonTCMService)
        {
            _componentService = componentService;
            _commonTCMService = commonTCMService;
        }

        public virtual JsonResult PhysicalProgressSave(
            PhysicalProgressViewModel physicalProgressModel)
        {
            SavePhysicalProgressResponse response = _componentService
                .SavePhysicalProgressData(physicalProgressModel);

            return Json(response);
        }

        public virtual JsonResult DeleteComponent(DeleteComponentViewModel deleteComponentModel)
        {
            DeleteComponentResponse response = _componentService.DeleteComponent(deleteComponentModel);

            return Json(response);
        }

        public virtual JsonResult DeleteOutput(DeleteOutputViewModel deleteOutputModel)
        {
            DeleteOutputResponse response = _componentService.DeleteOutput(deleteOutputModel);

            return Json(response);
        }

        public virtual JsonResult DeletePlannedYear(
            DeletePlannedYearViewModel deletePlannedYearModel)
        {
            DeletePlannedYearResponse response = _componentService
                .DeletePlannedYear(deletePlannedYearModel);

            return Json(response);
        }

        public virtual JsonResult ReassignOutput(ReassignOutputViewModel reassignedOutputModel)
        {
            ReassignOutputResponse response = _componentService
                .ReassignOutput(reassignedOutputModel);

            return Json(response);
        }

        public virtual JsonResult DeleteMilestone(DeleteMilestoneViewModel deleteMilestoneModel)
        {
            DeleteMilestoneResponse response = _componentService
                .DeleteMilestone(deleteMilestoneModel);

            return Json(response);
        }

        public virtual JsonResult DeleteDisaggregation(
            DeleteDisaggregationViewModel deleteDisaggregationModel)
        {
            DeleteDisaggregationResponse response = _componentService.DeleteDisaggregation(
                deleteDisaggregationModel);

            return Json(response);
        }

        public virtual JsonResult GetIndicator(string operationNumber)
        {
            AddNewOutputResponse response = _componentService.GetNewOutputIndicator(operationNumber);

            return Json(response);
        }

        public virtual JsonResult FilterStandarOutputs(
            StandardOutputFilterViewModel standardOutputFilterModel)
        {
            var response = _componentService.FilterStandardOutputs(standardOutputFilterModel);

            return Json(response);
        }

        public virtual JsonResult CreateNewOutputIndicator(
            CreateNewOutputViewModel createNewOutputViewModel)
        {
            var respone = _componentService.CreateNewOutputIndicator(
                createNewOutputViewModel, Localization.CurrentLanguage);

            return Json(respone);
        }

        public virtual JsonResult FinancialProgressSave(string operationNumber)
        {
            SaveFinancialProgressResponse response;
            var jsonDataResquest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<FinancialProgressViewModel>(jsonDataResquest.SerializedData);
            var outputYearPlanViewModel = ClientFieldDataMappers
                .UpdateOutputYearPlanViewModel(jsonDataResquest.ClientFieldData, viewModel);

            var otherCostViewModel = ClientFieldDataMappers
                .UpdateOtherCostViewModel(jsonDataResquest.ClientFieldData, viewModel);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper
                .AccessToResources("edit", TCMGlobalValues.URL_FINANCIAL_PROGRESS, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveFinancialProgressResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _componentService.UpdateFinancialProgressData(
                    outputYearPlanViewModel,
                    otherCostViewModel,
                    viewModel.Interval,
                    viewModel.ResultsMatrixId);
            }

            return Json(response);
        }

        public virtual JsonResult MappingProgressSave(string operationNumber)
        {
            SaveMappingProgressResponse response;
            var jsonDataResquest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel =
                PageSerializationHelper.DeserializeObject<MappingProgressViewModel>(
                    jsonDataResquest.SerializedData);
            var outputViewModel = ClientFieldDataMappers
                .UpdateOutputViewModel(jsonDataResquest.ClientFieldData, viewModel);
            var outputYearVisualizationViewModel = ClientFieldDataMappers
                .UpdateOutputYearVisualizationViewModel(jsonDataResquest.ClientFieldData, viewModel);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources(
                "edit", TCMGlobalValues.URL_MAPPING_PROGRESS, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveMappingProgressResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _componentService.UpdateMappingProgressData(
                    outputViewModel, outputYearVisualizationViewModel);
            }

            return Json(response);
        }

        public virtual JsonResult SaveOutputMilestoneDetail(string operationNumber)
        {
            var response = new SaveOutputIndicatorDetailResponse();
            var userName = IDBContext.Current.UserName;
            var loginName = IDBContext.Current.UserLoginName;
            try
            {
                var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
                var viewModel = PageSerializationHelper
                    .DeserializeObject<OutputMilestoneDetailViewModel>(
                    jsonDataRequest.SerializedData);

                viewModel.UpdateOutputMilestoneDetail(jsonDataRequest.ClientFieldData);
                response = _componentService.SaveMilestoneDetail(viewModel, userName);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        TCMGlobalValues.TCM_EDIT_COMPONENTS, operationNumber, loginName);
                }
            }
            catch (Exception ex)
            {
                response.IsValid = false;
                response.ErrorMessage = ex.Message;
            }

            return Json(response);
        }

        public virtual JsonResult SaveOutputIndicatorDetail(string operationNumber)
        {
            var response = new SaveOutputIndicatorDetailResponse();
            var userName = IDBContext.Current.UserName;
            var loginName = IDBContext.Current.UserLoginName;

            try
            {
                var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
                var viewModel = PageSerializationHelper
                    .DeserializeObject<OutputIndicatorDetailContentViewModel>(
                        jsonDataRequest.SerializedData);

                viewModel.UpdateOutputIndicatorDetail(jsonDataRequest.ClientFieldData);
                response = _componentService.SaveOuputIndicatorDetail(viewModel, userName);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(
                        TCMGlobalValues.TCM_EDIT_COMPONENTS, operationNumber, loginName);
                }
            }
            catch (Exception ex)
            {
                response.IsValid = false;
                response.ErrorMessage = ex.Message;
            }

            return Json(response);
        }

        public virtual JsonResult LinkToPredefinedIndicator(
            LinkToPredefinedIndicatorRequest PredefinedIndicatorRequest)
        {
            var response = _commonTCMService.LinkToPredefinedIndicator(
                PredefinedIndicatorRequest.TcmElementToLink,
                PredefinedIndicatorRequest.TcmElementToLinkId,
                PredefinedIndicatorRequest.PredefinedIndicatorId);

            return Json(response);
        }

        public virtual JsonResult UnLinkPredefinedIndicator(
            UnlinkPredefinedIndicatorsRequest unlinkPredefinedIndicatorsRequest)
        {
            var response = _commonTCMService
                .UnlinkPredefinedIndicators(unlinkPredefinedIndicatorsRequest.TcmElementToUnlink,
                    unlinkPredefinedIndicatorsRequest.TcmElementToUnlinkId,
                    unlinkPredefinedIndicatorsRequest.PredefinedIndicatorIds);

            return Json(response);
        }

        public virtual JsonResult GetCRFLinkedIndicators(
            LinkToPredefinedIndicatorRequest GetCRFLinkedIndicators)
        {
            var response = _commonTCMService
                .GetCRFLinkedIndicators(GetCRFLinkedIndicators.TcmElementToLink,
                GetCRFLinkedIndicators.TcmElementToLinkId);

            return Json(response);
        }
    }
}