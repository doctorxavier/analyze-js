using System.Collections.Generic;
using System.Web.Mvc;
using IDB.Presentation.MVC4.Areas.Contacts.Models;
using IDB.MW.Application.Contacts.Services.OperationContacts;
using IDB.MW.Application.Contacts.ViewModels;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Domain.Session;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Helpers;
using IDB.Architecture.Security.Models.UserIdentity;

namespace IDB.Presentation.MVC4.Areas.Contacts.Controllers
{
    public partial class ContactsController : Controller
    {
        #region Constants
        private readonly IOperationContactsService _operationContactsService;
        private readonly ICatalogService _catalogService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IAuthorizationManager _authorizationManager;
        #endregion

        public ContactsController(IOperationContactsService operationContactsService,
            ICatalogService catalogService,
            IAuthorizationManager authorizationManager)
        {
            _operationContactsService = operationContactsService;
            _catalogService = catalogService;
            _authorizationManager = authorizationManager;

            _viewModelMapperHelper = new ViewModelMapperHelper(_operationContactsService,
                _catalogService);
        }

        public virtual ActionResult OperationContacts(string operationNumber)
        {
            var model = new OperationContactsViewModel();
            var roleUser = _authorizationManager.GetRoles(IDBContext.Current.UserName, operationNumber);
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService.GetOperationContacts(operationNumber);
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _operationContactsService.GetInstitutionByOperation(operationNumber);
            var countries = _viewModelMapperHelper.GetCountryList();
            model.Display = _viewModelMapperHelper.GetDisplayedOptions();
            model.Institution = institutions.ListItemInstitution;
            model.Country = countries.CountryList;
            model.EaRole = eaRoles.EaRole;
            model.OperationContactsRow = response.OperationContacts.OperationContactsRow == null ?
                new List<OperationContactsRowViewModel>() :
                response.OperationContacts.OperationContactsRow;
            model.OnlyRead = response.OperationContacts.OnlyRead;
            model.IsEnableAllContacts = response.OperationContacts.IsEnableAllContacts;
            return PartialView(model);
        }

        public virtual ActionResult GetContact(string operationNumber, int executorContactId, string userName, bool readOnly)
        {
            var model = new OperationContactsRowViewModel();
            var response = _operationContactsService.GetContact(operationNumber, executorContactId, userName);
            var countryList = _viewModelMapperHelper.GetCountryList();
            model = response.Contact;
            model.ReadOnly = readOnly;
            model.Operation = operationNumber ?? IDBContext.Current.Operation;
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            return PartialView("~/Areas/Contacts/Views/Contacts/EditPartial/EditContact.cshtml", model);
        }

        public virtual ActionResult AddNewContact(string operationNumber, bool onlyRead)
        {
            var model = new OperationContactsViewModel();
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService.GetOperationContacts(operationNumber);
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _viewModelMapperHelper.GetInstitutionByOperation(operationNumber);
            var countries = _viewModelMapperHelper.GetCountryList();
            model.Institution = institutions.ListItemInstitution;
            model.Country = countries.CountryList;
            model.EaRole = eaRoles.EaRole;
            model.OperationContactsRow = response.OperationContacts.OperationContactsRow == null ?
                new List<OperationContactsRowViewModel>() :
                response.OperationContacts.OperationContactsRow;
            model.OnlyRead = onlyRead;
            return PartialView("~/Areas/Contacts/Views/Contacts/EditPartial/NewContact.cshtml", model);
        }

        public virtual ActionResult ReadContact(string operationNumber, int executorContactId, bool onlyRead = true, string userName = "")
        {
            var model = new OperationContactsRowViewModel();
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService.GetContact(operationNumber, executorContactId, userName);
            model = response.Contact;
            model.OperationNumber = operationNumber;

            if (model.Institution == null)
            {
                model.Institution = new List<ListItemViewModel>();
            }

            if (model.UserRole == null)
            {
                model.UserRole = new List<UserRoleViewModel>();
            }

            model.ReadOnly = onlyRead;
            return PartialView("~/Areas/Contacts/Views/Contacts/ReadPartial/ReadContact.cshtml", model);
        }

        public virtual ActionResult ControlPanelContacts()
        {
            var model = new OperationContactsViewModel();
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _viewModelMapperHelper.GetAllInstitutions();
            var countries = _viewModelMapperHelper.GetCountryList();
            model.Institution = institutions.ListItemInstitution;
            model.Country = countries.CountryList;
            model.EaRole = eaRoles.EaRole;
            model.Display = _viewModelMapperHelper.GetDisplayedOptions();
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ControlPanelOperationContacts.cshtml", model);
        }

        public virtual ActionResult GetControlPanelContact()
        {
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/EditPartial/EditControlPanelContact.cshtml");
        }

        public virtual ActionResult ReadControlPanelContact()
        {
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/ReadPartial/ReadControlPanelContacts.cshtml");
        }

        public virtual ActionResult AddNewControlPanelContact()
        {
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/EditPartial/NewControlPanelContact.cshtml");
        }

