(function () {
    $(function () {

        var _projectService = abp.services.app.project;
        var _dtHelper = abp.helper.dataTables;

        $("#StatusFilter").select2Init({
            allowClear: false,
            showAll: true,
            noSearch: true
        });

        $("#StartDateFilter").daterangepicker({
            autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            }
        },
        function (start, end, label) {
            $("#StartDateStartFilter").val(start.format('MM/DD/YYYY'));
            $("#StartDateEndFilter").val(end.format('MM/DD/YYYY'));
        });

        $("#StartDateFilter").on('blur', function () {
            var startDate = $("#StartDateStartFilter").val();
            var endDate = $("#StartDateEndFilter").val();
            $(this).val(startDate && endDate ? startDate + ' - ' + endDate : '');
        });

        $("#StartDateFilter").on('apply.daterangepicker', function (ev, picker) {
            console.log('bbbb');
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $("#StartDateStartFilter").val(picker.startDate.format('MM/DD/YYYY'));
            $("#StartDateEndFilter").val(picker.endDate.format('MM/DD/YYYY'));
            reloadMainGrid();
        });

        $("#StartDateFilter").on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            $("#StartDateStartFilter").val('');
            $("#StartDateEndFilter").val('');
            reloadMainGrid();
        });

        $("#EndDateFilter").daterangepicker({
            autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            }
        },
        function (start, end, label) {
            $("#EndDateStartFilter").val(start.format('MM/DD/YYYY'));
            $("#EndDateEndFilter").val(end.format('MM/DD/YYYY'));
        });

        $("#EndDateFilter").on('blur', function () {
            var startDate = $("#EndDateStartFilter").val();
            var endDate = $("#EndDateEndFilter").val();
            $(this).val(startDate && endDate ? startDate + ' - ' + endDate : '');
        });

        $("#EndDateFilter").on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $("#EndDateStartFilter").val(picker.startDate.format('MM/DD/YYYY'));
            $("#EndDateEndFilter").val(picker.endDate.format('MM/DD/YYYY'));
            reloadMainGrid();
        });

        $("#EndDateFilter").on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            $("#EndDateStartFilter").val('');
            $("#EndDateEndFilter").val('');
            reloadMainGrid();
        });

        var projectsTable = $('#ProjectsTable');
        var projectsGrid = projectsTable.DataTableInit({
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _projectService.getProjects(abpData).done(function (abpResult) {
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
                    responsivePriority: 1,
                    targets: 1,
                    data: "name",
                    title: "Name"
                },
                {
                    targets: 2,
                    data: "description",
                    title: "Description"
                },
                {
                    targets: 3,
                    data: "statusName",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.statusName); },
                    title: "Status",
                    orderable: false
                },
                {
                    targets: 4,
                    data: "startDate",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(full.startDate); },
                    title: "Start date"
                },
                {
                    targets: 5,
                    data: "endDate",
                    render: function (data, type, full, meta) { return _dtHelper.renderUtcDate(full.endDate); },
                    title: "End date"
                },
                {
                    targets: 6,
                    data: null,
                    orderable: false,
                    autoWidth: false,
                    defaultContent: '',
                    responsivePriority: 2,
                    width: '10px',
                    render: function (data, type, full, meta) {
                        if (full.status !== abp.enums.projectStatus.inactive) {
                            return '<div class="dropdown">'
                                + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                                + '<ul class="dropdown-menu dropdown-menu-right">'
                                + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                                + '<li><a class="btnInactivateRow" title="Inactive"><i class="fa fa-minus-circle"></i> Inactivate</a></li>'
                                + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                                + '</ul>'
                                + '</div>'
                                ;
                        } else {
                            return '<div class="dropdown">'
                                + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                                + '<ul class="dropdown-menu dropdown-menu-right">'
                                + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> Edit</a></li>'
                                + '<li><a class="btnDeleteRow" title="Delete"><i class="fa fa-trash"></i> Delete</a></li>'
                                + '</ul>'
                                + '</div>';
                        }
                    }
                }
            ]
        });

        function reloadMainGrid() {
            projectsGrid.ajax.reload();
        }

        abp.event.on('app.createOrEditProjectModalSaved', function () {
            reloadMainGrid();
        });

        $("#CreateNewProjectButton").click(function (e) {
            e.preventDefault();
            window.location = abp.appPath + 'app/Projects/Details/';
        });

        projectsTable.on('click', '.btnEditRow', function () {
            var projectId = _dtHelper.getRowData(this).id;
            window.location = abp.appPath + 'app/Projects/Details/' + projectId;
        });

        projectsTable.on('click', '.btnDeleteRow', async function (e) {
            e.preventDefault();
            var projectId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to delete the project?')) {
                _projectService.deleteProject({
                    id: projectId
                }).done(function () {
                    abp.notify.info('Successfully deleted.');
                    reloadMainGrid();
                });
            }
        });

        projectsTable.on('click', '.btnInactivateRow', async function (e) {
            e.preventDefault();
            var projectId = _dtHelper.getRowData(this).id;
            if (await abp.message.confirm('Are you sure you want to inactivate the project?')) {
                _projectService.inactivateProject({
                    id: projectId
                }).done(function () {
                    abp.notify.info('Successfully inactivated.');
                    reloadMainGrid();
                });
            }
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
            $("#StartDateStartFilter").val('');
            $("#StartDateEndFilter").val('');
            $("#EndDateStartFilter").val('');
            $("#EndDateEndFilter").val('');
            reloadMainGrid();
        });

    });
})();