(function ($) {
    app.modals.CopyOrderModal = function () {

        var _modalManager;
        var _orderService = abp.services.app.order;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _dateBeginPicker = null;
        var _dateEndPicker = null;
        var _$shiftSelect;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            var $dateBegin = _$form.find('#DateBegin');
            var $dateEnd = _$form.find('#DateEnd');
            $dateBegin.datepickerInit();
            $dateEnd.datepickerInit();
            _dateBeginPicker = $dateBegin.data('DateTimePicker');
            _dateEndPicker = $dateEnd.data('DateTimePicker');

            $dateBegin.on('dp.change', function (e) {
                var dateBegin = _dateBeginPicker.date();
                var dateEnd = _dateEndPicker.date();
                if (dateEnd < dateBegin) {
                    _dateEndPicker.date(dateBegin);
                }
            });

            $dateEnd.on('dp.change', function (e) {
                var dateEnd = _dateEndPicker.date();
                var dateBegin = _dateBeginPicker.date();
                if (dateEnd < dateBegin) {
                    _dateBeginPicker.date(dateEnd);
                }
            });

            _$shiftSelect = _$form.find('#Shift').select2Init({ allowClear: false });

        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var today = moment().startOf('day');
            var beginDate = _dateBeginPicker.date();
            var endDate = _dateEndPicker.date();
            var minDate = moment('1971-01-01', 'YYYY-MM-DD');
            var maxDate = moment('2100-01-01', 'YYYY-MM-DD');
            if (beginDate > maxDate || beginDate < minDate || endDate > maxDate || endDate < minDate) {
                abp.message.error('Please correct a date', 'Some of the data is invalid');
                return;
            }

            var daysBetween = endDate.diff(beginDate, 'days');
            if (daysBetween + 1 > 7) {
                abp.message.error('Please correct a date', 'You cannot copy order for more than 7 days.');
                return;
            }

            if (beginDate < today) {
                if (!await abp.message.confirm("You are creating an order on a previous date. Are you sure you want to do this?")) {
                    return;
                }
            }

            var formData = _$form.serializeFormToObject();
            try {
                _modalManager.setBusy(true);
                var newOrderIds = await _orderService.copyOrder({
                    orderId: formData.OrderId,
                    orderLineId: formData.OrderLineId,
                    dateBegin: formData.DateBegin,
                    dateEnd: formData.DateEnd,
                    shifts: _$shiftSelect.val(),
                    copyTrucks: formData.CopyOrderTrucks
                });

                if (formData.CopyOrderTrucks) {
                    _modalManager.setBusy(true);
                    var copyOrderTrucksInput = {
                        originalOrderId: formData.OrderId,
                        newOrderIds: newOrderIds,
                        orderLineId: formData.OrderLineId,
                        proceedOnConflict: false
                    };
                    var truckCopyResult = await _schedulingService.copyOrdersTrucks(copyOrderTrucksInput);

                    if (!truckCopyResult.completed) {
                        var s = truckCopyResult.conflictingTrucks.length > 1 ? 's' : '';
                        var is = truckCopyResult.conflictingTrucks.length > 1 ? 'are' : 'is';
                        var conflictingTrucks = truckCopyResult.conflictingTrucks.join(', ');
                        _modalManager.setBusy(false);
                        if (await abp.message.confirmWithOptions({
                            text: 'Truck' + s + ' ' + conflictingTrucks + ' ' + is + ' already scheduled on an order and can’t be copied. Do you want to continue with copying the remaining trucks?',
                            title: ' ',
                            cancelButtonText: 'No'
                        })) {
                            _modalManager.setBusy(true);
                            copyOrderTrucksInput.proceedOnConflict = true;
                            await _schedulingService.copyOrdersTrucks(copyOrderTrucksInput);
                        }
                        _modalManager.setBusy(true);
                    } else {
                        if (truckCopyResult.someTrucksAreNotCopied) {
                            abp.message.info('Something prevented some of the trucks from being copied with the order.');
                        }
                    }

                    await _orderService.recalculateStaggeredTimeForOrders({
                        orderIds: newOrderIds
                    });
                }

                abp.notify.info('Saved successfully.');
                _modalManager.setBusy(false);
                _modalManager.close();
                abp.event.trigger('app.orderModalCopied', {
                    newOrderId: newOrderIds[0]
                });
            }
            finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);