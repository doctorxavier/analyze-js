using System.Collections.Generic;

using IDB.MW.Application.CountryStrategyModule.ViewModels.UseCountry;
using IDB.MW.Application.OPUSModule.ViewModels.Common;

namespace IDB.Presentation.MVC4.Areas.CPD.Models
{
    public class CPDTableRelationHeaderModel
    {
        public RelationsViewModel ModelRelation { get; set; }
        public RelatedOperationRowViewModel ModelRelatedOperation { get; set; }

        public CPDTableRelationHeaderModel SetCSStytemRecordRowViewModels(RelationsViewModel modelRelation, RelatedOperationRowViewModel modelRelatedOperation)
        {
            ModelRelation = modelRelation;
            ModelRelatedOperation = modelRelatedOperation;
            return this;
        }
    }
}