using IDB.MW.Infrastructure.ApplicationBase.Messages;

namespace IDB.Presentation.MVC4.Areas.Reports.Messages
{
    public class DownloadVisualizationReportResponse : ResponseBase
    {
        public byte[] File { get; set; }
    }
}