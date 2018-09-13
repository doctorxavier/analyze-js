using System;
using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Application.BusinessRulesModule.Messages;
using IDB.MW.Application.BusinessRulesModule.Services;
using IDB.MW.Business.BusinessRulesModule.DTOs;
using IDB.Presentation.MVC4.Areas.BusinessRules.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.BusinessRules.Controllers
{
    public partial class ViewController : BaseController
    {
        #region Fields
        private readonly IBusinessRulesService _businessRulesService;
        #endregion

        #region Constructors

        public ViewController(IBusinessRulesService businessRulesService)
        {
            _businessRulesService = businessRulesService;
        }

        #endregion

        #region BusinessRulesEngine

        public virtual ActionResult BusinessRulesEngine()
        {
            var response = _businessRulesService.Search(new BusinessRulesSearchRequest
            {
                Filters = new List<string>()
            });

            if (!response.IsValid)
                throw new Exception(response.ErrorMessage);

            return View(response.Results);
        }

        public virtual ActionResult SearchBusinessRules(string filter, string searchRadio)
        {
            var response = _businessRulesService.Search(new BusinessRulesSearchRequest
            {
                Filters = new List<string> { filter ?? string.Empty, searchRadio ?? string.Empty }
            });

            if (!response.IsValid)
                throw new Exception(response.ErrorMessage);

            return PartialView("Partials/BusinessRulesEngine/BusinessRulesEngineTablePartial", response.Results);
        }
        #endregion

        #region BusinessRulesList

        public virtual ActionResult BusinessRulesList()
        {
            var response = _businessRulesService.Search(new BusinessRulesSearchRequest
            {
                Filters = new List<string>()
            });

            if (!response.IsValid)
                throw new Exception(response.ErrorMessage);

            return View(response.Results);
        }

        #endregion

        #region BackendRules

        // GET: /BusinessRules/BackendBusinessRuleEditor/
        public virtual ActionResult BackendBusinessRuleEditor(string brCode, bool editMode = false)
        {
            var model = brCode != null ? LoadBackendRule(brCode) : new BRERuleDTO();
            ViewBag.EditMode = editMode;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual ActionResult BackendBusinessRuleEditorPartial(string brCode)
        {
            var model = brCode != null ? LoadBackendRule(brCode) : new BRERuleDTO();
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partials/BackendBusinessRuleEditor/BackendBusinessRuleEditorPartial", model);
        }

        [ValidateInput(false)]
        public virtual JsonResult Compile(string brid, string sourcecode)
        {
            var response = _businessRulesService.Compile(sourcecode);
            return new JsonResult
            {
                Data = new
                {
                    response.IsValid,
                    response.ErrorMessage,
                    response.CompilerResults.Output,
                    response.CompilerResults.Errors
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #endregion

        #region FrontEndRules

        public virtual ActionResult FrontEndBusinessRuleEditor(string brCode, bool editMode = false)
        {
            var model = brCode != null ? LoadFrontEndRule(brCode) : new BRERuleDTO();
            ViewBag.EditMode = editMode;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return View(model);
        }

        public virtual ActionResult FrontendBusinessRuleEditorPartial(string brCode)
        {
            var model = brCode != null ? LoadFrontEndRule(brCode) : new BRERuleDTO();
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return PartialView("Partials/FrontendBusinessRuleEditor/FrontendBusinessRuleEditorPartial", model);
        }

        [ValidateInput(false)]
        public virtual JsonResult ParseJS(string brid, string sourcecode)
        {
            var response = _businessRulesService.ParseJS(sourcecode);
            return new JsonResult
            {
                Data = new
                {
                    response.IsValid,
                    response.ErrorMessage
                },
                JsonRequestBehavior = JsonRequestBehavior.AllowGet
            };
        }

        #endregion

        #region BRTest

        public virtual ActionResult BRTest()
        {
            var response = _businessRulesService.Execute("RULE995",
                new BusinessRuleRequest
                {
                    Input = new Dictionary<string, dynamic>
                    {
                        { "CurrentIDB", 100 },
                        { "TotalDisbursed", 200 },
                        { "Sample", new SampleViewModel { Value = "123" } }
                    }
                });

            return View(new SampleViewModel
            {
                Value = response.Output["CalculatedValue"].ToString()
            });
        }

        #endregion

        #region Private Methods

        private BRERuleDTO GetMockModel()
        {
            return new BRERuleDTO
            {
                Code = "BR_TEST_1",
                Description = "Test business Rule",
                SourceCode = _businessRulesService.GetBackendRuleTemplate()
            };
        }

        private BRERuleDTO LoadBackendRule(string brCode)
        {
            var response = _businessRulesService.GetBackendRule(brCode);
            if (!response.IsValid)
            {
                throw new Exception("Error loading BR " + brCode);
            }

            var model = response.Rule;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return model;
        }

        private BRERuleDTO LoadFrontEndRule(string brCode)
        {
            var response = _businessRulesService.GetFrontendRule(brCode);
            if (!response.IsValid)
            {
                throw new Exception("Error loading BR " + brCode);
            }

            var model = response.Rule;
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            return model;
        }
        
        #endregion
    }
}