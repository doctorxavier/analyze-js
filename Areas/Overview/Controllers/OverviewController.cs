using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Web.Mvc;

using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.UserProfiles;

using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.MW.Application.CaseManager.Business;
using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.Core.ViewModels.FormRendering;
using IDB.MW.Application.OPUSModule.Services.OperationDataService;
using IDB.MW.Application.OverviewModule.Services;
using IDB.MW.Domain.Contracts.DomainServices;
using IDB.MW.Domain.Contracts.ModelRepositories.Overview;
using IDB.MW.Domain.Values;
using IDB.Presentation.MVC4.Areas.Global.Controllers;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

[assembly: InternalsVisibleTo("UnitTests")]
namespace IDB.Presentation.MVC4.Areas.Overview.Controllers
{
    public partial class OverviewController : BaseController
    {
        #region Fields
        private readonly IOperationOverviewModelRepository _operationOverviewModelRepository;
        private readonly IOperationDataService _operationDataService;
        private readonly ILoanOperationDataService _clientLoan;
        private readonly IOverviewService _overviewService;
        private readonly ICatalogService _catalogService;
        #endregion

        internal bool TestMode { get; set; }

        public OverviewController(
            IOperationDataService operationDataService,
            ILoanOperationDataService clientLoan,
            IOperationOverviewModelRepository operationOverviewModelRepository,
            IOverviewService overviewService,
            ICatalogService catalogService)
        {
            _operationDataService = operationDataService;
            _clientLoan = clientLoan;
            _operationOverviewModelRepository = operationOverviewModelRepository;
            _overviewService = overviewService;
            _catalogService = catalogService;
        }

        public virtual ActionResult Index(string operationNumber)
        {
            var typeResponse = _operationDataService.GetOperationType(operationNumber);
            ViewBag.TypeOperation = typeResponse.OperationType;

            if (!TestMode)
                GlobalCommonLogic.SetLastOperation(operationNumber);

            var modelOverview = _operationOverviewModelRepository.GetOverviews(operationNumber, Localization.CurrentLanguage);
            modelOverview.Objetive = _operationOverviewModelRepository.GetObjetive(operationNumber, Localization.CurrentLanguage);
            modelOverview.BasicData.Responsible = _operationOverviewModelRepository.GetResponsible(operationNumber);
            ViewBag.OperationNumber = operationNumber;
            ViewBag.showLoader = true;

            string[] masterTypeList = new string[]
            {
                MasterType.SECTOR
            };

            var masterDataListByTypeCode = _catalogService
                .GetMasterDataListByTypeCode(typeCodes: masterTypeList);

            var operationSectorList = _catalogService.GetListMasterData(
                MasterType.SECTOR, listRepository: masterDataListByTypeCode);

            modelOverview.BasicData.Sector = GetSectorOverview(modelOverview.BasicData.SectorID, operationSectorList);

            try
            {
                modelOverview.EventsData.PartialEligibilityDate = _clientLoan.GetMinElegibilityDateByLoans(operationNumber);
            }
            catch
            {
                modelOverview.EventsData.PartialEligibilityDate = new DateTime();
            }

            var attributes = _operationDataService.GetAttributesBasicResponse(operationNumber);

            if (!TestMode)
                ViewBag.IsAppr = CMBusiness.Get().Context.APPRMilestone.IsCompleted(false);

            ViewBag.FormBasicAttributes = attributes.IsValid ? attributes.Attributes.FormAttributes : new FormDataViewModel();

            ViewBag.operationRelated = _overviewService.GetRelation(operationNumber);
            ViewBag.EventData = _overviewService.GetEventData(operationNumber);

            return View(modelOverview);
        }

        public virtual ActionResult IndexTeamMembers(int OperationId = -1)
        {
            string urlImage = string.Empty;
            var model = _overviewService.GetUsersTeamMembers(OperationId);

            foreach (var teamMember in model)
            {
                urlImage = GetSharepointPictureUrl(teamMember.UserName);
                if (urlImage != null)
                    teamMember.PictureUrl = urlImage;
                else
                    teamMember.PictureUrl = string.Empty;
            }

            return PartialView(model);
        }

        public virtual string GetSectorOverview(int? sectorID, IList<SelectListItem> operationSectorList)
        {
            var sectorIDtoString = sectorID.ToString();

            var sectorName = sectorID != null && operationSectorList.Any(x => x.Value == sectorIDtoString) ?
                operationSectorList.FirstOrDefault(x => x.Value == sectorIDtoString).Text :
                null;

            return sectorName;
        }

        private static string GetSharepointPictureUrl(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                username = username.Trim(); //To avoid white spaces wrongly given by the repository
            }

            string serverUrl = Globals.GetSetting("BasePath");
            var serverUrlImage = Globals.GetSetting("BasePath") + @"/Mainframe/CachedGetUserPhotoThumbnail"; //Url.Action("CachedGetUserPhotoThumbnail", "Mainframe"); 

            return string.Format(
                "{0}?userName={1}&width={2}&height={3}",
                serverUrlImage,
                username,
                30, 
                32);
        }
    }
}
