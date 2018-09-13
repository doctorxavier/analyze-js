using System;
using System.Collections.Generic;
using System.Linq;

using Castle.Core.Internal;

using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.TCM.ViewModels.K2Workflow;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Application.TCM.ViewModels.FindingAndRecommendation;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Components;
using IDB.MW.Application.TCM.ViewModels.ResultsMatrix.Outcomes;
using IDB.MW.Domain.Values;
using IDB.MW.Domain.Models.FindingRecomendations;
using IDB.MW.Domain.Session;
using IDB.Architecture.Logging;
using IDB.Architecture.Extensions;
using IDB.Presentation.MVC4.Areas.TCM.Values;

namespace IDB.Presentation.MVC4.Areas.TCM.Models
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateCreationWorkFlowViewModel(this WorkflowViewModels viewModel, ClientFieldData[] clientFieldData)
        {
            if (viewModel.UserComments == null)
            {
                viewModel.UserComments = new List<UserCommentViewModel>();
            }

            var editComments = clientFieldData.Where(o => o.Name.Equals("textComment"));
            var editCommentsId = clientFieldData.Where(o => o.Name.Equals("commentId"));
            viewModel.UserComments = mapperEditComments(viewModel.UserComments, editComments, editCommentsId);

            var newComments = clientFieldData.Where(o => o.Name.Equals("newComment"));
            viewModel.UserComments = mapperNewComment(viewModel.UserComments, newComments);

            var deleteComments = clientFieldData.FirstOrDefault(o => o.Name.Equals("deleteComments"));
            if (deleteComments != null)
            {
                string[] deleteC = deleteComments.Value.ToString().Split('|').Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (viewModel.DeleteComments == null)
                {
                    viewModel.DeleteComments = new List<int>();
                }

                foreach (string s in deleteC)
                {
                    viewModel.DeleteComments.Add(Convert.ToInt32(s));
                    viewModel.UserComments.RemoveAll(d => d.CommentId.Equals(Convert.ToInt32(s)));
                }
            }
        }

        #region Partner&Consultancies
        public static void UpdatePartnerAndConsultanciesViewModel(
            this PartnersAndConsultanciesModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var partnerId = clientFieldData.Where(o => o.Name.Equals("partnerId")).ToList();
            var typePartner = clientFieldData.Where(o => o.Name.Equals("typePartner")).ToList();
            var namePartner = clientFieldData.Where(o => o.Name.Equals("partnerName")).ToList();
            var engagementPartner = clientFieldData
                .Where(o => o.Name.Equals("partnerEngagement"))
                .ToList();

            viewModel.Partners = 
                (from idPartner in partnerId
                join partnerType in typePartner on partnerId.IndexOf(idPartner)
                equals typePartner.IndexOf(partnerType)
                join partnerName in namePartner on partnerId.IndexOf(idPartner)
                equals namePartner.IndexOf(partnerName)
                join partnerEngagement in engagementPartner on partnerId.IndexOf(idPartner)
                equals engagementPartner.IndexOf(partnerEngagement)
                select new PartnersTableModel
                {
                    PartnerId = int.Parse(idPartner.Value),
                    TypeOfPartner = int.Parse(partnerType.Value),
                    PartnerName = partnerName.Value,
                    PartnerEngagement = partnerEngagement.Value
                }).ToList();

            var consultanciesId = clientFieldData.Where(o => o.Name.Equals("consultanciesId"))
                .ToList();
            var consultanciesApprovalNumber = clientFieldData
                .Where(o => o.Name.Equals("consultanciesApprovalNumber")).ToList();
            var consultanciesName = clientFieldData
                .Where(o => o.Name.Equals("consultanciesName")).ToList();
            var consultanciesType = clientFieldData
                .Where(o => o.Name.Equals("consultanciesType")).ToList();
            var consultanciesNationality = clientFieldData
                .Where(o => o.Name.Equals("consultanciesNationality")).ToList();
            var consultanciesContractBegin = clientFieldData
                .Where(o => o.Name.Equals("consultanciesContractBegin")).ToList();
            var consultanciesContractEnd = clientFieldData
                .Where(o => o.Name.Equals("consultanciesContractEnd")).ToList();
            var consultanciesAmount = clientFieldData
                .Where(o => o.Name.Equals("consultanciesAmountUSD")).ToList();
            var consultanciesObjectives = clientFieldData
                .Where(o => o.Name.Equals("objectivesConsultancy")).ToList();
            var consultanciesQuality = clientFieldData
                .Where(o => o.Name.Equals("consultanciesQualityAssesment")).ToList();

            viewModel.Consultainces = (from idConsultancies in consultanciesId
                join approvalNumbers in consultanciesApprovalNumber 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesApprovalNumber.IndexOf(approvalNumbers)
                join nameConsultancies in consultanciesName 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesName.IndexOf(nameConsultancies)
                join typeConsultancies in consultanciesType 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesType.IndexOf(typeConsultancies)
                join nationalityConsultancies in consultanciesNationality 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesNationality.IndexOf(nationalityConsultancies)
                join contractBeginConsultancies in consultanciesContractBegin 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesContractBegin.IndexOf(contractBeginConsultancies)
                join contractEndConsultancies in consultanciesContractEnd 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesContractEnd.IndexOf(contractEndConsultancies)
                join amountConsultancies in consultanciesAmount 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesAmount.IndexOf(amountConsultancies)
                join objectivesConsultancites in consultanciesObjectives 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesObjectives.IndexOf(objectivesConsultancites)
                join qualityConsultancies in consultanciesQuality 
                    on consultanciesId.IndexOf(idConsultancies)
                    equals consultanciesQuality.IndexOf(qualityConsultancies)
                select new ConsultaincesTableModel
                {
                    ConsultaincesId = int.Parse(idConsultancies.Value),
                    Name = nameConsultancies.Value,
                    ApprovalNumber = approvalNumbers.Value,
                    Type = typeConsultancies.Value != "0" && 
                        !typeConsultancies.Value.IsNullOrEmpty() ? 
                            int.Parse(typeConsultancies.Value) : 
                            (int?)null,
                    NationalityId = nationalityConsultancies.Value != "0" && 
                        !nationalityConsultancies.Value.IsNullOrEmpty() ? 
                            int.Parse(nationalityConsultancies.Value) : 
                            (int?)null,
                    ContractBegin = contractBeginConsultancies.Value != string.Empty ? 
                        Convert.ToDateTime(contractBeginConsultancies.Value) : 
                        (DateTime?)null,
                    ContractEnd = contractEndConsultancies.Value != string.Empty ? 
                        Convert.ToDateTime(contractEndConsultancies.Value) : 
                        (DateTime?)null,
                    AmountUsd = decimal.Parse(amountConsultancies.Value),
                    ObjectiveOfConsultancy = objectivesConsultancites.Value.IsNullOrEmpty() || 
                        objectivesConsultancites.Value == " " ? 
                            string.Empty : 
                            objectivesConsultancites.Value,
                    QualityAssesMent = qualityConsultancies.Value != "0" && 
                        !qualityConsultancies.Value.IsNullOrEmpty() ? 
                            int.Parse(qualityConsultancies.Value) : 
                            (int?)null
                }).ToList();

            viewModel.ExecutingAgencies.Institutions = UpdateExecutingAgencies(clientFieldData);
        }

        #endregion

        #region Progress
        public static void UpdateProgressViewModel(this ProgressModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var outputId = clientFieldData.Where(o => o.Name.Equals("outputId")).ToList();
            var outputCommentId = clientFieldData.Where(o => o.Name.Equals("commenOutputId")).ToList();
            var outputComent = clientFieldData.Where(o => o.Name.Equals("AdvancesInExecution")).ToList();
            viewModel.OutputData = (from idOutputComment in outputCommentId
                                    join commentOutput in outputComent on outputCommentId.IndexOf(idOutputComment)
                                        equals outputComent.IndexOf(commentOutput)
                                    join idOutput in outputId on outputCommentId.IndexOf(idOutputComment)
                                        equals outputId.IndexOf(idOutput)
                                    select new OutPutDataTableModel
                                    {
                                        OutputId = int.Parse(idOutput.Value),
                                        AdvancesCommentId = int.Parse(idOutputComment.Value),
                                        AdvancesInExecution = commentOutput.Value
                                    }).ToList();
            var outcomeId = clientFieldData.Where(o => o.Name.Equals("outcomeId")).ToList();
            var outcomeCommnetId = clientFieldData.Where(o => o.Name.Equals("commentOutcomeId")).ToList();
            var outcomeComment = clientFieldData.Where(o => o.Name.Equals("AdvancesInTheAchievement")).ToList();
            viewModel.OutComeStatement = (from idOutcomeComment in outcomeCommnetId
                                          join commentOutcome in outcomeComment on outcomeCommnetId.IndexOf(idOutcomeComment)
                                              equals outcomeComment.IndexOf(commentOutcome)
                                          join idOutcome in outcomeId on outcomeCommnetId.IndexOf(idOutcomeComment)
                                              equals outcomeId.IndexOf(idOutcome)
                                          select new OutComeStatementModel
                                          {
                                              OutcomeId = int.Parse(idOutcome.Value),
                                              CommentOutcomeId = int.Parse(idOutcomeComment.Value),
                                              AdvancesInTheAchievement = commentOutcome.Value
                                          }).ToList();

            var disseminatedId =
                clientFieldData.FirstOrDefault(
                    o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("operationDisseminatedId"));
            if (disseminatedId != null)
            {
                viewModel.OperationDisseminatedId = Convert.ToInt32(disseminatedId.Value);
            }

            var disseminatedcheck =
                clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("disseminated"));
            if (disseminatedcheck != null)
            {
                viewModel.OperationDisseminated = disseminatedcheck.Value == "Yes";
            }

            var disseminatedComment =
                clientFieldData.FirstOrDefault(
                    o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("disseminatedComment"));
            if (disseminatedComment != null)
            {
                viewModel.OperationDisseminatedcomment = !string.IsNullOrEmpty(disseminatedComment.Value)
                    ? disseminatedComment.Value
                    : string.Empty;
            }

            var documentIdList = clientFieldData.Where(o => o.Name.Equals("DocumentId")).ToList();
            var docNumberList = clientFieldData.Where(o => o.Name.Equals("DocNumber")).ToList();
            var descriptionList = clientFieldData.Where(o => o.Name.Equals("Description")).ToList();

            viewModel.Documents = (from documentId in documentIdList
                                   join docNumber in docNumberList on documentIdList.IndexOf(documentId) equals
                                       docNumberList.IndexOf(docNumber)
                                   join description in descriptionList on documentIdList.IndexOf(documentId) equals
                                       descriptionList.IndexOf(description)
                                   select new DocumentsTableModel
                                   {
                                       DocumentId = int.Parse(documentId.Value),
                                       IdbDocNumber = docNumber.Value,
                                       Description = description.Value
                                   }).ToList();
        }
        #endregion

        #region ProjectManagement

        public static void UpdateProjectManagementViewModel(this ProjectManagementModel viewModel, ClientFieldData[] clientFieldData)
        {
            var frId = clientFieldData.Where(o => o.Name.Equals("findingRecommendationId")).ToList();
            var stageId = clientFieldData.Where(o => o.Name.Equals("stage")).ToList();
            var dimensionId = clientFieldData.Where(o => o.Name.Equals("dimension")).ToList();
            var finding = clientFieldData.Where(o => o.Name.Equals("finding")).ToList();
            var recommendation = clientFieldData.Where(o => o.Name.Equals("recommendation")).ToList();

            viewModel.TableProjectManagement = (from idfindingRecommendation in frId
                                                join stage in stageId on frId.IndexOf(idfindingRecommendation)
                                           equals stageId.IndexOf(stage)
                                                join dimension in dimensionId on frId.IndexOf(idfindingRecommendation)
                                           equals dimensionId.IndexOf(dimension)
                                                join commentFinding in finding on frId.IndexOf(idfindingRecommendation)
                                           equals finding.IndexOf(commentFinding)
                                                join commentRecommendation in recommendation on frId.IndexOf(idfindingRecommendation)
                                           equals recommendation.IndexOf(commentRecommendation)
                                                select new ProjectManagementTableModel
                                                {
                                                    FindingRecommendationId = int.Parse(idfindingRecommendation.Value),
                                                    DimensionId = int.Parse(dimension.Value),
                                                    StageId = int.Parse(stage.Value),
                                                    Findings = commentFinding.Value,
                                                    Recommendations = commentRecommendation.Value,
                                                    LastUpdate = DateTime.Now
                                                }).ToList();
        }

        #endregion

        #region SustainabilityAndInnovation

        public static void UpdateSustainabilityAndInnovationViewModel(this SustainabilityAndInnovationModel viewModel,
            ClientFieldData[] clientFieldData)
        {
            var sustainabilityBool = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Sustainability"));
            if (sustainabilityBool != null)
            {
                viewModel.OperationSustainable = sustainabilityBool.Value == "Yes";
            }

            var sustainabilityAreas = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("sustainabilityArea"));
            if (sustainabilityAreas != null)
            {
                viewModel.ExplainProjectSustainable = !string.IsNullOrEmpty(sustainabilityAreas.Value) ? sustainabilityAreas.Value : string.Empty;
            }

            var innovationBool = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Innovation"));
            if (innovationBool != null)
            {
                viewModel.InnovationBit = innovationBool.Value == "Yes";
            }

            var innovationId = clientFieldData.Where(o => o.Name.Equals("innovationId")).ToList();
            var typeInnovation = clientFieldData.Where(o => o.Name.Equals("typeInnovation")).ToList();
            var keyInnovation = clientFieldData.Where(o => o.Name.Equals("keyInnovation")).ToList();
            var extentSuccess = clientFieldData.Where(o => o.Name.Equals("extentSuccess")).ToList();
            viewModel.Innovation = (from idInnovation in innovationId
                                    join innovationType in typeInnovation on innovationId.IndexOf(idInnovation)
                                    equals typeInnovation.IndexOf(innovationType)
                                    join innovationKey in keyInnovation on innovationId.IndexOf(idInnovation)
                                    equals keyInnovation.IndexOf(innovationKey)
                                    join successExtent in extentSuccess on innovationId.IndexOf(idInnovation)
                                    equals extentSuccess.IndexOf(successExtent)
                                    select new TableInnovationModel
                                    {
                                        InnovationId = int.Parse(idInnovation.Value),
                                        TypeOfInnovation = int.Parse(innovationType.Value),
                                        KeyInnovation = innovationKey.Value,
                                        ExentOfSuccesText = successExtent.Value
                                    }).ToList();
        }
        #endregion

        #region StoriesFromField

        public static void UpdateStoriesFromFieldViewModel(this StoriesFromTheFieldModel viewModel,
            ClientFieldData[] clientField)
        {
            var storiesId = clientField.Where(o => o.Name.Equals("storiesId")).ToList();
            var typeStory = clientField.Where(o => o.Name.Equals("typeStory")).ToList();
            var electronicLink = clientField.Where(o => o.Name.Equals("electronicLink")).ToList();
            var commentStories = clientField.Where(o => o.Name.Equals("storiesArea")).ToList();
            viewModel.StoriesFromTheField = (from idStory in storiesId
                                             join storyType in typeStory on storiesId.IndexOf(idStory)
                                             equals typeStory.IndexOf(storyType)
                                             join linkElectronic in electronicLink on storiesId.IndexOf(idStory)
                                             equals electronicLink.IndexOf(linkElectronic)
                                             join storiesComment in commentStories on storiesId.IndexOf(idStory)
                                             equals commentStories.IndexOf(storiesComment)
                                             select new StoriesTableModel
                                             {
                                                 StoriesId = int.Parse(idStory.Value),
                                                 TypeOfStory = int.Parse(storyType.Value),
                                                 ElectronicLink = linkElectronic.Value.IsNullOrEmpty() || linkElectronic.Value == " " ? " " : linkElectronic.Value,
                                                 StoriesArea = storiesComment.Value
                                             }).ToList();
        }
        #endregion

        #region FinancialProgress

        public static List<OutputYearPlanViewModel> UpdateOutputYearPlanViewModel(
           ClientFieldData[] clientFieldData, FinancialProgressViewModel viewModel)
        {
            var allField = clientFieldData.Where(o => o.Name.Equals("ipdOriginalPlan") || o.Name.Equals("ipdAnnualPlan") || o.Name.Equals("ipdActualValue")).ToList();
            var allTotalOriginalPlan = clientFieldData.Where(o => o.Name.Equals("ipdTotalOriginalPlan")).ToList();
            var allTotalAnnualPlan = clientFieldData.Where(o => o.Name.Equals("ipdTotalAnnualPlan")).ToList();
            var allTotalActualPlan = clientFieldData.Where(o => o.Name.Equals("ipdTotalActualPlan")).ToList();
            var allOutput = allTotalOriginalPlan.Select(o => o.Id.Split(Convert.ToChar("|"))[0]).ToList();
            var allYear = allField.Select(o => o.Id.Split(Convert.ToChar("|"))[2]).Distinct();

            var allOutputYearPlan = new List<OutputYearPlanViewModel>();

            foreach (var output in allOutput)
            {
                foreach (var year in allYear)
                {
                    var outputYearPlanId =
                        allField.Where(
                            o =>
                                o.Id.Split(Convert.ToChar("|"))[0] == output &&
                                o.Id.Split(Convert.ToChar("|"))[2] == year && o.Name.Equals("ipdOriginalPlan"))
                            .Select(o => o.Id.Split(Convert.ToChar("|"))[1]).FirstOrDefault();

                    var originalPlan =
                        allField.Where(
                            o =>
                                o.Id.Split(Convert.ToChar("|"))[0] == output &&
                                o.Id.Split(Convert.ToChar("|"))[2] == year && o.Name.Equals("ipdOriginalPlan"))
                            .Select(o => o.Value).FirstOrDefault();

                    var annualPlan =
                        allField.Where(
                            o =>
                                o.Id.Split(Convert.ToChar("|"))[0] == output &&
                                o.Id.Split(Convert.ToChar("|"))[2] == year && o.Name.Equals("ipdAnnualPlan"))
                            .Select(o => o.Value).FirstOrDefault();

                    var actualValue =
                        allField.Where(
                            o =>
                                o.Id.Split(Convert.ToChar("|"))[0] == output &&
                                o.Id.Split(Convert.ToChar("|"))[2] == year && o.Name.Equals("ipdActualValue"))
                            .Select(o => o.Value)
                            .FirstOrDefault();

                    var yearPlan = new OutputYearPlanViewModel
                    {
                        OutputYearPlanId = outputYearPlanId.IsNullOrEmpty() ? 0 : int.Parse(outputYearPlanId),
                        OutputId = int.Parse(output),
                        Year = int.Parse(year),
                        OriginalPlan = originalPlan.IsNullOrEmpty() ? (decimal?)null : decimal.Parse(originalPlan),
                        AnnualPlan = annualPlan.IsNullOrEmpty() ? (decimal?)null : decimal.Parse(annualPlan),
                        ActualValue = actualValue.IsNullOrEmpty() ? (decimal?)null : decimal.Parse(actualValue),
                        IsCost = true
                    };

                    allOutputYearPlan.Add(yearPlan);
                }

                var totalOutputYearPlanId = allTotalOriginalPlan.Where(o => o.Id.Split(Convert.ToChar("|"))[0] == output).Select(o => o.Id.Split(Convert.ToChar("|"))[1]).FirstOrDefault();
                var totalOriginalPlan = allTotalOriginalPlan.Where(o => o.Id.Split(Convert.ToChar("|"))[0] == output).Select(o => o.Value).FirstOrDefault();
                var totalAnnualPlan = allTotalAnnualPlan.Where(o => o.Id.Split(Convert.ToChar("|"))[0] == output).Select(o => o.Value).FirstOrDefault();
                var totalActualPlan = allTotalActualPlan.Where(o => o.Id.Split(Convert.ToChar("|"))[0] == output).Select(o => o.Value).FirstOrDefault();

                var yearPlanTotal = new OutputYearPlanViewModel
                {
                    OutputYearPlanId = totalOutputYearPlanId.IsNullOrEmpty() ? 0 : int.Parse(totalOutputYearPlanId),
                    OutputId = int.Parse(output),
                    Year = TCMGlobalValues.END_OF_PROJECT_YEAR,
                    OriginalPlan = totalOriginalPlan.IsNullOrEmpty() ? (decimal?)null : decimal.Parse(totalOriginalPlan),
                    AnnualPlan = totalAnnualPlan.IsNullOrEmpty() ? (decimal?)null : decimal.Parse(totalAnnualPlan),
                    ActualValue = totalActualPlan.IsNullOrEmpty() ? (decimal?)null : decimal.Parse(totalActualPlan),
                    IsCost = true
                };
                allOutputYearPlan.Add(yearPlanTotal);
            }

            var ouputYearPlanViewModel =
                viewModel.Component.SelectMany(x => x.Outputs).SelectMany(x => x.OutputYearPlans).ToList();
            var endOfProjectViewModel = viewModel.Component.SelectMany(x => x.Outputs).Select(x => x.EndOfProject);
            ouputYearPlanViewModel.AddRange(endOfProjectViewModel);
            var outputYearPlanList = new List<OutputYearPlanViewModel>();
            foreach (var outputYearPlan in allOutputYearPlan)
            {
                if (outputYearPlan.OutputYearPlanId != 0)
                {
                    var outputYearPlanExist =
                        ouputYearPlanViewModel.FirstOrDefault(
                            x =>
                                x.OutputYearPlanId == outputYearPlan.OutputYearPlanId &&
                                 x.Year == outputYearPlan.Year);

                    if (outputYearPlanExist != null)
                    {
                        var update = new OutputYearPlanViewModel
                        {
                            OutputYearPlanId = outputYearPlanExist.OutputYearPlanId,
                            OutputId = outputYearPlan.OutputId,
                            Year = outputYearPlanExist.Year,
                            OriginalPlan = outputYearPlan.OriginalPlan,
                            AnnualPlan = outputYearPlan.AnnualPlan,
                            ActualValue = outputYearPlan.ActualValue,
                            IsCost = outputYearPlanExist.IsCost
                        };
                        outputYearPlanList.Add(update);
                    }
                }
                else
                {
                    var insert = new OutputYearPlanViewModel
                    {
                        OutputYearPlanId = outputYearPlan.OutputYearPlanId,
                        OutputId = outputYearPlan.OutputId,
                        Year = outputYearPlan.Year,
                        OriginalPlan = outputYearPlan.OriginalPlan,
                        AnnualPlan = outputYearPlan.AnnualPlan,
                        ActualValue = outputYearPlan.ActualValue,
                        IsCost = outputYearPlan.IsCost
                    };
                    outputYearPlanList.Add(insert);
                }
            }

            return outputYearPlanList;
        }

        public static List<OtherCostViewModel> UpdateOtherCostViewModel(ClientFieldData[] clientFieldData, FinancialProgressViewModel viewModel)
        {
            var allOtherCostDefinition =
                clientFieldData.Where(o => o.Name.Equals("otherCostDefinition")).ToList();

            var definitionAll = allOtherCostDefinition.Select(o => o.Value).ToList();

            var otherCostIdAll = allOtherCostDefinition.Select(o => o.Id.Split(Convert.ToChar("|"))[0]).ToList();

            var allField = clientFieldData.Where(o => o.Name.Equals("ipdOriginalPlanOtherCost")).ToList();

            var allYear =
                allField.Where(o => o.Id.Split(Convert.ToChar("|"))[2] != TCMGlobalValues.END_OF_PROJECT_YEAR.ToString())
                    .Select(o => o.Id.Split(Convert.ToChar("|"))[2])
                    .ToList()
                    .Distinct();

            var allOriginalPlanOtherCost =
                clientFieldData.Where(o => o.Name.Equals("ipdOriginalPlanOtherCost")).ToList();

            var allAnnualPlanOtherCost = clientFieldData.Where(o => o.Name.Equals("ipdAnnualPlanOtherCost")).ToList();

            var allActualValueOtherCost = clientFieldData.Where(o => o.Name.Equals("ipdActualPlanOtherCost")).ToList();

            var allOtherCostTotalOriginalPlan = clientFieldData.Where(o => o.Name.Equals("TotalOriginalPlanOtherCost")).ToList();

            var allOtherCostTotalAnnualPlan = clientFieldData.Where(o => o.Name.Equals("TotalAnnualPlanOtherCost")).ToList();

            var allOtherCostTotalActualPlan = clientFieldData.Where(o => o.Name.Equals("TotalActualPlanOtherCost")).ToList();

            List<OtherCostViewModel> otherCostAll = new List<OtherCostViewModel>();

            int count = 0;
            foreach (var otherCostId in otherCostIdAll)
            {
                int index = count;
                count++;

                var otherCostYearPlanId =
                    allOtherCostTotalOriginalPlan.Where(
                        o => o.Id.Split(Convert.ToChar("|"))[0] == otherCostId)
                        .Select(o => o.Id.Split(Convert.ToChar("|"))[1])
                        .FirstOrDefault();

                var otherCost = new OtherCostViewModel
                {
                    OtherCostId = int.Parse(otherCostId),
                    ResultsMatrixId = viewModel.ResultsMatrixId,
                    Definition = definitionAll[index],
                    OrderNumber = index + 1,
                    OtherCostYearPlan = new List<OtherCostYearPlanViewModel>()
                };

                List<OtherCostYearPlanViewModel> otherCostYearPlanAll = new List<OtherCostYearPlanViewModel>();
                foreach (var year in allYear)
                {
                    var otherCostYearPlanIdByYear =
                        allField.Where(
                            o =>
                                o.Id.Split(Convert.ToChar("|"))[0] == otherCostId &&
                                o.Id.Split(Convert.ToChar("|"))[2] == year && o.Name.Equals("ipdOriginalPlanOtherCost"))
                            .Select(o => o.Id.Split(Convert.ToChar("|"))[1])
                            .FirstOrDefault();
                    var originalPlanOtherCostByYear =
                        allOriginalPlanOtherCost.Where(o => o.Id.Split(Convert.ToChar("|"))[2] == year).ToList();
                    var annualPlanOtherCostByYear =
                        allAnnualPlanOtherCost.Where(o => o.Id.Split(Convert.ToChar("|"))[2] == year).ToList();
                    var actualValueOtherCostByYear =
                        allActualValueOtherCost.Where(o => o.Id.Split(Convert.ToChar("|"))[2] == year).ToList();

                    var otherCostYearPlan = (
                        from originalPlanOtherCost in originalPlanOtherCostByYear
                        join annualPlanOtherCost in annualPlanOtherCostByYear on
                            originalPlanOtherCostByYear.IndexOf(originalPlanOtherCost)
                            equals annualPlanOtherCostByYear.IndexOf(annualPlanOtherCost)
                        join actualValueOtherCost in actualValueOtherCostByYear on
                            originalPlanOtherCostByYear.IndexOf(originalPlanOtherCost)
                            equals actualValueOtherCostByYear.IndexOf(actualValueOtherCost)
                        select new OtherCostYearPlanViewModel
                        {
                            OtherCostYearPlanId =
                                otherCostYearPlanIdByYear.IsNullOrEmpty() ? 0 : int.Parse(otherCostYearPlanIdByYear),
                            OtherCostId = otherCostId.IsNullOrEmpty() ? 0 : int.Parse(otherCostId),
                            Year = int.Parse(year),
                            OriginalPlan =
                                originalPlanOtherCost.Value.IsNullOrEmpty()
                                    ? (decimal?)null
                                    : decimal.Parse(originalPlanOtherCost.Value),
                            AnnualPlan =
                                annualPlanOtherCost.Value.IsNullOrEmpty()
                                    ? (decimal?)null
                                    : decimal.Parse(annualPlanOtherCost.Value),
                            ActualValue =
                                actualValueOtherCost.Value.IsNullOrEmpty()
                                    ? (decimal?)null
                                    : decimal.Parse(actualValueOtherCost.Value),
                            IsValidated = false
                        }).ToList();
                    otherCostYearPlanAll.Add(otherCostYearPlan[index]);
                }

                var otherCostTotalEop = (
                    from otherCostTotalOriginalPlan in allOtherCostTotalOriginalPlan
                    join otherCostTotalAnnualPlan in allOtherCostTotalAnnualPlan on
                        allOtherCostTotalOriginalPlan.IndexOf(otherCostTotalOriginalPlan)
                        equals allOtherCostTotalAnnualPlan.IndexOf(otherCostTotalAnnualPlan)
                    join otherCostTotalActualPlan in allOtherCostTotalActualPlan on
                        allOtherCostTotalOriginalPlan.IndexOf(otherCostTotalOriginalPlan)
                        equals allOtherCostTotalActualPlan.IndexOf(otherCostTotalActualPlan)
                    select new OtherCostYearPlanViewModel
                    {
                        OtherCostYearPlanId = otherCostYearPlanId.IsNullOrEmpty() ? 0 : int.Parse(otherCostYearPlanId),
                        OtherCostId = otherCostId.IsNullOrEmpty() ? 0 : int.Parse(otherCostId),
                        Year = TCMGlobalValues.END_OF_PROJECT_YEAR,
                        OriginalPlan =
                            otherCostTotalOriginalPlan.Value.IsNullOrEmpty()
                                ? (decimal?)null
                                : decimal.Parse(otherCostTotalOriginalPlan.Value),
                        AnnualPlan =
                            otherCostTotalAnnualPlan.Value.IsNullOrEmpty()
                                ? (decimal?)null
                                : decimal.Parse(otherCostTotalAnnualPlan.Value),
                        ActualValue =
                            otherCostTotalActualPlan.Value.IsNullOrEmpty()
                                ? (decimal?)null
                                : decimal.Parse(otherCostTotalActualPlan.Value),
                        IsValidated = false
                    }).ToList();
                otherCostYearPlanAll.Add(otherCostTotalEop[index]);
                otherCost.OtherCostYearPlan = otherCostYearPlanAll;
                otherCostAll.Add(otherCost);
            }

            var otherCostViewModelList = viewModel.OtherCost.ToList();
            var endOfProjectOtherCostViewModel = viewModel.OtherCost.Select(x => x.EndOfProject).ToList();

            int i = 0;
            foreach (var otherCost in otherCostViewModelList)
            {
                var index = i;
                i++;
                otherCost.OtherCostYearPlan.AddRange(new[] { endOfProjectOtherCostViewModel[index] });
            }

            List<OtherCostViewModel> otherCostList = new List<OtherCostViewModel>();

            foreach (var otherCostElement in otherCostAll)
            {
                if (otherCostElement.OtherCostId != 0)
                {
                    var otherCostExist =
                        otherCostViewModelList.FirstOrDefault(x => x.OtherCostId == otherCostElement.OtherCostId);
                    if (otherCostExist != null && (otherCostElement.Definition != otherCostExist.Definition ||
                        otherCostElement.OrderNumber != otherCostExist.OrderNumber))
                    {
                        otherCostList.Add(otherCostElement);
                    }
                    else
                    {
                        foreach (var otherCostYearPlanElemet in otherCostElement.OtherCostYearPlan)
                        {
                            if (otherCostYearPlanElemet.OtherCostYearPlanId != 0)
                            {
                                var otherCostYearPlanExist =
                                    otherCostViewModelList.SelectMany(x => x.OtherCostYearPlan)
                                        .FirstOrDefault(
                                            x => x.OtherCostYearPlanId == otherCostYearPlanElemet.OtherCostYearPlanId);
                                if (otherCostYearPlanExist != null &&
                                    (otherCostYearPlanExist.OriginalPlan != otherCostYearPlanElemet.OriginalPlan ||
                                    otherCostYearPlanExist.AnnualPlan != otherCostYearPlanElemet.AnnualPlan ||
                                    otherCostYearPlanExist.ActualValue != otherCostYearPlanElemet.ActualValue))
                                {
                                    otherCostList.Add(otherCostElement);
                                    break;
                                }
                            }
                            else
                            {
                                otherCostList.Add(otherCostElement);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    otherCostList.Add(otherCostElement);
                }
            }

            return otherCostList;
        }

        public static List<OutputViewModel> UpdateOutputViewModel(ClientFieldData[] clientFieldData, MappingProgressViewModel viewModel)
        {
            var allCategories = clientFieldData.Where(o => o.Name.Equals("ddCategories")).ToList();
            var allOutput = allCategories.Select(x => x.Id).ToList();
            var allOutputCategory = allCategories.Select(x => x.Value).ToList();
            var outputClient = (from outputId in allOutput
                                join outputCategoryId in allOutputCategory on allOutput.IndexOf(outputId) equals
                                    allOutputCategory.IndexOf(outputCategoryId)
                                select new OutputViewModel
                                {
                                    OutputId = int.Parse(outputId),
                                    OutputCategoryId = int.Parse(outputCategoryId)
                                }).ToList();

            var updateList = (from output in viewModel.Component.SelectMany(x => x.Outputs)
                              let outputAux = outputClient.FirstOrDefault(x => x.OutputId == output.OutputId)
                              where
                                  outputAux != null && output.OutputCategoryId != outputAux.OutputCategoryId
                              select new OutputViewModel
                              {
                                  OutputId = output.OutputId,
                                  OutputCategoryId = outputAux.OutputCategoryId
                              }).ToList();

            return updateList;
        }

        public static List<OutputYearVisualizationViewModel> UpdateOutputYearVisualizationViewModel(ClientFieldData[] clientFieldData, MappingProgressViewModel viewModel)
        {
            var allField = clientFieldData.Where(o => o.Name.Equals("idMappedUnits")).ToList();

            var allOutput = allField.Select(o => o.Id.Split(Convert.ToChar("|"))[0]).Distinct().ToList();

            var allYear = allField.Select(o => o.Id.Split(Convert.ToChar("|"))[2]).Distinct().ToList();

            var allOutputYearVisualization = (from output in allOutput
                                              from year in allYear
                                              let outputYearVisualizationId =
                                                  allField.Where(
                                                      o =>
                                                          o.Id.Split(Convert.ToChar("|"))[0] == output && o.Id.Split(Convert.ToChar("|"))[2] == year &&
                                                          o.Name.Equals("idMappedUnits"))
                                                      .Select(o => o.Id.Split(Convert.ToChar("|"))[1])
                                                      .FirstOrDefault()
                                              let mappedUnits =
                                                  allField.Where(
                                                      o =>
                                                          o.Id.Split(Convert.ToChar("|"))[0] == output && o.Id.Split(Convert.ToChar("|"))[2] == year &&
                                                          o.Name.Equals("idMappedUnits")).Select(x => x.Value).FirstOrDefault()
                                              select new OutputYearVisualizationViewModel
                                              {
                                                  OutputYearVisualizationId =
                                                      outputYearVisualizationId != null ? int.Parse(outputYearVisualizationId) : 0,
                                                  OutputId = int.Parse(output),
                                                  Year = int.Parse(year),
                                                  MappedUnits = mappedUnits.IsNullOrEmpty() ? (decimal?)null : decimal.Parse(mappedUnits)
                                              }).ToList();

            var updateList = (from oyv in
                                  viewModel.Component.SelectMany(x => x.Outputs).SelectMany(x => x.OutputYearPlanVisualization)
                              let oyvAux =
                                  allOutputYearVisualization.FirstOrDefault(
                                      x => x.OutputId.Equals(oyv.OutputId) && x.Year.Equals(oyv.Year))
                              where
                                  (oyvAux != null && oyv.MappedUnits != oyvAux.MappedUnits)
                              select new OutputYearVisualizationViewModel
                              {
                                  OutputYearVisualizationId = oyv.OutputYearVisualizationId,
                                  OutputId = oyv.OutputId,
                                  Year = oyv.Year,
                                  MappedUnits = oyvAux.MappedUnits
                              }).ToList();

            return updateList;
        }

        #endregion

        #region Outcomes

        public static void UpdateOutcomeIndicatorDetail(this OutcomeIndicatorDetailViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var outcomeId = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdOutcomeId"));
            var outcomeIndicatorId = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdOutcomeIndicatorId"));
            var outcomeStatementName = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdOutcomeStatementName"));
            var outcomeIndicatorName = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdOutcomeIndicatorName"));
            var meansOfVerification = clientFieldData.FirstOrDefault(o => o.Name.Equals("MeansOfVerification"));
            var isAutoCalcEop = clientFieldData.FirstOrDefault(o => o.Name.Equals("IsAutoCalcEop"));
            var saveInResultMatrix = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdSaveInRM"));
            var tcmReportingPeriod = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdTcReportingPeriod"));

            if (outcomeId != null)
            {
                viewModel.OutcomeId = int.Parse(outcomeId.Value);
            }

            if (outcomeIndicatorId != null)
            {
                viewModel.OutComeIndicatorId = int.Parse(outcomeIndicatorId.Value);
            }

            if (outcomeStatementName != null)
            {
                viewModel.OutcomeStatementName = outcomeStatementName.Value;
            }

            if (outcomeIndicatorName != null)
            {
                viewModel.OutcomeIndicatorName = outcomeIndicatorName.Value;
            }

            if (meansOfVerification != null)
            {
                viewModel.MeansOfVerification = meansOfVerification.Value;
            }

            if (isAutoCalcEop != null)
            {
                viewModel.IsAutoCalcEop = bool.Parse(isAutoCalcEop.Value);
            }

            if (saveInResultMatrix != null)
            {
                viewModel.RegisterInMatrixChange = bool.Parse(saveInResultMatrix.Value);
            }

            if (tcmReportingPeriod != null)
            {
                viewModel.TcmReportingPeriod = int.Parse(tcmReportingPeriod.Value);
            }
        }

        #endregion

        #region Output Details

        public static void UpdateOutputMilestoneDetail(this OutputMilestoneDetailViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var outputId = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdOutputId"));
            var milestoneId = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdMilestoneId"));
            var meansOfVerification = clientFieldData.FirstOrDefault(o => o.Name.Equals("MeansOfVerification"));
            var baselineYear = clientFieldData.FirstOrDefault(o => o.Name.Equals("BaselineYear"));
            var isAutoCalcEop = clientFieldData.FirstOrDefault(o => o.Name.Equals("IsAutoCalcEop"));
            var saveInResultMatrix = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdSaveInResultMatrix"));

            if (outputId != null)
            {
                viewModel.OutputId = int.Parse(outputId.Value);
            }

            if (milestoneId != null)
            {
                viewModel.MilestoneId = int.Parse(milestoneId.Value);
            }

            if (meansOfVerification != null)
            {
                viewModel.MeansOfVerification = meansOfVerification.Value;
            }

            if (baselineYear != null)
            {
                int temp;
                viewModel.BaseLineYear = int.TryParse(baselineYear.Value, out temp) ? (int?)temp : null;
            }

            if (isAutoCalcEop != null)
            {
                viewModel.IsAutoCalcEop = bool.Parse(isAutoCalcEop.Value);
            }

            if (saveInResultMatrix != null)
            {
                viewModel.RegisterInMatrixChange = bool.Parse(saveInResultMatrix.Value);
            }
        }

        public static void UpdateOutputIndicatorDetail(this OutputIndicatorDetailContentViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            const string OUTPUT_DESCRIPTION_INPUT_TEXT_NAME = "txt-output-description-edit";

            var outputId = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdOutputId"));
            var meansOfVerification = clientFieldData.FirstOrDefault(o => o.Name.Equals("meansOfVerification"));
            var baselineYear = clientFieldData.FirstOrDefault(o => o.Name.Equals("baselineYear"));
            var baseline = clientFieldData.FirstOrDefault(o => o.Name.Equals("baseline"));
            var isAutoCalcEop = clientFieldData.FirstOrDefault(o => o.Name.Equals("isAutoCalcEop"));
            var inactivateOutputsDetails = clientFieldData.FirstOrDefault(o => o.Name.Equals("inactivateOutputsDetails"));
            var saveInResultMatrix = clientFieldData.FirstOrDefault(o => o.Name.Equals("hdSaveInResultMatrix"));
            var theme = clientFieldData.FirstOrDefault(o => o.Name.Equals("theme"));
            var fundIndicator = clientFieldData.FirstOrDefault(o => o.Name.Equals("fundIndicator"));
            var otherDescription = clientFieldData.FirstOrDefault(o => o.Name.Equals("otherDescription"));
            var outputDescription = clientFieldData
                .FirstOrDefault(o => o.Name.Equals(OUTPUT_DESCRIPTION_INPUT_TEXT_NAME));

            if (outputId != null)
            {
                viewModel.OutputId = int.Parse(outputId.Value);
            }

            if (meansOfVerification != null)
            {
                viewModel.MeansOfVerification = meansOfVerification.Value;
            }

            if (baselineYear != null)
            {
                int temp;
                viewModel.BaseLineYear = int.TryParse(baselineYear.Value, out temp) ? (int?)temp : null;
            }

            if (baseline != null)
            {
                decimal temp;
                viewModel.BaseLine = decimal.TryParse(baseline.Value, out temp) ? (decimal?)temp : null;
            }

            if (isAutoCalcEop != null)
            {
                viewModel.IsAutoCalcEop = bool.Parse(isAutoCalcEop.Value);
            }

            if (inactivateOutputsDetails != null)
            {
                viewModel.InactivateOutputIndicator = bool.Parse(inactivateOutputsDetails.Value);
            }

            if (saveInResultMatrix != null)
            {
                viewModel.RegisterInMatrixChange = bool.Parse(saveInResultMatrix.Value);
            }

            if (theme != null)
            {
                int temp;
                viewModel.ThemeId = int.TryParse(theme.Value, out temp) ? temp : 0;
            }

            if (fundIndicator != null)
            {
                int temp;
                viewModel.FundIndicatorId = int.TryParse(fundIndicator.Value, out temp) ? (int?)temp : null;
            }

            if (otherDescription != null)
            {
                viewModel.OtherDescription = otherDescription.Value.IsNullOrEmpty() || otherDescription.Value == null ? null : otherDescription.Value;
            }

            if (outputDescription != null)
            {
                viewModel.OutputDescription = outputDescription.Value;
            }
        }

        #endregion

        #region OverallProjectManagement

        public static void UpdateOverallProjectManagementViewModel(this FindingRecommendationHeaderModel viewModel,
                                                                    ClientFieldData[] clientField)
        {
            var user = IDBContext.Current.UserLoginName;
            Logger.GetLogger().WriteDebug("ClientFieldDataMappers", "Method: UpdateOverallProjectManagementViewModel - user: " + user + ")");
            var FindingRecommendationId = clientField.Where(o => o.Name.Equals("FindingRecommendationId")).ToList();
            var typeStages = clientField.Where(o => o.Name.Equals("typeStages")).ToList();
            var typeDimensions = clientField.Where(o => o.Name.Equals("typeDimensions")).ToList();
            var typeCategories = clientField.Where(o => o.Name.Equals("typeCategories")).ToList();
            var FindingsArea = clientField.Where(o => o.Name.Equals("FindingsArea")).ToList();
            var RecommendationsArea = clientField.Where(o => o.Name.Equals("RecommendationsArea")).ToList();

            var deleteFinding = clientField.FirstOrDefault(o => o.Name.Equals("deleteFindingsAndRecommendations"));
            if (deleteFinding != null)
            {
                string[] listFindingsId = deleteFinding.Value.ToString().Split('|')
                                                        .Where(x => !string.IsNullOrEmpty(x)).ToArray();

                if (viewModel.DeleteFindingId == null)
                {
                    viewModel.DeleteFindingId = new List<int>();
                }

                foreach (string id in listFindingsId)
                {
                    viewModel.DeleteFindingId.Add(Convert.ToInt32(id));
                }
            }

            if ((FindingRecommendationId != null) && (FindingRecommendationId.Count() > 0))
            {
                for (int i = 0; i < FindingRecommendationId.Count(); i++)
                {
                    if (FindingRecommendationId.ElementAt(i).Value.Trim().Equals("0"))
                    {
                        var finding = new FindingRecommendationModel();

                        finding.FindingRecommendationId = 0;
                        finding.CategoryID = Convert.ToInt32(typeCategories.ElementAt(i).Value.Trim());
                        finding.StageId = Convert.ToInt32(typeStages.ElementAt(i).Value.Trim());
                        finding.Finding = FindingsArea.ElementAt(i).Value.Trim();
                        finding.Recommendation = RecommendationsArea.ElementAt(i).Value.Trim();
                        finding.RegisterUser = user;
                        finding.RegisterDate = DateTime.Now;

                        viewModel.FindingRecommendations.Add(finding);
                    }
                    else
                    {
                        foreach (var item in viewModel.FindingRecommendations)
                        {
                            var idFinding = Convert.ToInt32(FindingRecommendationId.ElementAt(i).Value.Trim());
                            if (item.FindingRecommendationId == idFinding)
                            {
                                item.CategoryID = Convert.ToInt32(typeCategories.ElementAt(i).Value.Trim());
                                item.StageId = Convert.ToInt32(typeStages.ElementAt(i).Value.Trim());
                                item.Finding = FindingsArea.ElementAt(i).Value.Trim();
                                item.Recommendation = RecommendationsArea.ElementAt(i).Value.Trim();
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region AchievementDelay

        public static void UpdateAchievementDelayFromFieldViewModel(
            this DelaysEditModel viewModel,
            ClientFieldData[] clientField,
            Dictionary<int, string> ListOutputs,
            Dictionary<int, string> ListOutcomes,
            List<MW.Domain.Models.Architecture.MasterDefinitions.ConvergenceMasterDataModel> listType)
        {
            var outComesElements = ListOutcomes;
            var outPutsElements = ListOutputs;
            var outComesId = listType.Where(x => x.Name == "Outcome delay").FirstOrDefault().ConvergenceMasterDataId;
            var outPutsId = listType.Where(x => x.Name == "Output delay").FirstOrDefault().ConvergenceMasterDataId;
            var achievementDelayId = clientField.Where(o => o.Name.Equals("AchievementDelayId")).ToList();
            var delayType = clientField.Where(o => o.Name.Equals("delayType")).ToList();
            var nameDelay = clientField.Where(o => o.Name.Equals("nameDelay")).ToList();
            var stateIsSolvedDelay = clientField.Where(o => o.Name.Equals("stateIsSolvedDelay")).ToList();
            var delayDetailReason = clientField.Where(o => o.Name.Equals("delayDetailReason")).ToList();
            var finding = clientField.Where(o => o.Name.Equals("finding")).ToList();
            var recommendation = clientField.Where(o => o.Name.Equals("recommendation")).ToList();
            var deleteDelays = clientField.Where(o => o.Name.Equals("deleteAchievementDelay")).FirstOrDefault().Value;

            var achievementDelays = (from idAchievementDelay in achievementDelayId
                                     join TypeDelay in delayType on achievementDelayId.IndexOf(idAchievementDelay)
                                     equals delayType.IndexOf(TypeDelay)
                                     join delayName in nameDelay on achievementDelayId.IndexOf(idAchievementDelay)
                                     equals nameDelay.IndexOf(delayName)
                                     join delayStateIsSolved in stateIsSolvedDelay on achievementDelayId.IndexOf(idAchievementDelay)
                                     equals stateIsSolvedDelay.IndexOf(delayStateIsSolved)
                                     join reasonDelayDetail in delayDetailReason on achievementDelayId.IndexOf(idAchievementDelay)
                                     equals delayDetailReason.IndexOf(reasonDelayDetail)
                                     join findingDelay in finding on achievementDelayId.IndexOf(idAchievementDelay)
                                     equals finding.IndexOf(findingDelay)
                                     join recommendationDelay in recommendation on achievementDelayId.IndexOf(idAchievementDelay)
                                     equals recommendation.IndexOf(recommendationDelay)
                                     select new AchievementDelayModel
                                     {
                                         AchievementDelayId = int.Parse(idAchievementDelay.Value),
                                         DelayTypeId = int.Parse(TypeDelay.Value),
                                         ResultElementId = delayName.Value,
                                         IsSolved = Convert.ToBoolean(delayStateIsSolved.Value),
                                         DelayReason = reasonDelayDetail.Value,
                                         Finding = findingDelay.Value,
                                         Recommendation = recommendationDelay.Value,
                                     }).ToList();

            foreach (var item in achievementDelays)
            {
                if (item.DelayTypeId == outComesId)
                {
                    item.ResultElementName = outComesElements.Where(x => x.Key.ToString() == item.ResultElementId).FirstOrDefault().Value;
                }
                else
                {
                    item.ResultElementName = outPutsElements.Where(x => x.Key.ToString() == item.ResultElementId).FirstOrDefault().Value;
                }

                if (!viewModel.AchievementDelays.Any(q => q.AchievementDelayId == item.AchievementDelayId))
                {
                    item.AchievementDelayId = 0;
                }
            }

            viewModel.AchievementDelays = achievementDelays;
            if (!string.IsNullOrEmpty(deleteDelays))
            {
                var deleteIds = deleteDelays.Split('|');
                foreach (var item in deleteIds)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        viewModel.DeleteDelay.Add(Convert.ToInt32(item));
                    }
                }
            }

            var OtherDelayId = clientField.Where(o => o.Name.Equals("OtherDelayId")).ToList();
            var OtherDelayName = clientField.Where(o => o.Name.Equals("OtherDelayName")).ToList();
            var OtherStateIsSolved = clientField.Where(o => o.Name.Equals("OtherStateIsSolved")).ToList();
            var OtherDelayReason = clientField.Where(o => o.Name.Equals("OtherDelayReason")).ToList();
            var DeleteOtherDelay = clientField.Where(o => o.Name.Equals("deleteAchievementOtherDelay")).FirstOrDefault().Value;

            viewModel.OtherDelays = (from idOtherDelay in OtherDelayId
                                     join nameOtherDelay in OtherDelayName on OtherDelayId.IndexOf(idOtherDelay)
                                           equals OtherDelayName.IndexOf(nameOtherDelay)
                                     join solvedOtherStateIs in OtherStateIsSolved on OtherDelayId.IndexOf(idOtherDelay)
                                           equals OtherStateIsSolved.IndexOf(solvedOtherStateIs)
                                     join reasonOtherDelay in OtherDelayReason on OtherDelayId.IndexOf(idOtherDelay)
                                           equals OtherDelayReason.IndexOf(reasonOtherDelay)
                                     select new OtherDelayModel
                                     {
                                         OtherDelayId = int.Parse(idOtherDelay.Value),
                                         DelayName = nameOtherDelay.Value,
                                         IsSolved = Convert.ToBoolean(solvedOtherStateIs.Value),
                                         DelayReason = reasonOtherDelay.Value,
                                     }).ToList();

            if (!string.IsNullOrEmpty(DeleteOtherDelay))
            {
                var deleteOtherIds = DeleteOtherDelay.Split('|');
                foreach (var item in deleteOtherIds)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        viewModel.DeleteOtherDelay.Add(Convert.ToInt32(item));
                    }
                }
            }
        }
        #endregion

        private static List<UserCommentViewModel> mapperEditComments(List<UserCommentViewModel> userComments, IEnumerable<ClientFieldData> editComments, IEnumerable<ClientFieldData> editCommentsId)
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

        private static List<UserCommentViewModel> mapperNewComment(List<UserCommentViewModel> userComments, IEnumerable<ClientFieldData> newComments)
        {
            if (newComments != null)
            {
                for (int i = 0; i < newComments.Count(); i++)
                {
                    var comment = new UserCommentViewModel();
                    comment.Comment = newComments.ElementAt(i).Value.Trim();
                    userComments.Add(comment);
                }
            }

            return userComments;
        }

        static List<InstitutionRelatedViewModel> UpdateExecutingAgencies(ClientFieldData[] clientFields)
        {
            var model = new List<InstitutionRelatedViewModel>();

            var rowsData = clientFields.Where(cfd => !string.IsNullOrWhiteSpace(cfd.Name) &&
                (cfd.Name == FindingRecomendationField.EA_ROW_ID ||
                    cfd.Name == FindingRecomendationField.EA_COMMENT ||
                    cfd.Name == FindingRecomendationField.EA_AGENCY_ID ||
                    cfd.Name == FindingRecomendationField.EA_QUALITY_ASSESSMENT));

            if (!rowsData.HasAny())
            {
                return model;
            }

            rowsData.GroupBy(cdf => cdf.Id)
                .Where(gr => !string.IsNullOrEmpty(gr.Key))
                .ForEach(row => model.Add(new InstitutionRelatedViewModel
                {
                    RowId = row.ParseToIntByFieldName(FindingRecomendationField.EA_ROW_ID),
                    InstitutionAcronym = row.GetValueByFieldName(
                        FindingRecomendationField.EA_AGENCY_ID),
                    Comment = row.GetValueByFieldName(
                        FindingRecomendationField.EA_COMMENT),
                    QualityAssessmentId = row.GetValueByFieldName(
                        FindingRecomendationField.EA_QUALITY_ASSESSMENT)
                }));

            return model;
        }

        static int GetParseValueToInt(this ClientFieldData field)
        {
            if (field == null)
                return default(int);

            int result;
            int.TryParse(field.Value, out result);

            return result;
        }

        static int ParseToIntByFieldName(
            this IGrouping<string, ClientFieldData> group,
            string fieldName)
        {
            return int.Parse(group.First(cdf => cdf.Name == fieldName).Value);
        }

        static string GetValueByFieldName(
            this IGrouping<string, ClientFieldData> group,
            string fieldName)
        {
            var field = group.FirstOrDefault(cdf => cdf.Name == fieldName);

            if (field == null)
                return string.Empty;

            return field.Value;
        }
    }
}