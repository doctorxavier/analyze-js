using System;
using System.Linq;
using System.Collections.Generic;

using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Application.AdministrationModule.ViewModels.RuleEngService;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.RuleEng
{
    public static class ClientFieldDataMappers
    {
        #region Field Data
        private const string SEARCH_RULE_MODULE = "SearchRuleModule";
        private const string SEARCH_RULE_CODE = "SearchRuleCode";
        private const string SEARCH_RULE_NAME = "SearchRuleName";
        private const string SEARCH_RULE_DESCRIPTION = "SearchRuleDescription";
        private const string ACTIVE_RE = "activeRE";
        private const string CONDITION_ID_VALUE = "conditionIdValue";
        private const string RULE_ORDER = "RuleOrder";
        private const string ELEMENT_CONDITION = "elementCondition";
        private const string SETTING_CONDITION = "settingCondition";
        private const string SETTING_NAME_CONDITION = "settingNameCondition";
        private const string ACTIVE = "active";
        private const string RULE_GROUP_NUMBER = "RuleGoupNumber";
        private const string IS_NOT = "isNot";

        private const string SEARCH_RULE_GROUP_MODULE = "hidSearchRuleGroupModule";
        private const string SEARCH_RULE_GROUP_CODE = "SearchRuleGroupGroupRule";
        private const string SEARCH_RULE_GROUP_CLASSIFICATION = "SearchRuleGroupClasification";
        private const string BREAK_FIRST = "breakFirst";
        private const string RULE_ENG_ID = "ruleEngIdValue";
        private const string RULE_ENG_RESULT_ID = "ruleEngIdResultValue";
        private const string RULE_PRIORITY = "RulePriority";
        private const string CODE_ER = "codeER";
        private const string NAME_ER = "nameER";
        private const string DESCRIPTION_ER = "descriptionER";
        private const string RESULT = "result";
        private const string DELETED = "hidDelete";
        #endregion

        #region Mappers
        public static void UpdateRuleEngViewModel(
            this RuleEngViewModel viewModel, 
            ClientFieldData[] clientFieldData)
        {
            var searchModule = 
                clientFieldData.FirstOrDefault(o => 
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(SEARCH_RULE_MODULE));
            if (searchModule != null)
            {
                viewModel.ModuleId = Convert.ToInt32(searchModule.Value);
            }

            var ruleCode = 
                clientFieldData.FirstOrDefault(o => 
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(SEARCH_RULE_CODE));
            if (ruleCode != null)
            {
                viewModel.Code = ruleCode.Value;
            }

            var ruleName = 
                clientFieldData.FirstOrDefault(o => 
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(SEARCH_RULE_NAME));
            if (ruleName != null)
            {
                viewModel.Name = ruleName.Value;
            }

            var ruleDescription = 
                clientFieldData.FirstOrDefault(o => 
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(SEARCH_RULE_DESCRIPTION));
            if (ruleDescription != null)
            {
                viewModel.Description = ruleDescription.Value;
            }

            var isActive = clientFieldData.FirstOrDefault(o =>
                !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(ACTIVE_RE));

            if (isActive != null)
            {
                viewModel.IsActive = Convert.ToBoolean(isActive.Value);
            }

            var conditionIdValue = clientFieldData
                .Where(o => o.Name.Equals(CONDITION_ID_VALUE))
                .ToList();
            var ruleOrderIdValue = clientFieldData
                .Where(o => o.Name.Equals(RULE_ORDER))
                .ToList();
            var elementValue = clientFieldData
                .Where(o => o.Name.Equals(ELEMENT_CONDITION))
                .ToList();
            var settingValue = clientFieldData
                .Where(o => o.Name.Equals(SETTING_CONDITION))
                .ToList();
            var settingNameValue = clientFieldData
                .Where(o => o.Name.Equals(SETTING_NAME_CONDITION))
                .ToList();
            var activeValue = clientFieldData
                .Where(o => o.Name.Equals(ACTIVE))
                .ToList();
            var groupIdValue = clientFieldData
                .Where(o => o.Name.Equals(RULE_GROUP_NUMBER))
                .ToList();
            var isNotValue = clientFieldData
                .Where(o => o.Name.Equals(IS_NOT))
                .ToList();

            viewModel.RuleEngCondition = (
                from conditionId in conditionIdValue
                join ruleOrderId in ruleOrderIdValue
                    on conditionIdValue.IndexOf(conditionId) 
                    equals ruleOrderIdValue.IndexOf(ruleOrderId)
                join element in elementValue
                    on conditionIdValue.IndexOf(conditionId)
                    equals elementValue.IndexOf(element)
                join setting in settingValue
                    on conditionIdValue.IndexOf(conditionId)
                    equals settingValue.IndexOf(setting)
                join settingName in settingNameValue
                    on conditionIdValue.IndexOf(conditionId)
                    equals settingNameValue.IndexOf(settingName)
                join active in activeValue
                    on conditionIdValue.IndexOf(conditionId)
                    equals activeValue.IndexOf(active)
                join groupId in groupIdValue
                    on conditionIdValue.IndexOf(conditionId)
                    equals groupIdValue.IndexOf(groupId)
                join isNot in isNotValue
                    on conditionIdValue.IndexOf(conditionId)
                    equals isNotValue.IndexOf(isNot)
                select new RuleEngConditionViewModel
                {
                    ConditionId = int.Parse(conditionId.Value),
                    Order = int.Parse(ruleOrderId.Value),
                    Element = element.Value,
                    Value = setting.Value,
                    Description = settingName.Value,
                    IsActive = bool.Parse(active.Value),
                    GroupNumber = int.Parse(groupId.Value),
                    IsNot = bool.Parse(isNot.Value)
                }).ToList();
        }

        public static void UpdateRuleEngGroupViewModel(
            this RuleEngGroupViewModel viewModel, 
            ClientFieldData[] clientFieldData)
        {
            var searchModule =
                clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(SEARCH_RULE_GROUP_MODULE));
            if (searchModule != null)
            {
                viewModel.ModuleId = Convert.ToInt32(searchModule.Value);
            }

            var groupRule =
                clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals(SEARCH_RULE_GROUP_CODE));
            if (groupRule != null)
            {
                viewModel.GroupRuleId = Convert.ToInt32(groupRule.Value);
            }

            var classification =
                clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(SEARCH_RULE_GROUP_CLASSIFICATION));
            if (classification != null)
            {
                viewModel.RuleEngResultGroupClassificationId = Convert.ToInt32(classification.Value);
            }
            
            var breakFirst =
                clientFieldData.FirstOrDefault(o =>
                    !string.IsNullOrWhiteSpace(o.Name) &&
                    o.Name.Equals(BREAK_FIRST));
            if (breakFirst != null)
            {
                viewModel.BreakFirst = bool.Parse(breakFirst.Value);
            }

            var ruleEngIdValue = clientFieldData
                .Where(o => o.Name.Equals(RULE_ENG_ID))
                .ToList();
            var ruleEngResultIdValue = clientFieldData
                .Where(o => o.Name.Equals(RULE_ENG_RESULT_ID))
                .ToList();
            var ruleOrderIdValue = clientFieldData
                .Where(o => o.Name.Equals(RULE_ORDER))
                .ToList();
            var priorityValue = clientFieldData
                .Where(o => o.Name.Equals(RULE_PRIORITY))
                .ToList();
            var activeValue = clientFieldData
                .Where(o => o.Name.Equals(ACTIVE))
                .ToList();
            var resultValue = clientFieldData
                .Where(o => o.Name.Equals(RESULT))
                .ToList();
            var codeERValue = clientFieldData
                .Where(o => o.Name.Equals(CODE_ER))
                .ToList();
            var nameERValue = clientFieldData
                .Where(o => o.Name.Equals(NAME_ER))
                .ToList();
            var descriptionERValue = clientFieldData
                .Where(o => o.Name.Equals(DESCRIPTION_ER))
                .ToList();

            int reId;
            int roId;
            int prId;
            bool act;

            viewModel.RuleEngResult = (
                from ruleEngId in ruleEngIdValue
                join priority in priorityValue
                    on ruleEngIdValue.IndexOf(ruleEngId) equals priorityValue.IndexOf(priority)
                join ruleEngResultId in ruleEngResultIdValue
                    on ruleEngIdValue.IndexOf(ruleEngId)
                    equals ruleEngResultIdValue.IndexOf(ruleEngResultId)
                join ruleOrderId in ruleOrderIdValue
                    on ruleEngIdValue.IndexOf(ruleEngId)
                    equals ruleOrderIdValue.IndexOf(ruleOrderId)
                join active in activeValue
                    on ruleEngIdValue.IndexOf(ruleEngId)
                    equals activeValue.IndexOf(active)
                join result in resultValue
                    on ruleEngIdValue.IndexOf(ruleEngId)
                    equals resultValue.IndexOf(result)
                join codeER in codeERValue
                    on ruleEngIdValue.IndexOf(ruleEngId)
                    equals codeERValue.IndexOf(codeER)
                join nameER in nameERValue
                    on ruleEngIdValue.IndexOf(ruleEngId)
                    equals nameERValue.IndexOf(nameER)
                join descriptionER in descriptionERValue
                    on ruleEngIdValue.IndexOf(ruleEngId)
                    equals descriptionERValue.IndexOf(descriptionER)
                select new RuleEngResultViewModel
                {
                    RuleEngId = int.Parse(ruleEngId.Value),
                    RuleEngResultId = int.TryParse(ruleEngResultId.Value, out reId) ? reId : 0,
                    Order = int.TryParse(ruleOrderId.Value, out roId) ? roId : 0,
                    Priority = int.TryParse(priority.Value, out prId) ? prId : 0,
                    IsActive = bool.TryParse(active.Value, out act) ? act : false,
                    Result = result.Value,
                    Code = codeER.Value,
                    Name = nameER.Value,
                    Description = descriptionER.Value
                }).ToList();

            viewModel.Deleted = GetDeleted(clientFieldData);
        }

        public static IEnumerable<int> GetDeleted(ClientFieldData[] clientFieldData)
        {
            var delete = clientFieldData
                .FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && 
                    o.Name.Equals(DELETED));

            if (delete == null || string.IsNullOrEmpty(delete.Value))
                return Enumerable.Empty<int>();

            return delete.Value.Split(',').Select(int.Parse);
        }
        #endregion
    }
}