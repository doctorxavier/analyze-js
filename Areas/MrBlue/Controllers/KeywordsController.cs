using System.Web.Mvc;

using IDB.MW.Application.MrBlueModule.Services.Keywords;
using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Infrastructure.SecurityService.Contracts;

namespace IDB.Presentation.MVC4.Areas.MrBlue.Controllers
{
    public partial class KeywordsController : BaseController
    {
        #region Constants
        private const string NO_WRITE_PERMISSION = "TC.TCAbstractService.NoWritePermission";
        #endregion

        #region Fields
        private readonly IKeywordService _keywordService;
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        #endregion

        #region Contructors
        public KeywordsController(IKeywordService keywordService,
            IEnumMappingService enumMappingService,
            IAuthorizationService authorizationService)
            : base(authorizationService, enumMappingService)
        {
            _keywordService = keywordService;
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
        }

        #endregion

        #region KeyWords Tooltip

        public virtual JsonResult GetKeywordByTerm(string term)
        {
            var response = _keywordService.GetKeywordByTerm(term);
            return Json(new { IsValid = response.IsValid, ErrorMessage = response.ErrorMessage, Data = response });
        }

        #endregion
    }
}
