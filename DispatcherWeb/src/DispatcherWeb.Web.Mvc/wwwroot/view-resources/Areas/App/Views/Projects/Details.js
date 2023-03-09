(function () {
    $(function () {

        var _projectService = abp.services.app.project;
        var _quoteService = abp.services.app.quote;
        var _dtHelper = abp.helper.dataTables;
        var _projectId = $("#Id").val();

        $('form').validate();
        $.validator.addMethod(
            "regex",
            function (value, element, regexp) {
                var re = new RegExp(regexp, 'i');
                return this.optional(element) || re.test(value);
            },
            "Please check your input."
        );

        var _createOrEditProjectServiceModal = new app.ModalManager({
            viewUrl: abp.appPath + 'app/Projects/CreateOrEditProjectServiceModal',
            scriptUrl: abp.appPath + 'view-resources/Areas/app/Views/Projects/_CreateOrEditProjectServiceModal.js',
            modalClass: 'CreateOrEditProjectServiceModal'
        });

        var $dateBegin = $("#StartDate");
        var $dateEnd = $("#EndDate");
        $dateBegin.datepickerInit();
        $dateEnd.datepickerInit();

        var saveProjectAsync = function (callback) {
            var form = $("#ProjectForm");
            if (!form.valid()) {
                form.showValidateMessage();
                return;
            }
            if (!abp.helper.validateStartEndDates(
                { value: $dateBegin.val(), title: $('label[for="StartDate"]').text() },
                { value: $dateEnd.val(), title: $('label[for="EndDate"]').text() }
            )) {
                return;
            }

            var project = form.serializeFormToObject();
            abp.ui.setBusy(form);
            _projectService.editProject(project).done(function (data) {
                abp.notify.info('Saved successfully.');
                _projectId = data.id;
                $("#Id").val(_projectId);
                if (data.endDate) {
                    $dateEnd.val(_dtHelper.renderUtcDate(data.endDate)).change();
                }
                history.replaceState({}, "", abp.appPath + 'app/projects/details/' + _projectId);
                if (callback)
                    callback();
            }).always(function () {
                abp.ui.clearBusy(form);
            });
        };

        var today = new Date();
        today.setHours(0, 0, 0, 0);

        $("#Status").select2Init({
            showAll: true,
            allowClear: false
        });

        var projectServicesTable = $('#ProjectServicesTable');
        var projectServicesGrid = projectServicesTable.DataTableInit({
            paging: false,
            info: false,
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                if (_projectId === '') {
                    callback(_dtHelper.getEmptyResult());
                    return;
                }
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, { projectId: _projectId });
                _projectService.getProjectServices(abpData).done(function (abpResult) {
                    callback(_dtHelper.fromAbpResult(abpResult));
                });
            },
            footerCallback: function (tfoot, data, start, end, display) {
                var materialTotal = data.map(function (x) { return x.extendedMaterialPrice; }).reduce(function (a, b) { return a + b; }, 0);
                var serviceTotal = data.map(function (x) { return x.extendedServicePrice; }).reduce(function (a, b) { return a + b; }, 0);

                let grid = this;
                let setTotalFooterValue = function (columnName, total, visible) {
                    let footerCell = grid.api().column(columnName + ':name').footer();
                    $(footerCell).html(visible ? "Total: " + _dtHelper.renderMoney(total) : '');
                }
                setTotalFooterValue('extendedMaterialPrice', materialTotal, data.length);
                setTotalFooterValue('extendedServicePrice', serviceTotal, data.length);
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
                    data: "id",
                    visible: false
                },
                {
                    targets: 2,
                    data: "loadAtNamePlain",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.loadAtName); },
                    title: "Load At"
                },
                {
                    targets: 2,
                    data: "deliverToNamePlain",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.deliverToName); },
                    title: "Deliver To"
                },
                {
                    targets: 3,
                    data: "serviceName",
                    title: "Item"
                },
                {
                    targets: 4,
                    data: "materialUomName",
                    title: "Material<br>UOM"
                },
                {
                    targets: 4,
                    data: "freightUomName",
                    title: "Freight<br>UOM"
                },
                {
                    targets: 5,
                    data: "designation",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.designationName); },
                    title: "Designation",
                    orderable: false
                },
                {
                    targets: 6,
                    data: "pricePerUnit",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.pricePerUnit); },
                    title: "Material<br>Rate"
                },
                {
                    targets: 7,
                    data: "freightRate",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.freightRate); },
                    title: "Freight<br>Rate"
                },
                {
                    targets: 8,
                    data: "leaseHaulerRate",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.leaseHaulerRate); },
                    title: "LH Rate",
                    visible: abp.setting.getBoolean('App.LeaseHaulers.ShowLeaseHaulerRateOnQuote')
                },
                {
                    data: "materialQuantity",
                    title: "Material<br>Quantity"
                },
                {
                    data: "freightQuantity",
                    title: "Freight<br>Quantity"
                },
                {
                    data: "extendedMaterialPrice",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.extendedMaterialPrice); },
                    name: "extendedMaterialPrice",
                    title: "Extended<br>Material Price",
                    orderable: false
                },
                {
                    data: "extendedServicePrice",
                    render: function (data, type, full, meta) { return _dtHelper.renderMoney(full.extendedServicePrice); },
                    name: "extendedServicePrice",
                    title: "Extended<br>Freight Price",
                    orderable: false
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 1,
                    defaultContent: '<div class="dropdown action-button">'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                        + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                        + '</ul>'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>'

                }
            ]
        });

        var projectQuotesTable = $('#ProjectQuotesTable');


        var projectQuotesGrid = projectQuotesTable.DataTableInit({
            paging: false,
            info: false,
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                if (_projectId === '') {
                    callback(_dtHelper.getEmptyResult());
                    return;
                }
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, { projectId: _projectId });
                _quoteService.getProjectQuotes(abpData).done(function (abpResult) {
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
                    }
                },
                {
                    data: "id",
                    visible: false
                },
                {
                    data: "customerName",
                    title: "Customer Name"
                },
                {
                    data: "contactName",
                    title: "Contact Name"
                },
                {
                    data: "contactPhone",
                    title: "Contact Phone"
                },
                {
                    data: "contactEmail",
                    title: "Contact Email"
                },
                {
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    width: "10px",
                    responsivePriority: 1,
                    defaultContent: '<div class="dropdown action-button">'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                        + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                        + '</ul>'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>'
                }
            ]
        });

        var reloadProjectServicesGrid = function () {
            projectServicesGrid.ajax.reload();
        };

        var reloadProjectQuotesGrid = function () {
            projectQuotesGrid.ajax.reload();
        };

        abp.event.on('app.createOrEditProjectServiceModalSaved', function () {
            reloadProjectServicesGrid();
        });

        $("#CreateNewProjectServiceButton").click(function (e) {
            e.preventDefault();
            if (_projectId === '') {
                saveProjectAsync(function () {
                    reloadProjectServicesGrid();
                    _createOrEditProjectServiceModal.open({ projectId: _projectId });
                });
            } else {
                _createOrEditProjectServiceModal.open({ projectId: _projectId });
            }
        });

        $("#CreateNewQuoteButton").click(function (e) {
            e.preventDefault();
            saveProjectAsync(function () {
                abp.ui.setBusy();
                window.location = abp.appPath + 'app/quotes/details/?projectId=' + _projectId;
            });
        });

        $("#SaveProjectButton").click(function (e) {
            e.preventDefault();
            saveProjectAsync(function () {
                reloadProjectServicesGrid();
            });
        });

        projectServicesTable.on('click', '.btnEditRow', function () {
            var projectServiceId = _dtHelper.getRowData(this).id;
            _createOrEditProjectServiceModal.open({ id: projectServiceId });
        });

        projectQuotesTable.on('click', '.btnEditRow', function () {
            var quoteId = _dtHelper.getRowData(this).id;
            abp.ui.setBusy();
            window.location = abp.appPath + 'app/quotes/details/' + quoteId;
        });

        projectServicesTable.on('click', '.btnDeleteRow', async function () {
            var projectServiceId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to delete the item?')) {
                _projectService.deleteProjectService({
                    id: projectServiceId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadProjectServicesGrid();
                });
            }
        });

        projectQuotesTable.on('click', '.btnDeleteRow', async function () {
            var quoteId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to delete the quote?')) {
                _quoteService.deleteQuote({
                    id: quoteId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadProjectQuotesGrid();
                });
            }
        });

        $('#Status').on('change', enableOrDisableButtonsDependingOnProjectStatus);

        enableOrDisableButtonsDependingOnProjectStatus();
        function enableOrDisableButtonsDependingOnProjectStatus() {
            var $statusCtrl = $('#Status');
            $('#CreateNewProjectServiceButton, #CreateNewQuoteButton').prop('disabled', $statusCtrl.val() === $statusCtrl.data('inactive-status'));
        }

    });
})();