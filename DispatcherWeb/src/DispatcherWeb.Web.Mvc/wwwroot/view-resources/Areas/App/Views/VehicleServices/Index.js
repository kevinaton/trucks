﻿(function () {
    $(function () {

        var _serviceService = abp.services.app.vehicleService;
        var _dtHelper = abp.helper.dataTables;

        var _permissions = {
            edit: abp.auth.hasPermission('Pages.VehicleService.Edit')
        };

        var _createOrEditVehicleServiceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/VehicleServices/CreateOrEditVehicleServiceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/VehicleServices/_CreateOrEditVehicleServiceModal.js',
            modalClass: 'CreateOrEditVehicleServiceModal'
        });

        var servicesTable = $('#ServicesTable');
        var servicesGrid = servicesTable.DataTableInit({
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _serviceService.getPagedList(abpData).done(function (abpResult) {
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
                    responsivePriority: 1,
                    data: "name"
                },
                {
                    data: "description"
                },
                {
                    data: "recommendedTimeInterval"
                },
                {
                    data: "recommendedMileageInterval"
                },               
                {
                    data: "recommendedHourInterval"
                },               
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    width: '10px',
                    responsivePriority: 2,  
                    render: function (data, type, full, meta) {
                        return _permissions.edit ?
                            '<div class="dropdown">'
                            + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                            + '<ul class="dropdown-menu dropdown-menu-right">'
                            + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                            + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                            + '</ul>'
                            + '</div>' :
                            '';
                    }
                }
            ]
        });



        servicesTable.on('click', '.btnDeleteRow', function () {
            var record = _dtHelper.getRowData(this);
            deleteVehicleService(record);
        });

        servicesTable.on('click', '.btnEditRow', function () {
            var serviceId = _dtHelper.getRowData(this).id;
            _createOrEditVehicleServiceModal.open({ id: serviceId });
        });

        var getServices = function () {
            servicesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditVehicleServiceModalSaved', function () {
            getServices();
        });

        $("#CreateNewServiceButton").click(function (e) {
            e.preventDefault();
            _createOrEditVehicleServiceModal.open();
        });		

        async function deleteVehicleService(record) {
            if (await abp.message.confirm(
                'Are you sure you want to delete the service?'
            )) {
                _serviceService.delete(
                    record.id
                ).done(function () {
                    abp.notify.info('Successfully deleted.');
                    getServices();
                });
            }
        }

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            getServices();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();
            getServices();
        });

    });
})();