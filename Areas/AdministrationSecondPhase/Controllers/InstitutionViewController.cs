using System.Collections.Generic;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.AdministrationModule.Services.InstitutionService;
using IDB.MW.Application.AdministrationModule.ViewModels.Institution;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Institution;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class InstitutionViewController : BaseController
    {
        #region Constants
        private const string COUNTRY = "COUNTRY";
        private const string VALIDATIONSTAGE = "INSTITUTIONS_STATUS";
        private const string TYPE = "INSTITUTION_TYPE";
        private const string COUNTRY_ASSOCIATED = "COUNTRY_ALL";

        //this constant is used for see only the institution stage
        private const string PARTIALSTAGE = "INST_";
        #endregion

        #region Fields
        private readonly ICatalogService _catalogService;
        private readonly IInstitutionService _institutionService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        #endregion

        #region Constructors
        public InstitutionViewController(ICatalogService catalogService, IInstitutionService institutionService)
        {
            _catalogService = catalogService;
            _institutionService = institutionService;

            _viewModelMapperHelper = new ViewModelMapperHelper(ViewBag, _institutionService, _catalogService);
        }
        #endregion

        //GET: AdministrationSecondPhase/InstitutionView/Institution
        public virtual ActionResult Institution()
        {
            ViewBag.AcronymFilter = null;
            ViewBag.CountryFilter = _viewModelMapperHelper.GetListMasterData(COUNTRY_ASSOCIATED);
            ViewBag.StatusFilter = _viewModelMapperHelper.GetListMasterData(VALIDATIONSTAGE);
            ViewBag.TypeFilter = _viewModelMapperHelper.GetListMasterData(TYPE);
            ViewBag.CountryCodeList = _viewModelMapperHelper.GetCodeListMasterDataCode(COUNTRY_ASSOCIATED);
            ViewBag.FiltAcronym = new List<SelectListItem>();
            ViewBag.Usuario = IDBContext.Current.UserName;

            ViewBag.IsMain = true;

            return View();
        }

        //GET: AdministrationSecondPhase/InstitutionView/InstitutionEdit
        public virtual ActionResult InstitutionEdit(int id = 0, string isAssigned = "NO")
        {
            var model = DataInstitution(id, isAssigned);
            return View(model);
        }

        //GET: AdministrationSecondPhase/InstitutionView/InstitutionEdit
        public virtual ActionResult InstitutionEditReload(int id = 0)
        {
            var model = DataInstitution(id, "NO");
            return PartialView("Partial/InstitutionEditPartial", model);
        }

        public virtual ActionResult SearchInstitutionFilter(int searchInstitutionNameId, string searchInstitutionAcronymName, int searchInstitutionCountryId, int searchInstitutionStatusId, int searchInstitutionTypeId, bool searchInstitutionOperationAssigned)
        {
            var response = _institutionService.GetInstitutionList(searchInstitutionNameId, searchInstitutionAcronymName, searchInstitutionCountryId, searchInstitutionStatusId, searchInstitutionTypeId, searchInstitutionOperationAssigned, IDBContext.Current.UserName);
            ViewBag.ActionTable = response.GetListInstitution ?? new List<TableInstitutionViewModel>();
            return PartialView("Partial/InstitutionTableFilter", response.GetListInstitution ?? new List<TableInstitutionViewModel>());
        }

        public virtual JsonResult GetInstitutionName(string filter)
        {
            return Json(_viewModelMapperHelper.GetInstitutionList(filter), JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult AcronymInstitutionFilter(string term, string searchInstitutionAcronymName)
        {
            return Json(_viewModelMapperHelper.GetInstitutionAcronymList(term.ToUpper(), searchInstitutionAcronymName), JsonRequestBehavior.AllowGet);
        }

        #region Permission
        private void InstitutionPermission()
        {
            ViewBag.Permission = true;
            ViewBag.InstitutionPermission = true;
        }
        #endregion

        private InstitutionEditViewModel DataInstitution(int institutionId, string isAssigned)
        {
            InstitutionPermission();
            var model = _viewModelMapperHelper.GetInstitutionEdit(institutionId).InstitutionEdit;

            // TODO: It's saving info in the ViewBag that already exist in the model.
            // Use the model directly.
            ViewBag.Mode = model.InstitutionId > 0 ? "EDIT" : "NEW";
            ViewBag.State = model.ValidationStage;
            ViewBag.EditablaInst = !model.IsInactive && isAssigned != "YES";
            ViewBag.ID = institutionId;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            ViewBag.CountryFilter = _viewModelMapperHelper
                .GetListMasterData(COUNTRY_ASSOCIATED, true);
            ViewBag.TypeFilter = _viewModelMapperHelper.GetListMasterData(TYPE, true);

            if (institutionId > 0 && model.ValidationStage == null)
            {
                model.ErrorMessage = Localization.GetText("Institution.Deleted");
            }

            return model;
        }
    }
}