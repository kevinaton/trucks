(function ($) {
    app.modals.CreateOrEditTicketModal = function () {

        var _modalManager;
        var _ticketService = abp.services.app.ticket;
        var _$form = null;
        var _$ticketPhotoInput = null;
        var _ticketIdForPhotoUpload = null;
        var _dtHelper = abp.helper.dataTables;
        var _orderLineId = null;
        var _orderLineIsProductionPay = null;
        var _validateTrucksAndDrivers = abp.setting.getBoolean('App.General.ValidateDriverAndTruckOnTickets');

        this.init = function (modalManager) {
            _modalManager = modalManager;

            _$form = _modalManager.getModal().find('form[name=TicketForm]');
            _$ticketPhotoInput = _$form.find('#TicketPhoto');

            _orderLineId = Number(_$form.find('#OrderLineId').val());
            _orderLineIsProductionPay = _$form.find('#OrderLineIsProductionPay').val() === 'True';
            //var designation = Number(_$form.find('#OrderLineDesignation').val());

            _$form.find("#RefreshGridButton").click(function () {
                reloadGrid();
            });

            _$form.find("#AddTicketButton").click(function () {
                _modalManager.setBusy(true);
                _ticketService.editOrderTicket({
                    orderLineId: _orderLineId,
                    ticketNumber: ''
                }).done(function (e) {
                    abp.notify.info('Ticket added');
                    abp.event.trigger('app.ticketEditedModal', e);
                    reloadGrid();
                }).always(function () {
                    _modalManager.setBusy(false);
                });
            });

            _$ticketPhotoInput.change(function () {
                if (!_ticketIdForPhotoUpload) {
                    return;
                }

                if (!abp.helper.validateTicketPhoto(_$ticketPhotoInput)) {
                    return;
                }
                const file = _$ticketPhotoInput[0].files[0];
                const reader = new FileReader();

                reader.addEventListener("load", function () {
                    _ticketService.addTicketPhoto({
                        ticketId: _ticketIdForPhotoUpload,
                        ticketPhoto: reader.result,
                        ticketPhotoFilename: file.name
                    }).done(function (result) {
                        //result.ticketPhotoId
                        reloadGrid();
                    }).always(function () {
                        _ticketIdForPhotoUpload = null;
                        _$ticketPhotoInput.val('');
                        abp.ui.clearBusy(_modalManager.getModal());
                        _modalManager.setBusy(false);
                    });
                }, false);

                abp.ui.setBusy(_modalManager.getModal());
                _modalManager.setBusy(true);
                reader.readAsDataURL(file);
            });

            function getTicketFromRowData(rowData) {
                return {
                    id: rowData.id,
                    orderLineId: rowData.orderLineId,
                    ticketNumber: rowData.ticketNumber,
                    ticketDateTime: rowData.ticketDateTime,
                    quantity: rowData.quantity,
                    truckId: rowData.truckId,
                    truckCode: rowData.truck, //api is expecting 'truckCode' on edit, but sends 'truck' in grid data
                    driverId: rowData.driverId,
                    driverName: rowData.driverName
                };
            }

            function editTicket(ticket, cell) {
                //abp.ui.setBusy(cell);
                return _ticketService.editOrderTicket(ticket).done(function (result) {
                    abp.event.trigger('app.ticketEditedModal', result);
                    abp.notify.info('Saved successfully.');
                    if (result.warningText) {
                        abp.message.warn(result.warningText);
                    }
                    return result;
                }).fail(function () {
                    reloadGrid();
                }).always(function () {
                    //abp.ui.clearBusy(cell);
                });
            }


            var ticketsTable = $('#TicketsTable');
            var ticketsGrid = ticketsTable.DataTableInit({
                paging: false,
                info: false,
                ordering: false,
                ajax: function (data, callback, settings) {
                    _ticketService.loadTicketsByOrderLineId(_orderLineId).done(function (abpResult) {
                        callback(_dtHelper.fromAbpResult(abpResult));
                    });
                },
                editable: {
                    saveCallback: function (rowData, cell) {
                        var ticket = getTicketFromRowData(rowData);
                        return editTicket(ticket, cell);
                    }
                },
                columns: [
                    {
                        width: '20px',
                        className: 'control responsive',
                        orderable: false,
                        render: function () { return ''; }
                    },
                    {
                        data: "ticketDateTime",
                        title: "Time",
                        render: function (data, type, full, meta) { return _dtHelper.renderTime(full.ticketDateTime, ''); },
                        className: "all",
                        width: "85px",
                        editable: {
                            editor: _dtHelper.editors.time
                        }
                    },
                    {
                        data: "ticketNumber",
                        title: "Ticket Number",
                        className: "cell-text-wrap all",
                        editable: {
                            editor: _dtHelper.editors.text,
                            required: true,
                            maxLength: 20
                        }
                    },
                    {
                        data: "quantity",
                        title: "Quantity",
                        className: "all",
                        editable: {
                            editor: _dtHelper.editors.quantity
                        }
                    },
                    {
                        data: "uomName",
                        title: "UOM"
                    },
                    {
                        data: "truck",
                        title: "Truck",
                        width: "90px",
                        className: "truck-code-column all",
                        editable: {
                            editor: _dtHelper.editors.dropdown,
                            idField: 'truckId',
                            nameField: 'truck',
                            dropdownOptions: {
                                abpServiceMethod: abp.services.app.truck.getTrucksSelectList,
                                abpServiceParams: {
                                    allOffices: true,
                                    includeLeaseHaulerTrucks: true,
                                    activeOnly: true,
                                    orderLineId: _validateTrucksAndDrivers ? _orderLineId : null
                                },
                                showAll: false,
                                allowClear: true
                            },
                            editCompleteCallback: function editCompleteCallback(editResult, rowData, cell) {
                                var driverCell = $(cell).closest('tr').find('td.driver-name-column');
                                var driverEditor = driverCell.find('select');
                                if (!driverEditor.length) {
                                    driverCell.text(editResult.ticket.driverName);
                                } else {
                                    if (parseInt(rowData.driverId) !== editResult.ticket.driverId) {
                                        abp.helper.ui.addAndSetSelect2ValueSilently(driverEditor, editResult.ticket.driverId, editResult.ticket.driverName);
                                    }
                                }
                                rowData.driverId = editResult.ticket.driverId;
                                rowData.driverName = editResult.ticket.driverName;
                            }
                        }
                    },
                    {
                        data: "driverName",
                        title: "Driver",
                        width: "90px",
                        className: "driver-name-column all",
                        editable: {
                            editor: _dtHelper.editors.dropdown,
                            idField: 'driverId',
                            nameField: 'driverName',
                            dropdownOptions: {
                                abpServiceMethod: abp.services.app.driver.getDriversSelectList,
                                abpServiceParams: {
                                    includeLeaseHaulerDrivers: true,
                                    orderLineId: _validateTrucksAndDrivers ? _orderLineId : null
                                },
                                abpServiceParamsGetter: (params, rowData) => ({
                                    truckId: rowData.truckId
                                }),
                                showAll: false,
                                allowClear: true
                            },
                            editCompleteCallback: function editCompleteCallback(editResult, rowData, cell) {
                                var truckCell = $(cell).closest('tr').find('td.truck-code-column');
                                var truckEditor = truckCell.find('select');
                                if (!truckEditor.length) {
                                    truckCell.text(editResult.ticket.truckCode);
                                } else {
                                    if (parseInt(rowData.truckId) !== editResult.ticket.truckId) {
                                        abp.helper.ui.addAndSetSelect2ValueSilently(truckEditor, editResult.ticket.truckId, editResult.ticket.truckCode);
                                    }
                                }
                                rowData.truckId = editResult.ticket.truckId;
                                rowData.truckCode = editResult.ticket.truckCode;
                            }
                        }
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
                        responsivePriority: 1,
                        name: "Actions",
                        width: "10px",
                        className: "actions all",
                        render: function (data, type, full, meta) {
                            let uploadButtonCaption = full.ticketPhotoId ? 'Replace ticket' : 'Add ticket image';
                            return '<div class="dropdown action-button">'
                                + '<ul class="dropdown-menu dropdown-menu-right">'
                                + `<li><a class="btnUploadTicketPhotoForRow dropdown-item"><i class="la la-file-image-o"></i> ${uploadButtonCaption}</a></li>`
                                + (full.ticketPhotoId ? '<li><a class="btnDeleteTicketPhotoForRow dropdown-item"><i class="la la-file-image-o"></i> Delete image</a></li>' : '')
                                + '<li><a class="btnDeleteRow dropdown-item" title="Delete"><i class="fa fa-trash"></i> Delete entire ticket</a></li>'
                                + '</ul>'
                                + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                                + '</div>';
                        }
                    }
                ]
            });

            _modalManager.getModal().on('shown.bs.modal', function () {
                ticketsGrid.columns.adjust().responsive.recalc();
            });

            function reloadGrid() {
                ticketsGrid.ajax.reload(null, false);
            }

            ticketsTable.on('click', '.btnDeleteRow', async function () {
                var ticket = _dtHelper.getRowData(this);
                if (ticket.receiptLineId) {
                    abp.message.error('You can\'t delete tickets associated with receipts');
                    return;
                }
                
                if (!await abp.message.confirm('Are you sure you want to delete the item?')) {
                    return;
                }

                _ticketService.deleteTicket({ id: ticket.id }).done(function (e) {
                    abp.notify.info('Successfully deleted.');
                    abp.event.trigger('app.ticketDeletedModal', e);
                    reloadGrid();
                });
                
            });

            ticketsTable.on('click', '.btnUploadTicketPhotoForRow', function () {
                var ticket = _dtHelper.getRowData(this);
                _ticketIdForPhotoUpload = ticket.id;
                _$ticketPhotoInput.click();
            });

            ticketsTable.on('click', '.showTicketPhotoButton', function (e) {
                e.preventDefault();
                var ticket = _dtHelper.getRowData(this);
                let url = abp.appPath + 'app/Tickets/GetTicketPhoto/' + ticket.id;
                window.open(url);
            });

            ticketsTable.on('click', '.btnDeleteTicketPhotoForRow', async function (e) {
                e.preventDefault();
                var ticket = _dtHelper.getRowData(this);
                if (!await abp.message.confirm('Are you sure you want to delete the image?')) {
                    return;
                }
                
                _modalManager.setBusy(true);
                _ticketService.deleteTicketPhoto({
                    ticketId: ticket.id
                }).done(function () {
                    ticket.ticketPhotoId = null;
                    reloadGrid();
                }).always(function () {
                    _modalManager.setBusy(false);
                });

            });
        };

    };
})(jQuery);