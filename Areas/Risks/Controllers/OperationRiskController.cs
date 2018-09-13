using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;

using IDB.Architecture.Language;
using IDB.Architecture.Logging;
using IDB.Domain.Attributes;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Risks;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Risks;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVC4.Report;
using IDB.Presentation.MVCExtensions;
using IDB.MW.Application.PCRModule.Helpers;
using IDB.MW.DomainModel.Contracts.Repositories.PCRModule;
using IDB.MW.Domain.Contracts.ModelRepositories.Reports.Generic;
using IDB.MW.Domain.Entities;
using IDB.MW.Domain.Contracts.Specifications;
using IDB.MW.Domain.EntityHelpers;
using IDB.MW.Domain.Models.DocumentManagement;

namespace IDB.Presentation.MVC4.Areas.Risks.Controllers
{
    //[Interceptor(typeof(IExceptionHandlingAspect))]
    public partial class OperationRiskController : BaseController
    {
        public string Lang = IDBContext.Current.CurrentLanguage;
        private CommonDocument DoccumentObject = new CommonDocument();

        #region WCF Services

        private IOperationModelRepository _operation = null;
        public virtual IOperationModelRepository operation
        {
            get { return _operation; }
            set { _operation = value; }
        }

        private IPCRWorkflowStatusRepository _pcrWorkflowStatusRepository;
        public virtual IPCRWorkflowStatusRepository PCRWorkflowStatusRepository
        {
            get { return _pcrWorkflowStatusRepository; }
            set { _pcrWorkflowStatusRepository = value; }
        }

        private IReportsGenericRepository _clientGenericRepository = null;
        public IReportsGenericRepository ClientGenericRepository
        {
            get { return _clientGenericRepository; }
            set { _clientGenericRepository = value; }
        }

        #endregion

        // GET: /Risks/OperationRisk/Details
        [ExceptionHandling]
        [Authorize]
        public virtual ActionResult Details(string operationNumber, MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            var currentOperation = operation.GetOperationByOperationNumber(operationNumber);
            string[] masterDataItems = new string[] { "RISK_STATUS", "RISK_TYPE", "RISK_TARGET_AUDIENCE", "RISK_PROBABILITY", "RISK_IMPACT", "RISK_LEVEL" };
            var masterDataDetail = operation.GetMasterDataDetails(masterDataItems, Lang);
            this.ViewBag.listrisktype = masterDataDetail["RISK_TYPE"];
            this.ViewBag.listrisktargetaudience = masterDataDetail["RISK_TARGET_AUDIENCE"];
            this.ViewBag.listriskstatus = masterDataDetail["RISK_STATUS"];
            this.ViewBag.listriskprobability = operation.GetRiskProbability(masterDataDetail["RISK_PROBABILITY"]);
            this.ViewBag.listriskimpact = operation.RiskImpact(masterDataDetail["RISK_IMPACT"]);
            this.ViewBag.listrangerisklevel = operation.RangeRiskLevel();
            var operationmodel = operation.GetAllOperationRisks(currentOperation, Lang, masterDataDetail);

            ViewData["SelectedRiskType"] = string.Empty;
            ViewData["DescriptionFilter"] = string.Empty;
            ViewData["RiskLevelFilter"] = string.Empty;
            ViewData["RiskTargetFilter"] = string.Empty;
            ViewData["RiskStatusFilter"] = string.Empty;
            ViewData["OperationNumberFilter"] = string.Empty;
            ViewData["Message"] = null;
            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);
                ViewData["message"] = message;
            }

