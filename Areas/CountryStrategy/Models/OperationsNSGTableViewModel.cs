using System.Collections.Generic;
using IDB.MW.Application.CountryStrategyModule.ViewModels.MonitoringResultMatrix;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Models
{
    public class OperationsNsgTableViewModel
    {
        public bool Editmode { get; set; }

        public ICollection<OperationNSGViewModel> OperationsNSGTableRowViewModel { get; set; }

        public OperationsNsgTableViewModel SetOperationsNSGTableRowViewModel(ICollection<OperationNSGViewModel> operationsNSGTableRowViewModel)
        {
            OperationsNSGTableRowViewModel = operationsNSGTableRowViewModel;
            return this;
        }
    }
}