function GetIndicatorTypes(indicatorTypes) {
    var a = indicatorTypes.PredefinedIndicatorTypes;
    if (a.length > 0) {
        var texto = '';
        for (var i = 0; i < a.length; i++) {
            texto = texto + a[i].PredefinedIndicatorTypes.NameEn;
            if (i < a.length - 1) {
                texto = texto + ' - ';
            }
        }

        return texto;
    }

    return "";
}

var PredefinedIndicatorFilter = function () {
    var self = this;
    self.IndicatorName = ko.observable();
    self.IndicatorTypeResult = ko.observable();
    self.IndicatorNumber = ko.observable();
    self.UnitOfMeasure = ko.observable();
    self.DisplayExpired = ko.observable();
    self.IndicatorType = ko.observable();
    self.AreaId = ko.observable();
    self.Page = ko.observable();
    self.Size = ko.observable();
}

var ViewModel = function () {
    var self = this;
    self.IndicatorType = ko.observable();
    self.PriorityArea = ko.observableArray();
    self.SelectedPriorityArea = ko.observable();
    self.SelectedIndicatorTypes = ko.observable();
    self.IndicatorsResultTypes = ko.observableArray();
    self.indicators = ko.observableArray();
    self.AddPriorityArea = function (data) {
        self.PriorityArea(data);
    }
    self.AddIndicatorType = function (data) {
        self.IndicatorType(data);
    }

    self.PredefinedIndicatorFilter = new PredefinedIndicatorFilter();
    self.IndicatorTypeSelect = ko.dependentObservable({
        read: self.PredefinedIndicatorFilter.IndicatorType,
        write: function (indicatorType) {
            self.PredefinedIndicatorFilter.IndicatorType(indicatorType);

            // Get Indicator Types
            $.ajax({
                url: $("#ddlIndicatorType").data("getindicator-path") + "/?SelectedIndicatorTypes=" + indicatorType,
                dataType: "json",
                contentType: "application/json",
                type: 'post',
                success: function (data) {
                    vm.IndicatorsResultTypes(data);
                }
            });
        },
        owner: self
    });
}

