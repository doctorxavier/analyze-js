using System;
using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Application.MrBlueModule.Services.DynamicQuestionnaire;
using IDB.MW.Application.MrBlueModule.Services.Keywords;
using IDB.MW.Application.MrBlueModule.Services.Tracking;
using IDB.MW.Application.MrBlueModule.ViewModels.DynamicQuestionnaire;
using IDB.MW.Application.MrBlueModule.ViewModels.ManageMediaFiles;
using IDB.Presentation.MVC4.Areas.MrBlue.Models.Fake;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class TestController : MVC4.Controllers.ConfluenceController
    {
        private readonly IDynamicQuestionnaireService _dynamicQuestionnaireService;
        private readonly ITrackingDifferenceService _trackingDifferenceService;
        private readonly IKeywordService _keywordService;

        public TestController(IDynamicQuestionnaireService dynamicQuestionnaireService, IKeywordService keywordService, ITrackingDifferenceService trackingDifferenceService)
        {
            _dynamicQuestionnaireService = dynamicQuestionnaireService;
            _trackingDifferenceService = trackingDifferenceService;
            _keywordService = keywordService;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult QuestionnarieDemo1(int levelId = 100, int versionId = 0)
        {
            var viewModel = new QuestionnarieDemo1ViewModel
            {
                Id = 1,
                ViewModel = _dynamicQuestionnaireService.GetQuestionnaire(levelId, versionId)
            };

            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(viewModel.ViewModel);

            return View(viewModel);
        }

        public virtual JsonResult Save(string operationNumber)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<DynamicQuestionnaireViewModel>(jsonDataRequest.SerializedData);

            return Json(true);
        }

        // Tooltip
        public virtual ActionResult TestTooltip(string operationNumber)
        {
            string model = "Activities to be financed by the project are in a geographical area and sector exposed to <a href=\"javascript:ShowTerm('Natural Hazards', 1)\">natural hazards* </a>";
            return View(model: model);
        }

        public virtual JsonResult GetKeywordByTerm(string term)
        {
            var response = _keywordService.GetKeywordByTerm(term);
            return Json(new { IsValid = response.IsValid, ErrorMessage = response.ErrorMessage, Data = response });
        }

        //Tracking
        public virtual ActionResult TestTracking(string operationNumber)
        {
            return View();
        }

        public virtual ActionResult SetTracking(string operationNumber)
        {
            _trackingDifferenceService.RegisterTrackingDifference(operationNumber, 1);
            return View("TestTracking");
        }

        //Media File
        public virtual ActionResult TestMediaFile(string operationNumber)
        {
            List<ManageMediaFilesRowViewModel> listamodel = new List<ManageMediaFilesRowViewModel>();
            listamodel.Add(new ManageMediaFilesRowViewModel { RowId = 1, FileDate = DateTime.Now.ToString(), Caption = "File Media 1", Name = "MockUser" });
            return View(listamodel);
        }
    }
}