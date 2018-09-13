using System;
using System.Web.Mvc;

using IDB.MW.Application.Core.Services.CatalogService;
using IDB.MW.Application.VMRModule.Messages.Request;
using IDB.MW.Application.VMRModule.Services.MyMeetings;
using IDB.MW.Application.VMRModule.Services;
using IDB.MW.Application.VMRModule.Services.Comment;
using IDB.MW.Application.VMRModule.Services.GenericService;

using IDB.Presentation.MVC4.Areas.VMR.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;
using IDB.MW.Business.VMRModule.Contracts;

namespace IDB.Presentation.MVC4.Areas.VMR.Controllers
{
    public partial class MyMeetingsController : BaseController
    {
        #region Fields
        private readonly ViewModelMapperHelper _viewModelMapperHelper;
        private readonly ICatalogService _catalogService;
        private readonly IVmrService _vmrService;
        private readonly IMyMeetingsService _myMeetingsService;
        private readonly IVmrGenericService _vmrGenericService;
        private readonly ICommentService _commentService;
        private readonly ICommentRulesService _commentRulesService;
        #endregion

        #region Constructor

        public MyMeetingsController(ICatalogService catalogService,
            IVmrService vmrService,
            IMyMeetingsService myMeetingsService,
            IVmrGenericService vmrGenericService,
            ICommentService commentService,
            ICommentRulesService commentRulesService)
        {
            _catalogService = catalogService;
            _vmrService = vmrService;
             _myMeetingsService = myMeetingsService;
            _vmrGenericService = vmrGenericService;
            _commentService = commentService;
            _commentRulesService = commentRulesService;
            _viewModelMapperHelper = new ViewModelMapperHelper(
                                        ViewBag,
                                        _vmrService, 
                                        _catalogService,
                                        _vmrGenericService,
                                        _commentService,
                                        _commentRulesService);
        }
        #endregion

        public virtual ActionResult Index(string operationNumber)
        {
            var model = _myMeetingsService.GetMyMeetings(operationNumber).MyMeetingsTable;
             
            ViewBag.SerializedVmrInstancesViewModel = PageSerializationHelper.SerializeObject(model);
            _viewModelMapperHelper.GetMyMeetingsViewModel();

            return View(model);
        }

        #region Content
        public virtual ActionResult MyMeetingsContent(string operationNumber)
        {
            var model = _myMeetingsService.GetMyMeetings(operationNumber).MyMeetingsTable;
            _viewModelMapperHelper.IsEditableScreen(true);
            ViewBag.SerializedVmrInstancesViewModel = PageSerializationHelper.SerializeObject(model);
            _viewModelMapperHelper.GetMyMeetingsViewModel();
            return PartialView("Partials/DataTables/MyMeetingsTable", model.MyMeetingsList);
        }

        public virtual ActionResult FilterTableMyMeetings(
            string operationNumber, DateTime? meetingDate, int filterType, string operationName)
        {
            var filterRequest = new MyMeetingFilterRequest
            {
                MeetingDate = meetingDate,
                FilterType = filterType,
                OperationData = operationName,
                OperationNumber = operationNumber
            };

            var response =
                _myMeetingsService
                    .FilterMyMeetings(filterRequest).MeetingsList.MyMeetingsList;
            
            return PartialView("Partials/DataTables/MyMeetingsTable", response);
        }
        #endregion
    }
}