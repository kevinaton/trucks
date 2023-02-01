(function ($) {
    app.modals.CreateOrEditTicketModal = function () {

        var _modalManager;
        var _ticketService = abp.services.app.ticket;
        var _$form = null;
        var _lastTruckCode = null;
        var _orderLineId = null;
        var _addLocationTarget = null;
        var _validateTrucksAndDrivers = abp.setting.getBoolean('App.General.ValidateDriverAndTruckOnTickets');

        var _selectOrderLineModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Tickets/SelectOrderLineModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Tickets/_SelectOrderLineModal.js',
            modalClass: 'SelectOrderLineModal'
        });

        var _createOrEditLocationModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
            modalClass: 'CreateOrEditLocationModal',
            modalSize: 'lg'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();

            _$form = modal.find('form');
            _$form.validate();


            abp.helper.ui.initControls();

            var loadAtDropdown = _$form.find("#LoadAtId");
            var deliverToDropdown = _$form.find("#DeliverToId");

            _orderLineId = Number(_$form.find("#OrderLineId").val()) || null;

            modal.on('shown.bs.modal', function () {
                modal.find('#CustomerId').focus();
                //_$form.find("#TicketDateTime").datepicker('hide');
            });
            _$form.find('#TicketDateTime').datetimepickerInit();

            loadAtDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                width: 'calc(100% - 45px)'
            });
            deliverToDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                width: 'calc(100% - 45px)'
            });

            _$form.find("select#OfficeId").select2Init({
                abpServiceMethod: abp.services.app.office.getOfficesSelectList,
                showAll: true,
                allowClear: false
            });

            _$form.find('#CustomerId').select2Init({
                abpServiceMethod: abp.services.app.customer.getActiveCustomersSelectList,
                allowClear: false
                //dropdownParent: $("#" + _modalManager.getModalId())
            });

            _$form.find('#ServiceId').select2Init({
                abpServiceMethod: abp.services.app.service.getServicesSelectList,
                minimumInputLength: 0,
                allowClear: false
                //dropdownParent: $("#" + _modalManager.getModalId())
            });

            _$form.find('#UomId').select2Uom();

            let carrierDropdown = _$form.find('#CarrierId');
            carrierDropdown.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                //dropdownParent: $("#" + _modalManager.getModalId())
            }).change(function () {
                var newCarrierId = $(this).val();
                _$form.find('label[for="TruckCode"],label[for="DriverId"]').toggleClass('required-label', !newCarrierId);
            }).change();

            let driverDropdown = _$form.find('#DriverId');
            let truckCodeInput = _$form.find('#TruckCode');
            let truckOrDriverIsChanging = false;
            truckCodeInput.blur(async function () {
                let truckCode = _$form.find('#TruckCode').val();
                if (truckCode == _lastTruckCode) {
                    return;
                }
                _lastTruckCode = truckCode;

                if (!_validateTrucksAndDrivers) {
                    return;
                }

                let ticketDateTime = _$form.find("#TicketDateTime").val();
                if (!ticketDateTime && !_orderLineId || !truckCode) {
                    return;
                }

                if (truckOrDriverIsChanging) {
                    return;
                }
                let lockedControls = driverDropdown.closest('.form-group')
                    .add(truckCodeInput.closest('.form-group'));
                try {
                    truckOrDriverIsChanging = true;
                    abp.ui.setBusy(lockedControls);
                    let ticketDriver = await _ticketService.getDriverForTicketTruck({
                        orderLineId: _orderLineId,
                        orderDate: _orderLineId ? null : moment(ticketDateTime, 'L').format('L'),
                        truckCode: truckCode
                    });

                    if (!ticketDriver.truckCodeIsCorrect) {
                        if (!carrierDropdown.val()) {
                            abp.message.error("Truck Code is invalid");
                        }
                        return;
                    }

                    if (ticketDriver.driverId) {
                        abp.helper.ui.addAndSetDropdownValue(driverDropdown, ticketDriver.driverId, ticketDriver.driverName);
                    }
                    abp.helper.ui.addAndSetDropdownValue(carrierDropdown, ticketDriver.carrierId, ticketDriver.carrierName);
                }
                finally {
                    abp.ui.clearBusy(lockedControls);
                    truckOrDriverIsChanging = false;
                }
            });

            driverDropdown.select2Init({
                abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                abpServiceParams: { orderLineId: _validateTrucksAndDrivers ? _orderLineId : null },
                abpServiceParamsGetter: (params, rowData) => ({
                    truckCode: _lastTruckCode
                }),
                showAll: true,
            }).change(async function () {
                var driverId = $(this).val();
                if (!_validateTrucksAndDrivers) {
                    return;
                }

                let ticketDateTime = _$form.find("#TicketDateTime").val();
                if (!ticketDateTime && !_orderLineId || !driverId) {
                    return;
                }

                if (truckOrDriverIsChanging) {
                    return;
                }
                let lockedControls = driverDropdown.closest('.form-group')
                    .add(truckCodeInput.closest('.form-group'));
                try {
                    truckOrDriverIsChanging = true;
                    abp.ui.setBusy(lockedControls);
                    let ticketTruck = await _ticketService.getTruckForTicketDriver({
                        orderLineId: _orderLineId,
                        orderDate: _orderLineId ? null : moment(ticketDateTime, 'L').format('L'),
                        driverId: driverId
                    });

                    if (ticketTruck.truckId) {
                        truckCodeInput.val(ticketTruck.truckCode);
                        abp.helper.ui.addAndSetDropdownValue(carrierDropdown, ticketTruck.carrierId, ticketTruck.carrierName);
                    }
                }
                finally {
                    abp.ui.clearBusy(lockedControls);
                    truckOrDriverIsChanging = false;
                }
            });

            _modalManager.on('app.createOrEditLocationModalSaved', function (e) {
                if (!_addLocationTarget) {
                    return;
                }
                var targetDropdown = _$form.find("#" + _addLocationTarget);
                abp.helper.ui.addAndSetDropdownValue(targetDropdown, e.item.id, e.item.displayName);
                targetDropdown.change();
            });

            modal.find(".AddNewLocationButton").click(function (e) {
                e.preventDefault();
                _addLocationTarget = $(this).data('target-field');
                _createOrEditLocationModal.open({ mergeWithDuplicateSilently: true });
            });

            if (modal.find("#ReadOnly").val() === "true") {
                modal.find("input, select, textarea, form button").prop('disabled', true);
                modal.find('.save-button').hide();
                modal.find('.close-button').text('Close');
            }

            var ticketId = modal.find("#Id").val();
            var $ticketPhotoId = modal.find("#TicketPhotoId");
            var $ticketPhoto = modal.find("#TicketPhoto");
            var ticketPhotoId = $ticketPhotoId.val();
            if (ticketId) {
                if (ticketPhotoId) {
                    modal.find("#TicketPhotoBlock").show();
                } else {
                    modal.find("#AddTicketPhotoBlock").show();
                }
            }
            var addTicketPhotoButton = modal.find("#AddTicketPhotoButton");
            addTicketPhotoButton.click(function (e) {
                e.preventDefault();
                if (!validateTicketPhoto()) {
                    return;
                }
                const file = $ticketPhoto[0].files[0];
                const reader = new FileReader();

                reader.addEventListener("load", function () {
                    _ticketService.addTicketPhoto({
                        ticketId: ticketId,
                        ticketPhoto: reader.result,
                        ticketPhotoFilename: file.name
                    }).done(function (result) {
                        $ticketPhotoId.val(result.ticketPhotoId);
                        ticketPhotoId = result.ticketPhotoId;
                        modal.find("#AddTicketPhotoBlock").hide();
                        modal.find("#TicketPhotoBlock").show();
                    }).always(function () {
                        abp.ui.clearBusy(addTicketPhotoButton);
                        _modalManager.setBusy(false);
                    });
                }, false);

                abp.ui.setBusy(addTicketPhotoButton);
                _modalManager.setBusy(true);
                reader.readAsDataURL(file);
            });

            function validateTicketPhoto() {
                let files = $ticketPhoto.get(0).files;

                if (!files.length) {
                    showValidationError('No file is selected.');
                    return false;
                }

                let file = files[0];

                //File type check
                let type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                if ('|jpg|jpeg|png|gif|pdf|'.indexOf(type) === -1) {
                    showValidationError('Invalid file type.');
                    return false;
                }
                //File size check
                if (file.size > 8388608) //8 MB
                {
                    showValidationError('Size of the file exceeds allowed limits.');
                    return false;
                }
                return true;
            }

            function showValidationError(errorMessage) {
                abp.message.error('Please check the following: \n' + errorMessage, 'Some of the data is invalid');
            }

            modal.find("#showTicketPhotoButton").click(function (e) {
                e.preventDefault();
                $(this).hide();
                let url = abp.appPath + 'app/Tickets/GetTicketPhoto/' + ticketId;
                window.open(url);
                //$('<img>').attr('src', url).css('width', '100%').css('cursor', 'pointer').click(function () {
                //    window.open(url);
                //}).appendTo(modal.find("#TicketPhotoBlock"));
            });
        };

        this.save = async function () {
            let cannotEditReason = _$form.find("#CannotEditReason").val();
            if (cannotEditReason) {
                abp.message.error(cannotEditReason);
                return;
            }

            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var ticket = _$form.serializeFormToObject();

            if (!abp.helper.validateFutureDates(
                { value: $("#TicketDateTime").val(), title: $('label[for="TicketDateTime"]').text() }
            )) {
                return;
            }

            if (ticket.OrderDate) {
                if (moment(ticket.TicketDateTime, "MM/DD/YYYY") > moment(ticket.OrderDate, "MM/DD/YYYY")) {
                    if (!await abp.message.confirm(
                        'You have entered a ticket date that is different from the order date. This may cause your Revenue Report by Order to be different from the tickets for the same date range. Are you sure you want to do this?',
                    )) {
                        return;
                    }
                }
            }

            try {

                _modalManager.setBusy(true);
                let outOfServiceOrInactive = await _ticketService.checkTruckIsOutofServiceOrInactive(ticket);
                if (outOfServiceOrInactive !== '') {
                    if (!await abp.message.confirm(
                        'Truck ' + ticket.TruckCode + ' is ' + outOfServiceOrInactive + '. Are you sure you want to add a ticket for this truck?',
                    )) {
                        _modalManager.setBusy(false);
                        return;
                    }
                }

                //function checklookForExistingOrderLines(ticket) {
                //    _modalManager.setBusy(true);
                //    _ticketService.lookForExistingOrderLines(ticket).done(function (orderLines) {
                //        //console.log(orderLines);
                //        if (orderLines.length > 1) {
                //            abp.event.on('app.selectOrderLineModalSaved', function (orderLineId) {
                //                //console.log(orderLineId);
                //                ticket.orderLineId = orderLineId;
                //                saveTicketAndCloseModal(ticket);
                //            });
                //            _modalManager.setBusy(false);
                //            _selectOrderLineModal.open(ticket);
                //        } else {
                //            if (orderLines.length === 1) {
                //                ticket.orderLineId = orderLines[0].id;
                //            }
                //            saveTicketAndCloseModal(ticket);
                //        }
                //    }).catch(function () {
                //        _modalManager.setBusy(false);
                //    });
                //}

                
                //if (ticket.ReceiptLineId) {
                //    abp.message.confirm(
                //        'This ticket is associated with a receipt. If you change this ticket, your ticket and receipt amounts will not match. Are you sure you want to do this?',
                //        function (isConfirmed) {
                //            if (isConfirmed) {
                //                saveCallback(ticket);
                //            }
                //        }
                //    );
                //} else {
                //    saveCallback(ticket);
                //}

                //if (ticket.DriverId && ticket.OrderLineIsProductionPay === 'True' && !(Number(ticket.DriverPayRate) > 0)) {
                //    _modalManager.setBusy(true);
                //    let timeClassification = await abp.services.app.driver.getDriverPayRate({
                //        driverId: ticket.DriverId,
                //        productionPay: true
                //    });
                    
                //    if (!timeClassification.payRate) {
                //        await abp.message.warn(app.localize('Driver{0}DoesntHaveProductionRateSet', timeClassification.driverName));
                //    } else {
                //        let shouldUpdateRate = ticket.Id === "0" ? 'yes' : await swal(app.localize('DriverPayRateIs0WouldYouLikeToUpdate'), {
                //            buttons: { yes: 'Yes', no: 'No' }
                //        });
                //        if (shouldUpdateRate === 'yes') {
                //            ticket.DriverPayRate = timeClassification.payRate;
                //        }
                //    }
                //}

                _modalManager.setBusy(true);
                await _ticketService.editTicket(ticket);
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditTicketModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };

    };
})(jQuery);