using System;
using System.Linq;

using IDB.Presentation.MVC4.Models.ClientFieldData;
using IDB.MW.Application.AdministrationModule.ViewModels.Institution;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Institution
{
    public static class ClientFieldDataMappers
    {
        #region Mappers

        public static void UpdateInstitutionViewModel(this InstitutionEditViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var id = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("InstitutionId"));
            if (id != null)
            {
                viewModel.InstitutionId = Convert.ToInt32(id.Value);
            }            

            var name = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtName"));
            if (name != null)
            {
                viewModel.Name = name.Value;
            }

            var address = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtAddress"));
            if (address != null)
            {
                viewModel.Address = address.Value;
            }

            var city = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtCity"));
            if (city != null)
            {
                viewModel.City = city.Value;
            }

            var state = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtState"));
            if (state != null)
            {
                viewModel.State = state.Value;
            }

            var zipCode = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtZipCode"));
            if (zipCode != null)
            {
                viewModel.ZipCode = zipCode.Value;
            }

            var country = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("InstitutionEditCountry"));
            if (country != null)
            {
                viewModel.CountryId = Convert.ToInt32(country.Value);
            }

            var telephoneNumber = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtTelNumber"));
            if (telephoneNumber != null)
            {
                viewModel.TelNumber = telephoneNumber.Value;
            }

            var faxNumber = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtFaxNumber"));
            if (faxNumber != null)
            {
                viewModel.FaxNumber = faxNumber.Value;
            }

            var website = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtWebsite"));
            if (website != null)
            {
                viewModel.Website = website.Value;
            }

            var typeInstitution = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("InstitutionEditTypeList"));
            if (typeInstitution != null)
            {
                viewModel.TypeId = Convert.ToInt32(typeInstitution.Value);
            }

            var acronymCode = clientFieldData.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("TxtAcronym"));
            if (acronymCode != null)
            {
                viewModel.Acronym = acronymCode.Value;
            }
        }
        #endregion
    }
}