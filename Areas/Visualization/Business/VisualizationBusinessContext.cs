using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

using IDB.Architecture;
using IDB.Architecture.Extensions;
using IDB.Architecture.BusinessRules;
using IDB.Architecture.Language;
using IDB.MW.Domain.Contracts.ModelRepositories.Visualization;
using IDB.MW.Domain.Models.Architecture.Visualization;
using IDB.MW.Domain.Models.MasterData;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.Visualization.Enums;
using IDB.Presentation.MVC4.Areas.Visualization.Models;
using IDB.Presentation.MVC4.Areas.ResultsMatrix.Business;
using IDB.Presentation.MVC4.MVCCommon;
using IDB.MW.Application.Components;

namespace IDB.Presentation.MVC4.Areas.Visualization.Business
{
    public class VisualizationBusinessContext : BusinessContext
    {
        public IVisualizationRepository VisualizationRepository { get; set; }
        public OperationBusiness OperationContext;
        public ResultsMatrixBusiness ResultsMatrixContext;
        public List<VisualProjectModel> VisualProjects;

        public VisualProjectModel VisualProject { get; set; }
        public VisualOutputModel VisualOutput { get; set; }
        public UrlHelper Url { get; set; }
        public List<VisualOutputModel> VisualOutputs { get; set; }
        public List<VisualizationCommentModel> UserComments { get; set; }
        public bool EditMode { get; set; }
        public MasterDataModel VIS_TL;
        public MasterDataModel VIS_REV;
        public MasterDataModel VIS_DRAFY;
        public MasterDataModel VIS_READY;
        public MasterDataModel VIS_COO;
        public MasterDataModel TCM_VIS_FC;
        public List<MasterDataModel> LocationTypes { get; set; }
        public List<MasterDataModel> MediaSourceTypes { get; set; }
        public List<MasterDataModel> DeliveryStatusTypes { get; set; }
        public int? VisualProjectVersionId;
        public int? VisualProjectId { get; set; }
        public int VisualOutputId { get; set; }
        public int VisualOutputVersionId { get; set; }
        public int OutputId { get; set; }
        public SendToMapOperTypeEnum operationType { get; set; }
        public SendToMapSourceEnum operationOrigin { get; set; }
        public SendToMapDestinationEnum operationDestination { get; set; }
        public int sendToMapTaskId { get; set; }
        public List<SendToMapK2FlowValidationModel> sendToMapK2FlowValidationModels { get; set; }
        public string VerifyConcurrenceUrl { get; set; }
        public string[] VerifyConcurrenceData { get; set; }
        public bool? ShowWarningMessageOnCancelEdition { get; set; }
        public string AttributeCode { get; set; }

        public VisualizationBusinessContext()
            : base()
        {
            VisualizationRepository = Globals.Resolve<IVisualizationRepository>();
        }

        public void Load(string operationNumber)
        {
            OperationContext = new OperationBusiness();
            Globals.Rules.ApplyParentLogic(this, OperationContext);
            OperationContext.Load(operationNumber);
            ResultsMatrixContext = new ResultsMatrixBusiness();
            Globals.Rules.ApplyParentLogic(this, ResultsMatrixContext);
            ResultsMatrixContext.Load(OperationContext);
            Url = new UrlHelper(IDBContext.Current.ContextRequestContext);

            VIS_TL = MasterDefinitions.GetValidationStage("VIS_TL");
            VIS_REV = MasterDefinitions.GetValidationStage("VIS_REV");
            VIS_DRAFY = MasterDefinitions.GetValidationStage("VIS_DRAFY");
            VIS_READY = MasterDefinitions.GetValidationStage("VIS_READY");
            VIS_COO = MasterDefinitions.GetValidationStage("VIS_COO");
            LocationTypes = MasterDefinitions.GetMasterByType("VO_LOCATION_TYPE");
            MediaSourceTypes = MasterDefinitions.GetMasterByType("MEDIA_SOURCE");
            DeliveryStatusTypes = MasterDefinitions.GetMasterByType("VO_DELIVERY_STATUS")
                .OrderByDescending(a => a.Code).ToList();
            Execute("Visualization.Load");
        }

        public bool WorkflowMode { get; set; }

        public override object ContextObject
        {
            get
            {
                return this;
            }
        }

        public bool IsReadyForValidation(VisualOutputVersionModel vov)
        {
            if (IsReadyStage(vov.ValidationStageId) || vov.IsDeleted.IsTrue())
            {
                return true;
            }

            return false;
        }

        public bool IsReadyStage(int? validationStage)
        {
            if (validationStage == null)
            {
                return false;
            }

            return MasterDefinitions.GetValidationStageId("VIS_READY") == validationStage ||
                MasterDefinitions.GetValidationStageId("VIS_TL") == validationStage;
        }

        public bool IsVisibleForPublication(int? validationStage, bool? isCompleteToPublish)
        {
            return validationStage.HasValue ?
                (validationStage == VIS_DRAFY.MasterId ?
                    (isCompleteToPublish.HasValue && isCompleteToPublish.Value) :
                    validationStage == VIS_TL.MasterId) :
                false;
        }

        public bool IsSelectableForRemoval(int? validationStage)
        {
            if (validationStage == null)
                return false;

            return
                validationStage.Value == VIS_TL.MasterId ||
                validationStage.Value == VIS_COO.MasterId;
        }

        public bool IsSelectableForPublication(int? validationStage)
        {
            return validationStage.HasValue ?
                validationStage.Value == VIS_DRAFY.MasterId :
                false;
        }

        public bool IsSendToMapInternalChecked(int? validationStage, int? statusSend)
        {
            return validationStage.HasValue ?
                (validationStage.Value == VIS_DRAFY.MasterId ?
                    (statusSend.HasValue ?
                        statusSend.Value != (int)SendToMapDestinationEnum.None :
                        false) :
                    validationStage.Value == VIS_TL.MasterId) :
                false;
        }

        public bool IsK2WorkflowInProgress(VisualOutputVersionModel model)
        {
            return model.ValidationStageId == VIS_COO.MasterId && !model.IsValidated;
        }

        public VisualOutputModel VisualOutputGet(int visualOutputId)
        {
            return VisualizationRepository
                .VisualOutputGet(visualOutputId);
        }

        public decimal VisualOutputSummarize(int year)
        {
            decimal result = 0;
            foreach (var vo in VisualOutputs)
            {
                if (vo.VisualOutputVersionsData != null &&
                    vo.VisualOutputVersionsData.OutputYearPlan.Year == year &&
                    vo.VisualOutputVersionsData.OutputUnits.HasValue &&
                    vo.VisualOutputVersionsData.VisualOutputId != VisualOutputId)
                {
                    result += vo.VisualOutputVersionsData.OutputUnits.Value;
                }
            }

            return result;
        }

        public string GetVisualDataValidationStageName(int validationStageId, bool isValidated)
        {
            return validationStageId == VIS_COO.MasterId ?
                (isValidated ? Localization.GetText(MasterDefinitions.GetMasterName(validationStageId)) :
                    Localization.GetText(MasterDefinitions.GetMasterName(VIS_TL.MasterId))) :
                Localization.GetText(MasterDefinitions.GetMasterName(validationStageId));
        }

        internal bool NotForValidation(int? validationStage)
        {
            if (validationStage == null)
            {
                return false;
            }

            if (VIS_REV.MasterId == validationStage ||
                VIS_COO.MasterId == validationStage)
            {
                return true;
            }

            return false;
        }
    }
}