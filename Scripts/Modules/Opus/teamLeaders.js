function checkForNonStaffUsersAsTeamLeaders(
    dropDownElement, userName, fullName, teamLeaderRoles, userNotFromStaffMessage, urlAction)
{
    if (userName === null || userName === "")
        return false;

    var currentRoleId = dropDownElement.GetValue();

    if (teamLeaderRoles.indexOf(currentRoleId) < 0)
        return false;

    postUrlWithOptions(
        urlAction,
        { async: false },
        { userName: userName }).done(function (data) {

            if (!data.IsValid) {
                dropDownElement.FirstorDefault();

                showMessage(data.ErrorMessage);

                return false;
            }


            if (!data.HasCondition) {
                dropDownElement.FirstorDefault();

                showMessage(userNotFromStaffMessage.replace("{userName}", fullName));

                return true;
            }

            return false;
        });
}
