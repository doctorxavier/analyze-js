using System.Collections.Generic;
using System;
using System.Web.Mvc;

using IDB.MW.Application.TCM.Messages.FindingRecommendation;
using IDB.MW.Application.TCM.Services.FindingAndRecommendationService;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Application.TCM.ViewModels.FindingAndRecommendation;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Areas.TCM.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Domain.Models.FindingRecomendations;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.FindingAndRecomendations;

namespace IDB.Presentation.MVC4.Areas.TCM.Controllers
{
    public partial class FindingRecommendationSaveController : MVC4.Controllers.ConfluenceController
    {
        private readonly IFindingRecommendationService _findingRecomendationService;
        private readonly ITcmUniverseService _tcmUniverseService;
        private readonly IDelayAchievementModelRepository _ClientDelayArchievementRepository;

        public FindingRecommendationSaveController(IFindingRecommendationService findingRecomendationService, ITcmUniverseService tcmUniverseService, IDelayAchievementModelRepository ClientDelayArchievementRepository)
        {
            _findingRecomendationService = findingRecomendationService;
            _tcmUniverseService = tcmUniverseService;
            _ClientDelayArchievementRepository = ClientDelayArchievementRepository;
        }

        public virtual JsonResult PartnerAndConsultancies(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            SavePartnerAndConsultanciesResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PartnersAndConsultanciesModel>(jsonDataRequest.SerializedData);

            viewModel.UpdatePartnerAndConsultanciesViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;

            var errorMessage = SynchronizationHelper.AccessToResources("edit", TCMGlobalValues.URL_PARTNER_CONSULTANCIES, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SavePartnerAndConsultanciesResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _findingRecomendationService.SavePartnerAndConsultancies(rspnse.OperationNumber, viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(TCMGlobalValues.URL_PARTNER_CONSULTANCIES, rspnse.OperationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult IndexProgress(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            SaveProgressResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<ProgressModel>(jsonDataRequest.SerializedData);
            viewModel.UpdateProgressViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;

            var errorMessage = SynchronizationHelper.AccessToResources("edit", TCMGlobalValues.URL_PROGRESS_RESULT, rspnse.OperationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveProgressResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _findingRecomendationService.SaveProgress(viewModel, rspnse.OperationNumber);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(TCMGlobalValues.URL_PROGRESS_RESULT, rspnse.OperationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult ProjectManagement(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            SaveProjectManagementResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<ProjectManagementModel>(jsonDataRequest.SerializedData);
            viewModel.UpdateProjectManagementViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;

            var errorMessage = SynchronizationHelper.AccessToResources("edit", TCMGlobalValues.URL_PROJECT_MANAGEMENT, rspnse.OperationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveProjectManagementResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _findingRecomendationService.SaveProjectManagement(viewModel, rspnse.OperationNumber);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(TCMGlobalValues.URL_PROJECT_MANAGEMENT, rspnse.OperationNumber, userName);
                }
            }

            return Json(response);
        }

        #region SaveSustainabilityAndInnovation

        public virtual JsonResult SustainabilityAndInnovation(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            SaveSustainabilityAndInnovation response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<SustainabilityAndInnovationModel>(jsonDataRequest.SerializedData);
            viewModel.UpdateSustainabilityAndInnovationViewModel(jsonDataRequest.ClientFieldData);
            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", TCMGlobalValues.URL_SUSTAINABILITY_INNOVATION, rspnse.OperationNumber, userName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveSustainabilityAndInnovation
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _findingRecomendationService.SaveSustainabilityAndInnovation(rspnse.OperationNumber, viewModel);
                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(TCMGlobalValues.URL_SUSTAINABILITY_INNOVATION, rspnse.OperationNumber, userName);
                }
            }

            return Json(response);
        }
        #endregion

        #region StoriesFromField

        public virtual JsonResult StoriesFromField(string operationNumber)
        {
            var rspnse = _tcmUniverseService.GetParentOperation(operationNumber);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            SaveStoriesFromFieldResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<StoriesFromTheFieldModel>(jsonDataRequest.SerializedData);
            viewModel.UpdateStoriesFromFieldViewModel(jsonDataRequest.ClientFieldData);
            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", TCMGlobalValues.URL_STORIES_FROM_FIELD, rspnse.OperationNumber, userName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveStoriesFromFieldResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _findingRecomendationService.SaveStoriesFromField(rspnse.OperationNumber, viewModel);
                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(TCMGlobalValues.URL_STORIES_FROM_FIELD, rspnse.OperationNumber, userName);
                }
            }

            return Json(response);
        }
        #endregion

        public virtual JsonResult IndexDelaysDetails(string operationNumber)
        {
            DelayResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<DelaysEditModel>(jsonDataRequest.SerializedData);

            Dictionary<int, string> ListOutputs = new Dictionary<int, string>();
            Dictionary<int, string> ListOutcomes = new Dictionary<int, string>();

            var ResultOP = _ClientDelayArchievementRepository.GetListOutputs(operationNumber);
            var ResultOC = _ClientDelayArchievementRepository.GetListOutComes(operationNumber);
            foreach (var data in ResultOP)
            {
                if (data.Key != 0)
                {
                    ListOutputs.Add(data.Key, data.Value);
                }
            }

            foreach (var data in ResultOC)
            {
                if (data.Key != 0)
                {
                    ListOutcomes.Add(data.Key, data.Value);
                }
            }

            string lang = IDBContext.Current.CurrentLanguage.ToUpper();
            var ListDelaysType = _ClientDelayArchievementRepository.GetTypesDelay(lang);

            viewModel.UpdateAchievementDelayFromFieldViewModel(jsonDataRequest.ClientFieldData, ListOutputs, ListOutcomes, ListDelaysType);

            response = new DelayResponse();

            if (viewModel.DeleteDelay.Count > 0)
            {
                response.IsValid = _ClientDelayArchievementRepository.DeleteAchievement(viewModel.DeleteDelay.ToArray());
            }

            if (viewModel.DeleteOtherDelay.Count > 0)
            {
                response.IsValid = _ClientDelayArchievementRepository.DeleteOtherDelay(viewModel.DeleteOtherDelay.ToArray());
            }

            ViewBag.IsFREditable = _findingRecomendationService.OperationEditValidation(operationNumber);

            var userName = IDBContext.Current.UserLoginName;

            var errorMessage = SynchronizationHelper.AccessToResources("edit", TCMGlobalValues.URL_DELAYS_DETAILS, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = errorMessage;
            }
            else
            {
                response.IsValid = _ClientDelayArchievementRepository.SaveDelays(viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(TCMGlobalValues.URL_DELAYS_DETAILS, operationNumber, userName);
                }
            }

            return Json(response);
        }

        #region OverallProjectManagement

        public virtual JsonResult SaveOverallProjectManagement(string operationNumber)
        {
            SaveFindingRecommendationResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<FindingRecommendationHeaderModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateOverallProjectManagementViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit",
                TCMGlobalValues.URL_OVERRAL_PROJECT,
                operationNumber,
                userName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveFindingRecommendationResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _findingRecomendationService.SaveOverallProjectManagement(operationNumber, viewModel);
                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(TCMGlobalValues.URL_OVERRAL_PROJECT,
                        operationNumber,
                        userName);
                }
            }

            return Json(response);
        }
        #endregion
    }
}