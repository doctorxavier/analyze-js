$(document).on("ready", function ()
{

    $("#Department").children().addClass("toDelete");
    $("#Department").children().each(function (index, element)
    {
        if ($(element).text() == "INT" ||
            $(element).text() == "IFD" ||
            $(element).text() == "SCL" ||
            $(element).text() == "INE" ||
            $(element).text() == "CSD") {

            $(element).removeClass("toDelete");
        }
    })
    $("#Department").children(".toDelete").remove();

    $("#DepartmentSector").children().addClass("toDelete");
    $("#DepartmentSector").children().each(function (index, element) {
        if ($(element).text() == "INT" ||
            $(element).text() == "IFD" ||
            $(element).text() == "SCL" ||
            $(element).text() == "INE" ||
            $(element).text() == "CSD") {

            $(element).removeClass("toDelete");
        }
    })
    $("#DepartmentSector").children(".toDelete").remove();

    $("#SectorDepartment").children().addClass("toDelete");
    $("#SectorDepartment").children().each(function (index, element) {
        if ($(element).text() == "INT" ||
            $(element).text() == "IFD" ||
            $(element).text() == "SCL" ||
            $(element).text() == "INE" ||
            $(element).text() == "CSD") {

            $(element).removeClass("toDelete");
        }
    })
    $("#SectorDepartment").children(".toDelete").remove();
    ClearDivision();
    
});

function ClearDivision()
{
    $("#Division").children().addClass("toDelete");
    $("#Division").children().each(function (index, element) {
        if (
            $(element).text() == "INE/ENE" ||
            $(element).text() == "INE/TSP" ||
            $(element).text() == "INE/WSA" ||
            $(element).text() == "SCL/EDU" ||
            $(element).text() == "SCL/GDI" ||
            $(element).text() == "SCL/LMK" ||
            $(element).text() == "SCL/SPH" ||
            $(element).text() == "RES" || 
            $(element).text() == "INT" ||
            $(element).text() == "IFD/CMF" ||
            $(element).text() == "IFD/CTI" ||
            $(element).text() == "IFD/FMM" ||
            $(element).text() == "IFD/ICS" ||
            $(element).text() == "IFD" ||
            $(element).text() == "INT/TIN" ||
            $(element).text() == "INT/INL" ||
            $(element).text() == "CSD" ||
            $(element).text() == "CSD/RND" ||
            $(element).text() == "CSD/HUD" ||
            $(element).text() == "CSD/CCS") {
            $(element).removeClass("toDelete");
        }
    })
    $("#Division").children(".toDelete").remove();

}