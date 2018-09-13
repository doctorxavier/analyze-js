using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDB.Presentation.MVC4.Areas.TC.Enums
{
    public enum CommentTypeEnum : byte
    {
        TCAbstract = 1,
        TCAbstractInternal,
        TCAbstractTeamLeader,
        TCSingleWindowOperationsDecision,
        TCSingleWindowMeeting,
        TCFundCordinatorReview,
        TCRegionalTeamLeaderReview
    }
}
