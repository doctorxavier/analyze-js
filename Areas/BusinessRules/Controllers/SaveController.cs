using System.Web.Mvc;

using IDB.MW.Application.BusinessRulesModule.Messages;
using IDB.MW.Domain.Session;
using IDB.MW.Application.BusinessRulesModule.Services;
using IDB.MW.Business.BusinessRulesModule.DTOs;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.BusinessRules.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.BusinessRules.Controllers
{
    public partial class SaveController : BaseController
    {
        private const string UrlBackendBusinessRuleEditor = "/BusinessRules/View/BackendBusinessRuleEditor";
        private const string UrlFrontEndBusinessRuleEditor = "/BusinessRules/View/FrontEndBusinessRuleEditor";

        #region Fields
        private readonly IBusinessRulesService _businessRulesService;
        private readonly IAuthorizationService _authorizationService;
        #endregion

        #region Constructors

        public SaveController(IBusinessRulesService businessRulesService)
        {
            _businessRulesService = businessRulesService;
            _authorizationService = AuthorizationServiceFactory.Current;
        }

        #endregion

        public virtual JsonResult SaveBackendBusinessRule(string brCode)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<BRERuleDTO>(jsonDataRequest.SerializedData);

            viewModel.UpdateBusinessRuleViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName.ToString();
            var userNameK = IDBContext.Current.UserName;
            var errorMessage = string.Empty;

            var response = new SaveBackendRuleResponse
            {
                IsValid = true,
                ErrorMessage = string.Empty
            };

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = errorMessage;
            }
            else
            {
                response = _businessRulesService.SaveBackendRule(viewModel);
                if (response.IsValid)
                {
                }
                else
                {
                    response.IsValid = false;
                }
            }

            return Json(response);
        }

        public virtual JsonResult SaveFrontendBusinessRule(string brcode)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<BRERuleDTO>(jsonDataRequest.SerializedData);

            viewModel.UpdateBusinessRuleViewModel(jsonDataRequest.ClientFieldData);

            var userName = IDBContext.Current.UserLoginName.ToString();
            var userNameK = IDBContext.Current.UserName;
            var errorMessage = string.Empty;

            var response = new SaveFrontEndRuleResponse
            {
                IsValid = true,
                ErrorMessage = string.Empty
            };

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                response.IsValid = false;
                response.ErrorMessage = errorMessage;
            }
            else
            {
                response = _businessRulesService.SaveFrontEndRule(viewModel);
                if (response.IsValid)
                {
                }
                else
                {
                    response.IsValid = false;
                }
            }

            return Json(response);
        }
    }
}