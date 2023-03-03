(function () {
    $(function () {

        var _serviceService = abp.services.app.service;
        var _dtHelper = abp.helper.dataTables;
        var _permissions = {
            merge: abp.auth.hasPermission('Pages.Services.Merge')
        };
        var _createOrEditServiceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Services/CreateOrEditServiceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Services/_CreateOrEditServiceModal.js',
            modalClass: 'CreateOrEditServiceModal',
            modalSize: 'lg'
        });

        var statusFilter = $('#StatusFilter');
        statusFilter.select2Init({
            showAll: true,
            allowClear: false
        });

        var servicesTable = $('#ServicesTable');
        var servicesGrid = servicesTable.DataTableInit({
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _serviceService.getServices(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                    //abp.helper.ui.initControls();
                });
            },
            dataMergeOptions: {
                enabled: _permissions.merge,
                description: "The selected products or services are about to be merged into one entry. Select the entry that you would like them to be merged into. The other entries will be deleted. There is no undoing this process. If you don't want this to happen, press cancel.",
                entitiesName: 'entries',
                dropdownServiceMethod: _serviceService.getServicesByIdsSelectList,
                mergeServiceMethod: _serviceService.mergeServices
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
                    targets: 1,
                    data: "service1",
                    title: "Name",
                    responsivePriority: 1
                },
                {
                    targets: 2,
                    data: "description",
                    title: "Description"
                },
                {
                    targets: 3,
                    data: "isActive",
                    render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isActive); },
                    className: "checkmark",
                    width: "100px",
                    title: "Active"
                },
                {
                    targets: 4,
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    responsivePriority: 2,
                    width: '10px',
                    render: function (data, type, full, meta) {
                        return '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>';
                    }
                }
            ]
        });

        var reloadMainGrid = function () {
            servicesGrid.ajax.reload();
        };

        servicesTable.on('click', '.btnEditRow', function () {
            var record = _dtHelper.getRowData(this);
            _createOrEditServiceModal.open({ id: record.id });
        });

        servicesTable.on('click', '.btnDeleteRow', function (e) {
            e.preventDefault();
            var record = _dtHelper.getRowData(this);
            deleteSerivce(record);
        });

        abp.event.on('app.createOrEditServiceModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewServiceButton").click(function (e) {
            e.preventDefault();
            _createOrEditServiceModal.open();
        });


        async function deleteSerivce(record) {
            if (await abp.message.confirm('Are you sure you want to delete the service?')) {
                _serviceService.deleteService({
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

        $('#ExportServicesToCsvButton').click(function () {
            var $button = $(this);
            var abpData = {};
            $.extend(abpData, _dtHelper.getFilterData());
            abp.ui.setBusy($button);
            _serviceService
                .getServicesToCsv(abpData)
                .done(function (result) {
                    app.downloadTempFile(result);
                }).always(function () {
                    abp.ui.clearBusy($button);
                });
        });


    });
})();