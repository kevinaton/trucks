(function($) {
    app.modals.CreateOrEditQuoteServiceModal = function () {

        var _modalManager;
        var _quoteAppService = abp.services.app.quote;
        var _$form = null;
        var _designationDropdown = null;
        var _addLocationTarget = null;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            var createOrEditServiceModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Services/CreateOrEditServiceModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Services/_CreateOrEditServiceModal.js',
                modalClass: 'CreateOrEditServiceModal',
                modalSize: 'lg'
            });

            var createOrEditLocationModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Locations/CreateOrEditLocationModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Locations/_CreateOrEditLocationModal.js',
                modalClass: 'CreateOrEditLocationModal',
                modalSize: 'lg'
            });

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            var loadAtDropdown = _$form.find("#LoadAtId");
            var deliverToDropdown = _$form.find("#DeliverToId");
            var serviceDropdown = _$form.find("#ServiceId");
            var materialUomDropdown = _$form.find("#MaterialUomId");
            var freightUomDropdown = _$form.find("#FreightUomId");
            var designationDropdown = _designationDropdown = _$form.find("#Designation");
            var materialQuantityInput = _$form.find("#MaterialQuantity");
            var freightQuantityInput = _$form.find("#FreightQuantity");
            var materialRateInput = _$form.find("#PricePerUnit");
            var freightRateInput = _$form.find("#FreightRate");
            var leaseHaulerRateInput = _$form.find("#LeaseHaulerRate");

            if (leaseHaulerRateInput.val() !== "") {
                leaseHaulerRateInput.val(abp.utils.round(leaseHaulerRateInput.val()).toFixed(2));
            }

            if (!abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnQuote')) {
                leaseHaulerRateInput.closest('.form-group').hide();
            }

            loadAtDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                width: 'calc(100% - 45px)'
            });
            deliverToDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                width: 'calc(100% - 45px)'
            });
            serviceDropdown.select2Init({
                abpServiceMethod: abp.services.app.service.getServicesSelectList,
                minimumInputLength: 0,
                allowClear: false,
                width: 'calc(100% - 45px)'
            });
            materialUomDropdown.select2Uom();
            freightUomDropdown.select2Uom();
            designationDropdown.select2Init({
                showAll: true,
                noSearch: true,
                allowClear: false
            });

            abp.helper.ui.syncUomDropdowns(materialUomDropdown, freightUomDropdown, _designationDropdown, materialQuantityInput, freightQuantityInput);

            if (materialQuantityInput.val() !== "") {
                materialQuantityInput.val(abp.utils.round(materialQuantityInput.val()));
            }

            if (freightQuantityInput.val() !== "") {
                freightQuantityInput.val(abp.utils.round(freightQuantityInput.val()));
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

            _modalManager.on('app.createOrEditServiceModalSaved', function (e) {
                abp.helper.ui.addAndSetDropdownValue(serviceDropdown, e.item.Id, e.item.Service1);
                serviceDropdown.change();
            });

            _modalManager.on('app.createOrEditLocationModalSaved', function (e) {
                if (!_addLocationTarget) {
                    return;
                }
                var targetDropdown = _$form.find("#" + _addLocationTarget);
                abp.helper.ui.addAndSetDropdownValue(targetDropdown, e.item.id, e.item.displayName);
                targetDropdown.change();
            });

            _modalManager.getModal().find("#AddNewServiceButton").click(function (e) {
                e.preventDefault();
                createOrEditServiceModal.open();
            });

            _modalManager.getModal().find(".AddNewLocationButton").click(function (e) {
                e.preventDefault();
                _addLocationTarget = $(this).data('target-field');
                createOrEditLocationModal.open({ mergeWithDuplicateSilently: true });
            });

        };

        function designationHasMaterial() {
            var designation = Number(_designationDropdown.val());
            return abp.enums.designations.hasMaterial.includes(designation);
        }
        function designationIsMaterialOnly() {
            return abp.enums.designations.materialOnly.includes(Number(_designationDropdown.val()));
        }

        this.save = function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var quoteService = _$form.serializeFormToObject();

            _modalManager.setBusy(true);
            _quoteAppService.editQuoteService(quoteService).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.createOrEditQuoteServiceModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };

    };
})(jQuery);