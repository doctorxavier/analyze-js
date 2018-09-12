function showPopUpGroup(membersEsg, titlePopUp) {
    if (membersEsg != null) {
        var message = '<b>' + membersEsg.join('</b><br /><b>') + '</b>';
        showMessage(message, '', '', titlePopUp);
    }
}
