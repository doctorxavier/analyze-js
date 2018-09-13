using System.Collections.Generic;
using System.Linq;
using IDB.MW.Application.CountryStrategyModule.ViewModels.UseCountry;
using IDB.MW.Application.CountryStrategyModule.ViewModels.MonitoringResultMatrix;
using IDB.MW.Application.CountryStrategyModule.Enums;

namespace IDB.Presentation.MVC4.Areas.CountryStrategy.Models
{
    public class OperationsTableViewModel
    {
        public CSMonitoringTypeTable TypeTable { get; set; }

        public bool IsOperationManual { get; set; }

        public ICollection<OperationViewModel> OperationsTableRowViewModel { get; set; }

        public OperationsTableViewModel SetOperationsTableRowViewModel(ICollection<OperationViewModel> operationsTableRowViewModel, CSMonitoringTypeTable typeTable)
        {
            OperationsTableRowViewModel = operationsTableRowViewModel;
            IsOperationManual = operationsTableRowViewModel.Where(x => x.IsManual == true).Any();
            TypeTable = typeTable;
            return this;
        }
    }
}