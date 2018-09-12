(function () {
    var app = angular.module('disclosure', []);

    app.controller('ESGCtrl', ESGCtrl);

    app.factory("DataService", ['$http', '$q', function ($http, $q) {

        return {
            getAll: getAll,
            edit: edit,
            save: save,
            closeMilestone: closeMilestone,
            canCloseMilestone: canCloseMilestone,
            checkAndUpdatePublicationDate: checkAndUpdatePublicationDate
        }

        function getAll(route, object) {
            return callAjax(route, object);
        }

        function edit(route, object) {
            return callAjax(route, object);
        }

        function save(route, object) {
            return callAjax(route, object);
        }

        function closeMilestone(route, object) {
            return callAjax(route, object);
        }

        function canCloseMilestone(route, object) {
            return callAjax(route, object);
        }

        function checkAndUpdatePublicationDate(route, object) {
            return callAjax(route, object);
        }

        function callAjax(route, object) {
            var defered = $q.defer();
            var promise = defered.promise;

            $http.post(route, object,
                {
                    async: false,
                    cache: false
                })
                .then(function (result) {
                    defered.resolve(result.data)
                }, function (err) {
                    defered.reject(err)
                });

            return promise;
        }
    }]);

    function ESGCtrl($scope, DataService, $window, $timeout, $filter) {

        $scope.rootPath = null;
        $scope.savePath = null;
        $scope.saveAndSendPath = null;
        $scope.closeMilestonePath = null;
        $scope.canCloseMilestonePath = null;
        $scope.ESGDisclosureDocuments = [];

        $scope.LoadDisclosures = function (route) {
            DataService
                .getAll(route, new Object())
                .then(function (data) {
                    $scope.ESGDisclosureDocuments = data.Models;
                })
                .catch(function (err) {
                    $window.showMessage(Localization.GetText['ErrorMessage']);
                })
        };

        $scope.NewRow = function (i) {
            $window.showLoader();
            $timeout(function () {
                $scope.ESGDisclosureDocuments[i].DocumentESGDisclosure.push(
                {
                    DocumentId: null,
                    ESGDisclosureDocumentId: $scope.ESGDisclosureDocuments[i].ESGDisclosureDocumentId,
                    DisclosureActivityId: null,
                    DisclosureActivityName: '',
                    UploadDate: null,
                    UserName: null,
                    UserId: null,
                    EzShare: null,
                    DocumentName: null,
                    Action: null,
                    IsReadyToDisclosure: false,
                    IsSendToDisclosure: false,
                    IsDisclosureDate: null,
                    IsDelete: false,
                    IsNew: true,
                    HasESGRoleToWrite: $scope.ESGDisclosureDocuments[i].HasESGRoleToWrite,
                    HasTeamLeaderRole: $scope.ESGDisclosureDocuments[i].HasTeamLeaderRole,
                    Comments: []
                })
                $scope.$apply();
            }, 200);
            $window.hideLoader();
        }

        $scope.HasComment = function (pi, i) {
            if (!$scope.ESGDisclosureDocuments[pi].DocumentESGDisclosure[i].Comments.length) {
                $scope.NewComment(pi, i);
            }
        }

        $scope.NewComment = function (pi, i) {
            $window.showLoader();
            $timeout(function () {
                var userName = $scope.ESGDisclosureDocuments[0].UserLoginName;
                $scope.ESGDisclosureDocuments[pi].DocumentESGDisclosure[i].Comments.push(
                {
                    CommentId: 0,
                    CreatedBy: userName,
                    Created: new Date(),
                    Comment: null,
                    IsDelete: false,
                    CanEditOrDelete: true
                })
                $scope.$apply();
            }, 200);
            $window.hideLoader();
        }

        $scope.DeleteComment = function (ev, documents, commentId, idx) {
            $window.showLoader();
            $(ev.currentTarget).closest('div.inputComment ').find('textarea').css('border', '1px solid red');
            $timeout(function () {

                if (commentId == 0) {
                    documents.Comments.splice(idx, 1);
                    return;
                }

                var comment = $filter('filter')(documents.Comments, { CommentId: commentId }, true)[0];
                comment.IsDelete = true;

            }, 500);
            $window.hideLoader();
        }

        $scope.reloadRoute = function () {
            $window.location.reload();
        }

        $scope.reloadRootPath = function () {
            $window.location.href = Localization.GetText['RootPath'];
        }

        var validSave = function () {
            $scope.IsValidSave = true;
            angular.forEach($scope.ESGDisclosureDocuments, function (esgDocument) {
                angular.forEach(esgDocument.DocumentESGDisclosure, function (doc) {
                    if (!doc.IsDelete && (doc.DocumentId === null || doc.DisclosureActivityId === null)) {
                        $scope.IsValidSave = false;

                        if (doc.DisclosureActivityId === null)
                            doc.IsRequired = true;

                        var msg = doc.DocumentId === null ?
                            Localization.GetText['WarningCompleteRequired'] :
                            Localization.GetText['WarningCompleteActivity'];

                        $window.confirmActionCustom(msg, null);
                    }
                });
            });
        }

        var close = function (index) {
            var msg = index == 0 ?
                Localization.GetText['WarningCloseMilestone1'] :
                Localization.GetText['WarningCloseMilestone2'];

            $window.confirmAction(msg).done(function (value) {
                if (!value)
                    return false;

                $window.showLoader();
                DataService
                    .closeMilestone($scope.closeMilestonePath, { models: $scope.ESGDisclosureDocuments })
                    .then(function (data) {
                        showNotification({
                            'message': data.NotificationMessage,
                            'type': data.NotificationType,
                            "autoClose": "true",
                            "duration": "5"
                        });

                        $window.location.href = Localization.GetText['RootPath'];
                    })
                    .catch(function (err) {
                        $window.location.href = Localization.GetText['RootPath'];
                    })
            });
        }

        $scope.Edit = function (route) {
            $window.showLoader();
            $window.location.href = route;
        }

        $scope.Save = function (i) {
            validSave();

            if (!$scope.IsValidSave) {
                return false;
            }

            $window.showLoader();
            $scope.ESGDisclosureDocuments[i].IsSaveAndSend = false;
            DataService
                .save($scope.savePath, { models: $scope.ESGDisclosureDocuments })
                .then(function (data) {
                    showNotification({
                        'message': data.NotificationMessage,
                        'type': data.NotificationType,
                        "autoClose": "true",
                        "duration": "5"
                    });

                    $window.location.href = Localization.GetText['RootPath'];
                })
                .catch(function (err) {
                    $window.location.href = Localization.GetText['RootPath'];
                })
        }

        $scope.SaveAndSend = function (i) {
            validSave();

            if (!$scope.IsValidSave) {
                return false;
            }

            var msg = $scope.ESGDisclosureDocuments[i].HasESGRoleToWrite ?
                Localization.GetText['WarningESGToTL'] :
                Localization.GetText['WarningTLtoESG'];

            $window.confirmAction(msg).done(function (value) {
                if (!value)
                    return false;

                $window.showLoader();
                $scope.ESGDisclosureDocuments[i].IsSaveAndSend = true;
                DataService
                    .save($scope.saveAndSendPath, { models: $scope.ESGDisclosureDocuments })
                    .then(function (data) {
                        showNotification({
                            'message': data.NotificationMessage,
                            'type': data.NotificationType,
                            "autoClose": "true",
                            "duration": "5"
                        });

                        $window.location.href = Localization.GetText['RootPath'];
                    })
                    .catch(function (err) {
                        $window.location.href = Localization.GetText['RootPath'];
                    })
            });
        }

        $scope.CloseMilestone = function (i) {
            DataService
                .canCloseMilestone($scope.canCloseMilestonePath, { models: $scope.ESGDisclosureDocuments })
                .then(function (data) {
                    if (!data.IsValid) {
                        var milestoneName = $scope.ESGDisclosureDocuments[i].MilestoneName;
                        $window.confirmAction(String.format(data.ErrorMessage, milestoneName))
                            .done(function (value) {
                                if (!value)
                                    return false;
                            })

                        $('.vex-dialog-buttons').html('');
                    }
                    else {
                        close(i);
                    }
                })
                .catch(function (err) {
                    $window.location.href = Localization.GetText['RootPath'];
                })
        }

        $scope.UpdateDocument = function (model) {
            $scope.$apply(function () {
                DataService
                        .checkAndUpdatePublicationDate($scope.checkAndUpdatePublicationDate, model)
                        .then(function (data) {
                            if (data.IsValid) {
                                if (data.AuthorizedDate != '') {
                                    model.DisclosureDate = data.AuthorizedDate;
                                    model.IsReadyToDisclosure = false;
                                    model.IsSendToDisclosure = false;
                                }
                                else {
                                    model.DisclosureDate = "";
                                }

                                $scope.ESGDisclosureDocuments[$scope.parentIndex].DocumentESGDisclosure[$scope.Index] =
                                {
                                    DocumentId: model.DocumentId,
                                    ESGDisclosureDocumentId: $scope.ESGDisclosureDocuments[$scope.parentIndex].ESGDisclosureDocumentId,
                                    DisclosureActivityId: null,
                                    DisclosureActivityName: '',
                                    UploadDate: model.UploadDate,
                                    UserName: model.UserName,
                                    UserId: model.UserId,
                                    EzShare: model.EzShare,
                                    DocumentName: model.DocumentName,
                                    IsNew: model.IsNew,
                                    DisclosureDate: model.DisclosureDate,
                                    IsReadyNotificated: model.IsReadyNotificated,
                                    IsSendNotificated: model.IsSendNotificated
                                };
                            }
                            else {
                                showNotification({
                                    'message': data.ErrorMessage,
                                    'type': 'error',
                                    'autoClose': 'false',
                                    'duration': '5'
                                });
                            }
                        })
                        .catch(function (err) {
                            $window.location.href = Localization.GetText['RootPath'];
                        })
            });
            vex.close();
        };

        $scope.Cancel = function (route) {
            $window.confirmAction(Localization.GetText['WarningCancel']).done(function (value) {
                if (value)
                    $window.location.href = Localization.GetText['RootPath'];
            });
        }

        $scope.RemoveDocument = function (p, i) {
            $window.showLoader();
            $timeout(function () {
                if ($scope.ESGDisclosureDocuments[p].DocumentESGDisclosure[i].DocumentId === null) {
                    $scope.ESGDisclosureDocuments[p].DocumentESGDisclosure.splice(i, 1);
                    return;
                }

                $scope.ESGDisclosureDocuments[p].DocumentESGDisclosure[i].IsDelete = true;
            }, 200);
            $window.hideLoader();
        };

        $scope.SetIndexDocument = function (parentIndex, index) {
            $scope.parentIndex = parentIndex;
            $scope.Index = index;
        };
    };

})();
