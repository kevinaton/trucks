(function () {
    $(function () {

        var _projectHistoryService = abp.services.app.projectHistory;
        var _dtHelper = abp.helper.dataTables;

        setDefaultDate();

        function setDefaultDate() {
            var today = moment().startOf('day');
            var yesterday = moment().add(-1, 'days').startOf('day');
            $("#StartDateFilter").val(yesterday.format("MM/DD/YYYY"));
            $("#EndDateFilter").val(today.format("MM/DD/YYYY"));
            $("#DateFilter").val(yesterday.format("MM/DD/YYYY") + ' - ' + today.format("MM/DD/YYYY"));
        }

        $("#DateFilter").daterangepicker({
            autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            }
        }, function (start, end, label) {
            $("#StartDateFilter").val(start.clone().tz('UTC').format());
            $("#EndDateFilter").val(end.clone().tz('UTC').format());
        });

        $("#DateFilter").on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
            $("#StartDateFilter").val(picker.startDate.format("MM/DD/YYYY"));
            $("#EndDateFilter").val(picker.endDate.format("MM/DD/YYYY"));
            reloadMainGrid();
        });

        $("#DateFilter").on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
            $("#StartDateFilter").val('');
            $("#EndDateFilter").val('');
            reloadMainGrid();
        });

        $("#CustomerIdFilter").select2Init({
            abpServiceMethod: abp.services.app.customer.getCustomersSelectList,
            showAll: true
        });



        $("#ProjectIdFilter").select2Init({
            abpServiceMethod: abp.services.app.project.getProjectsSelectList
        });

        var projectHistoryTable = $('#ProjectHistoryTable');
        var projectHistoryGrid = projectHistoryTable.DataTableInit({
            paging: true,
            serverSide: true,
            processing: true,
            ajax: function (data, callback, settings) {
                var abpData = _dtHelper.toAbpData(data);
                $.extend(abpData, _dtHelper.getFilterData());
                _projectHistoryService.getProjectHistory(abpData).done(function (abpResult) {
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
                    responsivePriority: 1,
                    data: "projectName",
                    title: "Project"

                },
                {
                    data: "userName",
                    title: "User"
                },
                {
                    data: "dateTime",
                    //render: function (data, type, full, meta) { return _dtHelper.renderUtcDateTime(full.dateTime); },
                    render: function (dateTime) {
                        return moment(dateTime).format('L');
                    },
                    title: "Date"
                },
                {
                    data: "action",
                    render: function (data, type, full, meta) { return _dtHelper.renderText(full.actionName); },
                    title: "Action"
                },
                {
                    data: null,
                    orderable: false,
                    name: "Actions",
                    width: "10px",
                    responsivePriority: 2,
                    className: "actions",
                    defaultContent: '<div class="dropdown action-button">'
                        + '<ul class="dropdown-menu dropdown-menu-right">'
                        + '<li><a class="btnEditRow" title="Edit"><i class="fa fa-edit"></i> View Project</a></li>'
                        + '</ul>'
                        + '<button class="btn btn-primary btn-sm" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false"><i class="fa fa-ellipsis-h"></i></button>'
                        + '</div>'
                }
            ],
            order: [[2, "desc"]]
        });

        function reloadMainGrid() {
            projectHistoryGrid.ajax.reload();
        }

        projectHistoryTable.on('click', '.btnEditRow', function () {
            var projectId = _dtHelper.getRowData(this).projectId;
            window.location = abp.appPath + 'app/projects/details/' + projectId;
        });

        $("#SearchButton").closest('form').submit(function (e) {
            e.preventDefault();
            reloadMainGrid();
        });

        $("#ClearSearchButton").click(function () {
            $(this).closest('form')[0].reset();
            $(".filter").change();

            $("#StartDateFilter").val('');
            $("#EndDateFilter").val('');
            $("#DateFilter").val('');

            setDefaultDate();

            reloadMainGrid();
        });

    });
})();