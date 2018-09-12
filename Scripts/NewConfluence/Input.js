function focusDropDowns(element) {
    if (!element.is('li')) {
        element = element.closest('li');
    }

    if (element.closest('ul.dropdown-menu').css('display') === "none") {
        setTimeout(function () {
            focusDropDowns($('ul.dropdown-menu:visible').find('[dd-selected]').closest('li'));
        }, 100);
    }

    element.closest('ul.dropdown-menu').find('[dd-selected]').removeAttr('dd-selected');
    element.find('a').attr('dd-selected', '');
    element.closest('ul.dropdown-menu').scrollTop(element.index() * element.outerHeight());
}

function cargaDropDown(element) {

    $(element).closest('.dropdown').find('.dropdown-menu').css('display', '');

    var list = $($(element).closest('div.dropdown').find('ul.dropdown-menu'));

    $('div.dropdown').find('ul.dropdown-menu').not(list).hide();

    var value = $(element).closest('div.dropdown').find('input.hide').val();
    if (element === "") {
        $(element).FirstorDefault();
    } else {
        focusDropDowns($(element).closest('div.dropdown').find('ul.dropdown-menu').find('[dd-value="' + value + '"]'));
    }

    var width = $(element).closest('div.dropdown').css('width');
    $(element).closest('div.dropdown').find('ul.dropdown-menu').css('min-width', width);

    $(element).click(function () {
        var width = $(element).closest('div.dropdown').css('width');
        $(element).closest('div.dropdown').find('ul.dropdown-menu').css('min-width', width);
    });

    $(element).closest('div.dropdown').find('ul.dropdown-menu a[dd-value]').hover(function () {
        $(this).closest('ul.dropdown-menu').find('[dd-selected]').removeAttr('dd-selected');
        $(this).attr('dd-selected', '');
    });

    $($(element).closest('div.dropdown').find('ul.dropdown-menu a[dd-value]')).off("click");
    $($(element).closest('div.dropdown').find('ul.dropdown-menu a[dd-value]')).on("click", function () {
        if (!$(this).is('[disabled]')) {
            if ($(this).closest('div.dropdown').find('ul.dropdown-menu').find('a[dd-selected]').length > 0) {
                $(this).closest('div.dropdown').find('ul.dropdown-menu').find('a[dd-selected]').removeAttr('dd-selected');
            }
            $(this).closest('div.dropdown').find('button .valueText').text($(this).text());
            $(this).closest('div.dropdown').find('input').val($(this).attr('dd-value'));
            $(this).closest('div.dropdown').find('input').attr('value', $(this).attr('dd-value'));
            $(this).closest('div.dropdown').find('input').trigger('change');
            $(this).attr('dd-selected', '');

            if ($(this).closest('div.dropdown').find('ul.filled').length > 0) {
                $(this).closest('div.dropdown').find('ul.filled').removeClass('filled');
            }

            if ($(this).attr('dd-value').length === 0) {
                if ($(this).closest('div.dropdown').find('input.hide').is('[data-parsley-required]')) {
                    $(this).closest('div.dropdown').find('ul.parsley-errors-list').addClass('filled');
                    if ($(this).closest('div.dropdown').find('ul.filled li.parsley-required').length === 0) {
                        $(this).closest('div.dropdown').find('ul.filled').append('<li class="parsley-required">This value is required.</li>');
                    }
                }
            }

            if ($(this).GetValue() === "") {
                $(this).closest('div.dropdown').addClass('placeholder');
            } else {
                $(this).closest('div.dropdown').removeClass('placeholder');
            }
        }
    });
}

