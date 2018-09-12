    $(document).ready(function () {
        deletemodal__();
        
        var grid = $("#grillaContact").kendoGrid({
            dataSource: {              
                pageSize: 25,
                schema: {
                    model: {
                        fields: {
                            PERSON_TO: { type: "string" },
                            PERSON_TITLE: { type: "string" },
                            ACTIVE: { type: "string" },
                            INSTITUTION_NAME: { type: "string" },
                            
                        }
                    }
                },
               
            },
            scrollable: false,
            sortable: {
                allowUnsort: false
            },
            selectable: false,
            filterable: false,
            pageable: true,
            info: false,
            previousNext: false,
            resizable: true,
            allowUnsort: false,
            messages: {
                display: "",
                first: "",
                previous: "",
                next: "",
                last: "",
                refresh: ""
            },
            columns: [{
                field: "PERSON_TO",         
                width: 150
            }, {
                field: "PERSON_TITLE",
            }, {
                field: "Active",
            }, {
                field: "INSTITUTION_NAME"
            },
            {
                field: "Contract"
            },
            {
                field: "Operation"
            },
            {
                field: "Action",
                width: 120
            }
            ],
            dataBound: function (e) {
                $(".k-pager-nav").hide();
                $(".k-pager-info").hide();
                $(".k-pager-first").hide();
                $(".k-pager-last").hide();
            }
        }).data("kendoGrid");

        //Valida los campos obligatorios

        $('.SaveAddContact').click(function (e) {

            e.preventDefault();

            control = 0;
                   
            var id = $("#cmbusernames").val();

            var name = $.trim($("#ExecutorContactModel_PERSON_TO").val());
            var title = $.trim($("#ExecutorContactModel_PERSON_TITLE").val());
            var addres = $.trim($("#ExecutorContactModel_ADDRES").val());
            var phone = $.trim($("#ExecutorContactModel_PHONE").val());
            var fax = $.trim($("#ExecutorContactModel_FAX").val());
            var email = $.trim($("#ExecutorContactModel_EMAIL").val());
          
            var page = $("#page").val();
            var institutionEdit = $.trim($("#institutionEdit").val())
            var intitutionNewEdit = $.trim($("#txtName").val())
            
            if (name.length < 1) {
                $("#nameError").css("display", "inline-block");
                control++;
            } else { $("#nameError").css("display", "none"); }

            if (title.length < 1) {
                $("#titleError").css("display", "inline-block");
                control++;
            } else { $("#titleError").css("display", "none"); }

            if (addres.length < 1) {
                $("#addresError").css("display", "inline-block");
                control++;
            } else { $("#addresError").css("display", "none"); }

            if (phone.length < 1) {
                $("#phoneError").css("display", "inline-block");
                control++;
            } else { $("#phoneError").css("display", "none"); }

            if (fax.length < 1) {
                $("#faxError").css("display", "inline-block");
                control++;
            } else { $("#faxError").css("display", "none"); }

            if (email.length < 1) {
                $("#emailError").css("display", "inline-block");
                control++;
            } else { $("#emailError").css("display", "none"); }

            if (page == "edit") {
                if (institutionEdit == intitutionNewEdit) {
                    $("#institutionError").css("display", "none");
                    id = 1;
                } else {
                    if (id == null) {
                        $("#institutionError").css("display", "inline-block");
                        control++;
                        $("#institutionnotfound").css("display", "none");
                    } else {
                        $("#institutionError").css("display", "none");
                        var idSelect = $("#cmbusernames option:selected").attr("id")
                     
                        $("#ExecutorContactModel_INSTITUTION_ACRONYM").val(idSelect).val();
                    }
                }}

            if (page == "add") {
               

                var idSelect = $("#cmbusernames option:selected").attr("id")               
                if (idSelect == null) {
                        $("#institutionError").css("display", "inline-block");
                        control++;
                        $("#institutionnotfound").css("display", "none");
                } else {
                    $("#institutionError").css("display", "none");
                 
                    $("#ExecutorContactModel_INSTITUTION_ACRONYM").val(idSelect).val();
                }
                }

                if (control == 0)
                {
                    idbg.lockUi(null, true);
                    $("#ExecutorContactAdd").submit();              
                }
              
            
        });

        $("#clearBtn").click(function () {
            
            $("#institution").val('');
            $("#operation").val('');
            $("#contract").val('');
            $("#chkactivo").removeAttr('checked');
            $("#idStatus").val(0);
        });

        $("#cmbusernames").change(function () {
           var selectedText = $("#cmbusernames option:selected").first().val();
           if (selectedText) {
              $("#txtName").val(selectedText);
           }
           $(this).addClass('hide');
        });

        $(document).on('click', function () {

            if ($("#cmbusernames").is(':visible')) {
                $("#cmbusernames").hide();
            }
        });

        $("#txtName").attr('autocomplete', 'off');
    });

    function deletemodal__() {
        try {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
            $(".k-window-titlebar").remove();
        } catch (Exception) { }
    }

    $(document).on('click', ".ac-DeleteGetExecutorContact", function () {
       
        event.preventDefault();
        var executorContactId = $("#contactID").val();
        var title = $(this).data('windowTitle');
        var warningRoute = $(this).data('routeDeleteWarning');
        var actionRoute = $(this).data("route") + "?Contact_ID=" + executorContactId;

        deleteWarning(executorContactId, title, warningRoute, actionRoute);
    });
   


    $(document).on('click', ".ac-DeleteExecutorContact", function () {
        
        event.preventDefault();
        var executorContactId = $(this).data('version-id');
        var title = $(this).data('windowTitle');
        var warningRoute = $(this).data('routeDeleteWarning');
        var actionRoute = $(this).data("route") + "?Contact_ID=" + executorContactId;
      

        deleteWarning(executorContactId, title, warningRoute, actionRoute);
    });


    var deleteWarning = function (executorContactId, title, warningRoute, actionRoute) {
      
        
        event.preventDefault();
        var messageDelete =  $("#messageDelete").text();
        $(window.parent.document).find('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $('body').append('<div class="ui-widget-overlay ui-front"></div>');
        $("body").append(
                          '<div class="dinamicModal"><div style="padding: 20px;">' + messageDelete + '</div><div class="pie pieReassign"><div class="botones"> ' +
                             '<a title="Cancel" class="cancel" id="CancelWarningDialog" onclick="removemodal();" ' +
                            'href="javascript:void(0)">Cancel</a><label for="cancel">' +
                            '<a title="Delete" class="button blueButton" id="DeleteExecutorContact"' +
                            'href="'+ actionRoute +'">Delete</a><label for="delete"></div></div></div>');
                          
        var modal = $(".dinamicModal").kendoWindow({
            width: "800px",
            position: { top: 100 },
            title: title,
            draggable: false,
            resizable: false,
            pinned: false,
          // content: warningRoute,
            actions: ["Close"],
            modal: true,
            visible: false,
            close: function () {
                $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
                $("body").find(".ui-widget-overlay").remove();
                $(".ui-widget-overlay").remove();
                $(".k-window").remove();
            }
        }).data("kendoWindow");
        $(".k-window-titlebar").addClass("warning");
        $(".k-window-title").addClass("ico_warning");
        modal.open();
    };

    function removemodal() {
        
        $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
        $(".ui-widget-overlay").remove();
        $(".k-window").remove();
    }

    function DeleteExecutorContact(executorContactId) {
        

        $("#formExecutorContact").submit();

    };

    $(document).on('click', ".ac-EditContact", function () {
        event.preventDefault();
        
        var executorContactId = $(this).data('versionId');
        var actionRoute = $(this).data("route");
        location.href = actionRoute;
       
    });

    function deletemodal__() {
        try {
            $(window.parent.document).find('body').find(".ui-widget-overlay").remove();
            $(".ui-widget-overlay").remove();
            $(".k-window").remove();
            $(".k-window-titlebar").remove();
        } catch (Exception) { }
    }

    function fillCombo() {
        
        $.getJSON("../ExecutorContact/GetInstitution" + "?institution=" + $("#txtName").val(), null,
                
            function (data) {
                $("#cmbusernames").empty();
                $(".messages").remove();
                  
                var cont = 0;
                $.each(data, function (i, item) {
                        
                    $("#divcbn").fadeIn(1000);

                    $("#cmbusernames").append("<option id='" + item.ACRNM + "'>" + item.NM.toUpperCase() + "</option>").css('height', '80px');
                    cont++;
                });
                if (cont == 0) {
                    $("#cmbusernames").addClass("hide").hide();
                    $("#institutionnotfound").css("display", "inline-block");
                    $("#institutionError").css("display", "none");
                }
                else {
                    $("#cmbusernames").removeClass("hide").show();
                    $("#institutionnotfound").css("display", "none");
                    $("#institutionError").css("display", "none");
                }
            }
        );
    };

    $('.yearSelector').on('click', function () {
    
      var route = $('.contact-row').attr("data-route");
      location.href = route;
    }
  );