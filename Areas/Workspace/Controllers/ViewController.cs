using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using IDB.Architecture;
using IDB.MW.Business.WorkSpaceModule.Services;
using IDB.MW.Business.WorkSpaceModule.ViewModels;
using IDB.MW.Domain.Contracts.ModelRepositories.Security;
using IDB.MW.Domain.Session;
using IDB.Presentation.MVC4.Areas.Workspace.Models;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.Workspace.Controllers
{
    public partial class ViewController : BaseController
    {
        private readonly IBoxesService _boxesService;

        public ViewController(IBoxesService boxesService)
        {
            _boxesService = boxesService;
        }

        public virtual ActionResult Index()
        {
            var securityModelRepository = Globals.Resolve<ISecurityModelRepository>();
            ViewBag.Secirity = securityModelRepository.GetFieldBehaviorByPermissions(
                IDBContext.Current.Operation,
                WorkSpaceConstants.PAGE_VIEW,
                IDBContext.Current.Permissions,
                0,
                0)
                .ToList();
            var userName = IDBContext.Current.UserName;
            var response = _boxesService.GetBoxes(userName);
            var boxes = response.BoxesModels;
            ViewBag.ErrorMessage = response.ErrorMessage;
            ViewBag.UserName = userName;
            ViewBag.NoLoader = true;
            if (response.Type != 2)
            {
                ViewBag.Id = 0;
            }

            ViewBag.Id = response.TemplateId;
            return View(boxes);
        }

        public virtual ActionResult GetWidget(int id)
        {
            var userName = IDBContext.Current.UserName;
            var response = _boxesService.GetBox(id);
            ViewBag.UserName = userName;
            ViewBag.NoLoader = true;
            return View(response.Box);
        }

        public virtual ActionResult Loader()
        {
            return View();
        }

        public virtual FileResult GetDownload()
        {
            var name = Request["name"];
            var header = Request["head"];
            var data = Request["data"];

            var dataExport = new ExcelWorkspace
            {
                RowList = new List<RowWorkspace>(),
                SheetTitle = WorkSpaceConstants.EXCEL_SHEET_NAME
            };

            if (header != null)
            {
                header = header.Replace(WorkSpaceConstants.HEADER_INITIAL, string.Empty);
                header = header.Replace(WorkSpaceConstants.HEADER_FINALLY, string.Empty);
                header = header.Substring(0, header.LastIndexOf(WorkSpaceConstants.SEPARATOR));
                dataExport.RowList.Add(new RowWorkspace
                {
                    CellList = header.Split(WorkSpaceConstants.SEPARATOR).ToList()
                });
            }

            if (data != null)
            {
                var listData = data.Split(WorkSpaceConstants.NEWLINE).ToList();
                foreach (var line in listData.Where(line => !string.IsNullOrEmpty(line)))
                {
                    dataExport.RowList.Add(new RowWorkspace
                    {
                        CellList = line
                            .Substring(0, line.LastIndexOf(WorkSpaceConstants.SEPARATOR))
                            .Split(WorkSpaceConstants.SEPARATOR).ToList()
                    });
                }
            }

            var fileContents = _boxesService.CreateReport(dataExport);
            return new FileContentResult(fileContents.DataReport, "application/vnd.ms-excel") { FileDownloadName = string.Format("{0}.xlsx", name) };
        }
    }
}