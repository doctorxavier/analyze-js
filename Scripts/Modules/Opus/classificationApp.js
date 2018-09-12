(function () {
    var app = angular.module('ClassificationsApp', []);

    app.controller('ClassificationsCtrl', ClassificationsCtrl);

    function ClassificationsCtrl($scope) {

        $scope.Model =
        {
            ElementType: null,
            ElementToRemoveId: null,
            IsApprMilestoneCompleted: null,
            HasAnySAClassifWithUniqueIndicator: null,
            HeaderPopupMessage: '',
            SaveUrl: '',
            AvailableImpactIndicators: [],
            AvailableOutcomeIndicators: [],
            AvailableOutputIndicators: [],
            Classifications: [{
                ClassificationId: null,
                ClassificationName: '',
                AssociatedImpactIndicators: [],
                AssociatedOutcomeIndicators: [],
                AssociatedOutputIndicators: []
            }]
        };

        $scope.LoadClassifications = function (model) {
            $scope.$apply(function () {
                $scope.Model = model;
            });
        };

    }

})();

$(document).ready(function () {

    $('.init-select').select2({ minimumResultsForSearch: -1 });

    $(document).on('click', 'button[name="save"]', function () {

        checkIndicatorBeforeSave();
    });
});

var Classification = new Object();
var data;
var Route;

Classification._InitSelect = function (placeHolder) {
    $('.init-select').select2({
        minimumResultsForSearch: -1,
        placeholder: placeHolder,
        width: '100%'
    });
};

Classification._ShowModalOrUpdate = function () {
    var obj = angular.element(document.getElementById('classificationsContainer')).scope().Model;

    if (obj.HasAnySAClassifWithUniqueIndicator) {
        $('#checkRmIndicatorRelationsModal').modal();
    }
    else {
        $('#checkRmIndicatorRelationsModal .close').click();
        Classification._ClassificationSaveCallback.call(this);
    }
};

Classification._ClassificationSaveCallback = function () { };

function checkRmIndicatorRelationsToSAClassifications(placeHolder, route, saveCallback) {
    Classification._ClassificationSaveCallback = saveCallback ? saveCallback : function () { };

    $.ajax({
        url: route,
        type: 'GET',
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (!response.IsValid) {
                Alert.ShowError(response.ErrorMessage);
                return false;
            }

            angular
                .element(document.getElementById('classificationsContainer'))
                .scope()
                .LoadClassifications(response.Model);

            Classification._InitSelect(placeHolder);
            Classification._ShowModalOrUpdate();
        }
    });
}

function updateRMIndicatorRelationsToSAClassifications() {
    var obj = angular.element(document.getElementById('classificationsContainer')).scope().Model;
    Route = obj.SaveUrl;
    var isApprMilestoneCompleted = obj.IsApprMilestoneCompleted;


        $.ajax({
            url: Route,
            type: 'POST',
            data: JSON.stringify(obj),
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                if (!response.IsValid) {
                    Alert.ShowError(response.ErrorMessage);
                    return false;
                }

                $('#checkRmIndicatorRelationsModal .close').click();
                Classification._ClassificationSaveCallback.call(this);
            }
        });
}

function updateSAClassificationsFromIndicatorForm() {
    var obj = angular.element(document.getElementById('classificationsContainer')).scope().Model;
    Route = obj.SaveUrl;

    $.ajax({
        url: Route,
        type: 'POST',
        data: JSON.stringify(obj),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (!response.IsValid) {
                Alert.ShowError(response.ErrorMessage);
                return false;
            }

            $('#checkRmIndicatorRelationsModal .close').click();
        }
    });
}

function checkIndicatorBeforeSave() {
    var url = $('#hdnBeforeDeleteUrl').val();
    var obj = angular.element(document.getElementById('classificationsContainer')).scope().Model;

    $.ajax({
        url: url,
        type: 'POST',
        data: JSON.stringify(obj),
        contentType: 'application/json; charset=utf-8',
        success: function (response) {
            if (!response.IsValid) {
                Alert.ShowError(response.ErrorMessage);
                return false;
            }

            $('#checkRmIndicatorRelationsModal .close').click();
            Classification._ClassificationSaveCallback.call(this);
        }
    });
}
