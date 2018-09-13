using System.Web.Mvc;

using IDB.MW.Application.AttributesModule.Messages.AttributesService;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.AttributesModule.ViewModels.AttributesService;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Attribute;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Services.Core.DynamicFormsHelper;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class AttributeSaveController : MVC4.Controllers.ConfluenceController
    {
        // GET: AdministrationSecondPhase/AttributeSave
        private const string UrlAttributeView = "/AdministrationSecondPhase/Attributes";
        private readonly IAttributesManagementService _attributesManagementService;
        private readonly IAttributesEngineService _attributesEngineService;

        public AttributeSaveController(IAttributesManagementService attributesManagementService,
                                       IAttributesEngineService attributesEngineService)
        {
            _attributesManagementService = attributesManagementService;
            _attributesEngineService = attributesEngineService;
        }

        public virtual JsonResult AttributeEditSave(int id = 0)
        {
            AttributeEditSaveResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<AttributeEditViewModel>(jsonDataRequest.SerializedData);
            var isNew = viewModel.AttributeId == 0;
            viewModel.UpdateAttributeViewModel(jsonDataRequest.ClientFieldData);
            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = SynchronizationHelper.AccessToResources("edit", UrlAttributeView, id.ToString(), userName);
            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new AttributeEditSaveResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _attributesManagementService.AttributeEditSave(viewModel);
                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(UrlAttributeView, id.ToString(), userName);
                }

                response.IsValid = !isNew;
            }

            return Json(response);
        }

        #region LayoutEditor

        public virtual JsonResult SaveAttributeForm(int attributeFormId)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<AttributeFormViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateAttributesFormViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = string.Empty;

            SaveResponse response = new SaveResponse();

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = errorMessage;
            }
            else
            {
                response = _attributesEngineService.SaveAttributeFormResponse(viewModel);
                if (response.IsValid)
                {
                }
                else
                {
                    return Json(new
                    {
                        response.ErrorMessage
                    });
                }
            }

            response.IsValid = true;
            return Json(response);
        }

        #endregion
    }
}