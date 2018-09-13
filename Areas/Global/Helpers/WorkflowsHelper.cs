using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.GlobalModule.ViewModels;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.Session;
using IDB.MW.GenericWorkflow.Messages;

namespace IDB.Presentation.MVC4.Areas.Global.Helpers
{
    public static class WorkflowsHelper
    {
        public static WorkflowCreationRequest LoadClauseData(WorkflowCreationRequest workflowRequest,
            ClauseExtension clauseExtension)
        {
            if (clauseExtension != null)
            {
                var clauseNumber = clauseExtension.ClauseIndividual != null &&
                    clauseExtension.ClauseIndividual.Clause != null ?
                    clauseExtension.ClauseIndividual.Clause.ClauseNumber :
                    string.Empty;
                workflowRequest.Parameters.Add("clause-number", clauseNumber);
                workflowRequest.Parameters.Add("current-date", 
                    clauseExtension.Created.ToString());
                workflowRequest.Parameters.Add("idb-request",
                    clauseExtension.IdbRequest.ToString());
                workflowRequest.Parameters.Add("extension-date",
                    clauseExtension.RequestExtensionDate.ToString());
                workflowRequest.Parameters.Add("clause-description",
                    clauseExtension.Description);
            }

            return workflowRequest;
        }

        public static WorkflowCreationRequest InitialGenericWorkflowDataLoad(WorkflowViewModel workflowModel)
        {
            var workflowRequest = new WorkflowCreationRequest
            {
                Parameters = new Dictionary<string, string>(),
                AdditionalValidators = new List<string>()
            };

            workflowRequest.CreateUser = string.IsNullOrEmpty(workflowModel.UserName)
                ? IDBContext.Current.UserName : workflowModel.UserName;

            if (IDBContext.Current.Roles.Any(x => x.Length < 40))
            {
                workflowRequest.CreateProfile = IDBContext.Current.Roles
                        .FirstOrDefault(x => x.Length < 40);
            }

            var additionalValidators = workflowModel.Validators
                .Where(a => a.Mandatory == false)
                .Select(x => x.Role).ToList();

            workflowRequest.EntityId = workflowModel.EntityId;
            workflowRequest.EntityType = workflowModel.EntityType;
            workflowRequest.OperationNumber = workflowModel.OperationNumber;
            workflowRequest.WorkflowType = workflowModel.WorkflowCode;
            workflowRequest.ContractNumber = workflowModel.ContractNumber;
            workflowRequest.AdditionalValidators = additionalValidators;

            return workflowRequest;
        }
    }
}