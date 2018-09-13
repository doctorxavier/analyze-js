using System;
using System.Web.Mvc;

using IDB.Domain.Attributes;
using IDB.MW.Domain.Contracts.ModelRepositories.Architecture.Risks;
using IDB.MW.Domain.Models.Architecture.Enumerations;
using IDB.MW.Domain.Models.Architecture.Risks;
using IDB.MW.Domain.Models.Risks;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.Presentation.MVCExtensions;

namespace IDB.Presentation.MVC4.Areas.Risks.Controllers
{
    public partial class RiskController : BaseController
    {
        public string Lang = IDBContext.Current.CurrentLanguage;
        #region WCF Services

        private IOperationModelRepository _operation = null;
        public virtual IOperationModelRepository operation
        {
            get { return _operation; }
            set { _operation = value; }
        }

        #endregion

        #region Risk CRUD

        [HttpGet]
        public virtual ActionResult Details(string operationNumber, int riskId, MessageNotificationCodes messageStatus = MessageNotificationCodes.VoidMessage)
        {
            this.ViewBag.operationNumber = operationNumber;
            string[] masterDataItems = new string[] { "RISK_STATUS", "RISK_TYPE", "RISK_IMPACT", "RISK_PROBABILITY", "RISK_TARGET_AUDIENCE" };
            var masterDataDetail = operation.GetMasterDataDetails(masterDataItems, Lang);
            this.ViewBag.listRiskStatus = masterDataDetail["RISK_STATUS"];
            this.ViewBag.listRiskType = masterDataDetail["RISK_TYPE"];
            this.ViewBag.listRiskProbability = operation.GetRiskProbability(masterDataDetail["RISK_PROBABILITY"]);
            this.ViewBag.listRiskImpact = operation.RiskImpact(masterDataDetail["RISK_IMPACT"]);
            this.ViewBag.listRiskTargetAudience = masterDataDetail["RISK_TARGET_AUDIENCE"];
            RiskViewModel riskModel = operation.GetRiskByOperation(riskId);
            ViewData["Message"] = null;
            if (messageStatus != MessageNotificationCodes.VoidMessage)
            {
                MessageConfiguration message = MessageHandler.setMessage(messageStatus, true, 5);
                ViewData["message"] = message;
            }

            return View(riskModel);
        }

        [HttpGet]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult Create(string operationNumber, int operationId, int OperationRiskId, RiskViewModel riskModel)
        {
            this.ViewBag.operationNumber = operationNumber;
            this.ViewBag.listRiskStatus = operation.GetRiskStatus();
            this.ViewBag.listRiskType = operation.GetRiskType();
            this.ViewBag.listRiskProbability = operation.GetRiskProbability();
            this.ViewBag.listRiskImpact = operation.RiskImpact();
            this.ViewBag.listRiskTargetAudience = operation.RiskTargetAudience();
            ViewData["lista"] = operation.RiskTargetAudienceList();
            ViewData["ProbabilityList"] = operation.GetRiskProbability();
            ViewData["ImpactList"] = operation.RiskImpact();
            this.ViewBag.listRangeRiskLevel = operation.RangeRiskLevel();
            this.ViewBag.operationId = operationId;
            if (riskModel != null)
            {
                return View(riskModel);
            }
            else
            {
                var Model = new RiskViewModel() { OperationRiskId = OperationRiskId };
                return View(Model);
            }
        }

        [HttpPost]
        public virtual ActionResult CreateRisk(RiskViewModel riskViewModel, string operationNumber)
        {
            //TODO: FIX VALUES WITH THE TABLE
            riskViewModel.MyRiskStatus.Add(new RiskStatusModel()
            {
                RiskStatusId = -1,
                Description = riskViewModel.RiskStatusDescription,
                ChangeStatusDate = DateTime.Now,
                StatusId = riskViewModel.RiskStatusId
            });
            riskViewModel.userName = IDBContext.Current.UserName;

            var riskModel = operation.CreateNewRisk(riskViewModel);
            MessageNotificationCodes messageStatus = MessageNotificationCodes.SaveDataFail;
            if (riskModel != null)
            {
                messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            }

            return RedirectToAction(
                "Details",
                "Risk",
                new
                {
                    area = "Risks",
                    operationNumber = operationNumber,
                    riskId = riskModel != null ? riskModel.RiskId : default(int),
                    messageStatus = messageStatus
                });
        }

