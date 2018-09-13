using System;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Messages.InstitutionService;
using IDB.MW.Application.AdministrationModule.Services.InstitutionService;
using IDB.MW.Application.AdministrationModule.ViewModels.Institution;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Domain.LoanManagementV3;
using IDB.MW.Domain.Contracts.DomainServices;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class InstitutionDeleteController : MVC4.Controllers.ConfluenceController
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IInstitutionService _institutionService;
        private readonly ICatalogService _catalogService;
        private readonly ILoanManagementV3DataService _loanManagementV3DataService;

        #region Contructors
        public InstitutionDeleteController(IInstitutionService institutionService,
            ILoanManagementV3DataService loanManagementV3DataService,
            ICatalogService catalogService)
        {
            _institutionService = institutionService;
            _catalogService = catalogService;
            _authorizationService = AuthorizationServiceFactory.Current;
            _loanManagementV3DataService = loanManagementV3DataService;
        }
        #endregion

        public virtual JsonResult DeleteInstitution(int id = 0)
        {
            var response = _institutionService.DeleteInstitution(id);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult InactiveInstitution(int id = 0)
        {
            InstitutionDeleteResponse deleteInstitution = _institutionService.InactiveInstitution(id);
            return Json(deleteInstitution, JsonRequestBehavior.AllowGet);
        }

        #region Institution
        public string SendToLMSInstitution(InstitutionEditViewModel viewModel)
        {
            string codeType = _institutionService.GetMasterDataCode(viewModel.TypeId);
            string IsValid = string.Empty;
            try
            {
                _loanManagementV3DataService.SetCreateInstitutions(new CreateInstitutions
                {
                    Institution = new Institution
                    {
                        Acronym = viewModel.Acronym,
                        Address = viewModel.Address,
                        City = viewModel.City,
                        FaxNumber = viewModel.FaxNumber,
                        Name = viewModel.Name,
                        PCMailName = null,
                        State = viewModel.State,
                        TelephoneNumber = viewModel.TelNumber,
                        TypeCode = codeType,
                        Website = viewModel.Website,
                        ZipCode = viewModel.ZipCode
                    }
                });
            }
            catch (Exception e)
            {
                IsValid = e.Message;
            }

            return IsValid;
        }

        public string SendToLMSInactiveInstitution(InstitutionEditViewModel viewModel)
        {
            string codeType = _institutionService.GetMasterDataCode(viewModel.TypeId);
            string IsValid = string.Empty;
            try
            {
                _loanManagementV3DataService.SetUpdateInactivateInstitutions(new UpdateInactivateInstitutions
                {
                    Institution = new Institution
                    {
                        Acronym = viewModel.Acronym,
                        Address = viewModel.Address,
                        City = viewModel.City,
                        FaxNumber = viewModel.FaxNumber,
                        Name = viewModel.Name,
                        PCMailName = null,
                        State = viewModel.State,
                        TelephoneNumber = viewModel.TelNumber,
                        TypeCode = codeType,
                        Website = viewModel.Website,
                        ZipCode = viewModel.ZipCode
                    }
                });
            }
            catch (Exception e)
            {
                IsValid = e.Message;
            }

            return IsValid;
        }
        #endregion
    }
}