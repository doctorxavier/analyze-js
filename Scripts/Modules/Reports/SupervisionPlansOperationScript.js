var Activo = false;
$(document).on("ready", function () {

    $(".labelCheckSup").each(function (index, element)
    {
        $(element).text("");
    });

    $("#FormSupervicionPlanOperationReport").on("submit", function (e) {
        e.preventDefault();

        var route = $('#FormSupervicionPlanOperationReport').attr('action');

        $("#AjaxLoadForSupervisionReport").show()

        $.ajax({
            url: route,
            data: $("#FormSupervicionPlanOperationReport").serialize(),
            type: 'Post',
            success: function (resp) {
                if (resp.indexOf("http") >= 0)
                {
                    $("#frameReportSupervision").attr("src", resp);
                    $("#frameReportSupervision").show();

                }
                else
                {
                    alert(resp);
                }
            },
            error: function (e, err, erro)
            {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function ()
            {
                
                $("#AjaxLoadForSupervisionReport").hide();
            }
        });
    });

    $("#Btn_Clear").on("click", function () {
        $(".MultiSelectCustom option").each(function (index, element) {
            $(element).removeAttr("selected");
        });
    });

    $("#Year").on("change", function ()
    {
        var route = $('#FormSupervisionYear').attr('action');

        $("#AjaxLoadForSupervisionReport").show()
        $("#TableSupervisionVersion tbody tr").remove();

        $.ajax({
            url: route,
            data: $("#FormSupervisionYear").serialize(),
            type: 'Post',
            dataType: "json",
            success: function (resp)
            {
                $(".activeControlSup").attr("disabled", "disabled");
                $(".activeControlSup").addClass("disabledbutton");

                Activo = false
                GenerateCheckRows(resp);
            },
            error: function (e, err, erro) {
                alert('Error: ' + e + ' - ' + err + ' - ' + erro);
            },
            complete: function () {
                $("#AjaxLoadForSupervisionReport").hide();
                Activo = false;
            }
        });

    });

    Date.prototype.getMonthName = function () {
        var monthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun",
                      "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"];
        return monthNames[this.getMonth()];
    }

    $("#Btn_ExportExcel").on("click", function ()
    {
        $("#ExportType").attr("value", "EXCELOPENXML");
        ExportSupervisionReport();
    });

    $("#Btn_ExportPDF").on("click", function () {
        $("#ExportType").attr("value", "PDF");
        ExportSupervisionReport();
    });


});


function changeCheckboxSelected(check)
{
    var state = $(check).hasClass('activeCheck');    

    if (state)
    {
        var state2 = $(check).hasClass('ControlActivo');

        if (state2 && Activo)
        {
            $(check).css("background-position", "0 0px");
            $(check).removeClass("activeCheck");
            $(check).removeClass("ControlActivo");

            $(".activeControlSup").attr("disabled", "disabled");
            $(".activeControlSup").addClass("disabledbutton");

            Activo = false
        }
    }
    else
    {
        var state2 = $(check).hasClass('ControlActivo');

        if (!state2 && !Activo) {
            $(check).css("background-position", "0 -20px");
            $(check).addClass("activeCheck");
            $(check).addClass("ControlActivo");

            $(".activeControlSup").removeAttr("disabled");
            $(".activeControlSup").removeClass("disabledbutton");

            Activo = true;
        }
    }
}

function GenerateCheckRows(resp)
{
    var newHtml = '';
   

    for (var i = 0; i < resp.ListSupervisionPlanVersion.length; i++)
    {   
        var ValidationDate = resp.ListSupervisionPlanVersion[i].ValidationDate;
        
        var IsValidationDate = false; 
        
        var date = null; 
        var month_Name = null;
        var day = null;
        var year = null;

        if (ValidationDate != null && ValidationDate != '')
        {
            var x = [{ "id": 1, "start": ValidationDate }];
            date = new Date(x[0].start.match(/\d+/)[0] * 1);
            month_Name = date.getMonthName();
            day = date.getDate();
            year = date.getUTCFullYear().toString().substring(0, 4);
        }
        
        
        newHtml += '<tr>';
        newHtml += '<td>' + resp.Year + '</td>';

        if (ValidationDate != null && ValidationDate != '') {
            newHtml += '<td>' + day + ' ' + month_Name + ' ' + year + '</td>';
        }
        else
        {
            newHtml += '<td></td>';
        }
        newHtml += '<td>';
        newHtml += '<input class="fancy-checkbox" id="SupervisionVersion_' + resp.ListSupervisionPlanVersion[i].SupervisionPlanVersionID + '_" name="SupervisionVersion[' + resp.ListSupervisionPlanVersion[i].SupervisionPlanVersionID + ']" type="checkbox" value="true"><input name="SupervisionVersion[' + resp.ListSupervisionPlanVersion[i].SupervisionPlanVersionID + ']" type="hidden" value="false">';
        newHtml += '<label class="fancy-label blue bold labelCheckSup" for="SupervisionVersion_' + resp.ListSupervisionPlanVersion[i].SupervisionPlanVersionID + '_" onclick="changeCheckboxSelected(this)"></label>';
        
        newHtml += "</td>";
        newHtml += "</tr>";
    }

    $("#TableSupervisionVersion tbody").append(newHtml);


}

function ExportSupervisionReport()
{
    var Route = $("#UrlPostDownload").attr("data-route");

    $("#AjaxLoadForSupervisionReport").show();

    $.ajax({
        url: Route,
        data: $("#FormSupervicionPlanOperationReport").serialize(),
        type: 'Post',
        success: function (resp) {
            if (resp.indexOf("http") >= 0) {

                parent.location.href = resp;
            }
            else {
                alert(resp);
            }
        },
        error: function (e, err, erro) {
            alert('Error: ' + e + ' - ' + err + ' - ' + erro);
        },
        complete: function () {
            $("#AjaxLoadForSupervisionReport").hide();
        }
    });
}

