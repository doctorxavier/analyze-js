using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Services.RuleEngService;
using IDB.MW.Application.AdministrationModule.ViewModels.RuleEngService;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class RuleEngViewController : BaseController
    {
        #region fields
        private readonly IRuleEngService _ruleEngService;
        private readonly ICatalogService _catalogService;
        #endregion

        public RuleEngViewController(IRuleEngService ruleEngService, ICatalogService catalogService)
        {
            _ruleEngService = ruleEngService;
            _catalogService = catalogService;
        }

        public virtual ActionResult SearchRulesGroup()
        {
            return SearchRules();
        }

        public virtual ActionResult SearchRules()
        {
            ViewBag.RuleGroupModule = _ruleEngService.GetItemsRuleEngModuleCode();
            ViewBag.Status = _ruleEngService.GetItemsStatus();
            return View();
        }

        public virtual JsonResult GetItems(int item, string type, int groupId = 0)
        {
            var response = new RuleEngGroupsSelectViewModel
            {
                GetItemsGroup = _ruleEngService.GetItemsGroup(item, type).GetRuleEngGroup
            };

            if (groupId == 0 && type == RulesEngAttributes.GROUP)
            {
                var firstRuleGroup = response.GetItemsGroup.FirstOrDefault();

                response.FirstRuleGroupId =
                    firstRuleGroup != null ?
                        int.Parse(firstRuleGroup.Value) :
                        0;

                response.GetItemsRuleEngClassification =
                    _ruleEngService.GetItemsGroup(
                        response.FirstRuleGroupId,
                        RulesEngAttributes.CLASSIFICATION).GetRuleEngGroup;

                var firstClassification = response.GetItemsRuleEngClassification.FirstOrDefault();

                response.FirstClassificationId =
                    firstClassification != null ?
                        int.Parse(firstClassification.Value) :
                        0;
            }
            else if (groupId == 0 && type == RulesEngAttributes.CLASSIFICATION)
            {
                var firstClassification = response.GetItemsGroup.FirstOrDefault();

                response.FirstClassificationId =
                    firstClassification != null ?
                        int.Parse(firstClassification.Value) :
                        0;
            }

            if (type == RulesEngAttributes.CLASSIFICATION && groupId != 0)
            {
                response.GetItemsRuleEngCode = _ruleEngService.GetItemsRuleEngCode(groupId);
            }

            return Json(new { IsValid = response.GetItemsGroup.Any(), Data = response });
        }

        public virtual JsonResult GetCodeAvailable(string code)
        {
            var response = _ruleEngService.IsAvailableCode(code);
            return Json(new { Data = response });
        }

        public virtual JsonResult GetCodeActual(int ruleEngId, string code)
        {
            var response = _ruleEngService.IsActualCode(ruleEngId, code);
            return Json(new { Data = response });
        }

        public virtual JsonResult GetBreakFirst(int classification)
        {
            var response = _ruleEngService.GetBreakFirst(classification);
            return Json(new { IsValid = response, Data = response });
        }

        public virtual JsonResult GetRuleEngItems(int item)
        {
            var response = _ruleEngService.GetItemsRuleEngItems(item);
            return Json(new { IsValid = true, Data = response });
        }

        public virtual JsonResult GetAttributeParameters(int item, string type)
        {
            var response = _ruleEngService.GetAttributeParameters(item, type);
            return Json(new { IsValid = true, Data = response });
        }

        public virtual JsonResult GetAttributeValues(string item, int type)
        {
            var response = _ruleEngService.GetAttributeValues(item, type);
            return Json(new { IsValid = response.Any(), Data = response });
        }

        public virtual ActionResult SearchRuleGroupFilter(
            int searchModule,
            int searchGroupRule,
            int searchGroupClasification,
            string searchRuleName,
            int searchRuleGroupStatus)
        {
            var response = _ruleEngService.GetRuleEngGroupSearch(
                searchModule,
                searchGroupRule,
                searchGroupClasification,
                searchRuleName,
                searchRuleGroupStatus);

            return PartialView("Partials/RuleGroupTableFilter", response.GetRuleEngGroup);
        }

        public virtual ActionResult RulesGroupCreateAndEdit(
            int searchModule,
            int searchGroupRule,
            int searchGroupClasification,
            string searchRuleName,
            int searchRuleGroupStatus,
            string searchClassificationCode)
        {
            ViewBag.fixLoad = true;
            ViewBag.RuleGroupModule = _ruleEngService.GetItemsRuleEngModuleCode();

            var response = new RuleEngGroupViewModel
            {
                Name = string.Empty,
                Classification = string.Empty,
                Code = string.Empty,
                ClassificationId = 0,
                IsActive = false,
                CodeId = 0,
                GroupRuleId = 0,
                Module = string.Empty,
                ModuleId = 0,
                RuleEngGroupId = 0,
                GroupRule = string.Empty,
                BreakFirst = false,
                RuleEngResultGroupClassificationId = 0,
                RuleEngResult = new List<RuleEngResultViewModel>(),
                RuleEng = new List<RuleEngViewModel>(),
                RuleEngResultGroupClassification = new List<RuleEngResultGroupClassificationViewModel>()
            };

            if (searchGroupRule != 0)
            {
                response = _ruleEngService
                    .GetRulesGroupCreateAndEdit(
                        searchGroupRule,
                        searchModule,
                        searchGroupClasification,
                        searchRuleName,
                        searchRuleGroupStatus,
                        searchClassificationCode)
                    .GetRuleEngGroup;
            }

            response.GroupRole =
                _ruleEngService.GetItemsGroup(
                    searchGroupRule,
                    RulesEngAttributes.RULE_GROUP).GetRuleEngGroup;
            response.GroupClassification =
                _ruleEngService.GetItemsGroup(
                    searchGroupRule,
                    RulesEngAttributes.CLASSIFICATION).GetRuleEngGroup;
            ViewBag.RuleEngCode = _ruleEngService.GetItemsRuleEngCode(searchGroupRule);
            ViewBag.ID = searchGroupRule;
            ViewBag.SerializedViewModel = PageSerializationHelper
                .SerializeObject(response);

            return PartialView("Partials/RulesGroupEdit", response);
        }

        public virtual ActionResult SearchRuleFilter(
            int searchModule,
            string searchRuleCode,
            string searchRuleName,
            int searchRuleStatus)
        {
            var response = _ruleEngService.GetRuleEngSearch(
                searchModule,
                searchRuleCode,
                searchRuleName,
                searchRuleStatus);

            return PartialView("Partials/RuleTableFilter", response.GetRuleEngGroup);
        }

        public virtual ActionResult ImportRule(int groupId)
        {
            ViewBag.RuleEngCode = _ruleEngService.GetItemsRuleEngCode(groupId);
            return PartialView("Partials/ImportRule");
        }

        public virtual JsonResult VariableName(
            string ruleEngElementName)
        {
            var response = _ruleEngService.VariableName(ruleEngElementName);
            return Json(new { IsValid = true, Data = response });
        }

        public virtual ActionResult TestRule(string code, string name)
        {
            var response =
                new RuleEngGroupViewModel
                {
                    Name = name,
                    Code = code
                };

            return PartialView("Partials/TestRule", response);
        }

        public virtual ActionResult RulesConditions(int ruleId)
        {
            var response = _ruleEngService.GetItemsImportRule(ruleId);
            return PartialView("Partials/ImportRuleConditionsTable", response.RuleEngCondition);
        }

        public virtual JsonResult DeleteRuleFilter(
            int ruleEngId)
        {
            var response = _ruleEngService.DelRuleEng(ruleEngId);
            return Json(response);
        }

        public virtual ActionResult RulesCreateAndEdit(
            int idRule = 0,
            string module = "",
            string classification = "")
        {
            var response = new RuleEngViewModel
            {
                Description = string.Empty,
                Classification = string.Empty,
                ClassificationId = 0,
                Code = string.Empty,
                CodeId = 0,
                GroupRule = string.Empty,
                IsActive = true,
                Module = string.Empty,
                ModuleId = 0,
                Name = string.Empty,
                Order = 0,
                Priority = 0,
                Result = string.Empty,
                RuleEngId = 0,
                RuleId = idRule,
                SerializedViewModel = string.Empty,
                RuleEngCondition = new List<RuleEngConditionViewModel>(),
                RuleEngResult = new List<RuleEngResultViewModel>()
            };

            if (idRule != 0)
            {
                response = _ruleEngService.GetRulesCreateAndEdit(idRule, module).GetRuleEng;
            }

            response.RuleGroupModule = _ruleEngService.GetRuleEngGroups().GetRuleEngGroup;
            response.Source =
                _ruleEngService.GetItemsGroup(0, RulesEngAttributes.SOURCE).GetRuleEngGroup;
            ViewBag.fixLoad = RulesEngAttributes.RESPONSE_TRUE;
            response.MaxOrder = idRule != 0 ? _ruleEngService.GetRuleId(idRule) : 1;
            response.SerializedViewModel = PageSerializationHelper
                .SerializeObject(response);

            return PartialView("Partials/RulesEdit", response);
        }

        public virtual ActionResult GetRowCondition(
            int idRule,
            string element,
            string settingName,
            string settingCode,
            int lastOrderValue)
        {
            var response = new RuleEngConditionViewModel
            {
                Description = settingName,
                DescriptionCode = settingCode,
                Element = element,
                GroupNumber = lastOrderValue,
                ConditionId = 0
            };

            return PartialView("Partials/ConditionsTableNewRow", response);
        }

        public virtual ActionResult GetRowGroup(
            int idRule,
            string code,
            int idRuleEng,
            string name,
            string description,
            int priority,
            bool isActive,
            string result)
        {
            var response = new RuleEngResultViewModel
            {
                Code = code,
                Name = name,
                Description = description,
                Priority = priority,
                IsActive = isActive,
                RuleEngId = idRuleEng,
                Result = result,
                RuleEngResultId = 0,
                IsAvailable = true
            };

            return PartialView("Partials/RuleTableNewRow", response);
        }

        public virtual ActionResult GetMasterData(int typeId)
        {
            var masterData = _catalogService.GetMasterDataListByTypeId(true, typeId);
            var model = masterData.IsValid ? masterData.MasterDataCollection : null;
            return PartialView("Partials/masterDataTable", model);
        }

        public virtual ActionResult ImportConditionsRules(
            int conditionId,
            int order,
            string element,
            string setting,
            string settingCode,
            bool active,
            int groupNumber,
            bool not)
        {
            var response = new RuleEngConditionViewModel
            {
                Description = setting,
                DescriptionCode = settingCode,
                Element = element,
                GroupNumber = groupNumber,
                ConditionId = 0,
                Order = order,
                IsActive = active,
                IsNot = not
            };

            return PartialView("Partials/ConditionsTableNewRowImported", response);
        }

        public virtual ActionResult LoadTestRules(
            int ruleId,
            string ruleName,
            string ruleCode,
            int numberRegister = 10,
            string operationNumber = "")
        {
            var response = _ruleEngService.RuleEngTestRules(
                    ruleId,
                    ruleName,
                    ruleCode,
                    operationNumber,
                    numberRegister);

            return PartialView("Partials/TestRuleNewRow", response);
        }
    }
}