        public virtual JsonResult GetInstitutionName(string operationNumber, string filter)
        {
            operationNumber = operationNumber ?? IDBContext.Current.Operation;
            return Json(_viewModelMapperHelper.GetInstitutionList(filter, operationNumber), JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetUserName(string filter)
        {
            return Json(_viewModelMapperHelper.GetUserNameList(filter), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult AddNewRoleRowOperationContacts(string operationNumber)
        {
            var model = new OperationContactsViewModel();
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService.GetOperationContacts(operationNumber);
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _viewModelMapperHelper.GetInstitutionByOperation(operationNumber);
            var countries = _viewModelMapperHelper.GetCountryList();
            model.Institution = institutions.ListItemInstitution;
            model.Country = countries.CountryList;
            model.EaRole = eaRoles.EaRole;
            model.OperationContactsRow = response.OperationContacts.OperationContactsRow == null ?
                new List<OperationContactsRowViewModel>() :
                response.OperationContacts.OperationContactsRow;
            return PartialView("EditPartial/NewRoleRowPartial", model);
        }

        public virtual ActionResult AddNewRoleRowControlPanelContacts()
        {
            return PartialView("~/Areas/Contacts/Views/ControlPanelContacts/EditPartial/NewRoleRowPartial.cshtml");
        }

        public virtual ActionResult SearchContacts(string operationNumber,
            int searchInstitutionNameId,
            string searchTitle,
            string searchUserName,
            int searchEaRole,
            string searchContactName,
            bool searchAllContacts)
        {
            var model = new OperationContactsViewModel();
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService
                .GetFilteredOperationContacts(searchInstitutionNameId,
                searchTitle,
                searchUserName,
                searchEaRole,
                searchAllContacts,
                searchContactName,
                operationNumber);

            model.OperationContactsRow = response.OperationContacts.OperationContactsRow == null ?
                new List<OperationContactsRowViewModel>() :
                response.OperationContacts.OperationContactsRow;

            return PartialView("~/Areas/Contacts/Views/Contacts/ReadPartial/OperationContactsTablePartial.cshtml", response.OperationContacts);
        }

        public virtual JsonResult GetUsersList(string filter)
        {
            var response = _operationContactsService.SearchUser(filter);

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual ActionResult RemoveContact(string operationNumber, int executorContactId)
        {
            var removeContact = _operationContactsService.RemoveContact(executorContactId);
            var model = new OperationContactsViewModel();
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService.GetOperationContacts(operationNumber);
            var eaRoles = _viewModelMapperHelper.GetListEaRole();
            var institutions = _viewModelMapperHelper.GetInstitutionByOperation(operationNumber);
            var countries = _viewModelMapperHelper.GetCountryList();
            model.EaRole = eaRoles.EaRole;
            model.Institution = institutions.ListItemInstitution;
            model.Country = countries.CountryList;
            model.OperationContactsRow = response.OperationContacts.OperationContactsRow == null ?
                new List<OperationContactsRowViewModel>() :
                response.OperationContacts.OperationContactsRow;
            return PartialView("~/Areas/Contacts/Views/Contacts/OperationContacts.cshtml", model);
        }

        public virtual ActionResult CreateContact(string operationNumber, OperationContactsRowViewModel contact, UserRoleViewModel user, bool onlyRead)
        {
            var createContact = _operationContactsService.CreateContact(contact, user, operationNumber);
            var model = new OperationContactsViewModel();
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService.GetOperationContacts(operationNumber);
            model.OperationContactsRow = response.OperationContacts.OperationContactsRow == null ?
                new List<OperationContactsRowViewModel>() :
                response.OperationContacts.OperationContactsRow;
            createContact.OperationContactsReaload.OperationNumber = operationNumber;
            createContact.OperationContactsReaload.ReadOnly = onlyRead;

            return PartialView("~/Areas/Contacts/Views/Contacts/ReadPartial/ReadContact.cshtml", createContact.OperationContactsReaload);
        }

        public virtual ActionResult EditContact(
            string operationNumber,
            OperationContactsRowViewModel contact,
            List<UserRoleViewModel> user,
            string userName,
            bool readOnly)
        {
            var editContact = _operationContactsService.EditContact(contact, user, operationNumber, userName);
            var model = new OperationContactsViewModel();
            model.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService.GetOperationContacts(operationNumber);
            model.OperationContactsRow = response.OperationContacts.OperationContactsRow == null ?
                new List<OperationContactsRowViewModel>() :
                response.OperationContacts.OperationContactsRow;
            model.OnlyRead = readOnly;
            editContact.OperationContactsReaload.OperationNumber = operationNumber ?? IDBContext.Current.Operation;
            return PartialView("~/Areas/Contacts/Views/Contacts/ReadPartial/ReadContact.cshtml", editContact.OperationContactsReaload);
        }

        public virtual JsonResult CheckValidUsername(string userName)
        {
            var response = _operationContactsService.CheckValidUsername(userName);

            return new JsonResult { Data = response, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        public virtual FileResult DownloadClientManagerFile(string operationNumber,
            string formatType,
            string contactName,
            string title,
            string userName,
            int eaRole,
            int institutionNameId,
            bool allContacts)
        {
            operationNumber = operationNumber ?? IDBContext.Current.Operation;
            var response = _operationContactsService.GetClientManagerExport(operationNumber,
                formatType,
                contactName,
                title,
                userName,
                eaRole,
                institutionNameId,
                allContacts);

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