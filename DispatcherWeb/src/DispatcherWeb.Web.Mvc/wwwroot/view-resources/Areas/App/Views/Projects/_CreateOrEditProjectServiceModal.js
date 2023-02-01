(function($) {
    app.modals.CreateOrEditProjectServiceModal = function () {

        var _modalManager;
        var _projectAppService = abp.services.app.project;
        var _$form = null;
        var _materialUomDropdown = null;
        var _freightUomDropdown = null;
        var _designationDropdown = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            var materialUomDropdown = _materialUomDropdown = _$form.find("#MaterialUomId");
            var freightUomDropdown = _freightUomDropdown = _$form.find("#FreightUomId");
            var designationDropdown = _designationDropdown = _$form.find("#Designation");

            var loadAtDropdown = _$form.find("#LoadAtId");
            var deliverToDropdown = _$form.find("#DeliverToId");
            var serviceDropdown = _$form.find("#ServiceId");
            var materialQuantityInput = _$form.find("#MaterialQuantity");
            var freightQuantityInput = _$form.find("#FreightQuantity");
            var materialRateInput = _$form.find("#PricePerUnit");
            var freightRateInput = _$form.find("#FreightRate");
            var leaseHaulerRateInput = _$form.find("#LeaseHaulerRate");

            if (!abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnQuote')) {
                leaseHaulerRateInput.closest('.form-group').hide();
            }

            loadAtDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite
            });
            deliverToDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite
            });
            serviceDropdown.select2Init({
                abpServiceMethod: abp.services.app.service.getServicesSelectList,
                minimumInputLength: 0,
                allowClear: false
                //dropdownParent: $("#" + _modalManager.getModalId())
            });
            materialUomDropdown.select2Uom({
                //dropdownParent: $("#" + _modalManager.getModalId())
            });
            freightUomDropdown.select2Uom({
                //dropdownParent: $("#" + _modalManager.getModalId())
            });
            designationDropdown.select2Init({
                showAll: true,
                allowClear: false
                //dropdownParent: $("#" + _modalManager.getModalId())
            });

            abp.helper.ui.syncUomDropdowns(_materialUomDropdown, _freightUomDropdown, _designationDropdown, materialQuantityInput, freightQuantityInput);

            if (materialQuantityInput.val() !== "") {
                materialQuantityInput.val(abp.utils.round(materialQuantityInput.val()));
            }

            if (freightQuantityInput.val() !== "") {
                freightQuantityInput.val(abp.utils.round(freightQuantityInput.val()));
            }

            if (materialRateInput.val() !== "") {
                materialRateInput.val(abp.utils.round(materialRateInput.val()).toFixed(2));
            }

            if (freightRateInput.val() !== "") {
                freightRateInput.val(abp.utils.round(freightRateInput.val()).toFixed(2));
            }

            if (leaseHaulerRateInput.val() !== "") {
                leaseHaulerRateInput.val(abp.utils.round(leaseHaulerRateInput.val()).toFixed(2));
            }

            designationDropdown.change(function () {
                if (designationHasMaterial()) {
                    enableMaterialFields();
                } else {
                    disableMaterialFields();
                }
                if (designationIsMaterialOnly()) {
                    disableFreightFields();
                } else {
                    enableFreightFields();
                }
            }).change();

            function disableMaterialFields() {
                materialRateInput.attr('disabled', 'disabled').val('0');
                materialUomDropdown.attr('disabled', 'disabled').val('').change();
                materialQuantityInput.attr('disabled', 'disabled').val('');
                materialRateInput.closest('.form-group').hide();
                materialUomDropdown.closest('.form-group').hide();
                materialQuantityInput.closest('.form-group').hide();
            }
            function enableMaterialFields() {
                materialRateInput.removeAttr('disabled');
                materialUomDropdown.removeAttr('disabled');
                materialQuantityInput.removeAttr('disabled');
                materialRateInput.closest('.form-group').show();
                materialUomDropdown.closest('.form-group').show();
                materialQuantityInput.closest('.form-group').show();
            }
            function disableFreightFields() {
                freightRateInput.attr('disabled', 'disabled').val('0');
                freightUomDropdown.attr('disabled', 'disabled').val('').change();
                freightQuantityInput.attr('disabled', 'disabled').val('');
                freightRateInput.closest('.form-group').hide();
                freightUomDropdown.closest('.form-group').hide();
                freightQuantityInput.closest('.form-group').hide();
            }
            function enableFreightFields() {
                freightRateInput.removeAttr('disabled');
                freightUomDropdown.removeAttr('disabled');
                freightQuantityInput.removeAttr('disabled');
                freightRateInput.closest('.form-group').show();
                freightUomDropdown.closest('.form-group').show();
                freightQuantityInput.closest('.form-group').show();
            }
        };

        function designationHasMaterial() {
            var designation = Number(_designationDropdown.val());
            return abp.enums.designations.hasMaterial.includes(designation);
        }
        function designationIsMaterialOnly() {
            return abp.enums.designations.materialOnly.includes(Number(_designationDropdown.val()));
        }

        function validateFields(entity) {
            var isFreightUomValid = true;
            if (!designationIsMaterialOnly()) {
                if (!Number(entity.FreightUomId)) {
                    isFreightUomValid = false;
                }
            }

            var isMaterialUomValid = true;
            if (designationHasMaterial()) {
                if (!Number(entity.MaterialUomId)) {
                    isMaterialUomValid = false;
                }
            }

            if (!isFreightUomValid
                || !isMaterialUomValid) {
                abp.message.error('Please check the following: \n'
                    + (isMaterialUomValid ? '' : '"Material UOM" - This field is required.\n')
                    + (isFreightUomValid ? '' : '"Freight UOM" - This field is required.\n'), 'Some of the data is invalid');
                return false;
            }
            return true;
        }

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var projectService = _$form.serializeFormToObject();

            if (!validateFields(projectService)) {
                return;
            }

            _modalManager.setBusy(true);
            _projectAppService.editProjectService(projectService).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditProjectServiceModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };
})(jQuery);