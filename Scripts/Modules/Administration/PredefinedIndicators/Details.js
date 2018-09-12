var ItemTypeIndicator = function () {
    var self = this;
    self.Id = ko.observable(0);
    self.EffectiveDate = ko.observable(new Date());
    self.ExpirationDate = ko.observable('');
}

function GetField(technicalFields, code) {
    var array = JSLINQ(technicalFields)
        .Where(function (x) { return x.Code() == code; })
        .Select(function (x) { return x; });

    return array.items[0];
}

var technicalField = function (technicalFields) {
    var self = this;
    self.MDF = GetField(technicalFields, 'MDF');
    self.BASEL = GetField(technicalFields, 'BASEL');
    self.BASELYEAR = GetField(technicalFields, 'BASEL-YEAR');
    self.TARGET = GetField(technicalFields, 'TARGET');
    self.TARGERYEAR = GetField(technicalFields, 'TARGER-YEAR');
    self.ASSUM = GetField(technicalFields, 'ASSUM');
    self.DISSAG = GetField(technicalFields, 'DISSAG');
    self.SOURC = GetField(technicalFields, 'SOURC');
    self.PERIOD = GetField(technicalFields, 'PERIOD');
    self.RATION = GetField(technicalFields, 'RATION');
    self.PRIORAREA = GetField(technicalFields, 'PRIOR-AREA');
    self.LINKAREA = GetField(technicalFields, 'LINK-AREA');
    self.RELATOUTP = GetField(technicalFields, 'RELAT-OUTP');
    self.REGGOAL = GetField(technicalFields, 'REG-GOAL');
    self.OTHERORG = GetField(technicalFields, 'OTHER-ORG');
    self.ORGAN = GetField(technicalFields, 'ORGAN');
}

function ObtenerArray(disaggregations) {
    var array = new Array();
    for (var i = 0; i < disaggregations.length; i++) {
        array[i] = disaggregations[i].Id;
    }

    return array;
}

var viewModel = function (data, indicator) {
    var self = this;
    self.ListDisaggregations = ko.observableArray();
    self.Indicator = data;
    self.currentPageIndex = ko.observable(0);
    self.pageSize = ko.observable(10);
    self.SelectedDisaggregations = ko.observable(ObtenerArray(indicator.Disaggregations));
    self.minDate = new Date();
    self.AddListDisaggregations = function (registros) {
        self.ListDisaggregations(registros);
    }

    self.removeDisaggregation = function (item) {
        self.Indicator.Disaggregations.remove(item);
    }

    self.itemsOnCurrentPage = ko.computed(function () {
        var startIndex = self.pageSize() * self.currentPageIndex();
        return ko.unwrap(self.ListDisaggregations).slice(startIndex, startIndex + self.pageSize());
    }, self);

    self.nextPage = function () {
        if (((self.currentPageIndex() + 1) * self.pageSize()) < self.ListDisaggregations().length) {
            self.currentPageIndex(self.currentPageIndex() + 1);
        }
        else {
            self.currentPageIndex(0);
        }
    }

    self.previousPage = function () {
        if (self.currentPageIndex() > 0) {
            self.currentPageIndex(self.currentPageIndex() - 1);
        }
        else {
            self.currentPageIndex((Math.ceil(self.ListDisaggregations().length / self.pageSize())) - 1);
        }
    }

    self.TechnicalFields = ko.observable(new technicalField(self.Indicator.TechnicalFields()));

    self.ListIndicatorTypes = ko.observableArray();
    self.TypeIndicatorSelected = ko.observable();

    self.AddListIndicatorTypes = function (registros) {
        self.ListIndicatorTypes(registros);
    }

    self.Yesno = ko.observableArray();
    self.Countries = ko.observableArray(indicator.Countries);
    self.Sectors = ko.observableArray(indicator.Sectors);

    self.Paises = ko.observableArray();

    self.AddCountry = function () {
        self.Indicator.Countries.push(new ItemTypeIndicator());
    }
    self.RemoveCountry = function (item) {
        self.Indicator.Countries.remove(item);
    }

    self.AddSector = function () {
        self.Indicator.Sectors.push(new ItemTypeIndicator());
    }
    self.RemoveSector = function (item) {
        self.Indicator.Sectors.remove(item);
    }
    self.CountriesEdit = ko.observableArray();
    self.SectorsEdit = ko.observableArray();
    self.MinDate = new Date();

    self.ResultsFrameWorkSubType = ko.observableArray();
    self.AddResultsFrameWorkSubType = function (registros) {
        self.ResultsFrameWorkSubType(registros);
    }

    self.CanEditTypeIndicator = ko.computed(function () {
        if (self.Indicator.PredefinedIndicatorId() != 0) {
            return false;
        }

        return true;
    });

    self.VisibleRF = ko.computed(function () {
        if (self.Indicator.PredefinedIndicatorId() != 0) {
            return (ko.mapping.toJS(self.Indicator.TypeIndicator) == 'Results Framework');
        }

        return (ko.mapping.toJS(self.Indicator.TypeText) == 'Results Framework');
    }, self);

    self.VisibleCountry = ko.computed(function () {
        if (self.Indicator.PredefinedIndicatorId() != 0) {
            return (ko.mapping.toJS(self.Indicator.TypeIndicator) == 'Country');
        }

        return (ko.mapping.toJS(self.Indicator.TypeText) == 'Country');
    }, self);

    self.VisibleSector = ko.computed(function () {
        if (self.Indicator.PredefinedIndicatorId() != 0) {
            return (ko.mapping.toJS(self.Indicator.TypeIndicator) == 'Sector');
        }

        return (ko.mapping.toJS(self.Indicator.TypeText) == 'Sector');
    }, self);

    self.Prueba = ko.observable().extend({ required: true });
}

