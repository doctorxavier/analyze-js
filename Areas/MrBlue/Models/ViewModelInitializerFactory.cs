using IDB.MW.Application.MrBlueModule.ViewModels.GeneralInformation;
using IDB.MW.Application.MrBlueModule.ViewModels.HighRisk;
using IDB.MW.Application.MrBlueModule.ViewModels.Indicators;
using IDB.MW.Application.MrBlueModule.ViewModels.Reports;
using IDB.MW.Application.MrBlueModule.ViewModels.SafeguardToolkit;
using IDB.MW.Application.MrBlueModule.ViewModels.Star;
using IDB.MW.Application.MrBlueModule.ViewModels.SupervisionReport;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Models
{
    internal static class ViewModelInitializerFactory
    {
        #region SupervisionReport
        internal static SupervisionReportDashboardViewModel InitializeSupervisionReportDashboardViewModel()
        {
            return new SupervisionReportDashboardViewModel();
        }

        internal static SupervisionReportStep1ViewModel InitializeSupervisionReportPage1ViewModel()
        {
            return new SupervisionReportStep1ViewModel();
        }

        internal static SupervisionReportStep2ViewModel InitializeSupervisionReportPage2ViewModel()
        {
            return new SupervisionReportStep2ViewModel();
        }
        #endregion

        #region GeneralInformation
        internal static GeneralInformationViewModel InitializeGeneralInformationViewModel()
        {
            return new GeneralInformationViewModel();
        }
        #endregion

        #region HighRisk
        internal static HighRiskDashboardViewModel InitializeHighRiskDashboardViewModel()
        {
            return new HighRiskDashboardViewModel();
        }

        internal static HighRiskViewModel InitializeHighRiskViewModel()
        {
            return new HighRiskViewModel();
        }
        #endregion

        #region Indicators
        internal static IndicatorsDashboardViewModel InitializeIndicatorsDashboardViewModel()
        {
            return new IndicatorsDashboardViewModel();
        }

        internal static IndicatorsViewModel InitializeIndicatorsViewModel()
        {
            return new IndicatorsViewModel();
        }
        #endregion

        #region Report
        internal static ReportsCustomReportViewModel InitializeCustomReportViewModel()
        {
            return new ReportsCustomReportViewModel();
        }
        #endregion

        #region SafeguardToolkit
        internal static SafeguardToolkitDashboardViewModel InitializeSafeguardToolkitDashboardViewModel()
        {
            return new SafeguardToolkitDashboardViewModel();
        }

        internal static SafeguardToolkitStep2ViewModel InitializeSafeguardToolkitStep2ViewModel()
        {
            return new SafeguardToolkitStep2ViewModel();
        }

        internal static SafeguardToolkitStep3ViewModel InitializeSafeguardToolkitStep3ViewModel()
        {
            return new SafeguardToolkitStep3ViewModel();
        }

        internal static SafeguardToolkitStep4ViewModel InitializeSafeguardToolkitStep4ViewModel()
        {
            return new SafeguardToolkitStep4ViewModel();
        }

        internal static SafeguardToolkitStep5ViewModel InitializeSafeguardToolkitStep5ViewModel()
        {
            return new SafeguardToolkitStep5ViewModel();
        }

        internal static SafeguardToolkitStep6ViewModel InitializeSafeguardToolkitStep6ViewModel()
        {
            return new SafeguardToolkitStep6ViewModel();
        }
        #endregion

        #region Star
        internal static StarDashboardViewModel InitializeStarDashboardViewModel()
        {
            return new StarDashboardViewModel();
        }

        internal static StarViewModel InitializeStarViewModel()
        {
            return new StarViewModel();
        }
        #endregion
    }
}