        [HttpGet]
        [HasPermission(Permissions = "Risks Write")]
        public virtual ActionResult Edit(string operationNumber, int riskId, RiskViewModel riskModel)
        {
            this.ViewBag.operationNumber = operationNumber;
            string[] masterDataItems = new string[] { "RISK_STATUS", "RISK_TYPE", "RISK_TARGET_AUDIENCE", "RISK_PROBABILITY", "RISK_IMPACT", "RISK_LEVEL" };
            var masterDataDetail = operation.GetMasterDataDetails(masterDataItems, Lang);
            this.ViewBag.listRiskStatus = masterDataDetail["RISK_STATUS"];
            this.ViewBag.listRiskType = masterDataDetail["RISK_TYPE"];
            this.ViewBag.listRiskProbability = operation.GetRiskProbability(masterDataDetail["RISK_PROBABILITY"]);
            this.ViewBag.listRiskImpact = operation.RiskImpact(masterDataDetail["RISK_IMPACT"]);
            this.ViewBag.listRiskTargetAudience = masterDataDetail["RISK_TARGET_AUDIENCE"];
            ViewData["listaEdit"] = operation.RiskTargetAudienceList(masterDataDetail["RISK_TARGET_AUDIENCE"]);
            ViewData["ProbabilityList"] = operation.GetRiskProbability(masterDataDetail["RISK_PROBABILITY"]);
            ViewData["ImpactList"] = operation.RiskImpact(masterDataDetail["RISK_IMPACT"]);
            this.ViewBag.listRangeRiskLevel = operation.RangeRiskLevel();
            this.ViewBag.idRisk = riskId;
            RiskViewModel rieskViewModel = operation.GetRiskByOperation(riskId);
            return View(rieskViewModel);
        }

        public virtual ActionResult UpdateRisk(string operationNumber, RiskViewModel riskViewModel)
        {
            riskViewModel.userName = IDBContext.Current.UserName;

            if (riskViewModel.MyRiskStatus[0].StatusId != riskViewModel.RiskStatusId)
            {
                riskViewModel.MyRiskStatus.Add(new RiskStatusModel
                {
                    RiskStatusId = 0,
                    RiskId = riskViewModel.RiskId,
                    StatusId = riskViewModel.RiskStatusId,
                    ChangeStatusDate = DateTime.Now,
                    Description = riskViewModel.RiskStatusDescription
                });
            }

            riskViewModel = operation.EditRisk(riskViewModel);

            MessageNotificationCodes messageStatus = MessageNotificationCodes.SaveDataFail;
            if (riskViewModel != null)
            {
                messageStatus = MessageNotificationCodes.SaveDataSucessfully;
            }

            return RedirectToAction("Details", "Risk", new { area = "Risks", operationNumber = operationNumber, riskId = riskViewModel != null ? riskViewModel.RiskId : default(int), messageStatus = messageStatus });
        }

        [ExceptionHandling]
        [HttpPost]
        public virtual ActionResult AddComments(string opNumber, RiskViewModel riskViewModel)
        {
            return RedirectToAction("Details", "Risk", new { area = "Risks", operationNumber = opNumber, riskId = riskViewModel.RiskId });
        }

        [HttpGet]
        public virtual ActionResult AddRiskComment(int userCommentId, string text)
        {
            var riskModel = operation.CreateRiskComment(userCommentId, text);
            return View();
        }

        [HttpGet]
        public virtual ActionResult EditRiskComment(int userCommentId, string text)
        {
            var riskModel = operation.UpdateComment(userCommentId, text);
            return View();
        }

        [HttpGet]
        public virtual ActionResult DeleteComment(int userCommentId)
        {
            var riskModel = operation.DeleteComment(userCommentId);
            return View();
        }
        #endregion
    }
}
