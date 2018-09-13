using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;

using AutoMapper;

using Newtonsoft.Json;

using IDB.Architecture;
using IDB.Architecture.BusinessRules;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.Architecture.Extensions;
using IDB.MW.Application.VisualizationModule.Helpers;
using IDB.MW.Domain.Models.Architecture.Visualization;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.Visualization.Enums;
using IDB.Presentation.MVC4.Areas.Visualization.Models;
using IDB.Presentation.MVC4.MVCCommon;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Application.TCM.Services.K2IntegrationService;
using IDB.MW.Application.TCM.Helpers;
using IDB.MW.Application.TCM.Enums.K2IntegrationEnums;
using IDB.MW.DomainModel.Contracts.Repositories.OPUSModule;
using IDB.MW.Business.TCAbstractModule.Contracts;
using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;
using IDB.MW.Application.Components;
using IDB.MW.Application.Reformulation.Services;
using IDB.MW.DomainModel.Contracts.Repositories.Core;

namespace IDB.Presentation.MVC4.Areas.Visualization.Business
{
    public class VisualizationRules : IRulesContainer
    {
        private const string MAP_OPERATION_ENTITY = "MapOperation";
        private const string VISUAL_OUTPUT_ENTITY = "VisualOutput";
        private const string NO_COMMENTS_ADDDED = "No comments were added";
        private const string ACCEPT_WORKFLOW = "AcceptWorkflow";
        private const string REJECT_WORKFLOW = "RejectWorkflow";
        private const string TCM_REJECTED_ALL = "Rejected by ";

        private readonly IK2IntegrationVisualizationHelper _K2IntegrationVisualizationHelper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IK2IntegrationService _k2IntegrationService;
        private readonly IFundOperationRepository _fundOperationRepository;
        private readonly IFundsSharepointService _fundsSharepointService;
        private readonly ITcmUniverseService _tcmUniverseService;
        private readonly IOperationRepository _operationRepository;
        private readonly IOperationHistoryService _operationHistoryService;

        public VisualizationRules(
            IK2IntegrationVisualizationHelper K2IntegrationVisualizationHelper,
            IK2IntegrationService k2IntegrationService,
            IFundOperationRepository fundOperationRepository,
            IFundsSharepointService fundsSharepointService,
            ITcmUniverseService tcmUniverseService,
            IOperationRepository opRep,
            IOperationHistoryService opHistServ)
        {
            _K2IntegrationVisualizationHelper = K2IntegrationVisualizationHelper;
            _authorizationService = AuthorizationServiceFactory.Current;
            _k2IntegrationService = k2IntegrationService;
            _fundOperationRepository = fundOperationRepository;
            _fundsSharepointService = fundsSharepointService;
            _tcmUniverseService = tcmUniverseService;
            _operationRepository = opRep;
            _operationHistoryService = opHistServ;
        }

        #region Visualization.Load

        [BusinessRule(
            ruleKey: "Visualization.Load.HasParent",
            description: "Verify if current operation has parent, if true we change working operation to the parent")]
        public void vilohp(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var hasParent = context.OperationContext.GetResult<bool>("Operation.BasicData.HasParent");
            if (hasParent)
            {
                ////Fix Nullable Int convertion error v.2.1
                int ParentId = Convert.ToInt32(context.OperationContext.CurrentOperation.ParentOperationId.Value);
                ////
                context.OperationContext = new OperationBusiness();
                Globals.Rules.ApplyParentLogic(context.OperationContext, rule);
                context.OperationContext.Load(ParentId);
                rule.SetResult(true);
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Load.VP.Load",
            description: "Check if current operation has visual project and load it. If exist more than one VP then remove each until exists only one in the list",
            Type = RuleType.ProductionRule)]
        public void vilovplo(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            List<VisualProjectModel> vps;
            if (context.VisualProjectId.HasValue &&
                context.VisualProjectId.Value == -1)
            {
                context.VisualProjects = new List<VisualProjectModel>();
                return;
            }
            else if (context.VisualProjectId.HasValue)
            {
                vps = new List<VisualProjectModel>();
                vps.Add(context.VisualizationRepository.VisualProjectGet(
                    context.VisualProjectId.Value));
            }
            else
            {
                vps = context.VisualizationRepository.VisualProjectGetByOperation(
                    context.OperationContext.CurrentOperation.OperationId);
            }

            while (vps.Count > 1)
            {
                vps.RemoveAt(0);
            }

            context.VisualProjects = vps;
        }

        [BusinessRule(
            ruleKey: "Visualization.Load.VP.LoadVersion",
            description: "Sets the vpv for each vp",
            Type = RuleType.ProductionRule)]
        public void vilovplv(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            if (context.VisualProjects.Any())
            {
                context.VisualProject = context.VisualProjects
                    .OrderByDescending(vp => vp.VisualProjectId)
                    .FirstOrDefault();
                foreach (var vp in context.VisualProjects)
                {
                    foreach (var vpv in vp.VisualProjectVersions)
                    {
                        vpv.LocationTypeName =
                            Localization.GetText(MasterDefinitions.GetMasterName(vpv.LocationTypeId));
                        vpv.VisualProjectMedia.RemoveAll(vpvm => vpvm.IsDeleted.IsTrue());
                    }
                }
            }
            else
            {
                vivpcr(rule);
            }

            if (context.VisualProject != null &&
                context.VisualProject.VisualProjectVersionsData != null)
            {
                context.VisualProjectVersionId = context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId;
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Load.HasVO",
            description: "Check if current operation has visual outputs and load these")]
        public void vilohvo(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            List<VisualOutputModel> vos;

            if (context.OutputId != 0)
            {
                vos = context
                    .VisualizationRepository
                    .VisualOutputsGetByOutput(context.OutputId);
                if (context.VisualOutputId != 0)
                {
                    context.VisualOutput = vos.First(
                        vo => vo.VisualOutputId == context.VisualOutputId);
                }
            }
            else if (context.VisualOutputId != 0)
            {
                vos = new List<VisualOutputModel>();

                var vo = context
                    .VisualizationRepository
                    .VisualOutputGet(context.VisualOutputId);

                if (vo == null)
                    return;

                context.VisualOutput = vo;
                vos.Add(vo);
            }
            else
            {
                vos = context
                    .VisualizationRepository
                    .VisualOutputsGet(context.OperationContext.OperationId, 
                    IDBContext.Current.CurrentLanguage);
            }

            if (vos == null)
                return;

            context.VisualOutputs = vos;
            rule.SetResult(true);
        }

        [BusinessRule(
            ruleKey: "Visualization.Load.VOSelectVersion",
            description: "Select the correct version of VOs")]
        public void vilovosv(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            context.VisualOutputs.RemoveAll(vo => !vo.VisualOutputVersions.Any());
            foreach (var vo in context.VisualOutputs)
            {
                var vovs = vo.VisualOutputVersions;
                var vovsNotDeleted = vovs.Where(vov => vov.IsDeleted == null || !vov.IsDeleted.Value);

                if (vovsNotDeleted.Any())
                    vo.VisualOutputVersionsData = vovsNotDeleted.Aggregate((agg, next) =>
                        next.VisualOutputVersionId > agg.VisualOutputVersionId ? next : agg);
                else if (vovs.Any())
                    vo.VisualOutputVersionsData = vovs.Aggregate((agg, next) =>
                        next.VisualOutputVersionId > agg.VisualOutputVersionId ? next : agg);

                foreach (var vov in vo.VisualOutputVersions)
                {
                    foreach (var media in vov.VisualOutputMedia)
                    {
                        media.Media.MediaSource = MasterDefinitions
                            .GetMaster(media.Media.MediaSourceId).Name;
                        media.Media.MediaType = MasterDefinitions
                            .GetMaster(media.Media.MediaTypeId).Name;
                    }
                }
            }

            if (context.VisualOutput != null &&
                context.VisualOutput.VisualOutputVersionsData != null)
            {
                context.VisualOutputVersionId =
                    context.VisualOutput.VisualOutputVersionsData.VisualOutputVersionId;
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Load.ActiveVP",
            description:
            "Selects the active VPV, if more than one then select DRAFT version",
            Type = RuleType.ProductionRule)]
        public void viloav(RuleContext ruleContext)
        {
            var context = ruleContext.TContext<VisualizationBusinessContext>();
            foreach (var vp in context.VisualProjects)
            {
                var vpvs = context.VisualProject.VisualProjectVersions
                    .Where(vpv => vpv.IsDeleted == null || !vpv.IsDeleted.Value);

                if (vpvs.Any())
                    context.VisualProject.VisualProjectVersionsData = vpvs.Aggregate((agg, next) =>
                        next.VisualProjectVersionId > agg.VisualProjectVersionId ? next : agg);
                else
                    context.VisualProject.VisualProjectVersionsData =
                        context.VisualProject.VisualProjectVersions.DefaultIfEmpty(new VisualProjectVersionModel())
                            .Aggregate((agg, next) => next.VisualProjectVersionId > agg.VisualProjectVersionId ? next : agg);
            }

            if (context.VisualProject != null &&
                context.VisualProject.VisualProjectVersionsData != null)
            {
                context.VisualProjectVersionId = context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId;
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Load.SortMedias",
            description: "Sort medias",
            Type = RuleType.ProductionRule)]
        public void viloavpsm(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();

            if (context.VisualProject != null && context.VisualProject.VisualProjectVersionsData != null)
            {
                context.VisualProject.VisualProjectVersionsData.VisualProjectMedia
                    .Sort((x, y) =>
                    {
                        var xValue = x.MediaOrder.HasValue ? x.MediaOrder.Value : x.MediaId;
                        var yValue = y.MediaOrder.HasValue ? y.MediaOrder.Value : y.MediaId;
                        return xValue.CompareTo(yValue);
                    });
                var medias = context.VisualProject.VisualProjectVersionsData.VisualProjectMedia;
                for (int index = 0; index < medias.Count; index++)
                {
                    medias[index].MediaOrder = index;
                }
            }

            foreach (var vo in context.VisualOutputs)
            {
                foreach (var vov in vo.VisualOutputVersions)
                {
                    vov.VisualOutputMedia.Sort((x, y) =>
                    {
                        var xValue = x.MediaOrder.HasValue ? x.MediaOrder.Value : x.MediaId;
                        var yValue = y.MediaOrder.HasValue ? y.MediaOrder.Value : y.MediaId;
                        return xValue.CompareTo(yValue);
                    });
                    var vmedias = vov.VisualOutputMedia;
                    for (int index = 0; index < vmedias.Count; index++)
                    {
                        vmedias[index].MediaOrder = index;
                    }
                }
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Load.MediaMasterName",
            description:
            "Asign master names for each master id of medias",
            Type = RuleType.ProductionRule)]
        public void vilommn(RuleContext ruleContext)
        {
            var context = ruleContext.TContext<VisualizationBusinessContext>();
            if (context.VisualProjects != null)
            {
                foreach (var vp in context.VisualProjects)
                {
                    foreach (var vpv in vp.VisualProjectVersions)
                    {
                        foreach (var media in vpv.VisualProjectMedia)
                        {
                            media.Media.MediaSource = MasterDefinitions.GetMasterName(media.Media.MediaSourceId);
                            media.Media.PermissionStatus = MasterDefinitions.GetMasterName(media.Media.PermissionStatusId);
                            media.Media.MediaType = MasterDefinitions.GetMasterName(media.Media.MediaTypeId);
                        }
                    }
                }
            }

            if (context.VisualProject != null &&
                context.VisualProject.VisualProjectVersionsData != null)
            {
                foreach (var media in context.VisualProject.VisualProjectVersionsData.VisualProjectMedia)
                {
                    media.Media.MediaSource = MasterDefinitions.GetMasterName(media.Media.MediaSourceId);
                    media.Media.PermissionStatus = MasterDefinitions.GetMasterName(media.Media.PermissionStatusId);
                    media.Media.MediaType = MasterDefinitions.GetMasterName(media.Media.MediaTypeId);
                }
            }

            if (context.VisualOutputs != null)
            {
                foreach (var vo in context.VisualOutputs)
                {
                    foreach (var vov in vo.VisualOutputVersions)
                    {
                        foreach (var media in vov.VisualOutputMedia)
                        {
                            media.Media.MediaSource = MasterDefinitions.GetMasterName(media.Media.MediaSourceId);
                            media.Media.PermissionStatus = MasterDefinitions.GetMasterName(media.Media.PermissionStatusId);
                            media.Media.MediaType = MasterDefinitions.GetMasterName(media.Media.MediaTypeId);
                        }
                    }
                }
            }

            if (context.VisualOutput != null &&
                context.VisualOutput.VisualOutputVersionsData != null)
            {
                foreach (var media in context.VisualOutput.VisualOutputVersionsData.VisualOutputMedia)
                {
                    media.Media.MediaSource = MasterDefinitions.GetMasterName(media.Media.MediaSourceId);
                    media.Media.PermissionStatus = MasterDefinitions.GetMasterName(media.Media.PermissionStatusId);
                    media.Media.MediaType = MasterDefinitions.GetMasterName(media.Media.MediaTypeId);
                }
            }
        }
        #endregion

