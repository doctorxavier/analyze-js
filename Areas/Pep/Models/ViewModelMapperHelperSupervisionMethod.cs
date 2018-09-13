﻿using System.Collections.Generic;
using System.Linq;
using IDB.MW.Application.PEPModule.Services.Procurement;
using ViewModelProcurementMethod = IDB.MW.Application.PEPModule.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ViewModelMapperHelperSupervisionMethod
    {
        private readonly IPepCombosDependency _pepSupervisionMethod;

        public ViewModelMapperHelperSupervisionMethod(IPepCombosDependency pepSupervisionMethod)
        {
            _pepSupervisionMethod = pepSupervisionMethod;
        }

        public List<SupervisionMethodViewModel> ComboSupervisionMethodResponse(List<ViewModelProcurementMethod.SupervisionMethodViewModel> executingList)
        {
            return executingList.Select(ae => new SupervisionMethodViewModel
            {
                SupervisionMethodId = ae.SupervisionMethodId,
                SupervisionMethodName = ae.SupervisionMethodName
            }).ToList();
        }
    }
}