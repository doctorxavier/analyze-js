using System;
using System.Collections.Generic;
using System.Linq;

using IDB.MW.Application.AttributesModule.ViewModels.AttributesService;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.AdministrationSecondPhase.Models.Attribute
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateAttributeViewModel(this AttributeEditViewModel viewModel, ClientFieldData[] clientField)
        {
            var id = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("AttributeId"));
            if (id != null)
            {
                viewModel.AttributeId = Convert.ToInt32(id.Value);
            }

            var code = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("code"));
            if (code != null)
            {
                viewModel.Code = code.Value;
            }

            var nameEn =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("englishName"));
            if (nameEn != null)
            {
                viewModel.EnglishName = nameEn.Value;
            }

            var nameEs =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("spanishName"));
            if (nameEs != null)
            {
                viewModel.SpanishName = nameEs.Value;
            }

            var namePt =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("portugueseName"));
            if (namePt != null)
            {
                viewModel.PortugueseName = namePt.Value;
            }

            var nameFr =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("frenchName"));
            if (nameFr != null)
            {
                viewModel.FrenchName = nameFr.Value;
            }

            var description =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("description"));
            if (description != null)
            {
                viewModel.Description = description.Value;
            }

            var section =
                clientField.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("section")).ToList();
            if (section != null)
            {
                var firstOrDefault = section.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    viewModel.Section = firstOrDefault.Value.Split(',').Select(int.Parse).ToList();
                }
            }

            var group = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("group"));
            if (group != null)
            {
                if (!string.IsNullOrEmpty(group.Value))
                {
                    viewModel.Group = group.Value;
                }
                else
                {
                    viewModel.Group = string.Empty;
                }
            }

            var order = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("order"));
            if (order != null)
            {
                viewModel.Order = Convert.ToInt32(order.Value);
            }

            var columnSize =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("columnSize"));
            if (columnSize != null)
            {
                viewModel.ColumnSize = Convert.ToInt32(columnSize.Value);
            }

            var operationType =
                clientField.Where(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("operationType")).ToList();
            if (operationType != null)
            {
                var firstOrDefault = operationType.FirstOrDefault();
                if (firstOrDefault != null)
                {
                    viewModel.OperationType = firstOrDefault.Value.Split(',').Select(int.Parse).ToList();
                }
            }

            var relationshipType =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("relationshipType"));
            if (relationshipType != null)
            {
                if (!string.IsNullOrEmpty(relationshipType.Value))
                {
                    viewModel.RelationshipType = relationshipType.Value.Split(',').Select(int.Parse).ToList();
                }
                else
                {
                    viewModel.RelationshipType = new List<int>();
                }
            }

            var visibilityBr = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("visibilityBR"));
            if (visibilityBr != null)
            {
                if (!string.IsNullOrEmpty(visibilityBr.Value))
                {
                    viewModel.VisibilityBr = visibilityBr.Value;
                }
                else
                {
                    viewModel.VisibilityBr = null;
                }
            }

            var effectiveDate =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("effectiveDate"));
            if (effectiveDate != null)
            {
                if (!string.IsNullOrEmpty(effectiveDate.Value))
                {
                    viewModel.EffectiveDate = Convert.ToDateTime(effectiveDate.Value);
                }
                else
                {
                    viewModel.EffectiveDate = DateTime.Now;
                }
            }

            var expirationDate =
                clientField.FirstOrDefault(
                    o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("expirationDate"));
            if (expirationDate != null)
            {
                if (!string.IsNullOrEmpty(expirationDate.Value))
                {
                    viewModel.ExpirationDate = Convert.ToDateTime(expirationDate.Value);
                }
                else
                {
                    viewModel.ExpirationDate = null;
                }
            }

            var type = clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("type"));
            if (type != null)
            {
                viewModel.Type = Convert.ToInt32(type.Value);
            }

            var reference =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("reference"));
            if (reference != null)
            {
                if (!string.IsNullOrEmpty(reference.Value))
                {
                    viewModel.ReferenceId = Convert.ToInt32(reference.Value);
                }
                else
                {
                    viewModel.ReferenceId = null;
                }
            }

            var length =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("length"));
            if (reference != null)
            {
                if (!string.IsNullOrEmpty(length.Value))
                {
                    viewModel.Length = Convert.ToInt32(length.Value);
                }
                else
                {
                    viewModel.Length = null;
                }
            }
            
            var validation =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("validation"));
            if (validation != null)
            {
                viewModel.Validation = Convert.ToInt32(validation.Value);
            }

            var validationBrName =
                clientField.FirstOrDefault(o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("validationBRName"));
            if (validationBrName != null)
            {
                if (!string.IsNullOrEmpty(validationBrName.Value))
                {
                    viewModel.ValidationBrName = validationBrName.Value;
                }
                else
                {
                    viewModel.ValidationBrName = null;
                }
            }

            var validationMessage =
                clientField.FirstOrDefault(
                    o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("validationMessage"));
            if (validationMessage != null)
            {
                if (!string.IsNullOrEmpty(validationMessage.Value))
                {
                    viewModel.ValidationMessage = validationMessage.Value;
                }
                else
                {
                    viewModel.ValidationMessage = null;
                }
            }

            var valitionNewMessage =
                clientField.FirstOrDefault(
                    o => !string.IsNullOrWhiteSpace(o.Name) && o.Name.Equals("newValidationMessage"));
            if (valitionNewMessage != null)
            {
                viewModel.NewValidationMessage = Convert.ToBoolean(valitionNewMessage.Value);
            }
        }
    }
}