function dropdownNavigation(element, event) {
    var elementos = $(element).closest('div.dropdown').find('[dd-selected]').closest('ul.dropdown-menu');
    var elementValor = elementos.find('[dd-selected]');
    switch (event.keyCode) {
        case 40:
            if (elementos.find('li').length - 1 > elementValor.closest('li').index()) {
                focusDropDowns(elementValor.closest('li').next());
            }
            break;
        case 38:
            if (elementValor.closest('li').index() > 0) {
                focusDropDowns(elementValor.closest('li').prev());
            }
            break;
        case 13:
            $(element).closest('div.dropdown').find('[dd-selected]').click();
            $(element).closest('div.dropdown').find('button').click();
            break;
        default:
            var valor = $(element).attr('dd-find');
            var letter = String.fromCharCode(event.keyCode).toUpperCase();
            if (valor.length > 0) {
                valor = valor + letter;
                $(element).attr('dd-find', valor);
            } else {
                valor = letter;
                $(element).attr('dd-find', letter);
            }

            var array = new Array();
            elementos.find('li a').each(function () {
                if ($(this).html().length > 0) {
                    var contenido = $(this).html().substr(0, valor.length).toUpperCase();
                    if (contenido === valor) {
                        array.push($(this));
                    }
                }
            });

            if (array.length === 0) {
                $(element).attr('dd-find', '');
            }

            if (array.length === 1) {
                focusDropDowns(array[0]);
            }

            if (array.length > 1) {
                var seleccionado = false;
                var banderaIndex;
                for (var i = 0; i < array.length; i++) {
                    if ($(array[i]).is('[dd-selected]')) {
                        banderaIndex = i;
                        seleccionado = true;
                    }
                }

                if (!seleccionado) {
                    focusDropDowns(array[0]);
                } else {
                    if (array.length - 1 === banderaIndex) {
                        focusDropDowns(array[0]);
                    } else {
                        focusDropDowns(array[banderaIndex + 1]);
                    }
                }
            }
            break;
    }
}

