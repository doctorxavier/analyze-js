using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Domain.Contracts.ModelRepositories.K2Service;
using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Models.Clauses;
using IDB.MW.Application.ClausesAndContracts.Services.RulesEngine;
using IDB.MW.Infrastructure.Data.Optima.Context;

namespace IDB.Presentation.MVC4.Areas.Agreements.Controllers
{
    public class ConditionBusinessLogic
    {
        private const string CLAUSES_WORKFLOW_CODE_PREFFIX = "WF-CL-00";

        private readonly IClauseBusinessLogicRuleService _businessRuleService;

        public ConditionBusinessLogic(IClauseBusinessLogicRuleService businessRuleService)
        {
            _businessRuleService = businessRuleService;
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

            if (validator != null)
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

            if (ClauseConstants.EXTENSION_WORKFLOW_TYPE == workflowType)
            {
                return CheckExtensionValidatorRules(clauseExtensionId, validatorCandidates);
            }

            if (ClauseConstants.ClauseTypeCL01 == workflowType)
            {
                return CheckRevolvingFundValidatorRules();
            }

            if (ClauseConstants.FINAL_STATUS_WORKFLOW_TYPE == workflowType)
            {
                return CheckTCPExByValidatorRule();
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

            if (_businessRuleService.CheckApplyOA421LastDClauseExtensionValidator(ref validatorCandidates))
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

        string CheckTCPExByValidatorRule()
        {
            string rolSelect = string.Empty;
            if (_businessRuleService.CheckApplyTCPExByValidatorRule(ref rolSelect))
            {
                return rolSelect;
            }

            return null;
        }
    }
}