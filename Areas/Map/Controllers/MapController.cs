using System.Web.Http;

using IDB.Architecture;
using IDB.MW.Application.MapModule.Services;
using IDB.MW.Application.MapModule.ViewModels;
using IDB.MW.Application.MapModule.Messages;
using IDB.MW.Domain.Session;

namespace IDB.Presentation.MVC4.Areas.Map.Controllers
{
    public class MapController : ApiController
    {
        private IMapService _mapService = Globals.Resolve<IMapService>();

        public static bool PermissionESGSpecial()
        {
            string userLoginName = IDBContext.Current.UserName;
            IMapService _mapService = Globals.Resolve<IMapService>();
            var result = _mapService.PermissionESGTeamMember(userLoginName);
            return result.IsESGRole;
        }

        public IHttpActionResult GetBasinsMap()
        {
            MapResponse response;

            if (IDBContext.Current.HasPermission(Permission.MAP_ADMIN))
            {
                response = _mapService.GetMaps();
            }
            else
            {
                response = _mapService.GetMaps(IDBContext.Current.UserName);
            }

            return Ok(response);
        }

        public IHttpActionResult GetCountry()
        {
            var result = _mapService.GetCountries();

            return Ok(result);
        }

        public IHttpActionResult GetBasinLayersMap()
        {
            var result = _mapService.GetBasinLayers();

            return Ok(result);
        }

        public IHttpActionResult GetBasinGeoMap(int mapId)
        {
            var result = _mapService.GetShapeMap(mapId);

            return Ok(result);
        }

        public IHttpActionResult PostBasinMap(MapViewModel model)
        {
            var result = _mapService.CreateMap(model);

            return Ok(result);
        }

        [HttpPost]
        public IHttpActionResult PutBasinMap(MapViewModel model)
        {
            var result = _mapService.UpdateMap(model);

            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult DeleteBasinMap(int mapId)
        {
            var result = _mapService.DeleteMap(mapId);

            return Ok(result);
        }

        [HttpGet]
        public IHttpActionResult ShapeDelete(string shapeRemovalIdUuid, string shapeType)
        {
            var result = _mapService.ShapeDeleteById(shapeRemovalIdUuid, shapeType);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        public IHttpActionResult GetOperationDataReport(string operationNumber)
        {
            var result = _mapService.GetOperationDataReport(operationNumber);

            return Ok(result);
        }
    }
}