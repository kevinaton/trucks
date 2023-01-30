(function () {
    $(function () {
        'use strict';

        var _addTicketPhotoModal = new app.ModalManager({
            viewUrl: abp.appPath + 'App/DriverApplication/AddTicketPhotoModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/App/Views/DriverApplication/_AddTicketPhotoModal.js',
            modalClass: 'AddTicketPhotoModal'
        });

        $('#CreateNewTicketButton').click(function () {
            var createNewTicketOldValue = $('#CreateNewTicket').val() === "True";
            var createNewTicket = !createNewTicketOldValue;
            if (createNewTicket) {
                $('#TicketNumberLabel').removeClass('required-label');
            } else {
                $('#TicketNumberLabel').addClass('required-label');
            }
            $('#TicketNumber').prop('disabled', createNewTicket);
            $('#CreateNewTicket').val(createNewTicket ? "True" : "False");
        });

        window.validateTicketFields = function (ticket) {
            var isQuantityValid = true;

            if (abp.setting.getBoolean('App.DispatchingAndMessaging.RequireDriversToEnterTickets')) {
                if (ticket.Amount === '' || ticket.Amount === null || !(Number(ticket.Amount) > 0)) {
                    isQuantityValid = false;
                }
            }

            if (!isQuantityValid) {
                abp.message.error('Please check the following: \n'
                    + (isQuantityValid ? '' : '"Actual Quantity" - This field is required.\n'), 'Some of the data is invalid');
                return false;
            }
            return true;
        };

        $("#TakeTicketPhotoButton").click(function (e) {
            e.preventDefault();
            _addTicketPhotoModal.open({ guid: $("#Guid").val() });
        });

        abp.event.on('app.ticketPhotoAddedModal', function (eventData) {
            $("#TicketPhotoId").val(eventData.photoId);
            $("#TicketPhotoSelectedMessage").slideDown(function () { $(this).show(); });
            $("#TakeTicketPhotoButton").prop('disabled', true);
        });

    });
})();

