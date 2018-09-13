using System.Web.Mvc;

using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Institution;
using IDB.MW.Application.AdministrationModule.Services.InstitutionService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.AdministrationModule.Messages.InstitutionService;
using IDB.MW.Application.AdministrationModule.ViewModels.Institution;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class InstitutionSaveController : BaseController
    {
        private const string URL_INSTITUTION_VIEW = "/AdministrationSecondPhase/Institution";
        private readonly IInstitutionService _institutionService;
        private readonly ICatalogService _catalogService;

        #region Contructors

        public InstitutionSaveController(IInstitutionService institutionService,
            ICatalogService catalogService)
        {
            _institutionService = institutionService;
            _catalogService = catalogService;
        }

        #endregion

        public virtual JsonResult InstitutionEditSave(int id = 0, bool repeatAddress = false)
        {
            InstitutionEditSaveResponse response;

            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<InstitutionEditViewModel>(jsonDataRequest.SerializedData);
            var isNew = viewModel.InstitutionId == 0;
            viewModel.UpdateInstitutionViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", URL_INSTITUTION_VIEW, id.ToString(), userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new InstitutionEditSaveResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _institutionService.InstitutionEditSave(viewModel, repeatAddress);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(URL_INSTITUTION_VIEW, id.ToString(), userName);
                }

                response.IsValid = !isNew;
            }

            return Json(response);
        }

        public virtual JsonResult UpdateRegisterInstitution(int institutionId, string insModePag)
        {
            RegisterUpdateInstitutionLmsResponse response;

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", URL_INSTITUTION_VIEW, institutionId.ToString(), userName);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new RegisterUpdateInstitutionLmsResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _institutionService.RegisterUpdateInstitutionLms(institutionId, insModePag);
                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(URL_INSTITUTION_VIEW, institutionId.ToString(), userName);
                }
            }
            
            return Json(response);
        }
    }
}