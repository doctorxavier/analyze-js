using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using IDB.Architecture;
using IDB.Architecture.Language;
using IDB.Presentation.MVC4.Areas.Workspace.Models;
using IDB.Presentation.MVC4.Controllers;
using IDB.MW.Business.WorkSpaceModule.Services;
using IDB.MW.Business.WorkSpaceModule.ViewModels;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Helpers;

namespace IDB.Presentation.MVC4.Areas.Workspace.Controllers
{
    public partial class AdministrationWsController : BaseController
    {
        private readonly IAdministrationService _workspaceService;
        private readonly ViewModelMapperHelper _viewModelMapperHelper;

        public AdministrationWsController(IAdministrationService workspaceService)
        {
            _workspaceService = workspaceService;
            _viewModelMapperHelper = new ViewModelMapperHelper(_workspaceService);
        }

        public virtual ActionResult Index()
        {
            var securityModelRepository = Globals.Resolve<ISecurityModelRepository>();
            ViewBag.Secirity = securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                WorkSpaceConstants.PAGE_CHART,
                IDBContext.Current.Permissions,
                0,
                0)
                .ToList();

            ViewBag.ListChartType = _viewModelMapperHelper.GetChartTypeList();
            return View(new List<ChartRowViewModel>());
        }

        public virtual ActionResult Chart(int chartId)
        {
            var securityModelRepository = Globals.Resolve<ISecurityModelRepository>();
            ViewBag.Secirity = securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                WorkSpaceConstants.PAGE_CHART,
                IDBContext.Current.Permissions,
                0,
                0)
                .ToList();

            var model = DataChart(chartId);
            if (model.Code != null && model.Code.Equals(WorkSpaceConstants.BLANKCHART))
            {
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public virtual ActionResult SearchChartList(string graphName, int? graphTypeId)
        {
            var list = _viewModelMapperHelper.GetListGraphByFilter(graphName, graphTypeId, null);
            return PartialView("Partials/DataTable", list);
        }

        public virtual JsonResult ChartEditSave()
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<AdministrationViewModel>(jsonDataRequest.SerializedData);
            var chartTypeNames = _viewModelMapperHelper.GetChartTypeList();
            viewModel.UpdateAdministrationViewModel(jsonDataRequest.ClientFieldData, chartTypeNames);

            var response = _workspaceService.SaveChartEdit(viewModel);
            ViewBag.ChartId = response.ChartId;

            return Json(response);
        }

        public virtual ActionResult ChartEditReload(int id)
        {
            var model = DataChart(id);
            return PartialView("Partials/ChartEdit", model);
        }

        public virtual JsonResult GetPartialList(int chartType)
        {
            var response = _viewModelMapperHelper.GetPartialList(chartType);
            return Json(new { IsValid = response.Any(), Data = response });
        }