        #region Visualization.WorkflowAction.IsEnable
        [BusinessRule(
            ruleKey: "Visualization.Workflow.IsEnable",
            description: "Verify if current visual project can begin a workflow",
            Type = RuleType.AndRuleSet)]
        public void viwois(RuleContext rule)
        {
        }

        [BusinessRule(
            ruleKey: "Visualization.Workflow.IsEnable.HasReadyEntities",
            description: "Verify if current visual project has an (visual project version, visual output or history record) ready for validation")]
        public void viwoiehre(RuleContext rule)
        {
            var context = (VisualizationBusinessContext)rule.Context;
            var readyValStage = context.VIS_READY.MasterId;
            var tlValStage = context.VIS_TL.MasterId;
            var result = false;

            if (context.VisualProject.VisualProjectVersions.Any(vpv =>
                context.IsReadyStage((int)vpv.ValidationStageId)))
            {
                result = true;
            }

            context.VisualOutputs.ForEach((vo) =>
            {
                vo.VisualOutputVersions.ForEach((vov) =>
                {
                    if (context.IsReadyStage((int)
                        vov.ValidationStageId))
                    {
                        result = true;
                    }
                });
            });
            rule.SetResult(result);
        }

        [BusinessRule(
            ruleKey: "Visualization.Workflow.IsEnable.HasPermission",
            description: "Verify if current user has the needed permission")]
        public void viwoiehp(RuleContext rule)
        {
            if (IDBContext.Current.HasPermission("Visualization TL Write"))
            {
                rule.SetResult(true);
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Workflow.IsEnable.Action",
            description: "Create action path to begin visulization workflow",
            Type = RuleType.ProductionRule)]
        public void viwoisac(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var action = context.Url.Action("RequestDetails");
            rule.SetResult(action);
        }
        #endregion

        #region Visulization.VP.IsEditable

