using System.Collections.Generic;
using System.Linq;

using IDB.MW.Domain.Models.Architecture.MasterDefinitions;
using IDB.Architecture.Logging;

namespace IDB.Presentation.MVC4.Areas.Reports.BusinessLogic
{
    internal class VisualizationBusinessLogic
    {
        internal List<ConvergenceMasterDataModel> FilterCountryDepartments(
            IEnumerable<ConvergenceMasterDataModel> model)
        {
            return model.Where(
                x => !x.Code.Equals("IDB") &&
                    !x.Code.Equals("IDB/IDB"))
                .OrderBy(x => x.Name).ToList();
        }

        internal List<ConvergenceMasterDataModel> FilterCountries(
            IEnumerable<ConvergenceMasterDataModel> model)
        {
            return model.Where(
                x => !x.Code.Equals("UND"))
                .OrderBy(x => x.Name).ToList();
        }

        internal List<ConvergenceMasterDataModel> FilterSectorDepartment(
            IEnumerable<ConvergenceMasterDataModel> model)
        {
            return model.Where(
                x => x.Code.Equals("IFD") ||
                    x.Code.Equals("INE") ||
                    x.Code.Equals("INT") ||
                    x.Code.Equals("SCL") ||
                    x.Code.Equals("CSD"))
                .OrderBy(x => x.Name).ToList();
        }

        internal List<ConvergenceMasterDataModel> FilterDivisions(
            IEnumerable<ConvergenceMasterDataModel> model)
        {
            bool bIsIFDFMMIncluded = model.Any(x => x.Code.Equals("IFD/FMM"));
            Logger.GetLogger().WriteDebug(
                "Visualization", "Code Loaded IFDFMM: " + bIsIFDFMMIncluded);

            bool bIsCSDCCSIncluded = model.Any(x => x.Code.Equals("CSD/CCS"));
            Logger.GetLogger().WriteDebug(
                "Visualization", "Code Loaded CSDCCS: " + bIsCSDCCSIncluded);

            //ToDo: Remove INT/TIU, INE/CCS, INE/RND
            //after update EDW system with the new divisions codes.
            return model.Where(
                x => !x.Code.Equals("INE/100") &&
                    !x.Code.Equals("IFD/IFD") &&
                    !x.Code.Equals("INE/INE") &&
                    !x.Code.Equals("INT/INT") &&
                    !x.Code.Equals("INT/001") &&
                    !x.Code.Equals("INT/002") &&
                    !x.Code.Equals("INT/003") &&
                    !x.Code.Equals("INT/007") &&
                    !x.Code.Equals("INT/008") &&
                    !x.Code.Equals("SCL/010") &&
                    !x.Code.Equals("INE/103") &&
                    !x.Code.Equals("INE/300") &&
                    !x.Code.Equals("INE/IIR") &&
                    !x.Code.Equals("No Code") &&
                    !x.Code.Equals("SCL/100") &&
                    !x.Code.Equals("SCL/SCT") &&
                    !x.Code.Equals("FMM/EUR") &&
                    !x.Code.Equals("SCL/SCL") &&
                    !x.Code.Equals("CSD/CSD") &&
                    !x.Code.Equals("INT/TIU") &&
                    !x.Code.Equals("INE/CCS") &&
                    !x.Code.Equals("INE/RND"))
                .OrderBy(x => x.Code).ToList();
        }

        internal List<ConvergenceMasterDataModel> FilterPublicationsStatus(
            IEnumerable<ConvergenceMasterDataModel> model)
        {
            return model.Where(
                x => x.Code.Equals("VIS_CBO_NP") ||
                    x.Code.Equals("VIS_CBO_IM") ||
                    x.Code.Equals("VIS_CBO_EM"))
                .OrderBy(x => x.Name).ToList();
        }

        internal List<ConvergenceMasterDataModel> FilterReportTypes(
            IEnumerable<ConvergenceMasterDataModel> model)
        {
            return model.Where(
                x => x.Code.Equals("VIS_CBO_A") ||
                    x.Code.Equals("VIS_CBO_MO") ||
                    x.Code.Equals("VIS_CBO_VO"))
                .OrderBy(x => x.Name).ToList();
        }
    }
}