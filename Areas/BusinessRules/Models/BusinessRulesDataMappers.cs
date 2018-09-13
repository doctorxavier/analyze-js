using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDB.MW.Application.BusinessRulesModule.ViewModels;
using IDB.MW.Business.BusinessRulesModule.DTOs;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.BusinessRules.Models
{
    public static class BusinessRulesDataMappers
    {
        public static void UpdateBusinessRuleViewModel(this BRERuleDTO viewModel, ClientFieldData[] clientFieldData)
        {
            foreach (var fieldData in clientFieldData)
            {
                switch (fieldData.Name)
                {
                    case "Code": viewModel.Code = fieldData.Value;
                        break;
                    case "Name": viewModel.Name = fieldData.Value;
                        break;
                    case "Description": viewModel.Description = fieldData.Value;
                        break;
                    case "hiddenSourceCode": viewModel.SourceCode = fieldData.Value;
                        break;
                    case "StartDate": viewModel.StartDate = DateTime.Parse(fieldData.Value);
                        break;
                    case "EndDate": viewModel.EndDate = DateTime.Parse(fieldData.Value);
                        break;
                    case "Version": viewModel.Version = fieldData.Value;
                        break;
                    case "IsEnabled": viewModel.IsEnabled = fieldData.Value == "True";
                        break;
                }
            }
        }
    }
}