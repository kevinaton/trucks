'use strict';
(function () {
    $(function () {

        var _driverApplicationService = abp.services.app.driverApplication;

        var $elapsedTime = $('#ElapsedTime');
        var elapsedTimeMoment = moment($elapsedTime.data('value'), 'MM/DD/YYYY LT');

        var intervalId;
        var timerIsStarted = false;
        if ($elapsedTime.data('clockisstarted')) {
            intervalId = startClock();
            timerIsStarted = true;
        }

        var $actionButton = $('button[type="submit"]');

        $('#ClockOutButton').click(function () {
            var $clockOutButton = $('#ClockOutButton');
            abp.ui.setBusy($clockOutButton);
            if (timerIsStarted) {
                _driverApplicationService.setEmployeeTimeEndDateTime().done(function () {
                    clearInterval(intervalId);
                    timerIsStarted = false;
                    $clockOutButton.text('Clock In');
                    $actionButton.attr('disabled', 'disabled');
                }).fail(function () {
                    abp.notify.error('Clock In failed.');
                }).always(function () {
                    abp.ui.clearBusy($clockOutButton);
                });
            } else {
                var location = {};
                abp.helper.getLocation(function (locationResult) {
                    if (locationResult) {
                        location.latitude = locationResult.coords.latitude;
                        location.longitude = locationResult.coords.longitude;
                    }
                    _driverApplicationService.createEmployeeTime(location).done(function () {
                        intervalId = startClock();
                        timerIsStarted = true;
                        $clockOutButton.text('Clock Out');
                        $actionButton.removeAttr('disabled');
                    }).fail(function () {
                        abp.notify.error('Clock Out failed.');
                    }).always(function () {
                        abp.ui.clearBusy($clockOutButton);
                    });
                });
            }
        });

        function startClock() {
            var id = setInterval(function () {
                elapsedTimeMoment.add(1, 'minutes');
                $elapsedTime.text(elapsedTimeMoment.format('H:mm'));
            }, 60 * 1000);
            return id;
        }
    });
})();