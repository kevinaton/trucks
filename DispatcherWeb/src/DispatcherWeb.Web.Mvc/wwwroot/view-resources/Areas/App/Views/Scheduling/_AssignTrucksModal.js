(function ($) {
    app.modals.AssignTrucksModal = function () {

        var _modalManager;
        var _driverService = abp.services.app.driver;
        var _schedulingService = abp.services.app.scheduling;
        var _$form = null;
        var _dtHelper = abp.helper.dataTables;
        var _orderLineId = null;
        var _selectedRowIds = [];
        var _filter = null;

        var rowSelectionClass = 'invoice-row-selection';
        var rowSelectAllClass = 'invoice-row-select-all';

        var _assignDriverForTruckModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Scheduling/AssignDriverForTruckModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Scheduling/_AssignDriverForTruckModal.js',
            modalClass: 'AssignDriverForTruckModal'
        });

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            var $driverName = _$form.find('#DriverNameFilter');

            _orderLineId = Number(_$form.find('#OrderLineId').val());

            var vehicleCategoryDropdown = _$form.find("#VehicleCategoryIdsFilter");
            vehicleCategoryDropdown.select2Init({
                abpServiceMethod: abp.services.app.truck.getVehicleCategoriesSelectList,
                abpServiceParams: {
                    isPowered: true,
                    isInUse: true
                },
                showAll: true,
                placeholder: 'All'
                //allowClear: false
            });
            _$form.find("#BedConstructionFilter").select2Init({
                showAll: true,
                //allowClear: false
            });
            _$form.find("#IsApportionedFilter").select2Init({
                showAll: true,
                //allowClear: false
            });



            function initDriverNameTypeahead() {
                $driverName.typeahead({
                }, {
                    source: getDriverNamesList,
                    display: function (d) {
                        return d.firstName + ' ' + d.lastName;
                    },
                    limit: 500
                });

                //$driverName.on('typeahead:select', function (ev, suggestion) {
                //    handleDriverNameChange();
                //});

                //$driverName.on('typeahead:autocomplete', function (ev, suggestion) {
                //    handleDriverNameChange();
                //});

                //$driverName.change(handleDriverNameChange);

                getDriverNamesList();
            }

            let cachedDriverNames = null;
            function getDriverNamesList(query, syncCallback, asyncCallback) {
                if (!cachedDriverNames) {
                    _driverService.getDriverNames().then(function (result) {
                        cachedDriverNames = result;
                        getDriverNamesList(query, function (r) { asyncCallback && asyncCallback(r); });
                    });
                    return;
                }

                query = (query || '').toLowerCase();
                var matchedDrivers = cachedDriverNames
                    .filter(function (d) {
                        return d.firstName.toLowerCase().startsWith(query)
                            || d.lastName.toLowerCase().startsWith(query)
                            || (d.firstName.toLowerCase() + ' ' + d.lastName.toLowerCase()).startsWith(query)
                            || (d.lastName.toLowerCase() + ' ' + d.firstName.toLowerCase()).startsWith(query);
                    });
                syncCallback && syncCallback(matchedDrivers);
            }

            _modalManager.onOpenOnce(function () {
                vehicleCategoryDropdown.val(null).change();
                initDriverNameTypeahead();
                trucksGrid.columns.adjust().responsive.recalc();
            });

            _$form.find("#SearchButton").closest('form').submit(function (e) {
                e.preventDefault();
                updateFilter();
                reloadGrid();
            });

            _$form.find("#ClearSearchButton").click(function () {
                $(this).closest('form')[0].reset();
                $(".filter").change();
                //updateFilter();
                _filter = null;
                reloadGrid();
            });

            function updateFilter() {
                //_selectedRowIds = [];
                _filter = _$form.serializeFormToObject();
                delete _filter.VehicleCategoryIds;
                _filter.vehicleCategoryIds = vehicleCategoryDropdown.val();
            }

            function showValidationError(errorMessage) {
                abp.message.error('Please check the following: \n' + errorMessage, 'Some of the data is invalid');
            }

            var trucksTable = $('#TrucksTable');
            var trucksGrid = trucksTable.DataTableInit({
                paging: false,
                info: false,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClickSearch")
                },
                ajax: function (data, callback, settings) {
                    if (!_filter) {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, _filter);
                    _schedulingService.getTrucksToAssign(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                order: [[2, 'asc']],
                headerCallback: function (thead, data, start, end, display) {
                    var headerCell = $(thead).find('th').eq(1).html('');
                    headerCell.append($('<label class="checkbox-only-header-label"><input type="checkbox" class="minimal ' + rowSelectAllClass + '"></label>'));
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () { return ''; }
                    },
                    {
                        data: null,
                        orderable: false,
                        render: function (data, type, full, meta) {
                            return `<label class="m-checkbox cell-centered-checkbox"><input type="checkbox" class="minimal ${rowSelectionClass}" ${(_selectedRowIds.includes(full.id) ? 'checked' : '')}><span></span></label>`;
                        },
                        className: "checkmark text-center checkbox-only-header",
                        width: "30px",
                        title: " ",
                        responsiveDispalyInHeaderOnly: true
                    },
                    {
                        data: "truckCode",
                        title: "Truck",
                    },
                    {
                        data: "driverName",
                        title: "Driver",
                        render: function (data, type, full, meta) {
                            let icon = '<i class="fa-regular fa-circle-exclamation text-warning pl-1 assignDriverIcon" data-toggle="tooltip" title="No driver assigned to this truck. You won\'t be able to dispatch without a driver. Click here to assign a driver."></i>';
                            return _dtHelper.renderText(data)
                                + (!full.driverId ? icon : '');
                        }
                    },
                    {
                        data: "isApportioned",
                        title: "Apportioned",
                        render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); },
                    },
                    {
                        data: "bedConstructionName",
                        title: "Bed"
                    }
                ],
                drawCallback: function (settings) {
                    trucksTable.find('[data-toggle="tooltip"]').tooltip();
                }
            });

            function reloadGrid() {
                trucksGrid.ajax.reload(null, false);
            }

            trucksTable.on('click', '.assignDriverIcon', async function (e) {
                e.preventDefault();
                var row = _dtHelper.getRowData(this);
                var result = await app.getModalResultAsync(
                    _assignDriverForTruckModal.open({
                        truckId: row.id,
                        truckCode: row.truckCode,
                        leaseHaulerId: row.leaseHaulerId,
                        date: _$form.find("#Date").val(),
                        shift: _$form.find("#Shift").val(),
                        officeId: _$form.find("#OfficeId").val()
                    })
                );
                if (result.driverId && result.driverName) {
                    $(this).closest('td').html(_dtHelper.renderText(result.driverName));
                }
            });

            trucksTable.on('change', '.' + rowSelectAllClass, function () {
                if ($(this).is(":checked")) {
                    trucksTable.find('.' + rowSelectionClass).not(':checked').prop('checked', true).change();
                } else {
                    trucksTable.find('.' + rowSelectionClass + ':checked').prop('checked', false).change();
                }
            });

            trucksTable.on('change', '.' + rowSelectionClass, function () {
                var row = _dtHelper.getRowData(this);
                if ($(this).is(":checked")) {
                    if (trucksTable.find('.' + rowSelectionClass).not(':checked').length === 0) {
                        trucksTable.find('.' + rowSelectAllClass).not(':checked').prop('checked', true).change();
                    }
                    setRowSelectionState(row.id, true);
                } else {
                    if (trucksTable.find('.' + rowSelectionClass + ':checked').length === 0) {
                        trucksTable.find('.' + rowSelectAllClass + ':checked').prop('checked', false).change();
                    }
                    setRowSelectionState(row.id, false);
                }
                //$.each(selectionChangedCallbacks, function (ind, callback) {
                //var selectedRowsCount = trucksTable.find('.' + rowSelectionClass + ':checked').length;
                //    callback(selectedRowsCount);
                //});
            });

            function getSelectedRowsIds() {
                //var ids = [];
                //trucksTable.find('.'+rowSelectionClass + ':checked').each(function () {
                //    ids.push(abp.helper.dataTables.getRowData(this).id);
                //});
                //return ids;
                return _selectedRowIds;
            }

            function setRowSelectionState(id, isChecked) {
                if (isChecked) {
                    if (!_selectedRowIds.includes(id)) {
                        //console.log('adding ' + id);
                        _selectedRowIds.push(id);
                    }
                } else {
                    while (_selectedRowIds.includes(id)) {
                        //console.log('removing ' + id);
                        _selectedRowIds.splice(_selectedRowIds.indexOf(id), 1);
                    }
                }
                //console.log('array: ' + _selectedRowIds.join());
            }

            _$form.find("#AddToScheduleButton").click(function () {
                var truckIds = getSelectedRowsIds();
                if (!truckIds.length) {
                    abp.message.warn('Please select the trucks first');
                    return;
                }
                var model = {
                    orderLineId: _orderLineId,
                    truckIds: truckIds
                };
                _modalManager.setBusy(true);
                _schedulingService.assignTrucks(model).done(function () {
                    abp.event.trigger('app.trucksAssignedModal');
                    abp.notify.info('Saved successfully.');
                    _modalManager.close();
                }).fail(function () {
                    //reloadGrid();
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            });

        };

    };
})(jQuery);