using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Application.Core.Mappers.OperationLifeCycleSrevice;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.CountryStrategyModule.Enums;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Values;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.DomainModel.Contracts.Repositories.OPUSModule;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Architecture.Language;
using IDB.MW.Business.PCRModule.Enums;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.DomainModel.Entities.CaseManager;
using IDB.MW.Application.CountryStrategyModule.Services.MonitoringResultMatrix;
using IDB.MW.Business.PCRModule.Contracts;
using IDB.MW.DomainModel.Entities.PCRModule.Enums;
using IDB.MW.Infrastructure.Configuration;
using IDB.MW.Application.PCRModule.Enums;
using IDB.MW.Application.CaseManager.Business;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Domain.Models.MasterData;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Controllers
{
    public partial class TestCaseManagerController : BaseController
    {
        #region Constants
        #endregion

        #region Fields
        private readonly IAuthorizationService _authorizationService;
        private readonly IOperationRepository _operationRepository;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IOperationSiteRequestRepository _operationSiteRequestRepository;
        private readonly IPCRWorkflowStatusRepository _pcrWorkflowStatusRepository;
        private readonly IMonitoringCSResultMatrixService _IMonitoringCSResultMatrixService;
        private readonly IPCRRepository _pcrRepository;
        private readonly IPCRCalculationService _pcrCalculationService;
        private readonly IOperationTeamDataRepository _operationTeamDataRepository;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Contructors

        public TestCaseManagerController(
            IAuthorizationService authorizationService,
            IOperationRepository operationRepository,
            IEnumMappingService enumMappingService,
            IOperationSiteRequestRepository operationSiteRequestRepository,
            IPCRWorkflowStatusRepository pcrWorkflowStatusRepository,
            IMonitoringCSResultMatrixService iMonitoringCSResultMatrixService,
            IPCRRepository pcrRepository,
            IPCRCalculationService pcrCalculationService,
            IOperationTeamDataRepository operationTeamDataRepository,
            ICatalogService catalogService)
            : base(authorizationService)
        {
            _authorizationService = authorizationService;
            _operationRepository = operationRepository;
            _enumMappingService = enumMappingService;
            _operationSiteRequestRepository = operationSiteRequestRepository;
            _pcrWorkflowStatusRepository = pcrWorkflowStatusRepository;
            _IMonitoringCSResultMatrixService = iMonitoringCSResultMatrixService;
            _pcrRepository = pcrRepository;
            _pcrCalculationService = pcrCalculationService;
            _operationTeamDataRepository = operationTeamDataRepository;
            _catalogService = catalogService;
        }

        #endregion

        #region Action Methods

        public virtual ActionResult ChangeOperationState()
        {
            return View("ChangeOperationState");
        }

        public virtual ActionResult ChangeState(string operationNumber, string opNum, string state)
        {
            var operation = _operationRepository.GetOne(x => x.OperationNumber == opNum);
            var cmb = CMBusiness.Get(operation);
            cmb
                .Context
                .ActivityPlanItems
                .Where(x => x.Code.ToLower() != CMConstants.DefaultActivityItems.ON)
                .ToList()
                .ForEach(x => x.StatusId = CMConstants.ActivityItemStatus.NotStarted.MasterId);
            var activityItemAppr = cmb.Context.APPRMilestone;

            switch (state)
            {
                case "draft":
                    var draftCode = _enumMappingService.GetMappingCode(CSOverallStageEnum.DRAFT);
                    operation.OperationData.OperationStageId = draftCode;
                    break;

                case "preparation":
                    var preparationCode = _enumMappingService.GetMappingCode(CSOverallStageEnum.PREP);
                    operation.OperationData.OperationStageId = preparationCode;
                    break;

                case "approved":
                    var approvedCode = _enumMappingService.GetMappingCode(CSOverallStageEnum.APPR);
                    activityItemAppr.SetAsCompleted();
                    operation.OperationData.OperationStageId = approvedCode;
                    break;

                case "expired":
                    var expiredCode = _enumMappingService.GetMappingCode(CSOverallStageEnum.OVSTGEXP);
                    activityItemAppr.SetAsCompleted();
                    operation.OperationData.OperationStageId = expiredCode;
                    break;

                case "closed":
                    var closedCode = _enumMappingService.GetMappingCode(CSOverallStageEnum.CL);
                    activityItemAppr.SetAsCompleted();
                    operation.OperationData.OperationStageId = closedCode;
                    break;

                case "cancelled":
                    var cancelledCode = _enumMappingService.GetMappingCode(CSOverallStageEnum.CA);
                    activityItemAppr.SetAsCompleted();
                    operation.OperationData.OperationStageId = cancelledCode;
                    break;

                default:
                    break;
            }

            cmb.UpdateActivityItem(activityItemAppr);
            _operationRepository.Update(operation);
            var message = "Operation State changed to " + state;
            return View(viewName: "ChangeOperationState", model: message);
        }

        public virtual ActionResult GlobalPermissions()
        {
            return View("ActivateGlobalPermissions");
        }

        public virtual ActionResult ActivateGlobalPermissions(string opNum, string username)
        {
            username = username.ToUpper();
            var message = string.Empty;
            var operation = _operationRepository.GetOne(x => x.OperationNumber == opNum);
            if (operation != null)
            {
                var operationId = operation.OperationId;
                var operationSiteReq = _operationSiteRequestRepository.GetOne(x => x.OperationId == operationId);

                if (operationSiteReq == null)
                {
                    _operationSiteRequestRepository.Create(new OperationSiteRequest()
                    {
                        OperationId = operationId,
                        Status = "Created",
                        Error = "OK",
                        Created = DateTime.Now,
                        CreatedBy = "TestCaseManager"
                    });
                    message = "Operation " + opNum + " Global Permissions has been activated. ";
                }
                else
                {
                    message = "Operation " + opNum + " already had Global Permissions activated. ";
                }

                if (username != string.Empty)
                {
                    var teamLeaderId = _catalogService.GetConvergenceMasterDataIdByCode(MemberRoleCode.TEAM_LEADER, MasterType.MEMBER_ROLE).Id;
                    ////var fiduciarySpecialistId = _catalogService.GetConvergenceMasterDataIdByCode(MemberRoleCode.FIDUCIARY_SPECIALIST, MasterType.MEMBER_ROLE).Id;

                    var operationTeamData = _operationTeamDataRepository.GetOne(x => x.OperationId == operationId &&
                                                                                    x.UserRoleId == teamLeaderId &&
                                                                                    x.UserName == username);

                    ////var fiduciarySpecialistIData = _operationTeamDataRepository.GetOne(x => x.OperationId == operationId &&
                    ////                                                                x.UserRoleId == fiduciarySpecialistId &&
                    ////                                                                x.UserName == username);

                    if (operationTeamData == null)
                    {
                        _operationTeamDataRepository.Create(
                       new OperationTeamData()
                        { 
                            OperationId = operationId,
                            UserRoleId = teamLeaderId,
                            FullName = username,
                            UserName = username
                        });
                    }

                    ////if (fiduciarySpecialistIData == null)
                    ////{

                    ////    _operationTeamDataRepository.Create(
                    ////   new OperationTeamData()
                    ////{
                    ////       OperationId = operationId,
                    ////       UserRoleId = fiduciarySpecialistId,
                    ////       FullName = username,
                    ////       UserName = username
                    ////   });
                    ////}
                }
            }
            else
            {
                message = "Operation " + opNum + " not found in database.";
            }

            return View(viewName: "ActivateGlobalPermissions", model: message);
        }

        public virtual ActionResult TestPCR()
        {
            return View("TestPCR");
        }

        public virtual ActionResult EditPCR(string opNum, bool published, bool required)
        {
            var message = string.Empty;

            if (opNum == null || opNum == string.Empty)
            {
                message = "Operation Number is Empty";
                return View(viewName: "TestPCR", model: message);
            }

            var operation = _operationRepository.GetOne(x => x.OperationNumber == opNum);
            if (operation == null)
            {
                message = "Operation Null";
                return View(viewName: "TestPCR", model: message);
            }

            if (!OperationTypeHelper.GetOperationTypes(operation.OperationId).Contains(OperationType.LON) &&
                !OperationTypeHelper.GetOperationTypes(operation.OperationId).Contains(OperationType.GUA) &&
                !OperationTypeHelper.GetOperationTypes(operation.OperationId).Contains(OperationType.IGR))
            {
                message = "Operation Not SG";
                return View(viewName: "TestPCR", model: message);
            }

            var cmb = CMBusiness.Get(operation);
            var activityItemInter = cmb.GetActivityItem(CMConstants.DefaultActivityItems.PCRINTER);
            var activityItemIntra = cmb.GetActivityItem(CMConstants.DefaultActivityItems.PCRINTRA);

            if (activityItemInter == null && activityItemIntra == null)
            {
                message = "Operation Not Have PCRINTRA/PCRINTER";
                return View(viewName: "TestPCR", model: message);
            }

            var message1 = ChangeStatePCRMilestone(published, operation, activityItemInter, activityItemIntra);
            var message2 = ChangeWorkflowRequired(operation, required);
            if (published && required)
            {
                var message3 = CreateAverageScore(operation);
            }

            //Test
            message = TestPCRForDebugg(operation);
            return View(viewName: "TestPCR", model: "Result Value: " + message);
        }

        public virtual ActionResult FakeLifeCycle()
        {
            return View("FakeLifeCycle");
        }

        public virtual ActionResult AssociateTemplate(string operationNumber)
        {
            var message = string.Empty;
            var operation = _operationRepository.GetOne(x => x.OperationNumber == operationNumber);
            if (operation != null) 
            {
                var cmb = new CMBusiness(operation);
                var items = cmb.Context.ActivityPlanItems;
                cmb.DeleteCurrentActivityPlan();
                cmb.CreateActivityPlan(76, items, true);
                message = "Activity Plan 'Country Strategy' created for operation " + operationNumber;
            }
            else
            {
                message = "Operation " + operationNumber + " doesn´t exist.";
            }

            return View("FakeLifeCycle", model: message);
        }

        public virtual ActionResult SetPreparationStage(string operationNumber)
        {
            var message = string.Empty;
            var operation = _operationRepository.GetOne(x => x.OperationNumber == operationNumber);
            var cmNotStared = _enumMappingService.GetMasterData<CSMilestoneStateEnum>(CSMilestoneStateEnum.NotStarted);
            var cmComplete = _enumMappingService.GetMasterData<CSMilestoneStateEnum>(CSMilestoneStateEnum.Completed);

            var cmNotStaredModel = new MasterDataModel() { Code = cmNotStared.Code, MasterId = cmNotStared.MasterDataId, MasterTypeId = cmNotStared.MasterType.MasterTypeId, Name = cmNotStared.NameEn, TypeName = cmNotStared.MasterType.Type };
            var cmCompleteModel = new MasterDataModel() { Code = cmComplete.Code, MasterId = cmComplete.MasterDataId, MasterTypeId = cmComplete.MasterType.MasterTypeId, Name = cmComplete.NameEn, TypeName = cmComplete.MasterType.Type };
            
            if (operation != null)
            {
                var cmb = CMBusiness.Get(operation);

                var activityItems = cmb.Context.ActivityPlanItems;

                foreach (var activityItem in activityItems)
                {
                    activityItem.EndDate = null;
                    activityItem.StartDate = null;
                    activityItem.StatusId = CMConstants.ActivityItemStatus.NotStarted.MasterId;
                    cmb.UpdateActivityItem(activityItem);
                }

                var transMilestone = cmb.Context.GetActivityItem("TRANPEREXP");
                transMilestone.EndDate = null;
                transMilestone.StartDate = null;
                transMilestone.StatusId = CMConstants.ActivityItemStatus.Completed.MasterId;
                cmb.UpdateActivityItem(transMilestone);

                message = "Operation " + operationNumber + " is in Preparation Stage.";
            }
            else
            {
                message = "Operation " + operationNumber + " doesn´t exist.";
            }

            return View("FakeLifeCycle", model: message);
        }

        public virtual ActionResult SetApprovedStage(string operationNumber)
        {
            var message = string.Empty;
            var operation = _operationRepository.GetOne(x => x.OperationNumber == operationNumber);
            var cmNotStared = _enumMappingService.GetMasterData<CSMilestoneStateEnum>(CSMilestoneStateEnum.NotStarted);
            var cmComplete = _enumMappingService.GetMasterData<CSMilestoneStateEnum>(CSMilestoneStateEnum.Completed);

            var cmNotStaredModel = new MasterDataModel() { Code = cmNotStared.Code, MasterId = cmNotStared.MasterDataId, MasterTypeId = cmNotStared.MasterType.MasterTypeId, Name = cmNotStared.NameEn, TypeName = cmNotStared.MasterType.Type };
            var cmCompleteModel = new MasterDataModel() { Code = cmComplete.Code, MasterId = cmComplete.MasterDataId, MasterTypeId = cmComplete.MasterType.MasterTypeId, Name = cmComplete.NameEn, TypeName = cmComplete.MasterType.Type };

            if (operation != null)
            {
                var cmb = CMBusiness.Get(operation);

                var activityItems = cmb.Context.ActivityPlanItems;

                foreach (var activityItem in activityItems)
                {
                    activityItem.EndDate = null;
                    activityItem.StartDate = null;
                    activityItem.StatusId = CMConstants.ActivityItemStatus.NotStarted.MasterId;
                    cmb.UpdateActivityItem(activityItem);
                }

                var transMilestone = cmb.Context.GetActivityItem("CSCW");
                transMilestone.EndDate = null;
                transMilestone.StartDate = null;
                transMilestone.StatusId = CMConstants.ActivityItemStatus.Completed.MasterId;
                cmb.UpdateActivityItem(transMilestone);

                message = "Operation " + operationNumber + " is in Approval Stage.";
            }
            else
            {
                message = "Operation " + operationNumber + " doesn´t exist.";
            }

            return View("FakeLifeCycle", model: message);
        }

        public virtual ActionResult SetExpiredStage(string operationNumber)
        {
            var message = string.Empty;
            var operation = _operationRepository.GetOne(x => x.OperationNumber == operationNumber);
            var cmNotStared = _enumMappingService.GetMasterData<CSMilestoneStateEnum>(CSMilestoneStateEnum.NotStarted);
            var cmComplete = _enumMappingService.GetMasterData<CSMilestoneStateEnum>(CSMilestoneStateEnum.Completed);

            var cmNotStaredModel = new MasterDataModel() { Code = cmNotStared.Code, MasterId = cmNotStared.MasterDataId, MasterTypeId = cmNotStared.MasterType.MasterTypeId, Name = cmNotStared.NameEn, TypeName = cmNotStared.MasterType.Type };
            var cmCompleteModel = new MasterDataModel() { Code = cmComplete.Code, MasterId = cmComplete.MasterDataId, MasterTypeId = cmComplete.MasterType.MasterTypeId, Name = cmComplete.NameEn, TypeName = cmComplete.MasterType.Type };

            if (operation != null)
            {
                var cmb = CMBusiness.Get(operation);

                var activityItems = cmb.Context.ActivityPlanItems;

                foreach (var activityItem in activityItems)
                {
                    activityItem.EndDate = null;
                    activityItem.StartDate = null;
                    activityItem.StatusId = CMConstants.ActivityItemStatus.NotStarted.MasterId;
                    cmb.UpdateActivityItem(activityItem);
                }

                var transMilestone = cmb.Context.GetActivityItem("CSCW");
                transMilestone.EndDate = null;
                transMilestone.StartDate = null;
                transMilestone.StatusId = CMConstants.ActivityItemStatus.Completed.MasterId;
                cmb.UpdateActivityItem(transMilestone);

                transMilestone = cmb.Context.GetActivityItem("CSVP");
                transMilestone.EndDate = null;
                transMilestone.StartDate = null;
                transMilestone.StatusId = CMConstants.ActivityItemStatus.Completed.MasterId;
                cmb.UpdateActivityItem(transMilestone);

                message = "Operation " + operationNumber + " is in Expired Stage.";
            }
            else
            {
                message = "Operation " + operationNumber + " doesn´t exist.";
            }

            return View("FakeLifeCycle", model: message);
        }

        #endregion

        #region Private Methods

        private string ChangeStatePCRMilestone(
            bool published, 
            Operation operation, 
            ActivityItem activityItemInter, 
            ActivityItem activityItemIntra)
        {
            var result = string.Empty;

            // PCRINTER
            if (activityItemInter != null)
            {
                if (activityItemInter.StatusId == CMConstants.ActivityItemStatus.Completed.MasterId && !published)
                {
                    activityItemInter.StatusId = CMConstants.ActivityItemStatus.NotStarted.MasterId;
                }
                else if (activityItemInter.StatusId != CMConstants.ActivityItemStatus.Completed.MasterId && published)
                {
                    activityItemInter.StatusId = CMConstants.ActivityItemStatus.Completed.MasterId;
                }

                if (published)
                {
                    result = "1- PCR Published: Operation PCRINTER - Complete";
                }
                else
                {
                    result = "1- PCR Published: Operation PCRINTER - NotStarted";
                }

                CMBusiness.Get(operation).UpdateActivityItem(activityItemInter);
            }

            // PCRINTRA
            if (activityItemIntra != null && activityItemInter != null)
            {
                if (activityItemInter.StatusId == CMConstants.ActivityItemStatus.Completed.MasterId && !published)
                {
                    activityItemIntra.StatusId = CMConstants.ActivityItemStatus.NotStarted.MasterId;
                }
                else if (activityItemInter.StatusId != CMConstants.ActivityItemStatus.Completed.MasterId && published)
                {
                    activityItemIntra.StatusId = CMConstants.ActivityItemStatus.Completed.MasterId;
                }

                if (published)
                {
                    result = "1- PCR Published: Operation PCRINTRA - Complete";
                }
                else
                {
                    result = "1- PCR Published: Operation PCRINTRA - NotStarted";
                }

                CMBusiness.Get(operation).UpdateActivityItem(activityItemInter);
            }

            return result;
        }

        private string ChangeWorkflowRequired(Operation operation, bool required)
        {
            var result = string.Empty;

            var workFlow = _pcrWorkflowStatusRepository.GetOne(x => x.OperationId == operation.OperationId);

            if (workFlow != null)
            {
                if (workFlow.IsRequired != required)
                {
                    workFlow.IsRequired = required;
                    _pcrWorkflowStatusRepository.Update(workFlow);
                }
            }
            else
            {
                DateTime thisDay = DateTime.Today;
                var pcrStatusCode = _enumMappingService.GetMappingCode(PCRFollowUpTimelineStatusEnum.OnTime);

                var entitieWorkFlow = new PCRWorkflowStatus
                {
                    OperationId = operation.OperationId,
                    IsRequired = required,
                    PCRStatusId = pcrStatusCode,
                    Disbursed95Date = thisDay,
                    Disbursed100Date = thisDay
                };

                _pcrWorkflowStatusRepository.Create(entitieWorkFlow);
            }

            if (required)
            {
                result = "2- PCR Required: True";
            }
            else
            {
                result = "2- PCR Required: False";
            }

            return result;
        }

        private string CreateAverageScore(Operation operation)
        {
            string averageScore = string.Empty;

            var filteredPCRList = _pcrRepository.GetByCriteria(x => x.Operation.OperationId == operation.OperationId &&
                                     x.ValidateDate.HasValue).OrderByDescending(x => x.ValidateDate).ToList();

            if (!filteredPCRList.Any())
            {
                var codePcrStage = _enumMappingService.GetMappingCode(PCRStageEnum.SPDPreValidationStageReviewed);
                var codePcrValidationStage = _enumMappingService.GetMappingCode(PCRValidationStageEnum.SPDPreValidationStageReviewed);

                var pcr = new IDB.MW.Domain.Entities.PCR()
                {
                    OperationId = operation.OperationId,
                    PCRStageId = codePcrStage,
                    PCRValidationStageId = codePcrValidationStage,
                    CountryObjetive = "Test Average",
                    ValidateDate = DateTime.Now,
                    IsRejected = false,
                    SPDVersionReject = 2,
                    WasStageDuplicated = false
                };

                var listPCReneral = new List<PCRGeneral>();

                var code1 = _enumMappingService.GetMappingCode(PCRGeneralTypeEnum.CoreEfficiency);
                listPCReneral.Add(new PCRGeneral()
                {
                    PCRId = pcr.PCRId,
                    ValueId = code1,
                    Score = 1.75m
                });

                code1 = _enumMappingService.GetMappingCode(PCRGeneralTypeEnum.CoreRelevance);
                listPCReneral.Add(new PCRGeneral()
                {
                    PCRId = pcr.PCRId,
                    ValueId = code1,
                    Score = 1.75m
                });

                code1 = _enumMappingService.GetMappingCode(PCRGeneralTypeEnum.CoreSustainability);
                listPCReneral.Add(new PCRGeneral()
                {
                    PCRId = pcr.PCRId,
                    ValueId = code1,
                    Score = 1.75m
                });

                code1 = _enumMappingService.GetMappingCode(PCRGeneralTypeEnum.BankCorporateDevelopment);
                listPCReneral.Add(new PCRGeneral()
                {
                    PCRId = pcr.PCRId,
                    ValueId = code1,
                    Score = 1.75m
                });

                code1 = _enumMappingService.GetMappingCode(PCRGeneralTypeEnum.CountryDevelopmentObjectives);
                listPCReneral.Add(new PCRGeneral()
                {
                    PCRId = pcr.PCRId,
                    ValueId = code1,
                    Score = 1.75m
                });

                code1 = _enumMappingService.GetMappingCode(PCRGeneralTypeEnum.MonitoringandEvaluationPlan);
                listPCReneral.Add(new PCRGeneral()
                {
                    PCRId = pcr.PCRId,
                    ValueId = code1,
                    Score = 1.75m
                });

                code1 = _enumMappingService.GetMappingCode(PCRGeneralTypeEnum.UseOfCountrySystems);
                listPCReneral.Add(new PCRGeneral()
                {
                    PCRId = pcr.PCRId,
                    ValueId = code1,
                    Score = 1.75m
                });

                code1 = _enumMappingService.GetMappingCode(PCRGeneralTypeEnum.EnvironmentalAndSocialSafeguards);
                listPCReneral.Add(new PCRGeneral()
                {
                    PCRId = pcr.PCRId,
                    ValueId = code1,
                    Score = 1.75m
                });

                pcr.PCRGenerals = listPCReneral;
                _pcrRepository.Create(pcr);
            }

            return averageScore;
        }

        private string TestPCRForDebugg(Operation operation)
        {
            var operationId = operation.OperationId;
            var averageScore = string.Empty;

            var isPCRPublished = operation.OpIsPCRCompleted();
            var pcrWorkFlow = _pcrWorkflowStatusRepository.GetOne(x => x.OperationId == operationId);

            if (isPCRPublished == false && (pcrWorkFlow != null && pcrWorkFlow.IsRequired))
            {
                averageScore = Localization.GetText("CS.Monitoring.TBD");
            }

            if (isPCRPublished == false && (pcrWorkFlow == null || (pcrWorkFlow != null && !pcrWorkFlow.IsRequired)))
            {
                averageScore = Localization.GetText("CS.Monitoring.NotAvailable");
            }

            if (isPCRPublished == true && (pcrWorkFlow != null && !pcrWorkFlow.IsRequired))
            {
                averageScore = Localization.GetText("CS.Monitoring.NotAvailable");
            }

            if (isPCRPublished == true && (pcrWorkFlow != null && pcrWorkFlow.IsRequired))
            {
                var filteredPCRList = _pcrRepository.GetByCriteria(x => x.Operation.OperationId == operationId &&
                                      x.ValidateDate.HasValue).OrderByDescending(x => x.ValidateDate).ToList();

                var currentPCR = filteredPCRList.FirstOrDefault();

                if (currentPCR != null)
                {
                    if (currentPCR.PCRStage == null)
                    {
                        _pcrRepository.Reload(currentPCR, "PCRStage", "PCRValidationStage", "Operation");

                        if (filteredPCRList.Count >= 2)
                        {
                            PCRHelpers.CopyNavigationPropertyToNewPCR(filteredPCRList.ElementAt(1), currentPCR);
                        }
                    }

                    var calculateSummaryByStageDTO = _pcrCalculationService.CalculateSummaryByStage(currentPCR);

                    var category = string.Empty;

                    switch (calculateSummaryByStageDTO.Category)
                    {
                        case OperationScoreCategoryEnum.FullAchievement:
                            category = Localization.GetText("PCR.Summary.CoreCriterion.FullAchievement");
                            break;
                        case OperationScoreCategoryEnum.HighAchievement:
                            category = Localization.GetText("PCR.Summary.CoreCriterion.HighAchievement");
                            break;
                        case OperationScoreCategoryEnum.PartialAchievement:
                            category = Localization.GetText("PCR.Summary.CoreCriterion.PartialAchievement");
                            break;
                        case OperationScoreCategoryEnum.LowAchievement:
                            category = Localization.GetText("PCR.Summary.CoreCriterion.LowAchievement");
                            break;
                        case OperationScoreCategoryEnum.NoAchievement:
                            category = Localization.GetText("PCR.Summary.CoreCriterion.NoAchievement");
                            break;
                    }

                    averageScore = string.Format("{0:0.00}", calculateSummaryByStageDTO.AverageScore) +
                                   (!string.IsNullOrWhiteSpace(category) ? " - " : null) + category;

                    var isTestCaseManagerAviable = ConfigurationServiceFactory.Current.GetApplicationSettings().TestCaseManagerAviable;

                    if (isTestCaseManagerAviable)
                    {
                        averageScore = "0.72 - Partial Achievement";
                    }
                }
            }

            return averageScore;
        }

        #endregion
    }
}