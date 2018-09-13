using System.Linq;
using System;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Messages.DelegationService;
using IDB.MW.Application.AdministrationModule.Services.DelegationService;
using IDB.MW.Application.AdministrationModule.ViewModels.Delegation;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Delegation;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.AuthorizationAll;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class DelegationSaveController : MVC4.Controllers.ConfluenceController
    {
        private readonly IDelegationService _delegationService;

        #region Contructors

        public DelegationSaveController(IDelegationService delegationService)
        {
            _delegationService = delegationService;
        }
        #endregion

        public virtual JsonResult AuthorizeAllSave(AuthorizationAllViewModel model)
        {
            var response = _delegationService.AuthorizeAll(model.ConvertToAppModel());
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult DelegationSave()
        {
            DelegationSaveResponse response;
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<DelegationViewModel>(jsonDataRequest.SerializedData);

            viewModel.UpdateDelegationViewModel(jsonDataRequest.ClientFieldData);

            viewModel.AssignSubDelegation.Attributes
                .AddRange(_delegationService
                .GetAttributesEntitiesByOperationType(viewModel.AssignSubDelegation.OperationTypes
                    .Where(x => x.OperationTypeSelected)
                    .Select(x => x.OperationTypeName).ToList()));

            foreach (var attributeType in viewModel.AssignSubDelegation.Attributes)
            {
                var attributeTypeView =
                    jsonDataRequest.ClientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name)
                    && o.Name.Equals(attributeType.AttributeControlName));
                if (attributeTypeView != null)
                {
                    attributeType.AttributeSelected = Convert.ToBoolean(attributeTypeView.Value);
                }
            }

            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = viewModel.DelegationId != 0
                ? SynchronizationHelper.AccessToResources("edit",
                OPUSGlobalValues.URL_DELEGATION,
                viewModel.DelegationId.ToString(),
                userName)
                : null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new DelegationSaveResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _delegationService.DelegatorSearchSave(viewModel);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(OPUSGlobalValues.URL_DELEGATION,
                        viewModel.DelegationId.ToString(),
                        userName);
                }
            }

            return Json(response);
        }

        public virtual JsonResult ExpireDelegation(string delegationId)
        {
            ResponseBase response;

            int delegationToBeExpiredId = Convert.ToInt32(delegationId);
            var userName = IDBContext.Current.UserLoginName;
            var errorMessage = delegationToBeExpiredId != 0
                ? SynchronizationHelper.AccessToResources("edit",
                OPUSGlobalValues.URL_DELEGATION,
                delegationId,
                userName)
                : null;

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response = new DelegationSaveResponse
                {
                    IsValid = false,
                    ErrorMessage = errorMessage
                };
            }
            else
            {
                response = _delegationService.ExpireDelegationNow(delegationToBeExpiredId, -1);

                if (response.IsValid)
                {
                    SynchronizationHelper.TryReleaseLock(OPUSGlobalValues.URL_DELEGATION,
                        delegationId.ToString(),
                        userName);
                }
            }

            return Json(response);
        }
    }
}