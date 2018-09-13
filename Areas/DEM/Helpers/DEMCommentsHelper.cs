using IDB.MW.Domain.Values.Dem;

namespace IDB.Presentation.MVC4.Areas.DEM.Helpers
{
    public static class DEMCommentsHelper
    {
        #region Comments Color Class 
        public const string NewCommentBlue = "textAreaLabelAndInput myCommentBlue";
        public const string LastCommentFromOlderVersion = "textAreaLabelAndInput borderLightBlue";
        public const string OldComment = "textAreaLabelAndInput oldCommentGray";
        #endregion

        public static string CommentBoxClassAccordingDemCurrentVersion(string demCommentCurrentVersion, bool isNewCommentLastVersion)
        {
            string commentClass = string.Empty;

            if (demCommentCurrentVersion == DemGlobalValues.DRAFT)
            {
                commentClass = NewCommentBlue;
                return commentClass;
            }

            if (demCommentCurrentVersion != DemGlobalValues.DRAFT && isNewCommentLastVersion)
            {
                commentClass = LastCommentFromOlderVersion;
                return commentClass;
            }

            if (demCommentCurrentVersion != DemGlobalValues.DRAFT && !isNewCommentLastVersion)
            {
                commentClass = OldComment;
                return commentClass;
            }

            return commentClass;
        }

        public static bool IsCommentEditable(string demCurrentVersion)
        {
            return demCurrentVersion == DemGlobalValues.DRAFT;
        }
    }
}