(function () {
    $(function () {

        var _customerService = abp.services.app.customer;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            merge: abp.auth.hasPermission('Pages.Customers.Merge')
        };
        var _createOrEditCustomerModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Customers/CreateOrEditCustomerModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Customers/_CreateOrEditCustomerModal.js',
            modalClass: 'CreateOrEditCustomerModal',
            modalSize: 'lg'
        });

        var customersTable = $('#CustomersTable');
        var customersGrid = customersTable.DataTableInit({
            paging: true,
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _customerService.getCustomers(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                    abp.helper.ui.initControls();
                });
            },
            dataMergeOptions: {
                enabled: _permissions.merge,
                description: "The selected customers are about to be merged into one entry. Select the customer that you would like them to be merged into. The other customers will be deleted. If you don't want this to happen, press cancel.",
                dropdownServiceMethod: _customerService.getCustomersByIdsSelectList,
                mergeServiceMethod: _customerService.mergeCustomers
            },
            columns: [
                {
                    width: '20px',
                    className: 'control responsive',
                    orderable: false,
                    render: function () {
                        return '';
                    },
                    targets: 0
                },
                {
                    responsivePriority: 1,
                    targets: 1,
                    data: "name",
                    title: "Name"
                },
                {
                    targets: 2,
                    data: "accountNumber",
                    title: "Account #"
                },
                {
                    targets: 3,
                    data: "address1",
                    title: "Address 1"
                },
                {
                    targets: 4,
                    data: "address2",
                    title: "Address 2"
                },
                {
                    targets: 5,
                    data: "city",
                    title: "City"
                },
                {
                    targets: 6,
                    data: "state",
                    title: "State"
                },
                {
                    targets: 7,
                    data: "zipCode",
                    title: "Zip Code"
                },
                {
                    data: "countryCode",
                    title: "Country Code"
                },
                {
                    data: "isActive",
                    render: function (isActive) { return _dtHelper.renderCheckbox(isActive); },
                    className: "checkmark",
                    title: "Active"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 2,  
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        var reloadMainGrid = function () {
            customersGrid.ajax.reload();
        };
        customersTable.on('click', '.btnEditRow', function () {
            var record = _dtHelper.getRowData(this);
            _createOrEditCustomerModal.open({ id: record.id });
        });

        customersTable.on('click', '.btnDeleteRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            deleteCustomer(record);
        });

        abp.event.on('app.createOrEditCustomerModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewCustomerButton").click(function (e) {
            e.preventDefault();
            _createOrEditCustomerModal.open();
        });

        async function deleteCustomer(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the customer?',
            )) {
                _customerService.deleteCustomer({
                    id: record.id
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        }
        
        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

        $('#ExportCustomersToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            abp.ui.setBusy($button);
            _customerService
                .getCustomersToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });

    });
})();