        public virtual ActionResult GetRoles()
        {
            var securityModelRepository = Globals.Resolve<ISecurityModelRepository>();
            ViewBag.Secirity = securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                WorkSpaceConstants.PAGE_ROLES,
                IDBContext.Current.Permissions,
                0,
                0).ToList();
            ViewBag.ListChartType = _viewModelMapperHelper.GetChartTypeList();
            ViewBag.ListRoles = _viewModelMapperHelper.GetRoles();
            return View(new List<ChartRowViewModel>());
        }

        public virtual ActionResult SearchChartListByRol(string graphName, int? graphTypeId, int? role)
        {
            var model = _viewModelMapperHelper.GetListGraphByFilter(graphName, graphTypeId, role);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            ViewBag.RoleId = (role == null) ? string.Empty : role.ToString();
            return PartialView("Partials/DataTableRoles", model);
        }

        public virtual JsonResult RolesSave(int roleId)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<List<ChartRowViewModel>>(jsonDataRequest.SerializedData);
            viewModel.UpdateChartRowViewModel(jsonDataRequest.ClientFieldData);
            var response = _workspaceService.SaveRolesChart(viewModel, roleId);

            return Json(response);
        }

        public virtual JsonResult ChartDelete(int chartId)
        {
            var response = _workspaceService.DeleteChart(chartId);

            return Json(response);
        }

        public virtual ActionResult Personalize(string templateId, int? type, int? val)
        {
            var userName = IDBContext.Current.UserName;

            if (templateId == null || type == null)
            {
                return RedirectToAction("Template");
            }

            ViewBag.UserName = type == 2 ? userName : string.Empty;
            ViewBag.BLCid = _viewModelMapperHelper.GetBlankChartId();
            ViewBag.TemplateId = templateId;
            ViewBag.Type = type;
            ViewBag.Val = val;
            ViewBag.Personalize = true;

            if (val == 1)
            {
                ViewBag.ListRoles = _viewModelMapperHelper.GetRoles();
            }
            else
            {
                ViewBag.ChartList = _viewModelMapperHelper.GetChartItems(0, (int)type, userName);
            }

            var model = _workspaceService.GetLayout(templateId, (int)type);
            if (!model.IsValid)
            {
                ViewBag.ErrorMessage = model.ErrorMessage;
            }

            return View(model.BoxesModels);
        }

        public virtual ActionResult PersonalizeReload(string templateId, int? type, int? val)
        {
            var userName = IDBContext.Current.UserName;

            if (templateId == null || type == null)
            {
                return RedirectToAction("Template");
            }

            ViewBag.UserName = type == 2 ? userName : string.Empty;
            ViewBag.BLCid = _viewModelMapperHelper.GetBlankChartId();
            ViewBag.TemplateId = templateId;
            ViewBag.Type = type;
            ViewBag.Val = val;
            ViewBag.Personalize = true;

            if (val == 1)
            {
                ViewBag.ListRoles = _viewModelMapperHelper.GetRoles();
            }
            else
            {
                ViewBag.ChartList = _viewModelMapperHelper.GetChartItems(0, (int)type, userName);
            }

            var model = _workspaceService.GetLayout(templateId, (int)type);
            if (!model.IsValid)
            {
                ViewBag.ErrorMessage = model.ErrorMessage;
            }

            return PartialView("Partials/PersonalizePartial", model.BoxesModels);
        }

        public virtual JsonResult Save(string data)
        {
            var js = new JavaScriptSerializer();
            js.Deserialize<dynamic>(data);

            var viewModel = new PersonalizeViewModel();
            viewModel.UpdateTemplateChartViewModel(data);
            var response = _workspaceService.TemplateSave(viewModel);

            return Json(response);
        }

        public virtual JsonResult ChartInTemplate(int id)
        {
            var response = _workspaceService.CharExistInTemplates(id);

            return Json(response);
        }

        public virtual JsonResult DeleteChartInTemplate(int id)
        {
            var response = _workspaceService.DeleteChartInTemplate(id);

            return Json(response);
        }

        public virtual ActionResult GetChartItemViewModels(int role, int type, string name)
        {
            var response = _viewModelMapperHelper.GetChartItems(role, type, name);
            return PartialView("Partials/ChartList", response);
        }

        public virtual ActionResult GetRow()
        {
            return PartialView("Partials/rowPersonalize");
        }

        public virtual ActionResult Template()
        {
            var securityModelRepository = Globals.Resolve<ISecurityModelRepository>();
            ViewBag.Secirity = securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                WorkSpaceConstants.PAGE_ADM_TEMPLATE,
                IDBContext.Current.Permissions,
                0,
                0)
                .ToList();
            ViewBag.ListTemplate = _viewModelMapperHelper.GetTemplateList();
            ViewBag.ListRoles = _viewModelMapperHelper.GetRoles();
            return View(new List<TemplateRowViewModel>());
        }

        public virtual ActionResult SearchTemplates(string templateName, string type)
        {
            var model = _viewModelMapperHelper.GetTemplateRowViewModels(templateName, type);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            ViewBag.Type = type;
            return PartialView("Partials/DataTableTemplate", model);
        }

        public virtual ActionResult TemplateEdit(int templateId)
        {
            var securityModelRepository = Globals.Resolve<ISecurityModelRepository>();
            ViewBag.Secirity = securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                WorkSpaceConstants.PAGE_CHART,
                IDBContext.Current.Permissions,
                0,
                0).ToList();
            var model = DataTemplate(templateId);
            return View(model);
        }

        public virtual JsonResult TemplateEditSave(int templateId)
        {
            var jsonDataRequest = PageSerializationHelper.DeserializeJsonForm(Request.Form[0]);
            var viewModel = PageSerializationHelper.DeserializeObject<TemplateViewModel>(jsonDataRequest.SerializedData);
            viewModel.UpdateTemplateViewModel(jsonDataRequest.ClientFieldData);
            var response = _workspaceService.SaveTemplateEdit(viewModel);

            return Json(response);
        }

        public virtual ActionResult TemplateEditReload(int id)
        {
            var model = DataTemplate(id);
            return PartialView("Partials/TemplateForm", model);
        }

        public virtual JsonResult TemplateDelete(int templateId, string type)
        {
            var response = _workspaceService.DeleteTemplate(templateId, type);
            return Json(response);
        }

        private AdministrationViewModel DataChart(int chartId)
        {
            var model = chartId != 0 ? _viewModelMapperHelper.GetChartAttributes(chartId) : new AdministrationViewModel();
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model);
            ViewBag.ListPartialName = _viewModelMapperHelper.GetPartialList(chartId == 0 ? -1 : model.ChartTypeId);
            var listType = _viewModelMapperHelper.GetChartTypeList();
            ViewBag.ListChartType = listType;
            ViewBag.ChartId = chartId;
            ViewBag.MasterDataId = chartId == 0 ? 0 : model.MasterDataId;
            var type = listType.FirstOrDefault(o => o.Text.Equals(Localization.GetText("Workspace.ChartEdit.StaticChart")));
            if (type != null) ViewBag.StaticId = type.Value;
            return model;
        }

        private TemplateViewModel DataTemplate(int templateId)
        {
            var model = _viewModelMapperHelper.GetTemplateData(templateId);
            ViewBag.SerializedViewModel = PageSerializationHelper.SerializeObject(model.templateViewModel);
            ViewBag.TemplateId = templateId;

            return model.templateViewModel;
        }
    }
}