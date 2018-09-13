using System;
using System.Collections.Generic;
using System.Linq;

using IDB.Architecture.Language;
using IDB.MW.Application.Core.ViewModels;
using IDB.MW.Application.DEMModule.ViewModels;
using IDB.MW.Application.IndicatorsModuleNew.ViewModels.LinkPredefinedIndicator;
using IDB.Presentation.MVC4.Models.ClientFieldData;

namespace IDB.Presentation.MVC4.Areas.DEM.Models
{
    public static class ClientFieldDataMappers
    {
        public static void UpdateDemViewModel(
            this DemViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var noteFieldData = clientFieldData.First(x => 
                !string.IsNullOrWhiteSpace(x.Name) && 
                x.Name.Contains("EvaluabilityAssessmentNote"));

            var evaluabilityAssessmentNote = clientFieldData.First(x => 
                !string.IsNullOrWhiteSpace(x.Name) &&
                x.Name.Contains("EvaluabilityAssessmentNote"))
                .Value;
            
            if (noteFieldData.Name.Contains("-" + Language.EN))
            {
                if (viewModel.Summary.ShowDEMRequired)
                {
                    var isRequired = clientFieldData.First(x => 
                        !string.IsNullOrWhiteSpace(x.Name) && 
                        x.Name.Equals("txtRequiredDEM"))
                        .Value;

                    var required = isRequired == "False" ? 
                        clientFieldData.First(x => 
                            !string.IsNullOrWhiteSpace(x.Name) && 
                            x.Name.Equals("textRequired")).Value : 
                        string.Empty;

                    viewModel.Justification = required;
                    viewModel.Required = isRequired != "False";
                }
                
                viewModel.Summary.EvaluabilityAssessmentNote = 
                    !string.IsNullOrEmpty(evaluabilityAssessmentNote) ? 
                    evaluabilityAssessmentNote : string.Empty;
            }
            else
            {   
                viewModel.Resumen.EvaluabilityAssessmentNote = 
                    !string.IsNullOrEmpty(evaluabilityAssessmentNote) ? 
                    evaluabilityAssessmentNote : 
                    string.Empty;
            }
        }

        public static void UpdateRiskViewModel(
            this RiskViewModel viewModel, ClientFieldData[] clientFieldData)
        {
            var riskRate = clientFieldData.First(x => 
                !string.IsNullOrWhiteSpace(x.Name) && 
                x.Name.Equals("RiskRate"))
                .Value;

            viewModel.RiskRateValue = riskRate;

            var risksMatrix = clientFieldData.Where(x => 
                !string.IsNullOrWhiteSpace(x.Name) && 
                (x.Name.Equals("RisksAssess") || 
                x.Name.Equals("RiskMatrixInfoReferences")))
                .OrderBy(q => q.Id);

            var comments = clientFieldData.Where(x => 
                !string.IsNullOrEmpty(x.Name) && 
                x.Name.Contains("textComment"));

            var commentsRate = clientFieldData.Where(x => 
                !string.IsNullOrEmpty(x.Name) && 
                x.Name.Contains("txtCommentRate"));

            var commentsClassification = clientFieldData.Where(x => 
                !string.IsNullOrEmpty(x.Name) && 
                x.Name.Contains("txtCommentClassification"));

            var idRiskMatrix = risksMatrix.Select(q => q.Id).Distinct();
            
            var rowDelete = clientFieldData.FirstOrDefault(x =>
                x.Name.Equals("commentDeleteId")).Value.Split('|');

            if (rowDelete.Any())
            {
                foreach (var item in rowDelete)
                {
                    if (!item.Equals("'0'"))
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            var idComment = Convert.ToInt32(item);
                            viewModel.RowsDelete.Add(idComment);
                        }
                    }
                }
            }

