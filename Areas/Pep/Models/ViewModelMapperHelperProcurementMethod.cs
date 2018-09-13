﻿using System.Collections.Generic;
using System.Linq;
using IDB.MW.Application.PEPModule.Services.Procurement;
using ViewModelProcurementMethod = IDB.MW.Application.PEPModule.ViewModels;

namespace IDB.Presentation.MVC4.Areas.Pep.Models
{
    public class ViewModelMapperHelperProcurementMethod
    {
        private readonly IPepCombosDependency _pepProcurementMethod;

        public ViewModelMapperHelperProcurementMethod(IPepCombosDependency pepProcurementMethod)
        {
            _pepProcurementMethod = pepProcurementMethod;
        }

        public List<ProcurementMethodViewModel> DataProcurementMethodResponse(List<ViewModelProcurementMethod.ProcurementMethodViewModel> executingList)
        {
            return executingList.Select(ae => new ProcurementMethodViewModel
            {
                ProcurementTypeId = ae.ProcurementTypeId,
                ProcurementMethodId = ae.ProcurementMethodId,
                MethodName = ae.MethodName
            }).ToList();
        }
    }
}