        [BusinessRule(
            ruleKey: "Visualization.VP.IsEditable",
            description: "Check if current VP can be edited, if is editable produces URL of edit action and confirmation messages",
            Type = RuleType.OrRuleSet)]
        public void vivpie(RuleContext ruleContext)
        {
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.IsEditable.HasPermission",
            description: "Verify if current user has required permissions",
            ExecutionConditions = ExecutionCondition.MustBeTrue)]
        public void vivpiehp(RuleContext rule)
        {
            if (IDBContext.Current.HasPermission(Permission.VISUALIZATION_WRITE))
            {
                rule.SetResult(true);
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.IsEditable.IsDraft",
            description: "Verify if current VPV is draft")]
        public void vivpieid(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var vpv = context.VisualProject.VisualProjectVersionsData;
            if (vpv.ValidationStageId == context.VIS_DRAFY.MasterId)
            {
                rule.SetResult(true);
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.IsEditable.IsTL-Validated",
            description: "Verify if current VPV is VIS_TL and user is TL to enable edit")]
        public void vivpieit(RuleContext rule)
        {
            var context = (VisualizationBusinessContext)rule.Context;
            var vpv = context.VisualProject.VisualProjectVersionsData;
            if (vpv.ValidationStageId == context.VIS_TL.MasterId &&
                IDBContext.Current.HasPermission("Visualization TL Write"))
            {
                rule.SetResult(true);
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.IsEditable.Action",
            description: "Generate edit action of visual project",
            Type = RuleType.ProductionRule)]
        public void vivpieac(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            rule.SetResult(context.Url.Action("GridEdit"));
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.IsEditable.NotEditMode",
            description: "Only show edit button when is not editing",
            ExecutionConditions = ExecutionCondition.MustBeTrue)]
        public void vivpienem(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            if (!context.EditMode)
            {
                rule.SetResult(true);
            }
        }

        #endregion

        #region Visualization.Grid.Load

        [BusinessRule(
            ruleKey: "Visualization.Grid.Load.GetData",
            description: "Load results matrix outputs and filter it by: OutputYearPlans.IsCost, OutputYearPlans.Year>0 ",
            Type = RuleType.ProductionRule,
            Priority = -1)]
        public void vigrlo(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            context.ResultsMatrixContext.Execute("ResultsMatrix.Outputs.Load");
            var components = context.ResultsMatrixContext.Current.Components;
            components.Sort((x, y) =>
            {
                return x.OrderNumber.CompareTo(y.OrderNumber);
            });
            components.ForEach((component) =>
            {
                component.Outputs.ForEach((output) =>
                {
                    output.OutputYearPlans.RemoveAll(oyp =>
                        oyp.IsCost || oyp.Year <= 0);
                    output.OutputYearPlans.Sort((x, y) =>
                    {
                        return x.Year.CompareTo(y.Year);
                    });
                });
                component.Outputs.RemoveAll(op => !op.OutputYearPlans.Any());
            });
            components.RemoveAll(cp => !cp.Outputs.Any());
        }

        [BusinessRule(
            ruleKey: "Visualization.Grid.Load.EnableNewVOByMappingValues",
            description: "Set enable to create new VO when at least one output year plan has P(a) or A",
            Type = RuleType.ProductionRule,
            Priority = -1)]
        public void vigrloenvocr(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var oyvs = context
                .ResultsMatrixContext
                .Current
                .Components
                .SelectMany(cp => cp.Outputs)
                .SelectMany(op => op.OutputYearPlans);
            var outputs = context
                .ResultsMatrixContext
                .Current
                .Components
                .SelectMany(cp => cp.Outputs);
            foreach (var output in outputs)
            {
                output.HasVisualUnits = oyvs.Any(oyv =>
                    oyv.OutputId == output.OutputId &&
                    ((oyv.AnnualPlan.HasValue && oyv.AnnualPlan > 0) ||
                    (oyv.ActualValue.HasValue && oyv.ActualValue > 0)));
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Grid.Load.EnableNewVOBy",
            description: "Set enable to create new VO when at least one output year plan has P(a) or A",
            Type = RuleType.ProductionRule,
            Priority = -1)]
        public void vigrloenvocrli(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();

            var outputs = context
                .ResultsMatrixContext
                .Current
                .Components
                .SelectMany(cp => cp.Outputs);

            foreach (var output in outputs)
            {
                output.HasCategory = !string.IsNullOrEmpty(output.OutputCategory.NameEn) &&
                    output.OutputCategory.NameEn != "UNDEFINED";
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Grid.Load.CalculateAMapping",
            description: "Calculate the the actual mapping value",
            Type = RuleType.ProductionRule,
            Priority = -1)]
        public void vigrlocam(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var oyvs = context
                .ResultsMatrixContext
                .Current
                .Components
                .SelectMany(cp => cp.Outputs)
                .SelectMany(op => op.OutputYearVisualizations);
            foreach (var oyv in oyvs)
            {
                oyv.VisualObjects = 0;
            }

            foreach (var vo in context.VisualOutputs)
            {
                if (vo.VisualOutputVersionsData != null &&
                    vo.VisualOutputVersionsData.OutputYearPlan != null)
                {
                    var oyv = oyvs.FirstOrDefault(oy =>
                        oy.Year == vo.VisualOutputVersionsData.OutputYearPlan.Year &&
                        oy.OutputId == vo.VisualOutputVersionsData.OutputYearPlan.OutputId);
                    if (oyv != null && vo.VisualOutputVersionsData.OutputUnits.HasValue)
                    {
                        oyv.VisualObjects += vo.VisualOutputVersionsData.OutputUnits.Value;
                    }
                    else if (vo.VisualOutputVersionsData.OutputUnits.HasValue)
                    {
                        var output = context
                            .ResultsMatrixContext
                            .Current
                            .Components
                            .Select(cp => cp.Outputs
                                .FirstOrDefault(otp => otp.OutputId ==
                                    vo.VisualOutputVersionsData.OutputYearPlan.OutputId))
                            .FirstOrDefault();

                        if (output != null)
                        {
                            output.OutputYearVisualizations.Add(new MW.Domain.Models.Architecture.ResultMatrix.Outputs.OutputYearVisualizationModel()
                            {
                                OutputId = vo.VisualOutputVersionsData.OutputYearPlan.OutputId,
                                Year = vo.VisualOutputVersionsData.OutputYearPlan.Year,
                                VisualObjects = vo.VisualOutputVersionsData.OutputUnits.Value,
                                MappedUnits = 0
                            });
                        }
                    }
                }
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Grid.Load.VP.IsEmpty",
            description: "Creates an empty VP obj when there is not one in the database",
            Type = RuleType.ProductionRule)]
        public void vigrloavpie(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            if (context.VisualProject == null)
            {
                context.VisualProject = new VisualProjectModel()
                {
                    VisualProjectId = -1,
                    VisualProjectVersions = new List<VisualProjectVersionModel>()
                };
            }

            if (context.VisualProject.VisualProjectVersions == null)
            {
                context.VisualProject.VisualProjectVersions = new List<VisualProjectVersionModel>();
            }

            if (context.VisualProject.VisualProjectVersionsData == null)
            {
                context.VisualProject.VisualProjectVersionsData = new
                    VisualProjectVersionModel()
                    {
                        VisualProjectVersionId = -1,
                        VisualProjectMedia = new List<VisualProjectMediaModel>()
                    };
            }
        }
        #endregion

        #region Visualization.Grid.LoadComments
        [BusinessRule(
            ruleKey: "Visualization.Grid.LoadCommens.Load",
            description: "For displayed entities",
            Type = RuleType.ProductionRule,
            Priority = -1)]
        public void vigrloco(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var entities = new List<VisualizationCommentModel>();
            var vovs = context.VisualOutputs.Select(vo => vo.VisualOutputVersionsData);

            foreach (var vpv in context.VisualProject.VisualProjectVersions)
            {
                entities.Add(new VisualizationCommentModel()
                {
                    EntityType = VisualEntityTypes.VisualProjectVersion,
                    EntityId = vpv.VisualProjectVersionId
                });
            }

            foreach (var vov in vovs)
            {
                entities.Add(new VisualizationCommentModel()
                {
                    EntityType = VisualEntityTypes.VisualOutputVersion,
                    EntityId = vov.VisualOutputVersionId
                });
            }

            context.UserComments = context.VisualizationRepository.CommentsGet(entities);
        }

        #endregion

        #region Visualization.Grid.Filter
        [BusinessRule(
            ruleKey: "Visualization.Grid.Filter.Ready",
            description: "Filter ready for validation/TL Complete entities and its parent's",
            Type = RuleType.ProductionRule)]
        public void vigrlofr(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();

            var oyps = context.VisualOutputs
                .SelectMany(vo => vo.VisualOutputVersions)
                .Select(vov => vov.OutputYearPlan);
            var Components = context.ResultsMatrixContext
                .Current
                .Components;
            var components = Components
                .ToList();
            var outputs = components
                .SelectMany(cp => cp.Outputs).ToList()
                .ToList();
            var vps = context.VisualProjects;

            foreach (var vp in vps.ToList())
            {
                if (!context.IsReadyStage(vp.VisualProjectVersionsData.ValidationStageId))
                {
                    vps.Remove(vp);
                }
            }

            foreach (var component in components)
            {
                var rmComponent = Components.FirstOrDefault(
                    cp => cp.ComponentId == component.ComponentId);
                foreach (var output in outputs)
                {
                    if (!oyps.Any(oyp =>
                            oyp.OutputId == output.OutputId))
                    {
                        rmComponent.Outputs.Remove(output);
                    }
                }

                if (!rmComponent
                    .Outputs
                    .Any())
                {
                    Components.Remove(rmComponent);
                }
            }
        }

        #endregion

        #region Visualization.Grid.Edit

        #endregion

        #region Visualization.Grid.Save
        [BusinessRule(
            ruleKey: "Visualization.Grid.Save",
            description: "Generate redirect URL after save information",
            Type = RuleType.ProductionRule,
            Priority = -1)]
        public void vigrsa(RuleContext rule)
        {
            rule.SetResult(MVCHelper.RedirectToAction("Grid"));
        }

        [BusinessRule(
            ruleKey: "Visualization.Grid.Save.ReadyForValidation",
            description: "Store ready for validation stage id",
            Type = RuleType.LogicRule,
            Priority = -1)]
        public void vigrsarv(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var isTL = IDBContext.Current.HasRole(Role.TEAM_LEADER);

            var vovv = MvcMapper
                .Select("VisualOutputVersion.Validate", ControlType.checkbox);
            var vovs = context.VisualOutputs.Select(vo => vo.VisualOutputVersionsData);
            var hrvv = MvcMapper
                .Select("HistoryRecordVersion.Validate", ControlType.checkbox);

            var vpvv = MvcMapper
                .Select("VisualProjectVesion.Validate", ControlType.checkbox);
            var vpvs = context.VisualProject.VisualProjectVersions;

            vovs.ForEach(vov => vov.Modified = null);
            foreach (var vov in vovs)
            {
                var item = vovv.FirstOrDefault(vo =>
                    vo.Id == vov.VisualOutputVersionId.ToString());

                //only process when check is enabled
                if (!vov.IsForRequest)
                {
                    continue;
                }

                //only process when is ready for validation
                if (context.NotForValidation(vov.ValidationStageId))
                {
                    continue;
                }

                if (item == null || !item.Checked)
                {
                    vov.ValidationStageId = context.VIS_DRAFY.MasterId;
                }
                else
                {
                    if (isTL)
                    {
                        vov.ValidationStageId = context.VIS_TL.MasterId;
                    }
                    else
                    {
                        vov.ValidationStageId = context.VIS_READY.MasterId;
                    }
                }

                vov.Modified = DateTime.Now;
            }

            if (vovs.Any(vo => vo.Modified != null))
            {
                context.VisualizationRepository
                    .VisualOutputVersionSaveValstage(
                        vovs.Where(vo => vo.Modified != null)
                        .ToList());
            }

            foreach (var vpv in vpvs)
            {
                var item = vpvv.FirstOrDefault(vp =>
                    vp.Id == vpv.VisualProjectVersionId.ToString());

                //only process when check is enabled
                if (!vpv.IsForRequest)
                {
                    vpv.Modified = null;
                    continue;
                }

                //only process when is ready for validation
                if (context.NotForValidation(vpv.ValidationStageId))
                {
                    vpv.Modified = null;
                    continue;
                }

                if (item == null || !item.Checked)
                {
                    vpv.ValidationStageId = context.VIS_DRAFY.MasterId;
                }
                else
                {
                    if (isTL)
                    {
                        vpv.ValidationStageId = context.VIS_TL.MasterId;
                    }
                    else
                    {
                        vpv.ValidationStageId = context.VIS_READY.MasterId;
                    }
                }

                vpv.Modified = DateTime.Now;
            }

            if (vpvs.Any(vpv => vpv.Modified != null))
            {
                context.VisualizationRepository
                    .VisualProjectVersionSaveValstage(
                    vpvs.Where(vpv => vpv.Modified != null)
                    .ToList());
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.Grid.Save.Units",
            description: "Store units of each output year plan, it verifies if output recived exists",
            Type = RuleType.LogicRule,
            Priority = -1)]
        public void vigrsaun(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var oyvs = MvcMapper
                .Map<OutputYearVisualizationModel>("OutputYearVisualization");
            var outputs = context.ResultsMatrixContext.Current.Components
                .SelectMany(co => co.Outputs);
            foreach (var oyv in oyvs)
            {
                if (!outputs.Any(vo => vo.OutputId == oyv.OutputId))
                {
                    throw new Exception("The given visual output don't exists in current project");
                }
            }

            if (!oyvs.Any())
            {
                return;
            }

            context.VisualizationRepository
                    .OutputYearVisualizationSaveMultiple(oyvs);
        }

        [BusinessRule(
            ruleKey: "Visualization.Grid.Save.Category",
            description: "Store Categories of each output",
            Type = RuleType.ProductionRule,
            Priority = -1)]
        public void vigrsaca(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var outputs = MvcMapper
                .Map<OutputModel>("Output");
            if (!outputs.Any())
            {
                return;
            }

            context.VisualizationRepository
                    .OutputCategorySaveMultiple(outputs);
        }
        #endregion

        #region Visualization.Workflow.Request

        #endregion

        #region Visualization.Comments
        [BusinessRule(
            ruleKey: "Visualization.Comments.Save",
            description: "Save an comment associated to one obj",
            Type = RuleType.ProductionRule)]
        public void vicomsa(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var entityId = MvcMapper.Get<int>("entityId", true);
            var entityType = MvcMapper.Get<string>("entityType", true);
            var text = MvcMapper.Get<string>("text", true);
            var user = IDBContext.Current.UserName;
            var comment = new VisualizationCommentModel()
            {
                EntityId = entityId,
                EntityType = (VisualEntityTypes)Enum.Parse(typeof(VisualEntityTypes), entityType),
                Text = text
            };
            context.VisualizationRepository.SaveComment(comment);
        }
        #endregion

        #region Visualization.VP.IsEditable
        [BusinessRule(
            ruleKey: "Visualization.VPV.IsEditable",
            description: "Check if current VP can be edited, if is editable produces URL of edit action and confirmation messages",
            Type = RuleType.OrRuleSet)]
        public void vivpvie(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
        }

        [BusinessRule(
            ruleKey: "Visualization.VPV.IsEditable.HasPermission",
            description: "Check if current user has required permissiond",
            Type = RuleType.LogicRule,
            ExecutionConditions = ExecutionCondition.MustBeTrue)]
        public void vivpviehp(RuleContext rule)
        {
            rule.SetResult(IDBContext.Current.HasPermission(Permission.VISUALIZATION_WRITE));
        }

        [BusinessRule(
            ruleKey: "Visualization.VPV.IsEditable.IsNotDeleted",
            description: "Check if current vpv is not check to delete",
            Type = RuleType.LogicRule,
            ExecutionConditions = ExecutionCondition.MustBeTrue)]
        public void vivpvieind(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();

            rule.SetResult(context.VisualProject.VisualProjectVersionsData.IsDeleted == null ||
                !context.VisualProject.VisualProjectVersionsData.IsDeleted.Value);
        }

        [BusinessRule(
            ruleKey: "Visualization.VPV.IsEditable.TLValidated",
            description: "Disable the edit button when the state is validated and the user is not TL",
            Type = RuleType.LogicRule,
            ExecutionConditions = ExecutionCondition.MustBeTrue)]
        public void vivpietlv(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var result = true;
            if (context.VisualProject.VisualProjectVersionsData.ValidationStageId == context.VIS_TL.MasterId &&
                !IDBContext.Current.HasPermission(Permission.VISUALIZATION_WRITE_TL))
            {
                result = false;
            }

            rule.SetResult(result);
        }

        [BusinessRule(
            ruleKey: "Visualization.VPV.IsEditable.Confirmation",
            description: "Set validation message in edit link",
            Type = RuleType.ProductionRule)]
        public void vivpvieco(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();

            if (context.VisualProject.VisualProjectVersionsData.ValidationStageId == context.VIS_TL.MasterId ||
                context.VisualProject.VisualProjectVersionsData.ValidationStageId == context.VIS_COO.MasterId)

                rule.SetResult(Localization.GetText("VIS.map.already.validated.warning"));
        }

        [BusinessRule(
            ruleKey: "Visualization.VPV.IsEditable.Action",
            description: "Creates VPV edit link",
            Type = RuleType.ProductionRule)]
        public void vivpvieac(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();

            rule.SetResult(context.Url.Action("VPEdit", new
            {
                visualProjectId = context.VisualProject.VisualProjectVersionsData.VisualProjectId,
            }));
        }

        #endregion

        #region Visualization.VP.Edit

        [BusinessRule(
            ruleKey: "Visualization.VP.Edit.CheckValidationStage",
            description: "Verify current VPV validation stage and change if necesary",
            Type = RuleType.ProductionRule)]
        public void vivpvedcvs(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var currentValidationStage = context.VisualProject.VisualProjectVersionsData.ValidationStageId;

            if (currentValidationStage == context.VIS_REV.MasterId)
            {
                throw new InvalidOperationException("Current visual project version can't be edited because is in a workflow");
            }

            context.VisualProjectVersionId = context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId;

            if (currentValidationStage == context.VIS_COO.MasterId || currentValidationStage == context.VIS_TL.MasterId)
            {
                context.ShowWarningMessageOnCancelEdition = true;
                context.VisualProjectVersionId = context.VisualizationRepository
                    .VisualProjectVersionClone(context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId);
                rule.SetResult(true);
                context.VisualProject.VisualProjectVersionsData =
                    context.VisualizationRepository.VisualProjectVersionGet(context.VisualProjectVersionId.Value);
            }

            if (currentValidationStage != context.VIS_DRAFY.MasterId)
            {
                var vpvs = new List<VisualProjectVersionModel>();
                vpvs.Add(new VisualProjectVersionModel()
                {
                    VisualProjectVersionId = context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId,
                    ValidationStageId = context.VIS_DRAFY.MasterId
                });
                context.VisualProject.VisualProjectVersionsData.ValidationStageId = context.VIS_DRAFY.MasterId;
                context.VisualizationRepository.VisualProjectVersionSaveValstage(vpvs);
            }

            vilommn(rule);
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.Edit.Action",
            description: "Create a copy of the VPV and change the validation stage",
            Type = RuleType.ProductionRule)]
        public void vivpvedac(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            if (context.VisualProjectVersionId !=
                context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId)
            {
                rule.SetResult(MVCHelper.RedirectToAction("VPEdit",
                new
                {
                    visualProjectId = context.VisualProject.VisualProjectId,
                    visualProjectVersionId = context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId
                }));
            }
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.Edit.SortMedias",
            description: "Sort medias",
            Type = RuleType.ProductionRule)]
        public void vivpedsm(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            context.VisualProject.VisualProjectVersionsData.VisualProjectMedia
                .Sort((x, y) =>
                {
                    var xValue = x.MediaOrder.HasValue ? x.MediaOrder.Value : x.MediaId;
                    var yValue = y.MediaOrder.HasValue ? y.MediaOrder.Value : y.MediaId;
                    return xValue.CompareTo(yValue);
                });
            var medias = context.VisualProject.VisualProjectVersionsData.VisualProjectMedia;
            for (int index = 0; index < medias.Count; index++)
            {
                medias[index].MediaOrder = index;
            }
        }
        #endregion

        #region Visualization.VP.Create

        [BusinessRule(
            ruleKey: "Visualization.VP.Create",
            description: "Creates empty obj with predifined values",
            Type = RuleType.ProductionRule)]
        public void vivpcr(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var vpv = new VisualProjectVersionModel()
            {
                ValidationStageId = context.VIS_DRAFY.MasterId,
                Modified = DateTime.Now,
                IsDeleted = false,
                IsValidated = false,
                ValidateDate = null,
                VisualProjectVersionId = -1,
                VisualProjectId = -1
            };
            context.VisualProject = new VisualProjectModel()
            {
                OperationId = context.OperationContext.OperationId,
                VisualProjectId = -1,
                VisualProjectVersionsData = vpv,
                VisualProjectVersions = new List<VisualProjectVersionModel>()
            };
            context.VisualProject.VisualProjectVersions.Add(vpv);
            context.VisualProjectVersionId = -1;
            context.VisualProjects.Add(context.VisualProject);
            context.ShowWarningMessageOnCancelEdition = false;
        }
        #endregion

        #region Visualizacion.VP.Delete
        [BusinessRule(
            ruleKey: "Visualization.VPV.Delete",
            description: "Delete a specific VPV",
            Type = RuleType.ProductionRule)]
        public void vivpde(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            context.VisualProject.VisualProjectVersionsData =
                context.VisualProject.VisualProjectVersions.First(vpv => vpv.VisualProjectVersionId ==
                    context.VisualProjectVersionId);

            if (context.VisualProject.VisualProjectVersionsData.ValidationStageId ==
                context.VIS_COO.MasterId)
            {
                IDBContext.Current.ErrorMessage(Localization.GetText("Visual project version ready for deletion"));
                context.VisualizationRepository.VisualProjectVersionSetDeleted(
                    context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId);
                rule.SetResult(MVCHelper.RedirectToAction("vpdetails"));
                return;
            }

            context.VisualizationRepository.VisualProjectVersionDelete(context.VisualProject.VisualProjectVersionsData.VisualProjectVersionId);
            IDBContext.Current.NotifyMessage(Localization.GetText("Item deleted successfully"));
            rule.SetResult(MVCHelper.RedirectToAction("vpdetails"));
        }

        #endregion

        #region Visualization.VP.Save
        [BusinessRule(
            ruleKey: "Visualization.VP.Save.VisualProjectVersion",
            description: "Save visual project information",
            Type = RuleType.ProductionRule)]
        public void vivpsavpv(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var model = context.ViewModel as VPVVModels;
            Mapper.CreateMap<VPVVModels, VisualProjectVersionModel>();
            Mapper.CreateMap<VMVmodel, VisualProjectMediaModel>();
            Mapper.CreateMap<DocumentViewModel, VisualProjectDocumentModel>();

            var vpv = Mapper.Map<VisualProjectVersionModel>(model);
            foreach (var media in vpv.VisualProjectMedia)
            {
                media.PermissionStatusId = MasterDefinitions
                    .GetMaster("PERMISSION_STATUS", "NO_NEED").MasterId;
            }

            //Architecture team: sorry for the comment guys, it's needed due to repeated misunderstandings
            //If saving an edited VPV, it can only be a new one or a draft one, so ValStage = VIS_DRAFY
            vpv.ValidationStageId = context.VIS_DRAFY.MasterId;

            context.VisualProject = context
                .VisualizationRepository
                .VisualProjectVersionSave(vpv, context.OperationContext.OperationId);
            vilovplv(rule);
        }
        #endregion

        #region Visualization.VP.TLCompleted

        [BusinessRule(
            ruleKey: "Visualization.VP.TLCompleted.IsEnable",
            description: "Verify if current VP is editable by the logged user",
            Type = RuleType.AndRuleSet)]

        public void vivptcie(RuleContext rule)
        {
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.TLCompleted.IsEnable.HasValidationStage",
            description: "Verify if current vpv has the required validation stage",
            Type = RuleType.LogicRule)]
        public void vivptciehv(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            rule.SetResult(context.VisualProject.VisualProjectVersionsData.ValidationStageId ==
                context.VIS_DRAFY.MasterId);
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.TLCompleted.IsEnable.HasPermission",
            description: "Verify if current vpv has the required validation stage",
            Type = RuleType.LogicRule)]
        public void vivptciehp(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            rule.SetResult(
                IDBContext.Current.HasPermission(Permission.VISUALIZATION_WRITE_TL));
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.TLCompleted.SendToMap",
            description: "Sends a Visual Project to be Validated by COO for Publishing into External Map",
            Type = RuleType.ProductionRule)]
        public void vivptcstm(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var visualProjectVersion = new VisualProjectVersionModel()
            {
                VisualProjectVersionId = context.VisualProjectVersionId.Value,
                ValidationStageId = context.operationDestination == SendToMapDestinationEnum.Internal ?
                    context.VIS_TL.MasterId : context.VIS_COO.MasterId
            };

            if (context.operationDestination == SendToMapDestinationEnum.Internal)
            {
                context.VisualizationRepository.VisualProjectVersionSendToMap(visualProjectVersion);
                rule.SetResult(true);

                return;
            }

            var paramsDictionary = new Dictionary<string, object>();
            var dataForK2FlowTag = new SendToMapK2DataModel
            {
                sourceScreen = context.operationOrigin,
                visualProjectVersionId = context.VisualProjectVersionId.Value
            };

            paramsDictionary.Add(K2IntegrationVisualizationHelper.Screen,
                context.operationOrigin.ToString());
            paramsDictionary.Add(K2IntegrationVisualizationHelper.TaskUserNameKey,
                (string)IDBContext.Current.UserName);
            paramsDictionary.Add(K2IntegrationVisualizationHelper.OperationName,
                context.OperationContext.CurrentOperationData.OperationNameEn);
            paramsDictionary.Add(K2IntegrationVisualizationHelper.TagCodeKey,
                JsonConvert.SerializeObject(dataForK2FlowTag, typeof(SendToMapK2DataModel), null));
            paramsDictionary.Add(K2IntegrationVisualizationHelper.EntityType, MAP_OPERATION_ENTITY);

            var k2result = _K2IntegrationVisualizationHelper.AdvanceWorkflow(
                K2IntegrationVisualizationHelper.WorkflowCode,
                string.Empty,
                context.OperationContext.OperationNumber,
                context.VisualProjectVersionId.Value,
                K2IntegrationVisualizationHelper.K2Actions.StartWorkflow,
                paramsDictionary);

            if (k2result)
                context.VisualizationRepository.VisualProjectVersionSendToMap(visualProjectVersion);

            rule.SetResult(k2result);
        }

        [BusinessRule(
            ruleKey: "Visualization.VP.TLCompleted.RemoveFromMap",
            description: "Return the validation stage of the vpv to VIS_DRAFY",
            Type = RuleType.ProductionRule)]
        public void vivpir(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var vpvs = new List<VisualProjectVersionModel>();

            vpvs.Add(new VisualProjectVersionModel()
            {
                VisualProjectVersionId = context.VisualProjectVersionId.Value,
                IsValidated = false,
                ValidateDate = null,
                ValidationStageId = context.operationDestination == SendToMapDestinationEnum.External ?
                    context.VIS_TL.MasterId : context.VIS_DRAFY.MasterId
            });

            context.VisualizationRepository.VisualProjectVersionRemoveFromMap(vpvs);
            rule.SetResult(true);
        }

        #endregion

        #region Visualization.VO.Load
        [BusinessRule(
            ruleKey: "Visualization.VO.Load",
            description: "Calculate (actual mapped, EopTarget and ActualMapped)",
            Type = RuleType.ProductionRule)]
        public void vivolo(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var vovs = context
                .VisualOutputs
                .Select(vo1 => vo1.VisualOutputVersionsData);
            var outputs = context
                .ResultsMatrixContext
                .Current
                .Components
                .SelectMany(cp => cp.Outputs);
            var vo = context
                .VisualOutputs
                .FirstOrDefault(vo1 => vo1.VisualOutputId == context.VisualOutputId);
            var vov = vo.VisualOutputVersionsData;
            var output = outputs
                .FirstOrDefault(otp => otp.OutputId ==
                    vo.VisualOutputVersionsData.OutputYearPlan.OutputId);

            var oyp = output
                .OutputYearPlans
                .FirstOrDefault(oy => oy.Year == -1 && oy.IsCost == false);
            if (oyp != null)
            {
                vo.VisualOutputVersionsData.EopTarget = oyp.ActualValue;
                vo.VisualOutputVersionsData.EopTarget = vo.VisualOutputVersionsData.EopTarget ?? 0;
            }

            vov.ActualMapped = vovs.Where(vov1 => vov1.OutputYearPlan.OutputId == output.OutputId &&
                vov1.OutputYearPlan.Year == vov.OutputYearPlan.Year &&
                vov1.OutputUnits != null)
                .Sum(vov1 => vov1.OutputUnits);
            context.VisualOutputs.RemoveAll(vo1 => vo1.VisualOutputId != context.VisualOutputId);
        }
        #endregion

        #region Visualization.VO.Save
        [BusinessRule(
            ruleKey: "Visualization.VO.Save.MappedUnits",
            description: "Stores visual output information",
            Type = RuleType.ProductionRule,
            Priority = 1)]
        public void vivosamu(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var model = context.ViewModel as VOVVModel;
            decimal outputUnits = 0;
            var year = context
                .VisualOutput
                .VisualOutputVersionsData
                .OutputYearPlan
                .Year;
            foreach (var vo in context.VisualOutputs)
            {
                if (vo.VisualOutputVersionsData != null &&
                    vo.VisualOutputVersionsData.OutputYearPlan.Year == year &&
                    vo.VisualOutputVersionsData.OutputUnits.HasValue)
                {
                    outputUnits += vo.VisualOutputVersionsData.OutputUnits.Value;
                }
            }

            context
                .VisualizationRepository
                .OutputYearVisualizationUpdateVOs(
                    context.VisualOutput.VisualOutputVersionsData.OutputYearPlan.OutputId,
                    outputUnits,
                    year);
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.Save.VisualOutputVersion",
            description: "Stores visual output information",
            Type = RuleType.ProductionRule,
            Priority = 0)]
        public void vivosavov(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var model = context.ViewModel as VOVVModel;
            Mapper.CreateMap<VOVVModel, VisualOutputVersionModel>().ForMember(x => x.ValidationStage, opt => opt.Ignore());
            Mapper.CreateMap<VMVmodel, VisualOutputMediaModel>();
            Mapper.CreateMap<DocumentViewModel, VisualOutputDocumentModel>();

            var vov = Mapper.Map<VisualOutputVersionModel>(model);

            //Architecture team: sorry for the comment guys, it's needed due to repeated misunderstandings
            //If saving an edited VOV, it can only be a new one or a draft one, so ValStage = VIS_DRAFY
            vov.ValidationStageId = context.VIS_DRAFY.MasterId;

            foreach (var media in vov.VisualOutputMedia)
            {
                media.PermissionStatusId = MasterDefinitions
                    .GetMaster("PERMISSION_STATUS", "NO_NEED").MasterId;
            }

            if (vov.VisualOutputId == -1)
                vov.operationId = context.OperationContext.OperationId;

            context.VisualOutput = context.VisualizationRepository.VisualOutputVersionSave(vov);

            var vo = context.VisualOutputs
                .FirstOrDefault(vo0 => vo0.VisualOutputId == context.VisualOutput.VisualOutputId);

            if (vo != null)
                context.VisualOutputs[context.VisualOutputs.IndexOf(vo)] = context.VisualOutput;
            else
                context.VisualOutputs.Add(context.VisualOutput);

            vilovosv(rule);
            context.VisualOutputId = context.VisualOutput.VisualOutputId;
        }

        #endregion

        #region Visualization.VO.IsEditable
        [BusinessRule(
            ruleKey: "Visualization.VO.IsEditable",
            description: "Verify if current VO is editable by the logged user",
            Type = RuleType.OrRuleSet)]
        public void vivoie(RuleContext rule)
        {
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.IsEditable.HasPermission",
            description: "Calculate (actual mapped, EopTarget and ActualMapped)",
            Type = RuleType.LogicRule,
            ExecutionConditions = ExecutionCondition.MustBeTrue)]
        public void vivoiehp(RuleContext rule)
        {
            rule.SetResult(IDBContext.Current.HasPermission(Permission.VISUALIZATION_WRITE));
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.IsEditable.TLValidated",
            description: "Disable the edit button when the state is validated and the user is not TL",
            Type = RuleType.LogicRule,
            ExecutionConditions = ExecutionCondition.MustBeTrue)]
        public void vivoietlv(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var result = true;
            if (context.VisualOutput.VisualOutputVersionsData.ValidationStageId ==
                context.VIS_TL.MasterId &&
                !IDBContext.Current.HasPermission(Permission.VISUALIZATION_WRITE_TL))
            {
                result = false;
            }

            rule.SetResult(result);
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.IsEditable.Action",
            description: "Create edit destination path",
            Type = RuleType.ProductionRule)]
        public void vivoieac(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var action = context.Url.Action(
                "VOEdit",
                new
                {
                    visualOutputId = context.VisualOutputId,
                    OperationNumber = IDBContext.Current.Operation
                });
            
            rule.SetResult(action);
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.IsEditable.Confirmation",
            description: "Show visualization confirmation when the current VO is validated",
            Type = RuleType.ProductionRule)]
        public void vivoieco(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var vov = context.VisualOutput.VisualOutputVersionsData;

            if (vov.ValidationStageId == context.VIS_TL.MasterId)
                rule.SetResult(string.Format(
                    Localization.GetText("VIS.Current.VO.Already.Validated.Message"),
                    Localization.GetText("Team Leader")));
            else if (vov.ValidationStageId == context.VIS_COO.MasterId)
                rule.SetResult(string.Format(
                    Localization.GetText("VIS.Current.VO.Already.Validated.Message"),
                    Localization.GetText("Chief of Operations")));
        }

        #endregion

        #region Visualization.VO.Edit
        [BusinessRule(
            ruleKey: "Visualization.VO.Edit.Clone",
            description: "Verify if current VO is validated, so create a draft copy",
            Type = RuleType.LogicRule)]
        public void vivoedcl(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var vo = context.VisualOutput;

            if (vo.VisualOutputVersionsData.ValidationStageId != context.VIS_DRAFY.MasterId)
            {
                context.ShowWarningMessageOnCancelEdition = true;
                var newVOV = context.VisualizationRepository.VisualOutputVersionClone(
                    vo.VisualOutputVersionsData.VisualOutputVersionId, context.VIS_DRAFY.MasterId);

                vilohvo(rule);
                vilovosv(rule);
                viloavpsm(rule);
                vilommn(rule);
            }

            rule.SetResult(true);
        }
        #endregion

        #region Visualizacion.VO.Delete
        [BusinessRule(
            ruleKey: "Visualization.VOV.Delete",
            description: "Delete a specific VOV",
            Type = RuleType.ProductionRule)]
        public void vivode(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();

            context.VisualOutput = context.VisualOutputs.FirstOrDefault(vo => vo.VisualOutputVersions
                .Any(vov => vov.VisualOutputVersionId == context.VisualOutputVersionId));
            Validator.EntityNotFound(
                context.VisualOutput,
                "Visual output with output version id",
                context.VisualOutputVersionId);

            context.VisualOutput.VisualOutputVersionsData = context.VisualOutput.VisualOutputVersions
                .First(vpv => vpv.VisualOutputVersionId == context.VisualOutputVersionId);
            context.VisualizationRepository.VisualOutputVersionSetDeleted(
                context.VisualOutput.VisualOutputVersionsData.VisualOutputVersionId);

            rule.SetResult(MVCHelper.RedirectToAction("grid"));
            IDBContext.Current.NotifyMessage(Localization.GetText("Item deleted successfully"));
        }

        #endregion

        #region Visualization.VO.TLCompleted

        [BusinessRule(
            ruleKey: "Visualization.VO.TLCompleted.IsEnable",
            description: "Verify if current VO is editable by the logged user",
            Type = RuleType.AndRuleSet)]
        public void vivotcie(RuleContext rule)
        {
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.TLCompleted.IsEnable.HasValidationStage",
            description: "Verify if current vov has the required validation stage",
            Type = RuleType.LogicRule)]
        public void vivotciehv(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            rule.SetResult(context.VisualOutput.VisualOutputVersionsData.ValidationStageId ==
                context.VIS_DRAFY.MasterId);
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.TLCompleted.IsEnable.HasPermission",
            description: "Verify if current vov has the required validation stage",
            Type = RuleType.LogicRule)]
        public void vivotciehp(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            rule.SetResult(
                IDBContext.Current.HasPermission(Permission.VISUALIZATION_WRITE_TL));
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.TLCompleted.SendToMap",
            description: "Sends Visual Outputs to be Validated by COO for Publishing into External Map",
            Type = RuleType.ProductionRule)]
        public void vivotcstm(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var vovs = new List<VisualOutputVersionModel>();
            var lowestVovId = int.MaxValue;

            foreach (var vo in context.VisualOutputs)
            {
                var visualOutputVersionsData = context.VisualizationRepository
                    .VisualOutputVersionGet(vo.VisualOutputVersionId);

                if (visualOutputVersionsData.ValidationStageId == context.VIS_COO.MasterId &&
                    !visualOutputVersionsData.IsValidated)
                    continue;

                visualOutputVersionsData.IsCompleteToPublish = true;

                if (context.operationType.Equals(SendToMapOperTypeEnum.SaveAndValidate))
                {
                    visualOutputVersionsData.ValidationStageId =
                        context.operationDestination == SendToMapDestinationEnum.External ?
                            context.VIS_COO.MasterId :
                            (context.operationDestination == SendToMapDestinationEnum.Internal ?
                                context.VIS_TL.MasterId :
                                context.VIS_DRAFY.MasterId);
                    visualOutputVersionsData.StatusSend = (int)SendToMapDestinationEnum.None;
                    visualOutputVersionsData.StatusRemove = (int)SendToMapDestinationEnum.None;
                    visualOutputVersionsData.StatusValidation = (int)SendToMapDestinationEnum.None;

                    if (visualOutputVersionsData.VisualOutputVersionId < lowestVovId)
                        lowestVovId = visualOutputVersionsData.VisualOutputVersionId;
                }
                else
                    visualOutputVersionsData.StatusSend = (int)context.operationDestination;

                vovs.Add(visualOutputVersionsData);
            }

            if (!vovs.Any())
                return;

            if (context.operationType != SendToMapOperTypeEnum.SaveAndValidate
                || context.operationDestination != SendToMapDestinationEnum.External)
            {
                context.VisualizationRepository.VisualOutputVersionSendToMap(vovs);
                rule.SetResult(true);

                return;
            }

            var paramsDictionary = new Dictionary<string, object>();
            var dataForK2FlowTag = new SendToMapK2DataModel
                {
                    sourceScreen = context.operationOrigin,
                    visualOutputVersionIds = context.VisualOutputs.Select(x => x.VisualOutputVersionId)
                };
                        bool k2result = false;

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);
            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);

            Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - cycloOperation: " + cycleOperation);
            if (cycleOperation.Equals("TCM"))
            {
                string userName = IDBContext.Current.UserName;
                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - userName: " + userName);

                int entityId = lowestVovId;
                paramsDictionary.Add(K2TCMHelpers.EntityId, Convert.ToString(entityId));
                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - entityId: " + Convert.ToString(entityId));

                paramsDictionary.Add(K2TCMHelpers.GeneralTag, JsonConvert.SerializeObject(dataForK2FlowTag, typeof(SendToMapK2DataModel), null));

                string operationNumber = IDBContext.Current.Operation;
                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - operationNumber: " + operationNumber);

                var sharepointUrl = ConfigurationManager.AppSettings["BasePath"];
                var linkOpera = string.Format("{0}/operation/{1}/Pages/Default?idTask=nro", sharepointUrl, operationNumber);
                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - linkOpera: " + linkOpera);
                paramsDictionary.Add(K2TCMHelpers.LinkTask, linkOpera);

                string fundId = string.Empty;
                string fcUserName = string.Empty;
                string fundCode = string.Empty;

                var operation = _operationRepository
                    .GetOne(o => o.OperationNumber == operationNumber);

                var fundOperations = _fundOperationRepository
                    .GetByCriteria(_operationHistoryService.BuildFundOperationExpr(operation))
                    .ToList();

                foreach (var fund in fundOperations)
                {
                    var FundUserInfo = _fundsSharepointService.GetFundCoordinatorByCode(fund.Fund.FundCode);
                    if (!string.IsNullOrEmpty(fundId))
                    {
                        fundId += ",";
                        fundCode += ",";
                        fcUserName += ",";
                    }

                    fundId += fund.Fund.FundId.ToString();
                    fundCode += fund.Fund.FundCode;
                    fcUserName += FundUserInfo != null ? FundUserInfo.LoginName : string.Empty;
                }

                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - IdsFund: " + Convert.ToString(fundId));
                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - CodeFund: " + Convert.ToString(fundCode));
                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - FundUsers_Temp: " + Convert.ToString(fcUserName));

                var operationNameResponse = context.OperationContext.CurrentOperationData.OperationNameEn;

                paramsDictionary.Add(K2TCMHelpers.OperationName, operationNameResponse);
                paramsDictionary.Add(K2TCMHelpers.UserStart, userName);
                paramsDictionary.Add(K2TCMHelpers.IdsFund, fundId);
                paramsDictionary.Add(K2TCMHelpers.CodeFund, fundCode);
                paramsDictionary.Add(K2TCMHelpers.FundUsers_Temp, fcUserName);

                try
                {
                    k2result = _k2IntegrationService.StartAdvanceWorkflowTCM(K2TCMHelpers.WorkflowTypeTCM2,
                        "0",
                        operationNumber,
                        K2TCMHelpers.EntityTypeT2,
                        entityId,
                        null,
                        paramsDictionary,
                        K2IntegrationEnumerator.GeneralActions.StartWorkflow,
                        0);
                }
                catch (Exception e)
                {
                    Logger.GetLogger().WriteError("VisualizationRules", e.Message, e);
                }

                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivotcstm - Final");
            }
            else
            {
                paramsDictionary.Add(K2IntegrationVisualizationHelper.Screen,
                    context.operationOrigin.ToString());
                paramsDictionary.Add(K2IntegrationVisualizationHelper.TaskUserNameKey,
                    (string)IDBContext.Current.UserName);
                paramsDictionary.Add(K2IntegrationVisualizationHelper.OperationName,
                    context.OperationContext.CurrentOperationData.OperationNameEn);
                paramsDictionary.Add(K2IntegrationVisualizationHelper.TagCodeKey,
                    JsonConvert.SerializeObject(dataForK2FlowTag, typeof(SendToMapK2DataModel), null));
                paramsDictionary.Add(K2IntegrationVisualizationHelper.EntityType, VISUAL_OUTPUT_ENTITY);

                k2result = _K2IntegrationVisualizationHelper.AdvanceWorkflow(
                    K2IntegrationVisualizationHelper.WorkflowCode,
                    string.Empty,
                    context.OperationContext.OperationNumber,
                    lowestVovId,
                    K2IntegrationVisualizationHelper.K2Actions.StartWorkflow,
                    paramsDictionary,
                    0,
                    null);
            }

            if (k2result)
                context.VisualizationRepository.VisualOutputVersionSendToMap(vovs);

            rule.SetResult(k2result);
        }

        [BusinessRule(
            ruleKey: "Visualization.VO.TLCompleted.RemoveFromMap",
            description: "Changes the status of selected VOVs being removed to maps",
            Type = RuleType.ProductionRule)]
        public void vivotcrfm(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var vovs = new List<VisualOutputVersionModel>();

            foreach (var vo in context.VisualOutputs)
            {
                var visualOutputVersionsData = context.VisualizationRepository
                    .VisualOutputVersionGet(vo.VisualOutputVersionId);

                if (visualOutputVersionsData.ValidationStageId == context.VIS_COO.MasterId &&
                    !visualOutputVersionsData.IsValidated)
                    continue;

                if (context.operationType.Equals(SendToMapOperTypeEnum.SaveAndValidate))
                {
                    visualOutputVersionsData.ValidationStageId =
                        (context.operationDestination == SendToMapDestinationEnum.External) ?
                            context.VIS_TL.MasterId :
                            (context.operationDestination == SendToMapDestinationEnum.Internal ?
                                context.VIS_DRAFY.MasterId :
                                visualOutputVersionsData.ValidationStageId);
                    visualOutputVersionsData.IsValidated =
                        context.operationDestination == SendToMapDestinationEnum.None ?
                            visualOutputVersionsData.IsValidated :
                            false;
                    visualOutputVersionsData.ValidateDate =
                        context.operationDestination == SendToMapDestinationEnum.None ?
                            visualOutputVersionsData.ValidateDate :
                            null;
                    visualOutputVersionsData.StatusRemove = (int)SendToMapDestinationEnum.None;
                }
                else
                    visualOutputVersionsData.StatusRemove = (int)context.operationDestination;

                vovs.Add(visualOutputVersionsData);
            }

            if (!vovs.Any())
                return;

            context.VisualizationRepository.VisualOutputVersionRemoveFromMap(vovs);
            rule.SetResult(true);
        }

        #endregion

        #region Visualization.VO.Create
        [BusinessRule(
            ruleKey: "Visualization.VO.Create",
            description: "Create the empty entities required to create a new VO",
            Type = RuleType.LogicRule)]
        public void vivocr(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            context.VisualOutput = new VisualOutputModel()
            {
                VisualOutputId = -1,
                VisualOutputVersionsData = new VisualOutputVersionModel()
                {
                    VisualOutputVersionId = -1,
                    ValidationStageId = context.VIS_DRAFY.MasterId,
                    OutputYearPlan = new OutputYearPlanModel()
                    {
                        OutputId = context.OutputId,
                        OutputYearPlanId = -1
                    }
                }
            };
            context.ShowWarningMessageOnCancelEdition = false;
        }
        #endregion

        #region Visualization.External.Map.COO.Approval
        [BusinessRule(
            ruleKey: "Visualization.External.Map.COO.Approval",
            description: "Approves or Rejects the publication to External Map of MO/VOVs associated to a K2 flow",
            Type = RuleType.ProductionRule)]
        public void vivocoovd(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var GlobalModelRepository = new GlobalModelRepository();
            var taskModel = GlobalModelRepository.GetSpecificTask(context.sendToMapTaskId, 
                IDBContext.Current.CurrentLanguage);
            var taskTag = JsonConvert.DeserializeObject<SendToMapK2DataModel>(taskModel.TaskTag);

            if (taskTag.sourceScreen.Equals(SendToMapSourceEnum.Visual))
            {
                var vovs = new List<VisualOutputVersionModel>();
                var voListFork2 = string.Empty;
                var commentsList = new List<VisualizationCommentModel>();
                var lowestVovId = int.MaxValue;

                foreach (var k2FlowItem in context.sendToMapK2FlowValidationModels)
                {
                    var vov = context.VisualizationRepository
                        .VisualOutputVersionGet(k2FlowItem.visualDataVersionId);
                    vov.IsCompleteToPublish = true;
                    vov.StatusSend = (int)SendToMapDestinationEnum.None;
                    vov.StatusRemove = (int)SendToMapDestinationEnum.None;

                    var comment = new VisualizationCommentModel()
                    {
                        EntityId = k2FlowItem.visualDataVersionId,
                        EntityType = VisualEntityTypes.VisualOutputVersion,
                        Text = string.IsNullOrEmpty(k2FlowItem.voComment) ?
                            NO_COMMENTS_ADDDED : k2FlowItem.voComment
                    };

                    if (context.operationType == SendToMapOperTypeEnum.Save)
                    {
                        vov.ValidationStageId = context.VIS_COO.MasterId;
                        vov.IsValidated = false;
                        vov.ValidateDate = null;
                        vov.StatusValidation = (int)k2FlowItem.k2OperationType;
                    }
                    else
                    {
                        if (k2FlowItem.k2OperationType == SendToMapK2OperTypeEnum.Approved)
                        {
                            vov.ValidationStageId = context.VIS_COO.MasterId;
                            vov.IsValidated = true;
                            vov.ValidateDate = DateTime.Now;
                            vov.StatusValidation = (int)SendToMapDestinationEnum.None;

                            var vo = context.VisualizationRepository.VisualOutputGet(vov.VisualOutputId);

                            foreach (var vovPrevious in vo.VisualOutputVersions
                                .Where(x => x.VisualOutputVersionId != k2FlowItem.visualDataVersionId))
                            {
                                vovPrevious.ValidationStageId = context.VIS_DRAFY.MasterId;
                                vovPrevious.IsValidated = false;
                                vovPrevious.ValidateDate = null;

                                vovs.Add(vovPrevious);
                            }
                        }
                        else
                        {
                            vov.ValidationStageId = context.VIS_TL.MasterId;
                            vov.IsValidated = false;
                            vov.ValidateDate = null;
                            vov.StatusValidation = (int)SendToMapDestinationEnum.None;
                        }

                        if (vov.VisualOutputVersionId < lowestVovId)
                            lowestVovId = vov.VisualOutputVersionId;
                    }

                    voListFork2 = string.Join(
                        " ",
                        voListFork2,
                        k2FlowItem.k2OperationType,
                        vov.VisualOutputVersionId,
                        vov.NameEn,
                        comment.Text,
                        ",");

                    vovs.Add(vov);

                    if (!comment.Text.Equals(NO_COMMENTS_ADDDED))
                        commentsList.Add(comment);
                }

                if (context.operationType == SendToMapOperTypeEnum.SaveAndValidate)
                {
                    var k2Action = context.sendToMapK2FlowValidationModels
                        .Any(x => x.k2OperationType == SendToMapK2OperTypeEnum.Approved) ?
                            K2IntegrationVisualizationHelper.K2Actions.Approved :
                            K2IntegrationVisualizationHelper.K2Actions.Rejected;

                    var paramsDictionary = new Dictionary<string, object>();
                    paramsDictionary.Add(K2IntegrationVisualizationHelper.Screen,
                        taskTag.sourceScreen.ToString());
                    paramsDictionary.Add(K2IntegrationVisualizationHelper.TaskUserNameKey,
                        (string)IDBContext.Current.UserName);
                    paramsDictionary.Add(K2IntegrationVisualizationHelper.List, voListFork2);
                    paramsDictionary.Add(K2IntegrationVisualizationHelper.EntityType, VISUAL_OUTPUT_ENTITY);

                    var k2result = _K2IntegrationVisualizationHelper
                        .AdvanceWorkflow(
                            K2IntegrationVisualizationHelper.WorkflowCode,
                            taskModel.TaskFolio,
                            context.OperationContext.OperationNumber,
                            lowestVovId,
                            k2Action,
                            paramsDictionary,
                            taskModel.TaskId,
                            k2Action.ToString());

                    if (k2result)
                    {
                        context.VisualizationRepository.VisualOutputVersionSendToMap(vovs);
                        SaveVisualDataComments(context, commentsList);
                    }

                    rule.SetResult(k2result);
                }
                else
                {
                    context.VisualizationRepository.VisualOutputVersionSendToMap(vovs);
                    SaveVisualDataComments(context, commentsList);
                    rule.SetResult(true);
                }
            }
            else
            {
                var vpvs = new List<VisualProjectVersionModel>();
                K2IntegrationVisualizationHelper.K2Actions k2Action;
                var k2FlowItem = context.sendToMapK2FlowValidationModels.First();
                var vpv = context.VisualizationRepository
                    .VisualProjectVersionGet(k2FlowItem.visualDataVersionId);
                var comment = new VisualizationCommentModel()
                {
                    EntityId = k2FlowItem.visualDataVersionId,
                    EntityType = VisualEntityTypes.VisualProjectVersion,
                    Text = string.IsNullOrEmpty(k2FlowItem.voComment) ?
                        NO_COMMENTS_ADDDED : k2FlowItem.voComment
                };

                if (context.operationType == SendToMapOperTypeEnum.SaveAndValidate)
                {
                    k2Action = K2IntegrationVisualizationHelper.K2Actions.Approved;
                    vpv.ValidationStageId = context.VIS_COO.MasterId;
                    vpv.IsValidated = true;
                    vpv.ValidateDate = DateTime.Now;

                    var vp = context.VisualizationRepository.VisualProjectGet(vpv.VisualProjectId);

                    foreach (var vpvPrevious in vp.VisualProjectVersions
                        .Where(x => x.VisualProjectVersionId != k2FlowItem.visualDataVersionId))
                    {
                        vpvPrevious.ValidationStageId = context.VIS_DRAFY.MasterId;
                        vpvPrevious.IsValidated = false;
                        vpvPrevious.ValidateDate = null;

                        vpvs.Add(vpvPrevious);
                    }
                }
                else
                {
                    k2Action = K2IntegrationVisualizationHelper.K2Actions.Rejected;
                    vpv.ValidationStageId = context.VIS_TL.MasterId;
                    vpv.IsValidated = false;
                    vpv.ValidateDate = null;
                }

                vpvs.Add(vpv);

                var vpListFork2 = string.Join(
                    " ",
                    k2Action,
                    vpv.VisualProjectVersionId,
                    "Map Operation",
                    comment.Text);

                var paramsDictionary = new Dictionary<string, object>();
                paramsDictionary.Add(K2IntegrationVisualizationHelper.Screen,
                    taskTag.sourceScreen.ToString());
                paramsDictionary.Add(K2IntegrationVisualizationHelper.TaskUserNameKey,
                    (string)IDBContext.Current.UserName);
                paramsDictionary.Add(K2IntegrationVisualizationHelper.List, vpListFork2);
                paramsDictionary.Add(K2IntegrationVisualizationHelper.EntityType, MAP_OPERATION_ENTITY);

                var k2result = _K2IntegrationVisualizationHelper
                    .AdvanceWorkflow(K2IntegrationVisualizationHelper.WorkflowCode,
                        taskModel.TaskFolio,
                        context.OperationContext.OperationNumber,
                        vpv.VisualProjectVersionId,
                        k2Action,
                        paramsDictionary,
                        taskModel.TaskId,
                        k2Action.ToString());

                if (k2result)
                {
                    context.VisualizationRepository.VisualProjectVersionsSendToMap(vpvs);

                    if (!comment.Text.Equals(NO_COMMENTS_ADDDED))
                        context.VisualizationRepository.SaveComment(comment);

                    rule.SetResult(true);
                    return;
                }

                Logger.GetLogger().WriteDebug("VisualizationRules", string.Format(
                    "Error trying to advance K2 workflow for VP Approval/rejection (VisualProjectId : {0})",
                    vpv.VisualProjectId));

                rule.SetResult(false);
            }
        }
        #endregion

        #region Visualization.MapUpdate
        [BusinessRule(
            ruleKey: "Visualization.Map.VOUpdateLevel",
            description: "Update the level of current visual output")]
        public void vimavoup(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            string level1 = IDBContext.Current.RequestValue(RequestValues.LEVEL1);
            string level2 = IDBContext.Current.RequestValue(RequestValues.LEVEL2);
            string level3 = IDBContext.Current.RequestValue(RequestValues.LEVEL3);
            int visualOutputVersionId = int.Parse(
                IDBContext.Current.RequestValue(RequestValues.VISUAL_OUTPUT_VERSION_ID));
            context
                .VisualizationRepository
                .VisualOutputVersionUpdateLevel(
                    visualOutputVersionId,
                    level1,
                    level2,
                    level3);
        }

        [BusinessRule(
            ruleKey: "Visualization.Map.VPUpdateLevel",
            description: "Update the level of current visual project")]
        public void vimavpup(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            string level1 = IDBContext.Current.RequestValue(RequestValues.LEVEL1);
            string level2 = IDBContext.Current.RequestValue(RequestValues.LEVEL2);
            string level3 = IDBContext.Current.RequestValue(RequestValues.LEVEL3);
            int visualProjectVersionId = int.Parse(
                IDBContext.Current.RequestValue(RequestValues.VISUAL_PROJECT_VERSION_ID));

            context
                .VisualizationRepository
                .VisualProjectVersionUpdateLevel(
                    visualProjectVersionId,
                    level1,
                    level2,
                    level3);
        }
        #endregion

        #region Visualization.MapsVisualization.Load

        [BusinessRule(
        ruleKey: "Visualization.MapsVisualization.Load",
        description: "Load Visual Outputs and refer to their parent Output ",
        Type = RuleType.ProductionRule,
        Priority = -1)]
        public void vimvlo(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            context.ResultsMatrixContext.Execute("ResultsMatrix.Outputs.Load");
            var components = context.ResultsMatrixContext.Current.Components;
            components.Sort((x, y) =>
            {
                return x.OrderNumber.CompareTo(y.OrderNumber);
            });

            components.ForEach((component) =>
            {
                component.Outputs.RemoveAll(otp => otp.IsDeactivated);
                component.Outputs.ForEach((output) =>
                {
                    output.OutputYearPlans.RemoveAll(oyp =>
                        oyp.IsCost || oyp.Year <= 0);

                    output.OutputYearPlans.Sort((x, y) =>
                    {
                        return x.Year.CompareTo(y.Year);
                    });
                });

                component.Outputs.RemoveAll(op => !op.OutputYearPlans.Any());
            });

            components.RemoveAll(cp => !cp.Outputs.Any());
        }

        #endregion

        #region Visualization.External.Validate.TCM
        [BusinessRule(
            ruleKey: "Visualization.External.Validate.TCM",
            description: "Approves or Rejects the publication to External Map of VOVs associated to a K2 flow for TCM",
            Type = RuleType.ProductionRule)]
        public void vivocoovdTCM(RuleContext rule)
        {
            var context = rule.TContext<VisualizationBusinessContext>();
            var GlobalModelRepository = new GlobalModelRepository();
            var taskModel = GlobalModelRepository.GetSpecificTask(context.sendToMapTaskId,
                IDBContext.Current.CurrentLanguage);
            var taskTag = JsonConvert.DeserializeObject<SendToMapK2DataModelTCM>(taskModel.TaskTag);

            ResponseBase k2result = new ResponseBase();
            string operationNumber = IDBContext.Current.Operation;

            if (taskTag.sourceScreen.Equals(SendToMapSourceEnum.Visual))
            {
                var vovs = new List<VisualOutputVersionModel>();
                var voListFork2 = string.Empty;
                var commentsList = new List<VisualizationCommentModel>();
                var lowestVovId = int.MaxValue;

                //Save
                if (context.operationType == SendToMapOperTypeEnum.Save)
                {
                    foreach (var k2FlowItem in context.sendToMapK2FlowValidationModels)
                    {
                        var vov = context.VisualizationRepository.VisualOutputVersionGet(k2FlowItem.visualDataVersionId);

                        var comment = new VisualizationCommentModel()
                        {
                            EntityId = k2FlowItem.visualDataVersionId,
                            EntityType = VisualEntityTypes.VisualOutputVersion,
                            Text = string.IsNullOrEmpty(k2FlowItem.voComment) ? NO_COMMENTS_ADDDED : k2FlowItem.voComment
                        };
                        Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - Save - visualOutputVersionId: " +
                                                                k2FlowItem.visualDataVersionId);
                        Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - Save - ValidationStageId: " + 
                                                                context.TCM_VIS_FC.Name);
                        vov.ValidationStageId = context.TCM_VIS_FC.MasterId;
                        vov.IsValidated = false;
                        vov.ValidateDate = null;
                        vov.StatusValidation = (int)k2FlowItem.k2OperationType;

                        vovs.Add(vov);

                        if (!comment.Text.Equals(NO_COMMENTS_ADDDED))
                            commentsList.Add(comment);
                    }

                    context.VisualizationRepository.VisualOutputVersionSendToMap(vovs);
                    SaveVisualDataComments(context, commentsList);
                    rule.SetResult(true);
                }
                else 
                {
                    //Accept or Reject
                    var parameters = new Dictionary<string, object>();
                    string userName = IDBContext.Current.UserName;
                    Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - userName: " + userName);
                    parameters.Add(K2TCMHelpers.TaskUserName, userName);

                    foreach (var k2FlowItem in context.sendToMapK2FlowValidationModels)
                    {
                        var vov = context.VisualizationRepository.VisualOutputVersionGet(k2FlowItem.visualDataVersionId);
                        if (vov.VisualOutputVersionId < lowestVovId)
                            lowestVovId = vov.VisualOutputVersionId;
                    }

                    int entityId = Convert.ToInt32(lowestVovId);
                    Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - entityId: " + entityId);

                    string serialNro = taskTag.SerialNro;
                    Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - serialNro: " + serialNro);
                    parameters.Add(K2TCMHelpers.SerialNro, serialNro);

                    Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - TaskRol: " + taskModel.UserProfile);
                    parameters.Add(K2TCMHelpers.TaskRole, taskModel.UserProfile);

                    var workFlow = (context.operationType == SendToMapOperTypeEnum.SaveAndValidate) ? ACCEPT_WORKFLOW : REJECT_WORKFLOW;
                    Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - workFlow: " + workFlow);
                    switch (workFlow)
                    {
                        case ACCEPT_WORKFLOW:

                            k2result.IsValid = _k2IntegrationService.StartAdvanceWorkflowTCM(taskModel.Code,
                                taskModel.TaskFolio,
                                operationNumber,
                                K2TCMHelpers.EntityTypeT2,
                                entityId,
                                taskModel.TaskTypeCode,
                                parameters,
                                K2IntegrationEnumerator.GeneralActions.AcceptWorkflow,
                                taskModel.TaskId);

                            if (k2result.IsValid)
                            {
                                var result = _k2IntegrationService.IsTaskAccepted(taskModel.WorkflowInstanceId);
                                Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - isAllTasksAccepted: " + result.isAllTasksAccepted);

                                //outpus
                                foreach (var k2FlowItem in context.sendToMapK2FlowValidationModels)
                                {                                  
                                    var comment = new VisualizationCommentModel()
                                    {
                                        EntityId = k2FlowItem.visualDataVersionId,
                                        EntityType = VisualEntityTypes.VisualOutputVersion,
                                        Text = string.IsNullOrEmpty(k2FlowItem.voComment) ? NO_COMMENTS_ADDDED : k2FlowItem.voComment
                                    };

                                    if (!comment.Text.Equals(NO_COMMENTS_ADDDED))
                                        commentsList.Add(comment);

                                    if (result.isAllTasksAccepted)
                                    {
                                        var vov = context.VisualizationRepository.VisualOutputVersionGet(k2FlowItem.visualDataVersionId);
                                        vov.IsCompleteToPublish = true;
                                        vov.StatusSend = (int)SendToMapDestinationEnum.None;
                                        vov.StatusRemove = (int)SendToMapDestinationEnum.None;

                                        //Approved
                                        Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - Approved - visualOutputVersionId: " +
                                                                k2FlowItem.visualDataVersionId);
                                        Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - Approved - ValidationStageId: " +
                                                                context.TCM_VIS_FC.Name);
                                        vov.ValidationStageId = context.TCM_VIS_FC.MasterId;
                                        vov.IsValidated = true;
                                        vov.ValidateDate = DateTime.Now;
                                        vov.StatusValidation = (int)SendToMapDestinationEnum.None;

                                        var vo = context.VisualizationRepository.VisualOutputGet(vov.VisualOutputId);

                                        foreach (var vovPrevious in vo.VisualOutputVersions.Where(x => x.VisualOutputVersionId != k2FlowItem.visualDataVersionId))
                                        {
                                            vovPrevious.ValidationStageId = context.VIS_DRAFY.MasterId;
                                            vovPrevious.IsValidated = false;
                                            vovPrevious.ValidateDate = null;

                                            vovs.Add(vovPrevious);
                                        }

                                        vovs.Add(vov);
                                    }
                                }

                                context.VisualizationRepository.VisualOutputVersionSendToMap(vovs);
                                SaveVisualDataComments(context, commentsList);

                                rule.SetResult(true);
                            }

                            break;

                        case REJECT_WORKFLOW:

                            k2result.IsValid = _k2IntegrationService.StartAdvanceWorkflowTCM(taskModel.Code,
                                                    taskModel.TaskFolio,
                                                    operationNumber,
                                                    K2TCMHelpers.EntityTypeT2,
                                                    entityId,
                                                    taskModel.TaskTypeCode,
                                                    parameters,
                                                    K2IntegrationEnumerator.GeneralActions.RejectWorkflow,
                                                    taskModel.TaskId);

                            if (k2result.IsValid)
                            {
                                var task = new TaskDataModel()
                                {
                                    WorkflowInstanceId = taskModel.WorkflowInstanceId,
                                    TaskId = taskModel.TaskId,
                                    UserProfile = taskModel.UserProfile
                                };

                                k2result = _k2IntegrationService.ChangeStatusOthersTasks(task, TCM_REJECTED_ALL);

                                //outputs
                                foreach (var k2FlowItem in context.sendToMapK2FlowValidationModels)
                                {
                                    var vov = context.VisualizationRepository.VisualOutputVersionGet(k2FlowItem.visualDataVersionId);
                                    vov.IsCompleteToPublish = true;
                                    vov.StatusSend = (int)SendToMapDestinationEnum.None;
                                    vov.StatusRemove = (int)SendToMapDestinationEnum.None;

                                    var comment = new VisualizationCommentModel()
                                    {
                                        EntityId = k2FlowItem.visualDataVersionId,
                                        EntityType = VisualEntityTypes.VisualOutputVersion,
                                        Text = string.IsNullOrEmpty(k2FlowItem.voComment) ? NO_COMMENTS_ADDDED : k2FlowItem.voComment
                                    };

                                    //Rejected
                                    Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - Rejected - visualOutputVersionId: " +
                                        k2FlowItem.visualDataVersionId);
                                    Logger.GetLogger().WriteDebug("VisualizationRules", "Method: vivocoovdTCM - Rejected - ValidationStageId: " +
                                        context.VIS_TL.Name);
                                    vov.ValidationStageId = context.VIS_TL.MasterId;
                                    vov.IsValidated = false;
                                    vov.ValidateDate = null;
                                    vov.StatusValidation = (int)SendToMapDestinationEnum.None;

                                    vovs.Add(vov);

                                    if (!comment.Text.Equals(NO_COMMENTS_ADDDED))
                                        commentsList.Add(comment);
                                }

                                context.VisualizationRepository.VisualOutputVersionSendToMap(vovs);
                                SaveVisualDataComments(context, commentsList);
                                rule.SetResult(true);
                            }

                            break;
                    }
                }
            }
        }

        #endregion

        private void SaveVisualDataComments(
            VisualizationBusinessContext context, IList<VisualizationCommentModel> commentsList)
        {
            foreach (var comm in commentsList)
            {
                context.VisualizationRepository.SaveComment(comm);
            }
        }
    }
}