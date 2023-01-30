var app = app || {};
(function () {

    $.extend(app, {
        consts: {
            maxProfilPictureBytesUserFriendlyValue: 5,
            grid: {
                defaultPageSize: 10,
                defaultPageSizes: [10, 20, 50, 100, 1000]
            },
            userManagement: {
                defaultAdminUserName: 'admin'
            },
            contentTypes: {
                formUrlencoded: 'application/x-www-form-urlencoded; charset=UTF-8'
            },
            friendshipState: {
                accepted: 1,
                blocked: 2
            },
            maxQuantity: 1000000,
            maxDecimal: 999999999999999,
        }
    });

})();