function loadDropDownAjax(element) {
    if ($(element).closest('div.inputSearch').find('input[name*="_text"]').val().length >= $(element).attr('dd-minchar') * 1) {
        var values = new Object();
        var parameters;
        if ($(element).children("span#idGuaranteedType").length > 0) {
            parameters = {
                filter: $(element).closest('div.inputSearch').find('[data-search-ur]').val(),
                lendingType: $("[name='LendingType']").val()
            }
        } else {
            parameters = {
                filter: $(element).closest('div.inputSearch').find('[data-search-ur]').val()
            }
        }
        var url = $(element).closest('div.inputSearch').find('[data-search-ur]').attr('data-search-ur');
        var dependence = $(element).attr("dd-dependence");

        if (dependence !== "" && dependence != undefined) {
            if (url.indexOf('?') > -1) {
                url += '&';
            } else {
                url += '?';
            }

            var params = '';
            var name = dependence.split(';');
            var i = 0;
            var pos = -1;

            name.forEach(function (entry) {
                var obj = jQuery.parseJSON(entry);
                if (obj['BasicFilter'] != undefined) {
                    params += "param" + i + '=' +
                        ($(obj.BasicFilter.basic).GetValue() == null ? "" : $(obj.BasicFilter.basic).GetValue()) + "&";
                } else {
                    pos = i;
                }
                i++;
            });

            url += params;

            if (pos != -1) {
                var obj = jQuery.parseJSON(name[pos]);
                var controlElm
                if (obj['ElementFilter'] != undefined) {
                    controlElm = $(element);
                    if (obj.ElementFilter['closest'] != undefined) {
                        controlElm = controlElm.closest(obj.ElementFilter.closest);
                    }

                    if (obj.ElementFilter['find'] != undefined) {
                        controlElm = controlElm.find(obj.ElementFilter.find);
                    }
                }

                if (controlElm != null && controlElm!= undefined) {
                    url += "param" + pos.toString() + "=" +
                        ((controlElm).GetValue() != null ? (controlElm).GetValue() : "");
                }
            }
        }
        showLoaderOptional();
        $.ajax({
            dataType: "json",
            type: "get",
            async: true,
            obj: $(element),
            url: url,
            data: parameters,
            success: function (data) {
                if (data.length === 0) {
                    showMessage("No data to display");
                    hideLoaderOptional();
                    return;
                }
                if (!data.IsValid && data.ErrorMessage != null && data.ErrorMessage !== "") {
                    showMessage(data.ErrorMessage);
                    hideLoaderOptional();
                    return;
                }

                values = data.ListResponse;

                var element = $(this.obj);
                $(element).closest('div.inputSearch').find('ul.dropdown-menu li').remove();

                if ($(element).attr('dd-asc') === "true" && values !== null) {
                    values.sort(sortBy('Text'));
                }
                if (values !== null && values.length > 0) {
                    for (var i = 0; i < values.length; i++) {
                        var imgUrl = "";
                        var additionalData = "";
                        var item = values[i];
                        if (item.PathTemplate != null) {
                            imgUrl = "<img src='" + item.PathTemplate + "' class='img-flag' />";
                        } else {
                            imgUrl = "<img src='/IDB/Images/Contenido/basicdata/DefaultPictureUrl.PNG' class='img-flag' />";
                        }
                        if (item.AdditionalData != null) {
                            additionalData = "additional-data='" + item.AdditionalData + "'";
                        }

                        var txt = '<li><a dd-value="' + item.Value + '" ' + additionalData + '>';

                        if ($(element).closest('div.inputSearch').find('[data-show-photo]').attr("data-show-photo") === "true") {
                            txt += imgUrl + ' ';
                        }
                        txt += item.Text + '</a></li>';

                        $(element).closest('div.inputSearch').find('ul.dropdown-menu').append(txt);
                    }
                } else {
                    if ($(element).is('[dd-clearText]')) {
                        $(element).closest('div.inputSearch').find('input').val('');
                    }

                    $(element).closest('div.inputSearch').find('ul.dropdown-menu').append('<li>No results</li>');
                }

                var width = $(element).closest('div.inputSearch').css('width');
                $(element).closest('div.inputSearch').find('ul.dropdown-menu').css('min-width', width);
                imgFlag();

                $(element).closest('div.inputSearch').find('ul.dropdown-menu a[dd-value]').click(function () {
                    if ($(this).closest('div.inputSearch').find('ul.dropdown-menu').find('a[dd-selected]').length > 0) {
                        $(this).closest('div.inputSearch').find('ul.dropdown-menu').find('a[dd-selected]').removeAttr('dd-selected');
                    }

                    if ($(this).closest('div.pictureLabel').length > 0) {
                        $(this).closest('div.pictureLabel').find('img.img-flag').first().attr("src", $(this).find('img.img-flag').attr("src"));
                    }

                    $(this).closest('div.inputSearch').find('[data-dropdown-type]').val($(this).text());
                    $(this).closest('div.inputSearch').find('input.hidden').val($(this).attr('dd-value'));
                    var aditinalData = $(this).attr('additional-data');
                    $(this).closest('div.inputSearch').find('input.hidden').trigger('change', aditinalData);;
                    $(this).attr('dd-selected', '');

                    if ($(this).closest('div.inputSearch').find('ul.filled').length > 0) {
                        $(this).closest('div.inputSearch').find('ul.filled').removeClass('filled');
                    }

                    if ($(this).attr('dd-value').length === 0) {
                        $(this).closest('div.inputSearch').find('ul.parsley-errors-list').addClass('filled');
                        if ($(this).closest('div.inputSearch').find('ul.filled li.parsley-required').length === 0) {
                            $(this).closest('div.inputSearch').find('ul.filled').append('<li class="parsley-required">This value is required.</li>');
                        }
                    }
                });

                $(element).closest('div.inputSearch').find('ul.dropdown-menu a[dd-value]').hover(function () {
                    $(this).closest('ul.dropdown-menu').find('[dd-selected]').removeAttr('dd-selected');
                    $(this).attr('dd-selected', '');
                });

                if ($(element).closest('div.inputSearch').find('input.hidden').val().length > 0) {
                    var value = $(element).closest('div.inputSearch').find('input.hidden').val();
                    focusDropDowns($(element).closest('div.inputSearch').find('a[dd-value="' + value + '"]'));
                }
                $(element).attr('aria-expanded', 'true');
                $(element).closest('div').addClass('open');
                hideLoaderOptional();
            },
            error: function (jqXhr, textStatus, errorThrown) {
                $('span.ui-helper-hidden-accessible').remove();
                hideLoaderOptional();
                showMessage(errorThrown);
            }
        });
    } else {
        showMessage($(element).attr('dd-minmen') + ' ' + $(element).attr('dd-minchar'));
    }

}

