(function ($) {
    app.modals.CreateOrEditVehicleCategoryModal = function () {

        var _modalManager;
        var _vehicleCategoryService = abp.services.app.vehicleCategory;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditVehicleCategoryModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/VehicleCategories/CreateOrEditVehicleCategoryModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/VehicleCategories/_CreateOrEditVehicleCategoryModal.js',
            modalClass: 'CreateOrEditVehicleCategoryModal',
            modalSize: 'md'
        });

        var _modal = null;
        var _$form = null;
        var _vehicleCategoryId = null;

        var _idFieldHiddentInput = null;
        var _nameFieldInput = null;
        var _assetTypeInput = null;
        var _isPoweredInput = null;

        this.init = function (modalManager) {
            _modalManager = modalManager;
            _modal = _modalManager.getModal();
            _$form = _modal.find('form');
            _$form.validate();
            $.validator.addMethod(
                "regex",
                function (value, element, regexp) {
                    var re = new RegExp(regexp, 'i');
                    return this.optional(element) || re.test(value);
                },
                "Please check your input."
            );

            _idFieldHiddentInput = _$form.find("#Id");
            _nameFieldInput = _$form.find('#Name');
            _assetTypeInput = _$form.find('#AssetType');
            _isPoweredInput = _$form.find('#IsPowered');

            abp.helper.ui.initControls();

            _vehicleCategoryId = _idFieldHiddentInput.val();

            _$form.find("#AssetType").select2Init({
                abpServiceMethod: _vehicleCategoryService.getAssetTypesSelectList,
                showAll: true,
                allowClear: false
            });

        };

        this.focusOnDefaultElement = function () {
            if (_vehicleCategoryId) {
                return;
            }
            _nameFieldInput.focus();
        }

        var ensureNoDuplication = (vehicleCategory) => new Promise((resolve, reject) => {
            if (!_vehicleCategoryId) {
                _vehicleCategoryService.getVehicleCategories({ name: vehicleCategory.Name }).then((response) => {
                    if (response.totalCount > 0) {
                        reject(response.items[0]);
                    }
                    else resolve();
                });
            }
            else resolve();
        });

        var saveVehicleCategoryAsync = function () {

            var promise = new Promise((resolve, reject) => {

                if (!_$form.valid()) {
                    _$form.showValidateMessage();
                    throw new Error("Form is not valid");
                }

                abp.ui.setBusy(_$form);
                _modalManager.setBusy(true);

                var vehicleCategory = _$form.serializeFormToObject();

                ensureNoDuplication(vehicleCategory)
                    .then(
                        (result) => {
                            _vehicleCategoryService.editVehicleCategory(vehicleCategory).done(data => {
                                abp.notify.info('Saved successfully.');
                                _idFieldHiddentInput.val(data.id);
                                _vehicleCategoryId = data.id;
                                vehicleCategory.Id = data.id;
                                abp.event.trigger('app.CreateOrEditVehicleCategoryModalSaved', {
                                    item: data
                                });
                                resolve();
                            })
                        },
                        async (duplicateItem) => {
                            if (await abp.message.confirm("A vehicle category with the same name already exists. Do you want to edit the existing vehicle category instead?")) {
                                _modalManager.close();
                                setTimeout(() => {
                                    _createOrEditVehicleCategoryModal.open({ id: duplicateItem.id });
                                }, 100);
                            }
                            else throw new Error(error);
                        });
            });

            return promise;
        };

        this.save = function () {
            saveVehicleCategoryAsync().then(() => {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
                _modalManager.close();
            });
        };
    };
})(jQuery);