function findNewOutcomeOrImpactSection() {
    var hasValue = true;

    $("[name$=Statement][type=text]").each(function(index, item) {
        if($(item).val() == "") {
            hasValue = false;
            return false;
        }
    });
    
    return hasValue;
}