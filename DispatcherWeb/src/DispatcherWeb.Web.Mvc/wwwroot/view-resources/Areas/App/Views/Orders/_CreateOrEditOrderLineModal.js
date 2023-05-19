(function ($) {
    app.modals.CreateOrEditOrderLineModal = function () {

        var _modalManager;
        var _orderAppService = abp.services.app.order;
        var _$form = null;
        var _quoteId = null;
        var _quoteServiceId = null;
        var _pricing = null;
        var _orderLine = null;
        var _initializing = false;
        var _recalculating = false;
        var _saveEventArgs = {
            reloadMaterialTotalIfNotOverridden: false,
            reloadFreightTotalIfNotOverridden: false
        };
        var _loadAtDropdown = null;
        var _deliverToDropdown = null;
        var _serviceDropdown = null;
        var _materialUomDropdown = null;
        var _freightUomDropdown = null;
        var _designationDropdown = null;
        var _vehicleCategoriesDropdown = null;
        var _isMaterialPricePerUnitOverriddenInput = null;
        var _isFreightPricePerUnitOverriddenInput = null;
        var _materialQuantityInput = null;
        var _freightQuantityInput = null;
        var _materialPricePerUnitInput = null;
        var _freightPricePerUnitInput = null;
        var _freightRateToPayDriversInput = null;
        var _materialPriceInput = null; //total for the item
        var _freightPriceInput = null; //total for the item
        var _numberOfTrucksInput = null;
        var _timeOnJobInput = null;
        var _isMaterialPriceOverriddenInput = null;
        var _isFreightPriceOverriddenInput = null;
        var _unlockMaterialPriceButton = null;
        var _unlockFreightPriceButton = null;
        var _wasProductionPay = null;

        var _ratesLastValue = {};

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

            var setStaggeredTimesModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Orders/SetStaggeredTimesModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Orders/_SetStaggeredTimesModal.js',
                modalClass: 'SetStaggeredTimesModal'
            });

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            abp.helper.ui.initControls();

            _timeOnJobInput = _$form.find("#TimeOnJob");

            _numberOfTrucksInput = _$form.find("#NumberOfTrucks");
            _numberOfTrucksInput.change(function () {
                updateStaggeredTimeControls();
            }).change();

            _$form.find("#SetStaggeredTimeButton").click(function () {
                var orderLineId = _$form.find("#Id").val();
                if (orderLineId) {
                    setStaggeredTimesModal.open({ orderLineId });
                } else {
                    _orderLine = _orderLine || {};
                    let model = {
                        orderLineId: null,
                        staggeredTimeKind: Number(_$form.find("#StaggeredTimeKind").val()),
                        staggeredTimeInterval: _orderLine.staggeredTimeInterval,
                        firstStaggeredTimeOnJob: _orderLine.firstStaggeredTimeOnJob
                    };
                    setStaggeredTimesModal.open({}).done(function (modal, modalObject) {
                        modalObject.setModel(model);
                        modalObject.saveCallback = function (model) {
                            _orderLine.updateStaggeredTime = true;
                            _orderLine.staggeredTimeKind = model.staggeredTimeKind;
                            _orderLine.staggeredTimeInterval = model.staggeredTimeInterval;
                            _orderLine.firstStaggeredTimeOnJob = model.firstStaggeredTimeOnJob;
                        };
                    });
                }
            }).tooltip();

            _modalManager.on('app.staggeredTimesSetModal', function (e) {
                _$form.find("#StaggeredTimeKind").val(e.staggeredTimeKind);
                updateTimeOnJobInput();
            });
            updateTimeOnJobInput();


            _quoteId = _$form.find("#QuoteId").val();
            _quoteServiceId = _$form.find("#QuoteServiceId").val();

            _loadAtDropdown = _$form.find("#LoadAtId");
            _deliverToDropdown = _$form.find("#DeliverToId");
            _serviceDropdown = _$form.find("#ServiceId");
            _materialUomDropdown = _$form.find("#MaterialUomId");
            _freightUomDropdown = _$form.find("#FreightUomId");
            _designationDropdown = _$form.find("#Designation");
            _vehicleCategoriesDropdown = _$form.find("#VehicleCategories");
            _isMaterialPricePerUnitOverriddenInput = _$form.find("#IsMaterialPricePerUnitOverridden");
            _isFreightPricePerUnitOverriddenInput = _$form.find("#IsFreightPricePerUnitOverridden");
            _materialQuantityInput = _$form.find("#MaterialQuantity");
            _freightQuantityInput = _$form.find("#FreightQuantity");
            _materialPricePerUnitInput = _$form.find("#MaterialPricePerUnit");
            _freightPricePerUnitInput = _$form.find("#FreightPricePerUnit");
            _freightRateToPayDriversInput = _$form.find("#FreightRateToPayDrivers");

            _ratesLastValue = {
                freightPricePerUnit: Number(_freightPricePerUnitInput.val()) || 0
            };

            _materialPriceInput = _$form.find("#MaterialPrice"); //total for item
            _freightPriceInput = _$form.find("#FreightPrice"); //total for item
            var leaseHaulerRateInput = _$form.find("#LeaseHaulerRate");
            _isMaterialPriceOverriddenInput = _$form.find("#IsMaterialPriceOverridden");
            _isFreightPriceOverriddenInput = _$form.find("#IsFreightPriceOverridden");

            _materialPriceInput.val(round(_materialPriceInput.val()).toFixed(2));
            _freightPriceInput.val(round(_freightPriceInput.val()).toFixed(2));
            if (leaseHaulerRateInput.val() !== "") {
                leaseHaulerRateInput.val(abp.utils.round(leaseHaulerRateInput.val()).toFixed(2));
            }

            if (!abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnOrder')) {
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

            _loadAtDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownLoadSite,
                addItemCallback: abp.auth.hasPermission('Pages.Locations') ? addNewLocation : undefined
            });
            _deliverToDropdown.select2Location({
                predefinedLocationCategoryKind: abp.enums.predefinedLocationCategoryKind.unknownDeliverySite,
                addItemCallback: abp.auth.hasPermission('Pages.Locations') ? addNewLocation : undefined
            });
            _serviceDropdown.select2Init({
                abpServiceMethod: abp.services.app.service.getServicesWithTaxInfoSelectList,
                showAll: false,
                allowClear: false,
                addItemCallback: abp.auth.hasPermission('Pages.Services') ? async function (newItemName) {
                    var result = await app.getModalResultAsync(
                        createOrEditServiceModal.open({ name: newItemName })
                    );
                    _$form.find("#IsTaxable").val(result.isTaxable ? "True" : "False");
                    return {
                        id: result.id,
                        name: result.service1
                    };
                } : undefined
            });
            _materialUomDropdown.select2Uom();
            _freightUomDropdown.select2Uom();
            _designationDropdown.select2Init({
                showAll: true,
                allowClear: false
            });
            _vehicleCategoriesDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getVehicleCategoriesSelectList,
                showAll: true,
                allowClear: true
            });

            _designationDropdown.change(function () {
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

            _$form.find("#RequiresCustomerNotification").change(updateCustomerNotificationControlsVisibility);

            reloadPricing();
            refreshHighlighting();

            _serviceDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                var dropdownData = _serviceDropdown.select2('data');
                if (dropdownData && dropdownData.length && dropdownData[0].item) {
                    _$form.find("#IsTaxable").val(dropdownData[0].item.isTaxable ? "True" : "False");
                }
            });
            _materialUomDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
            });
            _freightUomDropdown.change(function () {
                var sender = $(this);
                reloadPricing(function () {
                    recalculate(sender);
                });
                disableProductionPayIfNeeded(true);
            });

            abp.helper.ui.syncUomDropdowns(_materialUomDropdown, _freightUomDropdown, _designationDropdown, _materialQuantityInput, _freightQuantityInput);

            _materialQuantityInput.change(function () {
                recalculate($(this));
            });
            _freightQuantityInput.change(function () {
                recalculate($(this));
            });
            _materialPricePerUnitInput.change(function () {
                recalculate($(this));
            });
            _freightPricePerUnitInput.change(function () {
                /* #12546: If the “Freight Rate to Pay Drivers” textbox isn’t being displayed, the FreightRateToPayDrivers 
                property should be updated to the same value as the “Freight Rate”.  
                When the “Freight Rate” is changed and the driver pay rate was the same as the prior “Freight Rate”, 
                the “Freight Rate to Pay Drivers” should be changed to be the same as the “Freight Rate”. 
                */
                var newFreightPricePerUnit = Number(_freightPricePerUnitInput.val()) || 0;
                var freightRateToPayDrivers = Number(_freightRateToPayDriversInput.val()) || 0;

                if (_freightRateToPayDriversInput.css("display") == "none"
                    || _freightRateToPayDriversInput.css("visibility") == "hidden"
                    || _ratesLastValue.freightPricePerUnit !== newFreightPricePerUnit && _ratesLastValue.freightPricePerUnit === freightRateToPayDrivers
                ) {
                    _freightRateToPayDriversInput.val(newFreightPricePerUnit);
                }
                _ratesLastValue.freightPricePerUnit = newFreightPricePerUnit;

                recalculate($(this));
            });

            _materialPriceInput.change(function () {
                _saveEventArgs.reloadMaterialTotalIfNotOverridden = true;
                setIsMaterialPriceOverridden(true);
            });
            _freightPriceInput.change(function () {
                _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
                setIsFreightPriceOverridden(true);
            });
            initOverrideButtons();
            disableProductionPayIfNeeded(false);
            disableQuoteRelatedFieldsIfNeeded();
            updateCustomerNotificationControlsVisibility();
        };

        function disableQuoteRelatedFieldsIfNeeded() {
            if (_quoteId) {
                _designationDropdown
                    .add(_loadAtDropdown)
                    //.add(_deliverToDropdown)
                    .add(_serviceDropdown)
                    .add(_freightUomDropdown)
                    .add(_materialUomDropdown)
                    //.add(_freightPricePerUnitInput)
                    //.add(_materialPricePerUnitInput)
                    //.add(_freightQuantityInput)
                    //.add(_materialQuantityInput)
                    .prop('disabled', true);
            }
        }

        function updateCustomerNotificationControlsVisibility() {
            _$form.find("#CustomerNotificationContactName").closest('.form-group').toggle(_$form.find('#RequiresCustomerNotification').is(':checked'));
            _$form.find("#CustomerNotificationPhoneNumber").closest('.form-group').toggle(_$form.find('#RequiresCustomerNotification').is(':checked'));
        }

        function initOverrideButtons() {
            _unlockMaterialPriceButton = _$form.find("#UnlockMaterialTotalButton");
            _unlockMaterialPriceButton.click(function () {
                setIsMaterialPriceOverridden(!getIsMaterialPriceOverridden());
                refreshTotalFields();
                refreshOverrideButtons();
                refreshHighlighting();
                if (getIsMaterialPriceOverridden()) {
                    _$form.find("#IsMultipleLoads").prop('checked', false).change();
                }
            });
            _unlockFreightPriceButton = _$form.find("#UnlockFreightTotalButton");
            _unlockFreightPriceButton.click(function () {
                setIsFreightPriceOverridden(!getIsFreightPriceOverridden());
                refreshTotalFields();
                refreshOverrideButtons();
                refreshHighlighting();
                if (getIsFreightPriceOverridden()) {
                    _$form.find("#IsMultipleLoads").prop('checked', false).change();
                }
            });
            refreshOverrideButtons();
        }

        function refreshTotalFields() {
            if (getIsMaterialPriceOverridden()) {
                _materialPriceInput.prop('disabled', false);
            } else {
                _materialPriceInput.prop('disabled', true);
                recalculate(_materialQuantityInput);
            }
            if (getIsFreightPriceOverridden()) {
                _freightPriceInput.prop('disabled', false);
            } else {
                _freightPriceInput.prop('disabled', true);
                recalculate(_freightQuantityInput);
            }
        }

        function refreshOverrideButtons() {
            refreshOverrideButton(_unlockMaterialPriceButton, getIsMaterialPriceOverridden());
            refreshOverrideButton(_unlockFreightPriceButton, getIsFreightPriceOverridden());
        }

        function refreshOverrideButton(button, isOverridden) {
            button
                .attr('title', app.localize(isOverridden ? "RemoveOverriddenValue" : "OverrideAutocalculatedValue"))
                .find('.fas').removeClass('fa-unlock fa-lock').addClass(isOverridden ? 'fa-unlock' : 'fa-lock');
        }

        function reloadPricing(callback) {
            if (_initializing) {
                return;
            }
            if (_serviceDropdown.val() === ''
                || _materialUomDropdown.val() === '' && _freightUomDropdown.val() === '') {
                _pricing = null;
                refreshHighlighting();
                if (callback)
                    callback();
                return;
            }
            abp.services.app.service.getServicePricing({
                serviceId: _serviceDropdown.val(),
                materialUomId: _materialUomDropdown.val(),
                freightUomId: _freightUomDropdown.val(),
                quoteServiceId: _quoteServiceId
            }).done(function (pricing) {
                _pricing = pricing;
                refreshHighlighting();
                if (callback)
                    callback();
            });
        }

        function refreshHighlighting() {
            if (_pricing && _pricing.quoteBasedPricing) {
                _serviceDropdown.addClass("quote-based-pricing");
                _materialUomDropdown.addClass("quote-based-pricing");
                _freightUomDropdown.addClass("quote-based-pricing");
            } else {
                _serviceDropdown.removeClass("quote-based-pricing");
                _materialUomDropdown.removeClass("quote-based-pricing");
                _freightUomDropdown.removeClass("quote-based-pricing");
            }

            if (getIsMaterialPricePerUnitOverridden()) {
                _materialPricePerUnitInput.addClass("overridden-price");
            } else {
                _materialPricePerUnitInput.removeClass("overridden-price");
            }

            if (getIsFreightPricePerUnitOverridden()) {
                _freightPricePerUnitInput.addClass("overridden-price");
            } else {
                _freightPricePerUnitInput.removeClass("overridden-price");
            }

            if (getIsMaterialPriceOverridden()) {
                _materialPriceInput.addClass("overridden-price");
            } else {
                _materialPriceInput.removeClass("overridden-price");
            }

            if (getIsFreightPriceOverridden()) {
                _freightPriceInput.addClass("overridden-price");
            } else {
                _freightPriceInput.removeClass("overridden-price");
            }
        }

        function getIsFreightPricePerUnitOverridden() {
            return _isFreightPricePerUnitOverriddenInput.val() === "True";
        }

        function setIsFreightPricePerUnitOverridden(val) {
            _isFreightPricePerUnitOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialPricePerUnitOverridden() {
            return _isMaterialPricePerUnitOverriddenInput.val() === "True";
        }

        function setIsMaterialPricePerUnitOverridden(val) {
            _isMaterialPricePerUnitOverriddenInput.val(val ? "True" : "False");
        }

        function getIsFreightPriceOverridden() {
            return _isFreightPriceOverriddenInput.val() === "True";
        }

        function setIsFreightPriceOverridden(val) {
            _isFreightPriceOverriddenInput.val(val ? "True" : "False");
        }

        function getIsMaterialPriceOverridden() {
            return _isMaterialPriceOverriddenInput.val() === "True";
        }

        function setIsMaterialPriceOverridden(val) {
            _isMaterialPriceOverriddenInput.val(val ? "True" : "False");
        }

        function updateTimeOnJobInput() {
            if (Number(_$form.find("#StaggeredTimeKind").val()) > 0) {
                disableTimeOnJobInput();
            } else {
                enableTimeOnJobInput();
            }
        }

        function enableTimeOnJobInput() {
            if (_timeOnJobInput.val() === 'Staggered') {
                _timeOnJobInput.val('');
            }
            _timeOnJobInput.prop('disabled', false).timepickerInit({ stepping: 1 });
        }

        function disableTimeOnJobInput() {
            var timeOnJobTimepicker = _timeOnJobInput.data('DateTimePicker');
            timeOnJobTimepicker && timeOnJobTimepicker.destroy();
            _timeOnJobInput.prop('disabled', true).val('Staggered');
        }

        function updateStaggeredTimeControls() {
            var isStaggeredTimeButtonVisible = Number(_numberOfTrucksInput.val()) > 1;
            _$form.find("#SetStaggeredTimeButton").closest('.input-group-btn').toggle(isStaggeredTimeButtonVisible);
            if (!isStaggeredTimeButtonVisible && Number(_$form.find('#StaggeredTimeKind').val()) > 0) {
                _$form.find('#StaggeredTimeKind').val("0");
                enableTimeOnJobInput();
            }
        }

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
                let freightUom = _freightUomDropdown.getSelectedDropdownOption().text();
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

        function setFreightRateFromPricingIfNeeded(rate, sender) {
            if (getIsFreightPricePerUnitOverridden() || designationIsMaterialOnly()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _freightPricePerUnitInput.val()) {
                return;
            }
            _freightPricePerUnitInput.val(rate).change();
        }

        function setMaterialRateFromPricingIfNeeded(rate, sender) {
            if (getIsMaterialPricePerUnitOverridden() || !designationHasMaterial()) {
                return;
            }
            //when quantity changes, don't reset the rate from pricing unless the rate was empty
            if ((sender.is(_materialQuantityInput) || sender.is(_freightQuantityInput)) && _materialPricePerUnitInput.val()) {
                return;
            }
            _materialPricePerUnitInput.val(rate);
        }

        function recalculate(sender) {
            if (_initializing || _recalculating) {
                return;
            }
            _recalculating = true;

            var freightRatePricing =
                _pricing && _pricing.quoteBasedPricing && _pricing.quoteBasedPricing.freightRate !== null ? _pricing.quoteBasedPricing.freightRate
                    : _pricing && _pricing.hasPricing && _pricing.freightRate !== null ? _pricing.freightRate
                        : null;

            var materialRatePricing =
                _pricing && _pricing.quoteBasedPricing && _pricing.quoteBasedPricing.pricePerUnit !== null ? _pricing.quoteBasedPricing.pricePerUnit
                    : _pricing && _pricing.hasPricing && _pricing.pricePerUnit !== null ? _pricing.pricePerUnit
                        : null;

            if (freightRatePricing !== null) {
                if (sender.is(_freightPricePerUnitInput)) {
                    setIsFreightPricePerUnitOverridden(freightRatePricing !== Number(_freightPricePerUnitInput.val()));
                } else {
                    setFreightRateFromPricingIfNeeded(freightRatePricing, sender);
                }
            } else {
                //no freight pricing
                if (!getIsFreightPricePerUnitOverridden() && (sender.is(_freightUomDropdown) || sender.is(_serviceDropdown))) {
                    _freightPricePerUnitInput.val('').change();
                }
            }

            if (materialRatePricing !== null) {
                if (sender.is(_materialPricePerUnitInput)) {
                    setIsMaterialPricePerUnitOverridden(materialRatePricing !== Number(_materialPricePerUnitInput.val()));
                } else {
                    setMaterialRateFromPricingIfNeeded(materialRatePricing, sender);
                }
            } else {
                //no material pricing
                if (!getIsMaterialPricePerUnitOverridden() && (sender.is(_materialUomDropdown) || sender.is(_serviceDropdown))) {
                    _materialPricePerUnitInput.val('');
                }
            }
            var materialPricePerUnit = _materialPricePerUnitInput.val();
            var freightPricePerUnit = _freightPricePerUnitInput.val();
            var materialQuantity = _materialQuantityInput.val();
            var freightQuantity = _freightQuantityInput.val();
            var materialPrice = round(materialPricePerUnit * materialQuantity);
            var freightPrice = round(freightPricePerUnit * freightQuantity);
            if (!getIsMaterialPriceOverridden()) {
                _materialPriceInput.val(materialPrice.toFixed(2));
            }
            if (!getIsFreightPriceOverridden()) {
                _freightPriceInput.val(freightPrice.toFixed(2));
            }
            refreshHighlighting();
            _saveEventArgs.reloadMaterialTotalIfNotOverridden = true;
            _saveEventArgs.reloadFreightTotalIfNotOverridden = true;
            _recalculating = false;
        }

        function round(num) {
            return abp.utils.round(num);
        }

        function designationHasMaterial() {
            var designation = Number(_designationDropdown.val());
            return abp.enums.designations.hasMaterial.includes(designation);
        }
        function designationIsMaterialOnly() {
            return abp.enums.designations.materialOnly.includes(Number(_designationDropdown.val()));
        }

        function disableMaterialFields() {
            _$form.find("label[for=MaterialUomId]").removeClass('required-label');
            _$form.find('#MaterialPricePerUnit').val('').closest('.form-group').hide();
            _$form.find('#MaterialPrice').val('0').closest('.form-group').hide();
            _$form.find('#MaterialUomId').val('').change().closest('.form-group').hide();
            _$form.find('#MaterialQuantity').val('').closest('.form-group').hide();
        }
        function enableMaterialFields() {
            _$form.find("label[for=MaterialUomId]").addClass('required-label');
            _$form.find('#MaterialPricePerUnit').closest('.form-group').show();
            _$form.find('#MaterialPrice').closest('.form-group').show();
            _$form.find('#MaterialUomId').closest('.form-group').show();
            _$form.find('#MaterialQuantity').closest('.form-group').show();
        }
        function disableFreightFields() {
            _$form.find("label[for=FreightUomId]").removeClass('required-label');
            _$form.find('#FreightPricePerUnit').val('').closest('.form-group').hide();
            _$form.find('#FreightRateToPayDrivers').val('').closest('.form-group').hide();
            _$form.find('#FreightPrice').val('0').closest('.form-group').hide();
            _$form.find('#FreightUomId').val('').change().closest('.form-group').hide();
            _$form.find('#FreightQuantity').val('').closest('.form-group').hide();
        }
        function enableFreightFields() {
            _$form.find("label[for=FreightUomId]").addClass('required-label');
            _$form.find('#FreightPricePerUnit').closest('.form-group').show();
            if (abp.setting.getBoolean('App.TimeAndPay.AllowDriverPayRateDifferentFromFreightRate')) {
                _$form.find('#FreightRateToPayDrivers').closest('.form-group').show();
            }
            _$form.find('#FreightPrice').closest('.form-group').show();
            _$form.find('#FreightUomId').closest('.form-group').show();
            _$form.find('#FreightQuantity').closest('.form-group').show();
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
            }
        }

        this.save = async function () {
            if (!_$form.valid()) {
                _$form.showValidateMessage();
                return;
            }

            var orderLine = _$form.serializeFormToObject();
            orderLine.Id = orderLine.Id ? Number(orderLine.Id) : null;
            orderLine.OrderId = orderLine.OrderId ? Number(orderLine.OrderId) : 0;
            orderLine.QuoteId = orderLine.QuoteId ? Number(orderLine.QuoteId) : null;
            orderLine.QuoteServiceId = orderLine.QuoteServiceId ? Number(orderLine.QuoteServiceId) : null;

            if (!parseFloat(orderLine.MaterialQuantity) && parseFloat(orderLine.MaterialPrice)
                || !parseFloat(orderLine.FreightQuantity) && parseFloat(orderLine.FreightPrice)) {
                abp.message.error(app.localize('QuantityIsRequiredWhenTotalIsSpecified'));
                return;
            }

            if (orderLine.RequiresCustomerNotification && (orderLine.CustomerNotificationContactName === "" || orderLine.CustomerNotificationPhoneNumber === "")) {
                abp.message.error('Please check the following: \n'
                    + (orderLine.CustomerNotificationContactName ? '' : '"Contact Name" - This field is required.\n')
                    + (orderLine.CustomerNotificationPhoneNumber ? '' : '"Phone Number" - This field is required.\n'), 'Some of the data is invalid');
                return;
            }

            if (orderLine.CustomerNotificationPhoneNumber) {
                if (!_$form.find("#CustomerNotificationPhoneNumber")[0].checkValidity()) {
                    abp.message.error(app.localize('IncorrectPhoneNumberFormatError'));
                    return;
                }
            }

            if (!validateFields(orderLine)) {
                return;
            }

            if (Number(orderLine.StaggeredTimeKind) !== abp.enums.staggeredTimeKind.none) {
                orderLine.TimeOnJob = null;
            }

            _orderLine = _orderLine || {};
            _orderLine.id = orderLine.Id;
            _orderLine.orderId = orderLine.OrderId;
            _orderLine.quoteId = orderLine.QuoteId;
            _orderLine.quoteServiceId = orderLine.QuoteServiceId;
            _orderLine.isMaterialPricePerUnitOverridden = orderLine.IsMaterialPricePerUnitOverridden === "True";
            _orderLine.isFreightPricePerUnitOverridden = orderLine.IsFreightPricePerUnitOverridden === "True";
            _orderLine.isMaterialPriceOverridden = orderLine.IsMaterialPriceOverridden === "True";
            _orderLine.isFreightPriceOverridden = orderLine.IsFreightPriceOverridden === "True";
            _orderLine.isTaxable = orderLine.IsTaxable === "True";
            _orderLine.staggeredTimeKind = Number(orderLine.StaggeredTimeKind) || 0;
            _orderLine.lineNumber = Number(orderLine.LineNumber);
            _orderLine.designation = orderLine.Designation;
            _orderLine.designationName = Number(orderLine.Designation) ? _$form.find("#Designation option:selected").text() : null;
            _orderLine.loadAtId = orderLine.LoadAtId;
            _orderLine.loadAtName = Number(orderLine.LoadAtId) ? _$form.find("#LoadAtId option:selected").text() : null;
            _orderLine.deliverToId = orderLine.DeliverToId;
            _orderLine.deliverToName = Number(orderLine.DeliverToId) ? _$form.find("#DeliverToId option:selected").text() : null;
            _orderLine.serviceId = orderLine.ServiceId;
            _orderLine.serviceName = Number(orderLine.ServiceId) ? _$form.find("#ServiceId option:selected").text() : null;
            _orderLine.materialUomId = orderLine.MaterialUomId;
            _orderLine.materialUomName = Number(orderLine.MaterialUomId) ? _$form.find("#MaterialUomId option:selected").text() : null;
            _orderLine.freightUomId = orderLine.FreightUomId;
            _orderLine.freightUomName = Number(orderLine.FreightUomId) ? _$form.find("#FreightUomId option:selected").text() : null;
            _orderLine.materialPricePerUnit = Number(orderLine.MaterialPricePerUnit) || 0;
            _orderLine.freightPricePerUnit = Number(orderLine.FreightPricePerUnit) || 0;
            _orderLine.freightRateToPayDrivers = Number(orderLine.FreightRateToPayDrivers) || 0;
            _orderLine.leaseHaulerRate = Number(orderLine.LeaseHaulerRate) || 0;
            _orderLine.materialQuantity = Number(orderLine.MaterialQuantity) || 0;
            _orderLine.freightQuantity = Number(orderLine.FreightQuantity) || 0;
            _orderLine.materialPrice = Number(orderLine.MaterialPrice) || 0;
            _orderLine.freightPrice = Number(orderLine.FreightPrice) || 0;
            _orderLine.numberOfTrucks = Number(orderLine.NumberOfTrucks) || 0;
            _orderLine.isMultipleLoads = !!orderLine.IsMultipleLoads;
            _orderLine.productionPay = !!orderLine.ProductionPay;
            _orderLine.timeOnJob = orderLine.TimeOnJob;
            _orderLine.jobNumber = orderLine.JobNumber;
            _orderLine.note = orderLine.Note;
            _orderLine.requiresCustomerNotification = !!orderLine.RequiresCustomerNotification;
            _orderLine.customerNotificationContactName = orderLine.CustomerNotificationContactName;
            _orderLine.customerNotificationPhoneNumber = orderLine.CustomerNotificationPhoneNumber;
            _orderLine.vehicleCategories = _$form.find("#VehicleCategories").select2('data').map(x => ({ id: x.id, name: x.title }));

            if (this.saveCallback) {
                this.saveCallback(_orderLine);
            }

            if (!orderLine.Id && !orderLine.OrderId) {
                _modalManager.close();
                abp.event.trigger('app.createOrEditOrderLineModalSaved', {});
                return;
            }

            let materialQuantity = orderLine.MaterialQuantity === "" ? null : abp.utils.round(parseFloat(orderLine.MaterialQuantity));
            let freightQuantity = orderLine.FreightQuantity === "" ? null : abp.utils.round(parseFloat(orderLine.FreightQuantity));
            let numberOfTrucks = orderLine.NumberOfTrucks === "" ? null : abp.utils.round(parseFloat(orderLine.NumberOfTrucks));

            if (orderLine.Id && !await abp.scheduling.checkExistingDispatchesBeforeSettingQuantityAndNumberOfTrucksZero(orderLine.Id, materialQuantity, freightQuantity, numberOfTrucks)) {
                _modalManager.close();
                return;
            }

            // #8363
            //if (orderLine.Id) {
            //    _orderAppService.getOrderLineUtilization(orderLine.Id)
            //        .done(function (utilization) {
            //            if (orderLine.NumberOfTrucks < utilization) {
            //                abp.message.warn(app.localize('RemoveSomeTrucks'));
            //            } else {
            //                save();
            //            }
            //        });
            //} else {
            //    save();
            //}

            _modalManager.setBusy(true);
            _orderAppService.editOrderLine(_orderLine).done(function (result) {
                abp.notify.info('Saved successfully.');
                _$form.find("#Id").val(result.orderLineId);
                _modalManager.close();
                abp.event.trigger('app.createOrEditOrderLineModalSaved', result);
            }).always(function () {
                _modalManager.setBusy(false);
            });
        }

        function validateFields(orderLine) {
            var isFreightUomValid = true;
            if (!designationIsMaterialOnly()) {
                if (!Number(orderLine.FreightUomId)) {
                    isFreightUomValid = false;
                }
            }

            var isMaterialUomValid = true;
            if (designationHasMaterial()) {
                if (!Number(orderLine.MaterialUomId)) {
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

        this.setOrderLine = function (orderLine) {
            _orderLine = orderLine;
            if (!_$form) {
                return;
            }
            _initializing = true;
            _$form.find("#Id").val(_orderLine.id);
            _$form.find("#OrderId").val(_orderLine.orderId);
            _$form.find("#QuoteId").val(_orderLine.quoteId);
            _$form.find("#QuoteServiceId").val(_orderLine.quoteServiceId);
            _$form.find("#IsMaterialPricePerUnitOverridden").val(_orderLine.isMaterialPricePerUnitOverridden ? "True" : "False");
            _$form.find("#IsFreightPricePerUnitOverridden").val(_orderLine.isFreightPricePerUnitOverridden ? "True" : "False");
            _$form.find("#IsMaterialPriceOverridden").val(_orderLine.isMaterialPriceOverridden ? "True" : "False");
            _$form.find("#IsFreightPriceOverridden").val(_orderLine.isFreightPriceOverridden ? "True" : "False");
            _$form.find("#IsTaxable").val(_orderLine.isTaxable ? "True" : "False");
            _$form.find("#StaggeredTimeKind").val(_orderLine.staggeredTimeKind);
            _$form.find("#LineNumber").val(_orderLine.lineNumber);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#Designation"), _orderLine.designation, _orderLine.designationName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#LoadAtId"), _orderLine.loadAtId, _orderLine.loadAtName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#DeliverToId"), _orderLine.deliverToId, _orderLine.deliverToName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#ServiceId"), _orderLine.serviceId, _orderLine.serviceName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#MaterialUomId"), _orderLine.materialUomId, _orderLine.materialUomName);
            abp.helper.ui.addAndSetDropdownValue(_$form.find("#FreightUomId"), _orderLine.freightUomId, _orderLine.freightUomName);
            _$form.find("#MaterialPricePerUnit").val(_orderLine.materialPricePerUnit);
            _$form.find("#FreightPricePerUnit").val(_orderLine.freightPricePerUnit);
            _ratesLastValue.freightPricePerUnit = Number(_orderLine.freightPricePerUnit) || 0;
            _$form.find("#FreightRateToPayDrivers").val(_orderLine.freightRateToPayDrivers);
            _$form.find("#LeaseHaulerRate").val(_orderLine.leaseHaulerRate);
            _$form.find("#MaterialQuantity").val(_orderLine.materialQuantity);
            _$form.find("#FreightQuantity").val(_orderLine.freightQuantity);
            _$form.find("#MaterialPrice").val(_orderLine.materialPrice);
            _$form.find("#FreightPrice").val(_orderLine.freightPrice);
            _$form.find("#NumberOfTrucks").val(_orderLine.numberOfTrucks);
            _$form.find("#IsMultipleLoads").prop('checked', _orderLine.isMultipleLoads);
            _$form.find("#ProductionPay").prop('checked', _orderLine.productionPay);
            _$form.find("#TimeOnJob").val(_orderLine.timeOnJob);
            _$form.find("#JobNumber").val(_orderLine.jobNumber);
            _$form.find("#Note").val(_orderLine.note);

            _quoteId = _$form.find("#QuoteId").val();
            _quoteServiceId = _$form.find("#QuoteServiceId").val();

            updateStaggeredTimeControls();
            updateTimeOnJobInput();
            updateProductionPay();
            disableProductionPayIfNeeded(false);
            disableQuoteRelatedFieldsIfNeeded();

            _modalManager.getModal().find('.modal-title').text(orderLine.isNew ? "Add new line" : "Edit line");
            _initializing = false;
            reloadPricing();

            refreshTotalFields();
            refreshOverrideButtons();
            refreshHighlighting();
        };

        this.saveCallback = null;

    };
})(jQuery);