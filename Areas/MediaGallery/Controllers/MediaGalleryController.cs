using System.IO;
using System.Web;
using System.Web.Mvc;

using IDB.Architecture.Language;
using IDB.MW.Application.MediaGalleryModule.Messages.MediaGallery;
using IDB.MW.Application.MediaGalleryModule.Services.MediaGallery;
using IDB.MW.Application.MediaGalleryModule.ViewModels.MediaGallery;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.MediaGallery.Controllers
{
    public partial class MediaGalleryController : MVC4.Controllers.ConfluenceController
    {
        public readonly IMediaGalleryManageService _mediaGalleryManageService;
        private const string NO_WRITE_PERMISSION = "MediaGallery.NoWritePermission";
        private const string URL_GENERAL_INFORMATION = "/MrBlue/GeneralInformation";

        public MediaGalleryController(IMediaGalleryManageService mediaGalleryManageService)
        {
            _mediaGalleryManageService = mediaGalleryManageService;
        }

        public virtual ActionResult MediaGallery(string operationNumber)
        {
            var model = new MediaFilesFilterViewModel();
            SetViewBagRoles(operationNumber);
            return View(model);
        }

        public virtual ActionResult LoadFilterMediaFiles(
            string operationNumber,
            MediaFilesFilterViewModel mediaFileFilterViewModel,
            int elementsForPage)
        {
            var response = _mediaGalleryManageService
                .GetMediaFilesFilter(operationNumber, mediaFileFilterViewModel, elementsForPage, 0);

            SetViewBagRoles(operationNumber);

            return PartialView("Partials/ResultFilterMediaFilesPartial", response.MediaFileList);
        }

        public virtual ActionResult LoadFilterMediaFilesNavigation(
            string operationNumber,
            MediaFilesFilterViewModel mediaFileFilterViewModel,
            int numberPage,
            int elementsForPage)
        {
            var response = _mediaGalleryManageService.GetMediaFilesFilter(
                operationNumber,
                mediaFileFilterViewModel,
                elementsForPage,
                numberPage);

            SetViewBagRoles(operationNumber);

            return PartialView("Partials/ResultNavigationFilterMediaFPartial", response.MediaFileList);
        }

        public virtual ActionResult AttachAddMediaFile(
            string operationNumber,
            MediaFileViewModel manageMediaViewModel,
            HttpPostedFileBase fileMedia)
        {
            var response = new SaveMediaFileResponse();

            SetViewBagRoles(operationNumber);

            if (!ViewBag.WriteRole)
                return Json(new { ErrorMessage = Localization.GetText(NO_WRITE_PERMISSION) });

            response = _mediaGalleryManageService
                .SaveMediaFile(operationNumber, manageMediaViewModel, fileMedia);

            return Json(response);
        }

        public virtual ActionResult GetMetadataMediaFile(HttpPostedFileBase fileMedia)
        {
            var response = new GetMetadataImageResponse();

            if (fileMedia != null)
            {
                var buffer = new MemoryStream();

                fileMedia.InputStream.CopyTo(buffer);
                response = _mediaGalleryManageService.GetMetadataImage(buffer);
            }

            return Json(response);
        }

        protected void SetViewBagRoles(string operationNumber)
        {
            if (ViewBag.IsValid ?? true)
            {
                ViewBag.ReadRole = true;
                ViewBag.WriteRole =
                    IDBContext.Current.HasPermission(Permission.MEDIA_GALLERY_WRITE_PERMISSIONS);
            }
            else
            {
                ViewBag.ReadRole = false;
                ViewBag.WriteRole = false;
            }
        }
    }
}