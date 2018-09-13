using System;
using System.IO;
using System.Collections.Generic;

using IDB.MW.Infrastructure.ApplicationBase.Messages;
using IDB.MW.Infrastructure.ReportManager.DataSource;

namespace IDB.Presentation.MVC4.Areas.Reports.Messages
{
    public class EmbeddedVisualizationReportResponse : ResponseBase
    {
        public Stream ReportDefinition { get; set; }

        public List<IDataSource> MonitoringReportList { get; set; }
    }
}