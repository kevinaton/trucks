(function () {
    $(function () {

        var _leaseHaulerService = abp.services.app.leaseHauler;
        var _dtHelper = abp.helper.dataTables;

        var _createOrEditLeaseHaulerModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/LeaseHaulers/CreateOrEditLeaseHaulerModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_CreateOrEditLeaseHaulerModal.js',
            modalClass: 'CreateOrEditLeaseHaulerModal',
            modalSize: 'lg'
        });

        var reloadMainGrid = function () {
            leaseHaulersGrid.ajax.reload();
        };

        var leaseHaulersTable = $('#LeaseHaulersTable');
        var leaseHaulersGrid = leaseHaulersTable.DataTableInit({
            paging: true,
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _leaseHaulerService.getLeaseHaulers(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                    abp.helper.ui.initControls();
                });
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
                    className: 'cell-text-wrap',
                    data: "name",
                    title: "Name"
                },
                {
                    data: "accountNumber",
                    title: "Account #"
                },
                {
                    data: "streetAddress1",
                    title: "Address1"
                },
                {
                    data: "city",
                    title: "City"
                },
                {
                    data: "state",
                    title: "State"
                },
                {
                    data: "zipCode",
                    title: "Zip Code"
                },
                {
                    data: "countryCode",
                    title: "Country Code"
                },
                {
                    data: "phoneNumber",
                    title: "Phone Number"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: "10px;",
                    responsivePriority: 1,
                    rowAction: {
                        items: [
                            {
                                text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    _createOrEditLeaseHaulerModal.open({ id: data.record.id });
                                }
                            },
                            {
                                text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                className: "btn btn-sm btn-default",
                                action: async function (data) {
                                    if (await abp.message.confirm(
                                        'Are you sure you want to delete the lease hauler?'
                                    )) {
                                        _leaseHaulerService.deleteLeaseHauler({
                                            id: data.record.id
                                        }).done(function () {
                                            abp.notify.info('Successfully deleted.');
                                            reloadMainGrid();
                                        });
                                    }
                                }
                            }
                        ]
                    }
                }
            ]
        });

        abp.event.on('app.createOrEditLeaseHaulerModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewLeaseHaulerButton").click(function (e) {
            e.preventDefault();
            _createOrEditLeaseHaulerModal.open();
        });

        leaseHaulersTable.on('click', '.btnEditRow', function () {
            var leaseHaulerId = _dtHelper.getRowData(this).id;
            _createOrEditLeaseHaulerModal.open({ id: leaseHaulerId });
        });

        $('#ShowAdvancedFiltersSpan').click(function () {
            $('#ShowAdvancedFiltersSpan').hide();
            $('#HideAdvancedFiltersSpan').show();
            $('.AdvacedAuditFiltersArea').slideDown();
        });

        $('#HideAdvancedFiltersSpan').click(function () {
            $('#HideAdvancedFiltersSpan').hide();
            $('#ShowAdvancedFiltersSpan').show();
            $('.AdvacedAuditFiltersArea').slideUp();
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            reloadMainGrid();
        });

        $('#ExportLeaseHaulersToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            abp.ui.setBusy($button);
            _leaseHaulerService
                .getLeaseHaulersToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });


    });
})();