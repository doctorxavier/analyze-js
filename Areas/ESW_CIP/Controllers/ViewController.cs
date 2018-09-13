using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Configuration;

using IDB.MVCControls.General.Helpers;
using IDB.MW.Application.ESW_CIPModule.Messages;
using IDB.MW.Application.ESW_CIPModule.Services;
using IDB.MW.Domain.Session;
using IDB.MW.Infrastructure.ReportManager.Enums;
using IDB.MW.Infrastructure.SecurityService;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.ESW_CIP.Controllers
{
    public partial class ViewController : MVC4.Controllers.ConfluenceController
    {
        #region Constants
        public static string CHART_PROPOSAL = "UI-ESWCIP-Proposal";
        public static string CHART_ANNUAL_ALLOCATION = "UI-ESWCIP-AnnualAllocation";
        #endregion

        #region Fields
        private readonly IESWCIPService _eswcipService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ISecurityModelRepository _securityModelRepository;
        #endregion

        #region Constructors
        public ViewController(
            IESWCIPService eswcipService,
            ISecurityModelRepository securityModelRepository)
        {
            _eswcipService = eswcipService;
            _authorizationService = AuthorizationServiceFactory.Current;
            _securityModelRepository = securityModelRepository;
        }
        #endregion

        // GET: ESW_CIP/View
        public virtual ActionResult ProposalList(string operationNumber)
        {
            var viewModel = _eswcipService.GetProposals(operationNumber);

            return View(viewModel.ProposalsList);
        }

        public virtual ActionResult Proposal(
            string proposalOperationId,
            string proposalYear,
            string proposalType)
        {
            ProposalResponse response = _eswcipService
                .GetProposal(proposalOperationId, proposalYear, proposalType);

            response.Proposal.HasProposalWritePermission = 
                IDBContext.Current.HasPermission(Permission.PROPOSAL_TL_WRITE) || 
                IDBContext.Current.HasPermission(Permission.PROPOSAL_WRITE);

            response.Proposal.FieldAccessList = _securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                CHART_PROPOSAL,
                IDBContext.Current.Permissions,
                0,
                0).ToList();

            ViewBag.DeliverableTypes = _eswcipService.GetDeliverablesTypes();
            ViewBag.SerializedViewModel = Helpers.PageSerializationHelper
                .SerializeObject(response.Proposal);
            ViewBag.CountryList = _eswcipService.GetCountries();

            var responseOrgUnits = _eswcipService.GetOrganizationalUnits(
                response.Proposal.GeneralInformation
                .OrganizationalUnit.AsQueryable().Select(x => x.UnitCode));

            if (!responseOrgUnits.IsValid)
                responseOrgUnits.OrganizationalUnits = new List<SelectListItem>();

            ViewBag.OrganizationalUnits = responseOrgUnits.OrganizationalUnits;

            return View(response.Proposal);
        }

        public virtual ActionResult AnnualAllocation(string operationNumber)
        {
            var response = _eswcipService.GetAnnualAllocation(operationNumber, true);

            response.AnnualAllocationContainer.HasAnnualAllocationWritePermission = 
                IDBContext.Current.HasPermission(Permission.ANNUAL_ALLOCATION_WRITE);
            ViewBag.OperationModality = response.AnnualAllocationContainer.OperationModality;
            ViewBag.OperationYear = response.AnnualAllocationContainer.OperationYear;

            ViewBag.SerializedViewModel = Helpers.PageSerializationHelper
                .SerializeObject(response.AnnualAllocationContainer);

            response.AnnualAllocationContainer.FieldAccessList = _securityModelRepository
                .GetFieldBehaviorByPermissions(
                    IDBContext.Current.Operation,
                    CHART_ANNUAL_ALLOCATION,
                    IDBContext.Current.Permissions,
                    0,
                    0).ToList();

            ViewBag.SerializedViewModel = Helpers.PageSerializationHelper
                .SerializeObject(response.AnnualAllocationContainer);
            ViewBag.SerializedViewModel = Helpers.PageSerializationHelper
                .SerializeObject(response.AnnualAllocationContainer);

            return View(response.AnnualAllocationContainer);
        }

        public virtual ActionResult GetPartialView(
            string operationNumber,
            string partial,
            string inputIdentifier,
            string addedOperationNumbers)
        {
            if (inputIdentifier != string.Empty && inputIdentifier != null)
            {
                ViewBag.inputID = inputIdentifier;

                return PartialView(partial);
            }

            var responseOrgUnits = _eswcipService.GetOrganizationalUnits(null, false);

            if (!responseOrgUnits.IsValid)
                responseOrgUnits.OrganizationalUnits = new List<SelectListItem>();

            ViewBag.OrganizationalUnits = responseOrgUnits.OrganizationalUnits;

            ViewBag.OperationsList = _eswcipService
                .GetOperations(operationNumber, addedOperationNumbers)
                .AnnualAllocationsList;

            return PartialView(partial);
        }

        public virtual JsonResult GetOperations(
            string operationNumber, 
            string operationNumberForSearch, 
            string operationYear, 
            string addedOperationNumbers, 
            string division)
        {
            var result = Json(new { data = string.Empty }, JsonRequestBehavior.AllowGet);

            GetAnnualAllocationOperationResponse response = _eswcipService
                .GetOperations(operationNumber, addedOperationNumbers);

            result.Data = new JavaScriptSerializer().Serialize(response);

            return result;
        }

        public virtual JsonResult AddNewDocument(string documentNumber)
        {
            var document = new
            {
                DocumentNumber = documentNumber,
                Date = FormatHelper.Format(DateTime.Now.Date),
                User = IDBContext.Current.UserName,
                Url = MW.Domain.EntityHelpers.DownloadDocumentHelper.GetDocumentLink(documentNumber)
            };

            var result = Json(new { data = document }, JsonRequestBehavior.AllowGet);

            return result;
        }

        public virtual FileResult DownloadAAExcelReport(string operationNumber)
        {
            var responseGetAA = _eswcipService.GetAnnualAllocation(operationNumber, true);

            var responseDownload = _eswcipService.DownloadAAReport(
                responseGetAA.AnnualAllocationContainer, 
                OutputFormatEnum.Excel);

            string nameReport = string.Format("Annual Allocation {0}.xls", operationNumber);
            return !responseDownload.IsValid ? null : File(responseDownload.File, "application/vnd.ms-excel", nameReport);
        }

        public virtual FileResult DownloadProposalReport(
            string operationNumber, 
            string proposalOperationId, 
            string proposalYear, 
            string proposalType, 
            string formatType)
        {
            ProposalResponse responseGetProposal = _eswcipService.GetProposal(
                proposalOperationId, 
                proposalYear, 
                proposalType);

            var responseDownload = _eswcipService.DownloadProposalReport(
                responseGetProposal.Proposal, 
                formatType);

            var reportName = "ProposalReport-" + operationNumber + "." + formatType;

            return !responseDownload.IsValid ? 
                null : File(responseDownload.File, "application/" + formatType, reportName);
        }
    }
}