$(document).ready(function () {
    
    idbg.lockUi(null, true);
    // Get Prioriry Areas
    $.ajax({
        url: $("#hdnPriorityArea").val(),
        dataType: "json",
        contentType: "application/json",
        type: 'post',
        success: function (data) {
            window.vm = new ViewModel();
            ko.applyBindings(vm);
            vm.AddPriorityArea(data);

            // Get Indicator Types
            $.ajax({
                url: $("#hdnTypeOfIndicator").val(),
                dataType: "json",
                contentType: "application/json",
                type: 'post',
                success: function (indicators) {
                    vm.AddIndicatorType(indicators);
                    $("div.loading-container").hide();
                }
            });
        }
    });

    $("#typeIndicatorId").change(function () {
        var selectedItem = $(this).val();
        var ddlPriority = $("#priorityArea");
        $.ajax({
            url: $("#hdnPriorityArea").val(),
            success: function (data) {

            }
        });
    });

    // Evento Click of button search
    $("#btnSearchIndicator").click(function (e) {
        $("#notFoundMessage").hide();
        e.preventDefault();

        $.ajax({
            url: $(this).data("search-path"),
            data: ko.mapping.toJSON(vm.PredefinedIndicatorFilter),
            dataType: "json",
            contentType: "application/json",
            type: 'post',
            success: function (data) {
                setDataSource(data);

            }
        });
    });

    // Evento Click of button search
    $("#btnNewIndicator").click(function (e) {
        $("#notFoundMessage").hide();
        e.preventDefault();
        document.location.href = $(this).data("detail-path") + '/?PredefinedIndicatorId=';
    });

    ko.bindingHandlers.kendoDropDown = {
        init: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            var unwrap = ko.utils.unwrapObservable;
            var dataSource = valueAccessor();
            var binding = allBindingsAccessor();
            var valueProp = unwrap(binding.dataValueField);
            var labelProp = unwrap(binding.dataTextField) || valueProp;
            var options = {};
            options.dataTextField = "text";
            options.dataValueField = "value";
            if (binding.dropDownOptions) {
                options = $.extend(options, binding.dropDownOptions);
            }

            //handle value changing
            var modelValue = binding.value;
            if (modelValue) {
                var handleValueChange = function () {
                    //Get the selected Value from the Kendo ComboBox
                    var selectedText = this.text();
                    var selectedValue = this.value();
                    if (ko.isWriteableObservable(modelValue)) {
                        //Since this is an observable, the update part will fire and select the 
                        //  appropriate display values in the controls
                        modelValue(selectedValue);
                    } else {  //write to non-observable
                        if (binding['_ko_property_writers'] && binding['_ko_property_writers']['value']) {
                            binding['_ko_property_writers']['value'](selectedValue);
                        }
                    }

                    return false;
                };

                options.change = handleValueChange;
            }

            //handle the choices being updated in a Dependant Observable (DO), so the update function doesn't 
            // have to do it each time the value is updated. Since we are passing the dataSource in DO, if it is
            // an observable, when you change the dataSource, the dependentObservable will be re-evaluated
            // and its subscribe event will fire allowing us to update the autocomplete datasource
            var mappedSource = ko.dependentObservable(function () {
                var mapped = ko.utils.arrayMap(unwrap(dataSource), function (item) {
                    var result = {};
                    result.text = unwrap(item[labelProp]);  //show in pop-up choices
                    result.value = unwrap(item[valueProp]);  //value 
                    return result;
                });
                return mapped;
            }, viewModel);
            //Subscribe to the knockout observable array to get new/remove items
            mappedSource.subscribe(function (newValue) {
                //displayElement.kendoAutoComplete("option", "source", newValue);
                var combo = $(element).data('kendoDropDownList');
                var previousValue = combo.value();
                combo.dataSource.data(newValue);
                //Make sure after we reset the datasource we reset the selected value also
                combo.value(previousValue);
            });

            //options.dataSource = ko.mapping.toJS({ data: ko.mapping.toJS(mappedSource()) });
            options.dataSource = new kendo.data.DataSource({ data: mappedSource() });
            $(element).kendoDropDownList(options);
        },
        update: function (element, valueAccessor, allBindingsAccessor, viewModel) {
            //update value based on a model change
            var unwrap = ko.utils.unwrapObservable;
            var dataSource = valueAccessor();
            var binding = allBindingsAccessor();
            var valueProp = unwrap(binding.dataValueField);
            var labelProp = unwrap(binding.dataTextField) || valueProp;
            var modelValue = binding.value;

            if (modelValue) {
                var currentModelValue = unwrap(modelValue);
                //Set the hidden box to be the same as the viewModels Bound property
                $(element).data('kendoDropDownList').value(currentModelValue);
            }
        }
    };

    $(document).ajaxStart(function () {
        idbg.lockUi(null, true);
    });

    $(document).ajaxStop(function () {
        $(".k-dropdown-wrap").css("height", "35px");
        $(".k-input").css("height", "35px");
        $(".this").css("width", "17%");
        $(".k-icon").css("vertical-align", "middle");
        $("div.loading-container").hide();
        $(".k-pager-wrap").css("width", "96%");
        $(".k-pager-numbers").css("float", "right");
        $(".k-grid").css("width", "1050px");
        $("#content").removeClass("hide");
        
    });

    $("#btnClearAll").click(function () {
        $(".cmbclear option[value=0]").selected = true;
        $("#ddlIndicatorType option[value=0]").selected = true;
        $("#unitOfMeasure").val("");
        $("#indicatorNumber").val("");
        $("#indicatorName").val("");
        $("#specialclause2").attr("checked", false);
    });
});

function setDataSource(results) {

    var ds = new kendo.data.DataSource({
        data: results,
        pageSize: 25,
        batch: true,
    });

    $("div.k-grid-pager").html("");

    if (results.length > 0) {

        $("#countResults").text(results.length);

        $("#countResultsContainer").show();

        var grid = $("#main-results-grid").kendoGrid({
            scrollable: false,
            dataSource: ds,
            editable: false,
            sortable: false,
            selectable: false,
            filterable: false,
            pageable: true,
            info: false,
            previousNext: false,
            resizable: true,
            messages: {
                display: "",
                first: "",
                previous: "",
                next: "",
                last: "",
                refresh: ""
            },
            columns: [
                {
                    field: "IndicatorNumber",
                    title: $("#hdnIndicatorNumber").val(),
                    width: 160
                },
            {
                field: "NameEn",
                title: $("#hdnIndicatorName").val(),
                width: 350
            },
            {
                template: "#= GetIndicatorTypes(data) #",
                title: $("#hdnTypeIndicator").val(),
                width: 160
            },
            {
                field: "UnitOfMeasureEn",
                title: $("#hdnUnitMesure").val(),
                width: 160
            },
            {
                title: $("#hdnActions").val(),
                template: kendo.template($("#gridCell_operationName").html()),
            }]
        });

        if (results.length <= 25) {
            $(".k-pager-wrap").hide();
        } else {
            $(".k-pager-wrap").show();
        }

        $("th.k-header").removeClass("k-header");

        $("a.k-pager-nav").hide();

        $("span.k-pager-info").hide();

        $("#countResultsContainer").show();

        $("#main-results-grid").show();

        //grid.dataSource.page(1);

    } else {
        if ($('#main-results-grid tr').length > 0) {

            $("#main-results-grid").data('kendoGrid').dataSource.data([]);

            $("#countResultsContainer").hide();

            $("#main-results-grid").hide();

            $("#notFoundMessage").show();
        } else {
            $("#notFoundMessage").show();
        }
    }
}


