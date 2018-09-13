using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.GlobalModule.ViewModels;
using IDB.MW.Domain.Session;
using IDB.MW.GenericWorkflow.Enums;
using IDB.MW.GenericWorkflow.Messages;

namespace IDB.Presentation.MVC4.Areas.Global
{
    public static class WorkflowsHelper
    {
        public static string GetWorkflowCreateUser(string workflowUserName)
        {
            if (!string.IsNullOrEmpty(workflowUserName))
            {
                return workflowUserName;
            }

            return IDBContext.Current.UserName;
        }

        public static string GetWorkflowCreateProfile(string defaultCreateProfile)
        {
            var currentRoles = IDBContext.Current.Roles;

            if (currentRoles.Any(x => x.Length < WorkflowConstants.ROLES_LENGTH))
            {
                return currentRoles.FirstOrDefault(x => x.Length < WorkflowConstants.ROLES_LENGTH);
            }

            return defaultCreateProfile;
        }

        public static WorkflowCreationRequest LoadWorkflowRequest(WorkflowViewModel workflowModel)
        {
            var workflowRequest = new WorkflowCreationRequest
            {
                CreateProfile = "GenericWorkflow",
            };

            workflowRequest.CreateUser = GetWorkflowCreateUser(workflowModel.UserName);
            workflowRequest.CreateProfile = GetWorkflowCreateProfile(workflowRequest.CreateProfile);
            workflowRequest.EntityId = workflowModel.EntityId;
            workflowRequest.EntityType = workflowModel.EntityType;
            workflowRequest.OperationNumber = workflowModel.OperationNumber;
            workflowRequest.WorkflowType = workflowModel.WorkflowCode;
            workflowRequest.ContractNumber = workflowModel.ContractNumber;

            var listAdditionalValidators = workflowModel.Validators.Where(a => a.Mandatory == false).Select(x => new { x.Role });

            if (listAdditionalValidators.Any())
            {
                workflowRequest.AdditionalValidators = new List<string>();
                foreach (var validator in listAdditionalValidators)
                {
                    workflowRequest.AdditionalValidators.Add(validator.Role);
                }
            }

            return workflowRequest;
        }
    }
}