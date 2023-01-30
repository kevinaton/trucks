(function($) {
    app.modals.CreateOrEditLeaseHaulerModal = function () {

        var _modalManager;
        var _leaseHaulerService = abp.services.app.leaseHauler;
        var _dtHelper = abp.helper.dataTables;
        var _$form = null;
        var _leaseHaulerId = null;

        var _sendMessageModal = abp.helper.createModal('SendMessage', 'LeaseHaulers');

        var saveLeaseHaulerAsync = function (callback) {
            if (!_$form.valid()) {
            	_$form.showValidateMessage();
                return;
            }

            var leaseHauler = _$form.serializeFormToObject();

            abp.ui.setBusy(_$form);
            _modalManager.setBusy(true);
            _leaseHaulerService.editLeaseHauler(leaseHauler).done(function (data) {
                abp.notify.info('Saved successfully.');
                _$form.find("#Id").val(data);
                _leaseHaulerId = data;
                leaseHauler.Id = data;
                abp.event.trigger('app.createOrEditLeaseHaulerModalSaved', {
                    item: leaseHauler
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

            abp.helper.ui.initControls();
            
            _leaseHaulerId = _$form.find("#Id").val();
            
            var _createOrEditLeaseHaulerContactModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/LeaseHaulers/CreateOrEditLeaseHaulerContactModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_CreateOrEditLeaseHaulerContactModal.js',
                modalClass: 'CreateOrEditLeaseHaulerContactModal'
            });

            var _createOrEditLeaseHaulerTruckModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/LeaseHaulers/CreateOrEditLeaseHaulerTruckModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_CreateOrEditLeaseHaulerTruckModal.js',
                modalClass: 'CreateOrEditLeaseHaulerTruckModal'
            });

            var _createOrEditLeaseHaulerDriverModal = new app.ModalManager({
                viewUrl: abp.appPath + 'app/LeaseHaulers/CreateOrEditLeaseHaulerDriverModal',
                scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/LeaseHaulers/_CreateOrEditLeaseHaulerDriverModal.js',
                modalClass: 'CreateOrEditLeaseHaulerDriverModal'
            });

            var leaseHaulerContactsTable = $('#LeaseHaulerContactsTable');
            var leaseHaulerContactsGrid = leaseHaulerContactsTable.DataTableInit({   
                serverSide: true,
                processing: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewContact"))
                },
                ajax: function (data, callback, settings) {
                    if (_leaseHaulerId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { leaseHaulerId: _leaseHaulerId });
                    _leaseHaulerService.getLeaseHaulerContacts(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                        abp.helper.ui.initControls();
                    });
                },
                paging: false,
                info: false,
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
                        data: "name",
                        title: "Name"
                    },
                    {
                        data: "title",
                        title: "Title"
                    },
                    {
                        data: "phone",
                        render: function (data, type, full, meta) { return _dtHelper.renderPhone(data); },
                        title: "Office Phone"
                    },
                    {
                        data: "cellPhoneNumber",
                        render: function (data, type, full, meta) { return _dtHelper.renderPhone(data); },
                        title: "Cell Phone"
                    },
                    {
                        data: "email",
                        title: "Email"
                    },                   
                    {
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        defaultContent: '',
                        width: "10px",
                        responsivePriority: 1,   
                        rowAction: {
                            items: [
                                {
                                    text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        _createOrEditLeaseHaulerContactModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        deleteLeaseHaulerContact(data.record);
                                    }
                                },
                                {
                                    text: '<i class="fas fa-comments"></i> ' + app.localize('SendSms'),
                                    className: "btn btn-sm btn-default",
                                    visible: function(data) {
                                        return data.record.cellPhoneNumber;
                                    },
                                    action: function (data) {
                                        _sendMessageModal.open({ leaseHaulerId: _leaseHaulerId, leaseHaulerContactId: data.record.id, messageType: 0 });
                                    }
                                },
                                {
                                    text: '<i class="fa fa-phone"></i> ' + app.localize('CallOffice'),
                                    className: "btn btn-sm btn-default",
                                    visible: function (data) {
                                        return data.record.phone;
                                    },
                                    action: function (data) {
                                        window.location = 'tel:' + data.record.phone;
                                    }
                                },
                                {
                                    text: '<i class="fa fa-phone"></i> ' + app.localize('CallCell'),
                                    className: "btn btn-sm btn-default",
                                    visible: function (data) {
                                        return data.record.cellPhoneNumber;
                                    },
                                    action: function (data) {
                                        window.location = 'tel:' + data.record.cellPhoneNumber;
                                    }
                                },
                                {
                                    text: '<i class="fas fa-comments"></i> ' + app.localize('SendEmail'),
                                    className: "btn btn-sm btn-default",
                                    visible: function (data) {
                                        return data.record.email;
                                    },
                                    action: function (data) {
                                        _sendMessageModal.open({ leaseHaulerId: _leaseHaulerId, leaseHaulerContactId: data.record.id, messageType: 1 });
                                    }
                                }
                            ]
                        }
                    }  
                ]
            });

            var leaseHaulerTrucksTable = $('#LeaseHaulerTrucksTable');
            var leaseHaulerTrucksGrid = leaseHaulerTrucksTable.DataTableInit({
                serverSide: true,
                processing: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewTruck"))
                },
                ajax: function (data, callback, settings) {
                    if (_leaseHaulerId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { leaseHaulerId: _leaseHaulerId });
                    _leaseHaulerService.getLeaseHaulerTrucks(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                        abp.helper.ui.initControls();
                    });
                },
                paging: false,
                info: false,
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
                        data: "truckCode",
                        title: app.localize('TruckCode')
                    },
                    {
                        data: "vehicleCategoryName",
                        title: app.localize('Category')
                    },
                    {
                        data: "defaultDriverName",
                        title: app.localize('DefaultDriver')
                    },
                    {
                        data: "alwaysShowOnSchedule",
                        render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(data); },
                        className: "checkmark text-center",
                        title: "Show",
                        titleHoverText: app.localize("AlwaysShowOnSchedule")
                    },
                    {
                        data: "isActive",
                        render: function (data, type, full, meta) { return _dtHelper.renderCheckbox(full.isActive); },
                        className: "checkmark text-center",
                        title: "Active"
                    },
                    {
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        defaultContent: '',
                        width: "10px",
                        responsivePriority: 1,
                        rowAction: {
                            items: [
                                {
                                    text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        _createOrEditLeaseHaulerTruckModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        deleteLeaseHaulerTruck(data.record);
                                    }
                                }
                            ]
                        }
                    }
                ]
            });

            var leaseHaulerDriversTable = $('#LeaseHaulerDriversTable');
            var leaseHaulerDriversGrid = leaseHaulerDriversTable.DataTableInit({
                serverSide: true,
                processing: true,
                language: {
                    emptyTable: app.localize("NoDataInTheTableClick{0}ToAdd", app.localize("AddNewDriver"))
                },
                ajax: function (data, callback, settings) {
                    if (_leaseHaulerId === '') {
                        callback(_dtHelper.getEmptyResult());
                        return;
                    }
                    var abpData = _dtHelper.toAbpData(data);
                    $.extend(abpData, { leaseHaulerId: _leaseHaulerId });
                    _leaseHaulerService.getLeaseHaulerDrivers(abpData).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                        abp.helper.ui.initControls();
                    });
                },
                paging: false,
                info: false,
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
                        data: "firstName",
                        title: "First Name"
                    },
                    {
                        responsivePriority: 3,
                        data: "lastName",
                        title: "Last Name"
                    },
                    {
                        data: "isInactive",
                        render: function (data, type, full, meta) {
                            var checked = full.isInactive === true ? "checked" : "";
                            return '<label class="m-checkbox"><input type="checkbox" class="minimal" disabled name="IsDefault" ' + checked + ' value="true"><span></span></label>';

                        },
                        className: "checkmark text-center",
                        width: "100px",
                        title: "Inactive"
                    },
                    {
                        targets: 4,
                        data: null,
                        orderable: false,
                        autoWidth: false,
                        defaultContent: '',
                        width: "10px",
                        responsivePriority: 1,
                        rowAction: {
                            items: [
                                {
                                    text: '<i class="fa fa-edit"></i> ' + app.localize('Edit'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        _createOrEditLeaseHaulerDriverModal.open({ id: data.record.id });
                                    }
                                },
                                {
                                    text: '<i class="fa fa-trash"></i> ' + app.localize('Delete'),
                                    className: "btn btn-sm btn-default",
                                    action: function (data) {
                                        deleteLeaseHaulerDriver(data.record);
                                    }
                                }
                            ]
                        }
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                leaseHaulerContactsGrid
                    .columns.adjust()
                    .responsive.recalc();
            });

            _modalManager.getModal().find('a[data-toggle="tab"]').on('shown.bs.tab', function () {
                leaseHaulerTrucksGrid.columns.adjust().responsive.recalc();
                leaseHaulerDriversGrid.columns.adjust().responsive.recalc();
            });

            var reloadLeaseHaulerContactGrid = function () {
                leaseHaulerContactsGrid.ajax.reload();
            };

            var reloadLeaseHaulerTruckGrid = function () {
                leaseHaulerTrucksGrid.ajax.reload();
            };

            var reloadLeaseHaulerDriverGrid = function () {
                leaseHaulerDriversGrid.ajax.reload();
            };

            abp.event.on('app.createOrEditLeaseHaulerContactModalSaved', function () {
                reloadLeaseHaulerContactGrid();
            });

            abp.event.on('app.createOrEditLeaseHaulerTruckModalSaved', function () {
                reloadLeaseHaulerTruckGrid();
            });

            abp.event.on('app.createOrEditLeaseHaulerDriverModalSaved', function () {
                reloadLeaseHaulerDriverGrid();
            });

            _modalManager.getModal().find("#CreateNewLeaseHaulerContactButton").click(function (e) {
                e.preventDefault();
                if (_leaseHaulerId === '') {
                    saveLeaseHaulerAsync(function () {
                        _createOrEditLeaseHaulerContactModal.open({ leaseHaulerId: _leaseHaulerId });
                    });
                } else {
                    _createOrEditLeaseHaulerContactModal.open({ leaseHaulerId: _leaseHaulerId });
                }
            });

            _modalManager.getModal().find("#CreateNewLeaseHaulerTruckButton").click(function (e) {
                e.preventDefault();
                if (_leaseHaulerId === '') {
                    saveLeaseHaulerAsync(function () {
                        _createOrEditLeaseHaulerTruckModal.open({ leaseHaulerId: _leaseHaulerId });
                    });
                } else {
                    _createOrEditLeaseHaulerTruckModal.open({ leaseHaulerId: _leaseHaulerId });
                }
            });

            _modalManager.getModal().find("#CreateNewLeaseHaulerDriverButton").click(function (e) {
                e.preventDefault();
                if (_leaseHaulerId === '') {
                    saveLeaseHaulerAsync(function () {
                        _createOrEditLeaseHaulerDriverModal.open({ leaseHaulerId: _leaseHaulerId });
                    });
                } else {
                    _createOrEditLeaseHaulerDriverModal.open({ leaseHaulerId: _leaseHaulerId });
                }
            });

            async function deleteLeaseHaulerContact(record) {
                if (await abp.message.confirm(
                    'Are you sure you want to delete the contact?'
                )) {
                    _leaseHaulerService.deleteLeaseHaulerContact({
                        id: record.id
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        reloadLeaseHaulerContactGrid();
                    });
                }
            }

            async function deleteLeaseHaulerTruck(record) {
                if (await abp.message.confirm(
                    'Are you sure you want to delete the truck?'
                )) {
                    _leaseHaulerService.deleteLeaseHaulerTruck({
                        id: record.id
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        reloadLeaseHaulerTruckGrid();
                    });
                }
            }

            async function deleteLeaseHaulerDriver(record) {
                if (await abp.message.confirm(
                    'Are you sure you want to delete the driver?'
                )) {
                    _leaseHaulerService.deleteLeaseHaulerDriver({
                        id: record.id
                    }).done(function () {
                        abp.notify.info('Successfully deleted.');
                        reloadLeaseHaulerDriverGrid();
                    });
                }
            }
        };

        this.save = function () {
            saveLeaseHaulerAsync(function () {
                _modalManager.close();
            });
        };
    };
})(jQuery);