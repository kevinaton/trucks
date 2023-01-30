'use strict';
(function () {
    $(function () {

        var _driverApplicationService = abp.services.app.driverApplication;

        $('button').click(function(e) {
            e.preventDefault();
            var location = {};
            abp.helper.getLocation(function (locationResult) {
                if (locationResult) {
                    location.latitude = locationResult.coords.latitude;
                    location.longitude = locationResult.coords.longitude;
                }
                _driverApplicationService.createEmployeeTime(location).done(function () {
                    window.location.reload(true);
                });
            });
        });

    });
})();