function sortBy(key, reverse) {
    var moveSmaller = reverse ? 1 : -1;
    var moveLarger = reverse ? -1 : 1;
    return function (a, b) {
        if (a[key] < b[key]) {
            return moveSmaller;
        }
        if (a[key] > b[key]) {
            return moveLarger;
        }
        return 0;
    };

}

function dropdownAjaxNavigation(element, event) {
    var elementos = $(element).closest('div.ctlAsyncr').find('ul.dropdown-menu');
    var elementValor = elementos.find('[dd-selected]');
    switch (event.keyCode) {
        case 40:
            if (elementos.find('li').length - 1 > elementValor.closest('li').index()) {
                if (elementValor.closest('li').index() === -1) {
                    focusDropDowns(elementos.find('a').first());
                } else {
                    focusDropDowns(elementValor.closest('li').next());
                }
            }
            break;
        case 38:
            if (elementValor.closest('li').index() > 0) {
                focusDropDowns(elementValor.closest('li').prev());
            }
            break;
        case 13:
            $(element).closest('div.ctlAsyncr').find('input').first().val(elementValor.text());
            $(element).closest('div.ctlAsyncr').find('input.hidden').val(elementValor.attr('dd-value'));
            var aditinalData = elementValor.attr('additional-data');
            $(element).closest('div.ctlAsyncr').find('input.hidden').trigger('change', aditinalData);
            break;
        default:
            var letter = String.fromCharCode(event.keyCode).toUpperCase();
            var array = new Array();
            elementos.find('li a').each(function () {
                if ($(this).html().length > 0) {
                    var contenido;
                    if ($(this).closest('div.ctlAsyncr').find('input').first().is('[data-show-photo="true"]')) {
                        contenido = $(this).html().substring($(this).html().indexOf('">') + 3)[0].toUpperCase();
                        if (contenido === letter) {
                            array.push($(this));
                        }
                    } else {
                        contenido = $(this).html()[0].toUpperCase();
                        if (contenido === letter) {
                            array.push($(this));
                        }
                    }
                }
            });

            if (array.length === 1) {
                focusDropDowns(array[0]);
            }

            if (array.length > 1) {
                var seleccionado = false;
                var banderaIndex;
                for (var i = 0; i < array.length; i++) {
                    if ($(array[i]).is('[dd-selected]')) {
                        banderaIndex = i;
                        seleccionado = true;
                    }
                }

                if (!seleccionado) {
                    focusDropDowns(array[0]);
                } else {
                    if (array.length - 1 === banderaIndex) {
                        focusDropDowns(array[0]);
                    } else {
                        focusDropDowns(array[banderaIndex + 1]);
                    }
                }
            }
            break;
    }
}

function cleanDropDownAjaxAll(element) {
    if ($(element).attr('dd-reset-click') === "true") {
        if ($(element).closest('div.inputSearch').find('input.hidden').val() !== "") {
            $(element).closest('div.inputSearch').find('input.hidden').val("");
            $(element).closest('div.inputSearch').find('input.hidden').trigger('change');
            $(element).val("");
        }
    }
}

function cleanDropDownAjax(element, event) {
    $(element).closest('div.inputSearch').find('input.hidden').val("");
    if (event.keyCode === 13) {
        $(element).closest('div.inputSearch').find('button').click();
        event.stopPropagation();
        event.stopImmediatePropagation();
        event.preventDefault();
    }
}

