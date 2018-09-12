function applyFieldsBehavior(countryId, typeId) {

    countryId = (countryId) ? countryId : '0';
    typeId = (typeId) ? typeId : '0';

    var arrPageNames = document.querySelectorAll('[data-idb-pagename]');

    var arrPagesConcat = '';
    if (arrPageNames != null) {
        for (var h = 0; h < arrPageNames.length; h++) {
            arrPagesConcat = arrPagesConcat.concat($(arrPageNames[h]).attr('data-idb-pagename'));
            if (h != arrPageNames.length - 1)
               arrPagesConcat = arrPagesConcat.concat('|');
        }
    }

    applyFieldsBehaviorByPages(arrPagesConcat, countryId, typeId);
}

function applyFieldsBehaviorByPages(pageNames, countryId, typeId) {
    
    var completeUrl = window.location.href;
    var pathArray = completeUrl.split("/");
    var newUrl = pathArray[0] + '//' + pathArray[2] + "/" + pathArray[3];

    var arr = [];
    var urlPath = idbg.getPath('Global/Security/GetFieldsBehaviourByPage');

    for (var x = 0; x < pathArray.length; x++) {
        var item = pathArray[x];
        if (item.length > 4 && (item.indexOf(':') === -1) && !isNaN(item.substring(item.length - 4, item.length))) {
            urlPath = urlPath.replace("Global", item+"/Global");
            break;
        }
    }

    var data = { 'pageNames': pageNames, 'countryId': countryId, 'typeId': typeId };

    $.ajax({
        type: "POST",
        data: JSON.stringify(data),
        async: false,
        url: urlPath,
        contentType: "application/json;charset=utf-8",
        dataType: "json",
        success: function (result) {
            arr = result;
        }
    });

    for (var i = 0; i < arr.length; i++) {
        var obj = arr[i];
        var elements = $("[data-idb-fieldname='" + obj.FieldName + "']");

        if (elements.length === 0)continue;

        if (Boolean(obj.IsReadOnly)) {
            elements.attr("disabled", true);
        } else {
            elements.removeAttr("disabled");
        }

        if (Boolean(obj.IsVisible)) {
            $(elements).show();
        }
        else {
            $(elements).hide();

        }

        if (Boolean(obj.IsRequired)) {
            elements.find("label").not(elements.find("label").parent("[data-pagemode=read]").find("label")).append("<span class='asteriskColor'>*</span>");
            elements.attr("data-parsley-required", true);
        } else {
            elements.find('label span.asteriskColor').remove();
            elements.find('label').siblings('span.asteriskColor').remove();
            elements.removeAttr("data-parsley-required");
        }
    }
}


$(window).load(function () { applyFieldsBehavior('0', '0') });