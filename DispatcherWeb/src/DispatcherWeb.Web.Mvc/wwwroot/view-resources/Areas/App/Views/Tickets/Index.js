﻿(function () {

    var _dtHelper = abp.helper.dataTables;
    var _ticketService = abp.services.app.ticket;
    var _lastAbpData = null;
    var _selectedRowIds = [];
    var _$ticketPhotoInput = $('#TicketPhoto');
    var _ticketForPhotoUpload = null;

    var _isFilterReady = false;
    var _isGridInitialized = false;

    var _isCustomerPortalUser = !abp.auth.hasPermission('Pages.Tickets.View')
        && abp.auth.hasPermission('CustomerPortal.TicketList')
        && abp.session.customerId;

    $('[data-toggle="tooltip"]').tooltip();

    var _createOrEditTicketModal = new app.ModalManager({
        viewUrl: abp.appPath + 'app/Tickets/CreateOrEditTicketModal',
        scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Tickets/_CreateOrEditTicketModal.js',
        modalClass: 'CreateOrEditTicketModal'
    });
    var useShifts = abp.setting.getBoolean('App.General.UseShifts');

    var _permissions = {
        invoices: abp.auth.hasPermission('Pages.Invoices')
    };

    var rowSelectionClass = 'invoice-row-selection';
    var rowSelectAllClass = 'invoice-row-select-all';
    var rowSelectionColumnClass = 'invoice-row-selection-column';

    //init the filter controls
    app.localStorage.getItem('tickets_filter', function (cachedFilter) {
        if (!cachedFilter) {
            cachedFilter = {
                ticketDateRangeBegin: moment().format("MM/DD/YYYY"),
                ticketDateRangeEnd: moment().add(1, 'days').format("MM/DD/YYYY"),
                officeId: abp.session.officeId,
                officeName: abp.session.officeName
            };
        }

        if (_isCustomerPortalUser) {
            cachedFilter.customerId = abp.session.customerId;
            cachedFilter.customerName = abp.session.customerName;
            $("#CustomerFilter").prop("disabled", true);
        }

        var ticketDateFilterIsEmpty = false;
        if (!cachedFilter.ticketDateRangeBegin || cachedFilter.ticketDateRangeBegin === 'Invalid date') {
            ticketDateFilterIsEmpty = true;
            //still need to init the daterangepicker with real dates first and clear the inputs only after the init.
            cachedFilter.ticketDateRangeBegin = moment().format("MM/DD/YYYY");
        }

        if (!cachedFilter.ticketDateRangeEnd || cachedFilter.ticketDateRangeEnd === 'Invalid date') {
            ticketDateFilterIsEmpty = true;
            cachedFilter.ticketDateRangeEnd = moment().add(1, 'days').format("MM/DD/YYYY");
        }

        $("#TicketDateRangeBeginInput").val(cachedFilter.ticketDateRangeBegin);
        $("#TicketDateRangeEndInput").val(cachedFilter.ticketDateRangeEnd);
        $("#TicketDateRangeFilter").val($("#TicketDateRangeBeginInput").val() + ' - ' + $("#TicketDateRangeEndInput").val());

        $("#TicketDateRangeFilter").daterangepicker({
            locale: {
                cancelLabel: 'Clear'
            },
            showDropDown: true,
            autoUpdateInput: false
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $("#TicketDateRangeBeginInput").val(picker.startDate.format('MM/DD/YYYY'));
            $("#TicketDateRangeEndInput").val(picker.endDate.format('MM/DD/YYYY'));
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            $("#TicketDateRangeBeginInput").val('');
            $("#TicketDateRangeEndInput").val('');
        });

        if (ticketDateFilterIsEmpty) {
            $("#TicketDateRangeBeginInput").val('');
            $("#TicketDateRangeEndInput").val('');
            $("#TicketDateRangeFilter").val('');
        }

        //orderDateFilter
        var orderDateFilterIsEmpty = false;

        if (!cachedFilter.orderDateRangeBegin || cachedFilter.orderDateRangeBegin === 'Invalid date') {
            orderDateFilterIsEmpty = true;
            //still need to init the daterangepicker with real dates first and clear the inputs only after the init.
            cachedFilter.orderDateRangeBegin = moment().format("MM/DD/YYYY");
        }

        if (!cachedFilter.orderDateRangeEnd || cachedFilter.orderDateRangeEnd === 'Invalid date') {
            orderDateFilterIsEmpty = true;
            cachedFilter.orderDateRangeEnd = moment().add(1, 'days').format("MM/DD/YYYY");
        }

        $("#OrderDateRangeBeginInput").val(cachedFilter.orderDateRangeBegin);
        $("#OrderDateRangeEndInput").val(cachedFilter.orderDateRangeEnd);
        $("#OrderDateRangeFilter").val($("#OrderDateRangeBeginInput").val() + ' - ' + $("#OrderDateRangeEndInput").val());

        $("#OrderDateRangeFilter").daterangepicker({
            locale: {
                cancelLabel: 'Clear'
            },
            showDropDown: true,
            autoUpdateInput: false
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $("#OrderDateRangeBeginInput").val(picker.startDate.format('MM/DD/YYYY'));
            $("#OrderDateRangeEndInput").val(picker.endDate.format('MM/DD/YYYY'));
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            $("#OrderDateRangeBeginInput").val('');
            $("#OrderDateRangeEndInput").val('');
        });

        if (orderDateFilterIsEmpty) {
            $("#OrderDateRangeBeginInput").val('');
            $("#OrderDateRangeEndInput").val('');
            $("#OrderDateRangeFilter").val('');
        }

        $('#TruckFilter').select2Init({
            abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
            abpServiceParams: {
                excludeTrailers: false,
                includeLeaseHaulerTrucks: true
            },
            showAll: false,
            allowClear: true
        });
        if (cachedFilter.truckId) {
            abp.helper.ui.addAndSetDropdownValue($("#TruckFilter"), cachedFilter.truckId, cachedFilter.truckCode);
        }

        $('#CarrierFilter').select2Init({
            abpServiceMethod: abp.services.app.leaseHauler.getLeaseHaulersSelectList,
            showAll: false,
            allowClear: true
        });
        if (cachedFilter.carrierId) {
            abp.helper.ui.addAndSetDropdownValue($("#CarrierFilter"), cachedFilter.carrierId, cachedFilter.carrierName);
        }

        $('#ServiceFilter').select2Init({
            abpServiceMethod: abp.services.app.service.getAllServicesSelectList,
            showAll: false,
            allowClear: true
        });
        if (cachedFilter.serviceId) {
            abp.helper.ui.addAndSetDropdownValue($("#ServiceFilter"), cachedFilter.serviceId, cachedFilter.serviceName);
        }

        $('#Shifts').select2Init({
            showAll: true,
            allowClear: false
        });
        if (cachedFilter.shifts) {
            cachedFilter.shifts.forEach(function (shift) {
                abp.helper.ui.addAndSetDropdownValue($("#Shifts"), shift);
            });
        }

        $('#BillingStatusFilter').select2Init({
            showAll: true,
            allowClear: true
        });
        if (cachedFilter.billingStatus) {
            abp.helper.ui.addAndSetDropdownValue($("#BillingStatusFilter"), cachedFilter.billingStatus, cachedFilter.billingStatusName);
        }

        $('#IsVerifiedFilter').select2Init({
            showAll: true,
            allowClear: true
        });
        if (cachedFilter.isVerified) {
            abp.helper.ui.addAndSetDropdownValue($("#IsVerifiedFilter"), cachedFilter.isVerified, cachedFilter.isVerifiedDescription);
        }

        $('#IsImportedFilter').select2Init({
            showAll: true,
            allowClear: true
        });
        if (cachedFilter.isImported) {
            abp.helper.ui.addAndSetDropdownValue($("#IsImportedFilter"), cachedFilter.isImported, cachedFilter.isImportedDescription);
        }

        $('#DriverFilter').select2Init({
            abpServiceMethod: abp.services.app.driver.getDriversSelectList,
            showAll: false,
            allowClear: true
        });
        if (cachedFilter.driverId) {
            abp.helper.ui.addAndSetDropdownValue($("#DriverFilter"), cachedFilter.driverId, cachedFilter.driverName);
        }

        $('#OfficeIdFilter').select2Init({
            abpServiceMethod: abp.services.app.office.getOfficesSelectList,
            showAll: true,
            allowClear: true
        });
        if (cachedFilter.officeId) {
            abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), cachedFilter.officeId, cachedFilter.officeName);
        }

        $('#CustomerFilter').select2Init({
            abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
            showAll: false,
            allowClear: true
        });
        if (cachedFilter.customerId) {
            abp.helper.ui.addAndSetDropdownValue($("#CustomerFilter"), cachedFilter.customerId, cachedFilter.customerName);
        }

        $('#LoadAtFilter').select2Init({
            abpServiceMethod: abp.services.app.location.getAllLocationsSelectList,
            showAll: false,
            allowClear: true
        });
        if (cachedFilter.loadAtId) {
            abp.helper.ui.addAndSetDropdownValue($("#LoadAtFilter"), cachedFilter.loadAtId, cachedFilter.loadAtName);
        }

        $('#DeliverToFilter').select2Init({
            abpServiceMethod: abp.services.app.location.getAllLocationsSelectList,
            showAll: false,
            allowClear: true
        });
        if (cachedFilter.deliverToId) {
            abp.helper.ui.addAndSetDropdownValue($("#DeliverToFilter"), cachedFilter.deliverToId, cachedFilter.deliverToName);
        }

        $('#TicketStatusFilter').select2Init({
            showAll: true,
            allowClear: true
        });
        if (cachedFilter.ticketStatus) {
            abp.helper.ui.addAndSetDropdownValue($("#TicketStatusFilter"), cachedFilter.ticketStatus, cachedFilter.ticketStatusDescription);
        }

        $('#OrderIdFilter').select2Init({
            abpServiceMethod: abp.services.app.order.getOrderIdsSelectList,
            showAll: false,
            allowClear: true
        });
        if (cachedFilter.orderId) {
            abp.helper.ui.addAndSetDropdownValue($("#OrderIdFilter"), cachedFilter.orderId, cachedFilter.orderId);
        }

        $("#TicketNumberFilter").val(cachedFilter.ticketNumber);
        $("#JobNumberFilter").val(cachedFilter.jobNumber);


        _isFilterReady = true;
        if (_isGridInitialized) {
            reloadMainGrid(null, false);
        }
    });

    var ticketTable = $('#TicketTable');
    var ticketGrid = ticketTable.DataTableInit({
        stateSave: true,
        stateDuration: 0,
        ajax: function (data, callback, settings) {
            if (!_isGridInitialized) {
                _isGridInitialized = true;
            }
            if (!_isFilterReady) {
                callback(_dtHelper.getEmptyResult());
                return;
            }
            var abpData = _dtHelper.toAbpData(data);
            _lastAbpData = $.extend({}, abpData);
            var filterData = _dtHelper.getFilterData();
            app.localStorage.setItem('tickets_filter', filterData);
            $.extend(abpData, filterData);
            _ticketService.ticketListView(abpData).done(function (abpResult) {
                callback(_dtHelper.fromAbpResult(abpResult));
            }).fail(function () {
                callback(null);
            });
        },
        editable: {
            saveCallback: async function (rowData, cell) {
                try {
                    await _ticketService.editTicketFromList({
                        id: rowData.id,
                        isVerified: rowData.isVerified
                    });
                    updateSelectionCellForRow(rowData, cell);
                } catch {
                    reloadMainGrid();
                }
            }
        },
        order: [[2, 'asc']],
        headerCallback: function (thead, data, start, end, display) {
            if (_permissions.invoices) {
                var headerCell = $(thead).find('th').eq(1).html('');
                headerCell.append($('<label class="m-checkbox cell-centered-checkbox checkbox-only-header-label"><input type="checkbox" class="minimal ' + rowSelectAllClass + '"><span></span></label>'));
            }
        },
        columns: [
            {
                width: '20px',
                className: 'control responsive',
                orderable: false,
                render: function () {
                    return '';
                }
            },
            {
                data: null,
                orderable: false,
                visible: _permissions.invoices,
                render: function (data, type, full, meta) {
                    return renderSelectionCheckbox(full);
                },
                className: `checkmark text-center checkbox-only-header ${rowSelectionColumnClass}`,
                width: "30px",
                title: " ",
                //responsivePriority: 3,
                responsiveDispalyInHeaderOnly: true
            },
            {
                data: "date",
                render: function (data, type, full, meta) { return (_dtHelper.renderDateShortTime(full.date) || '').replace(' ', '<br>'); },
                title: "Ticket Date"
            },
            {
                data: "orderDate",
                render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); },
                title: "Order Date"
            },
            {
                data: "shiftRaw",
                title: "Shift",
                visible: useShifts,
                render: function (data, type, full, meta) {
                    return _dtHelper.renderText(full.shift);
                }
            },
            {
                responsivePriority: 1,
                data: "customerName",
                title: "Customer"
            },
            {
                data: "jobNumber",
                title: "Job Nbr"
            },
            {
                data: "product",
                title: "Product / Service"
            },
            {
                data: "ticketNumber",
                title: "Ticket #"
            },
            {
                responsivePriority: 10000 + 1,
                data: "carrier",
                title: "Carrier",
                visible: abp.features.isEnabled('App.AllowLeaseHaulersFeature')
            },
            {
                data: "truck",
                title: "Truck"
            },
            {
                data: "driverName",
                title: "Driver",
                responsivePriority: 10000 + 2,
            },
            {
                data: "loadAtNamePlain",
                render: function (data, type, full, meta) { return _dtHelper.renderText(full.loadAtName); },
                title: "Load At"
            },
            {
                data: "deliverToNamePlain",
                render: function (data, type, full, meta) { return _dtHelper.renderText(full.deliverToName); },
                title: "Deliver To"
            },
            {
                data: "uom",
                title: "UOM"
            },
            {
                data: "quantity",
                title: "Qty"
            },
            {
                data: "rate",
                title: "Rate",
                orderable: false
            },
            {
                data: "revenue",
                title: "Revenue",
                visible: !_isCustomerPortalUser
            },
            {
                data: "isBilled",
                render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isBilled); },
                className: "checkmark",
                title: "Billed"
            },
            {
                data: "isVerified",
                title: "Ver",
                titleHoverText: "Verified",
                render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); },
                width: '30px',
                editable: {
                    editor: _dtHelper.editors.checkbox
                }
            },
            {
                data: "hasPayStatements",
                render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.hasPayStatements); },
                title: "Pay Stmt"
            },
            {
                data: "hasLeaseHaulerStatements",
                render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.hasLeaseHaulerStatements); },
                title: "LH Stmt"
            },
            {
                data: 'ticketPhotoId',
                width: "10px",
                className: "all",
                render: function (data, type, full, meta) {
                    return full.ticketPhotoId ? '<i class="la la-file-image-o showTicketPhotoButton"></i>' : '';
                }
            },
            {
                data: null,
                orderable: false,
                responsivePriority: 2,
                className: "actions",
                width: "10px",
                render: function (data, type, full, meta) {
                    if (!abp.auth.hasPermission('Pages.Tickets.Edit')) {
                        return '';
                    }
                    let uploadButtonCaption = full.ticketPhotoId ? 'Replace image' : 'Add image';
                    return '<div class="dropdown">'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnEditRow dropdown-item"><i class="fa fa-edit"></i> Edit ticket</a></li>'
                        + (!full.isBilled ? '<li><a class="btnBilledRow dropdown-item"><i class="fa fa-check"></i> Mark as billed</a></li>' : '')
                        + (full.ticketPhotoId ? '<li><a class="showTicketPhotoButton dropdown-item"><i class="la la-file-image-o"></i> View image</a></li>' : '')
                        + `<li><a class="btnUploadTicketPhotoForRow dropdown-item"><i class="la la-file-image-o"></i> ${uploadButtonCaption}</a></li>`
                        + (full.ticketPhotoId ? '<li><a class="btnDeleteTicketPhotoForRow dropdown-item"><i class="la la-file-image-o"></i> Delete image</a></li>' : '')
                        + '<li><a class="btnDeleteRow dropdown-item"><i class="fa fa-trash"></i> Delete entire ticket</a></li>'
                        + '</ul>'
                        + '</div>';
                }
            }
        ]
    });

    function updateSelectionCellForRow(rowData, updatedCell) {
        var selectionColumnCell = $(updatedCell).closest('tr').find('.' + rowSelectionColumnClass);
        var oldCellContent = selectionColumnCell.html();
        var newCellContent = renderSelectionCheckbox(rowData);
        if (oldCellContent !== newCellContent) {
            if (!newCellContent) {
                setRowSelectionState(rowData.id, false);
            }
            selectionColumnCell.html(newCellContent);
        }
    }

    function renderSelectionCheckbox(rowData) {
        if (rowData.isBilled || !rowData.revenue || rowData.invoiceLineId || !rowData.isVerified /*|| full.hasPayStatements*/) {
            return "";
        }
        return `<label class="m-checkbox cell-centered-checkbox"><input type="checkbox" class="minimal ${rowSelectionClass}" ${(_selectedRowIds.includes(rowData.id) ? 'checked' : '')}><span></span></label>`;
    }

    ticketTable.on('change', '.' + rowSelectAllClass, function () {
        if ($(this).is(":checked")) {
            ticketTable.find('.' + rowSelectionClass).not(':checked').prop('checked', true).change();
        } else {
            ticketTable.find('.' + rowSelectionClass + ':checked').prop('checked', false).change();
        }
    });

    ticketTable.on('change', '.' + rowSelectionClass, function () {
        var row = _dtHelper.getRowData(this);
        if ($(this).is(":checked")) {
            if (ticketTable.find('.' + rowSelectionClass).not(':checked').length === 0) {
                ticketTable.find('.' + rowSelectAllClass).not(':checked').prop('checked', true).change();
            }
            setRowSelectionState(row.id, true);
        } else {
            if (ticketTable.find('.' + rowSelectionClass + ':checked').length === 0) {
                ticketTable.find('.' + rowSelectAllClass + ':checked').prop('checked', false).change();
            }
            setRowSelectionState(row.id, false);
        }
        //$.each(selectionChangedCallbacks, function (ind, callback) {
        var selectedRowsCount = ticketTable.find('.' + rowSelectionClass + ':checked').length;
        //    callback(selectedRowsCount);
        //});
    });

    function getSelectedRowsIds() {
        //var ids = [];
        //ticketTable.find('.'+rowSelectionClass + ':checked').each(function () {
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

    ticketTable.on('click', '.btnBilledRow', function () {
        var ticketId = _dtHelper.getRowData(this).id;
        _ticketService.markAsBilledTicket({
            id: ticketId
        }).done(function () {
            abp.notify.info('Successfully marked.');
            setRowSelectionState(ticketId, false);
            reloadMainGrid();
        });
    });

    ticketTable.on('click', '.btnDeleteRow', async function () {
        var ticket = _dtHelper.getRowData(this);
        if (ticket.receiptLineId) {
            abp.message.error('You can\'t delete tickets associated with receipts');
            return;
        }

        if (!await abp.message.confirm('Are you sure you want to delete the ticket?')) {
            return;
        }

        _ticketService.deleteTicket({
            id: ticket.id
        }).done(function () {
            abp.notify.info('Successfully deleted.');
            setRowSelectionState(ticket.id, false);
            reloadMainGrid();
        });
    });

    ticketTable.on('click', '.btnEditRow', function () {
        var ticketId = _dtHelper.getRowData(this).id;
        _createOrEditTicketModal.open({ id: ticketId });
    });

    $('#CreateNewButton').click(function (e) {
        _createOrEditTicketModal.open();
    });

    abp.event.on('app.createOrEditTicketModalSaved', function () {
        reloadMainGrid();
    });

    ticketTable.on('click', '.showTicketPhotoButton', function (e) {
        e.preventDefault();
        var ticket = _dtHelper.getRowData(this);
        let url = abp.appPath + 'app/Tickets/GetTicketPhoto/' + ticket.id;
        window.open(url);
    });

    ticketTable.on('click', '.btnUploadTicketPhotoForRow', function (e) {
        e.preventDefault();
        var ticket = _dtHelper.getRowData(this);
        _ticketForPhotoUpload = ticket;
        _$ticketPhotoInput.click();
    });

    ticketTable.on('click', '.btnDeleteTicketPhotoForRow', async function (e) {
        e.preventDefault();
        var ticket = _dtHelper.getRowData(this);
        if (!await abp.message.confirm('Are you sure you want to delete the image?')) {
            return;
        }

        abp.ui.setBusy();
        _ticketService.deleteTicketPhoto({
            ticketId: ticket.id
        }).done(function () {
            ticket.ticketPhotoId = null;
            reloadMainGrid();
        }).always(function () {
            abp.ui.clearBusy();
        });
    });

    $('#InvoiceSelectedTicketsButton').click(function (e) {
        e.preventDefault();
        var ticketIds = getSelectedRowsIds();
        if (ticketIds.length === 0) {
            abp.message.warn('Please select some tickets first');
            return;
        }
        var button = $(this);
        abp.ui.setBusy(button);
        abp.services.app.invoice.createInvoicesForTickets({ ticketIds }).done(function (result) {
            if (result && result.batchId) {
                abp.notify.info('Invoices created successfully.');
                window.location = abp.appPath + 'app/invoices/?batchId=' + result.batchId;
            } else {
                abp.message.warn('No invoices were created');
            }
        }).always(function () {
            abp.ui.clearBusy(button);
        });
    });

    $('form').submit(function (event) {
        event.preventDefault();
        _selectedRowIds = [];
        reloadMainGrid();
    });
    $("#ClearSearchButton").click(function () {
        $(this).closest('form')[0].reset();
        $('#CarrierFilter').val(0).trigger("change");
        $('#ServiceFilter').val(0).trigger("change");
        $('#DriverFilter').val('').trigger("change");
        $('#TruckFilter').val('').trigger("change");
        $('#Shifts').val('').trigger("change");
        $(".filter").change();
        $("#TicketDateRangeBeginInput").val('');
        $("#TicketDateRangeEndInput").val('');
        $("#OrderDateRangeBeginInput").val('');
        $("#OrderDateRangeEndInput").val('');
        $('#OfficeIdFilter').val('').trigger("change");
        if (_isCustomerPortalUser) {
            abp.helper.ui.addAndSetDropdownValue($("#CustomerFilter"), abp.session.customerId, abp.session.customerName);
        }
        _selectedRowIds = [];
        reloadMainGrid();
    });

    $('#ExportAllTicketsToCsvButton').click(function (e) {
        e.preventDefault();
        exportTicketsToCsv();
    });

    $('#ExportSelectedTicketsToCsvButton').click(function (e) {
        e.preventDefault();
        var ticketIds = getSelectedRowsIds();
        if (!ticketIds.length) {
            abp.message.warn('Please select some tickets first');
            return;
        }
        exportTicketsToCsv({
            ticketIds
        });
    });

    function exportTicketsToCsv(additionalFilter) {
        var abpData = $.extend({}, _lastAbpData);
        var filterData = _dtHelper.getFilterData();
        $.extend(abpData, filterData, additionalFilter);

        abp.ui.setBusy();
        _ticketService
            .getTicketsToCsv(abpData)
            .done(function (result) {
                app.downloadTempFile(result);
            }).always(function () {
                abp.ui.clearBusy();
            });
    }

    function reloadMainGrid() {
        ticketGrid.ajax.reload();
    }

    _$ticketPhotoInput.change(function () {
        if (!_ticketForPhotoUpload) {
            return;
        }

        if (!abp.helper.validateTicketPhoto(_$ticketPhotoInput)) {
            return;
        }

        const file = _$ticketPhotoInput[0].files[0];
        const reader = new FileReader();

        reader.addEventListener("load", function () {
            _ticketService.addTicketPhoto({
                ticketId: _ticketForPhotoUpload.id,
                ticketPhoto: reader.result,
                ticketPhotoFilename: file.name
            }).done(function (result) {
                _ticketForPhotoUpload.ticketPhotoId = result.ticketPhotoId;
                reloadMainGrid();
            }).always(function () {
                _ticketForPhotoUpload = null;
                _$ticketPhotoInput.val('');
                abp.ui.clearBusy();
            });
        }, false);

        abp.ui.setBusy();
        reader.readAsDataURL(file);
    });

})();