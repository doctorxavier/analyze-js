using System;
using System.Collections.Generic;

using IDB.Architecture.Logging;
using IDB.MW.Domain.Models.K2Service;
using IDB.MW.Domain.Contracts.ModelRepositories.K2Service;
using IDB.MW.Domain.Models.Architecture.Enumerations;

namespace IDB.Presentation.MVC4.Areas.PMI
{
    public static class PMIWorkflowManager
    {
        //The RULE_ID in the RULE table (I know, I know; this sucks...)
        private const int PMI_VALIDATORS_NOT_SEPTEMBER_RULE_ID = 5;
        private const int PMI_VALIDATORS_SEPTEMBER_RULE_ID = 7;
        private const string VALIDATOR_ERROR_MESSAGE =
            "PMI: Mandatory validator is required in order to start this workflow";

        public static string GetValidators(
            IRuleEvaluatorService ruleService, decimal currentAmount, K2CallType callType)
        {
            Dictionary<string, string> variablesList = new Dictionary<string, string>();
            variablesList.Add("Role", string.Empty);

            string calculatedValidators = ruleService.EvaluateST(
                callType == K2CallType.September ?
                    PMI_VALIDATORS_SEPTEMBER_RULE_ID :
                    PMI_VALIDATORS_NOT_SEPTEMBER_RULE_ID,
                variablesList,
                currentAmount);

            Logger.GetLogger().WriteDebug("PMIWorkflowValidationManager",
                "Validators calculated: " + calculatedValidators);

            if (!calculatedValidators.Contains("Error"))
            {
                return calculatedValidators.Replace("\"", string.Empty);
            }

            return string.Empty;
        }

        public static void SetValidatorsToStartWorkflow(
            string userProfileValidators,
            string additionalValidators,
            PMIWorkflow workflow)
        {
            if (string.IsNullOrEmpty(userProfileValidators) &&
                additionalValidators.Contains("-mandatory"))
            {
                additionalValidators = additionalValidators.Replace("-mandatory", string.Empty);
                additionalValidators = additionalValidators.TrimEnd('|');
                workflow.UserProfileAdditional = string.Empty;

                if (string.IsNullOrEmpty(additionalValidators))
                {
                    throw new Exception(VALIDATOR_ERROR_MESSAGE);
                }

                workflow.UserProfileValidators = additionalValidators;
            }
            else if (string.IsNullOrEmpty(userProfileValidators))
            {
                throw new Exception(VALIDATOR_ERROR_MESSAGE);
            }
            else
            {
                workflow.UserProfileValidators = userProfileValidators;
                workflow.UserProfileAdditional = additionalValidators;
            }
        }

        public static string BeginK2Workflow(
            K2CallType callTypeEnum,
            PMIWorkflow workflow,
            IK2ServiceProxy proxy)
        {
            try
            {
                return proxy.BeginWorkflow(callTypeEnum, workflow);
            }
            catch (Exception ex)
            {
                Logger.GetLogger().WriteError("PMIWorkflowManager", "ERROR WHEN STARTING WORKFLOW: ", ex);
                throw;
            }
        }
    }
}