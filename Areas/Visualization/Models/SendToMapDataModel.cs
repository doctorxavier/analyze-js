using System;
using IDB.Presentation.MVC4.Areas.Visualization.Enums;

namespace IDB.Presentation.MVC4.Areas.Visualization.Models
{
    public class SendToMapDataModel
    {
        public int vovId { get; set; }

        public SendToMapDestinationEnum[] dest { get; set; }
    }
}