function dropdownAutoComplete(element, event) {
    var elementos = $(element).closest('div.ctlComplete').find('ul.dropdown-menu');
    var elementValor = elementos.find('[dd-selected]');
    var find = $(element).val().toUpperCase();
    var seleccion = elementos.find('[dd-text*="' + find + '"]');

    if ($(element).closest('div.ctlComplete').find('input.hidden').val().length > 0) {
        var value = $(element).closest('div.ctlComplete').find('input.hidden').val();
        $(element).closest('div.ctlComplete').find('a[dd-value="' + value + '"]').attr('dd-selected', '');
    }
    var index = elementos.find('li').not('li.hidden').find('[dd-selected]').closest('li').index();

    switch (event.keyCode) {
        case 40:
            if (seleccion.length > elementValor.closest('li').index()) {
                if (elementValor.closest('li').index() === -1) {
                    focusDropDowns(seleccion.first());
                } else {
                    index += 1;
                    if (seleccion.length > index) {
                        focusDropDowns($(elementos.find('li').not('li.hidden')[index]));
                    }
                }
            }
            if (seleccion.length === 1) {
                focusDropDowns($(elementos.find('li').not('li.hidden')[0]));
            }
            return false;
        case 38:
            index -= 1;
            if (seleccion.length > 0 && index > -1 && seleccion.length !== 1) {
                focusDropDowns($(elementos.find('li').not('li.hidden')[index]));
            }

            if (seleccion.length === 2 && index === 1) {
                focusDropDowns($(elementos.find('li').not('li.hidden')[index]));
            }
            return false;
        case 13:
            if (seleccion.length === 1) {
                $(element).closest('div.ctlComplete').find('input').first().val(seleccion.text());
                $(element).closest('div.ctlComplete').find('input.hidden').val(seleccion.attr('dd-value'));
                $(element).closest('div.ctlComplete').find('input.hidden').trigger('change');
                $(element).closest('div.ctlComplete').removeClass('open');
            } else {
                if (elementValor.length > 0) {
                    $(element).closest('div.ctlComplete').find('input').first().val(elementValor.text());
                    $(element).closest('div.ctlComplete').find('input.hidden').val(elementValor.attr('dd-value'));
                    $(element).closest('div.ctlComplete').find('input.hidden').trigger('change');
                    $(element).closest('div.ctlComplete').removeClass('open');
                }
            }
            return false;
        default:
            $(element).closest('div.ctlComplete').find('input.hidden').val("");

            if (seleccion.length === 0) {
                $(element).closest('div.ctlComplete').removeClass('open');
            }

            elementos.find('li').addClass('hidden');
            seleccion.closest('li').removeClass('hidden');
            if (seleccion.length > 0) {
                $(element).closest('div.ctlComplete').addClass('open');
            }
            break;
    }

    $(element).closest('div.ctlComplete').find('ul.dropdown-menu a[dd-value]').hover(function () {
        $(this).closest('ul.dropdown-menu').find('[dd-selected]').removeAttr('dd-selected');
        $(this).attr('dd-selected', '');
    });

    elementos.find('li a[dd-value]').click(function () {
        $(this).closest('div.ctlComplete').find('input').first().val($(this).text());
        $(this).closest('div.ctlComplete').find('input.hidden').val($(this).attr('dd-value'));
        $(this).closest('div.ctlComplete').find('input.hidden').trigger('change');
        $(this).closest('div.ctlComplete').removeClass('open');
    });

    $(document).not($(element).closest('div.ctlComplete.open')).click(function () {
        $('div.ctlComplete.open').removeClass('open');
    });
    return true;
}

function dropdownAutoCompleteCursor(element, event) {
    switch (event.keyCode) {
        case 40:
            event.preventDefault();
            focusDropDowns($(this));
            break;
        case 38:
            event.preventDefault();
            focusDropDowns($(this));
            break;
    }
}

function multiselectlist(field) {
    var c;
    for (var i = 0; i < field.options.length; i++) {
        (c = field.options[i]).className = c.selected ? 'selected' : '';
    }
}

function buttonGraphValue(element) {
    var hideId = $(element).closest('div.btn-group').find('button').not(element).attr('data-id');
    var showId = $(element).attr('data-id');

    $('div[data-id="' + hideId + '"]').addClass('hide');
    $('div[data-id="' + showId + '"]').removeClass('hide');

    $(element).closest('div.btn-group').find('button').removeClass('active');
    $(element).addClass('active');
}

