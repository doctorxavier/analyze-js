function searchRolesAndPermissions() {
    var operationNumber = $("[name='operation']").val();
    var url = $("[name='search']").data("url");
    var $operationNumberContainer = $("#operationNumber");
    var $searchResultContainer = $("#searchResultContainer");

    $.ajax({
        url: url,
        type: "GET",
        data: { "operation": operationNumber }
    }).done(function (response) {
        
        $searchResultContainer.css("display", "none");
        $operationNumberContainer.find(".operationNumberLabel").remove();
        
        if (!response.IsValid || !response.Model.HasOperation || !response.Model.Member) {
            showMessage(response.ErrorMessage);
        } else {
            $searchResultContainer.css("display", "block");
            populateRolesAndPermissions(response, "Roles", "#rolesResult");
            populateRolesAndPermissions(response, "Permissions", "#permissionsResult");
            $operationNumberContainer.append("<label class='labelNormal bold operationNumberLabel'>" + operationNumber + "</label>");
        }
    })
}

function populateRolesAndPermissions(response, group, container) {
    var data = "";
    
    if(response.Model[group] !== null || response.Model[group].length > 0){
        $.each(response.Model[group], function (index, value) {
            data += "<li>" + value + "</li>";
        })
    }else{
        data = "<li>" + response.ErrorMessage + "</li>";
    }

    $(container).html("").append(data);
}