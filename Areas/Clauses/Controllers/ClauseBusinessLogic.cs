using System;
using System.Collections.Generic;
using System.Linq;

using IDB.Architecture.Logging;
using IDB.MW.Application.ClausesAndContracts.Services.RulesEngine;
using IDB.MW.Application.ClausesAndContractsModule.Services.Clauses;
using IDB.MW.Domain.Contracts.ModelRepositories.K2Service;
using IDB.MW.Domain.Models.Clauses;
using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Values;
using IDB.MW.Infrastructure.Data.Optima.Context;

namespace IDB.Presentation.MVC4.Areas.Clauses.Controllers
{
    public class ClauseBusinessLogic
    {
        public const string CLAUSES_EXTENSION_NUMBER = "3";

        private const string CLAUSES_WORKFLOW_CODE_PREFFIX = "WF-CL-00";

        private readonly IClauseBusinessLogicRuleService _businessRuleService;
        private readonly IClauseService _clausesService;

        public ClauseBusinessLogic(
            IClauseBusinessLogicRuleService businessRuleService,
            IClauseService clauseServ)
        {
            _businessRuleService = businessRuleService;
            _clausesService = clauseServ;
        }

        public void SetValidatorsToStartWorkflow(
            string userProfileValidators,
            string additionalValidators,
            OperationRelatedModelWorkflow callInstance)
        {
            if (string.IsNullOrEmpty(userProfileValidators) &&
                additionalValidators.Contains("-mandatory"))
            {
                additionalValidators = additionalValidators.Replace("-mandatory", string.Empty);
                additionalValidators = additionalValidators.TrimEnd('|');
                callInstance.UserProfileAdditional = string.Empty;

                if (string.IsNullOrEmpty(additionalValidators))
                {
                    throw new Exception(
                        "Mandatory validator is required in order to start this workflow");
                }

                callInstance.UserProfileValidators = additionalValidators;
            }
            else if (string.IsNullOrEmpty(userProfileValidators))
            {
                throw new Exception(
                    "Mandatory validator is required in order to start this workflow");
            }
            else
            {
                callInstance.UserProfileValidators = userProfileValidators;
                callInstance.UserProfileAdditional = additionalValidators;
            }
        }

        public string GetUserProfileValidators(
            IRuleEvaluatorService ruleService,
            OperationRelatedModel modelValidation,
            string code,
            int operationId,
            int contractId,
            string mainOperationNumber,
            int clauseId,
            int clauseIndividualId,
            string clauseExtensionId,
            decimal amount)
        {
            string validator = RunNewRulesEngine(
                modelValidation,
                code,
                clauseExtensionId,
                contractId,
                amount);

            if (!string.IsNullOrEmpty(validator))
                return validator;

            return RunOldRulesEngine(
                ruleService,
                code,
                operationId,
                contractId,
                mainOperationNumber,
                clauseId,
                clauseIndividualId,
                clauseExtensionId,
                amount);
        }

        public void GetValidatorAndPopupMessage(
            IRuleEvaluatorService ruleService,
            OperationRelatedModel modelValidation,
            string code,
            int operationId,
            int contractId,
            string mainOperationNumber,
            int clauseId,
            int clauseIndividualId,
            string clauseExtensionId,
            decimal amount,
            out string mandatoryValidator,
            out string popupMessage)
        {
            mandatoryValidator = GetUserProfileValidators(
                ruleService,
                modelValidation,
                code,
                operationId,
                contractId,
                mainOperationNumber,
                clauseId,
                clauseIndividualId,
                clauseExtensionId,
                amount);

            popupMessage = _businessRuleService.GetPopupMessage();

            Logger.GetLogger()
                .WriteDebug(
                    "ClauseBusinessLogic",
                    string.Format(
                        "Validator is: {0} and popup message is: {1}",
                        mandatoryValidator, 
                        popupMessage));
        }