            ViewBag.isLessTaskFive = PCRHelpers.IsCurrentTaskLessThanFive(
                  ClientGenericRepository, PCRWorkflowStatusRepository, currentOperation.OperationId);
            ViewBag.RMAdministrator = IDBContext.Current.HasRole(Role.RM_ADMINISTRATOR);
            return View(operationmodel);
        }

        public virtual ActionResult FilterOperationRisk(
            OperationRiskViewModel model,
            List<Tuple<int, string>> riskType,
            List<Tuple<string, decimal, decimal>> riskLeven,
            List<Tuple<int, string>> riskTargetAudience,
            List<Tuple<int, string>> riskStatus,
            List<Tuple<int, int, string>> riskProbability,
            List<Tuple<int, int, string>> riskImpact)
        {
            this.ViewBag.listrisktype = riskType;
            this.ViewBag.listrangerisklevel = riskLeven;
            this.ViewBag.listrisktargetaudience = riskTargetAudience;
            this.ViewBag.listriskstatus = riskStatus;
            this.ViewBag.listriskprobability = riskProbability;
            this.ViewBag.listriskimpact = riskImpact;
            return View(model);
        }

        [HttpPost()]
        public virtual ActionResult FilterOperationRisk(string operationNumber, string operationNumberFilter, string descriptionFilter, int? riskTypeFilter, string riskLevelFilter, int? riskTargetFilter, int? riskStatusFilter, string operationParent)
        {
            var currentOperation = operation.GetOperationByOperationNumber(operationNumber);
            string[] masterDataItems = new string[] { "RISK_STATUS", "RISK_TYPE", "RISK_TARGET_AUDIENCE", "RISK_PROBABILITY", "RISK_IMPACT", "RISK_LEVEL" };
            var masterDataDetail = operation.GetMasterDataDetails(masterDataItems, Lang);
            this.ViewBag.listrisktype = masterDataDetail["RISK_TYPE"];
            this.ViewBag.listrangerisklevel = operation.RangeRiskLevel();
            this.ViewBag.listrisktargetaudience = masterDataDetail["RISK_TARGET_AUDIENCE"];
            this.ViewBag.listriskstatus = masterDataDetail["RISK_STATUS"];
            this.ViewBag.listriskprobability = operation.GetRiskProbability(masterDataDetail["RISK_PROBABILITY"]);
            this.ViewBag.listriskimpact = operation.RiskImpact(masterDataDetail["RISK_IMPACT"]);

            if (operationNumberFilter != "All")
            {
                var riskOperationModel = operation.GetAllOperationRisksFilter(
                    currentOperation,
                    operationNumberFilter,
                    descriptionFilter,
                    riskTypeFilter,
                    riskLevelFilter,
                    riskTargetFilter,
                    riskStatusFilter,
                    Lang,
                    masterDataDetail);

                ViewData["SelectedRiskType"] = riskTypeFilter;
                ViewData["DescriptionFilter"] = descriptionFilter;
                ViewData["RiskLevelFilter"] = riskLevelFilter;
                ViewData["RiskTargetFilter"] = riskTargetFilter;
                ViewData["RiskStatusFilter"] = riskStatusFilter;
                ViewData["OperationNumberFilter"] = operationNumberFilter;

                return View(riskOperationModel);
            }

            return RedirectToAction("Details", new { operationNumber = operationParent });
        }

        //[ExceptionHandling]
        [HttpGet]
        [HasPermission(Permissions = "Risks Write")]
        [Authorize]
        public virtual ActionResult Edit(string operationNumber)
        {
            string[] masterDataItems = new string[] { "RISK_STATUS", "RISK_TYPE", "RISK_TARGET_AUDIENCE", "RISK_PROBABILITY", "RISK_IMPACT", "RISK_LEVEL" };
            var currentOperation = operation.GetOperationByOperationNumber(operationNumber);
            var masterDataDetail = operation.GetMasterDataDetails(masterDataItems, Lang);
            var listoperationrisklevelQuery = operation.GetOperationRiskLevel(masterDataDetail["RISK_LEVEL"]);

            this.ViewBag.listrisktype = masterDataDetail["RISK_TYPE"];
            this.ViewBag.listrangerisklevel = operation.RangeRiskLevel();
            this.ViewBag.listrisktargetaudience = masterDataDetail["RISK_TARGET_AUDIENCE"];
            this.ViewBag.listriskstatus = masterDataDetail["RISK_STATUS"];
            this.ViewBag.listriskprobability = operation.GetRiskProbability(masterDataDetail["RISK_PROBABILITY"]);
            this.ViewBag.listriskimpact = operation.RiskImpact(masterDataDetail["RISK_IMPACT"]);
            var listoperationrisklevel = new List<SelectListItem>();

            foreach (var operationrisklevel in listoperationrisklevelQuery)
            {
                var item = new SelectListItem();
                item.Value = operationrisklevel.Item1.ToString();
                item.Text = operationrisklevel.Item3;
                listoperationrisklevel.Add(item);
            }

            this.ViewBag.listoperationrisklevel = listoperationrisklevel;

            var operationmodel = operation.GetAllOperationRisks(currentOperation, Lang, masterDataDetail);

            ViewData["SelectedRiskType"] = string.Empty;
            ViewData["DescriptionFilter"] = string.Empty;
            ViewData["RiskLevelFilter"] = string.Empty;
            ViewData["RiskTargetFilter"] = string.Empty;
            ViewData["RiskStatusFilter"] = string.Empty;
            ViewData["OperationNumberFilter"] = string.Empty;
            if (TempData.ContainsKey("IndexDocumentRelationshipErrorDelete"))
            {
                ViewBag.DeleteError = ((Exception)TempData["IndexDocumentRelationshipErrorDelete"]).Message;
                TempData.Remove("IndexDocumentRelationshipErrorDelete");
            }

            if (TempData.ContainsKey("IndexDocumentRelationshipError"))
                ViewBag.UploadFileError = ((Exception)TempData["IndexDocumentRelationshipError"]).Message;

            Logger.GetLogger().WriteDebug("OperationRiskController",
                  "Before redirect Edit view");
            return View(operationmodel);
        }

        [HttpPost]
        [ExceptionHandling]
        [Authorize]
        public virtual ActionResult EditSave(OperationRiskViewModel riskViewModel)
        {
            riskViewModel.userName = IDBContext.Current.UserName;
            bool operationUpdate = operation.UpdateOperationRisk(riskViewModel);
            MessageNotificationCodes messageStatus = MessageNotificationCodes.SaveDataFail;
            if (operationUpdate)
            {
                messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            }

            return RedirectToAction("Details", "OperationRisk", new { area = "Risks", operationNumber = riskViewModel.OperationNumber, messageStatus = messageStatus });
        }

        public virtual ActionResult ConfirmDeleteDocument(string operationNumber, int operationRiskID, int documentId)
        {
            this.ViewData["operationNumber"] = operationNumber;
            this.ViewData["OperationRiskID"] = operationRiskID;
            this.ViewData["documentId"] = documentId;
            return PartialView();
        }

        public virtual ActionResult DownloadReport(string operationNumber, string format = "")
        {
            if ((format == "pdf") || (format == "xls"))
            {
                string[] masterDataItems = new string[] { "RISK_STATUS", "RISK_TYPE", "RISK_TARGET_AUDIENCE", "RISK_PROBABILITY", "RISK_IMPACT", "RISK_LEVEL" };
                var currentOperation = operation.GetOperationByOperationNumber(operationNumber);
                var masterDataDetail = operation.GetMasterDataDetails(masterDataItems, Lang);
                var listrisktype = masterDataDetail["RISK_TYPE"];
                var listrangerisklevel = operation.RangeRiskLevel();
                var listrisktargetaudience = masterDataDetail["RISK_TARGET_AUDIENCE"];
                var listriskstatus = masterDataDetail["RISK_STATUS"];
                var listriskprobability = operation.GetRiskProbability(masterDataDetail["RISK_PROBABILITY"]);
                var listriskimpact = operation.RiskImpact(masterDataDetail["RISK_IMPACT"]);
                var operationmodel = operation.GetAllOperationRisks(currentOperation, Lang, masterDataDetail);

                ReportBuilder RB = new ReportBuilder();
                string Result = new ReportBuilder().Build_OperationRisk(operationmodel, listrisktype, listrangerisklevel, listrisktargetaudience, listriskstatus, listriskprobability, listriskimpact, format);

                string name = IDBContext.Current.Operation + "_" + Localization.GetText("OperationRisk");
                string filename = name + "." + format;
                Response.AppendHeader("Content-Disposition", "inline;filename=" + filename);

                if (format == "pdf")
                {
                    Response.ContentType = "application/vnd.pdf";
                }
                else
                {
                    Response.ContentType = "application/vnd.ms-excel";
                    Response.Write(Result);
                }

                Response.End();
            }

            return View();
        }

        [HttpPost]
        [Authorize]
        public virtual ActionResult DeleteDocumentRisk(
            OperationRiskViewModel riskViewModel,
            string deleteDocumentNumber)
        {
            if (riskViewModel != null)
            {
                riskViewModel.DocumentsRecord.Remove(riskViewModel.DocumentsRecord
                    .FirstOrDefault(x => x.DocumentNumber == deleteDocumentNumber));
                riskViewModel.TableDocumentsRisk = riskViewModel.DocumentsRecord.PopulateDataTable();
            }

            return PartialView("AddDocumentRiskPartial", riskViewModel);
        }
    }
}