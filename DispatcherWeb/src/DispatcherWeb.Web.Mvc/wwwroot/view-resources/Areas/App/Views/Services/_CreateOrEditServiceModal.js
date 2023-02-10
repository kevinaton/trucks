(function ($) {
    app.modals.CreateOrEditServiceModal = function () {

        var _modalManager;
        var _serviceService = abp.services.app.service;
        var _dtHelper = abp.helper.dataTables;
        var _$form = null;
        var _serviceId = null;

        var saveServiceAsync = function (callback) {
            if (!_$form.valid()) {
                return;
            }

            var service = _$form.serializeFormToObject();

            abp.ui.setBusy(_$form);
            _modalManager.setBusy(true);
            _serviceService.editService(service).done(function (data) {
                abp.notify.info('Saved successfully.');
                _$form.find("#Id").val(data);
                _serviceId = data;
                service.Id = data;
                abp.event.trigger('app.createOrEditServiceModalSaved', {
                    item: service
                });
                if (callback)
                    callback();
            }).always(function () {
                abp.ui.clearBusy(_$form);
                _modalManager.setBusy(false);
            });
        };

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form');
            _$form.validate();

            //abp.helper.ui.initControls();

            _serviceId = _modalManager.getModal().find("#Id").val(); //_$form.find('input[name="Id"]').val();

            var typeDropdown = _modalManager.getModal().find("#Type");
            typeDropdown.select2Init({
                allowClear: false,
                showAll: true
            });

            var _createOrEditServicePriceModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/Services/CreateOrEditServicePriceModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Services/_CreateOrEditServicePriceModal.js',
                modalClass: 'CreateOrEditServicePriceModal'
            });

            var servicePricesTable = _modalManager.getModal().find('#ServicePricesTable');
            var servicePricesGrid = servicePricesTable.DataTableInit({
                paging: false,
                info: false,
                serverSide: true,
                processing: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddRate"))
                },
                ajax: function (data, callback, settings) {
                    if (_serviceId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { serviceId: _serviceId });
                    _serviceService.getServicePrices(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
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
                        data: "materialUomName",
                        title: "Material<br>UOM"
                    },
                    {
                        targets: 2,
                        data: "pricePerUnit",
                        title: "Price<br>Per Unit"
                    },
                    {
                        targets: 1,
                        data: "freightUomName",
                        title: "Freight<br>UOM"
                    },
                    {
                        targets: 3,
                        data: "freightRate",
                        title: "Freight<br>Rate"
                    },
                    {
                        targets: 4,
                        data: "designationName",
                        title: "Designation",
                        orderable: false
                    },
                    {
                        targets: 5,
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        defaultContent: '',
                        responsivePriority: 1,
                        width: '10px',
                        rowAction: {
                            items: [{
                                text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    _createOrEditServicePriceModal.open({ id: data.record.id });
                                }
                            }, {
                                text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                className: "btn btn-sm btn-default",
                                action: function (data) {
                                    deleteServicePrice(data.record);
                                }
                            }]
                        }
                    }
                ]
            });

            modalManager.getModal().on('shown.bs.modal', function () {
                servicePricesGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            var reloadServicePriceGrid = function () {
                servicePricesGrid.ajax.reload();
            };

            abp.event.on('app.createOrEditServicePriceModalSaved', function () {
                reloadServicePriceGrid();
            });

            _modalManager.getModal().find("#CreateNewServicePriceButton").click(function (e) {
                e.preventDefault();
                if (_serviceId === '') {
                    saveServiceAsync(function () {
                        _createOrEditServicePriceModal.open({ serviceId: _serviceId });
                    });
                } else {
                    _createOrEditServicePriceModal.open({ serviceId: _serviceId });
                }
            });


            async function deleteServicePrice(record) {
                if (await abp.message.confirm('Are you sure you want to delete the price?')) {
                    _serviceService.deleteServicePrice({
                        id: record.id
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        reloadServicePriceGrid();
                    });
                }
            }
        };

        this.save = function () {
            saveServiceAsync(function () {
                _modalManager.close();
            });
        };
    };
})(jQuery);