            foreach (var item in idRiskMatrix)
            {
                var rowRiskMatrix = new RowRiskMatrixViewModel();
                rowRiskMatrix.RiskAssessment = risksMatrix.FirstOrDefault(x => 
                    x.Id == item && x.Name.Equals("RisksAssess")).Value.Equals("True");

                rowRiskMatrix.InfoAndReferences = risksMatrix.FirstOrDefault(x => 
                    x.Id == item && x.Name.Equals("RiskMatrixInfoReferences")).Value;

                rowRiskMatrix.RowId = Convert.ToInt32(item);
                foreach (var comment in comments)
                {
                    var listComments = comment.Name.Split('-');
                    if (listComments[2] == item)
                    {
                        var commentId = Convert.ToInt32(listComments[1]);
                        if (viewModel.RowsDelete.All(x => x != commentId))
                        {
                            rowRiskMatrix.UserComments.Add(new UserCommentDEMViewModel
                            {
                                CommentId = commentId,
                                Comment = comment.Value
                            });
                        }
                    }
                }
                
                viewModel.RiskMatrix.Add(rowRiskMatrix);
            }
            
            var mitigationMeasure = clientFieldData.Where(x => 
                !string.IsNullOrWhiteSpace(x.Name) && 
                (x.Name.Equals("MitigationMeasure") || 
                x.Name.Equals("InfoReferences") || 
                x.Name.Equals("textComment")))
                .OrderBy(q => q.Id);

            var idMitigationMeasure = mitigationMeasure.Select(q => q.Id).Distinct();

            foreach (var item in idMitigationMeasure)
            {
                var rowMitigationMeasures = new RowMitigationMeasuresViewModel();
                rowMitigationMeasures.RiskAssessment = mitigationMeasure.FirstOrDefault(x => 
                    x.Id == item && x.Name.Equals("MitigationMeasure")).Value.Equals("True");

                rowMitigationMeasures.InfoAndReferences = mitigationMeasure.FirstOrDefault(x => 
                    x.Id == item && x.Name.Equals("InfoReferences")).Value;

                rowMitigationMeasures.RowId = Convert.ToInt32(item);

                foreach (var comment in comments)
                {
                    var listComments = comment.Name.Split('-');
                    if (listComments[2] == item)
                    {
                        var commentId = Convert.ToInt32(listComments[1]);
                        if (viewModel.RowsDelete.All(x => x != commentId))
                        {
                            rowMitigationMeasures.UserComments.Add(new UserCommentDEMViewModel
                            {
                                CommentId = Convert.ToInt32(listComments[1]),
                                Comment = comment.Value
                            });
                        }
                    }
                }

                viewModel.MitigationMeasures.Add(rowMitigationMeasures);
            }

            foreach (var comment in commentsRate)
            {
                var listComments = comment.Name.Split('-');
                
                    var commentId = Convert.ToInt32(listComments[1]);
                    if (viewModel.RowsDelete.All(x => x != commentId))
                    {
                        viewModel.UserCommentsRate.Add(new UserCommentDEMViewModel
                        {
                            CommentId = Convert.ToInt32(listComments[1]),
                            Comment = comment.Value
                        });
                    }
            }

