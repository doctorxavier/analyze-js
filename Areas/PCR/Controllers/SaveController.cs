using System;
using System.Web.Mvc;

using IDB.MW.Application.PCRModule.Services.ChecklistService;
using IDB.MW.Application.PCRModule.Services.FollowUpService;
using IDB.MW.Application.PCRModule.ViewModels.ChecklistService;
using IDB.MW.Application.PCRModule.ViewModels.FollowUpService;
using IDB.Presentation.MVC4.Areas.PCR.Models;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.PCRModule.Services.GuidelineService;
using IDB.MW.Application.PCRModule.Messages.ChecklistService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Application.PCRModule.ViewModels.DocumentService;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.PCR.Controllers
{
    public partial class SaveController : BaseController
    {
        #region Constants
        private const string UrlSummary = "/PCR/Summary";
        private const string UrlEffectiveness = "/PCR/PCRChecklist/Effectiveness";
        private const string UrlGeneral = "/PCR/PCRChecklist/General";
        private const string UrlValidation = "/PCR/PCRChecklist/Validation";
        private const string UrlFollowUp = "/PCR/UrlFollowUp";

        #endregion

        #region Fields

        private readonly IPCRChecklistService _pcrChecklistService;
        private readonly IPCRGuidelineService _pcrGuidelineService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IPCRFollowUpService _pcrFollowUpService;

        #endregion

        #region Constructor
        public SaveController(IPCRChecklistService pcrChecklistService, IPCRGuidelineService pcrGuidelineService, IPCRFollowUpService pcrFollowUpService)
        {
            _authorizationService = AuthorizationServiceFactory.Current;
            _pcrChecklistService = pcrChecklistService;
            _pcrGuidelineService = pcrGuidelineService;
            _pcrFollowUpService = pcrFollowUpService;
        }
        #endregion

        #region SaveActions

        #region PCR-1
        public virtual JsonResult Summary(string operationNumber)
        {
            SaveSummaryResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PCRSummaryViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdatePCRSummaryViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", UrlSummary, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveSummaryResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _pcrChecklistService.SaveSummary(viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(UrlSummary, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult Effectiveness(string operationNumber)
        {
            SaveEffectivenessResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PCRChecklistViewModel>(jsonDataRequest.SerializedData);

            viewModel.PCREffectivenessViewModel.UpdatePCREffectivenessViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", UrlEffectiveness, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveEffectivenessResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _pcrChecklistService.SaveEffectiveness(viewModel.PCREffectivenessViewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(UrlEffectiveness, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult General(string operationNumber)
        {
            SaveGeneralResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PCRChecklistViewModel>(jsonDataRequest.SerializedData);            
         
            viewModel.PCRGeneralViewModel.UpdatePCRGeneralViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", UrlGeneral, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveGeneralResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _pcrChecklistService.SaveGeneral(viewModel.PCRGeneralViewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(UrlGeneral, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult Validation(string operationNumber)
        {
            SaveValidationsResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PCRChecklistViewModel>(jsonDataRequest.SerializedData);
            var userName = IDBContext.Current.UserName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", UrlValidation, operationNumber, userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new SaveValidationsResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _pcrChecklistService.SaveValidations(viewModel.PCRValidationsViewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(UrlValidation, operationNumber, userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult EffectivenessGuideLine()
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PCRChecklistViewModel>(jsonDataRequest.SerializedData);

            viewModel.PCREffectivenessViewModel.EffectivenessGuidelinesViewModel.UpdateEffectivenessGuidelinesViewModel(jsonDataRequest.ClientFieldData);
            var response = _pcrGuidelineService.SaveEffectivenessGuidelines(viewModel.PCREffectivenessViewModel.EffectivenessGuidelinesViewModel);
            return Json(response);
        }

        public virtual JsonResult GeneralGuideLine()
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PCRChecklistViewModel>(jsonDataRequest.SerializedData);

            viewModel.PCRGeneralViewModel.GeneralGuidelinesViewModel.UpdateGeneralGuidelinesViewModel(jsonDataRequest.ClientFieldData);
            var response = _pcrGuidelineService.SaveGeneralGuidelines(viewModel.PCRGeneralViewModel.GeneralGuidelinesViewModel);
            return Json(response);
        }

        #endregion

        #region PCR-2
        public virtual JsonResult FollowUp(string operationNumber, int currentTask)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<PCRFollowUpViewModel>(jsonDataRequest.SerializedData);
            var userName = IDBContext.Current.UserName;
            viewModel.UpdateFollowUp(jsonDataRequest.ClientFieldData, currentTask);

            var response = _pcrFollowUpService.SaveFollowUp(operationNumber, viewModel, currentTask);
            SynchronizationHelper.TryReleaseLock(UrlFollowUp, operationNumber, userName);
            return Json(response);
        }

        public virtual JsonResult FollowUpOldMethodology(PCRFollowUpOldMethodologyViewModel model)
        {
            var response = _pcrFollowUpService.SaveFollowUpOldMethodology(model);

            return Json(new
            {
                response.IsValid,
                response.ErrorMessage,
                Route = Url.Action("PCRFollowUp",
                    "View",
                    new
                    {
                        area = "PCR",
                        operationNumber = model.OperationNumber
                    })
            });
        }

        #endregion

        #endregion

        #region AJAX

        public virtual JsonResult EditPCR(string operationNumber)
        {
            var responseEditPCR = _pcrChecklistService.EditPCR(operationNumber);
            return Json(responseEditPCR);
        }

        public virtual JsonResult AddNewDocummentValidation(string documentNumber, int PCRId, string operationNumber)
        {
            var result = new JsonResult();

            PCRDocumentTLViewModel document = new PCRDocumentTLViewModel()
            {
                DocumentNumber = documentNumber,
                PCRId = PCRId,
                Date = DateTime.Now.Date,
                OperationNumber = operationNumber,
                userName = IDBContext.Current.UserName
            };
            var response = _pcrChecklistService.AddDocumentValidation(document);

            result.Data = new
            {
                response.IsValid,
                response.ErrorMessage
            };

            return result;
        }

        #endregion
    }
}