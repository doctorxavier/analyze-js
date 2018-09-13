using System.Linq;
using System.Web.Mvc;
using System.Collections.Generic;

using IDB.MW.Application.AdministrationModule.Messages.MasterDataService;
using IDB.MW.Application.Contacts.Services.OperationContacts;
using IDB.MW.Application.Contacts.Messages;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Architecture.Language;
using IDB.MW.Domain.Values;
using IDB.MW.Application.Core.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Contacts.Models
{
    public class ViewModelMapperHelper
    {
        private readonly IOperationContactsService _operationContactsService;
        private readonly ICatalogService _catalogService;

        public ViewModelMapperHelper(IOperationContactsService operationContactsService, ICatalogService catalogService)
        {
            _operationContactsService = operationContactsService;
            _catalogService = catalogService;
        }

        public GetSelectListItemResponse GetInstitutionList(string name, string operationNumber)
        {
            var response = new GetSelectListItemResponse();

            var institutionRepository = _operationContactsService.GetInstitutionNameList(name, operationNumber);

            if (institutionRepository.ListItemInstitution != null && institutionRepository.ListItemInstitution.Count > 0)
            {
                response.ListResponse = institutionRepository.ListItemInstitution
                    .Select(x => new SelectListItem
                    {
                        Value = x.Value,
                        Text = x.Text
                    }).ToList();
            }

            return response;
        }

        public GetSelectListItemResponse GetInstitutionList(string name)
        {
            var response = new GetSelectListItemResponse();

            var institutionRepository = _operationContactsService.GetInstitutionNameList(name);

            if (institutionRepository.ListItemInstitution != null && institutionRepository.ListItemInstitution.Count > 0)
            {
                response.ListResponse = institutionRepository.ListItemInstitution
                    .Select(x => new SelectListItem
                    {
                        Value = x.Value,
                        Text = x.Text
                    }).ToList();
            }

            return response;
        }

        public GetSelectListItemResponse GetUserNameList(string name)
        {
            var response = new GetSelectListItemResponse();

            var userNameRepository = _operationContactsService.GetUserNameList(name);

            if (userNameRepository.ListItemUserName != null && userNameRepository.ListItemUserName.Count > 0)
            {
                response.ListResponse = userNameRepository.ListItemUserName.Select(o => new SelectListItem
                {
                    Selected = false,
                    Value = o.Value,
                    Text = o.Text
                }).ToList();
            }

            return response;
        }

        public ListEaRoleResponse GetListEaRole()
        {
            var response = new ListEaRoleResponse();

            response = _operationContactsService.GetEaRoles();

            return response;
        }

        public ListCountryResponse GetCountryList()
        {
            var response = new ListCountryResponse();

            var countryList = _catalogService.GetMasterDataListByTypeCode(true, MasterType.COUNTRY_ASSOCIATED);

            response.CountryList = countryList.MasterDataCollection.Select(o => new ListItemViewModel
            {
                Value = o.MasterId.ToString(),
                Text = MvcHelpers.GetItemName(o, Localization.CurrentLanguage)
            }).OrderBy(x => x.Text).ToList();

            return response;
        }

        public ListItemInstitutionResponse GetInstitutionByOperation(string operationNumber)
        {
            var response = new ListItemInstitutionResponse();

            var institutionRepository = _operationContactsService.GetInstitutionByOperation(operationNumber);

            if (institutionRepository.ListItemInstitution != null && institutionRepository.ListItemInstitution.Count > 0)
            {
                response.ListItemInstitution = institutionRepository.ListItemInstitution;
            }

            return response;
        }

        public ListItemInstitutionResponse GetAllInstitutions()
        {
            var response = new ListItemInstitutionResponse();

            var institutionRepository = _operationContactsService.GetAllInstitutions();

            if (institutionRepository.ListItemInstitution != null && institutionRepository.ListItemInstitution.Count > 0)
            {
                response.ListItemInstitution = institutionRepository.ListItemInstitution;
            }

            return response;
        }

        public IList<ListItemViewModel> GetDisplayedOptions()
        {
            var selectableDisplayed = new List<ListItemViewModel>
            {
                new ListItemViewModel
                {
                    Text = string.Format("{0} {1}", Localization.GetText("Display"), "20"),
                    Value = "20"
                },
                new ListItemViewModel
                {
                    Text = string.Format("{0} {1}", Localization.GetText("Display"), "50"),
                    Value = "50"
                },
                new ListItemViewModel
                {
                    Text = string.Format("{0} {1}", Localization.GetText("Display"), "All"),
                    Value = "All"
                }
            };
            return selectableDisplayed;
        }
    }
}