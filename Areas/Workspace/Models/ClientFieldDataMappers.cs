using System.Web.Script.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.MW.Business.WorkSpaceModule.ViewModels;
using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.Architecture.Language;

namespace IDB.Presentation.MVC4.Areas.Workspace.Models
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateAdministrationViewModel(this AdministrationViewModel viewModel, ClientFieldData[] clientField, List<SelectListItem> typeNames)
        {
            var id = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("ChartId"));

            if (id != null)
            {
                viewModel.ChartId = Convert.ToInt32(id.Value);

                if (id.Value == "0")
                {
                    var code =
                        clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Code"));
                    if (code != null)
                    {
                        viewModel.Code = code.Value;
                    }
                }
            }

            var nameEn = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("ChartNameEN"));
            if (nameEn != null)
            {
                viewModel.ChartNameEn = nameEn.Value;
            }

            var nameEs =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("ChartNameES"));
            if (nameEs != null)
            {
                viewModel.ChartNameEs = nameEs.Value;
            }

            var namePt =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("ChartNamePT"));
            if (namePt != null)
            {
                viewModel.ChartNamePt = !string.IsNullOrEmpty(namePt.Value) ? namePt.Value : viewModel.ChartNameEn;
            }
            else
            {
                viewModel.ChartNamePt = viewModel.ChartNameEn;
            }

            var nameFr =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("ChartNameFR"));
            if (nameFr != null)
            {
                viewModel.ChartNameFr = !string.IsNullOrEmpty(nameFr.Value) ? nameFr.Value : viewModel.ChartNameEn;
            }
            else
            {
                viewModel.ChartNameFr = viewModel.ChartNameEn;
            }

            var partialId =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("PartialName"));
            if (partialId != null)
            {
                viewModel.PartialId = partialId.Value;
            }

            var height =
                  clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Height"));
            if (height != null)
            {
                viewModel.Height = Convert.ToInt32(height.Value);
            }

            var width = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Width"));
            if (width != null)
            {
                viewModel.Width = Convert.ToInt32(width.Value);
            }

            var charMasterDataId = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("MasterDataId"));
            if (charMasterDataId != null)
            {
                viewModel.MasterDataId = Convert.ToInt32(charMasterDataId.Value);
            }

            var efectiveDate =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("EffectiveDate"));
            if (efectiveDate != null)
            {
                viewModel.EffectiveDate = Convert.ToDateTime(efectiveDate.Value);
            }

            var expirationDate =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("ExpirationDate"));
            if (expirationDate != null)
            {
                if (!string.IsNullOrEmpty(expirationDate.Value))
                {
                    viewModel.ExpirationDate = Convert.ToDateTime(expirationDate.Value);
                }
            }

            var chartTypeId = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TypeChart"));
            if (chartTypeId != null)
            {
                viewModel.ChartTypeId = Convert.ToInt32(chartTypeId.Value);

                var typeName = typeNames.FirstOrDefault(t => t.Value == chartTypeId.Value);

                if (typeName != null)
                {
                    viewModel.ChartTypeName = typeName.Text;
                    if (typeName.Text == Localization.GetText("Workspace.ChartEdit.GraphChart"))
                    {
                        var query =
                       clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Query"));
                        if (query != null)
                        {
                            viewModel.Query = query.Value;
                        }

                        var queryExcel =
                        clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("QueryExcel"));
                        if (queryExcel != null)
                        {
                            viewModel.QueryExportExcel = queryExcel.Value;
                        }

                        var maxDeep = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("MaxDeep"));
                        if (maxDeep != null)
                        {
                            viewModel.MaxDeep = Convert.ToInt32(maxDeep.Value);
                        }
                    }

                    if (typeName.Text == Localization.GetText("Workspace.ChartEdit.FrameChart"))
                    {
                        var urlFrame =
                      clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Url"));
                        if (urlFrame != null)
                        {
                            viewModel.UrlFrame = urlFrame.Value;
                        }
                    }
                }
            }
        }

        public static void UpdateChartRowViewModel(this List<ChartRowViewModel> viewModel,
            ClientFieldData[] clientField)
        {
            var i = 0;
            var isSelectedList = clientField.Where(o => o.Name.Equals("CheckedRow")).ToList();

            foreach (var item in viewModel)
            {
                item.IsChecked = Convert.ToBoolean(isSelectedList[i].Value);
                i++;
            }
        }

        public static void UpdateTemplateViewModel(this TemplateViewModel viewModel,
            ClientFieldData[] clientField)
        {
            var id = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TemplateId"));

            if (id != null)
            {
                viewModel.TemplateId = Convert.ToInt32(id.Value);
            }

            var name = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TemplateName"));
            if (name != null)
            {
                viewModel.TemplateName = name.Value;
            }

            var i = 0;
            var isSelectedList = clientField.Where(o => o.Name.Equals("CheckedRow")).ToList();

            foreach (var item in viewModel.TemplateProfileList)
            {
                item.RoleIsChecked = Convert.ToBoolean(isSelectedList[i].Value);
                i++;
            }
        }

        public static PersonalizeViewModel UpdateTemplateChartViewModel(this PersonalizeViewModel viewModel,
            string clientField)
        {
            JavaScriptSerializer js = new JavaScriptSerializer();
            dynamic layoutData = js.Deserialize<dynamic>(clientField);
            var list = new List<TemplateChartViewModel>();
            foreach (var item in layoutData)
            {
                list.Add(
                    new TemplateChartViewModel
                    {
                        ChartId = Convert.ToInt32(item["ChartId"]),
                        TemplateRow = Convert.ToInt32(item["TemplateRow"]),
                        TemplateOrder = Convert.ToInt32(item["TemplateOrder"])
                    });
            }

            viewModel.TemplateId = layoutData[0]["TemplateId"].ToString();
            viewModel.Type = Convert.ToInt32(layoutData[0]["Type"]);
            viewModel.TemplateChartList = list;
            return viewModel;
        }
    }
}