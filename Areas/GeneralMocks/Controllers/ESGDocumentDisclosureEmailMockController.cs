using System;
using System.Collections.Generic;
using System.Web.Mvc;

using IDB.MW.Application.Core.Services.EnumMapping;
using IDB.MW.Application.MrBlueModule.Email.DocumentDisclosure;
using IDB.MW.Application.MrBlueModule.Email.Enumerators;
using IDB.MW.Application.MrBlueModule.Services.DisclosureESGDocuments;
using IDB.MW.DomainModel.Contracts.Repositories.Core;
using IDB.MW.Domain.Entities;
using IDB.MW.Infrastructure.SecurityService.Contracts;
using IDB.Presentation.MVC4.Areas.MrBlue.Controllers;

namespace IDB.Presentation.MVC4.Areas.Mock.Controllers
{
    public partial class ESGDocumentDisclosureEmailMockController : BaseController
    {
        private readonly IEnumMappingService _enumMappingService;
        private readonly IAuthorizationService _authorizationService;
        private readonly IDisclosureESGDocumentsService _disclosureESGDocumentsService;
        private readonly IDocumentDisclosureEmailManager _documentDisclosureEmailManager;
        private readonly IGenericRepository<DocumentESGDisclosure> _documentESGDisclosureRepository;

        public ESGDocumentDisclosureEmailMockController(
            IAuthorizationService authorizationService,
            IEnumMappingService enumMappingService,
            IDisclosureESGDocumentsService disclosureESGDocumentsService,
            IDocumentDisclosureEmailManager documentDisclosureEmailManager,
            IGenericRepository<DocumentESGDisclosure> documentESGDisclosureRepository) :
                base(authorizationService, enumMappingService)
        {
            _enumMappingService = enumMappingService;
            _authorizationService = authorizationService;
            _disclosureESGDocumentsService = disclosureESGDocumentsService;
            _documentDisclosureEmailManager = documentDisclosureEmailManager;
            _documentESGDisclosureRepository = documentESGDisclosureRepository;
        }

        public virtual ActionResult Index()
        {
            return View();
        }

        public virtual ActionResult SendEmail(string operationNumber, ESGDDEmailSendType emailType)
        {
            var documentIds = new List<int>();

            try
            {
                var response = _documentDisclosureEmailManager.SendESGDDEmail(emailType);

                if (response.IsValid)
                {
                    return RedirectToAction("Index", "ESGDocumentDisclosureEmailMock");
                }

                throw new ApplicationException(response.ErrorMessage);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}