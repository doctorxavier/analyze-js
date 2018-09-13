using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Services.MasterDataService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Attribute;
using IDB.MW.Application.AttributesModule.Services;
using IDB.MW.Application.AttributesModule.Services.ConvergencePermissionServices;
using IDB.MW.Application.AttributesModule.ViewModels.AttributesService;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class AttributeViewController : BaseController
    {
        #region constants
        private const string REFERENCE = "";
        private const string EDIT = "EDIT";
        private const string NEW = "NEW";
        #endregion

        #region Fields
        private readonly ICatalogService _catalogService;
        private readonly IAttributesManagementService _attributesManagementService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly IMasterDataService _masterDataService;
        private readonly IConvergencePermissionService _convergencePermissionService;
        private readonly IBreRuleService _breRuleService;
        private readonly IAttributesEngineService _attributesEngineService;
        #endregion

        public AttributeViewController(ICatalogService catalogService,
            IAttributesManagementService attributesManagementService,
            IMasterDataService masterDataService,
            IConvergencePermissionService convergencePermissionService,
            IBreRuleService breRuleService,
            IAttributesEngineService attributesEngineService)
        {
            _masterDataService = masterDataService;
            _catalogService = catalogService;
            _attributesManagementService = attributesManagementService;
            _convergencePermissionService = convergencePermissionService;
            _breRuleService = breRuleService;
            _viewModelMapperHelper = new ViewModelMapperHelper(ViewBag, _attributesManagementService, _catalogService, _masterDataService, _convergencePermissionService, _breRuleService);
            _attributesEngineService = attributesEngineService;
        }

        public virtual ActionResult AttributeList()
        {
            ViewBag.OperationTypeFilter = _catalogService.GetListMultiMasterData(MasterType.OPERATION_TYPE, true);
            ViewBag.RelationshipTypeFilter = _catalogService.GetListMultiMasterData(MasterType.RELATIONSHIP_TYPE, true);
            ViewBag.SectionFilter = _catalogService.GetListMasterData(MasterType.ATTR_SECTIONS);
            ViewBag.ExpirationDate = _catalogService.GetListMasterData(MasterType.EXPIRATION_DATE);
            ViewBag.AttributeFilter = new List<SelectListItem>();
            ViewBag.IsMain = true;

            return View();
        }

        public virtual ActionResult AttributeEdit(int id = 0)
        {
            var model = DataAttribute(id);
            return View(model);
        }

        public virtual ActionResult AttributeEditReload(int id = 0)
        {
            var model = DataAttribute(id);
            return PartialView("Partials/FormAttributeEdit", model);
        }

        public virtual JsonResult GetAttributeName(string filter)
        {
            return Json(_viewModelMapperHelper.GetAttributeList(filter), JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult GetReferenceList(string filter)
        {
            return Json(_viewModelMapperHelper.GetMasterTypes(filter), JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult SearchAttributeFilter(int searchAttributeId, int searchOperationType, int searchRelationshipType, int searchSection, bool searchExpirationDate)
        {
            var response = _attributesManagementService.GetAttributeList(searchAttributeId, searchOperationType, searchRelationshipType, searchSection, searchExpirationDate);
            ViewBag.ActionTable = response.GetAttributeList ?? new List<RowSearchAttributeViewModel>();

            return PartialView("Partials/TableAttributeList", response.GetAttributeList ?? new List<RowSearchAttributeViewModel>());
        }
        
        #region LayoutEditor

        public virtual ActionResult AttributeLayoutEditorByCode(string operationTypeCode, string relationshipCode, string sectionCode)
        {
            var response = _attributesEngineService
                .GetFormLayout(operationTypeCode, relationshipCode, sectionCode, true);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(response.AttributeForm);
            return View("AttributeLayoutEditor", response.AttributeForm);
        }

        public virtual ActionResult AttributeLayoutEditor(int operationTypeId, int? relationshipId, int sectionId)
        {
            var response = _attributesEngineService
                .GetFormLayout(operationTypeId, relationshipId, sectionId, true);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(response.AttributeForm);
            return View(response.AttributeForm);
        }

        public virtual ActionResult AttributeLayoutEditorEdit(int operationTypeId, int? relationshipId, int sectionId)
        {
            var response = _attributesEngineService
                .GetFormLayout(operationTypeId, relationshipId, sectionId, true);

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(response.AttributeForm);
            return PartialView("Partials/_AttributeLayoutEditorPartial", response.AttributeForm);
        }
        #endregion

        #region LayoutList
        public virtual ActionResult AttributeLayoutList()
        {
            ViewBag.OperationTypeFilter = _catalogService.GetListMasterData(MasterType.OPERATION_TYPE, true);
            ViewBag.RelationshipTypeFilter = _catalogService.GetListMasterData(MasterType.RELATIONSHIP_TYPE, true);
            ViewBag.SectionFilter = _catalogService.GetListMasterData(MasterType.ATTR_SECTIONS);
            ViewBag.AttributeFilter = new List<SelectListItem>();
            ViewBag.IsMain = true;

            return View();
        }

        public virtual ActionResult NewAttributeLayoutModal()
        {
            ViewBag.OperationTypeFilter = _catalogService.GetListMasterData(MasterType.OPERATION_TYPE, true);
            ViewBag.RelationshipTypeFilter = _catalogService.GetListMasterData(MasterType.RELATIONSHIP_TYPE, true);
            ViewBag.SectionFilter = _catalogService.GetListMasterData(MasterType.ATTR_SECTIONS);
            ViewBag.AttributeFilter = new List<SelectListItem>();
            ViewBag.IsMain = true;

            return PartialView("Partials/_NewAttributeLayoutListPartials");
        }

        public virtual ActionResult SearchAttributeLayoutFilter(int searchAttributeId, int searchOperationType, int searchRelationshipType, int searchSection)
        {
            var response = _attributesManagementService.GetAttributeLayoutList(searchAttributeId, searchOperationType, searchRelationshipType, searchSection);
            ViewBag.ActionTable = response.GetAttributeLayoutList ?? new List<RowSearchAttributeLayoutViewModel>();

            return PartialView("Partials/TableAttributeLayoutList", response.GetAttributeLayoutList ?? new List<RowSearchAttributeLayoutViewModel>());
        }

        #endregion

        private void AttributePermission()
        {
            ViewBag.Permission = true;
            ViewBag.AttributePermission = true;
        }

        private AttributeEditViewModel DataAttribute(int id)
        {
            AttributePermission();
            var model = _viewModelMapperHelper.GetAttributeEdit(id).AttributeEdit;
            ViewBag.Mode = model.AttributeId > 0 ? EDIT : NEW;
            ViewBag.ID = id;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            var sectionList = _catalogService.GetListMultiMasterData(MasterType.ATTR_SECTIONS);
            var operationList = _catalogService.GetListMultiMasterData(MasterType.OPERATION_TYPE, onlyCode: true);
            var relationshipList = _catalogService.GetListMultiMasterData(MasterType.RELATIONSHIP_TYPE, onlyCode: true);
            if (model.AttributeId > 0)
            {
                ViewBag.Section = sectionList
                    .Where(s => model.SectionListName.Contains(s.Value))
                    .Select(s => s.Text)
                    .ToList();

                ViewBag.SectionMulti = sectionList
                    .Select(s => new MultiSelectListItem
                    {
                        Text = s.Text,
                        Value = s.Value,
                        Selected = model.Section.Contains(int.Parse(s.Value))
                    }).ToList();
                ViewBag.OperationType = operationList.Where(op => model.OperationListName.Contains(op.Value)).Select(op => op.Text).ToList();
                ViewBag.OperationTypeMulti = operationList
                    .Select(op => new MultiSelectListItem
                    {
                        Text = op.Text,
                        Value = op.Value,
                        Selected = model.OperationType.Contains(int.Parse(op.Value))
                    }).ToList();
                ViewBag.RelationshipType = relationshipList.Where(rt => model.RelationListName.Contains(rt.Value)).Select(rt => rt.Text).ToList();
                ViewBag.RelationshipTypeMulti = relationshipList
                    .Select(rt => new MultiSelectListItem
                    {
                        Text = rt.Text,
                        Value = rt.Value,
                        Selected = model.RelationshipType.Contains(int.Parse(rt.Value))
                    })
                    .ToList();
            }
            else
            {
                ViewBag.Section = sectionList;
                ViewBag.SectionMulti = sectionList;
                ViewBag.OperationType = operationList;
                ViewBag.OperationTypeMulti = operationList;
                ViewBag.RelationshipType = relationshipList;
                ViewBag.RelationshipTypeMulti = relationshipList;
            }

            var types = _catalogService.GetListMasterData(MasterType.ATTR_CONTROL_TYPES);

            var textArea = _catalogService.GetConvergenceMasterDataIdByCode(
                MasterData.ATTR_TEXTAREA, MasterType.ATTR_CONTROL_TYPES);
            ViewBag.IdTextarea = textArea.IsValid ? textArea.Id.ToString() : string.Empty;

            var textBox = _catalogService.GetConvergenceMasterDataIdByCode(
                MasterData.ATTR_TEXTBOX, MasterType.ATTR_CONTROL_TYPES);
            ViewBag.IdTextbox = textBox.IsValid ? textBox.Id.ToString() : string.Empty;

            ViewBag.Type = types;
            ViewBag.Length = _viewModelMapperHelper.GetLenghtList();
            ViewBag.VisibilityBr = _viewModelMapperHelper.GetBreRuleList();

            var validations = _catalogService.GetListMasterData(MasterType.ATTR_VALIDATION_TYPES);

            var businessRule = _catalogService.GetConvergenceMasterDataIdByCode(
                MasterData.ATTR_BUSINESS_RULE, MasterType.ATTR_VALIDATION_TYPES);
            ViewBag.IdBusinessRule = businessRule.IsValid ? businessRule.Id.ToString() : string.Empty;

            ViewBag.Validation = validations;
            ViewBag.Order = _viewModelMapperHelper.GetColumnSize(1, 99);
            ViewBag.ColumnSize = _viewModelMapperHelper.GetColumnSize(1, 4);
            ViewBag.ValidationBrName = _viewModelMapperHelper.GetBreRuleList();
            ViewBag.ReferenceList = _viewModelMapperHelper.GetMasterTypes(REFERENCE).ListResponse;
            ViewBag.EffectiveDate = DateTime.Now;

            return model;
        }
    }
}