            foreach (var comment in commentsClassification)
            {
                var listComments = comment.Name.Split('-');

                var commentId = Convert.ToInt32(listComments[1]);
                if (viewModel.RowsDelete.All(x => x != commentId))
                {
                    viewModel.UserCommentsClassification.Add(new UserCommentDEMViewModel
                    {
                        CommentId = Convert.ToInt32(listComments[1]),
                        Comment = comment.Value
                    });
                }
            }
        }

        public static void UpdateAdditionalityViewModel(
            this AdditionalityTabViewModel additionalityData, ClientFieldData[] clientFieldData)
        {
            var idbEnvolvement = clientFieldData.Where(x => 
                !string.IsNullOrWhiteSpace(x.Name) && 
                (x.Name.Equals("chkAdditionality") || 
                    x.Name.Equals("txtInvInfoReferences") || 
                    x.Name.Equals("txtInvInfoReferencesR") || 
                    x.Name.Equals("txtInfoReferences")))
                .OrderBy(q => q.Id);
            
            var idIdbEnvolvement = idbEnvolvement.Select(p => p.Id).Distinct();

            var comments = clientFieldData.Where(x => 
                !string.IsNullOrEmpty(x.Name) && x.Name.Contains("textComment"));

            var rowDelete = clientFieldData.FirstOrDefault(x => 
                x.Name.Equals("commentDeleteId")).Value.Split('|');

            if (rowDelete.Any())
            {
                foreach (var item in rowDelete)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var idComment = Convert.ToInt32(item);
                        additionalityData.RowsDelete.Add(idComment);
                    }
                }
            }

            foreach (var idbId in idIdbEnvolvement)
            {
                var idbItem = new SheetAdditionality();
                idbItem.IdSheet = Convert.ToInt32(idbId);
                idbItem.CheckAdd = idbEnvolvement.FirstOrDefault(x =>
                    x.Id == idbId && x.Name.Equals("chkAdditionality")).Value.Equals("True");

                idbItem.InfoAndRef = (idbEnvolvement.FirstOrDefault(x =>
                    x.Id == idbId &&
                    (x.Name.Equals("txtInvInfoReferences") ||
                        x.Name.Equals("txtInfoReferences"))) != null) ?
                    idbEnvolvement.FirstOrDefault(x =>
                        x.Id == idbId &&
                        (x.Name.Equals("txtInvInfoReferences") ||
                            x.Name.Equals("txtInfoReferences"))).Value :
                    string.Empty;

                idbItem.InfoAndRefResumen = (idbEnvolvement.FirstOrDefault(x =>
                    x.Id == idbId && x.Name.Equals("txtInvInfoReferencesR")) != null) ?
                    idbEnvolvement.FirstOrDefault(x =>
                        x.Id == idbId && x.Name.Equals("txtInvInfoReferencesR")).Value :
                    string.Empty;

                foreach (var comment in comments)
                {
                    var listComments = comment.Name.Split('-');
                    if (listComments[3] == idbId)
                    {
                        var commentId = Convert.ToInt32(listComments[1]);
                        if (additionalityData.RowsDelete.All(x => x != commentId))
                        {
                            idbItem.UserComments.Add(new UserCommentDEMViewModel
                            {
                                CommentId = Convert.ToInt32(listComments[1]),
                                Comment = comment.Value
                            });
                        }
                    }
                }

                additionalityData.IDBInvolvement.Add(idbItem);
            }
        }

        public static void UpdateEvaluabilityViewModel(
            this EvaluabilityTabViewModel evaluabilityData, ClientFieldData[] clientFieldData)
        {
            var programLogic = clientFieldData.Where(x => 
                !string.IsNullOrWhiteSpace(x.Name) && 
                (x.Name.Equals("chkEvaluability") || 
                    x.Name.Equals("InvInfoReferences") || 
                    x.Name.Equals("InvInfoReferencesR") || 
                    x.Name.Equals("InfoReferences")))
                .OrderBy(q => q.Id);
            
            var idProgramLogic = programLogic.Select(p => p.Id).Distinct();
            var commentProgram = clientFieldData.Where(x => 
                !string.IsNullOrEmpty(x.Name) && x.Name.Contains("textComment"));

            var rowDelete = clientFieldData.FirstOrDefault(x => 
                x.Name.Equals("commentDeleteId")).Value.Split('|');

            if (rowDelete.Any())
            {
                foreach (var item in rowDelete)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        var idComment = Convert.ToInt32(item);
                        evaluabilityData.RowsDelete.Add(idComment);
                    }
                }
            }
           
            foreach (var programId in idProgramLogic)
            {
                var programItem = new SheetEvaluabilitySimple();
                programItem.IdSheet = Convert.ToInt32(programId);
                programItem.CheckAdd = programLogic.FirstOrDefault(x => 
                    x.Id == programId && x.Name.Equals("chkEvaluability")).Value.Equals("True");

                programItem.InfoAndReferences = (programLogic.FirstOrDefault(x => 
                    x.Id == programId && 
                    (x.Name.Equals("InvInfoReferences") || 
                        x.Name.Equals("InfoReferences"))) != null) ? 
                    programLogic.FirstOrDefault(x => 
                        x.Id == programId && 
                        (x.Name.Equals("InvInfoReferences") || 
                            x.Name.Equals("InfoReferences"))).Value : 
                    string.Empty;

                foreach (var comment in commentProgram)
                {
                    var listComments = comment.Name.Split('-');
                    if (listComments[3] == programId)
                    {
                        var commentId = Convert.ToInt32(listComments[1]);
                        if (evaluabilityData.RowsDelete.All(x => x != commentId))
                        {
                            programItem.UserComments.Add(new UserCommentDEMViewModel
                            {
                                CommentId = Convert.ToInt32(listComments[1]),
                                Comment = comment.Value
                            });
                        }
                    }
                }

                evaluabilityData.ProgramLogic.Add(programItem);
            }
        }

        public static AlignmentContributionDemViewModel UpdateCountryViewModel(
            this AlignmentContributionDemViewModel model, ClientFieldData[] formData)
        {
            var newModel = new AlignmentContributionDemViewModel
            {
                IsFreezingDataMode = model.IsFreezingDataMode
            };

            foreach (var indicatorData in model.IndicatorDataList)
            {
                var fieldsForIndicator = formData.Where(x => 
                    !x.ExtraData.Any() && x.Id == indicatorData.IndicatorId.ToString());

                var field = fieldsForIndicator.FirstOrDefault(x => 
                    x.Name == "indicator-Check-Country");

                var isChecked = GetBoolValue(field);

                if (!indicatorData.IsValid && !isChecked)
                {
                    newModel.IndicatorDataList.Add(indicatorData);
                }
                else if (indicatorData.IsValid)
                {
                    indicatorData.IsChecked = isChecked;
                    newModel.IndicatorDataList.Add(indicatorData);
                }
            }

            newModel.InformationDem = model.InformationDem;

            var updateComments = formData.Where(o => 
                o.Name.Equals("updateCommentTextIndicatorCountry"));

            var userCommentUpdateList = new List<UserCommentIndicatorViewModel>();
            if (updateComments != null)
            {
                var editCommentsId = formData.Where(o => 
                    o.Name.Equals("updateCommentIdIndicatorCountry"));

                var indicatorIdList = formData.Where(o => 
                    o.Name.Equals("updateIndicadorIdIndicatorCountry"));

                for (int i = 0; i < updateComments.Count(); i++)
                {
                    userCommentUpdateList.Add(new UserCommentIndicatorViewModel()
                    {
                        IndicatorId = Convert.ToInt32(indicatorIdList.ElementAt(i).Value),
                        CommentId = Convert.ToInt32(editCommentsId.ElementAt(i).Value),
                        Comment = updateComments.ElementAt(i).Value.Trim()
                    });
                }

                foreach (var commentView in userCommentUpdateList)
                {
                    foreach (var indicator in newModel.IndicatorDataList)
                    {
                        if (indicator.IndicatorId == commentView.IndicatorId)
                        {
                            foreach (var commentIndicator in indicator.UserCommentIndicatorList)
                            {
                                if ((commentIndicator.CommentId == commentView.CommentId)
                                    && (commentView.Comment != commentIndicator.Comment))
                                {
                                    commentIndicator.Comment = commentView.Comment;
                                    commentIndicator.IsModifiedComment = true;
                                    newModel.InformationDem.IsUpdateComment = true;
                                }
                            }
                        }
                    }
                }
            }

            var newTextCommentIndicator = formData.Where(o => 
                o.Name.Equals("newTextCommentIndicatorCountry"));

            if (newTextCommentIndicator != null)
            {
                var newIndicatorIdIndicator = formData.Where(o => 
                    o.Name.Equals("newIndicatorIdIndicatorCountry"));

                var newCommentIndicatorList = new List<UserCommentIndicatorViewModel>();

                for (int i = 0; i < newTextCommentIndicator.Count(); i++)
                {
                    newCommentIndicatorList.Add(new UserCommentIndicatorViewModel()
                    {
                        IndicatorId = Convert.ToInt32(newIndicatorIdIndicator.ElementAt(i).Value),
                        CommentId = 0,
                        Comment = newTextCommentIndicator.ElementAt(i).Value.Trim(),
                        Role = newModel.InformationDem.Role
                    });
                }

                newModel.InformationDem.NewCommentIndicatorList = newCommentIndicatorList;
            }

            var deleteComments = formData.FirstOrDefault(o => 
                o.Name.Equals("deleteCommentsIndicatorCountry"));

            if (deleteComments != null)
            {
                var idCommentList = deleteComments.Value.Split('|').Where(x => 
                    !string.IsNullOrEmpty(x)).ToArray();

                newModel.InformationDem.DeleteCommentsIndicatorList = new List<int>();

                foreach (string idComment in idCommentList)
                {
                    if (idComment != "0" && idComment != "'0'")
                    {
                        newModel.InformationDem.DeleteCommentsIndicatorList.Add(
                            Convert.ToInt32(idComment));
                    }
                }
            }

            var impactCheck = formData.Where(o => o.Name.Equals("impactIndicatorCheck"));
            if (impactCheck != null)
            {
                var impactCheckId = formData.Where(o => o.Name.Equals("impactIndicatorId"));

                var impactResultMatrixId = formData.Where(o => 
                    o.Name.Equals("impactResultsMatrixId"));

                for (int i = 0; i < impactCheck.Count(); i++)
                {
                    if (impactCheck.ElementAt(i).Value.Trim() == "False")
                    {
                        newModel.UnLinkIndicators.Add(new UnlinkIndicatorViewModel
                        {
                            IndicatorId = Convert.ToInt32(impactCheckId.ElementAt(i).Value),
                            AccessedByAdministrator = true,
                            TypeModule = LinkIndicatorTypeEnum.Impacts,
                            ResultsMatrixId = Convert.ToInt32(
                                impactResultMatrixId.ElementAt(i).Value)
                        });
                    }
                }
            }

            var outcomeCheck = formData.Where(o => o.Name.Equals("outcomeIndicatorCheck"));
            if (outcomeCheck != null)
            {
                var outcomeCheckId = formData.Where(o => o.Name.Equals("outcomeIndicatorId"));

                var outcomeResultMatrixId = formData.Where(o => 
                    o.Name.Equals("outcomeResultsMatrixId"));

                for (int i = 0; i < outcomeCheck.Count(); i++)
                {
                    if (outcomeCheck.ElementAt(i).Value.Trim() == "False")
                    {
                        newModel.UnLinkIndicators.Add(new UnlinkIndicatorViewModel
                        {
                            IndicatorId = Convert.ToInt32(outcomeCheckId.ElementAt(i).Value),
                            AccessedByAdministrator = true,
                            TypeModule = LinkIndicatorTypeEnum.Outcomes,
                            ResultsMatrixId = Convert.ToInt32(
                                outcomeResultMatrixId.ElementAt(i).Value)
                        });
                    }
                }
            }

            var outputCheck = formData.Where(o => o.Name.Equals("outputIndicatorCheck"));
            if (outputCheck != null)
            {
                var outputCheckId = formData.Where(o => o.Name.Equals("outputIndicatorId"));

                var outputResultMatrixId = formData.Where(o => 
                    o.Name.Equals("outputResultsMatrixId"));

                for (int i = 0; i < outputCheck.Count(); i++)
                {
                    if (outputCheck.ElementAt(i).Value.Trim() == "False")
                    {
                        newModel.UnLinkIndicators.Add(new UnlinkIndicatorViewModel
                        {
                            IndicatorId = Convert.ToInt32(outputCheckId.ElementAt(i).Value),
                            AccessedByAdministrator = true,
                            TypeModule = LinkIndicatorTypeEnum.Outputs,
                            ResultsMatrixId = Convert.ToInt32(
                                outputResultMatrixId.ElementAt(i).Value)
                        });
                    }
                }
            }

            return newModel;
        }

        private static bool GetBoolValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            value = value.ToLowerInvariant();

            return value == "true";
        }

        private static bool GetBoolValue(ClientFieldData field, bool valueIfNull = false)
        {
            return field == null ? valueIfNull : GetBoolValue(field.Value);
        }
    }
}