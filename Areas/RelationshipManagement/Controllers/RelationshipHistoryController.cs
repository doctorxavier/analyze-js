using System.Web.Mvc;

using IDB.MW.Application.RelationshipManagement.Services.History;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.RelationshipManagement.Controllers
{
    public partial class RelationshipHistoryController : BaseController
    {
        private readonly IRelationshipHistoryService _relationshipHistoryService;
        public RelationshipHistoryController(
            IRelationshipHistoryService relationshipHistoryService)
        {
            _relationshipHistoryService = relationshipHistoryService;
        }

        public virtual ActionResult GetPartialChangeHistory(
            string operationNumber, string partial)
        {
            var relatedOperationsHistory =
                _relationshipHistoryService.GetRelatedOperationsHistory(operationNumber);

            return PartialView(partial, relatedOperationsHistory.Model);
        }
    }
}