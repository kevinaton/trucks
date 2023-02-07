(function () {
    $(function () {

        var _orderService = abp.services.app.order;
        var _dtHelper = abp.helper.dataTables;
        var _isFilterReady = false;
        var _isGridInitialized = false;

        abp.helper.ui.initControls();

        //init the filter controls
        app.sessionStorage.getItem('BillingReconciliationFilter', function (cachedFilter) {
            if (!cachedFilter) {
                cachedFilter = {
                    startDate: null,
                    endDate: null,
                    customerId: "",
                    customerName: "Select a customer",
                    officeId: abp.session.officeId,
                    officeName: abp.session.officeName
                };
            }

            var dateFilterIsEmpty = false;

            if (!cachedFilter.startDate || cachedFilter.startDate === 'Invalid date') {
                dateFilterIsEmpty = true;
                //still need to init the daterangepicker with real dates first and clear the inputs only after the init.
                cachedFilter.startDate = moment().format("MM/DD/YYYY");
            }

            if (!cachedFilter.endDate || cachedFilter.endDate === 'Invalid date') {
                dateFilterIsEmpty = true;
                cachedFilter.endDate = moment().add(1, 'days').format("MM/DD/YYYY");
            }

            $("#DateStartFilter").val(cachedFilter.startDate);
            $("#DateEndFilter").val(cachedFilter.endDate);
            $("#DateFilter").val($("#DateStartFilter").val() + ' - ' + $("#DateEndFilter").val());

            $("#DateFilter").daterangepicker({
                locale: {
                    cancelLabel: 'Clear'
                }
            },
                function (start, end, label) {
                    $("#DateStartFilter").val(start.format('MM/DD/YYYY'));
                    $("#DateEndFilter").val(end.format('MM/DD/YYYY'));
                });

            $("#DateFilter").on('apply.daterangepicker', function (ev, picker) {
                $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
                $("#DateStartFilter").val(picker.startDate.format('MM/DD/YYYY'));
                $("#DateEndFilter").val(picker.endDate.format('MM/DD/YYYY'));
                reloadMainGrid();
            });

            $("#DateFilter").on('cancel.daterangepicker', function (ev, picker) {
                $(this).val('');
                $("#DateStartFilter").val('');
                $("#DateEndFilter").val('');
                reloadMainGrid();
            });

            if (dateFilterIsEmpty) {
                $("#DateFilter").val('');
                $("#DateStartFilter").val('');
                $("#DateEndFilter").val('');
            }

            $("#OfficeIdFilter").select2Init({
                abpServiceMethod: abp.services.app.office.getOfficesSelectList,
                minimumInputLength: 0,
                showAll: true,
                allowClear: false
            });
            if (cachedFilter.officeId) {
                abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), cachedFilter.officeId, cachedFilter.officeName);
            }

            $("#CustomerIdFilter").select2Init({
                abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
                showAll: false,
                allowClear: true
            });
            if (cachedFilter.customerId) {
                abp.helper.ui.addAndSetDropdownValue($("#CustomerIdFilter"), cachedFilter.customerId, cachedFilter.customerName);
            }

            _isFilterReady = true;
            if (_isGridInitialized) {
                reloadMainGrid(null, false);
            }
        });

        var lastSortingString = "";

        var billingReconciliationTable = $('#BillingReconciliationTable');
        var billingReconciliationGrid = billingReconciliationTable.DataTableInit({
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
                var filterData = _dtHelper.getFilterData();
                lastSortingString = abpData.sorting;
                app.sessionStorage.setItem('BillingReconciliationFilter', filterData);
                $.extend(abpData, filterData);
                _orderService.getBillingReconciliation(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            order: [[1, 'asc']],
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () { return ''; }
                },
                {
                    data: "isBilled",
                    title: "Billed",
                    orderable: false,
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); },
                    className: "checkmark isBilledCheckbox",
                    width: "20px"
                },
                {
                    data: "deliveryDate",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(data); },
                    title: "Delivery Date"
                },
                {
                    data: "customerName",
                    title: "Customer"
                },
                {
                    data: "codTotal",
                    orderable: false,
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(data); },
                    title: "Total"
                },
                {
                    data: null,
                    responsivePriority: 1,
                    orderable: false,
                    name: "Actions",
                    title: " ",
                    width: "40px",
                    className: "actions",
                    defaultContent: '<div class="dropdown action-button">'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '<ul class="dropdown-menu">'
                        + '<li><a class="btnEditRow" title="Edit Order"><i class="fa fa-edit"></i> Edit</a></li>'
                        + '</ul>'
                        + '</div>'
                }
            ],
            createdRow: function (row, data, index) {
                //if (data.isShared) {
                //    $(row).addClass('order-shared');
                //}
            }
        });
        billingReconciliationGrid.on("draw", () => {
            var pageSizeSelector = $("select.m-input");
            if (!pageSizeSelector.data('select2')) {
                pageSizeSelector.select2Init({
                    showAll: true,
                    allowClear: false
                });
            }
        });

        function reloadMainGrid(callback, resetPaging) {
            resetPaging = resetPaging === undefined ? true : resetPaging;
            billingReconciliationGrid.ajax.reload(callback, resetPaging);
        }

        //$(".filter").change(function () {
        //    reloadMainGrid();
        //});

        billingReconciliationTable.on('click', '.btnEditRow', function () {
            var orderId = _dtHelper.getRowData(this).id;
            window.location = abp.appPath + 'app/Orders/Details/' + orderId;
        });

        billingReconciliationTable.on('click', 'td.isBilledCheckbox', function () {
            var row = _dtHelper.getRowData(this);
            var isBilled = !row.isBilled;
            var cell = $(this).closest('td');
            abp.ui.setBusy(cell);
            _orderService.setOrderIsBilled({ orderId: row.id, isBilled: isBilled }).done(function () {
                row.isBilled = isBilled;
                cell.html(_dtHelper.renderCheckbox(isBilled));
            }).always(function () {
                abp.ui.clearBusy(cell);
            });
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            $("#DateStartFilter").val('');
            $("#DateEndFilter").val('');
            abp.helper.ui.addAndSetDropdownValue($("#OfficeIdFilter"), abp.session.officeId, abp.session.officeName);
            reloadMainGrid();
        });

        $("#ExportBillingReconciliationButton").click(function (e) {
            e.preventDefault();
            var request = {
                sorting: lastSortingString
            };
            $.extend(request, _dtHelper.getFilterData());
            _orderService.exportBillingReconciliationToExcel(request).done(function (result) {
                app.downloadTempFile(result);
            });
        });

    });
})();