using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using IDB.Architecture;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.GlobalModule.ViewModels;
using IDB.MW.Domain.Contracts.ModelRepositories.Global;
using IDB.MW.Domain.Session;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Infrastructure.Data.Optima.Repositories.Models.Global;

namespace IDB.Presentation.MVC4.Areas.Global.Models
{
    public class ValidatorsViewModelMapper
    {
        public List<ValidatorViewModel> ConvertHappyRoadValidatorsToViewModel(List<DetailValidatorModel> detailValidatorModel)
        {
            var _workflowConvergentData = Globals.Resolve<IConvergenceMasterDataRepository>();
            var _workflowTaskTypeRepository = Globals.Resolve<IWorkflowTaskTypeRepository>();
            var result = detailValidatorModel.Select(x => new ValidatorViewModel
            {
                Role = x.UserProfileValidator,
                Status = "Pending",
                TaskName = IDBContext.Current.CurrentLanguage == "en" ? _workflowTaskTypeRepository.Find(x.WorkflowTaskTypeId).NameEn : _workflowTaskTypeRepository.Find(x.WorkflowTaskTypeId).NameEs,
                Mandatory = _workflowConvergentData.GetOne(p => p.ConvergenceMasterDataId == x.ValidatorTypeId).Code == "MANDATORY" ? true : false,
            }).ToList();

            return result;
        }
    }
}