$(document).ready(function () {

    // Get Data of indicator detail
    $.ajax({
        url: $('#hdnpostUriAction').val(),
        data: JSON.stringify({ predefinedIndicatorId: $('#hdnDetail').val() }),
        dataType: "json",
        contentType: "application/json",
        type: 'post',
        success: function (data) {
            window.vm = new viewModel(ko.mapping.fromJS(data), data);

            $.ajax({
                url: $('#hdnpostUriGetTypeIndicators').val(),
                dataType: "json",
                contentType: "application/json",
                type: 'post',
                success: function (registros) {
                    vm.AddListIndicatorTypes(registros);
                }
            });

            $.ajax({
                url: $('#hdnpostUriGetResultsFrameWorkSubType').val(),
                dataType: "json",
                contentType: "application/json",
                type: 'post',
                success: function (registros) {
                    vm.AddResultsFrameWorkSubType(registros);
                }
            });

            if (vm.Sectors.length == 0) {
                $.ajax({
                    url: $('#hdnpostUriSectors').val(),
                    dataType: "json",
                    contentType: "application/json",
                    type: 'post',
                    success: function (data) {
                        vm.Sectors(data);
                    }
                });
            }

            if (vm.Countries.length == 0) {
                $.ajax({
                    url: $('#hdnpostUriCountries').val(),
                    dataType: "json",
                    contentType: "application/json",
                    type: 'post',
                    success: function (countries) {
                        vm.Countries(countries);
                        var arrayYesno = [{ NameEn: $("#hdnYes").val(), Value: $("#hdnYes").val() }, { NameEn: $("#hdnNo").val(), Value: $("#hdnNo").val() }]

                        vm.Yesno(arrayYesno);
                    }
                });
            }

            if (vm.Indicator.EffectiveDate() == null) {
                vm.Indicator.EffectiveDate(new Date());
            }

            AsignarCamposRequeridos();
            ko.applyBindings(vm);

        }
    });

    // Save data of indicator detail
    $(".btnSave").click(function (e) {
        if (ValidateSubmit()) {
            $.ajax({
                url: $('#hdnpostUriActionSave').val(),
                data: ko.mapping.toJSON(vm.Indicator),
                dataType: "json",
                contentType: "application/json",
                type: 'post',
                success: function (data) {
                    var result = data;
                    if (result.toUpperCase().indexOf('ERROR') == -1) {
                        document.location.href = $("#hdnpostUriIndex").val();
                    }
                }
            });
        }
    });

    // Get list of disaggregations
    $("#btnNewDisaggregation").click(function (e) {
        if (vm.ListDisaggregations._latestValue == null || vm.ListDisaggregations._latestValue.length == 0) {
            $.ajax({
                url: $('#hdnpostUriGetDisaggregations').val(),
                dataType: "json",
                contentType: "application/json",
                type: 'post',
                success: function (data) {
                    vm.AddListDisaggregations(data);
                }
            });
        }

        vm.SelectedDisaggregations(ObtenerArray(ko.mapping.toJS(vm.Indicator.Disaggregations())));
        //vm.SelectedDisaggregations(vm.Indicator.Disaggregations().slice(0));
        $("#myModal").data("kendoWindow").open();
    });

    $("#btnAcceptDisaggregation").click(function (e) {
        var disaggregations = new Array();
        var contador = 0;

        for (var i = 0; i < vm.SelectedDisaggregations().length; i++) {
            for (var j = 0; j < vm.ListDisaggregations().length; j++) {
                if (vm.ListDisaggregations()[j].Id == vm.SelectedDisaggregations()[i]) {
                    disaggregations[contador] = vm.ListDisaggregations()[j];
                    contador++;
                }
            }
        }

        vm.Indicator.Disaggregations(disaggregations);
        $("#myModal").data("kendoWindow").close();
    });

    // Evento Click of button search
    $("#btnNewIndicator").click(function (e) {
        $("#notFoundMessage").hide();
        e.preventDefault();
        document.location.href = $(this).data("detail-path") + '/?PredefinedIndicatorId=';
    });

    ko.validation.rules.pattern.message = 'Invalid.';

    ko.validation.configure({
        registerExtenders: true,
        messagesOnModified: true,
        insertMessages: true,
        parseInputAttributes: true,
        messageTemplate: null
    });

    ko.validation.rules['combo'] = {
        validator: function (val, validate) {
            return val != validate && val != 0 && val != '0';

        },
        message: 'Select a item.'
    }


    ko.validation.rules['rangeDates'] = {
        validator: function (val, validate) {
            if (validate() != '' && val != null) {
                return new Date(val) <= new Date(validate());
            }
            else {
                return false;
            }

        },
        message: 'Effective date must be less than Expiration Date.'
    }
    ko.validation.registerExtenders();

    function ValidateSubmit() {
        //showNotification({ message: "", type: "Error", autoClose: true, duration: 100 });
        // vm.Indicator.IndicatorNumber.isValid
        return true;
    }

    function AsignarCamposRequeridos() {
        vm.Indicator.IndicatorNumber.extend({ required: true });
        vm.Indicator.NameEn.extend({ required: true });
        vm.Indicator.UnitOfMeasureEn.extend({ required: true });
        vm.Indicator.UnitOfMeasureEs.extend({ required: true });
        vm.Indicator.UnitOfMeasurePt.extend({ required: true });
        vm.Indicator.UnitOfMeasureFr.extend({ required: true });
        vm.Indicator.NameEs.extend({ required: true });
        vm.Indicator.NamePt.extend({ required: true });
        vm.Indicator.NameFr.extend({ required: true });
        vm.Indicator.PriorityAreaId.extend({ notEqual: 0 });
        vm.Indicator.EffectiveDate.extend({
            required:true,
            rangeDates: function () {
                if (vm.Indicator.ExpirationDate() != null) {
                    return vm.Indicator.ExpirationDate();
                }
                return '';
            }
        });
        vm.Indicator.ExpirationDate.extend({ required: true });
    }

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
});

