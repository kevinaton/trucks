(function($) {
    app.modals.AddSharedTruckModal = function () {

        var _modalManager;
        var _truckService = abp.services.app.truck;
        var _$form = null;
		var _$shiftSelect;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();
            
            abp.helper.ui.initControls();

            var dateInput = _$form.find("#Date");
            var startDateInput = _$form.find("#StartDate");
            var endDateInput = _$form.find("#EndDate");
            var startDtInput = startDateInput.val();
            var endDtInput = endDateInput.val();

            var todayDt = new Date();
            var startDt = new Date(startDtInput);
            var endDt = new Date(endDtInput);
            if (startDt < todayDt) {
                startDtInput = abp.helper.getMMddyyyyDate(todayDt);
            }
            if (endDt < todayDt) {
                endDtInput = abp.helper.getMMddyyyyDate(todayDt);
            }

            dateInput.val(startDtInput + ' - ' + endDtInput);

            dateInput.daterangepicker({
                    autoUpdateInput: false,
                    locale: {
                        cancelLabel: 'Cancel'
					},
					minDate: new Date()
                },
                function (start, end, label) {
                    startDateInput.val(start.format('MM/DD/YYYY'));
                    endDateInput.val(end.format('MM/DD/YYYY'));
                });

            dateInput.on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                startDateInput.val(picker.startDate.format('MM/DD/YYYY'));
                endDateInput.val(picker.endDate.format('MM/DD/YYYY'));
            });

            dateInput.on('cancel.daterangepicker', function (ev, picker) {
            });

            _$shiftSelect = _$form.find('#Shift').select2Init({ allowClear: false });
        };

        this.save = function () {
            if (!_$form.valid()) {
            	_$form.showValidateMessage();
                return;
            }
            
            var formData = _$form.serializeFormToObject();
            
            _modalManager.setBusy(true);
            _truckService.addSharedTruck({
                truckId: formData.TruckId,
                officeId: formData.OfficeId,
                startDate: formData.StartDate,
				endDate: formData.EndDate,
				shifts: _$shiftSelect.val()
            }).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.sharedTruckModalAdded');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);
