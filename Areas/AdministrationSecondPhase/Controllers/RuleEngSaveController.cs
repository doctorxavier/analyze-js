using System.Web.Mvc;

using IDB.MW.Application.AdministrationModule.Services.RuleEngService;
using IDB.MW.Application.AdministrationModule.ViewModels.RuleEngService;
using IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.RuleEng;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Controllers
{
    public partial class RuleEngSaveController : BaseController
    {
        private readonly IRuleEngService _ruleEngService;

        #region Contructors
        public RuleEngSaveController(IRuleEngService ruleEngService)
        {
            _ruleEngService = ruleEngService;
        }

        #endregion

        public virtual JsonResult RulesCreateAndEditSave(int idRule = 0)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<RuleEngViewModel>(jsonDataRequest.SerializedData);
            viewModel.UpdateRuleEngViewModel(jsonDataRequest.ClientFieldData);

            var response = _ruleEngService.SaveRulesCreateAndEdit(viewModel, idRule);

            return Json(response);
        }

        public virtual JsonResult RulesGroupCreateAndEditSave(int idGroup = 0, string classification = "")
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper
                .DeserializeObject<RuleEngGroupViewModel>(jsonDataRequest.SerializedData);
            viewModel.UpdateRuleEngGroupViewModel(jsonDataRequest.ClientFieldData);

            var response = _ruleEngService.SaveRulesGroupCreateAndEdit(viewModel, idGroup, classification);

            return Json(response);
        }
    }
}