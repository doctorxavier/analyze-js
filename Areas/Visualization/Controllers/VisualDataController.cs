using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using System.IO;

using Newtonsoft.Json;

using IDB.Architecture;
using IDB.Architecture.BusinessRules;
using IDB.Architecture.Extensions;
using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.MW.Application.Components;
using IDB.MW.Application.DEMModule.Services.Core.Interfaces;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.Application.TCM.Services.TcmUniverseService;
using IDB.MW.Application.TCM.Services.ValidationProcessService;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.ResultMatrix.Outputs;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Contracts.ModelRepositories.Visualization;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.Visualization;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.DomainModel.Entities.Core.K2;
using IDB.Presentation.MVC4.Areas.Visualization.Business;
using IDB.Presentation.MVC4.Areas.Visualization.Enums;
using IDB.Presentation.MVC4.Areas.Visualization.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.DocumentManagement.Messages;
using IDB.MW.Domain.Models.Documents;
using IDB.MW.Application.DocumentManagement.Services;
using IDB.MW.Business.DocumentManagement.Enums;

namespace IDB.Presentation.MVC4.Areas.Visualization.Controllers
{
    public partial class VisualDataController : BaseController
    {
        private readonly IValidationService _validationService;
        private readonly ITcmUniverseService _tcmUniverseService;
        private readonly IDEMLockModulesService _demLockModulesService;
        private readonly IMediaGaleryService _mediaGaleryService;
        private IResultsMatrixModelRepository _resultMatrixOutputs = null;

        public VisualDataController(IValidationService validationService, 
            ITcmUniverseService tcmUniverseService,
            IDEMLockModulesService demLockModulesService,
            IMediaGaleryService mediaGaleryService)
        {
            _validationService = validationService;
            _tcmUniverseService = tcmUniverseService;
            _demLockModulesService = demLockModulesService;
            _mediaGaleryService = mediaGaleryService;
        }

        public virtual IResultsMatrixModelRepository ResultMatrixOutputs
        {
            get { return _resultMatrixOutputs; }
            set { _resultMatrixOutputs = value; }
        }

        public virtual ActionResult SendToMap(bool state = false)
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.Execute("Visualization.MapsVisualization.Load");

            if (state)
            {
                var message = MessageHandler.setK2Message(MessageNotificationCodes.SaveDataFail, false, 0);
                ViewData["message"] = message;
            }

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteMessage("VisualData SendToMap", "[ cycleOperation: " + cycleOperation + ", OperationNumber: " + rspnse.OperationNumber + "]");
            business.AttributeCode = cycleOperation;
            return View(business);
        }

        public virtual ActionResult RemoveFromMap()
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.Execute("Visualization.MapsVisualization.Load");

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteMessage("VisualData RemoveFromMap", "[ cycleOperation: " + cycleOperation + ", OperationNumber: " + rspnse.OperationNumber + "]");
            business.AttributeCode = cycleOperation;

