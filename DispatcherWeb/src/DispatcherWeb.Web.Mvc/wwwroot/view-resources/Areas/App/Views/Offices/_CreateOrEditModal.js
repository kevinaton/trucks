(function ($) {
    app.modals.CreateOrEditOfficeModal = function () {

        var _modalManager;
        var _officeService = abp.services.app.office;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            var truckColorContainer = _$form.find('#TruckColorContainer');

            truckColorContainer.colorpicker({
                format: 'hex',
                useAlpha: false,
                autoInputFallback: true,
                customClass: 'colorpicker-2x',
                sliders: {
                    saturation: {
                        maxLeft: 200,
                        maxTop: 200
                    },
                    hue: {
                        maxTop: 200
                    },
                    alpha: {
                        maxTop: 200
                    }
                }
            });

            var truckColorPicker = truckColorContainer.data("colorpicker");

            _$form.find('#TruckColor').click(function () {
                truckColorPicker.show();
            });

            _$form.find("#DefaultStartTime").timepickerInit({ stepping: 1 });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var office = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _officeService.editOffice(office).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditOfficeModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);