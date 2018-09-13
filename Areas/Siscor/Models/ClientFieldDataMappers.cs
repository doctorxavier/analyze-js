using System;
using System.Linq;

using IDB.MW.Application.SiscorModule.Enums;
using IDB.MW.Application.SiscorModule.ViewModels;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.Siscor.Models
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateOutputViewModel(this SiscorResponseViewModel viewModel, ClientFieldData[] clientFieldData, string literal)
        {
            if (viewModel == null)
            {
                viewModel = new SiscorResponseViewModel();
            }

            var OperationNumber = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("OperationNumber"));
            if (OperationNumber != null)
            {
                viewModel.Information.Operation = OperationNumber.Value;
            }

            var Contract = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Contract"));
            if (Contract != null)
            {
                viewModel.Information.Contract = Contract.Value;
            }

            var businessArea = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("BusinessArea"));
            if (businessArea != null)
            {
                viewModel.Information.BusinessAreaCode = businessArea.Value;
            }

            viewModel.Information.SendDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            var subject = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Subject"));
            if (subject != null)
            {
                viewModel.Information.Subject = subject.Value;
            }

            var country = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Country"));
            if (country != null)
            {
                viewModel.Information.CountryCode = country.Value;
            }

            var template = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Template"));
            if (template != null)
            {
                viewModel.Information.TemplateCode = template.Value;
            }

            var organization = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("Organization"));
            if (organization != null)
            {
                viewModel.Information.Organization = organization.Value;
            }

            var docStatus = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("DocumentStatus"));
            if (docStatus != null)
            {
                viewModel.Information.DocumentStatus = docStatus.Value;
            }

            var signBy = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("SignBy"));
            if (signBy != null)
            {
                viewModel.Information.SignById = signBy.Value;
            }

            foreach (var i in viewModel.GridCorrespondences.Correspondences)
            {
                i.StatusCodeType = SiscorEnums.STATUS_ON_REVISION;
                i.Status = literal;
            }
         }
    }
}