using System;
using System.Linq;
using System.Collections.Generic;

using IDB.MW.Application.ESW_CIPModule.ViewModels;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Application.Core.ViewModels.Common;
using IDB.MW.Application.ESW_CIPModule.ViewModels.K2Workflow;
using IDB.MW.Application.ESW_CIPModule.Enums;

namespace IDB.Presentation.MVC4.Areas.ESW_CIP.Models
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateCreationWorkFlowViewModel(this WorkflowViewModels viewModel, ClientFieldData[] clientFieldData)
        {
            if (viewModel.UserComments == null)
            {
                viewModel.UserComments = new List<CommentsViewModel>();
            }

            var editComments = clientFieldData.Where(o => o.Name.Equals("textComment"));
            var editCommentsId = clientFieldData.Where(o => o.Name.Equals("commentId"));
            var documentDescription = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentDescription"));
            var documentNumber = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentNumber")); 
            viewModel.UserComments = mapperEditComments(viewModel.UserComments, editComments, editCommentsId);

            var newComments = clientFieldData.Where(o => o.Name.Equals("newComment"));
            viewModel.UserComments = mapperNewComment(viewModel.UserComments, newComments);

            //Add
            var documentIndex = 0;

            foreach (var document in documentDescription)
            {
                var documentNumberValue = documentNumber.ToArray()[documentIndex].Value;

                viewModel.Documents.Add(
                    new DocumentViewModel
                    {
                        Description = document.Value,
                        DocNumber = documentNumberValue
                    });

                documentIndex++;
            }
        }

        public static void UpdateAnnualAllocationViewModel(this AnnualAllocationContainerViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var annualAllocationId = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("AnnualAllocationID")).ToList();
            var operationNumbers = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("OperationNumber")).ToList();
            var requestProposal = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("RequestProposal")).ToList();
            var studiesCommitee = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("StudiesCommite")).ToList();
            var approval = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("Approval")).ToList();
            var dismiss = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("Dismiss")).ToList();
            var allocatedAmount = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("AllocatedAmount")).ToList();
            var revisedAmount = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("RevisedAmount")).ToList();
            var actions = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("Actions")).ToList();
            var proposalStatusCode = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("ProposalStatusCode")).ToList(); 
            var hasChanges = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("HasChanges")).ToList(); 

            if (viewModel == null)
                viewModel = new AnnualAllocationContainerViewModel(); 

            for (int i = 0; i < annualAllocationId.Count(); i++)
            {
                var existingAnnualAllocation = viewModel.AnnualAllocations.Any(x => x.AnnualAllocationID.Equals(Convert.ToInt32(annualAllocationId.ElementAt(i).Value)));

                if (Convert.ToInt32(annualAllocationId.ElementAt(i).Value).Equals(0) || !existingAnnualAllocation)
                {
                    var annualAllocation = new AnnualAllocationViewModel
                    {
                        AnnualAllocationID = Convert.ToInt32(annualAllocationId.ElementAt(i).Value),
                        OperationNumber = operationNumbers.ElementAt(i).Value,
                        RequestProposalCreation = Convert.ToBoolean(requestProposal.ElementAt(i).Value),
                        StudiesCommittee = viewModel.OperationModality.Equals("ESW") ? Convert.ToBoolean(studiesCommitee.ElementAt(i).Value) : false,
                        Approval = Convert.ToBoolean(approval.ElementAt(i).Value),
                        Dismiss = Convert.ToBoolean(dismiss.ElementAt(i).Value),
                        AllocatedAmount = allocatedAmount.ElementAt(i).Value,
                        RevisedAmount = revisedAmount.ElementAt(i).Value,
                        Actions = actions.ElementAt(i).Value.Split('|').Where(x => !string.IsNullOrEmpty(x)).ToList(),
                        ProposalStatusCode = proposalStatusCode.ElementAt(i).Value,
                        HasChanges = Convert.ToBoolean(hasChanges.ElementAt(i).Value)
                    };

                    viewModel.AnnualAllocations.Add(annualAllocation);
                }
                else
                {
                    var index = viewModel.AnnualAllocations.FindIndex(x => x.AnnualAllocationID == Convert.ToInt32(annualAllocationId.ElementAt(i).Value));

                    viewModel.AnnualAllocations.ElementAt(index).RequestProposalCreation = Convert.ToBoolean(requestProposal.ElementAt(i).Value);
                    viewModel.AnnualAllocations.ElementAt(index).StudiesCommittee = viewModel.OperationModality.Equals("ESW") ? Convert.ToBoolean(studiesCommitee.ElementAt(i).Value) : false;
                    viewModel.AnnualAllocations.ElementAt(index).Approval = Convert.ToBoolean(approval.ElementAt(i).Value);
                    viewModel.AnnualAllocations.ElementAt(index).Dismiss = Convert.ToBoolean(dismiss.ElementAt(i).Value);
                    viewModel.AnnualAllocations.ElementAt(index).AllocatedAmount = allocatedAmount.ElementAt(i).Value;
                    viewModel.AnnualAllocations.ElementAt(index).RevisedAmount = revisedAmount.ElementAt(i).Value;
                    viewModel.AnnualAllocations.ElementAt(index).Actions = actions.ElementAt(i).Value.Split('|').Where(x => !string.IsNullOrEmpty(x)).ToList();
                    viewModel.AnnualAllocations.ElementAt(index).HasChanges = Convert.ToBoolean(hasChanges.ElementAt(i).Value);
                    viewModel.AnnualAllocations.ElementAt(index).ProposalStatusCode = proposalStatusCode.ElementAt(i).Value;
                }
            } 
        }

        public static void UpdateProposalOperationViewModel(this ProposalViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var readyForSubmission = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("ReadyForSubmission")).Value;
            var subtitleProposal = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("ProposalSubtitle")).Value;
            var orgUnitJointProposals = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("OrgUnitJointProposal")).ToList();
            var beneficiaryCountries = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("countriesToBeSaved")).Value;
            var deliverableId = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("DeliverableId")).ToList();
            var type = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("Type")).ToList();
            var name = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("Name")).ToList();
            var plannedDate = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("PlannedDate")).ToList();           
            var budgetId = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("BudgetId")).ToList();
            var activities = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("Activity")).ToList();
            var consultations = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("Consultation")).ToList();
            var travels = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("Travel")).ToList();
            var others = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("Other")).ToList();
            var otherFinancings = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Equals("OtherFinancing")).ToList();
            var requestedAmount = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("RequestedAmount")).Value;

            var objectives = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextObjectives")).Value;
            var mainOutputs = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextMainOutputs")).Value;

            string mainQuestions = null;
            string earlierLiteratureQuestions = null;
            string methodologicalAproach = null;
            string strategyAndActivities = null;

            //string nameOfPeerReview = null;
            //string additionalTechnicalInformation = null;
            string reportDeliverables = null;

            if (viewModel.ProposalType.Equals(ESWCIPEnums.TYPE_ESW))
            {
                mainQuestions = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextMainQuestions")).Value;
                earlierLiteratureQuestions = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextEarlierLiteratureQuestions")).Value;
                methodologicalAproach = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextMethodologicalApproach")).Value;
                strategyAndActivities = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextStrategyAndActivities")).Value;

                //nameOfPeerReview = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextNameOfPeerReview")).Value;
                //additionalTechnicalInformation = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextAdditionalTechnicalInformation")).Value;
            }

            if (viewModel.ProposalType.Equals(ESWCIPEnums.TYPE_CIP))
            {
                reportDeliverables = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextReportDeliverables")).Value;
            }

            var risks = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextRisksContent")).Value;
            var coordinationMdbs = clientFieldData.First(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("inputTextSummaryOfCollaboration")).Value;
            var documentDescription = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentDescription"));
            var documentNumber = clientFieldData.Where(x => !string.IsNullOrWhiteSpace(x.Name) && x.Name.Contains("documentNumber")); 

            viewModel.IsReadyForSubmission = Convert.ToBoolean(readyForSubmission);

            viewModel.GeneralInformation.SubtitleOfProposal = subtitleProposal;

            var alreadyExistingOrganizationalUnits = viewModel.GeneralInformation.OrganizationalUnit.Select(x => x.UnitId.ToString()).ToList();
            var alreadyExistingBeneficiaryCountries = viewModel.GeneralInformation.BeneficiaryCountries.Select(x => x.CountryId.ToString()).ToList();

            if (viewModel.GeneralInformation.NewOrganizationalUnits == null)
                viewModel.GeneralInformation.NewOrganizationalUnits = new List<int>();

            if (viewModel.GeneralInformation.DeletedOrganizationalUnits == null)
                viewModel.GeneralInformation.DeletedOrganizationalUnits = new List<int>();

            orgUnitJointProposals = orgUnitJointProposals.Where(x => !string.IsNullOrWhiteSpace(x.Value)).ToList();

            foreach (var unit in orgUnitJointProposals)
            {
                if (!alreadyExistingOrganizationalUnits.Contains(unit.Value))
                {
                    viewModel.GeneralInformation.NewOrganizationalUnits.Add(Convert.ToInt32(unit.Value));
                }
            }

            foreach (var eou in alreadyExistingOrganizationalUnits)
            {
                if (!orgUnitJointProposals.Select(x => x.Value).ToList().Contains(eou))
                    viewModel.GeneralInformation.DeletedOrganizationalUnits.Add(Convert.ToInt32(eou));
                viewModel.GeneralInformation.OrganizationalUnit.RemoveAll(bc => bc.UnitId.Equals(Convert.ToInt32(eou)));
            }

            var beneficiaryCountriesSave = beneficiaryCountries.Split('|').Where(x => !string.IsNullOrEmpty(x)).Distinct().ToArray();

            if (viewModel.GeneralInformation.NewBeneficiaryCountries == null)
                viewModel.GeneralInformation.NewBeneficiaryCountries = new List<int>();

            if (viewModel.GeneralInformation.DeletedBeneficiaryCountries == null)
                viewModel.GeneralInformation.DeletedBeneficiaryCountries = new List<int>();

            foreach (var bc in beneficiaryCountriesSave)
            {
                if (!alreadyExistingBeneficiaryCountries.Contains(bc))
                    viewModel.GeneralInformation.NewBeneficiaryCountries.Add(Convert.ToInt32(bc));
            }

            foreach (var bcd in alreadyExistingBeneficiaryCountries)
            {
                if (!beneficiaryCountriesSave.Contains(bcd))
                    viewModel.GeneralInformation.DeletedBeneficiaryCountries.Add(Convert.ToInt32(bcd));
                viewModel.GeneralInformation.BeneficiaryCountries.RemoveAll(bc => bc.CountryId.Equals(Convert.ToInt32(bcd)));
            } 

            viewModel.Oera.Objectives = objectives;
            viewModel.Oera.MainOutputs = mainOutputs;
            viewModel.Oera.ReportDeliverables = reportDeliverables;
            viewModel.Oera.MainQuestions = mainQuestions;
            viewModel.Oera.EarlierLiteratureQuestions = earlierLiteratureQuestions;
            viewModel.Oera.MethodologicalApproach = methodologicalAproach;
            viewModel.Oera.StrategyAndActivities = strategyAndActivities;

            viewModel.Risks.ImplementationRisks = risks;
            viewModel.CoordinationMDBs.SummaryOfCollaborationText = coordinationMdbs;

            for (int i = 0; i < deliverableId.Count(); i++)
            {
                var uType = type.ElementAt(i).Value;
                var uName = name.ElementAt(i).Value;
                var uPlannedDate = plannedDate.ElementAt(i).Value;

                if (Convert.ToInt32(deliverableId.ElementAt(i).Value).Equals(0))
                {
                    var deliverable = new DeliverablesViewModel
                    {
                        DeliverableId = Convert.ToInt32(deliverableId.ElementAt(i).Value),
                        Type = uType,
                        Name = uName,
                        PlannedDate = Convert.ToDateTime(uPlannedDate)
                    };

                    viewModel.Deliverables.Add(deliverable);
                }
                else
                {
                    var index = viewModel.Deliverables.FindIndex(x => x.DeliverableId == Convert.ToInt32(deliverableId.ElementAt(i).Value));

                    viewModel.Deliverables.ElementAt(index).Type = uType;
                    viewModel.Deliverables.ElementAt(index).Name = uName;
                    viewModel.Deliverables.ElementAt(index).PlannedDate = Convert.ToDateTime(uPlannedDate);
                }
            } 

            for (int i = 0; i < budgetId.Count(); i++)
            {
                var act = activities.ElementAt(i).Value;
                var con = !string.IsNullOrEmpty(consultations.ElementAt(i).Value) ? (decimal?)Convert.ToDecimal(consultations.ElementAt(i).Value) : null;
                var tra = !string.IsNullOrEmpty(travels.ElementAt(i).Value) ? (decimal?)Convert.ToDecimal(travels.ElementAt(i).Value) : null;
                var oth = !string.IsNullOrEmpty(others.ElementAt(i).Value) ? (decimal?)Convert.ToDecimal(others.ElementAt(i).Value) : null;
                var ofi = !string.IsNullOrEmpty(otherFinancings.ElementAt(i).Value) ? (decimal?)Convert.ToDecimal(otherFinancings.ElementAt(i).Value) : null;

                if (Convert.ToInt32(budgetId.ElementAt(i).Value).Equals(0))
                {
                    var budget = new BudgetViewModel
                    {
                        BudgetId = Convert.ToInt32(budgetId.ElementAt(i).Value),
                        Activity = act,
                        Consultation = con,
                        Travel = tra,
                        Other = oth,
                        OtherFinancing = ofi
                    };

                    viewModel.Budgets.Add(budget);
                }
                else
                {
                    var index = viewModel.Budgets.FindIndex(x => x.BudgetId == Convert.ToInt32(budgetId.ElementAt(i).Value));

                    viewModel.Budgets.ElementAt(index).Activity = act;
                    viewModel.Budgets.ElementAt(index).Consultation = con;
                    viewModel.Budgets.ElementAt(index).Travel = tra;
                    viewModel.Budgets.ElementAt(index).Other = oth;
                    viewModel.Budgets.ElementAt(index).OtherFinancing = ofi;
                }
            }

            viewModel.RequestedAmount = !string.IsNullOrEmpty(requestedAmount) ? (decimal?)Convert.ToDecimal(requestedAmount) : null;

            var deleteBudgets = clientFieldData.FirstOrDefault(o => o.Name.Equals("deleteBudgets"));

            if (deleteBudgets == null) return;

            var deleteB = deleteBudgets.Value.Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();

            if (viewModel.DeleteBudgets == null)
                viewModel.DeleteBudgets = new List<int>();

            foreach (var s in deleteB)
            {
                viewModel.DeleteBudgets.Add(Convert.ToInt32(s));
                viewModel.Budgets.RemoveAll(b => b.BudgetId.Equals(Convert.ToInt32(s)));
            }

            var deleteDeliverables = clientFieldData.FirstOrDefault(o => o.Name.Equals("deleteDeliverables"));

            if (deleteDeliverables == null) return;

            var deleteD = deleteDeliverables.Value.Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();

            if (viewModel.DeleteDeliverables == null)
                viewModel.DeleteDeliverables = new List<int>();

            foreach (var s in deleteD)
            {
                viewModel.DeleteDeliverables.Add(Convert.ToInt32(s));
                viewModel.Deliverables.RemoveAll(b => b.DeliverableId.Equals(Convert.ToInt32(s)));
            } 

            //Add
            var documentIndex = 0;

            foreach (var document in documentDescription)
            {
                var documentNumberValue = documentNumber.ToArray()[documentIndex].Value;

                viewModel.Oera.Documents.Add(
                    new DocumentViewModel
                    {
                        Description = document.Value,
                        DocNumber = documentNumberValue
                    });

                documentIndex++;
            }
        }

        private static List<CommentsViewModel> mapperEditComments(List<CommentsViewModel> userComments, IEnumerable<ClientFieldData> editComments, IEnumerable<ClientFieldData> editCommentsId)
        {
            if (editComments != null && editCommentsId != null)
            {
                for (int i = 0; i < editComments.Count(); i++)
                {
                    var index = userComments.FindIndex(x => x.CommentId == Convert.ToInt32(editCommentsId.ElementAt(i).Value));
                    if (!userComments.ElementAt(index).Comment.Equals(editComments.ElementAt(i).Value.Trim()))
                    {
                        userComments.ElementAt(index).Comment = editComments.ElementAt(i).Value.Trim();
                    }
                }
            }

            return userComments;
        }

        private static List<CommentsViewModel> mapperNewComment(List<CommentsViewModel> userComments, IEnumerable<ClientFieldData> newComments)
        {
            if (newComments != null)
            {
                for (int i = 0; i < newComments.Count(); i++)
                {
                    var comment = new CommentsViewModel();
                    comment.Comment = newComments.ElementAt(i).Value.Trim();

                    userComments.Add(comment);
                }
            }

            return userComments;
        }
    }
}