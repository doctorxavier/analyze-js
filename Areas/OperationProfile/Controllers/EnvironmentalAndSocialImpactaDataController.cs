using System.Web.Mvc;

using IDB.MW.Domain.Models.OperationProfile.EnvirontmentAndSocialData;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.EnvirontmentAndSocialData;
using IDB.MW.Domain.Contracts.ModelRepositories.OperationProfile.BasicData;
using IDB.Domain.Attributes;
using IDB.Presentation.MVC4.Controllers;

namespace IDB.Presentation.MVC4.Areas.OperationProfile.Controllers
{
   public partial class EnvironmentalAndSocialImpactaDataController : BaseController
   {
      // GET: /OperationProfile/EnvironmentalAndSocialImpactaData/
      private IEnvironmentalAndSocialImpactDataModelRepository _client = null;
      public IEnvironmentalAndSocialImpactDataModelRepository client
      {
         get { return _client; }
         set { _client = value; }
      }

      private IBasicDataModelRepository _clientBasicData = null;

      public virtual IBasicDataModelRepository clientBasicData
      {
         get { return _clientBasicData; }
         set { _clientBasicData = value; }
      }

      [ExceptionHandling]
      public virtual ActionResult Index(string OperationNumber)
      {
         var environmentalModel = client.FindOneEnvironmentalAndSocialImpactaDataModel(new EnvironmentalAndSocialImpactDataModel() { OperationNumber = OperationNumber });

         return View(environmentalModel);
      }
   }
}
