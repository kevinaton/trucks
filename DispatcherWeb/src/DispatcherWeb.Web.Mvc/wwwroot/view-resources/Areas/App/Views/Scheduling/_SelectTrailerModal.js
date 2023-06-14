(function ($) {
    app.modals.SelectTrailerModal = function () {

        var _modalManager;
        var _truckService = abp.services.app.truck;
        var _$form = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            var modalArgs = modalManager.getArgs();

            _$form = _modalManager.getModal().find('form');

            if (modalArgs.optional) {
                _$form.find('#TrailerId').removeAttr('required').closest('.form-group').find('.required-label').removeClass('required-label');
            }

            _$form.validate();

            abp.helper.ui.initControls();

            var vehicleCategoryDropdown = _$form.find('#VehicleCategoryId');
            var bedConstructionDropdown = _$form.find('#BedConstruction');
            var makeDropdown = _$form.find('#Make');
            var modelDropdown = _$form.find('#Model');

            _$form.find('#ModalSubtitle').text(modalArgs.modalSubtitle || '');
            _$form.find("#Message").text(modalArgs.message || '');

            vehicleCategoryDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getVehicleCategoriesSelectList,
                abpServiceParams: {
                    assetType: abp.enums.assetType.trailer
                },
                showAll: true,
                allowClear: true
            });
            vehicleCategoryDropdown.change(updateCategoryControls);

            refreshControlsVisibillity();

            async function updateCategoryControls() {
                try {
                    abp.ui.setBusy(bedConstructionDropdown.closest('.form-group'));
                    let bedConstructions = await _truckService.getBedConstructions({
                        vehicleCategoryId: vehicleCategoryDropdown.val()
                    });
                    bedConstructionDropdown.val(null).change();
                    bedConstructionDropdown.removeUnselectedOptions();
                    bedConstructions.forEach(x => {
                        bedConstructionDropdown.append($('<option>').attr('value', x.id).text(x.name));
                    });
                }
                finally {
                    abp.ui.clearBusy(bedConstructionDropdown.closest('.form-group'));
                }

                refreshControlsVisibillity();
            }

            function refreshControlsVisibillity() {
                if (vehicleCategoryDropdown.val() == '') {
                    bedConstructionDropdown.val('').change().closest('.form-group').hide();
                    makeDropdown.val('').change().closest('.form-group').hide();
                    modelDropdown.val('').change().closest('.form-group').hide();
                } else {
                    bedConstructionDropdown.closest('.form-group').show();
                    makeDropdown.closest('.form-group').show();
                    modelDropdown.closest('.form-group').show();
                }
            }

            bedConstructionDropdown.select2Init({
                showAll: true,
                allowClear: true
            });
            
            makeDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getMakesSelectList,
                abpServiceParamsGetter: (params) => ({
                    vehicleCategoryId: vehicleCategoryDropdown.val()
                }),
                showAll: true,
                allowClear: true
            });

            modelDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getModelsSelectList,
                abpServiceParamsGetter: (params) => ({
                    vehicleCategoryId: vehicleCategoryDropdown.val(),
                    make: makeDropdown.val()
                }),
                showAll: true,
                allowClear: true
            });
            
            _$form.find('#TrailerId').select2Init({
                abpServiceMethod: abp.services.app.truck.getActiveTrailersSelectList,
                abpServiceParamsGetter: (params) => ({
                    vehicleCategoryId: vehicleCategoryDropdown.val(),
                    bedConstruction: bedConstructionDropdown.val(),
                    make: makeDropdown.val(),
                    model: modelDropdown.val()
                }),
                showAll: true,
                allowClear: true
            });
        };

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var formData = _$form.serializeFormToObject();

            let result = formData.TrailerId ? {
                id: Number(formData.TrailerId),
                truckCode: _$form.find('#TrailerId').getSelectedDropdownOption().text()
            } : null;

            _modalManager.setResult(result);
            _modalManager.close();
        };
    };
})(jQuery);