        string RunNewRulesEngine(
            OperationRelatedModel modelValidation,
            string code,
            string clauseExtensionId,
            int contractId,
            decimal amount)
        {
            string validatorCandidates = string.Empty;
            string workflowType = CLAUSES_WORKFLOW_CODE_PREFFIX + code;

            if (string.IsNullOrEmpty(clauseExtensionId))
                clauseExtensionId = "0";

            _businessRuleService.SetUpRulesContext(
                modelValidation, amount, workflowType, int.Parse(clauseExtensionId), contractId);

            if (workflowType == ClauseConstants.EXTENSION_WORKFLOW_TYPE ||
                workflowType == ClauseConstants.FINAL_STATUS_WORKFLOW_TYPE)
            {
                string validator = null;
                var normative = _clausesService
                    .GetNormative(modelValidation.OperationNumber, amount);

                var category = _clausesService.GetClauseCategory(modelValidation);

                if (!category.IsValid)
                {
                    var ex = new Exception(category.ErrorMessage);

                    Logger.GetLogger().WriteError(
                        "ClauseBusinessLogic",
                        "Category id is not valid",
                        ex);
                }
                    
                if (category.Model.Code == null)
                {
                    var ex = new ArgumentException("Cannot find Category code");

                    Logger.GetLogger().WriteError(
                        "ClauseBusinessLogic",
                        "Category code is null",
                        ex);
                }
                    
                if (normative == ClauseNormative.OA_422 &&
                    _businessRuleService.CheckApplyOA422NormativeValidatorRule(ref validator) &&
                    category.Model.Code != ClauseConstants.LAST_DISBURMENT)
                {
                    return validator;
                }
            }

            if (ClauseConstants.EXTENSION_WORKFLOW_TYPE == workflowType)
                return CheckExtensionValidatorRules(clauseExtensionId, validatorCandidates);

            if (ClauseConstants.ClauseTypeCL01 == workflowType)
                return CheckRevolvingFundValidatorRules();

            if (ClauseConstants.ClauseTypeCL02 == workflowType)
                return CheckElegibilityValidatorRules();

            if (ClauseConstants.FINAL_STATUS_WORKFLOW_TYPE == workflowType)
            {
                string validator = null;

                if (_businessRuleService
                    .CheckApplyTCPForConditionExtensionValidatorRule(ref validator))
                {
                    return validator;
                }

                _businessRuleService.CheckApplyTCPExByValidatorRule(ref validator);

                return validator;
            }

            return null;
        }

        string RunOldRulesEngine(
            IRuleEvaluatorService ruleService,
            string code,
            int operationId,
            int contractId,
            string mainOperationNumber,
            int clauseId,
            int clauseIndividualId,
            string clauseExtensionId,
            decimal amount)
        {
            Dictionary<string, string> variablesList = new Dictionary<string, string>();
            variablesList.Add("OperationId", operationId.ToString());
            variablesList.Add("ContractId", contractId.ToString());
            variablesList.Add("MainOperationNumber", mainOperationNumber);

            using (var dbOptimaContainer = new OptimaContainer())
            {
                variablesList.Add("ValidationStageAprobId", dbOptimaContainer.ConvergenceMasterData
                    .Where(a => a.Code == ClauseConstants.APPROVED_CLAUSE_STATUS).ToString());
            }

            variablesList.Add("ClauseId", clauseId.ToString());
            variablesList.Add("ClauseIndividualId", clauseIndividualId.ToString());
            variablesList.Add("ClauseExtensionId", clauseExtensionId);
            variablesList.Add("Role", string.Empty);
            variablesList.Add("Temp", string.Empty);

            string validatorCandidates = ruleService.EvaluateST(int.Parse(code), variablesList, amount);

            if (!validatorCandidates.Contains("Error"))
                validatorCandidates = validatorCandidates.Replace("\"", string.Empty);

            return validatorCandidates;
        }

        string CheckExtensionValidatorRules(string clauseExtensionId, string validatorCandidates)
        {
            if (string.IsNullOrEmpty(clauseExtensionId))
                return null;

            if (_businessRuleService
                .CheckApplyOA421LastDClauseExtensionValidator(ref validatorCandidates))
            {
                return validatorCandidates;
            }

            if (_businessRuleService.CheckApplyOA420LastDClauseExtensionValidator(
                ref validatorCandidates))
            {
                return validatorCandidates;
            }

            if (_businessRuleService.CheckApplyOA422LastDClauseExtensionValidator(
                ref validatorCandidates))
            {
                return validatorCandidates;
            }

            if (_businessRuleService.CheckApplySpecialExtensionValidatorRule(
                ref validatorCandidates))
            {
                return validatorCandidates;
            }

            if (_businessRuleService.CheckApplyExtensionOtherPValidatorRule(ref validatorCandidates))
            {
                return validatorCandidates;
            }

            ////You should check if the preceding rule is equivalent to it or if it overlaps. 
            if (_businessRuleService.CheckApplyExtensionValidatorRule(ref validatorCandidates))
            {
                return validatorCandidates;
            }

            return null;
        }

        string CheckRevolvingFundValidatorRules()
        {
            string rolSelect = string.Empty;

            if (_businessRuleService.CheckApplyRevolvingFundValidator(ref rolSelect))
            {
                return rolSelect;
            }

            return null;
        }

        string CheckElegibilityValidatorRules()
        {
            string validatorRole = null;

            if (_businessRuleService.CheckApplyElegibilityValidatorRule(ref validatorRole))
                return validatorRole;

            return null;
        }
    }
}