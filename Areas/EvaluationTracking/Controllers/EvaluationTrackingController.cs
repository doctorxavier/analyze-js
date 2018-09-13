using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture.Logging;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Domain.Session;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.EvaluationTracking;
using IDB.MW.Domain.Models.Architecture.EvaluationTracking;
using IDB.MW.Domain.Contracts.ModelRepositories.Documents;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Entities;
using IDB.MW.Business.DocumentManagement.Contracts;
using IDB.MW.Business.DocumentManagement.Messages;

namespace IDB.Presentation.MVC4.Areas.EvaluationTracking.Controllers
{
    public partial class EvaluationTrackingController : BaseController
    {
        #region ENPOINTS

        private IEvaluationTrackingModelRepository _clientEvaluationTracking = null;
        public IEvaluationTrackingModelRepository ClientEvaluationTracking
        {
            get { return _clientEvaluationTracking; }
            set { _clientEvaluationTracking = value; }
        }

        private IDocumentManagementService _docManagementService = null;
        public virtual IDocumentManagementService docManagementService
        {
            get { return _docManagementService; }
            set { _docManagementService = value; }
        }

        private IDocumentModelRepository _clientDocumentModel = null;
        public virtual IDocumentModelRepository ClientDocumentModel
        {
            get { return _clientDocumentModel; }
            set { _clientDocumentModel = value; }
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

        #endregion

        public virtual ActionResult Index(string operationNumber)
        {
            var result = ClientEvaluationTracking.IfExistEvaluation(operationNumber);
            if (result.ExistEvaluation == true)
            {
                return RedirectToAction("Details", new { operationNumber = operationNumber });
            }

            return View(result);
        }

        public virtual ActionResult Details(string operationNumber)
        {
            var listApprovedOrNA = ClientEvaluationTracking
                .VerifyOperationApproved(operationNumber, IDBContext.Current.CurrentLanguage);
            ViewBag.CodeNotChangedReason = listApprovedOrNA[0].Code;
            var model = ClientEvaluationTracking
                .GetDetailsModel(operationNumber, IDBContext.Current.CurrentLanguage);
            return View(model);
        }

        public virtual ActionResult CreateNewEvaluation(string operationNumber)
        {
            var model = InitEvaluationItem(operationNumber);
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Create(List<EvaluationTrackingModel> model)
        {
            try
            {
                foreach (var itemModel in model)
                {
                    ClientEvaluationTracking.SaveModel(itemModel, IDBContext.Current.UserName);
                }

                if (model.Count == 0)
                    return Json(Url.Action("Details", new { operationNumber = string.Empty }));

                string op = ClientEvaluationTracking.GetOperationNumber(model[0].ResultsMatrixId);
                return Json(Url.Action("Details", new { operationNumber = op }));
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "EvaluationTracking", "Error when create Evaluation Tracking: ", e);
                throw;
            }
        }

        [HasPermission(Permissions = "Evaluation Tracking Write")]
        public virtual ActionResult NewEditEvaluation(string operationNumber)
        {
            return View();
        }

        public virtual ActionResult EditEvaluationTracking(string operationNumber)
        {
            var listApprovedOrNA = ClientEvaluationTracking
                .VerifyOperationApproved(operationNumber, IDBContext.Current.CurrentLanguage);
            ViewBag.CodeNotChangedReason = listApprovedOrNA[0].Code;
            int idMatrix = ClientEvaluationTracking.GetResultsMatrix(operationNumber);
            List<EvaluationTrackingModel> model = ClientEvaluationTracking
                .GetDetailsModel(operationNumber, IDBContext.Current.CurrentLanguage);
            EvaluationTrackingModel loadDropDownList = LoadDropDownList();
            List<EvaluationTrackingModel> itemModel = new List<EvaluationTrackingModel>();

            foreach (var item in model)
            {
                item.GetTopicIntervention = loadDropDownList.GetTopicIntervention;
                item.GetTopicEvaluation = loadDropDownList.GetTopicEvaluation;
                item.GetMethodologyDem = loadDropDownList.GetMethodologyDem;
                item.GetMainTopicEvaluation = loadDropDownList.GetTopicEvaluation;
                item.GetMainTopicIntervention = loadDropDownList.GetTopicIntervention;
                item.GetMethodologyCurrent = loadDropDownList.GetMethodologyCurrent;
                item.GetStage = loadDropDownList.GetStage;
                item.GetResponsible = loadDropDownList.GetResponsible;
                item.GetSourceOfFunding = loadDropDownList.GetSourceOfFunding;
                item.GetTypeDocument = loadDropDownList.GetTypeDocument;
                item.GetDocumentPermission = loadDropDownList.GetDocumentPermission;
                item.GetTypeDocumentString = loadDropDownList.GetTypeDocumentString;
                item.GetAllStageString = loadDropDownList.GetAllStageString;
                itemModel.Add(item);
            }

            var templateModel = InitEvaluationItem(operationNumber);
            templateModel.EvaluationTrackingId = -1;
            itemModel.Add(templateModel);

            return View(itemModel);
        }

        [HttpPost]
        public virtual ActionResult Edit(List<EvaluationTrackingModel> model)
        {
            try
            {
                foreach (var itemModel in model)
                {
                    if (itemModel.EvaluationTrackingId > 0)
                    {
                        ClientEvaluationTracking.ModifiedModel(
                            itemModel, IDBContext.Current.UserName);
                    }
                    else
                    {
                        ClientEvaluationTracking.SaveModel(
                            itemModel, IDBContext.Current.UserName);
                    }
                }

                if (model.Count == 0)
                    return Json(Url.Action("Details", new { operationNumber = string.Empty }));

                string op = ClientEvaluationTracking.GetOperationNumber(model[0].ResultsMatrixId);
                return Json(Url.Action("Details", new { operationNumber = op }));
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "EvaluationTracking", "Error when edit Evaluation Tracking: ", e);
                throw;
            }
        }

