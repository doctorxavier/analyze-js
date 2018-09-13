using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using IDB.MW.Application.Contacts.Services.OperationContacts;
using IDB.MW.Application.Contacts.ViewModels;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.Contacts.Models;
using IDB.MW.Application.Contacts.Messages;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.Contacts.Controllers
{
    public partial class ControlPanelContactsController : Controller
    {
        private readonly IOperationContactsService _operationContactsService;
        private readonly ICatalogService _catalogService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;

        public ControlPanelContactsController(IOperationContactsService operationContactsService,
        ICatalogService catalogService)
        {
            _operationContactsService = operationContactsService;
            _catalogService = catalogService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_operationContactsService,
                _catalogService);
        }

        public virtual ActionResult SearchControlPanelContacts(string operationNumber,
                int searchInstitutionNameId,
                string searchTitle,
                string searchUserName,
                int searchEaRole,
                string searchContactName)
        {
            operationNumber = operationNumber ?? IDBContext.Current.Operation;
            var model = new OperationContactsViewModel
            {
                OperationContactsRow = new List<OperationContactsRowViewModel>(),
                OperationNumber = operationNumber
            };
            model.Display = _viewModelMapperHelper.GetDisplayedOptions();
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ReadPartial/OperationContactsTablePartial.cshtml", model);
        }

        public virtual ActionResult SearchAdminControlPanelContacts(string operationNumber,
            int searchInstitutionNameId,
            string searchTitle,
            string searchUserName,
            int searchEaRole,
            string searchContactName)
        {
            var response = _operationContactsService.GetFilteredControlPanelContacts(searchInstitutionNameId,
                searchTitle,
                searchUserName,
                searchEaRole,
                searchContactName,
                operationNumber);

            if (!response.IsValid)
            {
                response.OperationContacts = new OperationContactsViewModel
                {
                    OperationContactsRow = new List<OperationContactsRowViewModel>()
                };

                return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ReadPartial/OperationContactsTablePartial.cshtml", response.OperationContacts);
            }

            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ReadPartial/OperationContactsTablePartial.cshtml", response.OperationContacts);
        }

        public virtual ActionResult ReadControlPanelContact(string operationNumber, int executorContactId, string userName = "")
        {
            var model = new OperationContactsRowViewModel();
            var response = _operationContactsService.GetControlPanelContact(operationNumber, executorContactId, userName);
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _viewModelMapperHelper.GetAllInstitutions();
            var countries = _viewModelMapperHelper.GetCountryList();
            model = response.Contact;
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ReadPartial/ReadControlPanelContacts.cshtml", model);
        }

        public virtual ActionResult RemoveContact(string operationNumber, int executorContactId)
        {
            var removeContact = _operationContactsService.RemoveContact(executorContactId);
            var model = new OperationContactsViewModel();
            var response = _operationContactsService.GetOperationContacts(operationNumber);
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _viewModelMapperHelper.GetAllInstitutions();
            var countries = _viewModelMapperHelper.GetCountryList();
            model.EaRole = eaRoles.EaRole;
            model.Institution = institutions.ListItemInstitution;
            model.Country = countries.CountryList;
            model.OperationContactsRow = response.OperationContacts.OperationContactsRow == null ?
                new List<OperationContactsRowViewModel>() :
                response.OperationContacts.OperationContactsRow;
            model.Display = _viewModelMapperHelper.GetDisplayedOptions();
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ControlPanelOperationContacts.cshtml", model);
        }

        public virtual JsonResult GetInstitutionName(string filter)
        {
            return Json(_viewModelMapperHelper.GetInstitutionList(filter), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetControlPanelContact(string operationNumber, int executorContactId, string userName)
        {
            var model = new OperationContactsRowViewModel();
            var response = _operationContactsService.GetControlPanelContact(operationNumber, executorContactId, userName);
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _viewModelMapperHelper.GetAllInstitutions();
            var countries = _viewModelMapperHelper.GetCountryList();
            model = response.Contact;
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/EditPartial/EditControlPanelContact.cshtml", model);
        }

        public virtual ActionResult AddNewRoleRowControlPanelContacts()
        {
            var model = new OperationContactsViewModel();
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            model.EaRole = eaRoles.EaRole;
            return PartialView("EditPartial/NewRoleRowPartial", model);
        }

        public virtual JsonResult GetOperationsList(string filter)
        {
            var response = _operationContactsService.SearchOperation(filter);

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult EditContact(
            string operationNumber,
            OperationContactsRowViewModel contact,
            List<UserRoleViewModel> user,
            string userName)
        {
            var editContact =
                _operationContactsService.EditControlPanelContact(
                    contact,
                    user,
                    operationNumber,
                    userName);
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ReadPartial/ReadControlPanelContacts.cshtml", editContact.OperationContactsReaload);
        }

        public virtual JsonResult RemoveRoleRow(int executorId, int roleId, string operationId)
        {
            var response = _operationContactsService.RemoveContactRole(executorId, roleId, operationId);
            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult AddNewControlPanelContact()
        {
            var model = new OperationContactsViewModel();
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _viewModelMapperHelper.GetAllInstitutions();
            var countries = _viewModelMapperHelper.GetCountryList();
            model.Institution = institutions.ListItemInstitution;
            model.Country = countries.CountryList;
            model.EaRole = eaRoles.EaRole;
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/EditPartial/NewControlPanelContact.cshtml", model);
        }

        public virtual ActionResult CreateControlPanelContact(string operationNumber, OperationContactsRowViewModel contact, List<UserRoleViewModel> user)
        {
            if (user == null)
            {
                user = new List<UserRoleViewModel>();
            }

            var createContact = _operationContactsService.CreateControlPanelContact(contact, user, operationNumber);

            if (!createContact.IsValid)
            {
                var modell = new OperationContactsViewModel();
                var eaRoles = _viewModelMapperHelper.GetListEaRole();
                var institutions = _viewModelMapperHelper.GetAllInstitutions();
                var countries = _viewModelMapperHelper.GetCountryList();
                modell.Institution = institutions.ListItemInstitution;
                modell.Country = countries.CountryList;
                modell.EaRole = eaRoles.EaRole;
                modell.Display = _viewModelMapperHelper.GetDisplayedOptions();
                return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ControlPanelOperationContacts.cshtml", modell);
            }

            var OperationContacts = createContact.OperationContacts;

            if (OperationContacts == null)
            {
                var modell = new OperationContactsViewModel();
                var eaRoles = _viewModelMapperHelper.GetListEaRole();
                var institutions = _viewModelMapperHelper.GetAllInstitutions();
                var countries = _viewModelMapperHelper.GetCountryList();
                modell.Institution = institutions.ListItemInstitution;
                modell.Country = countries.CountryList;
                modell.EaRole = eaRoles.EaRole;
                modell.Display = _viewModelMapperHelper.GetDisplayedOptions();
                return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ControlPanelOperationContacts.cshtml", modell);
            }

            var listOperationContacts = OperationContacts.OperationContactsRow;

            var contactResult = listOperationContacts.FirstOrDefault();

            return ReadControlPanelContact(operationNumber, contactResult.ExecutorContactId, contactResult.UserName);
        }

        public virtual JsonResult InstitutionInformation(int institutionId = 0)
        {
            var instInfo = new InstitutionInformationResponse();

            if (institutionId != 0)
            {
                return
                    Json(
                        _operationContactsService.InstitutionInformation(institutionId),
                        JsonRequestBehavior.AllowGet);
            }

            return Json(instInfo, JsonRequestBehavior.AllowGet);
        }

        public virtual FileResult DownloadClientManagerAdminFile(
           string formatType,
           string operationNumber,
           string contactName,
           string title,
           string userName,
           int eaRole,
           int institutionNameId)
        {
            var response = _operationContactsService.GetClientManagerAdminExport(operationNumber,
                formatType,
                contactName,
                title,
                userName,
                eaRole,
                institutionNameId);

            if (!response.IsValid)
            {
                return null;
            }

            var fileName = CmGlobalValues.CONTACTS_FILE_NAME;
            string application;
            switch (formatType)
            {
                case CmGlobalValues.EXCEL:
                    application = MimeTypeMap.GetMimeType(CmGlobalValues.EXCEL);
                    fileName = fileName + CmGlobalValues.DOTEXCEL;
                    break;
                default:
                    application = MimeTypeMap.GetMimeType(CmGlobalValues.PDF);
                    fileName = fileName + CmGlobalValues.DOTPDF;
                    break;
            }

            return File(response.File, application, fileName);
        }
    }
}