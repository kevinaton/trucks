
(function ($) {
    app.modals.AddLeaseHaulerStatementModal = function () {

        var _modalManager;
        var _leaseHaulerStatementService = abp.services.app.leaseHaulerStatement;
        var _dtHelper = abp.helper.dataTables;
        var _$form = null;
        var _leaseHaulersDropdown = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            var modal = _modalManager.getModal();
            _$form = modal.find('form');
            _$form.validate();

            _leaseHaulersDropdown = _$form.find("#LeaseHaulerIds");
            _leaseHaulersDropdown.select2Init({
                abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
                showAll: true,
                allowClear: false
            });
            _modalManager.onOpenOnce(function () {
                _leaseHaulersDropdown.val(null).change();
            });

            _$form.find("#DateRange").daterangepicker({
                locale: {
                    cancelLabel: 'Clear'
                },
                showDropDown: true,
                autoUpdateInput: false
            }).on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            }).on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
            });

        };

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var model = _$form.serializeFormToObject();
            $.extend(model, _dtHelper.getDateRangeObject(model.DateRange, 'startDate', 'endDate'));
            model.LeaseHaulerIds = _leaseHaulersDropdown.val();
            delete model.dateRange;

            try {
                _modalManager.setBusy(true);
                await _leaseHaulerStatementService.addLeaseHaulerStatement(model);
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.addLeaseHaulerStatementModalSaved');
            } finally {
                _modalManager.setBusy(false);
            }
        };
    };
})(jQuery);