            return View(business);
        }

        public virtual ActionResult SendToMapMO()
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.Execute("Visualization.MapsVisualization.Load");
            return View(business);
        }

        private IPCRWorkflowStatusRepository _pcrWorkflowStatusRepository;

        public virtual IPCRWorkflowStatusRepository PCRWorkflowStatusRepository
        {
            get { return _pcrWorkflowStatusRepository; }
            set { _pcrWorkflowStatusRepository = value; }
        }

        private IReportsGenericRepository _ClientGenericRepository = null;

        public IReportsGenericRepository ClientGenericRepository
        {
            get { return _ClientGenericRepository; }
            set { _ClientGenericRepository = value; }
        }

        private IDocumentManagementService _docManagementService = null;
        public virtual IDocumentManagementService docManagementService 
        {
            get { return _docManagementService; }
            set { _docManagementService = value; }
        }

        public virtual ActionResult Grid(
            string operationNumber,
            SendToMapNotifMessageEnum notifStatus = SendToMapNotifMessageEnum.None)
        {
            var business = new VisualizationBusinessContext();
            business.Load(operationNumber);
            business.Execute("Visualization.Grid.Load");

            ViewBag.RMAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
                 ClientGenericRepository, PCRWorkflowStatusRepository);

            ViewBag.DEMLockReviewProcessData = _demLockModulesService
                .BuildLockReviewProcessDEMDataSerializable(IDBContext.Current.Operation);

            switch (notifStatus)
            {
                case SendToMapNotifMessageEnum.K2WorkflowStartedOk:
                    IDBContext.Current.NotifyMessage(Localization.GetText("Workflow initiated correctly"));
                    break;
                case SendToMapNotifMessageEnum.DataSavedCorrectly:
                    IDBContext.Current.NotifyMessage(MessageHandler.setMessage(
                        MessageNotificationCodes.SaveDataSucessfully, true, 5).Message);
                    break;
                case SendToMapNotifMessageEnum.ErrorSavingData:
                    IDBContext.Current.ErrorMessage(MessageHandler.setMessage(
                        MessageNotificationCodes.SaveDataFail, false, 5).Message);
                    break;
            }

            return View(business);
        }

        public virtual ActionResult GridEdit()
        {
            var business = new VisualizationBusinessContext();
            business.EditMode = true;
            business.Load(IDBContext.Current.Operation);
            business.Execute("Visualization.Grid.Load");
            business.Execute("Visualization.Grid.Edit");
            int totalRows = 0;
            var modelCategories = ResultMatrixOutputs.getCategories(out totalRows, int.MaxValue, 0);
            ViewBag.Categories = modelCategories;

            ViewBag.RMAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
                 ClientGenericRepository, PCRWorkflowStatusRepository);
            return View("Grid", business);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult GridSave(string operation)
        {
            var business = new VisualizationBusinessContext();
            business.EditMode = true;
            business.Load(IDBContext.Current.Operation);
            business.Execute("Visualization.Grid.Load");

            var saveResult = business.Execute("Visualization.Grid.Save");
            return saveResult.GetResult<ActionResult>();
        }

        public virtual ActionResult RequestDetails()
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            var enable = business.Execute("Visualization.Workflow.IsEnable");
            if (!enable.GetResult<bool>())
            {
                IDBContext.Current.ErrorMessage(
                   Localization.GetText("Operation is not enable to begin workflow, check for required permissions or operation status"));
                return RedirectToAction("Index");
            }

            business.EditMode = true;
            business.Execute("Visualization.Grid.Load");
            business.EditMode = false;
            business.Execute("Visualization.Grid.Filter.Ready");
            business.WorkflowMode = true;
            return View(business);
        }

        public virtual JsonResult SaveComment()
        {
            var business = new VisualizationBusinessContext();
            var saveResult = business.Execute("Visualization.Comments.Save");
            return Json(new
            {
                result = "ok"
            });
        }

        public virtual ActionResult VPDetails(
            MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);

            if (!business.VisualProjects.Any())
                business.Execute("Visualization.VP.Create");

            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);

                if (messageStatus == MessageNotificationCodes.SaveDataSucessfully)
                    IDBContext.Current.NotifyMessage(message.Message);
                else if (messageStatus == MessageNotificationCodes.SaveDataFail)
                    IDBContext.Current.ErrorMessage(message.Message);
            }

            business.VerifyConcurrenceUrl = business.Url.RouteUrl("HomeAPI_operation", new
                {
                    controller = "Concurrence",
                    action = "Get"
                });
            business.VerifyConcurrenceData = GenerateDataToVerifyConcurrence(
                "VPEdit",
                new RouteValueDictionary(new
                    {
                        visualProjectId =
                            business.VisualProject.VisualProjectVersionsData.VisualProjectId,
                    }),
                business);

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteMessage("VisualData VPDetails", "[ cycleOperation: " + cycleOperation + ", OperationNumber: " + rspnse.OperationNumber + "]");
            ViewBag.CycleTypeOperation = cycleOperation;

            return View(business);
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        [AcceptVerbs(HttpVerbs.Get)]
        public virtual ActionResult VPEdit(int visualProjectId)
        {
            var business = new VisualizationBusinessContext();
            business.VisualProjectId = visualProjectId;
            business.Load(IDBContext.Current.Operation);

            var editRules = business.Execute("Visualization.VP.Edit");
            var actionResult = editRules.GetResult<ActionResult>("Action");
            if (actionResult != null)
            {
                return actionResult;
            }

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteMessage("VisualData VPEdit", "[ cycleOperation: " + cycleOperation + ", OperationNumber: " + rspnse.OperationNumber + "]");

            ViewBag.CycleTypeOperation = cycleOperation;

            return View(business);
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        [AcceptVerbs(HttpVerbs.Post)]
        public virtual ActionResult VPSave(VPVVModels vpv)
        {
            if (!ModelState.IsValid)
            {
                IDBContext.Current.ErrorMessage("Not valid values");
                return RedirectToAction("VPEdit", new { visualProjectId = vpv.VisualProjectId });
            }

            var business = new VisualizationBusinessContext();
            vpv.LocationTypeId = MasterDefinitions.GetMaster("UNDEFINED").MasterId;
            business.VisualProjectId = vpv.VisualProjectId;
            business.Load(IDBContext.Current.Operation);
            business.ViewModel = vpv;
            business.Execute("Visualization.VP.Save");

            if (vpv.VisualProjectId > 0)
            {
                return RedirectToAction("VPDetails", new { visualProjectId = vpv.VisualProjectId });
            }
            else
            {
                return Json(new
                {
                    VisualProjectVersionId = business
                        .VisualProject
                        .VisualProjectVersions
                        .First()
                        .VisualProjectVersionId,
                    VisualProjectId = business
                        .VisualProject
                        .VisualProjectId
                });
            }
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult VPVDelete(int visualProjectVersionId, bool? includeValidated)
        {
            var business = new VisualizationBusinessContext();
            business.VisualProjectVersionId = visualProjectVersionId;
            business.Load(IDBContext.Current.Operation);
            var deleteRules = business.Execute("Visualization.VPV.Delete");
            if (includeValidated.Value)
            {
                business.VisualProjectVersionId = business.VisualProject.VisualProjectVersions
                   .FirstOrDefault(vpv => vpv.ValidationStageId == business.VIS_COO.MasterId)
                   .VisualProjectVersionId;
                business.Execute("Visualization.VPV.Delete");
            }

            return deleteRules.GetResult<ActionResult>();
        }

        public virtual ActionResult VODetails(int visualOutputId)
        {
            var business = new VisualizationBusinessContext();
            business.VisualOutputId = visualOutputId;
            business.OutputId = business
                .VisualOutputGet(visualOutputId)
                .VisualOutputVersions
                .First()
                .OutputYearPlan
                .OutputId;
            business.Load(IDBContext.Current.Operation);
            business.ResultsMatrixContext.Execute("ResultsMatrix.Outputs.Load");

            business.VerifyConcurrenceUrl = business.Url.RouteUrl("HomeAPI_operation", new
            {
                controller = "Concurrence",
                action = "Get"
            });

            business.VerifyConcurrenceData = GenerateDataToVerifyConcurrence(
                "VOEdit",
                new RouteValueDictionary(new
                    {
                        visualOutputId =
                            business.VisualOutput.VisualOutputVersionsData.VisualOutputId,
                    }),
                business);

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);

            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteMessage("VisualData VODetails", "[ cycleOperation: " + cycleOperation + ", OperationNumber: " + rspnse.OperationNumber + "]");

            ViewBag.CycleTypeOperation = cycleOperation;

            return View(business);
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult VOEdit(int visualOutputId)
        {
            var business = new VisualizationBusinessContext();
            business.VisualOutputId = visualOutputId;
            business.OutputId = business
                .VisualOutputGet(visualOutputId)
                .VisualOutputVersions
                .First()
                .OutputYearPlan
                .OutputId;
            business.Load(IDBContext.Current.Operation);
            business.ResultsMatrixContext.Execute("ResultsMatrix.Outputs.Load");

            var editAvailaible = business.Execute("Visualization.VO.IsEditable");
            if (!editAvailaible.GetResult<bool>())
            {
                throw new Exception("Current VO is not editable by the current user or is in invalid state");
            }

            business.Execute("Visualization.VO.Edit");

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteMessage("VisualData VOEdit", "[ cycleOperation: " + cycleOperation + ", OperationNumber: " + rspnse.OperationNumber + "]");

            ViewBag.CycleTypeOperation = cycleOperation;

            return View(business);
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult VOSave(VOVVModel vov)
        {
            if (!ModelState.IsValid)
            {
                IDBContext.Current.ErrorMessage("Not valid values");
                return RedirectToAction("VOEdit", new { visualOutputId = vov.VisualOutputId });
            }

            var business = new VisualizationBusinessContext();

            if (vov.VisualOutputId == -1)
                business.OutputId = vov.OutputId.Value;
            else
            {
                business.VisualOutputId = vov.VisualOutputId;
                business.OutputId = business
                    .VisualOutputGet(vov.VisualOutputId)
                    .VisualOutputVersions
                    .First()
                    .OutputYearPlan
                    .OutputId;
            }

            business.Load(IDBContext.Current.Operation);
            business.ViewModel = vov;
            business.Execute("Visualization.VO.Save");

            if (vov.VisualOutputId != -1)
                return RedirectToAction("VODetails", new { visualOutputId = vov.VisualOutputId });
            else
            {
                business.Load(IDBContext.Current.Operation);
                return Json(new
                    {
                        VisualOutputVersionId = business.VisualOutput.VisualOutputVersionsData
                            .VisualOutputVersionId,
                        VisualOutputId = business.VisualOutput.VisualOutputId
                    });
            }
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult MediaAdd()
        {
            ViewBag.operationNumber = IDBContext.Current.Operation;
            ViewBag.isFirstTime = true;
            return View();
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult MediaAddSave(string operation)
        {
            if (string.IsNullOrEmpty(operation))
            {
                operation = IDBContext.Current.Operation;
            }

            UploadDocumentRequest requestUpload;
            var urlMadia = string.Empty;
            if (Request.Files.Count == 0)
            {
                return Json(new { status = "Error", message = "No files submitted" }, "text/plain");
            }

            try
            {
                MemoryStream buffer;
                var file = Request.Files[0];
                buffer = new MemoryStream();
                file.InputStream.CopyTo(buffer);

                requestUpload = new UploadDocumentRequest
                {
                    OperationNumber = operation,
                    FileName = file.FileName,
                    FileStream = buffer.ToArray(),
                    AccessInformation = AccessInformationCategoryEnum.CONFIDENTIAL,
                    BusinessAreaCode = BusinessAreaCodeEnum.BA_MEDIA
                };

                var responseMedia = _mediaGaleryService.UploadMedia(requestUpload);
                if (!responseMedia.IsValid)
                {
                    return Json(new { status = "Error", message = responseMedia.ErrorMessage }, "text/plain");
                }

                var UploadMediaModel = responseMedia.UploadMediaModel.FirstOrDefault();
                if (UploadMediaModel != null)
                {
                    urlMadia = UploadMediaModel.MediaUrl;
                }
            }
            catch (Exception ex)
            {
                Logger.GetLogger()
                    .WriteError(GetType().Name, ex.Message, ex);

                return Json(new { status = "Error", message = ex.Message }, "text/plain");
            }

            Response.Clear();
            Response.ContentType = "text/plain";
            return Json(new { status = "OK", urlMadia = urlMadia }, "text/plain");
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult MediaSearch(
           string name,
           string dateFrom,
           string dateTo)
        {
            SearchDocumentRequest requestSearch;
            var operationNumber = IDBContext.Current.Operation;
            var business = new VisualizationBusinessContext();
            DateTime parsedFrom, parsedTo;
            DateTime.TryParse(dateFrom, out parsedFrom);
            DateTime.TryParse(dateTo, out parsedTo);
            var from = parsedFrom == DateTime.MinValue ? default(DateTime?) : parsedFrom;
            var to = parsedTo == DateTime.MinValue ? default(DateTime?) : parsedTo;

            requestSearch = new SearchDocumentRequest
            {
                MultiLoans = operationNumber,
                BusinessAreaCode = BusinessAreaCodeEnum.BA_MEDIA.Value
            };

            var responseMedia = _mediaGaleryService.SearchMedia(requestSearch);
            if (!responseMedia.IsValid)
            {
                ViewBag.name = null;
                ViewBag.from = null;
                ViewBag.to = null;
                ViewBag.Medias = responseMedia.SearchMediaModel;
                return View();
            }

            ViewBag.Medias = responseMedia.SearchMediaModel;

            ViewBag.name = name;
            ViewBag.from = from;
            ViewBag.to = to;
            return View();
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult MediaRelate(List<string> elements)
        {
            var business = new VisualizationBusinessContext();
            var images = new Dictionary<string, int>();
            var imageTypes = new string[] { "png", "jpg", "jpeg", "gif", "bpm" };
            var videoTypes = new string[] { "avi", "wmf", "mp4", "flv" };

            foreach (var element in elements)
            {
                if (element.LastIndexOf('.') != -1)
                {
                    var fileExtension = element.Substring(element.LastIndexOf('.'));
                    var isImage = imageTypes.Any(it => it == fileExtension);
                    if (isImage)
                    {
                        images.Add(element,
                           MasterDefinitions.GetMaster("MEDIA_TYPE", "IMAGE").MasterId);
                    }
                    else
                    {
                        images.Add(element,
                           MasterDefinitions.GetMaster("MEDIA_TYPE", "VIDEO").MasterId);
                    }
                }
                else
                {
                    images.Add(element,
                          MasterDefinitions.GetMaster("MEDIA_TYPE", "VIDEO").MasterId);
                }
            }

            return Content("ok");
        }

        public virtual ActionResult MapSelection()
        {
            return View();
        }

        public virtual ActionResult DocumentAdd()
        {
            var operationId = Request["operationId"];            
            var mainOperationNumber = Request["mainOperationNumber"];
            var entityRelated = Request["entityRelated"];
            var isFirstTime = Request["isFirstTime"];
            var parentEntityId = Request["parentEntityId"];
            var subParentEntityId = Request["subParentEntityId"];
            var subSubParentEntityId = Request["subsubParentEntityId"];
            var entityRegisterId = Request["entityRegisterId"];
            var businessAreaCode = "BA_MAP_OPERATION"; 

            DocumentsViewModel documentModel = new DocumentsViewModel
            {
                OperationId = int.Parse(operationId),
                BusinessAreaCode = businessAreaCode,
                MainOperationNumber = mainOperationNumber,
                EntityRelated = entityRelated,
                ParentEntityId = int.Parse(parentEntityId),
                SubParentEntityId = int.Parse(subParentEntityId),
                SubsubParentEntityId = int.Parse(subSubParentEntityId),
                EntityRegisterId = int.Parse(entityRegisterId)
            };

            ViewBag.isFirstTime = isFirstTime;

            return View(documentModel);
        }

        public virtual ActionResult DocumentAddSectionDocument()
        {
            return View();
        }

        public virtual ActionResult DocumentCloseWindow(int? documentId, string documentNumber, string name)
        {
            return View(new DocumentAddViewModel { DocumentId = documentId, DocumentNumber = documentNumber, Name = name });
        }

        public virtual ActionResult DocumentNewSectionVO(string idbdocNumber, string description)
        {
            var documentData = _validationService.GetDocumentData(idbdocNumber);

            var model = new VisualOutputDocumentModel()
            {
                DocumentId = documentData != null ? documentData.DocumentId : 0,
                User = documentData != null ? documentData.CreatedBy : IDBContext.Current.UserName,
                Date = documentData != null ? documentData.Created.Value : DateTime.Now.Date,
                DocNumber = idbdocNumber,
                Description = documentData != null ? documentData.Description : string.Empty
            };

            return View(model);
        }

        public virtual ActionResult DocumentNewSectionVP(string idbdocNumber, string description)
        {
            var documentData = _validationService.GetDocumentData(idbdocNumber);

            var model = new VisualProjectDocumentModel()
            {
                DocumentId = documentData != null ? documentData.DocumentId : 0,
                User = documentData != null ? documentData.CreatedBy : IDBContext.Current.UserName,
                Date = documentData != null ? documentData.Created.Value : DateTime.Now.Date,
                DocNumber = idbdocNumber,
                Description = documentData != null ? documentData.Description : string.Empty
            };

            return View(model);
        }

        public virtual ActionResult SendToMapSelectionMap()
        {
            return View();
        }

        public virtual ActionResult DocumentDelete(int documentId, int mediaId)
        {
            var respository = Globals.Resolve<IVisualizationRepository>();
            respository.DocumentUnlink(documentId, mediaId);
            return Content("ok");
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult VOVDelete(int visualOutputVersionId)
        {
            var business = new VisualizationBusinessContext();
            business.VisualOutputVersionId = visualOutputVersionId;
            business.Load(IDBContext.Current.Operation);
            var deleteRules = business.Execute("Visualization.VOV.Delete");

            if (IsJson)
                return Content("ok");

            return deleteRules.GetResult<ActionResult>();
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult VisualOutputCreate(int outputId)
        {
            var business = new VisualizationBusinessContext();
            business.OutputId = outputId;
            business.Load(IDBContext.Current.Operation);
            business.ResultsMatrixContext.Execute("ResultsMatrix.Outputs.Load");
            business.Execute("Visualization.VO.Create");

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteMessage("VisualData VisualOutputCreate", "[ cycleOperation: " + cycleOperation + ", OperationNumber: " + rspnse.OperationNumber + "]");
            ViewBag.CycleTypeOperation = cycleOperation;

            return View("VOEdit", business);
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE_TL)]
        [HttpPost]
        public virtual ActionResult VisualProjectSendToMap(
            int visualProjectVersionId,
            SendToMapSourceEnum operationOrigin,
            SendToMapDestinationEnum destination)
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.operationOrigin = operationOrigin;
            business.operationDestination = destination;
            business.VisualProjectVersionId = visualProjectVersionId;

            var ruleForVPSendToMap = "Visualization.VP.TLCompleted.SendToMap";
            var ruleResult = business.Execute(ruleForVPSendToMap);

            return Json(new { success = ruleResult.GetResult<bool>(ruleForVPSendToMap) }, "application/json");
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE_TL)]
        [HttpPost]
        public virtual ActionResult VisualOutputSendToMap(
            SendToMapOperTypeEnum operationType,
            SendToMapSourceEnum operationOrigin,
            IList<SendToMapDataModel> sendToMapDataModels)
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.operationType = operationType;
            business.operationOrigin = operationOrigin;
            var execResult = true;

            if (sendToMapDataModels.HasAny())
            {
                var sendToNonSelected = sendToMapDataModels.Where(x => x.dest == null)
                .Select(x => new VisualOutputModel { VisualOutputVersionId = x.vovId }).ToList();
                var sendToInternalMap = sendToMapDataModels
                    .Where(x => x.dest != null && x.dest.Contains(SendToMapDestinationEnum.Internal) &&
                        !x.dest.Contains(SendToMapDestinationEnum.External))
                    .Select(x => new VisualOutputModel { VisualOutputVersionId = x.vovId }).ToList();
                var sendToExternalMap = sendToMapDataModels
                    .Where(x => x.dest != null && x.dest.Contains(SendToMapDestinationEnum.External))
                    .Select(x => new VisualOutputModel { VisualOutputVersionId = x.vovId }).ToList();
                RuleContext ruleResult;                
                var ruleForVOSendToMap = "Visualization.VO.TLCompleted.SendToMap";

                if (sendToNonSelected.Any())
                {
                    business.VisualOutputs = null;
                    business.VisualOutputs = sendToNonSelected;
                    business.operationDestination = SendToMapDestinationEnum.None;

                    ruleResult = business.Execute(ruleForVOSendToMap);
                    execResult = ruleResult.GetResult<bool>(ruleForVOSendToMap);
                }

                if (sendToInternalMap.Any() && execResult)
                {
                    business.VisualOutputs = null;
                    business.VisualOutputs = sendToInternalMap;
                    business.operationDestination = SendToMapDestinationEnum.Internal;

                    ruleResult = business.Execute(ruleForVOSendToMap);
                    execResult = ruleResult.GetResult<bool>(ruleForVOSendToMap);
                }

                if (sendToExternalMap.Any() && execResult)
                {
                    business.VisualOutputs = null;
                    business.VisualOutputs = sendToExternalMap;
                    business.operationDestination = SendToMapDestinationEnum.External;

                    ruleResult = business.Execute(ruleForVOSendToMap);
                    execResult = ruleResult.GetResult<bool>(ruleForVOSendToMap);

                    return Json(new
                    {
                        success = execResult,
                        notifStatus = execResult ?
                                (operationType.Equals(SendToMapOperTypeEnum.SaveAndValidate) ?
                                    SendToMapNotifMessageEnum.K2WorkflowStartedOk :
                                    SendToMapNotifMessageEnum.DataSavedCorrectly) :
                                SendToMapNotifMessageEnum.ErrorSavingData
                    },
                        "application/json");
                }
            }

            return Json(new
            {
                success = execResult,
                notifStatus = execResult ? SendToMapNotifMessageEnum.DataSavedCorrectly :
                    SendToMapNotifMessageEnum.ErrorSavingData
            },
            "application/json");
        }

        public virtual ActionResult VisualDataCOOTaskReview(string operationNumber, TaskDataModel model)
        {
            if (!IDBContext.Current.HasRole(Role.CHIEF_OF_OPERATIONS))
            {
                Logger.GetLogger().WriteMessage("VisualData", "No COO role. Security issue?");
                throw new Exception("You don't have the Role to see Visualization Tasks.");
            }

            var taskTag = JsonConvert.DeserializeObject<SendToMapK2DataModel>(model.TaskTag);

            if (taskTag.sourceScreen.Equals(SendToMapSourceEnum.Visual))
                return View("VisualOutputsCOOCompleted", VisualOutputsCOOCompleted(
                    operationNumber, model.TaskId, taskTag.visualOutputVersionIds));

            if (taskTag.sourceScreen.Equals(SendToMapSourceEnum.Map))
                return View("VisualProjectCOOCompleted", VisualProjectCOOCompleted(
                    operationNumber, model.TaskId, taskTag.visualProjectVersionId.Value));

            throw new InvalidOperationException(
                "Unable to determine whether the task is Visual Outputs or Visual Project related");
        }

        public virtual ActionResult VisualDataTaskT2ReviewTCM(string operationNumber, TaskDataModel model)
        {
            var taskTag = JsonConvert.DeserializeObject<SendToMapK2DataModelTCM>(model.TaskTag);

            if (taskTag.sourceScreen.Equals(SendToMapSourceEnum.Visual))
                return View("~/Areas/TCM/Views/WorkflowK2/TaskDetailWorkflow2.cshtml",
                    VisualOutputsTaskT2TCM(operationNumber, model.TaskId, taskTag.visualOutputVersionIds));

            throw new InvalidOperationException(
                "Unable to determine whether the task is Visual Outputs or Visual Project related");
        }

        [HttpPost]
        public virtual ActionResult VisualDataCOOValidateExternal(
            int sendToMapTaskId,
            SendToMapOperTypeEnum operationType,
            List<SendToMapK2FlowValidationModel> sendToMapK2FlowValidationModels)
        {
            if (!IDBContext.Current.HasRole(Role.CHIEF_OF_OPERATIONS))
            {
                Logger.GetLogger().WriteMessage("VisualData", "No COO role. Security issue?");
                throw new Exception("You don't have the Role to see Visualization Tasks.");
            }

            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.sendToMapTaskId = sendToMapTaskId;
            business.operationType = operationType;
            business.sendToMapK2FlowValidationModels = sendToMapK2FlowValidationModels;

            var ruleForCOOApproval = "Visualization.External.Map.COO.Approval";
            var ruleResult = business.Execute(ruleForCOOApproval);

            return Json(new { success = ruleResult.GetResult<bool>(ruleForCOOApproval) }, "application/json");
        }

        [HttpPost]
        public virtual ActionResult VisualOutputValidateTCM(int sendToMapTaskId,
            SendToMapOperTypeEnum operationType,
            List<SendToMapK2FlowValidationModel> sendToMapK2FlowValidationModels)
        {
            var business = new VisualizationBusinessContext();
            business.sendToMapTaskId = sendToMapTaskId;
            business.operationType = operationType;
            business.sendToMapK2FlowValidationModels = sendToMapK2FlowValidationModels;

            business.VIS_TL = MasterDefinitions.GetValidationStage("VIS_TL");
            business.VIS_DRAFY = MasterDefinitions.GetValidationStage("VIS_DRAFY");
            business.TCM_VIS_FC = MasterDefinitions.GetValidationStage("TCM_VIS_FC");

            var ruleApprovalTCM = "Visualization.External.Validate.TCM";
            var ruleResult = business.Execute(ruleApprovalTCM);

            return Json(new { success = ruleResult.GetResult<bool>(ruleApprovalTCM) }, "application/json");
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE_TL)]
        [HttpPost]
        public virtual ActionResult VisualOutputRemoveFromMap(
            SendToMapOperTypeEnum operationType,
            SendToMapSourceEnum operationOrigin,
            IList<SendToMapDataModel> removeFromMapDataModels)
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.operationType = operationType;
            business.operationOrigin = operationOrigin;

            var removeFromNone = removeFromMapDataModels.Where(x => x.dest == null)
                .Select(x => new VisualOutputModel { VisualOutputVersionId = x.vovId }).ToList();
            var removeFromInternalMap = removeFromMapDataModels
                .Where(x => x.dest != null && x.dest.Contains(SendToMapDestinationEnum.Internal))
                .Select(x => new VisualOutputModel { VisualOutputVersionId = x.vovId }).ToList();
            var removeFromExternalMap = removeFromMapDataModels
                .Where(x => x.dest != null && x.dest.Contains(SendToMapDestinationEnum.External) &&
                    !x.dest.Contains(SendToMapDestinationEnum.Internal))
                .Select(x => new VisualOutputModel { VisualOutputVersionId = x.vovId }).ToList();
            RuleContext ruleResult;
            var execResult = true;
            var ruleForVORemoveFromMap = "Visualization.VO.TLCompleted.RemoveFromMap";

            if (removeFromExternalMap.Any())
            {
                business.VisualOutputs = null;
                business.VisualOutputs = removeFromExternalMap;
                business.operationDestination = SendToMapDestinationEnum.External;

                ruleResult = business.Execute(ruleForVORemoveFromMap);
                execResult = ruleResult.GetResult<bool>(ruleForVORemoveFromMap);
            }

            if (removeFromInternalMap.Any() && execResult)
            {
                business.VisualOutputs = null;
                business.VisualOutputs = removeFromInternalMap;
                business.operationDestination = SendToMapDestinationEnum.Internal;

                ruleResult = business.Execute(ruleForVORemoveFromMap);
                execResult = ruleResult.GetResult<bool>(ruleForVORemoveFromMap);
            }

            if (removeFromNone.Any() && execResult)
            {
                business.VisualOutputs = null;
                business.VisualOutputs = removeFromNone;
                business.operationDestination = SendToMapDestinationEnum.None;

                ruleResult = business.Execute(ruleForVORemoveFromMap);
                execResult = ruleResult.GetResult<bool>(ruleForVORemoveFromMap);
            }

            return Json(new
            {
                success = execResult,
                notifStatus = execResult ? SendToMapNotifMessageEnum.DataSavedCorrectly :
                    SendToMapNotifMessageEnum.ErrorSavingData
            },
            "application/json");
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE_TL)]
        public virtual ActionResult VisualProjectRemoveFromMap(
            int visualProjectVersionId, SendToMapSourceEnum operationOrigin, SendToMapDestinationEnum destination)
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.operationOrigin = operationOrigin;
            business.operationDestination = destination;
            business.VisualProjectVersionId = visualProjectVersionId;

            var ruleForVPRemoveFromMap = "Visualization.VP.TLCompleted.RemoveFromMap";
            var ruleResult = business.Execute(ruleForVPRemoveFromMap);

            return Json(new { success = ruleResult.GetResult<bool>(ruleForVPRemoveFromMap) }, "application/json");
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE_TL)]
        public virtual ActionResult VisualProjectCreate()
        {
            var business = new VisualizationBusinessContext();
            business.Load(IDBContext.Current.Operation);
            business.ResultsMatrixContext.Execute("ResultsMatrix.Outputs.Load");
            business.Execute("Visualization.VP.Create");

            var rspnse = _tcmUniverseService.GetParentOperation(IDBContext.Current.Operation);
            if (!rspnse.IsValid) throw new Exception(rspnse.ErrorMessage);

            var cycleOperation = _tcmUniverseService.GetCode(rspnse.OperationNumber);
            Logger.GetLogger().WriteMessage("VisualData VisualProjectCreate", "[ cycleOperation: " + cycleOperation + ", OperationNumber: " + rspnse.OperationNumber + "]");
            ViewBag.CycleTypeOperation = cycleOperation;

            return View("VPEdit", business);
        }

        #region Map services

        public virtual JsonResult VisualOutputGetCountryName(int VisualOutputId)
        {
            var repository = Globals.Resolve<IVisualizationRepository>();
            return Json(new
            {
                Geolocation = repository.VisualOutputGetCountryName(VisualOutputId)
            }, JsonRequestBehavior.AllowGet);
        }

        public virtual JsonResult VisualProjectGetCountryName(int VisualProjectId)
        {
            var repository = Globals.Resolve<IVisualizationRepository>();
            return Json(new
            {
                Geolocation = repository.VisualProjectGetCountryName(VisualProjectId)
            }, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult VisualOutputVersionGet(int VisualOutputId, bool IsValidate)
        {
            var repository = Globals.Resolve<IVisualizationRepository>();
            var result = repository.VisualOutputVersionGetValidated(VisualOutputId, IsValidate);

            if (result == null)
            {
                return Json("{}", JsonRequestBehavior.AllowGet);
            }

            var jc = JsonConvert.SerializeObject(result);
            Response.ContentType = "application/json";
            return Content(jc);
        }

        public virtual ActionResult VisualProjectVersionGet(int VisualProjectId, bool IsValidate)
        {
            var repository = Globals.Resolve<IVisualizationRepository>();
            var result = repository.VisualProjectVersionGetValidated(VisualProjectId, IsValidate);
            if (result == null)
            {
                return Json("{}", JsonRequestBehavior.AllowGet);
            }

            var jc = JsonConvert.SerializeObject(result);
            Response.ContentType = "application/json";
            return Content(jc);
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult VisualOutputUpdateLevel(
           string Level1,
           string Level2,
           string Level3,
           int visualOutputVersionId)
        {
            var business = new VisualizationBusinessContext();
            business.Execute("Visualization.Map.VOUpdateLevel");
            return Json("ok", JsonRequestBehavior.AllowGet);
        }

        [HasPermission(Permissions = Permission.VISUALIZATION_WRITE)]
        public virtual ActionResult VisualProjectUpdateLevel(
           string Level1,
           string Level2,
           string Level3,
           int VisualProjectVersionId)
        {
            var business = new VisualizationBusinessContext();
            business.Execute("Visualization.Map.VPUpdateLevel");
            return Json("ok", JsonRequestBehavior.AllowGet);
        }
        #endregion

        private bool IsJson
        {
            get
            {
                return
                    IDBContext.Current.ContextContentType.Split(';')
                    .Any(t => t.Equals("application/json", StringComparison.OrdinalIgnoreCase));
            }
        }

        private VisualizationBusinessContext VisualOutputsCOOCompleted(
            string operationNumber, int taskId, IEnumerable<int> vovsOnCurrentK2Flow)
        {
            var business = new VisualizationBusinessContext();

            business.Load(IDBContext.Current.Operation);
            business.sendToMapTaskId = taskId;
            business.ResultsMatrixContext.Execute("ResultsMatrix.Outputs.Load");
            business.VisualOutputs.RemoveAll((vo) => !vovsOnCurrentK2Flow.Any(x =>
                x == vo.VisualOutputVersionsData.VisualOutputVersionId));

            return business;
        }

        private VisualizationBusinessContext VisualProjectCOOCompleted(
            string operationNumber, int taskId, int visualProjectVersionId)
        {
            var business = new VisualizationBusinessContext();

            business.Load(IDBContext.Current.Operation);
            business.sendToMapTaskId = taskId;
            business.VisualProjectVersionId = visualProjectVersionId;

            return business;
        }

        private string[] GenerateDataToVerifyConcurrence(
            string targetMethod,
            RouteValueDictionary routeValues,
            VisualizationBusinessContext business)
        {
            var requestFullUrl = IDBContext.Current.ContextRequestContext.HttpContext.Request.Url;
            var urlToCheckConcurrence = business.Url.Action(
                targetMethod,
                "VisualData",
                routeValues,
                requestFullUrl.Scheme,
                requestFullUrl.Host);

            return urlToCheckConcurrence.Split('?');
        }

        private VisualizationBusinessContext VisualOutputsTaskT2TCM(string operationNumber, int taskId, IEnumerable<int> vovsOnCurrentK2Flow)
        {
            var business = new VisualizationBusinessContext();

            business.Load(IDBContext.Current.Operation);
            business.sendToMapTaskId = taskId;
            business.ResultsMatrixContext.Execute("ResultsMatrix.Outputs.Load");
            business.VisualOutputs.RemoveAll((vo) => !vovsOnCurrentK2Flow.Any(x => x == vo.VisualOutputVersionsData.VisualOutputVersionId));

            return business;
        }

        private int? GetYear(string year)
        {
            int result = 0;
            int.TryParse(year, out result);

            return result;
        }
    }
}