        [HttpPost]
        public virtual ActionResult StageValidation(int resultsMatrixId, string verifyContent)
        {
            try
            {
                var StageValidation = ClientEvaluationTracking
                    .GetStageValidationData(resultsMatrixId, verifyContent);
                return Json(new { StageValidation = StageValidation[0].RESP });
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "EvaluationTracking", "Error when edit Evaluation Tracking: ", e);
                throw;
            }
        }

        [HttpPost]
        public virtual ActionResult SaveDocuments(List<EvaluationTrackingModel> model)
        {
            try
            {
                string stringDocumentId = ClientEvaluationTracking.SaveDocument(model);
                return Json(new { DocumentEvaluationTrackingId = stringDocumentId });
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "EvaluationTracking", "Error when edit Evaluation Tracking: ", e);
                throw;
            }
        }

        public virtual FileResult GetDocument(string docNum)
        {
            if (string.IsNullOrEmpty(docNum))
                return null;
            try
            {
                var downloadResponse = _docManagementService.Download(new DownloadRequest
                {
                    DocumentNumber = docNum,
                    Library = string.Empty,
                    Version = string.Empty
                });

                if (!downloadResponse.IsValid)
                {
                    throw new Exception("The file could not be retrieved from IDBDocs.");
                }

                CommonDocument documentObject = new CommonDocument();
                var contentType = documentObject.GetContentType(downloadResponse.Document.FileName);
                return File(downloadResponse.Document.File, contentType, downloadResponse.Document.FileName);
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "EvaluationTracking", "Error when get document Evaluation Tracking: ", e);
                throw;
            }
        }

        public virtual JsonResult FilterStage(EvaluationTrackingModel model)
        {
            List<int> stageIds = new List<int>();
            if (model.MethodologyCurrentId.HasValue)
            {
                stageIds.Add(model.MethodologyCurrentId.Value);
                model.GetStage = ClientEvaluationTracking
                    .GetStageFilter(IDBContext.Current.CurrentLanguage, stageIds)
                    .OrderBy(x => x.ConvergenceMasterDataId).ToList();
            }

            return new JsonResult() { Data = model.GetStage };
        }

        public virtual JsonResult FilterTypeDocument(List<int> IdStage)
        {
            EvaluationTrackingModel result = new EvaluationTrackingModel();
            result.GetTypeDocument = ClientEvaluationTracking
                .GetTypeDocumentFilter(IDBContext.Current.CurrentLanguage, IdStage)
                .OrderBy(x => x.ConvergenceMasterDataId).ToList();

            var resultNamesChanged = from r in result.GetTypeDocument
                                     select new
                                     {
                                         value = r.ConvergenceMasterDataId,
                                         text = r.Name
                                     };

            return new JsonResult() { Data = resultNamesChanged };
        }

        public virtual ActionResult DocumentAdd()
        {
            return View();
        }

        public virtual ActionResult DocumentCloseWindow(string documentId, string operation)
        {
            return View();
        }

        [HttpGet]
        public virtual ActionResult TablePartial(int idEvaluationDocument)
        {
            var modelDoc = ClientEvaluationTracking.GetDocument(
                idEvaluationDocument, IDBContext.Current.Operation);
            return PartialView(
                "~/Areas/EvaluationTracking/Views/EvaluationTracking/Partial/_DocumentTablePartial.cshtml",
                modelDoc.OrderByDescending(x => x.DocumentEvaluationTrackingId).ToList());
        }

        [HttpPost]
        public virtual ActionResult DeleteDocument(int documentEvaluationTrackingId)
        {
            try
            {
                bool boolRemoveDoc = ClientEvaluationTracking.DeleteDocument(documentEvaluationTrackingId);
                return Json(new { responseDeleteDoc = boolRemoveDoc });
            }
            catch (Exception e)
            {
                Logger.GetLogger().WriteError(
                    "EvaluationTracking", "Error when delete document Evaluation Tracking: ", e);
                throw;
            }
        }

        private EvaluationTrackingModel LoadDropDownList()
        {
            EvaluationTrackingModel result = new EvaluationTrackingModel();
            string language = IDBContext.Current.CurrentLanguage;
            var listTopic = ClientEvaluationTracking.GetTopic(language)
                .OrderBy(x => x.Name).ToList();
            result.GetTopicIntervention = listTopic;
            result.GetTopicEvaluation = listTopic;
            result.GetMethodologyDem = ClientEvaluationTracking.GetMethodology(language);
            result.GetMainTopicEvaluation = listTopic;
            result.GetMainTopicIntervention = listTopic;
            result.GetMethodologyCurrent = ClientEvaluationTracking.GetMethodology(language)
                .OrderBy(x => x.Name).ToList();
            result.GetMethodologyCurrent.Remove(result.GetMethodologyCurrent
                .First(x => x.Code == "ETMETHNA"));
            result.GetMethodologyCurrent.Remove(result.GetMethodologyCurrent
                .First(x => x.Code == "ETMETHB2009"));
            result.GetStage = ClientEvaluationTracking.GetStage(language);
            result.GetResponsible = ClientEvaluationTracking.GetResponsible(language)
                .OrderBy(x => x.Name).ToList();
            result.GetSourceOfFunding = ClientEvaluationTracking.GetSourceOfFunding(language);
            result.GetDocumentPermission = ClientEvaluationTracking.GetDocumentPermission(language);
            result.GetTypeDocument = ClientEvaluationTracking.GetTypeDocument(language);
            result.GetTypeDocumentString = ClientEvaluationTracking.GetTypeDocumentString(language);
            result.GetAllStageString = ClientEvaluationTracking.GetAllStageString(language);

            return result;
        }

        private EvaluationTrackingModel InitEvaluationItem(string operationNumber)
        {
            var listApprovedOrNA = ClientEvaluationTracking
                .VerifyOperationApproved(operationNumber, IDBContext.Current.CurrentLanguage);

            var listItems = new List<SelectListItem>();

            listItems.Add(new SelectListItem
            {
                Text = listApprovedOrNA[0].Name,
                Value = Convert.ToString(listApprovedOrNA[0].ConvergenceMasterDataId),
                Selected = true
            });

            ViewBag.CodeNotChangedReason = listApprovedOrNA[0].Code;
            ViewBag.itemAprrovedOrNA = listItems;
            int idMatrix = ClientEvaluationTracking.GetResultsMatrix(operationNumber);

            EvaluationTrackingModel model = LoadDropDownList();

            var demMethodology = model.GetMethodologyDem.ToList();

            foreach (var item in demMethodology)
            {
                if (!listItems.Any(x => x.Value == item.ConvergenceMasterDataId.ToString()))
                {
                    model.GetMethodologyDem.Remove(item);
                }
            }

            model.ResultsMatrixId = idMatrix;
            model.OperationNumber = operationNumber;
            model.MethodologyCurrentId = model.GetMethodologyCurrent[0].ConvergenceMasterDataId;
            FilterStage(model);
            model.ResultsMatrix = ClientEvaluationTracking.GetHeaderResultsMatrix(operationNumber);

            return model;
        }
    }
}