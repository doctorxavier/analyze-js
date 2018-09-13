using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Business.WorkSpaceModule.Services;
using IDB.MW.Business.WorkSpaceModule.Messages;
using IDB.MW.Business.WorkSpaceModule.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Workspace.Models
{
    public class ViewModelMapperHelper
    {
        #region fields

        private readonly IAdministrationService _administrationService;

        #endregion

        public ViewModelMapperHelper(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        public List<ChartRowViewModel> GetListGraphByFilter(string graphName, int? typeId, int? role)
        {
            return _administrationService.GetChartRowViewModel(graphName, typeId, role);
        }

        public List<SelectListItem> GetChartTypeList()
        {
            return _administrationService.GetChartTypeList();
        }

        public AdministrationViewModel GetChartAttributes(int chartId)
        {
            return _administrationService.GetChartAttributes(chartId).AdministrationViewModel;
        }

        public List<SelectListItem> GetPartialList(int typeId)
        {
            return _administrationService.GetPartialNameList(typeId);
        }

        public List<SelectListItem> GetRoles()
        {
            return _administrationService.GetRolesList();
        }

        public int GetBlankChartId()
        {
            return _administrationService.GetBlankChartId();
        }

        public List<ChartItemViewModel> GetChartItems(int roles, int type, string name)
        {
            return _administrationService.GetChartItemViewModels(roles, type, name);
        }

        public List<SelectListItem> GetTemplateList()
        {
            return _administrationService.GetTemplateList();
        }

        public List<TemplateRowViewModel> GetTemplateRowViewModels(string templateName, string type)
        {
            return _administrationService.GetTemplateRowViewModels(templateName, type); 
        }

        public TemplateEditResponse GetTemplateData(int templateId)
        {
            return _administrationService.GetTemplateData(templateId);
        }
}
}