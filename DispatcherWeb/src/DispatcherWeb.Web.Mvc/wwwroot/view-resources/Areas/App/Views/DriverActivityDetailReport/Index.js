(function () {
    'use strict';

    var _dtHelper = abp.helper.dataTables;

    $(function () {

        //abp.helper.reports.setReportService(abp.services.app.revenueBreakdownReport);
        //abp.helper.reports.setFormDataHandler(function (formData) {
        //});

        //$("#DateFilter").val(moment().startOf('day').add(-1, 'd').format('MM/DD/YYYY')).datepickerInit();

        $('#DateFilter').val(moment().add(-1, 'd').format('MM/DD/YYYY - MM/DD/YYYY'));
        $("#DateFilter").daterangepicker({
            //autoUpdateInput: false,
            locale: {
                cancelLabel: 'Clear'
            },
            showDropDown: true
        }).on('apply.daterangepicker', function (ev, picker) {
            $(this).val(picker.startDate.format('MM/DD/YYYY') + ' - ' + picker.endDate.format('MM/DD/YYYY'));
        }).on('cancel.daterangepicker', function (ev, picker) {
            $(this).val('');
        });

        $("#DriverIdFilter").select2Init({
            abpServiceMethod: abp.services.app.driver.getDriversSelectList,
            showAll: false,
            allowClear: true
        });

        $('#CreateReportPdf, #CreateReportCsv').off('click');
        $('#CreateReportPdf').click(function (e) {
            e.preventDefault();
            var formData = $('#CreateReportForm').serializeFormToObject();
            $.extend(formData, _dtHelper.getDateRangeObject(formData.Date, 'DateBegin', 'DateEnd'));
            delete formData.Date;
            window.open(abp.appPath + 'app/DriverActivityDetailReport/GetReport?' + $.param(formData));
        });

    });
})();