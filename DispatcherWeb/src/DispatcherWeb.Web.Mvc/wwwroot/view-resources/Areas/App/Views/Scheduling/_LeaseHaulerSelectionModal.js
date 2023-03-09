(function ($) {
    app.modals.LeaseHaulerSelectionModal = function () {

        var _modalManager;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _cache = {
            leaseHaulers: [],
            trucks: {},
            drivers: {}
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            _$form.find("#AddAllTrucksRadioButton").click(function () {
                _$form.find("#TruckSelectionBlock").slideUp();
            });

            _$form.find("#AddSpecificTrucksRadioButton").click(function () {
                _$form.find("#TruckSelectionBlock").slideDown();
            });

            function addRow() {
                var newRow = getTemplateRow().clone();
                newRow.attr('data-id', 0);
                newRow.appendTo(_$form.find("#TruckSelectionBlock"));
                newRow.slideDown();
                return newRow;
            }

            function addRowIfEmpty() {
                if (!_$form.find(".truck-selection-row[data-id!='']").length) {
                    addRow();
                }
            }

            function getTemplateRow() {
                return _$form.find(".truck-selection-row[data-id='']");
            }

            _$form.find("#AddTruckSelectionRow").click(function () {
                addRow();
            });

            _$form.on('click', '.delete-truck-selection-row-button', async function () {
                var row = $(this).closest('.truck-selection-row');
                if (!await abp.message.confirm('Are you sure you want to revoke the share for the truck for selected date?')) {
                    return;
                }

                var truckDropdown = row.find('.truck-select');
                if (!await confirmTruckRemove(truckDropdown)) {
                    return;
                }

                row.slideUp({
                    complete: function () {
                        row.remove();
                        addRowIfEmpty();
                    }
                });
            });

            function addTrucksToCache(records) {
                if (records && records.length) {
                    for (var i = 0; i < records.length; i++) {
                        var truck = records[i];
                        if (!_cache.trucks[truck.leaseHaulerId]) {
                            _cache.trucks[truck.leaseHaulerId] = [];
                        }
                        _cache.trucks[truck.leaseHaulerId].push(truck);
                    }
                }
            }

            function addDriversToCache(records) {
                if (records && records.length) {
                    for (var i = 0; i < records.length; i++) {
                        var driver = records[i];
                        if (!_cache.drivers[driver.leaseHaulerId]) {
                            _cache.drivers[driver.leaseHaulerId] = [];
                        }
                        _cache.drivers[driver.leaseHaulerId].push(driver);
                    }
                }
            }

            function populateLeaseHaulerDropdownWithRecords(dropdown, records) {
                if (records && records.length) {
                    _cache.leaseHaulers = records;
                    for (var i = 0; i < records.length; i++) {
                        var leaseHauler = records[i];
                        $('<option>').attr('value', leaseHauler.id).text(leaseHauler.name).appendTo(dropdown);
                    }
                }
            }

            function populateTruckDropdownWithRecords(dropdown, records) {
                dropdown.find('option[value!=""]').remove();
                if (records && records.length) {
                    for (var i = 0; i < records.length; i++) {
                        var truck = records[i];
                        $('<option>').attr('value', truck.truckId).attr('default-driver-id', truck.defaultDriverId).data('truck', truck).text(truck.truckCode).appendTo(dropdown);
                    }
                }
            }

            function populateDriverDropdownWithRecords(dropdown, records) {
                dropdown.find('option[value!=""]').remove();
                if (records && records.length) {
                    for (var i = 0; i < records.length; i++) {
                        var driver = records[i];
                        $('<option>').attr('value', driver.driverId).text(driver.driverName).appendTo(dropdown);
                    }
                }
            }

            function populateTruckDropdownFromCacheOrServer(dropdown, leaseHaulerId, callback) {
                if (!leaseHaulerId) {
                    populateTruckDropdownWithRecords(dropdown, null);
                    return;
                }
                if (_cache.trucks[leaseHaulerId]) {
                    populateTruckDropdownWithRecords(dropdown, _cache.trucks[leaseHaulerId]);
                    callback && callback();
                } else {
                    _modalManager.setBusy(true);
                    _schedulingService.getLeaseHaulerTrucks({ ids: [leaseHaulerId] }).done(function (records) {
                        addTrucksToCache(records);
                        populateTruckDropdownWithRecords(dropdown, records);
                        callback && callback();
                    }).always(function () {
                        _modalManager.setBusy(false);
                    });
                }
            }

            function populateDriverDropdownFromCacheOrServer(dropdown, leaseHaulerId, callback) {
                if (!leaseHaulerId) {
                    populateTruckDropdownWithRecords(dropdown, null);
                    return;
                }
                if (_cache.drivers[leaseHaulerId]) {
                    populateDriverDropdownWithRecords(dropdown, _cache.drivers[leaseHaulerId]);
                    callback && callback();
                } else {
                    _modalManager.setBusy(true);
                    _schedulingService.getLeaseHaulerDrivers({ ids: [leaseHaulerId] }).done(function (records) {
                        addDriversToCache(records);
                        populateDriverDropdownWithRecords(dropdown, records);
                        callback && callback();
                    }).always(function () {
                        _modalManager.setBusy(false);
                    });
                }
            }

            function getFilterData() {
                return {
                    OfficeId: _$form.find('#OfficeId').val(),
                    Date: _$form.find('#Date').val(),
                    Shift: _$form.find('#Shift').val()
                };
            }

            _schedulingService.getLeaseHaulerSelectionModel(getFilterData()).done(function (model) {
                populateLeaseHaulerDropdownWithRecords(getTemplateRow().find('.lease-hauler-select'), model.leaseHaulers);
                addTrucksToCache(model.trucks);
                addDriversToCache(model.drivers);

                if (model.rows && model.rows.length) {
                    for (var i = 0; i < model.rows.length; i++) {
                        var rowData = model.rows[i];
                        var rowElement = addRow();
                        rowElement.attr('data-id', rowData.id);
                        rowElement.find('.lease-hauler-select').val(rowData.leaseHaulerId);
                        var truckDropdown = rowElement.find('.truck-select');
                        var driverDropdown = rowElement.find('.driver-select');
                        populateTruckDropdownFromCacheOrServer(truckDropdown, rowData.leaseHaulerId, function () {
                            truckDropdown.val(rowData.truckId);
                            populateDriverDropdownFromCacheOrServer(driverDropdown, rowData.leaseHaulerId, function () {
                                driverDropdown.val(rowData.driverId);
                            });
                        });
                    }
                }
                addRowIfEmpty();
            });

            async function confirmTruckRemove($select) {
                if ($select.attr('data-truck-isinuse') === 'True') {
                    if (!await confirmTruckChangeOrRemovePopup()) {
                        return false;
                    }
                    $select.removeAttr('data-truck-isinuse').removeAttr('data-truck-originalid');
                }
                return true;
            }

            async function confirmTruckChange($select) {
                if ($select.attr('data-truck-isinuse') === 'True' && $select.attr('data-truck-originalid') !== $select.val()) {
                    if (!await confirmTruckChangeOrRemovePopup()) {
                        $select.val($select.attr('data-truck-originalid')).change();
                        return false;
                    }
                    $select.removeAttr('data-truck-isinuse').removeAttr('data-truck-originalid');
                }
                return true;
            }

            async function confirmTruckChangeOrRemovePopup() {
                return await abp.message.confirm('This truck has already been scheduled and will be removed from the schedule if you click Yes.');
            }

            _$form.on('change', '.lease-hauler-select', async function () {
                var row = $(this).closest('.truck-selection-row');
                var leaseHaulerId = parseInt($(this).val());
                var truckDropdown = row.find('.truck-select');
                var driverDropdown = row.find('.driver-select');
                if (await confirmTruckChange(truckDropdown)) {
                    populateTruckDropdownFromCacheOrServer(truckDropdown, leaseHaulerId, function () {
                        populateDriverDropdownFromCacheOrServer(driverDropdown, leaseHaulerId);
                    });
                };
            });

            _$form.on('change', '.truck-select', function () {
                var row = $(this).closest('.truck-selection-row');
                var defaultDriverId = $(this).find('option[value="' + $(this).val() + '"]').attr('default-driver-id');
                var driverDropdown = row.find('.driver-select');
                driverDropdown.val(defaultDriverId);
            });
        };

        this.save = function () {


            var formData = _$form.serializeFormToObject();

            formData = {
                Date: formData.Date,
                Shift: formData.Shift,
                OfficeId: formData.OfficeId,
                AddAllTrucks: formData.AddAllTrucks === "1"
            };

            if (!formData.AddAllTrucks) {

                var hasValidationErrors = false;
                formData.Rows = [];
                var rows = _$form.find(".truck-selection-row[data-id!='']");
                rows.each(function () {
                    var rowElement = $(this);
                    var rowData = {
                        id: rowElement.attr('data-id'),
                        leaseHaulerId: rowElement.find('.lease-hauler-select').val(),
                        truckId: rowElement.find('.truck-select').val(),
                        driverId: rowElement.find('.driver-select').val()
                    };
                    if (!rowData.leaseHaulerId && !rowData.truckId && !rowData.driverId) {
                        return;
                    }
                    if (!rowData.leaseHaulerId || !rowData.truckId || !rowData.driverId) {
                        hasValidationErrors = true;
                        return;
                    }
                    formData.Rows.push(rowData);
                });

                if (hasValidationErrors) {
                    abp.message.error('Lease Hauler, Truck and Driver are required fields');
                    return;
                }
            }

            _modalManager.setBusy(true);
            _schedulingService.updateLeaseHaulerSelection(formData).done(function () {
                abp.notify.info('Saved successfully.');
                _modalManager.close();
                abp.event.trigger('app.leaseHaulerSelectionModalSaved');
            }).always(function () {
                _modalManager.setBusy(false);
            });
        };
    };
})(jQuery);