function uploadFile(element) {
    $(element).change(function () {
        var file = $(this).val().split("\\");
        if (file !== 'undefined') {
            $(this).closest('.uploadFile').find('input[type="text"]').val(file[file.length - 1]);
        }
    });
}

function dropdownShowList(element, event) {

    var elementos = $(element).closest('div.ctlComplete').find('ul.dropdown-menu');
    var elementValor = elementos.find('[dd-selected]');
    var find = $(element).val().toUpperCase();
    var seleccion = elementos.find('[dd-text*="' + find + '"]');

    $(element).closest('div.ctlComplete').find('li').addClass('hidden');
    $(element).closest('div.ctlComplete').removeClass('open');

    if ($(element).closest('div.ctlComplete').find('input.hidden').val().length > 0) {
        var value = $(element).closest('div.ctlComplete').find('input.hidden').val();
        $(element).closest('div.ctlComplete').find('a[dd-value="' + value + '"]').attr('dd-selected', '');
    }
    var index = elementos.find('li').not('li.hidden').find('[dd-selected]').closest('li').index();


    switch (event.keyCode) {
        case 40:
            if (seleccion.length > elementValor.closest('li').index()) {
                if (elementValor.closest('li').index() === -1) {
                    focusDropDowns(seleccion.first());
                } else {
                    index += 1;
                    if (seleccion.length > index) {
                        focusDropDowns($(elementos.find('li').not('li.hidden')[index]));
                    }
                }
            }
            if (seleccion.length === 1) {
                focusDropDowns($(elementos.find('li').not('li.hidden')[0]));
            }
            return false;
        case 38:
            index -= 1;
            if (seleccion.length > 0 && index > -1 && seleccion.length !== 1) {
                focusDropDowns($(elementos.find('li').not('li.hidden')[index]));
            }

            if (seleccion.length === 2 && index === 1) {
                focusDropDowns($(elementos.find('li').not('li.hidden')[index]));
            }
            return false;
        case 13:
            if (seleccion.length === 1) {
                $(element).closest('div.ctlComplete').find('input').first().val(seleccion.text());
                $(element).closest('div.ctlComplete').find('input.hidden').val(seleccion.attr('dd-value'));
                $(element).closest('div.ctlComplete').find('input.hidden').trigger('change');
                $(element).closest('div.ctlComplete').removeClass('open');
            } else {
                if (elementValor.length > 0) {
                    $(element).closest('div.ctlComplete').find('input').first().val(elementValor.text());
                    $(element).closest('div.ctlComplete').find('input.hidden').val(elementValor.attr('dd-value'));
                    $(element).closest('div.ctlComplete').find('input.hidden').trigger('change');
                    $(element).closest('div.ctlComplete').removeClass('open');
                }
            }
            return false;
        default:
            $(element).closest('div.ctlComplete').find('input.hidden').val("");

            if (seleccion.length > 0) {
                setTimeout(function () {
                    seleccion.closest('li').removeClass('hidden');
                    $(element).closest('div.ctlComplete').addClass('open');
                }, 200)

            } else {
                setTimeout(function () {
                    $(element).closest('div.ctlComplete').find('li').removeClass('hidden');
                    $(element).closest('div.ctlComplete').addClass('open');
                }, 200)

            }
            break;
    }

    $(element).closest('div.ctlComplete').find('ul.dropdown-menu a[dd-value]').hover(function () {
        $(this).closest('ul.dropdown-menu').find('[dd-selected]').removeAttr('dd-selected');
        $(this).attr('dd-selected', '');
    });

    elementos.find('li a[dd-value]').click(function () {
        $(this).closest('div.ctlComplete').find('input').first().val($(this).text());
        $(this).closest('div.ctlComplete').find('input.hidden').val($(this).attr('dd-value'));
        $(this).closest('div.ctlComplete').find('input.hidden').trigger('change');
        $(this).closest('div.ctlComplete').removeClass('open');
    });

    $(document).not($(element).closest('div.ctlComplete.open')).click(function () {
        $('div.ctlComplete.open').removeClass('open');
    });

    return true;
}