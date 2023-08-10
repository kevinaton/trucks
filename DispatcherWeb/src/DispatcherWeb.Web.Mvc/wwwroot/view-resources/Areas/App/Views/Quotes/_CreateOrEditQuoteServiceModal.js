﻿(function ($) {
    app.modals.CreateOrEditQuoteServiceModal = function () {

        var _modalManager;
        var _quoteAppService = abp.services.app.quote;
        var _$form = null;
        var _designationDropdown = null;
        var _ratesLastValue = {};
        var _wasProductionPay = null;

        this.init = function (modalManager) {
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
            var vehicleCategoriesDropdown = _$form.find("#VehicleCategories");
            var materialUomDropdown = _$form.find("#MaterialUomId");
            var freightUomDropdown = _$form.find("#FreightUomId");
            var designationDropdown = _designationDropdown = _$form.find("#Designation");
            var materialQuantityInput = _$form.find("#MaterialQuantity");
            var freightQuantityInput = _$form.find("#FreightQuantity");
            var materialRateInput = _$form.find("#PricePerUnit");
            var freightRateInput = _$form.find("#FreightRate");
            var leaseHaulerRateInput = _$form.find("#LeaseHaulerRate");
            var freightRateToPayDriversInput = _$form.find("#FreightRateToPayDrivers");

            _ratesLastValue.freightRate = Number(freightRateInput.val()) || 0;

            if (leaseHaulerRateInput.val() !== "") {
                leaseHaulerRateInput.val(abp.utils.round(leaseHaulerRateInput.val()).toFixed(2));
            }

            if (!abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnQuote')) {
                leaseHaulerRateInput.closest('.form-group').hide();
            }

            async function addNewLocation(newItemName) {
                var result = await app.getModalResultAsync(
                    createOrEditLocationModal.open({ mergeWithDuplicateSilently: true, name: newItemName })
                );
                return {
                    id: result.id,
                    name: result.displayName
                };
            }

            loadAtDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                addItemCallback: abp.auth.hasPermission('Pages.Locations') ? addNewLocation : undefined
            });
            deliverToDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                addItemCallback: abp.auth.hasPermission('Pages.Locations') ? addNewLocation : undefined
            });
            serviceDropdown.select2Init({
                abpServiceMethod: abp.services.app.service.getServicesSelectList,
                showAll: false,
                allowClear: false,
                addItemCallback: abp.auth.hasPermission('Pages.Services') ? async function (newItemName) {
                    var result = await app.getModalResultAsync(
                        createOrEditServiceModal.open({ name: newItemName })
                    );
                    return {
                        id: result.id,
                        name: result.service1
                    };
                } : undefined
            });
            vehicleCategoriesDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getVehicleCategoriesSelectList,
                showAll: true,
                allowClear: true
            });
            materialUomDropdown.select2Uom();
            freightUomDropdown.select2Uom();
            designationDropdown.select2Init({
                showAll: true,
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
                updateProductionPay();
            }).change();

            freightUomDropdown.change(function () {
                disableProductionPayIfNeeded(true);
                updateFreightRateForDriverPayVisibility(false);
            });

            _$form.find("#ProductionPay").change(updateFreightRateForDriverPayVisibility);

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
                freightRateToPayDriversInput.attr('disabled', 'disabled').val('0');
                freightUomDropdown.attr('disabled', 'disabled').val('').change();
                freightQuantityInput.attr('disabled', 'disabled').val('');
                freightRateInput.closest('.form-group').hide();
                freightRateToPayDriversInput.closest('.form-group').hide();
                freightUomDropdown.closest('.form-group').hide();
                freightQuantityInput.closest('.form-group').hide();
            }
            function enableFreightFields() {
                freightRateInput.removeAttr('disabled');
                freightUomDropdown.removeAttr('disabled');
                freightQuantityInput.removeAttr('disabled');
                freightRateInput.closest('.form-group').show();
                if (abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate')) {
                    freightRateToPayDriversInput.removeAttr('disabled');
                    freightRateToPayDriversInput.closest('.form-group').show();
                }
                freightUomDropdown.closest('.form-group').show();
                freightQuantityInput.closest('.form-group').show();
            }

            freightRateInput.change(function () {
                /* #12546: If the “Freight Rate to Pay Drivers” textbox isn’t being displayed, the FreightRateToPayDrivers 
                property should be updated to the same value as the “Freight Rate”.  
                When the “Freight Rate” is changed and the driver pay rate was the same as the prior “Freight Rate”, 
                the “Freight Rate to Pay Drivers” should be changed to be the same as the “Freight Rate”. 
                */
                var newFreightRate = Number(freightRateInput.val()) || 0;
                var freightRateToPayDrivers = Number(freightRateToPayDriversInput.val()) || 0;

                if (freightRateToPayDriversInput.css("display") == "none"
                    || freightRateToPayDriversInput.css("visibility") == "hidden"
                    || _ratesLastValue.freightRate !== newFreightRate && _ratesLastValue.freightRate === freightRateToPayDrivers
                ) {
                    freightRateToPayDriversInput.val(newFreightRate);
                }
                _ratesLastValue.freightRate = newFreightRate;

                recalculate($(this));
            });

            function disableProductionPayIfNeeded(forceUncheck) {
                if (!shouldDisableProductionPay()) {
                    enableProductionPay();
                } else {
                    let productionPayInput = _$form.find('#ProductionPay')

                    if (forceUncheck) {
                        productionPayInput.prop('checked', false);
                    } else {
                        if (productionPayInput.is(':checked')) {
                            return;
                        }
                    }

                    disableProductionPay();
                }
            }

            function shouldDisableProductionPay() {
                if (abp.setting.getBoolean('App.TimeAndPay.PreventProductionPayOnHourlyJobs')) {
                    let freightUom = freightUomDropdown.getSelectedDropdownOption().text();
                    if (['hours', 'hour'].includes((freightUom || '').toLowerCase())) {
                        return true;
                    }
                }
                return false;
            }

            function disableProductionPay() {
                let productionPayInput = _$form.find('#ProductionPay');
                productionPayInput.prop('disabled', true);
                productionPayInput.closest('label').attr('title', app.localize('PreventProductionPayOnHourlyJobsHint')).tooltip();
            }

            function enableProductionPay() {
                let productionPayInput = _$form.find('#ProductionPay');
                productionPayInput.prop('disabled', false);
                productionPayInput.closest('label').attr('title', '').tooltip('dispose');
            }

            function updateFreightRateForDriverPayVisibility() {
                if (abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate') && _$form.find('#ProductionPay').is(':checked')) {
                    freightRateToPayDriversInput.closest('.form-group').show();
                } else {
                    freightRateToPayDriversInput.val(freightRateInput.val()).change().closest('.form-group').hide();
                }
            
                updateLoadBasedVisibility();
            }

            function updateLoadBasedVisibility() {
                let freightUom = freightUomDropdown.getSelectedDropdownOption().text();
                if (abp.setting.getBoolean('App.TimeAndPay.AllowLoadBasedRates') && !(['hours', 'hour'].includes((freightUom || '').toLowerCase()))
                        && _$form.find('#ProductionPay').is(':checked')) {
                    _$form.find('#LoadBased').closest('.form-group').show();
                } else {
                    _$form.find('#LoadBased').prop('checked', false).closest('.form-group').hide();
                }
            }

            function updateProductionPay() {
                let productionPay = _$form.find('#ProductionPay');
                let productionPayContainer = productionPay.closest('.form-group');
                if (designationIsMaterialOnly()) {
                    if (_wasProductionPay === null) {
                        _wasProductionPay = productionPay.is(':checked');
                    }
                    productionPay.prop('checked', false);
                    productionPayContainer.hide();
                    updateFreightRateForDriverPayVisibility();
                } else {
                    if (_wasProductionPay !== null) {
                        if (shouldDisableProductionPay()) {
                            disableProductionPay();
                        } else {
                            productionPay.prop('checked', _wasProductionPay);
                        }
                        _wasProductionPay = null;
                    }
                    productionPayContainer.show();
                    updateFreightRateForDriverPayVisibility();
                }
            }

            disableProductionPayIfNeeded(false);
            updateFreightRateForDriverPayVisibility();
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
            quoteService.VehicleCategories = _$form.find("#VehicleCategories").select2('data').map(x => ({ id: x.id, name: x.name }));

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